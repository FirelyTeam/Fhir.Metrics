using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitsOfMeasure;
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
            Assert.AreEqual(2.3343m, e.Value);
            Assert.AreEqual(9, e.Exponent);
            Assert.AreEqual(0.00005m, e.Error);

            e = new Exponential("4.3");
            Assert.AreEqual(4.3m, e.Value);
            Assert.AreEqual(0, e.Exponent);
            Assert.AreEqual(0.05m, e.Error);

            e = new Exponential("40");
            Assert.AreEqual(4.0m, e.Value);
            Assert.AreEqual(1, e.Exponent);
            Assert.AreEqual(0.05m, e.Error);
        }



        [TestMethod]
        public void NotationErrors()
        {
            Exponential a;
            a = new Exponential(4);
            Assert.AreEqual(4m, a.Value);
            Assert.AreEqual(0.5m, a.Error);
            
            a = new Exponential(4.0m);
            Assert.AreEqual(4m, a.Value);
            Assert.AreEqual(0.05m, a.Error);

            a = new Exponential(4e3m);
            Assert.AreEqual(4m, a.Value);
            Assert.AreEqual(3, a.Exponent);
            Assert.AreEqual(0.0005m, a.Error);
        }

        [TestMethod]
        public void Normalizing()
        {
            Exponential a, b;
            
            a = new Exponential(34, 3);
            Assert.AreEqual(3.4m, a.Value);
            Assert.AreEqual(4, a.Exponent);

            a = new Exponential(0.0049m, 0);
            Assert.AreEqual(4.9m, a.Value);
            Assert.AreEqual(-3, a.Exponent);

            a = new Exponential("9.1093822e-31m");
            Assert.AreEqual(9.1093822m, a.Value);
            Assert.AreEqual(-31, a.Exponent);

        }
    }
}
