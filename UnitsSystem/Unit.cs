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
    public class Unit
    {
        public string Classification;
        public string Dimension;
        public string Name;
        public string Symbol;

        public Unit(string classification, string name, string symbol)
        {
            this.Classification = classification;
            this.Name = name;
            this.Symbol = symbol;
        }

        public override string ToString()
        {
            return Symbol;
        }
    }
}
