using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Reflection;
using System.Xml.Linq;

namespace Fhir.Metrics.Tests
{
    public class UcumTestSet
    {
        private XDocument document;

        public struct Validation 
        {
            public string Id;
            public string Unit;
            public bool Valid;
            public string Reason;
        }

        public struct Conversion
        {
            public string Id;
            public string Value;
            public string SourceUnit;
            public string DestUnit;
            public string Outcome;
        }

        /*
         *  public UcumReader(string docname) : this(File.OpenRead(docname))
        {
        }

        public UcumReader(Stream stream)
        {
            document = XDocument.Load(stream);
            document.CreateNavigator();
            navigator = document.CreateNavigator();
            ns = new XmlNamespaceManager(navigator.NameTable);
            ns.AddNamespace("u", "http://unitsofmeasure.org/ucum-essence");
        }

         */



        public static XDocument Load()
        {
            
            var assembly = typeof(UcumTestSet).GetTypeInfo().Assembly;
            //string[] names = assembly.GetManifestResourceNames();
            Stream s = assembly.GetManifestResourceStream("Fhir.Metrics.Tests.Data.ucum-tests.xml");
            var document = XDocument.Load(s);
            return document;
        }

        public UcumTestSet()
        {
            document = Load();
        }

        public IEnumerable<Validation> Validations()
        {

            XPathNavigator nav = document.CreateNavigator();
            foreach(XPathNavigator n in nav.Select("ucumTests/validation/case"))
            {
                Validation validation = new Validation();
                validation.Id = n.SelectSingleNode("@id").Value;
                validation.Unit = n.SelectSingleNode("@unit").Value;
                validation.Valid = n.SelectSingleNode("@valid").Value == "true";
                validation.Reason = n.SelectSingleNode("@reason")?.Value;
                yield return validation;
            }
        }

        public IEnumerable<Conversion> Conversions()
        {
            XPathNavigator nav = document.CreateNavigator();
            foreach (XPathNavigator n in nav.Select("ucumTests/conversion/case"))
            {
                Conversion conversion = new Conversion();
                conversion.Id = n.SelectSingleNode("@id").Value;
                conversion.Value = n.SelectSingleNode("@value").Value;
                conversion.SourceUnit = n.SelectSingleNode("@srcUnit").Value;
                conversion.DestUnit = n.SelectSingleNode("@dstUnit").Value; 
                conversion.Outcome = n.SelectSingleNode("@outcome").Value;
                //validation.Reason = = n.SelectSingleNode("unit").Value ;
                yield return conversion;

            }
        }

    }
}
