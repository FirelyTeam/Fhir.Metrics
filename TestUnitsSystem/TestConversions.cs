using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fhir.UnitsSystem;
using System.Globalization;

namespace UnitsOfMeasure
{
    [TestClass]
    public class TestConversions
    {
        UnitsSystem system = Systems.Metric;
        [TestMethod]
        public void SingleConversion()
        {
            Quantity quantity = system.Convert("4[in_i]", "m");
            Assert.AreEqual(quantity.Unit.Symbol, "m");
            Assert.AreEqual((decimal)quantity.Value, 0.1016m);
            Assert.AreEqual((decimal)quantity.Value.Error, 0.127000m);
        }

        [TestMethod]
        public void StringBasedConversion()
        {
            string s = system.Convert("4pnd", "kg").ToString();
            Assert.AreEqual(s, "2kg");
        }

        [TestMethod]
        public void TestUcumReader()
        {
            UcumReader reader = new UcumReader("http://unitsofmeasure.org/ucum-essence.xml");
            UnitsSystem ucum = reader.Read();
            Assert.IsNotNull(ucum.Units.FindPrefix("k"));
            Assert.IsNotNull(ucum.Units.FindPrefix("y"));
            
            Assert.IsNotNull(ucum.Units.FindUnit("g"));
            Assert.IsNotNull(ucum.Units.FindUnit("Cel"));
        }

        [TestMethod]
        public void TestExpressionBuilding()
        {
            UcumReader reader = new UcumReader("http://unitsofmeasure.org/ucum-essence.xml");
            ConversionMethod method = reader.BuildConversion("nvt", 4m);
            Exponential result = method(2);

        }

    }
}
