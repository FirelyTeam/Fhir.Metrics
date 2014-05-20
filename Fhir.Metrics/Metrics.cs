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

namespace Fhir.Metrics
{
    public class Metrics
    {
        public List<Prefix> Prefixes = new List<Prefix>();
        public List<Unit> Units = new List<Unit>();

        public Unit FindUnit(string symbol)
        {
            return Units.FirstOrDefault(u => u.Symbol == symbol);
        }

        public Prefix GetPrefix(string symbols)
        {
            return Prefixes.FirstOrDefault(p => symbols.StartsWith(p.Symbol));
        }

        public Metric.Axis ParseAxis(string expression, int exponent)
        {
            Unit unit = null;
            Prefix prefix = null;

            
            unit = FindUnit(expression);
            if (unit == null)
            {
                prefix = GetPrefix(expression);
                if (prefix == null)
                    throw new ArgumentException(string.Format("Unknown Unit or prefix in expression '{0}'", expression));

                int count = prefix.Symbol.Length;
                string s = expression.Remove(0, count);
                unit = FindUnit(s);
                
                if (unit == null)
                    throw new ArgumentException(string.Format("Unknown Unit or prefix in expression '{0}'", expression));
            }
            Metric.Axis component = (unit != null) ? new Metric.Axis(prefix, unit, exponent) : null;
            return component;
        }

        public Metric ParseMetric(string expression)
        {
            return ParseMetric(Parser.ToUnaryTokens(expression));
        }

        public Metric ParseMetric(IEnumerable<Unary> tokens)
        {
            List<Metric.Axis> components = new List<Metric.Axis>();

            foreach (Unary u in tokens)
            {
                Metric.Axis component = ParseAxis(u.Expression, u.Exponent);
                components.Add(component);
            }
            return new Metric(components);
        }
    }
}
