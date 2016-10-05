using System;
using Xunit;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

namespace Fhir.Metrics.Tests
{
    
    public class TestParser
    {
        [Fact]
        public void Parse()
        {
            string expression = "kg.m/s2";
            string tokenpattern = @"^(((?<m>[\.\/])?(?<m>[^\.\/]+))*)?$";
            Match m = Regex.Match(expression, tokenpattern, RegexOptions.ExplicitCapture);
            List<string> list = m.Captures("m").ToList();
            Assert.Equal(5, list.Count);
            Assert.Equal("kg", list[0]);
            Assert.Equal("s2", list[4]);
        }
    }
}
