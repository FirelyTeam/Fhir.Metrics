/*
* Copyright (c) 2014, Furore (info@furore.com) and contributors
* See the file CONTRIBUTORS for details.
*
* This file is licensed under the BSD 3-Clause license
*/
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
        public decimal Value;

        public Prefix() { }
        public Prefix(string name, string symbol, decimal value)
        {
            this.Name = name;
            this.Symbol = symbol;
            this.Value = value;
        }
        public decimal ConvertToBase(decimal value)
        {
            decimal factor = this.Value;   
            return value * factor;  // 1kg -> 1000 g
        }
        public decimal ConvertFromBase(decimal value)
        {
            decimal factor = this.Value;
            return value / factor;  // 1000g -> 1kg
        }
        public override string ToString()
        {
            return Symbol;
        }
    }

    public class Unit
    {
        public string Classification;
        public string Name;
        public string Symbol;
        public Unit(string classification, string name, string symbol)
        {
            this.Classification = classification;
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
        public Unit Add(string classification, string name, string symbol)
        {
            Unit u = new Unit(classification, name, symbol);
            units.Add(u);
            return u;
        }
        public Prefix Add(string name, string symbol, decimal value)
        {
            Prefix p = new Prefix(name, symbol, value);
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
                int count = prefix.Symbol.Length;
                string s = expression.Remove(0, count);
                unit = FindUnit(s);
            }
            Metric metric = new Metric(prefix, unit);
            return metric;
        }
    }
}
