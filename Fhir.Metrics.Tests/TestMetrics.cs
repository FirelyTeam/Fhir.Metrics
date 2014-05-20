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
        public void TestMetricParsing()
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
    }
}
