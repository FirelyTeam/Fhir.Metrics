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
    /// <summary>
    /// Extensions to convert Quantity components to a searchable string
    /// </summary>
    public static class SearchExtensions
    {
        ///<summary>
        /// Creates a string from a decimal that allows compare-from-left string searching 
        /// for finding values that fall within the precision of a given string representing a decimal .
        ///</summary>
        private static string LeftSearchableNumberString(string s)
        {
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


        /// <summary>
        /// Transforms the value of a quantity to a string that can be compared from the left.
        /// Each Digit caries the significance of the original digits more to the right.
        /// </summary>
        public static string ValueAsSearchablestring(this Quantity quantity)
        {
            quantity = quantity.UnPrefixed();
            
            StringBuilder b = new StringBuilder();
            b.Append("E");
            b.Append(quantity.Value.Exponent.ToString());
            b.Append("x");
            string value = quantity.Value.SignificandText;
            b.Append(LeftSearchableNumberString(value));
            return b.ToString();
        }

        /// <summary>
        /// Transforms the units and the value of a quantity to a single string that can be compared from the left.
        /// Each Digit caries the significance of the original digits more to the right.
        /// </summary>
        public static string LeftSearchableString(this Quantity quantity)
        {
            quantity = quantity.UnPrefixed();
            StringBuilder b = new StringBuilder();
            b.Append(quantity.Metric);
            b.Append(quantity.ValueAsSearchablestring());
            return b.ToString();
        }

        public static bool SearchLeft(this Quantity needle, Quantity haystack)
        {
            string n = needle.LeftSearchableString();
            string h = haystack.LeftSearchableString();
            return h.StartsWith(n);
        }
    }
}
