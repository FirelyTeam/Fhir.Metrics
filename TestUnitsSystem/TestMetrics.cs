using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fhir.UnitsSystem;

namespace UnitsOfMeasure
{
    [TestClass]
    public class TestMetrics
    {
        static UnitsSystem system;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            UcumReader reader = new UcumReader(Systems.UcumStream());
            system = reader.Read();
        }

        [TestMethod]
        public void TestMethod1()
        {
            Quantity quantity;
            
            quantity = system.Quantity("4J/s");
            quantity = system.Quantity("1[ft_i].[lbf_av]/s");

        }
    }
}
