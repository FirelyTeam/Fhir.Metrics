using System;
using Fhir.Metrics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Fhir.Metrics.Tests
{
    public class TestConversions
    {
        SystemOfUnits system;

        
        public TestConversions()
        {
            system = UCUM.Load();
        }

        [Fact]
        public void FaultyFormatting()
        {
            Quantity quantity;

            //Valid:
            quantity = system.Quantity("4.3[in_i]");

            // Invalid number
            try
            {
                quantity = system.Quantity("4,4[in_i]");
                Assert.True(false, "Should have thrown an error");
            }
            catch (Exception e)
            {
                Assert.IsType<ArgumentException>(e);
            }

            // Missing number
            try
            {
                quantity = system.Quantity("[in_i]");
                Assert.True(false, "Should have thrown an error");
            }
            catch (Exception e)
            {
                Assert.IsType<ArgumentException>(e);
            }

            // Quantity without a unit
            quantity = system.Quantity("4");
            Assert.True(quantity.IsDimless);

            // Non existent unit
            try
            {
                quantity = system.Quantity("4[nonexistent]");
                Assert.True(false, "Should have thrown an error");
            }
            catch (ArgumentException)
            {

            }
            catch (Exception)
            {
                Assert.True(false, "It should have been an ArgumentException");
            }

        }

        [Fact]
        public void ToBaseConversions()
        {
            // inch to m
            Quantity quantity, result, expected;
            quantity = system.Quantity("4[in_i]");
            result = system.Conversions.Canonical(quantity);

            // pound force to kg.m.s-2
            quantity = system.Quantity("4.0[lbf_av]");
            result = system.Conversions.Canonical(quantity);
            expected = system.Quantity("18kg.m.s-2").UnPrefixed();
            Assert.True(result.Approximates(expected));
            
            // newton
            quantity = system.Quantity("8.0N");
            result = system.Conversions.Canonical(quantity);
            expected = system.Quantity("8kg.m.s-2").UnPrefixed();
            Assert.True(result.Approximates(expected));
        }

        [Fact]
        public void UnitDivisions()
        {
            Quantity quantity, result, expected;

            quantity = system.Quantity("10.0km/h");
            result = system.Canonical(quantity);
            expected = system.Quantity("2.8m/s").UnPrefixed();
            Assert.True(result.Approximates(expected));


            quantity = system.Quantity(60m, "/min");
            result = system.Canonical(quantity);
            expected = system.Quantity("1/s").UnPrefixed();
            Assert.True(result.Approximates(expected));



            /*
            120 mm[Hg]		15998640000 g.m-1.s-2
            176 g/L			0.176 g.m-3
            5.9 10*12/L		0.059 m-3
            99 fL			0.099 m3 
            */

        }

        [Fact]
        public void Sentinels()
        {
            Quantity a = system.Quantity("0/min");
            Quantity b = system.Canonical(a);
        }

        [Fact]
        public void Mercury()
        {
            Quantity quantity, result, expected;
            quantity = system.Quantity("120mm[Hg]");
            result = system.Canonical(quantity);
            expected = system.Quantity("1.6e7g.m-1.s-2");
            Assert.True(result.Approximates(expected));
        }

        [Fact]
        public void Reductions()
        {
            Quantity quantity, result, expected;
            quantity = system.Quantity("2.00/[in_i]");
            result = system.Canonical(quantity);
            expected = system.Quantity("0.8/cm").UnPrefixed();
            Assert.True(result.Approximates(expected));

            quantity = system.Quantity("2.00/[in_i]2");
            result = system.Canonical(quantity);
            expected = system.Quantity("3.1e3/m2");
            Assert.True(result.Approximates(expected));

            // psi => kg.m.s-2/m2 => g.m-1.s-2. 
            quantity = system.Quantity("2.000[psi]");
            result = system.Canonical(quantity);
            expected = system.Quantity("1.379e7g.m-1.s-2").UnPrefixed();
            Assert.True(result.Approximates(expected));
        }

        [Fact]
        public void UnPrefixed()
        {
            Quantity quantity, result, expected;

            quantity = system.Quantity("8dm3");
            result = quantity.UnPrefixed();
            expected = system.Quantity("0.008m3");
            Assert.True(result.Approximates(expected));

            quantity = system.Quantity("4g");
            result = quantity.UnPrefixed();
            expected = system.Quantity("4g");
            Assert.True(result.Approximates(expected));
        }

        [Fact]
        public void ConstantConversions()
        {
            Quantity quantity, result, expected;

            quantity = system.Quantity("2.000[pi].kg");
            result = system.Canonical(quantity);
            expected = system.Quantity("6.3kg").UnPrefixed();
            Assert.True(result.Approximates(expected));

            quantity = system.Quantity("180.00deg");
            result = system.Canonical(quantity);
            expected = system.Quantity("3.14rad").UnPrefixed();
            Assert.True(result.Approximates(expected));
        }

        //[Fact]
        public void ConversionToTargetUnit()
        {
            // Feature is not built. Unit test should fail here with NotImplementedException
            Quantity quantity = system.Convert("4[in_i]", "m");
            Assert.Equal(quantity.Metric.Symbols, "m");
            Assert.Equal((decimal)quantity.Value, 0.1016m);
            Assert.Equal((decimal)quantity.Value.Error, 0.127000m);

            Quantity target = system.Quantity("74e9Bq");
            quantity = system.Convert("2.00Ci", "Bq");
            Assert.True(quantity.Approximates(target));

            quantity = system.Convert("2.00Ci", "mBq");
            Assert.True(quantity.Approximates(target));

            quantity = system.Convert("2.00Ci", "MBq");
            Assert.True(quantity.Approximates(target));
            
            quantity = system.Convert("3.000[ft_br]", "[in_br]");
            target = system.Quantity("36.0[in_br]");
            Assert.True(quantity.Approximates(target));

            quantity = system.Convert("2[acr_br]", "[yd_br]2");
            target = system.Quantity("9680.0[yd_br]2");
            Assert.True(quantity.Approximates(target));
        }

        [Fact]
        public void UcumReader()
        {
            // Prefixes
            Assert.NotNull(system.Metrics.GetPrefix("k"));
            
            Assert.NotNull(system.Metrics.GetPrefix("y"));

            // Base-units
            Assert.NotNull(system.Metrics.FindUnit("g"));

            // Units
            Assert.NotNull(system.Metrics.FindUnit("Cel"));
            Assert.NotNull(system.Metrics.FindUnit("[psi]"));
            Assert.NotNull(system.Metrics.FindUnit("[psi]"));
        }
    }
}
