using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fhir.UnitsSystem;

namespace TestUnitsSystem
{
    [TestClass]
    public class TestConversions
    {
        [TestMethod]
        public void SingleConversion()
        {
            UnitsSystem system = new UnitsSystem();

            Unit inch = system.Units.Add("Length", "inch", "[in_i]");
            Unit meter = system.Units.Add("Length", "meter", "m");
            Prefix kilo = system.Units.Add("Kilo", "k", 1000);
            system.Conversions.Add(inch, meter, d => d * 2);

            Quantity q = new Quantity(4, kilo, inch);
            Quantity r = system.Conversions.Convert(q, meter);

            Assert.AreEqual(r.Unit.Symbol, "m");
            Assert.AreEqual(r.Value, 8);
        }

        [TestMethod]
        public void StringBasedConversion()
        {

            

            UnitsSystem system = new UnitsSystem();
            system.Units.Add("Weight", "gramme", "g");
            system.Units.Add("Weight", "pound", "pnd");
            system.Units.Add("Kilo", "k", 10e3M);
            system.Units.Add("Hecto", "h", 10e2M);
            system.Add("pnd", "g", p => p * 5*10e2M);
            string s = system.Convert("4pnd", "kg");
            
            // todo: precision issues have to be solved.
            Assert.AreEqual(s, "2kg");
            
            
        }
    }
}
