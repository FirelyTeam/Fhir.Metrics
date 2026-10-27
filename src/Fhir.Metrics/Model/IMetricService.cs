#nullable enable
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Fhir.Metrics;

/// <summary>
/// Service interface for canonicalization, comparison and arithmetic operations on UCUM quantities. The service uses tuples (string, string, string) to represent quantities.
/// </summary>
public interface IMetricService
{
    /// <summary>
    /// Tries to convert a given quantity to its canonical form.
    /// </summary>
    /// <param name="quantity">the quantity to convert</param>
    /// <param name="canonical">the canonicalized quantity, or null if the operation failed</param>
    /// <returns>true if the canonicalization succeeded, false otherwise</returns>
    public bool TryCanonicalize((string value, string unit, string? codesystem) quantity, [NotNullWhen(true)] out (string value, string unit, string? codesystem)? canonical);
    
    /// <summary>
    /// Tries to divide quantity 1 by quantity 2. This will also perform a division of the units. 
    /// </summary>
    /// <param name="quantity1">the quantity to perform the operation on</param>
    /// <param name="quantity2">the quantity to divide the first quantity by</param>
    /// <param name="result">the canonical form of the result of the division, or null if the operation failed</param>
    /// <returns>true if the operation succeeded, false otherwise</returns>
    public bool TryDivide((string value, string unit, string? codesystem) quantity1, (string value, string unit, string? codesystem) quantity2, [NotNullWhen(true)] out (string value, string unit, string? codesystem)? result);
    
    /// <summary>
    /// Tries to multiply quantity 1 with quantity 2. This will also perform a multiplication of the units
    /// </summary>
    /// <param name="quantity1">the quantity to perform the operation on</param>
    /// <param name="quantity2">the quantity to multiply the first quantity by</param>
    /// <param name="result">the result of the multiplication, or null if the operation failed</param>
    /// <returns>true if the operation succeeded, false otherwise</returns>
    public bool TryMultiply((string value, string unit, string? codesystem) quantity1, (string value, string unit, string? codesystem) quantity2, [NotNullWhen(true)] out (string value, string unit, string? codesystem)? result);
    
    /// <summary>
    /// Tries to compare quantity 1 with quantity 2. Will fail if the units have different dimensions.
    /// </summary>
    /// <param name="quantity1"></param>
    /// <param name="quantity2"></param>
    /// <param name="result">&lt;0 if quantity1 &lt; quantity2, 0 if quantity1 == quantity2 (within error margin), &gt;0 if quantity1 &gt; quantity2, or null if the operands could not be compared</param>
    /// <returns>true if the operation succeeded, false otherwise</returns>
    public bool TryCompare((string value, string unit, string? codesystem) quantity1, (string value, string unit, string? codesystem) quantity2, [NotNullWhen(true)] out int? result);
    
    
    // from here on - not yet implemented in the old one
    
    /// <summary>
    /// Tries to convert quantity 1 to the target unit. Will fail if the units have different dimensions, or if the target unit could not be found.
    /// </summary>
    /// <param name="quantity">the quantity to convert</param>
    /// <param name="targetUnit">the target unit</param>
    /// <param name="converted">the result if the conversion, or null if the operation failed</param>
    /// <returns>true if the conversion succeeded, false otherwise</returns>
    public bool TryConvertTo((string value, string unit, string? codesystem) quantity, string targetUnit, [NotNullWhen(true)] out (string value, string unit, string? codesystem)? converted);
    
    /// <summary>
    /// Tries to subtract quantity 2 from quantity 1. Will fail if the units have different dimensions.
    /// </summary>
    /// <param name="quantity1">the quantity to subtract from</param>
    /// <param name="quantity2">the quantity to subtract from the first operand</param>
    /// <param name="result">the result of the subtraction in canonical form, or null if the operation failed</param>
    /// <returns>true if the operation succeeded, false otherwise</returns>
    public bool TrySubtract((string value, string unit, string? codesystem) quantity1, (string value, string unit, string? codesystem) quantity2, [NotNullWhen(true)] out (string value, string unit, string? codesystem)? result);
    
    /// <summary>
    /// Tries to add quantity 2 to quantity 1. Will fail if the units have different dimensions.
    /// </summary>
    /// <param name="quantity1">the quantity to add to</param>
    /// <param name="quantity2">the quantity to add to the first operand</param>
    /// <param name="result">the result of the addition in canonical form, or null if the operation failed</param>
    /// <returns>true if the operation succeeded, false otherwise</returns>
    public bool TryAdd((string value, string unit, string? codesystem) quantity1, (string value, string unit, string? codesystem) quantity2, [NotNullWhen(true)] out (string value, string unit, string? codesystem)? result);
}

/// <summary>
/// Overloads of the <see cref="IMetricService"/> members that take and return native decimals instead of strings.
/// </summary>
/// <remarks>
/// A value the service returns is accepted in plain notation (2500.0) and in exponential notation (2.5E+03, 6.6E-5). A value a decimal
/// cannot hold makes the call return false: one that is too large (1E+29), and one so small that a decimal would round it to zero
/// (6.6E-31), since a zero would hide that the value was lost.
/// </remarks>
public static class MetricServiceExtensions
{
    private static readonly char[] _exponentMarkers = ['e', 'E'];
    private static readonly char[] _nonZeroDigits = ['1', '2', '3', '4', '5', '6', '7', '8', '9'];

    /// <summary>
    /// Parses a value the service returned, in plain or exponential notation. Fails for a value that does not fit in a decimal, and for
    /// a non-zero value that a decimal would round to zero.
    /// </summary>
    private static bool TryParseValue(string value, out decimal result)
    {
        if (!decimal.TryParse(value, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out result))
        {
            return false;
        }

        if (result != 0 || !HasNonZeroDigit(value))
        {
            return true;
        }

        result = 0;
        return false;
    }

    private static bool HasNonZeroDigit(string value)
    {
        var exponentIndex = value.IndexOfAny(_exponentMarkers);
        var mantissa = exponentIndex < 0 ? value : value.Substring(0, exponentIndex);
        return mantissa.IndexOfAny(_nonZeroDigits) >= 0;
    }

    /// <inheritdoc cref="IMetricService.TryCanonicalize"/>
    public static bool TryCanonicalize(this IMetricService service, (decimal value, string unit, string? codesystem) quantity, [NotNullWhen(true)] out (decimal value, string unit, string? codesystem)? canonical)
    {
        if(!service.TryCanonicalize((quantity.value.ToString(CultureInfo.InvariantCulture), quantity.unit, quantity.codesystem), out var canonicalTuple))
        {
            canonical = null;
            return false;
        }

        if (!TryParseValue(canonicalTuple.Value.value, out var value))
        {
            canonical = null;
            return false;
        }

        canonical = (value, canonicalTuple.Value.unit, canonicalTuple.Value.codesystem);
        return true;
    }
    
    /// <inheritdoc cref="IMetricService.TryDivide"/>
    public static bool TryDivide(this IMetricService service, (decimal value, string unit, string? codesystem) quantity1, (decimal value, string unit, string? codesystem) quantity2, [NotNullWhen(true)] out (decimal value, string unit, string? codesystem)? result)
    {
        if(!service.TryDivide((quantity1.value.ToString(CultureInfo.InvariantCulture), quantity1.unit, quantity1.codesystem), (quantity2.value.ToString(CultureInfo.InvariantCulture), quantity2.unit, quantity2.codesystem), out var resultTuple))
        {
            result = null;
            return false;
        }

        if (!TryParseValue(resultTuple.Value.value, out var value))
        {
            result = null;
            return false;
        }

        result = (value, resultTuple.Value.unit, resultTuple.Value.codesystem);
        return true;
    }
    
    /// <inheritdoc cref="IMetricService.TryMultiply"/>
    public static bool TryMultiply(this IMetricService service, (decimal value, string unit, string? codesystem) quantity1, (decimal value, string unit, string? codesystem) quantity2, [NotNullWhen(true)] out (decimal value, string unit, string? codesystem)? result)
    {
        if(!service.TryMultiply((quantity1.value.ToString(CultureInfo.InvariantCulture), quantity1.unit, quantity1.codesystem), (quantity2.value.ToString(CultureInfo.InvariantCulture), quantity2.unit, quantity2.codesystem), out var resultTuple))
        {
            result = null;
            return false;
        }

        if (!TryParseValue(resultTuple.Value.value, out var value))
        {
            result = null;
            return false;
        }

        result = (value, resultTuple.Value.unit, resultTuple.Value.codesystem);
        return true;
    }
    
    /// <inheritdoc cref="IMetricService.TryCompare"/>
    public static bool TryCompare(this IMetricService service, (decimal value, string unit, string? codesystem) quantity1, (decimal value, string unit, string? codesystem) quantity2, [NotNullWhen(true)] out int? result)
    {
        if(!service.TryCompare((quantity1.value.ToString(CultureInfo.InvariantCulture), quantity1.unit, quantity1.codesystem), (quantity2.value.ToString(CultureInfo.InvariantCulture), quantity2.unit, quantity2.codesystem), out var resultInt))
        {
            result = null;
            return false;
        }

        result = resultInt;
        return true;
    }
    
    /// <inheritdoc cref="IMetricService.TryConvertTo"/>
    public static bool TryConvertTo(this IMetricService service, (decimal value, string unit, string? codesystem) quantity, string targetUnit, [NotNullWhen(true)] out (decimal value, string unit, string? codesystem)? converted)
    {
        if(!service.TryConvertTo((quantity.value.ToString(CultureInfo.InvariantCulture), quantity.unit, quantity.codesystem), targetUnit, out var convertedTuple))
        {
            converted = null;
            return false;
        }

        if (!TryParseValue(convertedTuple.Value.value, out var value))
        {
            converted = null;
            return false;
        }

        converted = (value, convertedTuple.Value.unit, convertedTuple.Value.codesystem);
        return true;
    }
    
    /// <inheritdoc cref="IMetricService.TrySubtract"/>
    public static bool TrySubtract(this IMetricService service, (decimal value, string unit, string? codesystem) quantity1, (decimal value, string unit, string? codesystem) quantity2, [NotNullWhen(true)] out (decimal value, string unit, string? codesystem)? result)
    {
        if(!service.TrySubtract((quantity1.value.ToString(CultureInfo.InvariantCulture), quantity1.unit, quantity1.codesystem), (quantity2.value.ToString(CultureInfo.InvariantCulture), quantity2.unit, quantity2.codesystem), out var resultTuple))
        {
            result = null;
            return false;
        }

        if (!TryParseValue(resultTuple.Value.value, out var value))
        {
            result = null;
            return false;
        }

        result = (value, resultTuple.Value.unit, resultTuple.Value.codesystem);
        return true;
    }
    
    /// <inheritdoc cref="IMetricService.TryAdd"/>
    public static bool TryAdd(this IMetricService service, (decimal value, string unit, string? codesystem) quantity1, (decimal value, string unit, string? codesystem) quantity2, [NotNullWhen(true)] out (decimal value, string unit, string? codesystem)? result)
    {
        if(!service.TryAdd((quantity1.value.ToString(CultureInfo.InvariantCulture), quantity1.unit, quantity1.codesystem), (quantity2.value.ToString(CultureInfo.InvariantCulture), quantity2.unit, quantity2.codesystem), out var resultTuple))
        {
            result = null;
            return false;
        }

        if (!TryParseValue(resultTuple.Value.value, out var value))
        {
            result = null;
            return false;
        }

        result = (value, resultTuple.Value.unit, resultTuple.Value.codesystem);
        return true;
    }
}