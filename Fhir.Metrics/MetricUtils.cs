using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Fhir.Metrics
{
    public class MetricUtils
    {
        public const decimal DEFAULT_ERROR_DIGIT=1m;

        private static readonly IFormatProvider format = new CultureInfo("en-US");

        // convert a decimal to a string, using the optional "error" (default=0) to format and round it to a specific precision.
        public static string DecimalToString(decimal value, decimal error = 0)
        {
            if (error == 0) return value.ToString(format);
            string errstr = DecimalToString(RoundUncertainty(error));
            var pt = errstr.IndexOf('.');

            var prec = pt >= 0 ? errstr.Substring(pt + 1).Length : 0;
            var roundpos = prec <= 0 ? 0 : prec;
            return Round(value, roundpos, prec);
        }

        // convert a string to a decimal number conserving the original precision
        public static decimal StringToDecimal(string s)
        {
            return (string.IsNullOrEmpty(s)) ? 0 : Convert.ToDecimal(s, format);
        }

        // identify the precision of s and return the default error digit for that decimal position.
        public static decimal NotationError(decimal s)
        {
            // this function ASSUMES there are no exponents 
            string value = DecimalToString(s);
            int c = value.Length;
            int pt = value.IndexOf('.');
            int precision = (pt >= 0) ? c - pt - 1: 0;

            decimal error = Shift(DEFAULT_ERROR_DIGIT, -precision);
            return error;
        }

        // shift a decimal to the left up to the desired amount of digits
        public static decimal Shift(decimal d, int digits)
        {
            return d * (decimal)Math.Pow(10, digits);
        }

        // return the significant figures in a number or "string.Empty" if there are no significant figures.
        // n.b.: the trailing zeros of a number not containig decimal points are considered significant.
        public static string SignificantFigures(string s)
        {
            var firstSignificant = -1;
            var lastSignificant = -1;
            s = s.Replace(".", string.Empty);
            var matches = Regex.Matches(s, "[1-9]");
            if (matches.Count > 0)
            {
                firstSignificant = matches[0].Index;
                lastSignificant = s.Length - 1;
                return s.Substring(firstSignificant, 1 + lastSignificant - firstSignificant);
            }
            return string.Empty;
        }

        // round and format a decimal up to a specific precision (position is relative to the comma and can be negative to act at the left side)
        public static string Round(Decimal s, int pos)
        {
            return Round(s, pos, pos < 0 ? 0 : pos);
        }

        // round a decimal to a specific fractional digit and format it to a specific decimal precision.
        // pos is relative to the comma and can be negative. A negative pos will round the number at the left side of the comma.
        // prec is relative to the comma and can not be negative.
        public static string Round(Decimal s, int pos, int prec)
        {
            if (prec < 0) throw new ArgumentOutOfRangeException("Parameter prec cannot be negative");
            var rounded=pos<1?Shift(Math.Round(Shift(s, pos), -pos), -pos):Math.Round(s, pos);
            return ToPrecision(DecimalToString(rounded), prec);
        }

        // format a decimal string with the desired precision (as in digits after the comma)
        public static string ToPrecision(string s, int prec)
        {
            StringBuilder b;
            var pt = s.IndexOf('.');
            var currentPrec = 0;
            if (pt > 0)
            {
                var delta = prec > 0 ? 1 : 0; // when precision=0 the substring avoids the dot
                b = new StringBuilder(s.Substring(0, Math.Min(s.Length, prec + pt + delta)));
                currentPrec = s.Length - pt - 1;
            }
            else
            {
                b = new StringBuilder(s);
                if (prec > 0) b.Append('.');
            }
            while (currentPrec < prec)
            {
                b.Append('0');
                currentPrec++;
            }
            return b.ToString();
        }

        // If first significant digit of uncertainty is 1, use 2 significant figures to round it off, otherwise use 1 significant figure
        // Return the uncertainty rounded off at the first or second significant figure
        public static decimal RoundUncertainty(decimal uncertainty)
        {
            var rounded = uncertainty;
            var strUncertainty = MetricUtils.DecimalToString(uncertainty);
            var dotPos = strUncertainty.IndexOf('.');
            string significantDigits = MetricUtils.SignificantFigures(strUncertainty);
            if (significantDigits.Length > 0)
            {
                // find the position of the first significant digit relative to the position of the comma (can be negative)
                var firstSignDigit = significantDigits[0];
                var firstSignDigitPos = strUncertainty.IndexOf(firstSignDigit) - (dotPos > 0 ? dotPos : 0);

                //var firstSignDigitPos = strUncertainty.IndexOf(firstSignDigit);
                //var deltaFirstSignDigitPos = (dotPos > 0 && dotPos > firstSignDigitPos) ? dotPos : 0;
                

                var availableDecimalDigits = dotPos < 0 ? 0 : strUncertainty.Length - dotPos - 1;
                // round off uncertainty to 2 or 1 significant figures if first significant figure is respectively 1 or bigger
                //var roundOffPoint = firstSignDigit == '1' ? firstSignDigitPos + (firstSignDigitPos == -1 ? 2 : 1) : firstSignDigitPos;
                int roundOffPoint = 0;
                if (firstSignDigit == '1')
                {
                    roundOffPoint = firstSignDigitPos + (firstSignDigitPos == -1 ? 2 : 1);
                }
                else
                {
                    roundOffPoint = firstSignDigitPos;
                }
                return MetricUtils.StringToDecimal(MetricUtils.Round(uncertainty, Math.Min(availableDecimalDigits, roundOffPoint)));
            }
            return 0;
        }

    }
}