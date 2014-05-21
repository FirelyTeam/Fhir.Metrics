using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fhir.Metrics
{
    public static class RegexLinq
    {

        public static IEnumerable<Group> Groups(this Match match)
        {
            for (int i = 0; i <= match.Groups.Count; i++) 
            {
                yield return match.Groups[i];
            }
        }
        
        public static IEnumerable<string> Values(this CaptureCollection captures)
        {
            for (int i = 0; i <= captures.Count-1; i++)
            {
                yield return captures[i].Value;
            }
        }

        public static IEnumerable<string> Captures(this Group group)
        {
            return group.Captures.Values();
        }

        public static IEnumerable<string> Captures(this Match match, string name)
        {
            return match.Groups[name].Captures();
        }

        public static IEnumerable<Group> Successes(this Match match)
        {
            return match.Groups().Where(g => g.Success);
        }

        public static IEnumerable<string> Captures(this Match match)
        {
            return match.Successes().Skip(1).Select(g => g.Value);
        }
    }
}
