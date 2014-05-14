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

namespace Fhir.UnitsSystem
{
    public struct Exponential
    {
        public decimal Significand; // decimal is working for now, or should we use a float?
        public int Exponent;
        public decimal Error;
        
        private static readonly string regex = @"^(\d+(\.\d+)?)(e([+-]?\d+))?$";
        private static readonly IFormatProvider format = new CultureInfo("en-US");

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
            this.Error = notationError(value);
            this.Normalize();
        }

        public Exponential(decimal value)
        {
            this.Significand = value;
            this.Exponent = 0;
            this.Error = notationError(value);
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
            
            this.Significand = StringToDecimal(value);
            this.Exponent = Convert.ToInt32(exp);
            this.Error = notationError(this.Significand);
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

        public static string DecimalToString(decimal d)
        {
            return d.ToString(format);
        }

        public static decimal StringToDecimal(string s)
        {
            return (string.IsNullOrEmpty(s)) ? 0 : Convert.ToDecimal(s, format);
        }

        public static decimal Shift(decimal d, int digits)
        {
            return d * (decimal)Math.Pow(10, digits);
        }

        public Exponential Raised(int n)
        {
            Exponential e = Exponential.CopyOf(this);
            e.Exponent += n;
            return e;
        }

        private static decimal notationError(decimal value)
        {
            // this function ASSUMES there are no exponents 
            string s = DecimalToString(value);
            int c = s.Length;
            int pt = s.IndexOf('.');
            int precision = (pt >= 0) ? c - pt : 1;

            decimal error = Shift(5m, -precision); 
            return error;
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
            while (Math.Abs(value) > 10)
            {
                value /=  10;
                E += 1;
            }
            while (Math.Abs(value) < 1)
            {
                value *= 10;
                E -= 1;
            }
            this.Significand = value;
            this.Exponent += E;
            this.Error = Shift(this.Error, -E);
        }

        private static Exponential rebase(Exponential a, Exponential b)
        {
            // base the value of a to the exponent of b
            Exponential result;
            
            int dex = b.Exponent - a.Exponent; // dex = decimal exponent
            result.Significand = Shift(a.Significand, -dex);
            result.Error = Shift(a.Error, -dex);
            result.Exponent = a.Exponent + dex;
            return result;
        }
        
        public static Exponential Add(Exponential a, Exponential b)
        {
            Exponential result;
            a = rebase(a, b);

            result.Significand = a.Significand + b.Significand;
            result.Exponent = a.Exponent;
            result.Error = a.Error + b.Error;
            result.Normalize();
            return result;
        }

        public static Exponential Substract(Exponential a, Exponential b)
        {
            Exponential result;
            a = rebase(a, b);

            result.Significand = a.Significand - b.Significand;
            result.Exponent = a.Exponent + b.Exponent;
            result.Error = a.Error - b.Error;
            result.Normalize();
            return result;
        }

        public static Exponential Multiply(Exponential a, Exponential b)
        {
            Exponential result;
            result.Significand = a.Significand * b.Significand;
            result.Exponent = a.Exponent + b.Exponent;
            result.Error = a.Significand * b.Error + b.Significand * a.Error;
            result.Normalize();
            return result;
        }

        public static Exponential Divide(Exponential a, Exponential b)
        {
            Exponential result;
            result.Significand = a.Significand / b.Significand;
            result.Exponent = a.Exponent - b.Exponent;
            result.Error = a.Significand * b.Error + b.Significand * a.Error;
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

        public override string ToString()
        {
            return string.Format("[{0} ± {2}]E{1} ", this.Significand, this.Exponent, this.Error);
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
        /// Tests if the an number including error margin lies within the error margin of the given value.
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

        public static Exponential Power(Exponential value, int power)
        {
            double f = Math.Pow(value.ValueToFloat(), power);
            double e = Math.Pow(value.ErrorToFloat(), power);

            return new Exponential((decimal)f, 0, (decimal)e); ;
            
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
            get {
                return Exponential.Exact(1);
            }
        }
        
        /// <summary>
        /// Returns Exponential of exactly 0 with no measurement error.
        /// </summary>
        public static Exponential Zero
        {
            get {
                return Exponential.Exact(0);
            }
        }
    }

 
}
