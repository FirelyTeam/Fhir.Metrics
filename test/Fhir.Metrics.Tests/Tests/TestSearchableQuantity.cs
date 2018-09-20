using System;
using Xunit;
using Fhir.Metrics;

namespace Fhir.Metrics.Tests
{
    
    public class QuantityExtensionsTest
    {
        SystemOfUnits system;

        
        public QuantityExtensionsTest()
        {
            system = UCUM.Load();
        }

        [Fact]
        public void SignificandText()
        {
            Quantity quantity = system.Quantity("4000g");
            string s = quantity.Value.SignificandText;
            Assert.Equal("4.000", s);

            quantity = system.Quantity("4kg");
            s = quantity.Value.SignificandText;
            Assert.Equal("4", s);
        }

        [Fact]
        public void LeftSearchableString()
        {
            Quantity q = system.Quantity("41.234567kg.m/s2");
            string s = q.LeftSearchableString();
            Assert.Equal("g.m.s-2E4x4.1235677", s);

            Quantity mass = system.Quantity("4.0kg");
            Quantity acceleration = system.Quantity("2.0m/s2");
            Quantity force = mass * acceleration;
            s = force.LeftSearchableString();
            Assert.Equal("g.m.s-2E3x8", s);


            q = system.Quantity("4000g");
            s = q.LeftSearchableString();
            Assert.Equal("gE3x4.000", s);
        }

        [Fact]
        public void LeftSearchableStringWithSlashAsCode()
        {
            Quantity q = system.Quantity(3M, "/a");// this creates a quantity with a value of 9.5 and an error of 1.5, which leads to an ArrayOutOfBoundsException when calculating SignificandText
            Quantity qc = system.Canonical(q);
            string s = qc.LeftSearchableString();
            Assert.Equal("s-1E-8x9.516436344219736296402018867", s);
        }

    }
}
