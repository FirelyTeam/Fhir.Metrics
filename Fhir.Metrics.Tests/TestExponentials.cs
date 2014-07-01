using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fhir.Metrics;
using System.Text.RegularExpressions;

namespace UnitsSystemTests
{
    [TestClass]
    public class TestExponentials
    {
        [TestMethod]
        public void StringConversion()
        {
            Exponential e;

            e = new Exponential("23.343e8");
            Assert.AreEqual(2.3343m, e.Significand);
            Assert.AreEqual(9, e.Exponent);
            Assert.AreEqual(0.0001m * MetricUtils.DEFAULT_ERROR_DIGIT, e.Error);

            e = new Exponential("4.3");
            Assert.AreEqual(4.3m, e.Significand);
            Assert.AreEqual(0, e.Exponent);
            Assert.AreEqual(0.1m * MetricUtils.DEFAULT_ERROR_DIGIT, e.Error);

            e = new Exponential("40");
            Assert.AreEqual(4.0m, e.Significand);
            Assert.AreEqual(1, e.Exponent);
            Assert.AreEqual(0.1m * MetricUtils.DEFAULT_ERROR_DIGIT, e.Error);
        }

        [TestMethod]
        public void NotationErrors()
        {
            Exponential a;
            a = new Exponential(4);
            Assert.AreEqual(4m, a.Significand);
            Assert.AreEqual(1m * MetricUtils.DEFAULT_ERROR_DIGIT, a.Error);
            
            a = new Exponential(4.0m);
            Assert.AreEqual(4m, a.Significand);
            Assert.AreEqual(0.1m * MetricUtils.DEFAULT_ERROR_DIGIT, a.Error);

            a = new Exponential(4e3m);
            Assert.AreEqual(4m, a.Significand);
            Assert.AreEqual(3, a.Exponent);
            Assert.AreEqual(0.001m * MetricUtils.DEFAULT_ERROR_DIGIT, a.Error);
        }

        [TestMethod]
        public void Normalizing()
        {
            Exponential a;
            
            a = new Exponential(34, 3);
            Assert.AreEqual(3.4m, a.Significand);
            Assert.AreEqual(4, a.Exponent);

            a = new Exponential(0.0049m, 0);
            Assert.AreEqual(4.9m, a.Significand);
            Assert.AreEqual(-3, a.Exponent);

            a = new Exponential("9.1093822e-31");
            Assert.AreEqual(9.1093822m, a.Significand);
            Assert.AreEqual(-31, a.Exponent);

            a = new Exponential("13.214");
            Assert.AreEqual(1.3214m, a.Significand);
        }

        [TestMethod]
        public void ImplicitTypeConversions()
        {
            Exponential a = 60.0m;
            Assert.AreEqual(6.00m, a.Significand);
            Assert.AreEqual(1, a.Exponent);

            a = 4;
            Assert.AreEqual(4m, a.Significand);
            Assert.AreEqual(0, a.Exponent);
        }

        [TestMethod]
        public void ExplicitTypeConversions()
        {
            Exponential a;

            a = (Exponential)4.4;
            Assert.AreEqual(4.4m, a.Significand);
            Assert.AreEqual(0, a.Exponent);

            a = (Exponential)60.0;
            Assert.AreEqual(6.00m, a.Significand);
            Assert.AreEqual(1, a.Exponent);
        }

        [TestMethod]
        public void Add()
        {
            Exponential a, b, c;

            a = new Exponential(4.0m);
            b = new Exponential(2.0m);
            c = a + b;
            Assert.AreEqual(6.0m, c.Significand);
            Assert.AreEqual(0.2m * MetricUtils.DEFAULT_ERROR_DIGIT, c.Error);
            Assert.AreEqual(0, c.Exponent);

            a = new Exponential(4.0m);
            b = new Exponential(20.0m);
            c = a + b;
            Assert.AreEqual(2.4m, c.Significand);
            Assert.AreEqual(0.02m * MetricUtils.DEFAULT_ERROR_DIGIT, c.Error);
            Assert.AreEqual(1, c.Exponent);

            // Normalization test:
            a = 8m;
            b = 7m;
            c = a + b;
            Assert.AreEqual(1.5m, c.Significand);
            Assert.AreEqual(0.2m * MetricUtils.DEFAULT_ERROR_DIGIT, c.Error);
            Assert.AreEqual(1, c.Exponent);

            // Different precision #1
            a = new Exponential("13.214");
            b = new Exponential("234.6");
            c = a + b; // 247.81 => 2.4781, err:shift(0.1,-2)=>0.001, default error digit "1" brings us 2 significant digits (81)
            Assert.AreEqual(2.4781m, c.Significand);
            Assert.AreEqual(0.00101m * MetricUtils.DEFAULT_ERROR_DIGIT, c.Error);
            Assert.AreEqual(2, c.Exponent);

            // Different precision #2
            a = new Exponential("13.214e3");
            b = new Exponential("234.6e3");
            c = a + b; // 247.8 => 2.478, err:shift(0.1,-2)=>0.001, default error digit "1" brings us 2 significant digits (81)
            Assert.AreEqual(2.4781m, c.Significand);
            Assert.AreEqual(0.00101m * MetricUtils.DEFAULT_ERROR_DIGIT, c.Error);
            Assert.AreEqual(5, c.Exponent);
        }

        [TestMethod]
        public void Subtraction()
        {
            Exponential a, b, c;

            a = 4.0m;
            b = 2.0m;
            c = a - b;
            Assert.AreEqual(2.0m, c.Significand);
            Assert.AreEqual(0.2m * MetricUtils.DEFAULT_ERROR_DIGIT, c.Error);
            Assert.AreEqual(0, c.Exponent);

            a = 12.0m;
            b = 8.0m;
            c = a - b;
            Assert.AreEqual(4.0m, c.Significand);
            Assert.AreEqual(0.2m * MetricUtils.DEFAULT_ERROR_DIGIT, c.Error);
            Assert.AreEqual(0, c.Exponent);

            // We have to use string here, because float translates 12.0e3 to 12000 (loses precision information)
            a = new Exponential("12.0e3");
            b = new Exponential("8.0e3");
            c = a - b;
            Assert.AreEqual(4.0m, c.Significand);
            Assert.AreEqual(0.2m * MetricUtils.DEFAULT_ERROR_DIGIT, c.Error); 
            Assert.AreEqual(3, c.Exponent);
        }

        [TestMethod]
        public void Multiplication()
        {
            Exponential a, b, c;
            a = new Exponential(4.52m,0,0.02m);
            b = new Exponential(2.0m, 0, 0.2m);
            c = a * b;
            var s = c.ToString();
            Assert.AreEqual("[9.0±0.9]e0", s);

            
            a = new Exponential(4.0m, 0, 0.5m);
            b = new Exponential(2.0m, 0, 0.5m);
            c = a * b;
            Assert.AreEqual(8.0m, c.Significand);
            Assert.AreEqual(3m, c.Error);
            Assert.AreEqual(0, c.Exponent);
            s = c.ToString();
            Assert.AreEqual("[8±3]e0", s);

            a = new Exponential(40.00m, 0, 0.05m);
            b = new Exponential(30.00m, 0, 0.05m);
            
            c = a * b;
            Assert.AreEqual(1.2m, c.Significand);
            Assert.AreEqual(0.0035m, c.Error);
            Assert.AreEqual(3, c.Exponent);
            s = c.ToString();
            Assert.AreEqual("[1.200±0.004]e3", s);
        }

        [TestMethod]
        public void Division()
        {
            Exponential a, b, c;
            a = new Exponential(2.0m, 0, 0.2m);
            b = new Exponential(3.0m, 0, 0.6m);
            c = a / b;
            Assert.AreEqual(7, c.Significand);
            Assert.AreEqual(2, c.Error);
            Assert.AreEqual(-1, c.Exponent);

            a = new Exponential(200.0m, 0, 0.5m);
            b = new Exponential(50.0m, 0, 0.5m);

            c = a / b;
            Assert.AreEqual(4.0m, c.Significand);
            Assert.AreEqual(0.05m, c.Error);
            Assert.AreEqual(0, c.Exponent);
        }

        [TestMethod]
        public void Powers()
        {
            Exponential a = new Exponential(4.1m,0,0.1m);
            Exponential res= Exponential.Power(a, 2);
            Assert.AreEqual(1.68m, res.Significand);
            Assert.AreEqual(1, res.Exponent);
            Assert.AreEqual(0.082m, res.Error);
            var s = res.ToString();
            Assert.AreEqual("[1.68±0.08]e1", s);

            a = new Exponential(4.1m, 0, 0.1m);
            res = Exponential.Power(a, 0.5m);
            Assert.AreEqual(2.02m, res.Significand);
            Assert.AreEqual(0, res.Exponent);
            s = res.ToString();
            Assert.AreEqual("[2.02±0.02]e0", s);

        }

        [TestMethod]
        public void ProductsAndPowers()
        {
            // #1
            Exponential a, b, c, d;
            a = new Exponential(4.52m, 0, 0.02m);
            b = new Exponential(2.0m, 0, 0.2m);
            c = new Exponential(3.02m, 0, 0.6m);

            d = a * Exponential.Power(c, 2) / Exponential.Power(b, 0.5m);

            Assert.AreEqual(2.9m, d.Significand);
            Assert.AreEqual(1, d.Exponent);
            var s = d.ToString();
            Assert.AreEqual("[2.9±1.3]e1", s);


            // #2 
            a = new Exponential(4.52m, 0, 0.02m);
            b = new Exponential(2.0m, 0, 0.2m);
            c = new Exponential(3.0m, 0, 0.6m);

            d = a * b + Exponential.Power(c, 2);

            Assert.AreEqual(1.8m, d.Significand);
            Assert.AreEqual(0.4544m, d.Error);
            Assert.AreEqual(1, d.Exponent);
            s = d.ToString();
            Assert.AreEqual("[1.8±0.5]e1", s); // if results were rounded on the go, the error could be rounded up to 0.4
        }

        [TestMethod]
        public void DivisionWithDifferentExponents()
        {
            Exponential a, b, c, d, expected;
            
            a = new Exponential("10.0");
            b = Exponential.Exact("60");
            c = a / b;
            d = c / b; // divide twice by 60
            expected = new Exponential("2.8e-3");
            Assert.IsTrue(d.Approximates(expected));
        }

        [TestMethod]
        public void ExponentialToString()
        {
            Exponential value;
            string s;

            value = new Exponential(4.55555m, 0, 0.7m);
            s = value.ToString();
            Assert.AreEqual("[4.6±0.7]e0", s);

            value = new Exponential(4.666666666m, 0, 0.01234m);
            s = value.ToString();
            Assert.AreEqual("[4.667±0.012]e0", s);

            Exponential a, b;
            a = new Exponential(200.00m,0,0.01m);
            b = new Exponential(50.0m,0,0.1m);
            value = a / b;
            s = value.ToString();
            Assert.AreEqual("[4.000±0.008]e0", s);

            // we assert the rule: if the first errordigit is 1, we use two-digit precision instead of 1
            value = new Exponential(0.5m, 0, 0.1m);
            s = value.ToString();
            Assert.AreEqual("[5.0±1.0]e-1", s);

            // errordigit is 2: 1-digit precision is used here
            value = new Exponential(0.5m, 0, 0.2m);
            s = value.ToString();
            Assert.AreEqual("[5±2]e-1", s);

        }

        [TestMethod]
        public void ExponentialStringToString()
        {
            var i = new Exponential("4000");
            Assert.AreEqual("[4.000±0.001]e3", i.ToString());
            i = new Exponential("4000.");
            Assert.AreEqual("[4.000±0.001]e3", i.ToString());
            i = new Exponential("4000.0");
            Assert.AreEqual("[4.0000±0.0001]e3", i.ToString());
            // we assert the rule: if the first errordigit is 1, we use two-digit precision instead of 1
            i = new Exponential(".5");
            Assert.AreEqual("[5.0±1.0]e-1", i.ToString());
            i = new Exponential("0.5");
            Assert.AreEqual("[5.0±1.0]e-1", i.ToString());
            i = new Exponential("000.5");
            Assert.AreEqual("[5.0±1.0]e-1", i.ToString());
            i = new Exponential("0.50");
            Assert.AreEqual("[5.00±0.10]e-1", i.ToString());
            i = new Exponential(".505");
            Assert.AreEqual("[5.050±0.010]e-1", i.ToString());
        }

        [TestMethod]
        public void SignificandText()
        {
            Exponential exp = new Exponential("4000");
            string s = exp.SignificandText;
            Assert.AreEqual("4.000", s);

            exp = new Exponential("4");
            s = exp.SignificandText;
            Assert.AreEqual("4", s);

            exp = new Exponential("4.08");
            s = exp.SignificandText;
            Assert.AreEqual("4.08", s);
            exp.Error = 0.1M;
            s = exp.SignificandText;
            Assert.AreEqual("4.1", s);

            exp = new Exponential("4.080");
            exp.Error = 0.01M;
            s = exp.SignificandText;
            Assert.AreEqual("4.08", s);

            exp = new Exponential("4.03");
            exp.Error = 0.5M;
            s = exp.SignificandText;
            Assert.AreEqual("4.0", s);

            exp = new Exponential("4.030");
            exp.Error = 0.12M;
            s = exp.SignificandText;
            Assert.AreEqual("4.03", s);

            exp = new Exponential("4.03");
            exp.Error = 0.0005M;
            s = exp.SignificandText;
            Assert.AreEqual("4.0300", s);
        }


    }
}
