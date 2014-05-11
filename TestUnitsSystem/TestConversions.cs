using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fhir.UnitsSystem;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace UnitsOfMeasure
{
    [TestClass]
    public class TestConversions
    {
        static UnitsSystem system;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            UcumReader reader = new UcumReader(Systems.UcumStream());
            system = reader.Read();
        }

        [TestMethod]
        public void TestToBaseConversions()
        {
            // inch to m
            Quantity quantity, result, expected;
            quantity = system.Quantity("4[in_i]");
            result = system.Conversions.ToBaseUnits(quantity);

            // pound force to kg.m.s-2
            quantity = system.Quantity("4.0[lbf_av]");
            result = system.Conversions.ToBaseUnits(quantity);
            expected = system.Quantity("18kg.m.s-2").ToBase();
            Assert.IsTrue(result.Approximates(expected));

            // newton
            quantity = system.Quantity("8.0N");
            result = system.Conversions.ToBaseUnits(quantity);
            expected = system.Quantity("8kg.m.s-2").ToBase();
            Assert.IsTrue(result.Approximates(expected));

          
        }

        public void TestConstantConversions()
        {
            Quantity quantity, result, expected;

            quantity = system.Quantity("2[pi]");
            result = system.Conversions.ToBaseUnits(quantity);
            expected = system.Quantity("8kg.m.s-2").ToBase();
            Assert.IsTrue(result.Approximates(expected));
        }

        [TestMethod]
        public void SingleConversion()
        {
            Quantity quantity = system.Convert("4[in_i]", "m");
            Assert.AreEqual(quantity.Metric.Symbols, "m");
            Assert.AreEqual((decimal)quantity.Value, 0.1016m);
            Assert.AreEqual((decimal)quantity.Value.Error, 0.127000m);

            Quantity target = system.Quantity("74e9Bq");
            quantity = system.Convert("2.00Ci", "Bq");
            Assert.IsTrue(quantity.Approximates(target));

            quantity = system.Convert("2.00Ci", "mBq");
            Assert.IsTrue(quantity.Approximates(target));

            quantity = system.Convert("2.00Ci", "MBq");
            Assert.IsTrue(quantity.Approximates(target));
            
            quantity = system.Convert("3.000[ft_br]", "[in_br]");
            target = system.Quantity("36.0[in_br]");
            Assert.IsTrue(quantity.Approximates(target));

            quantity = system.Convert("2[acr_br]", "[yd_br]2");
            target = system.Quantity("9680.0[yd_br]2");
            Assert.IsTrue(quantity.Approximates(target));
        }

        [TestMethod]
        public void ShortestPathTest()
        {
            Quantity quantity = system.Convert("4[lb_av]", "g");
            Assert.AreEqual((decimal)quantity.Value, 1.81436948e3m);
            Assert.AreEqual(quantity.Metric.Symbols, "g");
        }

        [TestMethod]
        public void ConversionToSystemTest()
        {
            Quantity quantity = system.ConvertToSsytem("4.00[lb_av]", "si");

            Quantity approx = system.Quantity("1.8e3g");
            Assert.IsTrue(quantity.Approximates(approx));
        }
    
        [TestMethod]
        public void TestUcumReader()
        {
            // Prefixes
            Assert.IsNotNull(system.Metrics.GetPrefix("k"));
            Assert.IsNotNull(system.Metrics.GetPrefix("y"));

            // Constants
            Assert.IsNotNull(system.Metrics.FindConstant("[pi]"));

            // Base-units
            Assert.IsNotNull(system.Metrics.FindUnit("g"));

            // Units
            Assert.IsNotNull(system.Metrics.FindUnit("Cel"));
            Assert.IsNotNull(system.Metrics.FindUnit("Cel"));
        }

        [TestMethod]
        public void TestExpressionBuilding()
        {
            string pattern = @"([^\.\/]+|[\.\/])";
            foreach (string token in Parser.Tokenize("[ft_i].[lbf_av].10*-1/s", pattern))
            {
                Debug.WriteLine("-" + token);
            }
            
            IEnumerable<string> list = new List<string>();
            
            Debug.WriteLine("done");

            // Conversion.Add("deg", "rad", f => f * 2 * pi / 360)
        }

    }
}
