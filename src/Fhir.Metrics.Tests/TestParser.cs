using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

namespace Fhir.Metrics.Tests
{
    [TestClass]
    public class TestParser
    {
        [TestMethod]
        public void Parse()
        {
            string expression = "kg.m/s2";
            string tokenpattern = @"^(((?<m>[\.\/])?(?<m>[^\.\/]+))*)?$";
            Match m = Regex.Match(expression, tokenpattern, RegexOptions.ExplicitCapture);
            List<string> list = m.Captures("m").ToList();
            Assert.AreEqual(5, list.Count);
            Assert.AreEqual("kg", list[0]);
            Assert.AreEqual("s2", list[4]);
        }
    }
}
