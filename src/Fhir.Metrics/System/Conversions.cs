/*
* Copyright (c) 2014, Furore (info@furore.com) and contributors
* See the file CONTRIBUTORS for details.
*
* This file is licensed under the BSD 3-Clause license
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Fhir.Metrics
{
    public class Conversions 
    {
        private List<Conversion> conversions = new List<Conversion>();

        public Quantity Convert(Quantity quantity, Conversion conversion)
        {
            Quantity baseq = quantity.UnPrefixed();
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
                if (step != null)
                {
                    steps.Add(step);
                    m = step.To;
                    found = predicate(step);
                }
            }
            while (step != null && !found);
            if (found)
            {
                return steps;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Tries to convert each axis of the metric of the quantity once.
        /// </summary>
        /// <returns>true if a change has been made</returns>
        private bool canonicalize(ref Quantity quantity)
        {
            List<Metric.Axis> axes = new List<Metric.Axis>();
            Exponential value = quantity.Value;
            bool modified = false;

            foreach (Metric.Axis axis in quantity.Metric.Axes)
            {
                Conversion conversion = Find(axis.Unit);
                if (conversion != null)
                {
                    Quantity part = conversion.Convert(1);
                    part.Metric = part.Metric.MultiplyExponents(axis.Exponent);
                    part.Value = Exponential.Power(part.Value, axis.Exponent);
                    axes.AddRange(part.Metric.Axes);
                    value *= part.Value;
                    modified = true;
                }
                else
                {
                    axes.Add(Metric.Axis.CopyOf(axis));
                }
            }

            if (modified)
                quantity = new Quantity(value, new Metric(axes));

            return modified;
        }

        /// <summary>
        /// Converts a Quantity to it's canonical form in base units without prefixes
        /// </summary>
        public Quantity Canonical(Quantity quantity)
        {
            Quantity output = Quantity.CopyOf(quantity).UnPrefixed();
            bool canonicalized;
            do
            {
                canonicalized = canonicalize(ref output);
                output = output.UnPrefixed();
            }
            while (canonicalized);

            if (!output.IsInBaseUnits())
                throw new InvalidCastException("Quantity could not be converted to base units");

            output.Metric = output.Metric.Reduced();
            return output;
        }
              
        public Quantity Convert(Quantity quantity, Metric metric)
        {
            throw new NotImplementedException();
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
