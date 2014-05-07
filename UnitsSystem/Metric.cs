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

        public override bool Equals(object obj)
        {
            if (obj is Metric)
            {
                Metric m = (Metric)obj;
                return
                    (this.Prefix == m.Prefix)
                    &&
                    (this.Unit == m.Unit);
            }
            else return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

}
