using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fhir.UnitsSystem
{
    public class Constant
    {
        public string Symbols;
        public Exponential Value;
        public Constant(string symbols, Exponential value)
        {
            this.Symbols = symbols;
            this.Value = value;
        }
        public override string ToString()
        {
            return Symbols;
        }
    }
}
