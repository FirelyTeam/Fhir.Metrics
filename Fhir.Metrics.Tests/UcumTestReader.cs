using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Reflection;

namespace Fhir.Metrics.Tests
{
    public class UcumTestSet
    {
        private XmlDocument document;

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

        public static XmlDocument Load()
        {
            XmlDocument document = new XmlDocument();
            string[] names = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            Stream s = typeof(UcumTestSet).Assembly.GetManifestResourceStream("Fhir.Metrics.Tests.Data.ucum-tests.xml");
            document.Load(s);
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
                //validation.Reason = = n.SelectSingleNode("unit").Value ;
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
