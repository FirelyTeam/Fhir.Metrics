using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fhir.UnitsSystem;
using System.Text.RegularExpressions;

namespace UnitsSystemTests
{
    [TestClass]
    public class TestExponentials
    {
        [TestMethod]
        public void TestStringConversion()
        {
            Exponential e;

            e = new Exponential("23.343e8");
            Assert.AreEqual(2.3343m, e.Significand);
            Assert.AreEqual(9, e.Exponent);
            Assert.AreEqual(0.00005m, e.Error);

            e = new Exponential("4.3");
            Assert.AreEqual(4.3m, e.Significand);
            Assert.AreEqual(0, e.Exponent);
            Assert.AreEqual(0.05m, e.Error);

            e = new Exponential("40");
            Assert.AreEqual(4.0m, e.Significand);
            Assert.AreEqual(1, e.Exponent);
            Assert.AreEqual(0.05m, e.Error);
        }

        [TestMethod]
        public void NotationErrors()
        {
            Exponential a;
            a = new Exponential(4);
            Assert.AreEqual(4m, a.Significand);
            Assert.AreEqual(0.5m, a.Error);
            
            a = new Exponential(4.0m);
            Assert.AreEqual(4m, a.Significand);
            Assert.AreEqual(0.05m, a.Error);

            a = new Exponential(4e3m);
            Assert.AreEqual(4m, a.Significand);
            Assert.AreEqual(3, a.Exponent);
            Assert.AreEqual(0.0005m, a.Error);
        }

        [TestMethod]
        public void Normalizing()
        {
            Exponential a;
            
            a = new Exponential(34, 3);
            Assert.AreEqual(3.4m, a.Significand);
            Assert.AreEqual(4, a.Exponent);

            a = new Exponential(0.0049m, 0);
            Assert.AreEqual(4.9m, a.Significand);
            Assert.AreEqual(-3, a.Exponent);

            a = new Exponential("9.1093822e-31m");
            Assert.AreEqual(9.1093822m, a.Significand);
            Assert.AreEqual(-31, a.Exponent);

        }

        [TestMethod]
        public void ImplicitTypeConversions()
        {
            Exponential a = 60.0m;
            Assert.AreEqual(6.00m, a.Significand);
            Assert.AreEqual(1, a.Exponent);

            a = 4;
            Assert.AreEqual(4m, a.Significand);
            Assert.AreEqual(0, a.Exponent);
        }

        [TestMethod]
        public void ExplicitTypeConversions()
        {
            Exponential a;

            a = (Exponential)4.4;
            Assert.AreEqual(4.4m, a.Significand);
            Assert.AreEqual(0, a.Exponent);

            a = (Exponential)60.0;
            Assert.AreEqual(6.00m, a.Significand);
            Assert.AreEqual(1, a.Exponent);
        }

        [TestMethod]
        public void TestAdd()
        {
            Exponential a, b, c;

            a = new Exponential(4.0m);
            b = new Exponential(2.0m);
            c = a + b;
            Assert.AreEqual(6.0m, c.Significand);
            Assert.AreEqual(0.1m, c.Error);
            Assert.AreEqual(0, c.Exponent);

            a = new Exponential(4.0m);
            b = new Exponential(20.0m);
            c = a + b;
            Assert.AreEqual(2.4m, c.Significand);
            Assert.AreEqual(0.01m, c.Error);
            Assert.AreEqual(1, c.Exponent);

            // Normalization test:
            a = 8m;
            b = 7m;
            c = a + b;
            Assert.AreEqual(1.5m, c.Significand);
            Assert.AreEqual(0.1m, c.Error);
            Assert.AreEqual(1, c.Exponent);
        }

        [TestMethod]
        public void TestSubstract()
        {
            Exponential a, b, c;

            a = 4.0m;
            b = 2.0m;
            c = a - b;
            Assert.AreEqual(2.0m, c.Significand);
            Assert.AreEqual(0.00m, c.Error);
            Assert.AreEqual(0, c.Exponent);

            a = 12.0m;
            b = 8.0m;
            c = a - b;
            Assert.AreEqual(4.0m, c.Significand);
            Assert.AreEqual(0.0m, c.Error);
            Assert.AreEqual(0, c.Exponent);
        }

        [TestMethod]
        public void TestMultiplication()
        {
            Exponential a, b, c;
            a = 4.0m;
            b = 2.0m;
            c = a * b;
            Assert.AreEqual(8.0m, c.Significand);
            Assert.AreEqual(0.3m, c.Error);
            Assert.AreEqual(0, c.Exponent);

            // Normalization test:
            a = 40m;
            b = 30m;
            c = a * b;
            Assert.AreEqual(1.2m, c.Significand);
            Assert.AreEqual(0.035m, c.Error);
            Assert.AreEqual(3, c.Exponent);
        }

        [TestMethod]
        public void TestDivision()
        {
            Exponential a, b, c;
            a = 200.00m;
            b = 50.0m;
            c = a / b;
            Assert.AreEqual(4.0m, c.Significand);
            Assert.AreEqual(0.1025m, c.Error);
            Assert.AreEqual(0, c.Exponent);
        }
    }
}
