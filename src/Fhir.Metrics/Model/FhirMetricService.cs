using System;
using System.Collections.Generic;
using System.Globalization;

namespace Fhir.Metrics;

public class FhirMetricService : IMetricService
{
    private static readonly Lazy<SystemOfUnits> _system = new(() => UCUM.Load());
    
    public static readonly Lazy<FhirMetricService> Instance = new(() => new FhirMetricService());
    
    /// <inheritdoc />
    public bool TryCanonicalize((string value, string unit, string codesystem) quantity, out (string value, string unit, string codesystem)? canonical)
    {
        try
        {
            Quantity q = ToQuantity(quantity);
            q = _system.Value.Conversions.Canonical(q);
            canonical = ToTuple(q);
            return true;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidCastException)
        {
            canonical = null;
            return false;
        }
    }

    /// <inheritdoc />
    public bool TryDivide((string value, string unit, string codesystem) quantity1, (string value, string unit, string codesystem) quantity2,
        out (string value, string unit, string codesystem)? result)
    {
        try
        {
            Quantity q1 = _system.Value.Conversions.Canonical(ToQuantity(quantity1));
            Quantity q2 = _system.Value.Conversions.Canonical(ToQuantity(quantity2));
            Quantity resultQuantity = q1 / q2;
            result = ToTuple(resultQuantity);
            return true;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidCastException)
        {
            result = null;
            return false;
        }
    }
    
    /// <inheritdoc />

    public bool TryMultiply((string value, string unit, string codesystem) quantity1, (string value, string unit, string codesystem) quantity2,
        out (string value, string unit, string codesystem)? result)
    {
        try
        {
            Quantity q1 = _system.Value.Conversions.Canonical(ToQuantity(quantity1));
            Quantity q2 = _system.Value.Conversions.Canonical(ToQuantity(quantity2));
            Quantity resultQuantity = q1 * q2;
            result = ToTuple(resultQuantity);
            return true;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidCastException)
        {
            result = null;
            return false;
        }
    }

    /// <inheritdoc />
    public bool TryCompare((string value, string unit, string codesystem) quantity1, (string value, string unit, string codesystem) quantity2, out int? result)
    {
        try
        {
            string s1 = _system.Value.Conversions.Canonical(ToQuantity(quantity1)).LeftSearchableString();
            string s2 = _system.Value.Conversions.Canonical(ToQuantity(quantity2)).LeftSearchableString();
            result = String.Compare(s1, s2, StringComparison.Ordinal);
            return true;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidCastException)
        {
            result = null;
            return false;
        }
    }

    /// <inheritdoc />
    public bool TryConvertTo((string value, string unit, string codesystem) quantity, string targetUnit, out (string value, string unit, string codesystem)? converted)
    {
        try
        {
            Quantity src = _system.Value.Conversions.Canonical(ToQuantity(quantity));
            Quantity targetBase = _system.Value.Conversions.Canonical(ToQuantity(("1", targetUnit, quantity.codesystem)));

            if (!Quantity.SameDimension(src, targetBase))
            {
                converted = null;
                return false;
            }

            Exponential convertedValue = Exponential.Divide(src.Value, targetBase.Value);
            converted = (convertedValue.ToDecimal().ToString(CultureInfo.InvariantCulture), targetUnit, quantity.codesystem);
            return true;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidCastException)
        {
            converted = null;
            return false;
        }
    }

    /// <inheritdoc />
    public bool TrySubtract((string value, string unit, string codesystem) quantity1, (string value, string unit, string codesystem) quantity2,
        out (string value, string unit, string codesystem)? result)
    {
        try
        {
            Quantity q1 = _system.Value.Conversions.Canonical(ToQuantity(quantity1));
            Quantity q2 = _system.Value.Conversions.Canonical(ToQuantity(quantity2));
            Quantity resultQuantity = q1 - q2;
            result = ToTuple(resultQuantity);
            return true;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidCastException)
        {
            result = null;
            return false;
        }
    }

    /// <inheritdoc />
    public bool TryAdd((string value, string unit, string codesystem) quantity1, (string value, string unit, string codesystem) quantity2,
        out (string value, string unit, string codesystem)? result)
    {
        try
        {
            Quantity q1 = _system.Value.Conversions.Canonical(ToQuantity(quantity1));
            Quantity q2 = _system.Value.Conversions.Canonical(ToQuantity(quantity2));
            Quantity resultQuantity = q1 + q2;
            result = ToTuple(resultQuantity);
            return true;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidCastException)
        {
            result = null;
            return false;
        }
    }
    
    private static Quantity ToQuantity((string value, string unit, string codesystem) quantity)
    {
        Metric metric = quantity.unit != null ? _system.Value.Metric(quantity.unit) : new Metric(new List<Metric.Axis>());
        return new Quantity(new Exponential(quantity.value), metric);
    }
    
    private static (string, string, string) ToTuple(Quantity quantity)
    {
        return (quantity.Value.ToDecimal().ToString(CultureInfo.InvariantCulture), quantity.Metric.ToString(), "http://unitsofmeasure.org");
    }
}