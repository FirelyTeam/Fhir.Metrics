using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fhir.Metrics;
using System.Text.RegularExpressions;

namespace UnitsSystemTests
{
    [TestClass]
    public class TestExponentials
    {
        [TestMethod]
        public void StringConversion()
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

            a = new Exponential("9.1093822e-31");
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
        public void Add()
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
        public void Substracting()
        {
            Exponential a, b, c;

            a = 4.0m;
            b = 2.0m;
            c = a - b;
            Assert.AreEqual(2.0m, c.Significand);
            Assert.AreEqual(0.10m, c.Error);
            Assert.AreEqual(0, c.Exponent);

            a = 12.0m;
            b = 8.0m;
            c = a - b;
            Assert.AreEqual(4.0m, c.Significand);
            Assert.AreEqual(0.1m, c.Error);
            Assert.AreEqual(0, c.Exponent);

            // We have to use string here, because float translates 12.0e3 to 12000 (loses precision information)
            a = new Exponential("12.0e3");
            b = new Exponential("8.0e3");
            c = a - b;
            Assert.AreEqual(4.0m, c.Significand);
            Assert.AreEqual(0.1m, c.Error); 
            Assert.AreEqual(3, c.Exponent);
        }

        [TestMethod]
        public void Multiplication()
        {
            Exponential a, b, c;
            a = 4.0m;
            b = 2.0m;
            c = a * b;
            Assert.AreEqual(8.0m, c.Significand);
            Assert.AreEqual(0.32m, c.Error);
            Assert.AreEqual(0, c.Exponent);

            // Normalization test:
            a = 40m;
            b = 30m;
            c = a * b;
            Assert.AreEqual(1.2m, c.Significand);
            Assert.AreEqual(0.038m, c.Error);
            Assert.AreEqual(3, c.Exponent);
        }

        [TestMethod]
        public void Division()
        {
            Exponential a, b, c;
            a = 200.00m;
            b = 50.0m;
            c = a / b;
            Assert.AreEqual(4.0m, c.Significand);
            Assert.AreEqual(0.004101m, c.Error);
            Assert.AreEqual(0, c.Exponent);
        }

        [TestMethod]
        public void DivisionWithDifferentExponents()
        {
            Exponential a, b, c, d, expected;
            
            a = new Exponential("10.0");
            b = Exponential.Exact("60");
            c = a / b;
            d = c / b; // divide twice by 60
            expected = new Exponential("2.8e-3");
            Assert.IsTrue(d.Approximates(expected));
        }

        [TestMethod]
        public void ExponentialToString()
        {
            Exponential value;
            string s;

            value = new Exponential(4.55555m, 0, 0.07m);
            s = value.ToString();
            Assert.AreEqual("[4.55±0.07]e0", s);

            value = new Exponential(4.666666666m, 0, 0.01234m);
            s = value.ToString();
            Assert.AreEqual("[4.67±0.01]e0", s);

            Exponential a, b;
            a = 200.00m;
            b = 50.0m;
            value = a / b;
            s = value.ToString();
            Assert.AreEqual("[4.0±0.004]e0", s);
        }
    }
}
