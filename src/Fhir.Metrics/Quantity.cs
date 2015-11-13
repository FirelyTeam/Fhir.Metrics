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

        public Quantity UnPrefixed()
        {
            Exponential value = this.Metric.UnPrefix(this.Value);
            Metric metric = this.Metric.UnPrefixed();
            return new Quantity(value, metric);
        }

        public bool Approximates(Quantity q)
        {
            Quantity a = this.UnPrefixed();
            Quantity b = q.UnPrefixed();
            
            bool met = a.Metric.Equals(b.Metric);
            bool val = a.Value.Approximates(b.Value);
            return met && val;
        }

        public bool IsDimless
        {
            get
            {
                return Metric.Axes.Count == 0;
            }
        }
        
        public static bool SameDimension(Quantity a, Quantity b)
        {
            return a.Metric.Equals(b.Metric);
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

        public static Quantity Multiply(Quantity a, Quantity b)
        {
            Exponential value = Exponential.Multiply(a.Value, b.Value);
            Metric metric = Metric.Multiply(a.Metric, b.Metric);
            return new Quantity(value, metric);
        }

        public static Quantity Divide(Quantity a, Quantity b)
        {
            Exponential value = Exponential.Divide(a.Value, b.Value);
            Metric metric = Metric.Divide(a.Metric, b.Metric);
            return new Quantity(value, metric);
        }

        public static Quantity Add(Quantity a, Quantity b)
        {

            if (!Quantity.SameDimension(a, b))
                throw new ArgumentException("Quantities of a different dimension cannot be added ");

            a = a.UnPrefixed();
            b = b.UnPrefixed();

            Exponential value = Exponential.Add(a.Value, b.Value);
            return new Quantity(value, a.Metric);
        }

        public static Quantity Substract(Quantity a, Quantity b)
        {

            if (!Quantity.SameDimension(a, b))
                throw new ArgumentException("Quantities of a different dimension cannot be added ");

            a = a.UnPrefixed();
            b = b.UnPrefixed();

            Exponential value = Exponential.Substract(a.Value, b.Value);
            return new Quantity(value, a.Metric);
        }

        public static Quantity operator *(Quantity a, Quantity b)
        {
            return Quantity.Multiply(a, b);
        }

        public static Quantity operator /(Quantity a, Quantity b)
        {
            return Quantity.Divide(a, b);
        }

        public static Quantity operator +(Quantity a, Quantity b)
        {
            return Quantity.Add(a, b);
        }

        public static Quantity operator -(Quantity a, Quantity b)
        {
            return Quantity.Substract(a, b);
        }

    }
}
