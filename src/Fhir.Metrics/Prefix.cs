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
    public class Prefix
    {
        public string Name;
        public string Symbol;
        public Exponential Factor; // 10 based exponent

        public Prefix() { }

        public Prefix(string name, string symbol, Exponential factor)
        {
            this.Name = name;
            this.Symbol = symbol;
            this.Factor = factor;
        }

        public Exponential ConvertToBase(Exponential value)
        {
            return value * this.Factor;  // 1kg -> 1000 g
        }

        public Exponential ConvertFromBase(Exponential value)
        {
            return value / this.Factor;  // 1000g -> 1kg
        }

        public override string ToString()
        {
            return Symbol;
        }
    }
}
