Fhir.Metrics
============
Unified Code for Units of Measure (UCUM) implementation for FHIR.

UcumReader now reads UCM essence xml set. And parses basic conversions and more Complex conversions 
Example:
```  
Quantity q = system.Convert("4.3[psi]'");
```
will result in
```
{[2,25 ± 0,005649]E2 g.m-1.s-2} (approximated)
```
Classes:
- Exponential (Normalized Scientific Notation number, with error propagation)
- Unit (measurable dimensions like gram, meter, Kelvin, pound force, etc.)
- Prefix (kilo, mega, mili, micro etc.)
- Metric (A composite set of (prefixed) units like m/s, kg.m/s2)
- Quantity (a measurement: Exponential + Metric)
- UcumReader (reader of the XML essence of UCUM)
 

TODO:
- UcumReader doesn't implement special cases (like fahrenheid/celcius conversion formulas)
- Add support for [nested terms](https://ucum.org/ucum.html#section-Syntax-Rules)
