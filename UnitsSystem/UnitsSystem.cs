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

        public void AddConversion(string symbolfrom, string symbolto, ConversionMethod method)
        {
            Unit unitfrom = Units.FindUnit(symbolfrom);
            Unit unitto = Units.FindUnit(symbolto);
            Conversions.Add(unitfrom, unitto, method);
        }
        public Unit AddUnit(string dimension, string name, string symbol)
        {
            return Units.Add(dimension, name, symbol);
        }
        public Prefix AddPrefix(string name, string symbol, Exponential factor)
        {
            return Units.Add(name, symbol, factor);
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

        public Quantity ExpressionToQuantity(string expression)
        {
            MatchCollection matches = Regex.Matches(expression, @"(\-?\d+((\,|\.)\d+)?(e\d+)?)(.+)");
            
            string number = matches[0].Groups[1].Value;
            string symbols = matches[0].Groups[5].Value;
            
            Exponential value = new Exponential(number);
            Metric metric = Units.ParseMetric(symbols);
            
            Quantity quantity = new Quantity(value, metric);
            
            return quantity;
        }

        public Quantity Convert(string expression, string metric)
        {
            Quantity q = ExpressionToQuantity(expression);
            Metric m = Units.ParseMetric(metric);

            Quantity output = this.Convert(q, m);
            return output;
            
        }

        public Quantity ConvertToSsytem(string expression, string system)
        {
            Quantity q = ExpressionToQuantity(expression);
            return Conversions.ConvertToSystem(q, system);
        }

        
    }
}
