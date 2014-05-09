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
        private List<Constant> constants = new List<Constant>();
        private List<Prefix> prefixes = new List<Prefix>();
        private List<Unit> units = new List<Unit>();

        public void Add(Constant constant)
        {
            constants.Add(constant);
        }
        
        public void Add(Unit unit)
        {
            units.Add(unit);
        }

        public void Add(Prefix prefix)
        {
            prefixes.Add(prefix);
        }

        public Unit Add(string dimension, string name, string symbol)
        {
            Unit u = new Unit(dimension, name, symbol);
            units.Add(u);
            return u;
        }

        public Prefix Add(string name, string symbol, Exponential factor)
        {
            Prefix p = new Prefix(name, symbol, factor);
            prefixes.Add(p);
            return p;
        }

        public Unit FindUnit(string symbol)
        {
            return units.FirstOrDefault(u => u.Symbol == symbol);
        }

        public Prefix GetPrefix(string symbols)
        {
            return prefixes.FirstOrDefault(p => symbols.StartsWith(p.Symbol));
        }

        public Constant FindConstant(string symbols)
        {
            return constants.FirstOrDefault(c => c.Symbols == symbols);
        }

        public bool ConsumeConstant(string symbols, out Constant constant, out string rest)
        {
            constant = constants.FirstOrDefault(f => symbols.StartsWith(f.Symbols));
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
        public Metric.Component ParseComponent(string expression, int exponent)
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
            Metric.Component component = (unit != null) ? new Metric.Component(prefix, unit, exponent) : null;
            return component;
        }

        public Metric ParseMetric(string expression)
        {
            List<Metric.Component> components = new List<Metric.Component>();

            foreach(Unary u in Parser.ToUnaryTokens(expression))
            {
                Metric.Component component = ParseComponent(u.Expression, u.Exponent);
                components.Add(component);
            }
            return new Metric(components);
        }
    }
}
