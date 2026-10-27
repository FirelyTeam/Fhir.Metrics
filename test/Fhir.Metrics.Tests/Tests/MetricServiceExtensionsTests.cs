#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using FluentAssertions;
using Xunit;

namespace Fhir.Metrics.Tests.Tests;

/// <summary>
/// The decimal overloads in <see cref="MetricServiceExtensions"/> parse the strings an <see cref="IMetricService"/> returns. These tests
/// feed them a service that returns a fixed string, so they cover the parsing alone: plain and exponential notation, and values a decimal
/// cannot hold.
/// </summary>
public class MetricServiceExtensionsTests
{
    private const string Ucum = "http://unitsofmeasure.org";
    private static readonly (decimal value, string unit, string? codesystem) _anyQuantity = (1m, "kg", Ucum);

    private delegate (decimal value, string unit, string? codesystem)? Operation(IMetricService service, (decimal value, string unit, string? codesystem) quantity);

    [Theory]
    [InlineData("2500.0", "2500.0")]
    [InlineData("-0.800", "-0.800")]
    [InlineData("0.025400", "0.025400")]
    [InlineData("2.5E+03", "2500")]
    [InlineData("2.5E3", "2500")]
    [InlineData("2.5e+03", "2500")]
    [InlineData("2.50E+03", "2500")]
    [InlineData("1.3E-02", "0.013")]
    [InlineData("-6.6E-5", "-0.000066")]
    [InlineData("1E-28", "0.0000000000000000000000000001")]
    [InlineData("20E-29", "0.0000000000000000000000000002")]
    [InlineData("1.0000000000000000000000000001", "1.0000000000000000000000000001")]
    [InlineData("0E+10", "0")]
    public void TryCanonicalize_ServiceReturnsPlainOrExponentialNotation_ReturnsTheValue(string returned, string expected)
    {
        var service = new FixedValueMetricService(returned);

        service.TryCanonicalize(_anyQuantity, out var canonical).Should().BeTrue();

        canonical!.Value.value.ToString(CultureInfo.InvariantCulture).Should().Be(expected);
        canonical.Value.unit.Should().Be(FixedValueMetricService.Unit);
        canonical.Value.codesystem.Should().Be(Ucum);
    }

    [Theory]
    [InlineData("1E+29", "larger than a decimal holds")]
    [InlineData("6.6E-31", "a decimal would round it to zero")]
    [InlineData("1e-29", "a decimal would round it to zero")]
    [InlineData("0.00000000000000000000000000001", "a decimal would round it to zero")]
    [InlineData("1.5E-28", "the 5 lies beyond the 28th decimal place, so a decimal would round it to 2E-28")]
    [InlineData("6.6260755E-28", "the digits beyond the 28th decimal place would be rounded to 7E-28")]
    [InlineData("25E-29", "the 5 lies beyond the 28th decimal place, so a decimal would round it to 3E-28")]
    [InlineData("1.00000000000000000000000000001", "the last 1 lies beyond the 28th decimal place")]
    [InlineData("abc", "not a number")]
    [InlineData("", "not a number")]
    public void TryCanonicalize_ServiceReturnsAValueADecimalCannotHold_ReturnsFalse(string returned, string because)
    {
        var service = new FixedValueMetricService(returned);

        service.TryCanonicalize(_anyQuantity, out var canonical).Should().BeFalse(because);

        canonical.Should().BeNull();
    }

    [Fact]
    public void TryCanonicalize_ServiceFails_ReturnsFalse()
    {
        var service = new FixedValueMetricService(null);

        service.TryCanonicalize(_anyQuantity, out var canonical).Should().BeFalse();

        canonical.Should().BeNull();
    }

    [Theory]
    [InlineData(nameof(MetricServiceExtensions.TryDivide))]
    [InlineData(nameof(MetricServiceExtensions.TryMultiply))]
    [InlineData(nameof(MetricServiceExtensions.TryConvertTo))]
    [InlineData(nameof(MetricServiceExtensions.TrySubtract))]
    [InlineData(nameof(MetricServiceExtensions.TryAdd))]
    public void DecimalOverload_ServiceReturnsExponentialNotation_ReturnsTheValue(string operationName)
    {
        var service = new FixedValueMetricService("2.5E+03");

        var result = GetOperation(operationName)(service, _anyQuantity);

        result.Should().NotBeNull();
        result!.Value.value.Should().Be(2500m);
        result.Value.unit.Should().Be(FixedValueMetricService.Unit);
    }

    [Theory]
    [InlineData(nameof(MetricServiceExtensions.TryDivide))]
    [InlineData(nameof(MetricServiceExtensions.TryMultiply))]
    [InlineData(nameof(MetricServiceExtensions.TryConvertTo))]
    [InlineData(nameof(MetricServiceExtensions.TrySubtract))]
    [InlineData(nameof(MetricServiceExtensions.TryAdd))]
    public void DecimalOverload_ServiceReturnsAValueADecimalRoundsToZero_ReturnsNull(string operationName)
    {
        var service = new FixedValueMetricService("6.6E-31");

        var result = GetOperation(operationName)(service, _anyQuantity);

        result.Should().BeNull();
    }

    private static Operation GetOperation(string name) => name switch
    {
        nameof(MetricServiceExtensions.TryDivide) => (service, quantity) => service.TryDivide(quantity, quantity, out var result) ? result : null,
        nameof(MetricServiceExtensions.TryMultiply) => (service, quantity) => service.TryMultiply(quantity, quantity, out var result) ? result : null,
        nameof(MetricServiceExtensions.TryConvertTo) => (service, quantity) => service.TryConvertTo(quantity, "g", out var result) ? result : null,
        nameof(MetricServiceExtensions.TrySubtract) => (service, quantity) => service.TrySubtract(quantity, quantity, out var result) ? result : null,
        nameof(MetricServiceExtensions.TryAdd) => (service, quantity) => service.TryAdd(quantity, quantity, out var result) ? result : null,
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, "not a decimal overload with a value to parse")
    };

    /// <summary>
    /// A service that answers every operation with the same value string, or fails every operation when the value is null.
    /// </summary>
    private sealed class FixedValueMetricService(string? value) : IMetricService
    {
        public const string Unit = "g";

        public bool TryCanonicalize((string value, string unit, string? codesystem) quantity, [NotNullWhen(true)] out (string value, string unit, string? codesystem)? canonical)
            => TryAnswer(out canonical);

        public bool TryDivide((string value, string unit, string? codesystem) quantity1, (string value, string unit, string? codesystem) quantity2, [NotNullWhen(true)] out (string value, string unit, string? codesystem)? result)
            => TryAnswer(out result);

        public bool TryMultiply((string value, string unit, string? codesystem) quantity1, (string value, string unit, string? codesystem) quantity2, [NotNullWhen(true)] out (string value, string unit, string? codesystem)? result)
            => TryAnswer(out result);

        public bool TryCompare((string value, string unit, string? codesystem) quantity1, (string value, string unit, string? codesystem) quantity2, [NotNullWhen(true)] out int? result)
        {
            result = value is null ? null : 0;
            return result is not null;
        }

        public bool TryConvertTo((string value, string unit, string? codesystem) quantity, string targetUnit, [NotNullWhen(true)] out (string value, string unit, string? codesystem)? converted)
            => TryAnswer(out converted);

        public bool TrySubtract((string value, string unit, string? codesystem) quantity1, (string value, string unit, string? codesystem) quantity2, [NotNullWhen(true)] out (string value, string unit, string? codesystem)? result)
            => TryAnswer(out result);

        public bool TryAdd((string value, string unit, string? codesystem) quantity1, (string value, string unit, string? codesystem) quantity2, [NotNullWhen(true)] out (string value, string unit, string? codesystem)? result)
            => TryAnswer(out result);

        private bool TryAnswer([NotNullWhen(true)] out (string value, string unit, string? codesystem)? answer)
        {
            answer = value is null ? null : (value, Unit, Ucum);
            return answer is not null;
        }
    }
}
