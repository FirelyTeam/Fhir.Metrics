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
    public class Unit
    {
        public string Dimension; // only for base units!
        public string Name;
        public string Symbol;

        public Unit(string name, string symbol, string dimension)
        {
            this.Dimension = dimension;
            this.Name = name;
            this.Symbol = symbol;
        }

        public bool IsBase
        {
            get
            {
                return Dimension != null;
            }
        }

        public override string ToString()
        {
            return Symbol;
        }
    }
}
