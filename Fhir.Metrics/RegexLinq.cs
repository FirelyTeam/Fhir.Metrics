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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fhir.Metrics
{
    /// <summary>
    /// Class for making RegEx components accessible in Linq statements.
    /// </summary>
    public static class RegexLinq
    {
        /// <summary>
        /// Enumerates all groups of the RegEx match
        /// </summary>
        public static IEnumerable<Group> Groups(this Match match)
        {
            for (int i = 0; i <= match.Groups.Count; i++) 
            {
                yield return match.Groups[i];
            }
        }
        
        /// <summary>
        /// Enumerates all capture values in a RegEx CaptureCollection
        /// </summary>
        public static IEnumerable<string> Values(this CaptureCollection captures)
        {
            for (int i = 0; i <= captures.Count-1; i++)
            {
                yield return captures[i].Value;
            }
        }

        /// <summary>
        /// Enumerates all capture values in a RegEx group
        /// </summary>
        public static IEnumerable<string> Captures(this Group group)
        {
            return group.Captures.Values();
        }

        /// <summary>
        /// Enumerates all named capture values in a RegEx match 
        /// </summary>
        /// <param name="match">The RegEx Match instance</param>
        /// <param name="name">The name of the captures to enumerate</param>
        public static IEnumerable<string> Captures(this Match match, string name)
        {
            return match.Groups[name].Captures();
        }

        /// <summary>
        /// Enumerates all groups that were succesful in a RegEx match.
        /// </summary>
        public static IEnumerable<Group> Successes(this Match match)
        {
            return match.Groups().Where(g => g.Success);
        }

        /// <summary>
        /// Enumerates all succesful capture values of a RegEx match excluding the capture of the string as a whole 
        /// </summary>
        public static IEnumerable<string> Captures(this Match match)
        {
            return match.Successes().Skip(1).Select(g => g.Value);
        }
    }
}
