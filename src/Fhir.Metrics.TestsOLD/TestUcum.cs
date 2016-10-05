using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace Fhir.Metrics.Tests
{
    [TestClass]
    public class UcumTests
    {
        static SystemOfUnits system;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            system = UCUM.Load();
        }

        [TestMethod]
        public void Validation()
        {
            UcumTestSet reader = new UcumTestSet();
            foreach (UcumTestSet.Validation v in reader.Validations())
            {
                try
                {
                    Metric m = system.Metric(v.Unit);
                    
                    if (!v.Valid) throw new AssertFailedException(string.Format("Test {0} succeeded, but should not have", v.Id));
                }
                catch (Exception e)
                {
                    if (e is AssertFailedException)
                        throw;
                    else if (v.Valid)
                        throw new Exception(string.Format("Test {0} failed", v.Id));
                }
            }
        }

        [TestMethod]
        public void Conversion()
        {
            UcumTestSet reader = new UcumTestSet();
            foreach (UcumTestSet.Conversion conversion in reader.Conversions())
            {
                string expression = conversion.Value + conversion.SourceUnit;
                Quantity quantity = system.Canonical(expression);

                Exponential value = new Exponential(conversion.Outcome);
                Metric metric = system.Metric(conversion.DestUnit);
                try
                {
                    Assert.AreEqual(metric, quantity.Metric);
                    Assert.IsTrue(Exponential.Similar(value, quantity.Value));
                }
                catch (Exception e)
                {
                    throw new AssertFailedException(string.Format("Test {0} failed", conversion.Id), e);
                }
            }
        }

    }
}
