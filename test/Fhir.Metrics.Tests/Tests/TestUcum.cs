using System;
using Xunit;

namespace Fhir.Metrics.Tests
{

    public class UcumTests
    {
        SystemOfUnits system;

        public UcumTests()
        {
            system = UCUM.Load();
        }

        [Fact]
        public void Validation()
        {
            UcumTestSet tests = new UcumTestSet();
            foreach (UcumTestSet.Validation v in tests.Validations())
            {
                try
                {
                    Metric m = system.Metric(v.Unit);
                    
                    if (!v.Valid) throw new AssertFailedException(string.Format($"Test {v.Id} succeeded, but should not have. Reason: '{v.Reason}'"));
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

        [Fact]
        public void Conversion()
        {
            // this test fails, because in our opinion the input xml is incorrect. See the Data/ucum-tests.xml for the comment.
            UcumTestSet reader = new UcumTestSet();
            foreach (UcumTestSet.Conversion conversion in reader.Conversions())
            {
                string expression = conversion.Value + conversion.SourceUnit;
                Quantity quantity = system.Canonical(expression);

                Exponential value = new Exponential(conversion.Outcome);
                Metric metric = system.Metric(conversion.DestUnit);
                try
                {
                    Assert.Equal(metric, quantity.Metric);
                    Assert.True(Exponential.Similar(value, quantity.Value));
                }
                catch (Exception e)
                {
                    throw new AssertFailedException(string.Format("Test {0} failed", conversion.Id), e);
                }
            }
        }

    }
}
