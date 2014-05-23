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
            Assert.AreEqual("g.m.s-2E4+4.1235677", s);

            Quantity mass = system.Quantity("4.0kg");
            Quantity acceleration = system.Quantity("2.0m/s2");
            Quantity force = mass * acceleration;
            s = force.LeftSearchableString();
            Assert.AreEqual("g.m.s-2E3+8.0", s);

        }

    }
}
