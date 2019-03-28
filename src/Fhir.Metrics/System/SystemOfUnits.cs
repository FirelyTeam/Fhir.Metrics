/*
* Copyright (c) 2014, Furore (info@furore.com) and contributors
* See the file CONTRIBUTORS for details.
*
* This file is licensed under the BSD 3-Clause license
*/

using System;
using System.Text.RegularExpressions;

namespace Fhir.Metrics
{
    /// <summary>
    /// A system that contains units, prefixes and conversions between the units
    /// </summary>
    public class SystemOfUnits
    {
        /// <summary>
        /// The Metric system of the system of units
        /// </summary>
        public Metrics Metrics = new Metrics();
        /// <summary>
        /// The list of conversions that can convert from one unit to another
        /// </summary>
        public Conversions Conversions = new Conversions();


        /// <summary>
        /// Adds a conversion rule to the system
        /// </summary>
        /// <param name="symbolfrom">A single unit symbol that is known in the system</param>
        /// <param name="symbolto">A formula containing numbers and unit symbols defining the target units</param>
        /// <param name="method">A delegate that does the actual conversion of an Exponential</param>
        /// <returns></returns>
        public Conversion AddConversion(string symbolfrom, string symbolto, ConversionMethod method)
        {
            Metric from = Metrics.ParseMetric(symbolfrom);
            Metric to = Metrics.ParseMetric(symbolto);
            return Conversions.Add(from, to, method);
        }
        
        /// <summary>
        /// Addes a prefix to the system
        /// </summary>
        /// <param name="name">The name of the prefix (for example "Kilo")</param>
        /// <param name="symbol">The symbol for the prefix (for example "k")</param>
        /// <param name="factor">The multiplication factor (in case of kilo, 1000)</param>
        /// <returns></returns>
        public Prefix AddPrefix(string name, string symbol, Exponential factor)
        {
            Prefix p = new Prefix(name, symbol, factor);
            Metrics.Prefixes.Add(p);
            return p;
        }
        
        /// <summary>
        /// Adds a unit to the system
        /// </summary>
        /// <param name="name">The name of the unit (for example 'Newton')</param>
        /// <param name="symbol">The symbol for the unit ('N')</param>
        /// <param name="dimension">If the unit is a fundamental unit, the dimension that the unit is expressed in (A meter would be in dimension length)</param>
        /// <returns></returns>
        public Unit AddUnit(string name, string symbol, string dimension = null)
        {
            Unit u = new Unit(name, symbol, dimension);
            Metrics.Units.Add(u);
            return u;
        }

        private Regex regex = new Regex(@"^(\-?\d+(?:\.\d+)?(?:e\d+)?)(.+)?$");

        /// <summary>
        /// Builds a quantity from an Exponential and a metric (parsed from the symbols)
        /// </summary>
        /// <param name="value">The number part of a quantity</param>
        /// <param name="symbols">
        /// An expression containing units that are separated by a multiplication '.' or division '/' and can be followed 
        /// by a power. <para> For example: kg.m.s-2 or kg.m/s2</para>
        /// </param>
        public Quantity Quantity(Exponential value, string symbols)
        {
            Metric metric = Metrics.ParseMetric(symbols);
            return new Quantity(value, metric);
        }

        /// <summary>
        /// Parses a value and symbol string to a Quantity
        /// </summary>
        /// <param name="value">A number as a string. May contain a decimal power (e)</param>
        /// <param name="symbols">
        /// An expression containing units that are separated by a multiplication '.' or division '/' and can be followed 
        /// by a power. <para> For example: kg.m.s-2 or kg.m/s2</para>
        /// </param>
        public Quantity Quantity(string value, string symbols)
        {
            Exponential number = new Exponential(value);
            Metric metric = Metrics.ParseMetric(symbols);
            return new Quantity(number, metric);
        }

        /// <summary>
        /// Parses a string expression containing a number and a set of units to a quantity.
        /// </summary>
        /// <param name="expression">
        /// Must be a number followed by a unit expression (metric).
        /// The units are separated by a multiplication '.' or division '/' and can be followed 
        /// by a power. <para> For example: 1.2e4kg.m.s-2 or 1.2e4kg.m/s2</para>
        /// </param>
        public Quantity Quantity(string expression)
        {
            Match match = regex.Match(expression);
            if (match.Groups.Count != 3)
                throw new ArgumentException("Expression cannot be parsed as a quantity");

            string number = match.Groups[1].Value;
            string symbols = match.Groups[2].Value;

            return this.Quantity(number, symbols);
           
        }

        /// <summary>
        /// Parses a string containing a set of units to a metric.
        /// </summary>
        /// <param name="expression">
        /// Must be a set of known units separated by a multiplication '.' or division '/' and can be followed 
        /// by a power. <para> For example: 1.2e4kg.m.s-2 or 1.2e4kg.m/s2 </para>
        /// </param>
        public Metric Metric(string expression)
        {
            Metric metric = Metrics.ParseMetric(expression);
            return metric;
        }

        /// <summary>
        /// Converts a quantity to standardized units without a prefix
        /// </summary>
        public Quantity Canonical(Quantity quantity)
        {
            Quantity outcome = Conversions.Canonical(quantity);
            return outcome.UnPrefixed();
        }

        /// <summary>
        /// Interprets an expression as a quantity and converts this to standardized units without a prefix
        /// </summary>
        public Quantity Canonical(string expression)
        {
            Quantity quantity = this.Quantity(expression);
            return this.Canonical(quantity);
        }

        public Quantity Convert(Quantity quantity, Metric metric)
        {
            return Conversions.Convert(quantity, metric);
        }
        
        public Quantity Convert(Quantity quantity, string metric)
        {
            Metric m = Metrics.ParseMetric(metric);
            return this.Convert(quantity, m);
        }

        public Quantity Convert(string expression, string metric)
        {
            Quantity q = Quantity(expression);
            Metric m = Metrics.ParseMetric(metric);

            Quantity output = this.Convert(q, m);
            return output;
            
        }

    }
}
