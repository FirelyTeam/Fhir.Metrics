using System;
using Xunit;
using Fhir.Metrics;

namespace Fhir.Metrics.Tests
{

    public class TestMetrics
    {
        SystemOfUnits system;

        
        public TestMetrics()
        {
            system = UCUM.Load();
        }

        [Fact]
        public void Parsing()
        {
            Metric metric;
            
            metric = system.Metric("J/s");
            Assert.Equal(2, metric.Axes.Count);

            metric = system.Metric("[ft_i].[lbf_av]/s");
            Assert.Equal(3, metric.Axes.Count);
        }

        [Fact]
        public void TestMetricDimensions()
        {
            Quantity q = system.Quantity("2.3[psi]");
            q = system.Canonical(q);
            Console.WriteLine(q.Metric.DimensionText);
            Assert.Equal("mass^1.length^-1.time^-2", q.Metric.DimensionText);
        }

        [Fact]
        public void Equality()
        {
            Metric a, b;
            a = system.Metric("kg");
            b = system.Metric("kg");
            Assert.Equal(a, b);
            Assert.Equal(b, a);

            a = system.Metric("kg");
            b = system.Metric("N");
            Assert.NotEqual(a, b);
            Assert.NotEqual(b, a);
            

            a = system.Metric("kg");
            b = system.Metric("kg.m2");
            Assert.NotEqual(a, b);
            Assert.NotEqual(b, a);

            a = system.Metric("kg");
            b = system.Metric("kg.m.s-2");
            Assert.NotEqual(a, b);
            Assert.NotEqual(b, a);
        }

        [Fact]
        public void Algebra()
        {
            Metric kg = system.Metric("kg");
            Metric ms2 = system.Metric("m/s2");
            Metric F = kg * ms2;
            Assert.Equal("kg.m.s-2", F.ToString());

            Metric m = system.Metric("m");
            Metric s2 = system.Metric("s2");
            Metric t = m / s2;

            Assert.Equal(ms2, t);

        }
    }
}
