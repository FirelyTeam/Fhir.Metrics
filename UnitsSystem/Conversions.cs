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
    public class Conversions 
    {
        private List<Conversion> conversions = new List<Conversion>();

        public Quantity Convert(Quantity quantity, Conversion conversion)
        {
            Quantity baseq = quantity.ToBase();
            Quantity output = new Quantity();
            output.Value = conversion.Convert(quantity.Value);
            output.Metric = new Metric(null, conversion.To);
            return output;
        }

        public List<Conversion> Path(Unit from, Func<Conversion, bool> predicate)
        {
            Unit u = from;

            List<Conversion> steps = new List<Conversion>();
            Conversion step;
            bool found = false;
            do
            {
                step = Find(u);
                if (step != null) steps.Add(step);
                u = step.To;
                found = predicate(step);
            }
            while (steps != null && !found);
            if (found)
            {
                return steps;
            }
            else
            {
                return null;
            }
        }

        public bool Path(Unit from, Unit to, out List<Conversion> list)
        {

            list = Path(from, c => c.To == to);
            return (list != null);
            /*
            Unit u = from;

            List<Conversion> steps = new List<Conversion>();
            Conversion step;
            do
            {
                step = Find(u);
                if (step != null) steps.Add(step);
                u = step.To;
            }
            while (steps != null && u != to);
            if (u == to)
            {
                list = steps;
                return true;
            }
            else
            {
                list = null;
                return false;
            }
            */
        }

        public bool PathToSystem(Unit from, string systemname, out List<Conversion> list)
        {
            list = Path(from, c => c.To.Classification == systemname);
            return (list != null);
        }
        
        public Quantity ConvertViaPath(Quantity quantity, IEnumerable<Conversion> steps)
        {
            Quantity q = quantity;
            foreach (Conversion conversion in steps)
            {
                q = Convert(q, conversion);
            }
            return q;
        }
        
        public Quantity Convert(Quantity quantity, Unit unit)
        {
            List<Conversion> steps;
            Quantity q = quantity.ToBase();
            if (Path(q.Unit, unit, out steps))
            {
                return ConvertViaPath(q, steps);
            }
            else throw new InvalidCastException(string.Format("Quantity {0} cannot be converted to {1}", quantity, unit));
        }
               
        public Quantity Convert(Quantity quantity, Metric metric)
        {
            Quantity q = Convert(quantity, metric.Unit);
            q = q.ConvertToPrefix(metric.Prefix);
            return q;
        }

        public Quantity ConvertToSystem(Quantity quantity, string systemname)
        {
            List<Conversion> steps;
            Quantity q = quantity.ToBase();
            if (PathToSystem(q.Unit, systemname, out steps))
            {
                return ConvertViaPath(q, steps);
            }
            else throw new InvalidCastException(string.Format("Quantity {0} cannot be converted to {1}", quantity, systemname));
        }

        public Conversion Find(Unit from)
        {
            return conversions.FirstOrDefault(c => c.From == from);
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

        public Conversion Add(Unit from, Unit to, ConversionMethod method)
        {
            Conversion conversion = new Conversion(from, to, method);
            conversions.Add(conversion);
            return conversion;
        }
    }
}
