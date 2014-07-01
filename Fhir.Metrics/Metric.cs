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
    public class Metric
    {
        public class Axis : IComparable<Axis>
        {
            public Prefix Prefix;
            public Unit Unit;
            public int Exponent;

            public string Symbols
            {
                get
                {
                    string prefix = (Prefix == null) ? "" : Prefix.Symbol;
                    string exp = (Exponent != 1) ? Exponent.ToString() : "";
                    return prefix + Unit.Symbol + exp;
                }
            }

            public Axis(Prefix prefix, Unit unit, int exponent = 1)
            {
                this.Prefix = prefix;
                this.Unit = unit;
                this.Exponent = exponent;
            }

            public Axis Merge(Axis other)
            {
                if (this.Unit != other.Unit || this.Prefix != other.Prefix)
                    throw new ArgumentException("Cannot merge metric components with a different unit or prefix");

                int exponent = this.Exponent + other.Exponent;
                Axis target = new Axis(this.Prefix, this.Unit, exponent);
                return target;
            }

            /// <summary>
            /// Converts an number value to represent the value of the axis unit without a prefix.
            /// </summary>
            /// <returns></returns>
            public Exponential UnPrefix(Exponential value)
            {
                if (Prefix != null) 
                {
                    Exponential factor = Exponential.Power(Prefix.Factor, this.Exponent);
                    return value * factor;
                }
                else return value;
            }

            /// <summary>
            /// Return the axis without it's prefix. To be used in combination of ToBase 
            /// </summary>
            /// <returns></returns>
            public Axis UnPrefixed()
            {
                return new Axis(null, this.Unit, this.Exponent);
            }

            /// <summary>
            /// Returns true if the axis has no prefix
            /// </summary>
            public bool Prefixed
            {
                get
                {
                    return this.Prefix != null;
                }
            }

            /// <summary>
            /// Returns true if the axis has no meaning (exponent is zero)
            /// </summary>
            public bool IsVoid
            {
                get
                {
                    return Exponent == 0; 
                }
            }

            public override string ToString()
            {
                string prefix = (Prefix != null) ? Prefix.ToString() : "";
                string unit = (Unit != null) ? Unit.ToString() : "";
                string exp = null;
                switch (Exponent)
                {
                    case 0: return "1";
                    case 1: exp = ""; break;
                    default: exp = "^"+Exponent.ToString(); break;
                }
                return prefix + unit + exp;
            }

            public override bool Equals(object obj)
            {
                if (obj is Metric.Axis)
                {
                    Metric.Axis c = (Metric.Axis)obj;
                    return
                        (this.Prefix == c.Prefix)
                        &&
                        (this.Unit == c.Unit);
                }
                else return false;
            }

            public override int GetHashCode()
            {
                int i = this.ToString().GetHashCode();
                return i;
            }

            public static Axis CopyOf(Axis axis)
            {
                return new Axis(axis.Prefix, axis.Unit, axis.Exponent);
            }

            public int CompareTo(Axis other)
            {
                return this.ToString().CompareTo(other.ToString());
            }
        }

        public List<Axis> Axes = new List<Axis>();

        internal Metric()
        {

        }

        public Metric(Unit unit)
        {
            Add(null, unit);
        }

        public Metric(Prefix prefix, Unit unit, int exponent = 1)
        {
            Add(prefix, unit, exponent);
        }

        public Metric(List<Axis> axes)
        {
            this.Axes.AddRange(axes);
        }
        
        public void Add(Prefix prefix, Unit unit, int exponent = 1)
        {
            Axes.Add(new Axis(prefix, unit, exponent));
        }

        public void Add(Unit unit, int exponent = 1)
        {
            Axes.Add(new Axis(null, unit, exponent));
        }
        
        public void Add(params Axis[] axes)
        {
            this.Axes.AddRange(axes);
        }
        public void Add(IEnumerable<Axis> axes)
        {
            this.Axes.AddRange(axes);
        }

        public string Symbols
        {
            get
            {
                return string.Join(".", Axes.Select(c => c.Symbols));
            }
        }

        public string DimensionText
        {
            get
            {
                return string.Join(".", Axes.Select(a => string.Format("{0}^{1}", a.Unit.Dimension, a.Exponent)));
            }
        }

        public Exponential UnPrefix(Exponential value)
        {
            foreach (Axis axis in this.Axes)
            {
                value = axis.UnPrefix(value);
            }
            return value;
        }
        
        public Metric UnPrefixed()
        {
            Metric metric = new Metric();
            foreach(Axis axis in this.Axes)
            {
                metric.Add(null, axis.Unit, axis.Exponent);
            }
            return metric;
        }

        public bool IsInBaseUnits()
        {
            foreach(Axis axis in Axes)
            {
                if (!axis.Unit.IsBase) return false;
            }
            return true;
        }

        public static Metric CopyOf(Metric m)
        {
            return new Metric(m.Axes);
        }
        
        public Metric Merge(Axis axis)
        {
            Metric m = Metric.CopyOf(this);

            foreach (Axis a in this.Axes)
            {
                Axis b;
                if (a.Unit == axis.Unit)
                {
                    b = a.Merge(axis);
                }
                else
                {
                    b = Axis.CopyOf(axis);
                }
                m.Axes.Add(b);
            }
            return m;
        }
       
        public Metric Reduced()
        {
            Metric result = new Metric();

            foreach (Axis a in this.Axes)
            {
                Axis b = result.Axes.FirstOrDefault(x => x.Unit == a.Unit);
                if (b != null)
                {
                    result.Axes.Remove(b);
                    b = b.Merge(a);
                    result.Axes.Add(b);
                }
                else
                {
                    result.Axes.Add(Axis.CopyOf(a));
                }
            }
            result.clearVoids();
            result.sort();
            return result;
        }

        public Metric MultiplyExponents(int exponent)
        {
            // N^-1 => (kg.m.s-2)^-1
            var axes = new List<Axis>();
            foreach(Axis axis in Axes)
            {
                Axis a = new Axis(axis.Prefix, axis.Unit, axis.Exponent * exponent);
                axes.Add(a);
            }
            return new Metric(axes);
        }

        public override string ToString()
        {
            return Symbols ?? "(dimensionless)";
        }

        public override bool Equals(object obj)
        {
            if (obj is Metric)
            {
                Metric m = (Metric)obj;
                
                Metric a = this.UnPrefixed();
                Metric b = m.UnPrefixed();

                bool equala = a.EqualAxes(b);
                bool equalf = a.UnPrefix(1) == b.UnPrefix(1); 
                return equala && equalf;
            }
            else return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static Metric Multiply(Metric a, Metric b)
        {
            Metric metric = Metric.CopyOf(a);
            metric.Add(b.Axes);
            return metric.Reduced();
        }

        public static Metric Divide(Metric a, Metric b)
        {
            a = Metric.CopyOf(a);
            b = b.MultiplyExponents(-1);
            return Metric.Multiply(a, b);
        }

        public static Metric operator *(Metric a, Metric b)
        {
            return Metric.Multiply(a, b);
        }

        public static Metric operator /(Metric a, Metric b)
        {
            return Metric.Divide(a, b);
        }

        public static Quantity operator *(Exponential e, Metric m)
        {
            return new Quantity(e, m);
        }

        public static Quantity operator /(Quantity q, Metric m)
        {
            Metric metric = q.Metric / m;
            return new Quantity(q.Value, metric);

        }

        private void clearVoids()
        {
            this.Axes.RemoveAll(a => a.IsVoid);
        }

        private void sort()
        {
            this.Axes.Sort();
        }

        private bool EqualAxes(Metric other)
        {
            Metric a = this.Reduced();
            Metric b = other.Reduced();

            if (a.Axes.Count != b.Axes.Count)
                return false;

            bool equal = true;
            for (int i = 0; i <= a.Axes.Count - 1; i++)
            {
                equal &= a.Axes[i].Equals(b.Axes[i]);
                i++;
            }
            return equal;
        }
    }
}
