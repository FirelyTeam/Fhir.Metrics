using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fhir.Metrics;

namespace Fhir.Metrics.Tests
{
    [TestClass]
    public class QuantityExtensionsTest
    {
        static SystemOfUnits system;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            system = UCUM.Load();
        }

        [TestMethod]
        public void LeftSearchableString()
        {
            Quantity q = system.Quantity("41.234567kg.m/s2");
            string s = q.LeftSearchableString();
            Assert.AreEqual("g.m.s-2E4x4.12356770", s);

            Quantity mass = system.Quantity("4.0kg");
            Quantity acceleration = system.Quantity("2.0m/s2");
            Quantity force = mass * acceleration;
            s = force.LeftSearchableString();
            Assert.AreEqual("g.m.s-2E3x8.0", s);

            q = system.Quantity("4000g");
            s = q.LeftSearchableString();
            Assert.AreEqual("gE3x4.000", s);

            q = system.Quantity("4.1235555kg.m/s2");
            q.Value.Error=0.0000005m;
            s = q.LeftSearchableString();
            Assert.AreEqual("g.m.s-2E3x4.1235555", s);

            q = system.Quantity("4.123456");
            q.Value.Error = 0.000005m;
            s = q.LeftSearchableString();
            Assert.AreEqual("E0x4.123566", s);
        }



        [TestMethod]
        public void LeftSearch()
        {
            // testing round down
            var haystack = system.Quantity("4.1234555");
            Assert.IsFalse(SearchExtensions.SearchLeft(system.Quantity("4.5555"),haystack));
            Assert.IsTrue(SearchExtensions.SearchLeft(system.Quantity("4.1"), haystack));
            Assert.IsTrue(SearchExtensions.SearchLeft(system.Quantity("4.12"), haystack));
            Assert.IsTrue(SearchExtensions.SearchLeft(system.Quantity("4.123"), haystack));
            Assert.IsFalse(SearchExtensions.SearchLeft(system.Quantity("4.124"), haystack));
            Assert.IsTrue(SearchExtensions.SearchLeft(system.Quantity("4.1234"), haystack));
            Assert.IsTrue(SearchExtensions.SearchLeft(system.Quantity("4.12345"), haystack));
            Assert.IsTrue(SearchExtensions.SearchLeft(system.Quantity("4.123455"), haystack));
            Assert.IsTrue(SearchExtensions.SearchLeft(system.Quantity("4.1234555"), haystack));

            // testing round up
            haystack = system.Quantity("4.1234567");
            Assert.IsTrue(SearchExtensions.SearchLeft(system.Quantity("4.123"), haystack));
            Assert.IsFalse(SearchExtensions.SearchLeft(system.Quantity("4.124"), haystack));
            Assert.IsTrue(SearchExtensions.SearchLeft(system.Quantity("4.1235"), haystack));
            Assert.IsTrue(SearchExtensions.SearchLeft(system.Quantity("4.12346"), haystack));
            Assert.IsTrue(SearchExtensions.SearchLeft(system.Quantity("4.123457"), haystack));
            Assert.IsTrue(SearchExtensions.SearchLeft(system.Quantity("4.1234567"), haystack));
        }

    }
}
