using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Fhir.Metrics.Tests
{
    
    public class TestQuantity
    {
        SystemOfUnits system;

        
        public TestQuantity()
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
            catch (Exception e)
            {
                Assert.IsType<ArgumentException>(e);
            }
        }

        [Fact]
        public void Dimensions()
        {
            Quantity a = system.Quantity("4kg.m.s-2");
            Quantity b = system.Quantity("8kg.m/s-2");
            bool same = Quantity.SameDimension(a, b);
            Assert.True(same);
        }

        [Fact]
        public void Algebra()
        {
            Quantity a, b, result, expected;
            bool same;

            a = system.Quantity("4.0km");
            b = system.Quantity("2.0km");
            result = a * b;
            expected = system.Quantity("8e6m3");
            same = Quantity.SameDimension(a, b);
            Assert.True(same);
            Assert.True(result.Approximates(expected));

            a = system.Quantity("4.0km");
            b = system.Quantity("2.0km");
            result = a / b; 
            expected = system.Quantity("2e0");
            Assert.True(result.IsDimless);
            same = Quantity.SameDimension(a, b);
            Assert.True(same);
            Assert.True(result.Approximates(expected));

            a = system.Quantity("4.0km");
            b = system.Quantity("2.0km");
            result = a + b;
            expected = system.Quantity("6e3m");
            same = Quantity.SameDimension(a, expected);
            Assert.True(same);
            Assert.True(result.Approximates(expected));

            a = system.Quantity("4.0kg.m/s2");
            b = system.Quantity("2.0e3g.m.s-2");
            result = a + b;
            expected = system.Quantity("6e3g.m.s-2");
            same = Quantity.SameDimension(a, expected);
            Assert.True(same);
            Assert.True(result.Approximates(expected));

            a = system.Quantity("4.0kg.m/s2");
            b = system.Quantity("2.0e3g.m.s-2");
            result = a - b;
            expected = system.Quantity("2e3g.m.s-2");
            same = Quantity.SameDimension(a, expected);
            Assert.True(same);
            Assert.True(result.Approximates(expected));

        }

        [Fact]
        public void MixedAlgebra()
        {
            Metric meter = system.Metric("m");
            Metric second = system.Metric("s");
            
            Quantity q = 4.0m * meter / second;

            Metric speed = meter / second;
            Assert.Equal(speed, q.Metric);
            
        }
    }

}
