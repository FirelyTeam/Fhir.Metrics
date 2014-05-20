using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fhir.Metrics;

namespace Fhir.Metrics.Tests
{
    [TestClass]
    public class TestMetrics
    {
        static SystemOfUnits system;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            system = UCUM.Load();
        }

        [TestMethod]
        public void Parsing()
        {
            Metric metric;
            
            metric = system.Metric("J/s");
            Assert.AreEqual(2, metric.Axes.Count);

            metric = system.Metric("[ft_i].[lbf_av]/s");
            Assert.AreEqual(3, metric.Axes.Count);
        }

        [TestMethod]
        public void TestMetricDimensions()
        {
            Quantity q = system.Quantity("2.3[psi]");
            q = system.ToBase(q);
            Console.WriteLine(q.Metric.DimensionText);
            Assert.AreEqual("mass^1.length^-1.time^-2", q.Metric.DimensionText);
        }

        [TestMethod]
        public void Equality()
        {
            Metric a, b;
            a = system.Metric("kg");
            b = system.Metric("kg");
            Assert.AreEqual(a, b);
            Assert.AreEqual(b, a);

            a = system.Metric("kg");
            b = system.Metric("N");
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(b, a);

            a = system.Metric("kg");
            b = system.Metric("kg.m2");
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(b, a);

            a = system.Metric("kg");
            b = system.Metric("kg.m.s-2");
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(b, a);
        }

        [TestMethod]
        public void Algebra()
        {
            Metric kg = system.Metric("kg");
            Metric ms2 = system.Metric("m/s2");
            Metric F = kg * ms2;
            Assert.AreEqual("kg.m.s-2", F.ToString());

            Metric m = system.Metric("m");
            Metric s2 = system.Metric("s2");
            Metric t = m / s2;

            Assert.AreEqual(ms2, t);

        }
    }
}
