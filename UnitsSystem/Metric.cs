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
        public class Component
        {
            public Unit Unit;
            public Prefix Prefix;
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
            public Component(Prefix prefix, Unit unit, int exponent = 1)
            {
                this.Prefix = prefix;
                this.Unit = unit;
                this.Exponent = exponent;
            }
            public Component Merge(Component a)
            {
                if (this.Unit != a.Unit || this.Prefix != a.Prefix)
                    throw new ArgumentException("Cannot merge metric components with a different unit or prefix");

                int exponent = this.Exponent + a.Exponent;
                Component target = new Component(this.Prefix, this.Unit, exponent);
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
                if (obj is Metric.Component)
                {
                    Metric.Component c = (Metric.Component)obj;
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
        public Metric(List<Component> components)
        {
            this.Add(components);
        }
        public List<Component> Components = new List<Component>();

        public void Add(Prefix prefix, Unit unit, int exponent = 1)
        {
            Components.Add(new Component(prefix, unit, exponent));
        }

        public void Add(Unit unit, int exponent = 1)
        {
            Components.Add(new Component(null, unit, exponent));
        }
        public void Add(List<Component> components)
        {
            this.Components.AddRange(components);
        }

        public string Symbols
        {
            get
            {
                return string.Join(".", Components.Select(c => c.Symbols));
            }
        }

        public Exponential CalcFactor()
        {
            Exponential factor = Exponential.Exact(1);
            foreach (Component c in this.Components)
            {
                factor *= c.Factor;
            }
            return factor;
        }
        public Metric ToBase()
        {
            Metric m = new Metric();
            foreach(Component c in this.Components)
            {
                m.Add(null, c.Unit, c.Exponent);
            }
            return m;
        }

        public Metric Reduced()
        {
            List<Component> target = new List<Component>();
 
            foreach(Component component in Components)
            {
                Component t = target.Find(c => c.Unit == component.Unit);
                if (t != null)
                    t.Merge(component);
            }
            target.RemoveAll(c => c.Factor == 0);
            return new Metric(target);

        }

        private bool componentsEqual(Metric m)
        {
            if (this.Components.Count != m.Components.Count)
                return false;

            int i = 0;
            bool equal = true;
            while (this.Components[i] != null)
            {
                equal &= this.Components[i].Equals(m.Components[i]);
            }
            return equal;
        }

    
        public override string ToString()
        {
            return Symbols;
        }

        public override bool Equals(object obj)
        {
            if (obj is Metric)
            {
                Metric m = (Metric)obj;
                
                Metric a = this.ToBase();
                Metric b = m.ToBase();

                bool equal = a.componentsEqual(b);
                return equal;
            }
            else return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

}
