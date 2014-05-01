/*
* Copyright (c) 2014, Furore (info@furore.com) and contributors
* See the file CONTRIBUTORS for details.
*
* This file is licensed under the BSD 3-Clause license
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace Fhir.UnitsSystem
{
    
    public class BaseUnit
    {
        public string Code;
        public string Name;
        public string Symbol;
        public string Property;
        public string Unit;
    }

    public class UcumReader
    {
        public List<Prefix> Prefixes = new List<Prefix>();

        public void Read(IXPathNavigable navigable)
        {

            XPathNavigator navigator = navigable.CreateNavigator();
            XmlNamespaceManager ns = new XmlNamespaceManager(navigator.NameTable);
            ns.AddNamespace("u", "http://unitsofmeasure.org/ucum-essence");

            foreach(XPathNavigator n in navigator.Select("u:root/prefix", ns))
            {

                Prefix p = new Prefix();
                p.Name = n.SelectSingleNode("name").ToString();
                p.Dex = Convert.ToInt32(n.SelectSingleNode("value/@value").ToString());
                p.Symbol  = n.SelectSingleNode("printSymbol").ToString();
                    //Symbol = n.SelectSingleNode("@Code").ToString(),
                    //Html = n.SelectSingleNode("value").InnerXml
                Prefixes.Add(p);
            }
        }

       
        public void Read()
        {
            XmlDocument document = new XmlDocument();
            document.Load("http://unitsofmeasure.org/ucum-essence.xml");
            Read(document);
        }
    }
}
