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
                system.Units.Add(p);
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

                system.Units.Add(unit);
            }
        }

        private void ReadUnits(UnitsSystem system)
        {
            foreach (XPathNavigator n in navigator.Select("u:root/unit", ns))
            {
                string name = n.SelectSingleNode("name").ToString();
                string classification = n.SelectSingleNode("@class", ns).ToString();
                string symbol = n.SelectSingleNode("@Code").ToString();
                Unit unit = new Unit(classification, name, symbol);

                system.Units.Add(unit);
            }
        }



        public ConversionMethod BuildConversion(string formula, Exponential number)
        {
            // NB! This method is still a (test) dummy.

            ParameterExpression param = Expression.Parameter(typeof(Exponential), "value");
            Expression body;
            int idx = formula.IndexOfAny(new char[] { '.', '/' }); 
            if (idx > 0)
            {
                body = Expression.Multiply(param, Expression.Constant(number));
            }
            else
            {
                body = Expression.Multiply(param, Expression.Constant(number));
            }
            
            Expression<ConversionMethod> expression = Expression.Lambda<ConversionMethod>(body, param);
            ConversionMethod method = expression.Compile();
            return method;
        }

        public void AddConversion(UnitsSystem system, string from, string to, Exponential number)
        {
            Metric metricfrom = system.Units.ParseMetric(from);
            Metric metricto = system.Units.ParseMetric(to);
            if ( (metricfrom != null) && (metricto != null) )
            {
                Exponential factor = number;
                if (metricfrom.Prefix != null) factor *= metricfrom.Prefix.Factor;
                if (metricto.Prefix != null) factor *= metricto.Prefix.Factor;

                ConversionMethod method = BuildConversion(to, factor);
                system.Conversions.Add(metricfrom.Unit, metricto.Unit, method);
            }

        }
            
        public void ReadConversions(UnitsSystem system)
        {
            foreach (XPathNavigator n in navigator.Select("u:root/unit", ns))
            {
                string from = n.SelectSingleNode("@Code").ToString();
                string to = n.SelectSingleNode("value/@Unit").ToString();
                try
                {
                    string value = n.SelectSingleNode("value/@value").ToString();
                    if (value.Length > 16)
                        value = value.Substring(0, 16);
                    Exponential number = Exponential.Exact(value);
                    AddConversion(system, from, to, number);
                }
                catch
                {

                }
                
            }
        }
       
        public UnitsSystem Read()
        {
            UnitsSystem system = new UnitsSystem();
            ReadPrefixes(system);
            ReadBaseUnits(system);
            ReadUnits(system);
            ReadConversions(system);
            return system;
        }

    }
}
