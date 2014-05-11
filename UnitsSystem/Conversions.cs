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
            if (!baseq.Metric.Equals(conversion.To))
                throw new InvalidCastException(string.Format("Quantity {0} cannot be converted to {1}", quantity, conversion.To));

            Quantity output = conversion.Convert(baseq.Value);
            return output;
        }

        public List<Conversion> Path(Metric from, Func<Conversion, bool> predicate)
        {
            Metric m = from;

            List<Conversion> steps = new List<Conversion>();
            Conversion step;
            bool found = false;
            do
            {
                step = Find(m);
                if (step != null) steps.Add(step);
                m = step.To;
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

        public Quantity ToBaseUnits(Quantity quantity)
        {
            Quantity output = Quantity.CopyOf(quantity);

            do
            {
                List<Metric.Axis> axes = new List<Metric.Axis>();
                Exponential factor = 1;

                foreach (Metric.Axis axis in output.Metric.Axes)
                {
                    Conversion conversion = Find(axis.Unit);
                    if (conversion != null)
                    {
                        Quantity q = conversion.Convert(Exponential.One);
                        q = q.ToBase();
                        axes.AddRange(q.Metric.Axes);
                        factor *= q.Value;
                    }
                    else
                    {
                        axes.Add(Metric.Axis.CopyOf(axis));
                    }
                }
                Metric metric = new Metric(axes);
                output = new Quantity(output.Value * factor, metric);
            } while (!output.IsInBaseUnits());

            return output;
        }

        public bool Path(Metric from, Metric to, out List<Conversion> list)
        {
            throw new NotImplementedException();
            //list = Path(from, c => c.To == to);
            //return (list != null);
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

        public bool PathToSystem(Metric from, string systemname, out List<Conversion> list)
        {
            throw new NotImplementedException();
            //list = Path(from, c => c.To.Classification == systemname);
            //return (list != null);
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
        
        public Quantity Convert(Quantity quantity, Metric metric)
        {
            List<Conversion> steps;
            Quantity q = quantity.ToBase();
            if (Path(q.Metric, metric, out steps))
            {
                return ConvertViaPath(q, steps);
            }
            else throw new InvalidCastException(string.Format("Quantity {0} cannot be converted to {1}", quantity, metric));
        }

        public Quantity ConvertToSystem(Quantity quantity, string systemname)
        {
            List<Conversion> steps;
            Quantity q = quantity.ToBase();
            if (PathToSystem(q.Metric, systemname, out steps))
            {
                return ConvertViaPath(q, steps);
            }
            else throw new InvalidCastException(string.Format("Quantity {0} cannot be converted to {1}", quantity, systemname));
        }

        public Conversion Find(Metric from)
        {
            return conversions.FirstOrDefault(c => c.From.Equals(from));
        }

        public Conversion Find(Unit u)
        {
            Metric m = new Metric(u);
            return Find(m);
        }
       
        public Conversion Add(Metric from, Metric to, ConversionMethod method)
        {
            Conversion conversion = new Conversion(from, to, method);
            conversions.Add(conversion);
            return conversion;
        }

    }
}
