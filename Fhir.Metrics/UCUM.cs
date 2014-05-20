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

namespace Fhir.Metrics
{
    public static class UCUM
    {
        public static Stream Stream
        {
            get
            {
                Stream s = typeof(UCUM).Assembly.GetManifestResourceStream("Fhir.Metrics.Data.ucum-essence.xml");
                return s;
            }
        }

        public static SystemOfUnits Load()
        {
             SystemOfUnits system;
             UcumReader reader = new UcumReader(UCUM.Stream);
             system = reader.Read();
             return system;
        }
    }
}
