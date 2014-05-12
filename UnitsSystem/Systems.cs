/*
* Copyright (c) 2014, Furore (info@furore.com) and contributors
* See the file CONTRIBUTORS for details.
*
* This file is licensed under the BSD 3-Clause license
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fhir.UnitsSystem
{
    public static class Systems
    {
        private static SystemOfUnits getMetric()
        {
            SystemOfUnits system = new SystemOfUnits();

            system.AddPrefix("Micro", "µ", 1e-6m);
            system.AddPrefix("Kilo", "k", 1e3m);
            system.AddPrefix("Test", "t", -6);

            system.AddUnit("Length", "meter", "m");
            system.AddUnit("Length", "inch", "[in_i]");
            system.AddUnit("Surface", "meter", "m2");
            system.AddUnit("Volume", "Meter", "m3");
            system.AddUnit("Mass", "Gramme", "g");
            system.AddUnit("Temperature", "Kelvin", "K");
            system.AddUnit("Temperature", "Celsius", "C");
            
            system.AddConversion("g", "T", g => g * 10e9M);
            system.AddConversion("[in_i]", "m", d => d * Exponential.Exact(0.0254m));
            system.AddConversion("K", "C", x => x + 273.15m);

            system.AddConversion("Temperature", "°F", x => ((x - 32) / 1.8m) + 273.15m);
            system.AddConversion("Volume", "l", x => x);
            system.AddConversion("pnd", "g", p => p * 5 * 10e2M);

            return system;
        }

        private static SystemOfUnits metric = null;

        public static SystemOfUnits Metric
        {
            get
            {
                if (metric == null)
                    metric = getMetric();
                return metric;
            }
        }

        public static Stream UcumStream()
        {
            Stream s = typeof(Systems).Assembly.GetManifestResourceStream("Fhir.UnitsSystem.Data.ucum-essence.xml");
            return s;
        }

        public static SystemOfUnits LoadUcum()
        {

            UcumReader reader = new UcumReader("http://unitsofmeasure.org/ucum-essence.xml");
            SystemOfUnits system = reader.Read();
            return system;
        }

    }
}
