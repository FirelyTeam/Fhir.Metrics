using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fhir.Metrics.Tests
{
    [TestClass]
    public class TestMetricUtils
    {
        [TestMethod]
        public void NotationError()
        {
            Assert.AreEqual(1m * MetricUtils.DEFAULT_ERROR_DIGIT, MetricUtils.NotationError(0));
            Assert.AreEqual(1m * MetricUtils.DEFAULT_ERROR_DIGIT, MetricUtils.NotationError(1));
            Assert.AreEqual(0.1m * MetricUtils.DEFAULT_ERROR_DIGIT, MetricUtils.NotationError(0.0m));
            Assert.AreEqual(0.01m * MetricUtils.DEFAULT_ERROR_DIGIT, MetricUtils.NotationError(0.12m));
            Assert.AreEqual(0.001m * MetricUtils.DEFAULT_ERROR_DIGIT, MetricUtils.NotationError(0.120m));
            Assert.AreEqual(0.01m * MetricUtils.DEFAULT_ERROR_DIGIT, MetricUtils.NotationError(-2.12m));
        }

        [TestMethod]
        public void Shift()
        {
            Assert.AreEqual(10m, MetricUtils.Shift(1m, 1));
            Assert.AreEqual(100m, MetricUtils.Shift(1m, 2));
            Assert.AreEqual(100000m, MetricUtils.Shift(1m, 5));
            Assert.AreEqual(0.1m, MetricUtils.Shift(1m, -1));
            Assert.AreEqual(0.00001m, MetricUtils.Shift(1m, -5));
        }

        [TestMethod]
        public void ToPrecision()
        {
            Assert.AreEqual("50.00",MetricUtils.ToPrecision("50", 2));
            Assert.AreEqual("50.0", MetricUtils.ToPrecision("50", 1));
            Assert.AreEqual("50", MetricUtils.ToPrecision("50", 0));
            Assert.AreEqual("50", MetricUtils.ToPrecision("50.", 0));
            Assert.AreEqual("50", MetricUtils.ToPrecision("50.10", 0));
            Assert.AreEqual("000050.00", MetricUtils.ToPrecision("000050", 2));
            Assert.AreEqual("000050.00", MetricUtils.ToPrecision("000050.", 2));
            Assert.AreEqual("000050.10", MetricUtils.ToPrecision("000050.10", 2));
            Assert.AreEqual("50.0", MetricUtils.ToPrecision("50.0001", 1));
            Assert.AreEqual("50.000", MetricUtils.ToPrecision("50.0001", 3));
            Assert.AreEqual("50.0001", MetricUtils.ToPrecision("50.0001", 4));
            Assert.AreEqual("50.00010", MetricUtils.ToPrecision("50.0001", 5));
            Assert.AreEqual("0.12", MetricUtils.ToPrecision("0.1234", 2));
            Assert.AreEqual("1.12", MetricUtils.ToPrecision("1.1234", 2));
            Assert.AreEqual("1.00", MetricUtils.ToPrecision("1.0", 2));
            Assert.AreEqual("1.00", MetricUtils.ToPrecision("1", 2));
            Assert.AreEqual("1.00", MetricUtils.ToPrecision("1.", 2)); 
        }
        
        [TestMethod]
        public void SignificantFigures()
        {
            Assert.AreEqual(string.Empty,MetricUtils.SignificantFigures("000"));
            Assert.AreEqual("1230", MetricUtils.SignificantFigures("01230"));
            Assert.AreEqual("1230", MetricUtils.SignificantFigures("01230."));
            Assert.AreEqual("12301", MetricUtils.SignificantFigures("01230.1"));
            Assert.AreEqual("12301234000", MetricUtils.SignificantFigures("01230.1234000"));
            Assert.AreEqual("23", MetricUtils.SignificantFigures(".23"));
        }

        [TestMethod]
        public void StringToDecimal()
        {
            Assert.AreEqual(50.50100m, MetricUtils.StringToDecimal("00050.50100"));
            Assert.AreEqual(1000m, MetricUtils.StringToDecimal("1000"));
            Assert.AreEqual(-1000.11m, MetricUtils.StringToDecimal("-1000.11"));
            Assert.AreEqual(1.0m, MetricUtils.StringToDecimal("1."));
            Assert.AreEqual(0.1m, MetricUtils.StringToDecimal(".1"));
        }

        [TestMethod]
        public void Round()
        {
            Assert.AreEqual("123",MetricUtils.Round(123.45678m, 0, 0));
            Assert.AreEqual("123", MetricUtils.Round(123.45678m, 1, 0));
            Assert.AreEqual("123.4", MetricUtils.Round(123.45678m, 2, 1));
            Assert.AreEqual("123.5", MetricUtils.Round(123.45678m, 1, 1));
            Assert.AreEqual("123.50", MetricUtils.Round(123.45678m, 1, 2));
            Assert.AreEqual("123.46", MetricUtils.Round(123.45678m, 2, 2));
            Assert.AreEqual("457", MetricUtils.Round(456.789m, 0, 0));
            Assert.AreEqual("457.0", MetricUtils.Round(456.789m, 0, 1));
            Assert.AreEqual("456.8", MetricUtils.Round(456.789m, 1, 1));
            Assert.AreEqual("456.7", MetricUtils.Round(456.789m, 2, 1));
            Assert.AreEqual("456.79", MetricUtils.Round(456.789m, 2, 2));
        }

        [TestMethod]
        public void RoundUncertainty()
        {
            Assert.AreEqual(0.1m, MetricUtils.RoundUncertainty(0.1m));
            Assert.AreEqual(0.12m, MetricUtils.RoundUncertainty(0.12m));
            Assert.AreEqual(0.15m, MetricUtils.RoundUncertainty(0.15m));
            Assert.AreEqual(0.15m, MetricUtils.RoundUncertainty(0.152m));
            Assert.AreEqual(0.16m, MetricUtils.RoundUncertainty(0.157m));
            Assert.AreEqual(0.3m,MetricUtils.RoundUncertainty(0.345m));
            Assert.AreEqual(0.4m, MetricUtils.RoundUncertainty(0.389m));
            Assert.AreEqual(1.3m, MetricUtils.RoundUncertainty(1.3m));
            Assert.AreEqual(5m, MetricUtils.RoundUncertainty(5m));
            Assert.AreEqual(5m, MetricUtils.RoundUncertainty(5.1m));
            Assert.AreEqual(6m, MetricUtils.RoundUncertainty(5.52m));
            Assert.AreEqual(500m, MetricUtils.RoundUncertainty(500.2m));
        }

        [TestMethod]
        public void DecimalToString()
        {
            Assert.AreEqual("0.5",MetricUtils.DecimalToString(0.5m,0.82m));
            Assert.AreEqual("18.8", MetricUtils.DecimalToString(18.850m, 1.3m));
            Assert.AreEqual("9.0", MetricUtils.DecimalToString(9.04m, 0.9m));
            Assert.AreEqual("0.7", MetricUtils.DecimalToString(0.6667m, 0.2m));
            Assert.AreEqual("29", MetricUtils.DecimalToString(28.638m, 14.03m));
            Assert.AreEqual("1.20", MetricUtils.DecimalToString(1.204m, 0.181m));
            Assert.AreEqual("12.03", MetricUtils.DecimalToString(12.0349m, 0.153m));
            Assert.AreEqual("4000", MetricUtils.DecimalToString(4000m, 0m));
            Assert.AreEqual("4000.0", MetricUtils.DecimalToString(4000m, 0.1m));
        }

        [TestMethod]
        public void SignificantDigits()
        {
            Assert.AreEqual("4000",MetricUtils.SignificantFigures("4000"));
            Assert.AreEqual("4",MetricUtils.SignificantFigures("4")); 
            Assert.AreEqual("4",MetricUtils.SignificantFigures("4."));
            Assert.AreEqual("41",MetricUtils.SignificantFigures("4.1"));
            Assert.AreEqual("4000",MetricUtils.SignificantFigures("4.000"));
            Assert.AreEqual("4005",MetricUtils.SignificantFigures("4.005"));
            Assert.AreEqual("4055", MetricUtils.SignificantFigures("4.055"));
            Assert.AreEqual("4555", MetricUtils.SignificantFigures("4.555"));
            Assert.AreEqual(String.Empty, MetricUtils.SignificantFigures("0"));
            Assert.AreEqual(String.Empty, MetricUtils.SignificantFigures("0.0"));
            Assert.AreEqual(String.Empty, MetricUtils.SignificantFigures(""));
            Assert.AreEqual(String.Empty, MetricUtils.SignificantFigures("0.000"));
            Assert.AreEqual("5",MetricUtils.SignificantFigures("0.005"));
            Assert.AreEqual("55",MetricUtils.SignificantFigures("0.055"));
            Assert.AreEqual("555",MetricUtils.SignificantFigures("0.555"));
            Assert.AreEqual("5",MetricUtils.SignificantFigures("0.5"));
            Assert.AreEqual("50",MetricUtils.SignificantFigures("0.50"));
            Assert.AreEqual("500",MetricUtils.SignificantFigures("0.500"));
            Assert.AreEqual("400",MetricUtils.SignificantFigures("400."));
            Assert.AreEqual("4000",MetricUtils.SignificantFigures("400.0"));
        }
    }
}
