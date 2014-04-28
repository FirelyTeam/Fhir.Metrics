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
    public static class Systems
    {
        private static UnitsSystem getMetric()
        {
            UnitsSystem system = new UnitsSystem();

            system.Units.Add("Micro", "µ", 10e-6M);
            

            system.Units.Add("Length", "Meter", "m");
            system.Units.Add("Surface", "meter", "m2");
            system.Units.Add("Volume", "Meter", "m3");
            system.Units.Add("Mass", "Gramme", "g");
            system.Units.Add("Temperature", "Kelvin", "K");

            system.Add("g", "T", g => g * 10e9M);
          
            /*
            system.Add("Temperature", "kelvin", x => x);
            system.Add("Temperature", "°K", x => x);
            system.Add("Temperature", "°C", x => x + 273.15);
            system.Add("Temperature", "°F", x => ((x - 32) / 1.8) + 273.15);
            system.Add("Volume", "l", x => x);
            */
            return system;
        }

        private static UnitsSystem metric = null;

        public static UnitsSystem Metric
        {
            get
            {
                if (metric == null)
                    metric = getMetric();
                return metric;
            }
        }



    }
}
