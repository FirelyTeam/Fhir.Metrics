using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fhir.UnitsSystem;
using System.Globalization;
using System.IO;

namespace UnitsOfMeasure
{
    [TestClass]
    public class TestConversions
    {
       static UnitsSystem system;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            //UcumReader reader = new UcumReader("http://unitsofmeasure.org/ucum-essence.xml");
            UcumReader reader = new UcumReader(UcumReader.UcumStream());
            system = reader.Read();
        }

        [TestMethod]
        public void SingleConversion()
        {
            Quantity quantity = system.Convert("4[in_i]", "m");
            Assert.AreEqual(quantity.Unit.Symbol, "m");
            Assert.AreEqual((decimal)quantity.Value, 0.1016m);
            Assert.AreEqual((decimal)quantity.Value.Error, 0.127000m);
        }

        [TestMethod]
        public void ShortestPathTest()
        {
            Quantity quantity = system.Convert("4[lb_av]", "g");
            Assert.AreEqual((decimal)quantity.Value, 1.81436948e3m);
            Assert.AreEqual(quantity.Unit.Symbol, "g");
        }

        [TestMethod]
        public void ConversionToSystemTest()
        {
            Quantity quantity = system.ConvertToSsytem("4.00[lb_av]", "si");

            Quantity approx = system.ExpressionToQuantity("1.8e3g");
            Assert.IsTrue(quantity.Approximates(approx));
        }
    
        [TestMethod]
        public void TestUcumReader()
        {
            Assert.IsNotNull(system.Units.FindPrefix("k"));
            Assert.IsNotNull(system.Units.FindPrefix("y"));
            
            Assert.IsNotNull(system.Units.FindUnit("g"));
            Assert.IsNotNull(system.Units.FindUnit("Cel"));
        }
        
        [TestMethod]
        public void TestExpressionBuilding()
        {
            //ConversionMethod method = reader.BuildConversion("nvt", 4m);
            //Exponential result = method(2);
        }

    }
}
