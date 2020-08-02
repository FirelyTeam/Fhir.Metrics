using System;
using Xunit;

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
            Quantity q, qc;

            q = system.Quantity(3, "/a"); 
            qc = system.Canonical(q);
            string s = qc.LeftSearchableString();
            Assert.Equal("s-1E-8x9.516436344219736296402018867", s);

            // This creates a quantity with a value of 9.5 and an error of 1.5
            // It was leading to an ArrayOutOfBoundsException when calculating SignificandText
            // caused by the fact that the error was of the same order as the first digit.
            q = system.Quantity(3M, "/a");
            qc = system.Canonical(q);
            s = qc.LeftSearchableString();
            Assert.Equal("s-1E-8x9", s);
        }

        [Fact]
        public void TestLeftSearchable()
        {
            // Test decimal with no fraction
            var q = system.Quantity("100kg");
            var s = q.LeftSearchableString();
            Assert.Equal("gE5x1.00", s);

            // Test quantity with different iterations of 1's and 0's
            q = system.Quantity("100.10kg");
            s = q.LeftSearchableString();
            Assert.Equal("gE5x1.0010", s);

            q = system.Quantity("100.101kg");
            s = q.LeftSearchableString();
            Assert.Equal("gE5x1.00101", s);

            q = system.Quantity("100.1010kg");
            s = q.LeftSearchableString();
            Assert.Equal("gE5x1.001010", s);

            q = system.Quantity("100.101010kg");
            s = q.LeftSearchableString();
            Assert.Equal("gE5x1.00101010", s);

            // The reminder should be copied over to the left for any digit > 5
            q = system.Quantity("100.1234567890kg");
            s = q.LeftSearchableString();
            Assert.Equal("gE5x1.001235678990", s);

            // The reminder should get copied over to the left for all 9's
            q = system.Quantity("1.999999kg");
            s = q.LeftSearchableString();
            Assert.Equal("gE3x2.000009", s);

            q = system.Quantity("1.6kg");
            s = q.LeftSearchableString();
            Assert.Equal("gE3x2.6", s);

            // The reminder should not get lost if the leftmost digit is indicating a copy to the left
            q = system.Quantity("9.6kg");
            s = q.LeftSearchableString();
            Assert.Equal("gE4x0.6", s);

            q = system.Quantity("99.6kg");
            s = q.LeftSearchableString();
            Assert.Equal("gE5x0.06", s);

            q = system.Quantity("999.6kg");
            s = q.LeftSearchableString();
            Assert.Equal("gE6x0.006", s);
        }

        [Fact]
        public void TestComparisonLeftSearchable()
        {
            var q1 = system.Quantity("100kg");
            var q2 = system.Quantity("10kg");
            var s1 = q1.LeftSearchableString();
            var s2 = q2.LeftSearchableString();

            Assert.Equal(1, s1.CompareTo(s2)); // q1 > q2

            q1 = system.Quantity("100.1010kg");
            q2 = system.Quantity("100.101kg");
            s1 = q1.LeftSearchableString();
            s2 = q2.LeftSearchableString();

            Assert.Equal(1, s1.CompareTo(s2)); // q1 has a higher precision than q2

            q1 = system.Quantity("100.9876543210kg");
            q2 = system.Quantity("100.1234567890kg");
            s1 = q1.LeftSearchableString();
            s2 = q2.LeftSearchableString();

            Assert.Equal(1, s1.CompareTo(s2));

            q1 = system.Quantity("9.6kg"); // gE4x0.6
            q2 = system.Quantity("1.5kg"); // gE3x1.5
            s1 = q1.LeftSearchableString();
            s2 = q2.LeftSearchableString();

            Assert.Equal(1, s1.CompareTo(s2));

            q1 = system.Quantity("9.6kg");  // gE4x0.6
            q2 = system.Quantity("10.6kg"); // gE4x11.6
            s1 = q1.LeftSearchableString();
            s2 = q2.LeftSearchableString();

            Assert.Equal(-1, s1.CompareTo(s2));

            q1 = system.Quantity("9.6e3g");  // 9.6kg =>  gE4x0.6
            q2 = system.Quantity("10.6e2g"); // 1.06kg => gE3x1.16
            s1 = q1.LeftSearchableString();
            s2 = q2.LeftSearchableString();

            Assert.Equal(1, s1.CompareTo(s2));

            q1 = system.Quantity("9.6g");
            q2 = system.Quantity("0.94e1g");
            s1 = q1.LeftSearchableString();
            s2 = q2.LeftSearchableString();

            Assert.Equal(1, s1.CompareTo(s2));
        }

    }
}
