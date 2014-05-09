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

namespace Fhir.UnitsSystem
{
    public delegate int ExprMethod(int value);

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

        
        private void ReadPrefixes(UnitsSystem system)
        {
            foreach(XPathNavigator n in navigator.Select("u:root/prefix", ns))
            {
                Prefix p = new Prefix();
                p.Name = n.SelectSingleNode("name").ToString();
                string s = n.SelectSingleNode("value/@value").ToString();

                p.Factor = Exponential.Exact(s);
                p.Symbol  = n.SelectSingleNode("printSymbol").ToString();
                system.Metrics.Add(p);
            }
        }

        private void ReadBaseUnits(UnitsSystem system)
        {
            foreach (XPathNavigator n in navigator.Select("u:root/base-unit", ns))
            {
                string name = n.SelectSingleNode("name").ToString();
                string classification = "si";
                string symbol = n.SelectSingleNode("@Code").ToString();
                Unit unit = new Unit(classification, name, symbol);

                system.Metrics.Add(unit);
            }
        }

        private Constant BuildConstant(UnitsSystem system, string name, string formula, string number)
        {
            Exponential factor = Exponential.Exact(number);

            foreach(Unary u in Parser.ToUnaryTokens(formula))
            {
                Constant c; 
                string rest = u.Expression;
                if (system.Metrics.ConsumeConstant(rest, out c, out rest))
                {
                    int power = Convert.ToInt32(rest)*u.Exponent;
                    factor = factor * Exponential.Power(c.Value, power);
                }
            }
            return new Constant(name, factor);
        }

        private void ReadConstants(UnitsSystem system)
        {
            foreach (XPathNavigator n in navigator.Select("u:root/unit[class='dimless']", ns))
            {
                string name = n.SelectSingleNode("name").ToString();
                string classification = n.SelectSingleNode("@class", ns).ToString();
                string symbol = n.SelectSingleNode("@Code").ToString();
                string number = n.SelectSingleNode("value/@value").ToString();
                string formula = n.SelectSingleNode("value/@Unit").ToString();

                Constant c = BuildConstant(system, name, formula, number);
                system.Metrics.Add(c);
            }
        }

        private void ReadUnits(UnitsSystem system)
        {
            foreach (XPathNavigator n in navigator.Select("u:root/unit", ns))
            {
                string name = n.SelectSingleNode("name").ToString();
                string classification = n.SelectSingleNode("@class", ns).ToString();
                string symbol = n.SelectSingleNode("@Code").ToString();
                if (classification != "dimless")
                {
                    Unit unit = new Unit(classification, name, symbol);
                    system.Metrics.Add(unit);
                }
            }
        }

        public static ConversionMethod BuildConversion(string formula, Exponential number)
        {
            // NB! This method is still a (test) dummy.
            Match match = Regex.Match(formula, @"(\-?\d+|)");
            ParameterExpression param = Expression.Parameter(typeof(Exponential), "value");
            Expression body = Expression.Multiply(param, Expression.Constant(number));
            
            Expression<ConversionMethod> expression = Expression.Lambda<ConversionMethod>(body, param);
            ConversionMethod method = expression.Compile();

            return method;
        }
        
        public void AddConversion(UnitsSystem system, string from, string formula, Exponential number)
        {
            Metric metricfrom = system.Metrics.ParseMetric(from);
            //List<string> tokens = Parser.Tokenize(formula);

            Metric metricto = system.Metrics.ParseMetric(formula);

            if ( (metricfrom != null) && (metricto != null) )
            {
                Exponential factor = number * metricto.CalcFactor() / metricfrom.CalcFactor();
                
                ConversionMethod method = BuildConversion(formula, factor);
                system.Conversions.Add(metricfrom, metricto, method);
            }

        }

        public void ReadConversions(UnitsSystem system)
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
                    AddConversion(system, from, formula, number);
                }
                catch
                {

                }
                
            }
        }
       
        public UnitsSystem Read()
        {
            UnitsSystem system = new UnitsSystem();
            ReadConstants(system);
            ReadPrefixes(system);
            ReadBaseUnits(system);
            ReadUnits(system);
            ReadConversions(system);
            return system;
        }
    }
}
