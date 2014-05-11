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

    public class Metric
    {
        public class Axis
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
            public Exponential Factor
            {
                get
                {
                    return (Prefix != null) ? Prefix.Factor : 1;
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
                return base.GetHashCode();
            }
            public static Axis CopyOf(Axis axis)
            {
                return new Axis(axis.Prefix, axis.Unit, axis.Exponent);
            }
        }

        private Metric()
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
            this.Add(axes);
        }
        
        public List<Axis> Axes = new List<Axis>();

        public void Add(Prefix prefix, Unit unit, int exponent = 1)
        {
            Axes.Add(new Axis(prefix, unit, exponent));
        }
        public void Add(Unit unit, int exponent = 1)
        {
            Axes.Add(new Axis(null, unit, exponent));
        }
        public void Add(List<Axis> axes)
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

        public Exponential CalcFactor()
        {
            Exponential factor = Exponential.Exact(1);
            foreach (Axis c in this.Axes)
            {
                factor *= c.Factor;
            }
            return factor;
        }
        
        public Metric ToBase()
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
        private void reduce()
        {
            List<Axis> reduced = new List<Axis>();
 
            foreach(Axis component in Axes)
            {
                Axis t = reduced.Find(c => c.Unit == component.Unit);
                if (t != null)
                    t.Merge(component);
                else
                    reduced.Add(t);
            }
            reduced.RemoveAll(c => c.Factor == 0);
            this.Axes = reduced;
        }

        public Metric Reduced()
        {
            Metric m = new Metric(this.Axes);
            m.reduce();
            return m;
        }

        private bool EqualAxes(Metric m)
        {
            if (this.Axes.Count != m.Axes.Count)
                return false;

            
            bool equal = true;
            for (int i = 0; i <= this.Axes.Count; i++ )
            {
                equal &= this.Axes[i].Equals(m.Axes[i]);
                i++;
            }
            return equal;
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
                
                Metric a = this.ToBase();
                Metric b = m.ToBase();

                bool equala = a.EqualAxes(b);
                bool equalf = a.CalcFactor() == b.CalcFactor();

                return equala && equalf;
            }
            else return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

}
