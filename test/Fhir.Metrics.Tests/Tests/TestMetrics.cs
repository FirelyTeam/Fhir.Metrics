using System;
using FluentAssertions;
using Xunit;

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

        [Fact]
        public void BugfixAvoidLoop()
        {
            // #3 - this case caused infinite work in Regex.IsMatch with our previous regex pattern
            // https://github.com/FirelyTeam/Fhir.Metrics/issues/3
            // ^(((?[./])?(?[^./]+)))?$
            // Is now replaced with: (((?[./])?(?[^./]+)))?
            string unit = "ForinterpretationofeGFRseehttp://www.kidney.org.au";
            Assert.Throws<ArgumentException>(() => system.Metric(unit));
        }

        [Fact]
        public void MetricWithValidAnnotations()
        {
            Metric metric;

            metric = system.Metric("{rbc}");
            Assert.Single(metric.Axes);
            Assert.Equal("1", metric.Symbols);

            metric = system.Metric("mL/min/{1.73_m2}");
            Assert.Equal(3, metric.Axes.Count);
            Assert.Equal("mL.min-1.1-1", metric.Symbols);

            metric = system.Metric("10*3.{RBC}");
            Assert.Equal(2, metric.Axes.Count);
            Assert.Equal("10*3.1", metric.Symbols);

            metric = system.Metric("ml{total}");
            Assert.Single(metric.Axes);
            Assert.Equal("ml", metric.Symbols);
            
            metric = system.Metric("{reads}/{base}");
            Assert.Single(metric.Axes);
            Assert.Equal("1/1", metric.Symbols);
        }

        [Fact]
        public void MetricWithInvalidAnnotations()
        {
            Action act = () => system.Metric("{{1}}");
            act.Should().Throw<ArgumentException>("Nested annotations are not allowed");
        }

        [Fact]
        public void MetricShouldRejectWhitespaces()
        {
            var metric = "/min 1/min {breaths}/min {breath}/min {resp}/min";
            Action act = () => system.Metric(metric);
            act.Should().Throw<ArgumentException>("Whitespaces are not allowed in a metric expression").And.Message.Should().Contain(metric);

            metric = " /min ";
            act = () => system.Metric(" /min ");
            act.Should().Throw<ArgumentException>("Whitespaces are not allowed in a metric expression").And.Message.Should().Contain(metric);

            metric = "/min ";
            act = () => system.Metric("/min ");
            act.Should().Throw<ArgumentException>("Whitespaces are not allowed in a metric expression").And.Message.Should().Contain(metric);
        }
    }
}
