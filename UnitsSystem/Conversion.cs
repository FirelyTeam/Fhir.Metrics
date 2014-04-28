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
    public delegate decimal ConversionMethod(decimal value);
  
    public class Conversion
    {
        public Unit From;
        public Unit To;
        ConversionMethod method;

        public Conversion(Unit from, Unit to, ConversionMethod method)
        {
            this.From = from;
            this.To = to;
            this.method = method;
        }
        public decimal Convert(decimal value)
        {
            return method(value);
        }
    }

    
}
