/*
* Copyright (c) 2014, Furore (info@furore.com) and contributors
* See the file CONTRIBUTORS for details.
*
* This file is licensed under the BSD 3-Clause license
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Fhir.Metrics
{
    public class Metrics
    {
        private Dictionary<string, Prefix> _prefixes;
        public List<Prefix> Prefixes => _prefixes.Values.ToList();

        public Dictionary<string, Unit> _units;
        public List<Unit> Units => _units.Values.ToList();

        private const string UnitySymbol = "1";
        internal static Unit Unity = new Unit("Unity", UnitySymbol, "");

        public Metrics()
        {
            _units = new Dictionary<string, Unit>();
            _units.Add(UnitySymbol, Unity);

            _prefixes = new Dictionary<string, Prefix>();
        }

        public Unit FindUnit(string symbol)
        {
            return _units.FirstOrDefault(u => u.Key == symbol).Value;
        }

        public void AddUnit(Unit unit)
        {
            if (!_units.ContainsKey(unit.Symbol))
                _units.Add(unit.Symbol, unit);
        }

        public void AddPrefix(Prefix prefix)
        {
            if (!_prefixes.ContainsKey(prefix.Symbol))
                _prefixes.Add(prefix.Symbol, prefix);
        }

        public Prefix GetPrefix(string symbols)
        {
            return _prefixes.FirstOrDefault(p => symbols.StartsWith(p.Key)).Value;
        }

        public Metric.Axis ParseAxis(string expression, int exponent)
        {
            Prefix prefix = null;
            Unit unit = FindUnit(expression);
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
