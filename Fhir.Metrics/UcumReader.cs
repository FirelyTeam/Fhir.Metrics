/*
* Copyright (c) 2014, Furore (info@furore.com) and contributors
* See the file CONTRIBUTORS for details.
*
* This file is licensed under the BSD 3-Clause license
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace Fhir.Metrics
{
    public class UcumReader
    {
        string docname;
        XPathNavigator navigator;
        XmlDocument document = new XmlDocument();
        XmlNamespaceManager ns; 

        public void init(IXPathNavigable navigable)
        {
            navigator = document.CreateNavigator();
            ns = new XmlNamespaceManager(navigator.NameTable);
            ns.AddNamespace("u", "http://unitsofmeasure.org/ucum-essence");
        }

        public UcumReader(string docname)
        {
            this.docname = docname;
            document.Load(this.docname);
            init(document);
        }

        public UcumReader(Stream s)
        {
            document.Load(s);
            init(document);
        }

        private void ReadPrefixes(SystemOfUnits system)
        {
            foreach(XPathNavigator n in navigator.Select("u:root/prefix", ns))
            {
                string name = n.SelectSingleNode("name").ToString();
                string s = n.SelectSingleNode("value/@value").ToString();
                Exponential factor = Exponential.Exact(s);

                //string symbol = n.SelectSingleNode("printSymbol").ToString();
                string symbol = n.SelectSingleNode("@Code").ToString();
                system.AddPrefix(name, symbol, factor);
            }
        }

        private void ReadBaseUnits(SystemOfUnits system)
        {
            foreach (XPathNavigator n in navigator.Select("u:root/base-unit", ns))
            {
                string name = n.SelectSingleNode("name").ToString();
                string dimension = n.SelectSingleNode("property").ToString();
                string symbol = n.SelectSingleNode("@Code").ToString();

                system.AddUnit(name, symbol, dimension);
            }
        }
      
        private void ReadUnits(SystemOfUnits system)
        {
            foreach (XPathNavigator n in navigator.Select("u:root/unit", ns))
            {
                string name = n.SelectSingleNode("name").ToString();
                string classification = n.SelectSingleNode("@class", ns).ToString();
                
                string symbol = n.SelectSingleNode("@Code").ToString();
                //if (classification != "dimless")
                {
                    system.AddUnit(name, symbol);
                }
            }
        }

        public static ConversionMethod FormulaToConversionMethod(string formula, Exponential number)
        {
            ParameterExpression param = Expression.Parameter(typeof(Exponential), "value");

            Expression body = Expression.Multiply(param, Expression.Constant(number)); 
            foreach (Unary u in Parser.ToUnaryTokens(formula).Numerics())
            {
                Exponential factor = u.Numeric();
                if (u.Exponent == 1)
                    body = Expression.Multiply(body, Expression.Constant(factor));
                else if (u.Exponent == -1)
                    body = Expression.Divide(body, Expression.Constant(factor));
            }

            Expression<ConversionMethod> expression = Expression.Lambda<ConversionMethod>(body, param);
            ConversionMethod method = expression.Compile();

            return method;
        }

        public static Metric FormulaToMetric(SystemOfUnits system, string formula)
        {
            return system.Metrics.ParseMetric(Parser.ToUnaryTokens(formula).NonNumerics());
        }
        
        public static Exponential FactorFromFormula(string formula)
        {
            Exponential f = Exponential.One;
            foreach (Unary u in Parser.ToUnaryTokens(formula).Numerics())
            {
                f *= u.Factor();
            }
            return f;
        }

        public static Conversion ParseConversion(SystemOfUnits system, string from, string formula)
        {
            return ParseConversion(system, from, formula, Exponential.One);
        }

        public static Conversion ParseConversion(SystemOfUnits system, string from, string formula, Exponential number)
        {
            Metric metricfrom = system.Metrics.ParseMetric(from);
            Metric metricto = FormulaToMetric(system, formula);

            if ( (metricfrom != null) && (metricto != null) )
            {
                ConversionMethod method = FormulaToConversionMethod(formula, number);
                Conversion conversion = new Conversion(metricfrom, metricto, method);
                return conversion;
            }
            return null;
        }

        public void ReadConversions(SystemOfUnits system)
        {
            foreach (XPathNavigator n in navigator.Select("u:root/unit", ns))
            {
                string from = n.SelectSingleNode("@Code").ToString();
                string formula = n.SelectSingleNode("value/@Unit").ToString();
                try
                {
                    string value = n.SelectSingleNode("value/@value").ToString();
                    if (value.Length > 16)
                        value = value.Substring(0, 16);
                    Exponential number = Exponential.Exact(value);

                    Conversion conversion = ParseConversion(system, from, formula, number);
                    system.Conversions.Add(conversion);
                }
                catch
                {

                }
                
            }
        }
       
        public SystemOfUnits Read()
        {
            SystemOfUnits system = new SystemOfUnits();
            //ReadConstants(system);
            ReadPrefixes(system);
            ReadBaseUnits(system);
            ReadUnits(system);
            ReadConversions(system);
            return system;
        }
    }
}
