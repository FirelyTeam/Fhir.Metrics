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

namespace Fhir.UnitsSystem
{
    public class Metrics
    {
        public List<Constant> Constants = new List<Constant>();
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

        public Constant FindConstant(string symbols)
        {
            return Constants.FirstOrDefault(c => c.Symbols == symbols);
        }

        public bool ConsumeConstant(string symbols, out Constant constant, out string rest)
        {
            constant = Constants.FirstOrDefault(f => symbols.StartsWith(f.Symbols));
            if (constant != null)
            {
                rest = symbols.Substring(0, constant.Symbols.Length);
                return true;
            }
            else
            {
                rest = symbols;
                return false;
            }
        }
        public Metric.Axis ParseComponent(string expression, int exponent)
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
            }
            Metric.Axis component = (unit != null) ? new Metric.Axis(prefix, unit, exponent) : null;
            return component;
        }

        public Metric ParseMetric(string expression)
        {
            List<Metric.Axis> components = new List<Metric.Axis>();

            foreach(Unary u in Parser.ToUnaryTokens(expression))
            {
                Metric.Axis component = ParseComponent(u.Expression, u.Exponent);
                components.Add(component);
            }
            return new Metric(components);
        }
    }
}
