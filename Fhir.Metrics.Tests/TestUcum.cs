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
            foreach (UcumTestSet.ValidationTest v in reader.Validations())
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


        /** This testcase comes from UCUM and: 
         * (1) UCUM does not manage errors and error propagation yet; 
         * (2) UCUM can convert from one to another unit of measure, while Fhir.Metrics can currently convert to canonical
         * (3) in error propagation Fhir.Metrics uses the standard rule that if first error digit is 1, two significant digits 
         * of the significand will be used instead of 1.
         **/
        //[Ignore]
        [TestMethod]
        public void Conversion()
        {
            UcumTestSet reader = new UcumTestSet();
            foreach (UcumTestSet.ConversionTest conversion in reader.Conversions())
            {
                // Source:
                Metric sourceMetric = UcumReader.FormulaToMetric(system, conversion.SourceUnit);
                Exponential sourcefactor = UcumReader.FactorFromFormula(conversion.SourceUnit);
                //Exponential value = new Exponential(conversion.Value);
                Exponential value = Exponential.Exact(conversion.Value);
                value *= sourcefactor;
                Quantity source = new Quantity(value, sourceMetric);
                Exponential outcomeExp = new Exponential(conversion.Outcome);
                // if last error digit==1, two significant digits would be used in the assert, 
                // while the expected value comes from a system where this rule is not used. 
                outcomeExp.Error *= 2.0m; 
                Metric outcomeMetric = system.Metric(conversion.DestUnit).Reduced();

                // Conversion to base
                Quantity canonicalConverted = system.Canonical(source);
                Quantity canonicalExpectedQuantity = system.Canonical(new Quantity(outcomeExp, outcomeMetric));
                canonicalConverted.Value.Error = canonicalExpectedQuantity.Value.Error; // use same precision as what used to compute expected value

                try
                {
                    Assert.AreEqual(outcomeMetric, canonicalConverted.Metric);
                    //Assert.AreEqual(canonicalExpectedQuantity.Value.Error, canonicalConverted.Value.Error);
                    Assert.AreEqual(canonicalExpectedQuantity.Value.Exponent, canonicalConverted.Value.Exponent);
                    Assert.AreEqual(canonicalExpectedQuantity.Value.SignificandText, canonicalConverted.Value.SignificandText);
                }
                catch (Exception)
                {
                    Console.Error.WriteLine(string.Format("Test {0} failed, expected:{1} got {2}", conversion.Id, canonicalExpectedQuantity.Value.SignificandText, canonicalConverted.Value.SignificandText));
                    throw new AssertFailedException(string.Format("Test {0} failed, expected:{1} got {2}", conversion.Id, canonicalExpectedQuantity.Value.SignificandText, canonicalConverted.Value.SignificandText));
                }
            }
        }

    }
}
