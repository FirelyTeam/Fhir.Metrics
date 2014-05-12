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

            public Exponential ToBase(Exponential value)
            {
                if (Prefix != null) 
                {
                    Exponential factor = Exponential.Power(Prefix.Factor, this.Exponent);
                    return value * factor;
                }
                else return value;
            }

            public Axis Base()
            {
                return new Axis(null, this.Unit, this.Exponent);
            }
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

        public Exponential ToBase(Exponential value)
        {
            foreach (Axis axis in this.Axes)
            {
                value = axis.ToBase(value);
            }
            return value;
        }
        
        public Metric Base()
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
            List<Axis> axes = this.Axes.ToList();
            Axis target = axes.Find(c => c.Unit == axis.Unit);
            if (target != null)
                target.Merge(axis);
            else
                this.Axes.Add(axis);
            return new Metric(axes);
        }

        private void clearVoids()
        {
            this.Axes.RemoveAll(a => a.IsVoid);
        }

        public Metric Reduced()
        {
            Metric result = new Metric();

            foreach (Axis axis in Axes)
            {
                result.Merge(axis);
            }
            result.clearVoids();
            return result;
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
                
                Metric a = this.Base();
                Metric b = m.Base();

                bool equala = a.EqualAxes(b);
                bool equalf = a.ToBase(1) == b.ToBase(1); 
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
