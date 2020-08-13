/*
* Copyright (c) 2014, Furore (info@furore.com) and contributors
* See the file CONTRIBUTORS for details.
*
* This file is licensed under the BSD 3-Clause license
*/

using System;
using System.Text;


namespace Fhir.Metrics
{
    /// <summary>
    /// Extensions to convert Quantity components to a searchable string
    /// </summary>
    public static class SearchExtensions
    {
        ///<summary>
        /// Creates a string from a decimal that allows compare-from-left string searching 
        /// for finding values that fall within a the precision of a given string representing a decimal.
        /// Each Digit caries the significance of the original digits more to the right.
        /// This method fails for exponents larger than 99
        ///</summary>
        private static string LeftSearchableNumberString(string original, int exponent)
        {
            StringBuilder leftSearchable = new StringBuilder(original);
            int reminder = 0;

            for (int i = leftSearchable.Length - 1; i >= 0; i--)
            {
                if (leftSearchable[i] == '.') continue; // End of the decimal fraction part of the quantity

                // Add the reminder to the number on the left side
                int n = (int)char.GetNumericValue(leftSearchable[i]);
                n += reminder;

                reminder = n / 10; // Reset the reminder to 0 in case n < 10
                n = n % 10;
                if (n > 5)
                    reminder += 1;

                leftSearchable[i] = Convert.ToString(n)[0];
            }

            // Check if we still have a reminder that we need to carry to the left
            if (reminder != 0 && leftSearchable[0] == '0')
                exponent++;

            var minPadLenght = 2;
            var paddedExponend = exponent.ToString().PadLeft(minPadLenght, '0');

            return $"E{paddedExponend}x{leftSearchable}";
        }

        /// <summary>
        /// Transforms the value of a quantity to a string that can be compared from the left.
        /// Each Digit caries the significance of the original digits more to the right.
        /// </summary>
        public static string ValueAsSearchablestring(this Quantity quantity)
        {
            quantity = quantity.UnPrefixed();
            var leftSearchable = LeftSearchableNumberString(quantity.Value.SignificandText, quantity.Value.Exponent);
            return leftSearchable;
        }

        /// <summary>
        /// Transforms the units and the value of a quantity to a single string that can be compared from the left.
        /// Each Digit caries the significance of the original digits more to the right.
        /// </summary>
        public static string LeftSearchableString(this Quantity quantity)
        {
            quantity = quantity.UnPrefixed();
            var leftSearchable = LeftSearchableNumberString(quantity.Value.SignificandText, quantity.Value.Exponent);

            StringBuilder b = new StringBuilder();
            b.Append(quantity.Metric);
            b.Append(leftSearchable);
            return b.ToString();
        }

        public static string LeftSearchableNumberString(string metric, int exponent, string value)
        {
            var leftSearchable = LeftSearchableNumberString(value, exponent);

            StringBuilder b = new StringBuilder();
            b.Append(metric);
            b.Append(leftSearchable);
            return b.ToString();
        }
    }
}
