using System;
using Xunit;
using Fhir.Metrics;
using System.Text.RegularExpressions;

namespace UnitsSystemTests
{
    public class TestExponentials
    {
        [Fact]
        public void StringConversion()
        {
            Exponential e;

            e = new Exponential("23.343e8");
            Assert.Equal(2.3343m, e.Significand);
            Assert.Equal(9, e.Exponent);
            Assert.Equal(0.00005m, e.Error);

            e = new Exponential("4.3");
            Assert.Equal(4.3m, e.Significand);
            Assert.Equal(0, e.Exponent);
            Assert.Equal(0.05m, e.Error);

            e = new Exponential("40");
            Assert.Equal(4.0m, e.Significand);
            Assert.Equal(1, e.Exponent);
            Assert.Equal(0.05m, e.Error);
        }

        [Fact]
        public void NotationErrors()
        {
            Exponential a;
            a = new Exponential(4);
            Assert.Equal(4m, a.Significand);
            Assert.Equal(0.5m, a.Error);
            
            a = new Exponential(4.0m);
            Assert.Equal(4m, a.Significand);
            Assert.Equal(0.05m, a.Error);

            a = new Exponential(4e3m);
            Assert.Equal(4m, a.Significand);
            Assert.Equal(3, a.Exponent);
            Assert.Equal(0.0005m, a.Error);
        }

        [Fact]
        public void Normalizing()
        {
            Exponential a;
            
            a = new Exponential(34, 3);
            Assert.Equal(3.4m, a.Significand);
            Assert.Equal(4, a.Exponent);

            a = new Exponential(0.0049m, 0);
            Assert.Equal(4.9m, a.Significand);
            Assert.Equal(-3, a.Exponent);

            a = new Exponential("9.1093822e-31");
            Assert.Equal(9.1093822m, a.Significand);
            Assert.Equal(-31, a.Exponent);

        }

        [Fact]
        public void ImplicitTypeConversions()
        {
            Exponential a = 60.0m;
            Assert.Equal(6.00m, a.Significand);
            Assert.Equal(1, a.Exponent);

            a = 4;
            Assert.Equal(4m, a.Significand);
            Assert.Equal(0, a.Exponent);
        }

        [Fact]
        public void ExplicitTypeConversions()
        {
            Exponential a;

            a = (Exponential)4.4;
            Assert.Equal(4.4m, a.Significand);
            Assert.Equal(0, a.Exponent);

            a = (Exponential)60.0;
            Assert.Equal(6.00m, a.Significand);
            Assert.Equal(1, a.Exponent);
        }

        [Fact]
        public void Add()
        {
            Exponential a, b, c;

            a = new Exponential(4.0m);
            b = new Exponential(2.0m);
            c = a + b;
            Assert.Equal(6.0m, c.Significand);
            Assert.Equal(0.1m, c.Error);
            Assert.Equal(0, c.Exponent);

            a = new Exponential(4.0m);
            b = new Exponential(20.0m);
            c = a + b;
            Assert.Equal(2.4m, c.Significand);
            Assert.Equal(0.01m, c.Error);
            Assert.Equal(1, c.Exponent);

            // Normalization test:
            a = 8m;
            b = 7m;
            c = a + b;
            Assert.Equal(1.5m, c.Significand);
            Assert.Equal(0.1m, c.Error);
            Assert.Equal(1, c.Exponent);
        }

        [Fact]
        public void Substracting()
        {
            Exponential a, b, c;

            a = 4.0m;
            b = 2.0m;
            c = a - b;
            Assert.Equal(2.0m, c.Significand);
            Assert.Equal(0.10m, c.Error);
            Assert.Equal(0, c.Exponent);

            a = 12.0m;
            b = 8.0m;
            c = a - b;
            Assert.Equal(4.0m, c.Significand);
            Assert.Equal(0.1m, c.Error);
            Assert.Equal(0, c.Exponent);

            // We have to use string here, because float translates 12.0e3 to 12000 (loses precision information)
            a = new Exponential("12.0e3");
            b = new Exponential("8.0e3");
            c = a - b;
            Assert.Equal(4.0m, c.Significand);
            Assert.Equal(0.1m, c.Error); 
            Assert.Equal(3, c.Exponent);
        }

        [Fact]
        public void Multiplication()
        {
            Exponential a, b, c;
            a = 4.0m;
            b = 2.0m;
            c = a * b;
            Assert.Equal(8.0m, c.Significand);
            Assert.Equal(0.32m, c.Error);
            Assert.Equal(0, c.Exponent);

            // Normalization test:
            a = 40m;
            b = 30m;
            c = a * b;
            Assert.Equal(1.2m, c.Significand);
            Assert.Equal(0.038m, c.Error);
            Assert.Equal(3, c.Exponent);
        }

        [Fact]
        public void Division()
        {
            Exponential a, b, c;
            a = 200.00m;
            b = 50.0m;
            c = a / b;
            Assert.Equal(4.0m, c.Significand);
            Assert.Equal(0.004101m, c.Error);
            Assert.Equal(0, c.Exponent);
        }

        [Fact]
        public void DivisionWithDifferentExponents()
        {
            Exponential a, b, c, d, expected;
            
            a = new Exponential("10.0");
            b = Exponential.Exact("60");
            c = a / b;
            d = c / b; // divide twice by 60
            expected = new Exponential("2.8e-3");
            Assert.True(d.Approximates(expected));
        }

        [Fact]
        public void ExponentialToString()
        {
            Exponential value;
            string s;

            value = new Exponential(4.55555m, 0, 0.07m);
            s = value.ToString();
            Assert.Equal("[4.55±0.07]e0", s);

            value = new Exponential(4.666666666m, 0, 0.01234m);
            s = value.ToString();
            Assert.Equal("[4.67±0.01]e0", s);

            Exponential a = 200.00m, b = 50.0m;
            value = a / b;
            s = value.ToString();
            Assert.Equal("[4.0±0.004]e0", s);
        }
    }
}
