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
        static SystemOfUnits system;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            system = UCUM.Load();
        }

        [TestMethod]
        public void FaultyFormatting()
        {
            Quantity quantity;

            //Valid:
            quantity = system.Quantity("4.3[in_i]");

            // Invalid number
            try
            {
                quantity = system.Quantity("4,4[in_i]");
                Assert.Fail("Should have thrown an error");
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(e, typeof(ArgumentException));
            }

            // Missing number
            try
            {
                quantity = system.Quantity("[in_i]");
                Assert.Fail("Should have thrown an error");
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(e, typeof(ArgumentException));
            }

            // Quantity without a unit
            quantity = system.Quantity("4");
            Assert.IsTrue(quantity.IsDimless);

            // Non existent unit
            try
            {
                quantity = system.Quantity("4[nonexistent]");
                Assert.Fail("Should have thrown an error");
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(e, typeof(ArgumentException));
            }

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

        [TestMethod]
        public void ReductionTest()
        {
            Quantity quantity, result, expected;
            // psi to kg.m.s-2/m2 = kg.m-1.s-2. 
            quantity = system.Quantity("2.00[psi]");
            result = system.Conversions.ToBaseUnits(quantity);
            expected = system.Quantity("2.3e2g.m-1.s-2").ToBase();
            Assert.IsTrue(result.Approximates(expected));
        }

        [TestMethod]
        public void TestUnPrefixed()
        {
            Quantity quantity, result, expected;

            quantity = system.Quantity("8dm3");
            result = quantity.ToBase();
            expected = system.Quantity("0.008m3");
            Assert.IsTrue(result.Approximates(expected));

            quantity = system.Quantity("4g");
            result = quantity.ToBase();
            expected = system.Quantity("4g");
            Assert.IsTrue(result.Approximates(expected));
        }

        [TestMethod]
        public void TestConstantConversions()
        {
            Quantity quantity, result, expected;

            quantity = system.Quantity("2.000[pi].kg");
            result = system.ToBase(quantity);
            expected = system.Quantity("6.3kg").ToBase();
            Assert.IsTrue(result.Approximates(expected));

            quantity = system.Quantity("180.00deg");
            result = system.ToBase(quantity);
            expected = system.Quantity("3.14rad").ToBase();
            Assert.IsTrue(result.Approximates(expected));
        }

        [TestMethod]
        public void ConversionToTargetUnit()
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
        public void TestUcumReader()
        {
            // Prefixes
            Assert.IsNotNull(system.Metrics.GetPrefix("k"));
            Assert.IsNotNull(system.Metrics.GetPrefix("y"));

            // Base-units
            Assert.IsNotNull(system.Metrics.FindUnit("g"));

            // Units
            Assert.IsNotNull(system.Metrics.FindUnit("Cel"));
            Assert.IsNotNull(system.Metrics.FindUnit("Cel"));
        }
    }
}
