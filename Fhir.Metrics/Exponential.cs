/*
* Copyright (c) 2014, Furore (info@furore.com) and contributors
* See the file CONTRIBUTORS for details.
*
* This file is licensed under the BSD 3-Clause license
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Fhir.Metrics
{
    public struct Exponential
    {
        public decimal Significand; // decimal is working for now, or should we use a float?
        public int Exponent;
        public decimal Error;

        private static readonly string regex = @"^(\-?\d*(\.\d*)?)(e([+-]?\d+))?$";


        public Exponential(decimal value, int exponent, decimal error)
        {
            this.Significand = value;
            this.Exponent = exponent;
            this.Error = error;
            this.Normalize();
        }

        public Exponential(decimal value, int exponent)
        {
            this.Significand = value;
            this.Exponent = exponent;
            this.Error = MetricUtils.NotationError(value);
            this.Normalize();
        }

        public Exponential(decimal value)
        {
            this.Significand = value;
            this.Exponent = 0;
            this.Error = MetricUtils.NotationError(value);
            this.Normalize();
        }

        public Exponential(string s)
        {
            GroupCollection groups = Regex.Match(s, regex).Groups;
            string value = groups[1].Value;
            string exp = groups[4].Value;
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException(string.Format("Expression '{0}' is not a valid exponential number ", s));

            if (string.IsNullOrEmpty(exp)) exp = "0";

            this.Significand = MetricUtils.StringToDecimal(value);
            this.Exponent = Convert.ToInt32(exp);
            this.Error = MetricUtils.NotationError(this.Significand);
            this.Normalize();
        }

        public static Exponential Exact(decimal value)
        {
            return new Exponential(value, 0, 0);
        }

        public static Exponential Exact(string s)
        {
            Exponential e = new Exponential(s);
            e.Error = 0;
            return e;
        }

        public Exponential Raised(int n)
        {
            Exponential e = Exponential.CopyOf(this);
            e.Exponent += n;
            return e;
        }


        public static Exponential CopyOf(Exponential e)
        {
            return new Exponential(e.Significand, e.Exponent, e.Error);
        }

        public static Exponential Normalize(Exponential e)
        {
            Exponential result = Exponential.CopyOf(e);
            result.Normalize();
            return result;
        }

        public void Normalize()
        {
            decimal value = this.Significand;
            if (value == 0)
                return;

            int E = 0;
            while (Math.Abs(value) >= 10)
            {
                value /= 10;
                E += 1;
            }
            while (Math.Abs(value) < 1)
            {
                value *= 10;
                E -= 1;
            }
            this.Exponent += E;
            this.Error = MetricUtils.Shift(this.Error, -E);
            // NOTE (ER): we only round up when we want a representation of the number.
            // This leads us to use the significand at a possibly wrong level of precision while operating on it.
            // We could alternatively round here the error and consequently the significand as follows:
            //this.Error = MetricUtils.RoundUncertainty(MetricUtils.Shift(this.Error, -E));
            this.Significand = MetricUtils.StringToDecimal(MetricUtils.DecimalToString(value, this.Error));
        }

        public static Exponential Add(Exponential a, Exponential b)
        {
            Exponential result;
            a = rebase(a, b);

            result.Significand = a.Significand + b.Significand;
            result.Exponent = a.Exponent;
            result.Error = sumErrorProp(a, b);
            result.Normalize();
            return result;
        }

        public static Exponential Substract(Exponential a, Exponential b)
        {
            Exponential result;
            a = rebase(a, b);

            result.Significand = a.Significand - b.Significand;
            result.Exponent = a.Exponent;
            result.Error = sumErrorProp(a,b);
            result.Normalize();
            return result;
        }

        public static Exponential Multiply(Exponential a, Exponential b)
        {
            Exponential result;
            result.Significand = a.Significand * b.Significand;
            result.Exponent = a.Exponent + b.Exponent;

            result.Error = factorErrorProp(result.Significand, a, b);

            result.Normalize();
            return result;
        }

        /// <summary>
        /// Divides two exponentials
        /// </summary>
        /// <param name="a">numerator</param>
        /// <param name="b">denominator</param>
        /// <returns></returns>
        public static Exponential Divide(Exponential a, Exponential b)
        {
            Exponential result;
            result.Significand = a.Significand / b.Significand;
            result.Exponent = a.Exponent - b.Exponent;
            result.Error = factorErrorProp(result.Significand, a, b);
            
            result.Normalize();
            return result;
        }

        public static Exponential operator +(Exponential a, Exponential b)
        {
            return Exponential.Add(a, b);
        }

        public static Exponential operator -(Exponential a, Exponential b)
        {
            return Exponential.Substract(a, b);
        }

        public static Exponential operator *(Exponential a, Exponential b)
        {
            return Exponential.Multiply(a, b);
        }

        public static Exponential operator /(Exponential a, Exponential b)
        {
            return Exponential.Divide(a, b);
        }

        public static bool operator ==(Exponential a, Exponential b)
        {
            return (a.Significand == b.Significand) && (a.Exponent == b.Exponent) && (a.Error == b.Error);
        }

        public static bool operator !=(Exponential a, Exponential b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static implicit operator Exponential(decimal value)
        {
            return new Exponential(value);
        }

        public static explicit operator Exponential(double value)
        {
            decimal d = (decimal)value;
            return new Exponential(d);
        }

        public static implicit operator Exponential(int value)
        {
            return new Exponential(value, 0, 0);
        }

        public static explicit operator decimal(Exponential value)
        {
            return value.ToDecimal();
        }

        public string SignificandText
        {
            get
            {
                return MetricUtils.DecimalToString(this.Significand, MetricUtils.RoundUncertainty(this.Error));
            }
        }

        public override string ToString()
        {
            string error = MetricUtils.DecimalToString(MetricUtils.RoundUncertainty(this.Error));
            return string.Format("[{0}±{2}]e{1}", SignificandText, this.Exponent, error);
        }

        public decimal ToDecimal()
        {
            return this.Significand * (decimal)Math.Pow(10, this.Exponent);
            // todo: remove precision to reflect real error
        }

        public double ValueToFloat()
        {
            return (double)this.Significand * Math.Pow(10, this.Exponent);
        }

        public double ErrorToFloat()
        {
            return (double)this.Error * Math.Pow(10, this.Exponent);
        }

        /// <summary>
        /// Tests if this number including error margin lies within the error margin of the given value.
        /// </summary>
        /// <param name="e">the value that determines the range</param>
        public bool Approximates(Exponential e)
        {
            bool expo = (this.Exponent == e.Exponent);
            bool sigp = (e.Significand + e.Error) >= (this.Significand + this.Error);
            bool sigm = (e.Significand - e.Error) <= (this.Significand - this.Error);

            return expo && sigp && sigm;
        }

        public bool Approximates(string s)
        {
            Exponential e = new Exponential(s);
            return Approximates(e);
        }

        public static Exponential Power(Exponential value, decimal power)
        {
            decimal f = (decimal)Math.Pow(value.ValueToFloat(), (double)power);
            decimal e = powerErrorProp(f, value, power);

            return new Exponential(f, 0, e);
        }

        /// <summary>
        /// Raise value with 10^digits.
        /// </summary>
        public static Exponential Raise(Exponential value, int digits)
        {
            return value.Raised(digits);
        }

        /// <summary>
        /// Returns Exponential of exactly 1 with no measurement error.
        /// </summary>
        public static Exponential One
        {
            get { return Exponential.Exact(1); }
        }

        /// <summary>
        /// Returns Exponential of exactly 0 with no measurement error.
        /// </summary>
        public static Exponential Zero
        {
            get { return Exponential.Exact(0); }
        }

        private static Exponential rebase(Exponential a, Exponential b)
        {
            // base the value of a to the exponent of b
            Exponential result;

            int dex = b.Exponent - a.Exponent; // dex = decimal exponent
            result.Significand = MetricUtils.Shift(a.Significand, -dex);
            result.Error = MetricUtils.Shift(a.Error, -dex);
            result.Exponent = a.Exponent + dex;
            return result;
        }

        private static decimal sumErrorProp(Exponential a, Exponential b)
        {
            return a.Error + b.Error;
        }

        private static decimal factorErrorProp(decimal significant, Exponential a, Exponential b)
        {
            decimal delta_a = (a.Significand != 0) ? (a.Error / a.Significand) : 0;
            decimal delta_b = (b.Significand != 0) ? (b.Error / b.Significand) : 0;
            decimal factor = delta_a + delta_b;
            return significant * factor;
        }

        private static decimal powerErrorProp(decimal significant, Exponential a, decimal power)
        {
            if (significant == 0) return 0;
            return significant * power * a.Error / a.Significand;
        }

    }
}