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
    public delegate Exponential ConversionMethod(Exponential value);
  
    public class Conversion
    {
        public Metric From;
        public Metric To;
        ConversionMethod method;

        public Conversion(Metric from, Metric to, ConversionMethod method)
        {
            this.From = from;
            this.To = to;
            this.method = method;
        }
        public Quantity Convert(Quantity quantity)
        {
            Quantity q = new Quantity();
            q.Metric = this.To;
            q.Value = this.method(quantity.Value);
            return q;
        }
    }

}
