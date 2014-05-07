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
    public class UnitsSystem
    {
        public Units Units = new Units();
        public Conversions Conversions = new Conversions();
        
        public Conversion AddConversion(string symbolfrom, string symbolto, ConversionMethod method)
        {
            Unit unitfrom = Units.FindUnit(symbolfrom);
            Unit unitto = Units.FindUnit(symbolto);
            return Conversions.Add(unitfrom, unitto, method);
        }
        
        public Prefix AddPrefix(string name, string symbol, Exponential factor)
        {
            return Units.Add(name, symbol, factor);
        }

        public Unit AddUnit(string classification, string name, string symbol)
        {
            return Units.Add(classification, name, symbol);
        }

        public Quantity Quantity(string expression)
        {
            MatchCollection matches = Regex.Matches(expression, @"(\-?\d+((\,|\.)\d+)?(e\d+)?)(.+)");

            string number = matches[0].Groups[1].Value;
            string symbols = matches[0].Groups[5].Value;

            Exponential value = new Exponential(number);
            Metric metric = Units.ParseMetric(symbols);

            Quantity quantity = new Quantity(value, metric);

            return quantity;
        }

        public Quantity Convert(Quantity quantity, Unit unit)
        {
            return Conversions.Convert(quantity, unit);
        }
        
        public Quantity Convert(Quantity quantity, Metric metric)
        {
            return Conversions.Convert(quantity, metric);
        }
        
        public Quantity Convert(Quantity quantity, string metric)
        {
            Metric m = Units.ParseMetric(metric);
            return this.Convert(quantity, m);
        }

        public Quantity Convert(string expression, string metric)
        {
            Quantity q = Quantity(expression);
            Metric m = Units.ParseMetric(metric);

            Quantity output = this.Convert(q, m);
            return output;
            
        }

        public Quantity ConvertToSsytem(string expression, string system)
        {
            Quantity q = Quantity(expression);
            return Conversions.ConvertToSystem(q, system);
        }

    }
}
