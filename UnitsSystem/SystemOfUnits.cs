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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fhir.UnitsSystem
{
    public class SystemOfUnits
    {
        public Metrics Metrics = new Metrics();
        public Conversions Conversions = new Conversions();

        public Conversion AddConversion(string symbolfrom, string symbolto, ConversionMethod method)
        {
            Metric from = Metrics.ParseMetric(symbolfrom);
            Metric to = Metrics.ParseMetric(symbolto);
            return Conversions.Add(from, to, method);
        }
        
        public Prefix AddPrefix(string name, string symbol, Exponential factor)
        {
            Prefix p = new Prefix(name, symbol, factor);
            Metrics.Prefixes.Add(p);
            return p;
        }
        
        public Unit AddUnit(string name, string symbol, string dimension = null)
        {
            Unit u = new Unit(name, symbol, dimension);
            Metrics.Units.Add(u);
            return u;
        }

        private Regex quant = new Regex(@"^(\-?\d+(?:\.\d+)?(?:e\d+)?)(.+)?$");
        
        /// <summary>
        /// Parses a string to a quantity with all units interpreted.
        /// The string must be of format number followed by a unit expression (metric).
        /// The units are separated by a multiplication '.' or division '/' and can be followed 
        /// by a power. <para> For example: 1.2e4kg.m.s-2 </para>
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public Quantity Quantity(string expression)
        {
            Match match = quant.Match(expression);
            string[] result = quant.Split(expression);
            if (match.Groups.Count != 3)
                throw new ArgumentException("Expression cannot be parsed as a quantity");

            string number = match.Groups[1].Value;
            string symbols = match.Groups[2].Value;

            Exponential value = new Exponential(number);
            Metric metric = Metrics.ParseMetric(symbols);

            Quantity quantity = new Quantity(value, metric);

            return quantity;
        }

        public Metric Metric(string expression)
        {
            Metric metric = Metrics.ParseMetric(expression);
            return metric;
        }

        public Quantity ToBase(Quantity quantity)
        {
            return Conversions.ToBaseUnits(quantity);
        }

        public Quantity ToBase(string expression)
        {
            Quantity quantity = this.Quantity(expression);
            return this.ToBase(quantity);
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
