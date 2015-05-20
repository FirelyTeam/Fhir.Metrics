using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fhir.Metrics.Tests
{
    [TestClass]
    public class TestValidation
    {
        static SystemOfUnits system;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            system = UCUM.Load();
        }

        [TestMethod]
        public void ValidateSimple()
        {
            Quantity a;
            a = system.Quantity("4 kg");
            a = system.Quantity("4 aapjes");
        }

        [TestMethod]
        public void TestMmol()
        {
            Quantity input = system.Quantity("4.1234mmol");
            Quantity output = system.Canonical(input);
            
            string s = output.LeftSearchableString();
            Assert.AreEqual("abcd", s);
        }
    }

}
