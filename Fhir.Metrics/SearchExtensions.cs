/*
* Copyright (c) 2014, Furore (info@furore.com) and contributors
* See the file CONTRIBUTORS for details.
*
* This file is licensed under the BSD 3-Clause license
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fhir.Metrics
{
    public static class SearchExtensions
    {
        ///<summary>
        /// Creates a string from a decimal that allows compare-from-left string searching 
        /// for finding values that fall within a the precision of a given string representing a decimal .
        ///</summary>
        public static string LeftSearchableString(decimal value)
        {
            string s = Convert.ToString(value, new CultureInfo("en-US"));
            StringBuilder b = new StringBuilder(s);

            int reminder = 0;

            for (int i = b.Length - 1; i >= 0; i--)
            {
                if (b[i] == '.') continue;
                int n = (int)Char.GetNumericValue(b[i]);
                n += reminder;

                reminder = n / 10;
                n = n % 10;
                reminder += (n > 5) ? 1 : 0;
                char c = Convert.ToString(n)[0];
                b[i] = c;

            }
            return b.ToString();
        }

        public static string LeftSearchableString(this Quantity quantity)
        {
            decimal value = quantity.Value.Significand;
            StringBuilder b = new StringBuilder();
            b.Append(quantity.Metric);
            b.Append("E");
            b.Append(quantity.Value.Exponent.ToString());
            b.Append("+");
            b.Append(LeftSearchableString(value));
            return b.ToString();
        }
    }
}
