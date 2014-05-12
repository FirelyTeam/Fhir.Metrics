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
    public class Quantity
    {
        public Exponential Value;
        public Metric Metric;

        public Quantity() { }

        public Quantity(Exponential value, Unit unit)
        {
            this.Value = value;
            this.Metric = new Metric(unit);
        }

        public Quantity(Exponential value, Prefix prefix, Unit unit)
        {
            this.Value = value;
            this.Metric = new Metric(prefix, unit);
        }

        public Quantity(Exponential value, Metric metric)
        {
            this.Value = value;
            this.Metric = metric;
        }

        public string Symbols
        {
            get
            {
                return this.Metric.Symbols;
            }
        }

        public Quantity ToBase()
        {
            Exponential value = this.Metric.ToBase(this.Value);
            Metric metric = this.Metric.Base();
            return new Quantity(value, metric);
        }

        public bool Approximates(Quantity q)
        {
            Quantity a = this.ToBase();
            Quantity b = q.ToBase();
            
            bool met = a.Metric.Equals(b.Metric);
            bool val = a.Value.Approximates(b.Value);
            return met && val;
        }

        public static Quantity CopyOf(Quantity q)
        {
            return new Quantity(q.Value, q.Metric);
        }
      
        public override string ToString()
        {
            return string.Format("{0}{1}", Value, Metric);
        }

        public bool IsInBaseUnits()
        {
            return this.Metric.IsInBaseUnits();
        }

    }
}
