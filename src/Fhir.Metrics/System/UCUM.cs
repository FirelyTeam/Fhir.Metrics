/*
* Copyright (c) 2014, Furore (info@furore.com) and contributors
* See the file CONTRIBUTORS for details.
*
* This file is licensed under the BSD 3-Clause license
*/

using System;
using System.IO;
using System.Reflection;

namespace Fhir.Metrics
{
    /// <summary>
    /// Static class to give access to UCUM properties 
    /// </summary>
    public static class UCUM
    {
        /// <summary>
        /// THe identifying URI for the UCUM system
        /// </summary>
        public static Uri Uri = new Uri("http://unitsofmeasure.org");

        private static Stream Stream
        {
            get
            {
                var assembly = typeof(UCUM).GetTypeInfo().Assembly;
                Stream s = assembly.GetManifestResourceStream("Fhir.Metrics.Data.ucum-essence.xml");
                return s;
            }
        }

        /// <summary>
        /// Loads the internal resource with UCUM data into a <b>SystemOfUnits</b> class
        /// </summary>
        public static SystemOfUnits Load()
        {
             SystemOfUnits system;
             UcumReader reader = new UcumReader(UCUM.Stream);
             system = reader.Read();
             return system;
        }
    }
}
