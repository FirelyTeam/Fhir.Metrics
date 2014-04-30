using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Globalization;

namespace UnitsOfMeasure
{
    public struct Exponential
    {
        public decimal Value;
        public int Exponent;
        public decimal Error;
        private const string regex = @"(\d+(\.\d+)?)(e([+-]?\d+))?";

        public Exponential(decimal value, int exponent, decimal error)
        {
            this.Value = value;
            this.Exponent = exponent;
            this.Error = error;
            normalize();
        }

        public Exponential(decimal value, int exponent)
        {
            this.Value = value;
            this.Exponent = exponent;
            this.Error = notationError(value);
            normalize();
        }

        public Exponential(decimal value)
        {
            this.Value = value;
            this.Exponent = 0;
            this.Error = notationError(value);
            normalize();
        }

        public Exponential(string s)
        {
            GroupCollection groups = Regex.Match(s, regex).Groups;
            string value = groups[1].Value;
            string exp = groups[4].Value;
            if (string.IsNullOrEmpty(exp)) exp = "0";
            
            this.Value = StringToDecimal(value);
            this.Exponent = Convert.ToInt32(exp);
            this.Error = notationError(this.Value);
            normalize();
        }

        private static IFormatProvider format = new CultureInfo("en-US");
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

       

        public static Exponential Copy(Exponential e)
        {
            return new Exponential(e.Value, e.Exponent, e.Error);
        }

        public void normalize()
        {
            decimal value = this.Value;
            if (value == 0)
                return;

            int E = 0;
            while (value > 10)
            {
                value /=  10;
                E += 1;
            }
            while (value < 1)
            {
                value *= 10;
                E -= 1;
            }
            this.Value = value;
            this.Exponent += E;
            this.Error = Shift(this.Error, -E);
        }

        public void Add(Exponential exponential)
        {
            
        }

        public override string ToString()
        {
            return string.Format("{0}e{1} ± {2}", this.Value, this.Exponent, this.Error);
        }
    }
}
