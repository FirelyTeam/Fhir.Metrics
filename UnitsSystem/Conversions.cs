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
    public class Conversions // UCMS
    {
        // todo: limit conversion to the system of from-quantity
        //public Units Units = new Units();
       
        private List<Conversion> conversions = new List<Conversion>();

        public Quantity ConvertToBaseUnit(Quantity quantity)
        {
            return ConvertToPrefix(quantity, null);
        }
        public Quantity ConvertToPrefix(Quantity quantity, Prefix prefix = null)
        {
            Quantity output = new Quantity();
            int fromfactor = (quantity.Prefix == null) ? 1 : quantity.Prefix.Dex;
            int tofactor = (prefix == null) ? 1 : prefix.Dex;
            int factor = fromfactor / tofactor;
            output.Value = quantity.Value * factor;
            output.Metric = new Metric(prefix, quantity.Unit);
            return output;
        }
        public Quantity Convert(Quantity quantity, Conversion conversion)
        {
            Quantity baseq = ConvertToBaseUnit(quantity);
            Quantity output = new Quantity();
            output.Value = conversion.Convert(quantity.Value);
            output.Metric = new Metric(null, conversion.To);
            return output;
        }
        public Quantity Convert(Quantity quantity, Unit unit)
        {
            Unit from = quantity.Unit;
            Conversion conversion = Find(from, unit);
            return Convert(quantity, conversion);
        }
        public Quantity Convert(Quantity quantity, Metric metric)
        {
            Quantity q = ConvertToBaseUnit(quantity);
            Conversion conversion = Find(quantity.Unit, metric.Unit);
            
            Quantity output = new Quantity();
            output.Value = conversion.Convert(q.Value);
            output.Metric = new Metric(null, metric.Unit);
            output = ConvertToPrefix(output, metric.Prefix);
            return output;
        }
        public Conversion Find(Unit from, Unit to)
        {
            foreach (Conversion conversion in conversions)
            {
                if ((conversion.From == from) && (conversion.To == to))
                    return conversion;
            }
            return null;
        }

        public void Add(Unit from, Unit to, ConversionMethod method)
        {
            conversions.Add(new Conversion(from, to, method));
        }
    }
}
