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
            this.Metric = new Metric(null, unit);
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

        public Prefix Prefix
        {
            get
            {
                return this.Metric.Prefix;
            }
        }
        public Unit Unit
        {
            get
            {
                return this.Metric.Unit;
            }
            set
            {
                this.Metric.Unit = value;
            }
        }
        public string Symbols
        {
            get
            {
                return this.Metric.Symbols;
            }
        }
        public override string ToString()
        {
            return string.Format("{0}{1}", Value, Metric);
        }
    }
}
