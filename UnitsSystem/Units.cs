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
    public class Prefix
    {
        public string Name; 
        public string Symbol;
        public Exponential Factor; // 10 based exponent

        public Prefix() { }
        public Prefix(string name, string symbol, Exponential factor)
        {
            this.Name = name;
            this.Symbol = symbol;
            this.Factor = factor;
        }
        public Exponential ConvertToBase(Exponential value)
        {
            return value * this.Factor;  // 1kg -> 1000 g
        }
        public Exponential ConvertFromBase(decimal value)
        {
            return value / this.Factor;  // 1000g -> 1kg
        }
        public override string ToString()
        {
            return Symbol;
        }
    }

    public class Unit
    {
        public string Dimension;
        public string Name;
        public string Symbol;
        public Unit(string classification, string name, string symbol)
        {
            this.Dimension = classification;
            this.Name = name;
            this.Symbol = symbol;
        }
        public override string ToString()
        {
            return Symbol;
        }
    }

    public class Metric
    {
        public Unit Unit;
        public Prefix Prefix;
        public Metric(Prefix prefix, Unit unit)
        {
            this.Prefix = prefix;
            this.Unit = unit;
        }
        public string Symbols
        {
            get
            {
                string prefix = (Prefix == null) ? "" : Prefix.Symbol;
                return prefix + Unit.Symbol;
            }
        }
        public override string ToString()
        {
            return Symbols;
        }
    }

    public class Units
    {
        private List<Prefix> prefixes = new List<Prefix>();
        private List<Unit> units = new List<Unit>();
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
        public Prefix FindPrefix(string symbols)
        {
            return prefixes.FirstOrDefault(p => symbols.StartsWith(p.Symbol));
        }
        public Metric ParseMetric(string expression)
        {
            Unit unit = null;
            Prefix prefix = null;
            
            unit = FindUnit(expression);
            if (unit == null)
            {
                prefix = FindPrefix(expression);
                if (prefix == null)
                    throw new ArgumentException(string.Format("Unit or prefix found in {0}", expression));

                int count = prefix.Symbol.Length;
                string s = expression.Remove(0, count);
                unit = FindUnit(s);
            }
            Metric metric = new Metric(prefix, unit);
            return metric;
        }
    }
}
