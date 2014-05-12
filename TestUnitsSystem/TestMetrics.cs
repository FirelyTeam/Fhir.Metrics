using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fhir.UnitsSystem;

namespace UnitsOfMeasure
{
    [TestClass]
    public class TestMetrics
    {
        static SystemOfUnits system;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            UcumReader reader = new UcumReader(Systems.UcumStream());
            system = reader.Read();
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
    }
}
