using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fhir.Metrics.Tests
{
    
    public class TestValidation
    {
        SystemOfUnits system;

        public TestValidation()
        {
            system = UCUM.Load();
        }

        [Fact]
        public void TestMmol()
        {
            Quantity input = system.Quantity("4.1234mmol");
            Quantity output = system.Canonical(input);
            
            string s = output.LeftSearchableString();
            Assert.Equal("E21x2.5832", s);
        }
    }

}
