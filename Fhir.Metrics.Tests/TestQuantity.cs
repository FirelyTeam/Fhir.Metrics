using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fhir.Metrics.Tests
{
    [TestClass]
    public class TestQuantity
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
        public void Dimensions()
        {
            Quantity a = system.Quantity("4kg.m.s-2");
            Quantity b = system.Quantity("8kg.m/s-2");
            bool same = Quantity.SameDimension(a, b);
            Assert.IsTrue(same);

        }

        [TestMethod]
        public void Algebra()
        {
            Quantity a, b, result, expected;
            bool same;

            a = system.Quantity("4.0km");
            b = system.Quantity("2.0km");
            result = a * b;
            expected = system.Quantity("8e6m3");
            same = Quantity.SameDimension(a, b);
            Assert.IsTrue(same);
            Assert.IsTrue(result.Approximates(expected));

            a = system.Quantity("4.0km");
            b = system.Quantity("2.0km");
            result = a / b; 
            expected = system.Quantity("2e0");
            Assert.IsTrue(result.IsDimless);
            same = Quantity.SameDimension(a, b);
            Assert.IsTrue(same);
            Assert.IsTrue(result.Approximates(expected));

            a = system.Quantity("4.0km");
            b = system.Quantity("2.0km");
            result = a + b;
            expected = system.Quantity("6e3m");
            same = Quantity.SameDimension(a, expected);
            Assert.IsTrue(same);
            Assert.IsTrue(result.Approximates(expected));

            a = system.Quantity("4.0kg.m/s2");
            b = system.Quantity("2.0e3g.m.s-2");
            result = a + b;
            expected = system.Quantity("6e3g.m.s-2");
            same = Quantity.SameDimension(a, expected);
            Assert.IsTrue(same);
            Assert.IsTrue(result.Approximates(expected));

            a = system.Quantity("4.0kg.m/s2");
            b = system.Quantity("2.0e3g.m.s-2");
            result = a - b;
            expected = system.Quantity("2e3g.m.s-2");
            same = Quantity.SameDimension(a, expected);
            Assert.IsTrue(same);
            Assert.IsTrue(result.Approximates(expected));

        }
    }
}
