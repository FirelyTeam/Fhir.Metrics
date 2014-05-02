/*
* Copyright (c) 2014, Furore (info@furore.com) and contributors
* See the file CONTRIBUTORS for details.
*
* This file is licensed under the BSD 3-Clause license
*/
using System;
using System.Collections.Generic;
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
        XmlDocument document;
        XmlNamespaceManager ns; 

        public UcumReader(string docname)
        {
            this.docname = docname;
            document = new XmlDocument();
            document.Load(this.docname);
            navigator = document.CreateNavigator();
            ns = new XmlNamespaceManager(navigator.NameTable);
            ns.AddNamespace("u", "http://unitsofmeasure.org/ucum-essence");
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
                string classification = n.SelectSingleNode("@class", ns).ToString();
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
            Expression body = Expression.Multiply(param, Expression.Constant(number));
            
            Expression<ConversionMethod> expression = Expression.Lambda<ConversionMethod>(body, param);
            ConversionMethod method = expression.Compile();
            return method;
        }

        public void ReadConversions(UnitsSystem system)
        {
            foreach (XPathNavigator n in navigator.Select("u:root/unit", ns))
            {
                string from = n.SelectSingleNode("@Code").ToString();
                string to = n.SelectSingleNode("u:value/@Unit").ToString();
                decimal number = Convert.ToDecimal(n.SelectSingleNode("u:/value/@value").ToString());
                ConversionMethod method = null;
                system.AddConversion(from, to, method);
            }
        }
       
        public UnitsSystem Read()
        {
            UnitsSystem system = new UnitsSystem();
            ReadPrefixes(system);

            return system;
        }
    }
}
