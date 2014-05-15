Fhir.Metrics
============
Unified Code for Units of Measure (UCUM) implementation for FHIR.

UcumReader now reads UCM essence xml set. And parses basic conversions.
Complex conversions (formula's with mixes of units with numbers) not yet implemented. 

Classes:
- Exponential (Normalized Scientific Notation number, with error propagation)
- Unit (measurable dimensions like gram, meter, Kelvin, pound force, etc.)
- Prefix (kilo, mega, mili, micro etc.)
- Metric (A composite set of (prefixed) units like m/s, kg.m/s2)
- Quantity (a measurement: Exponential + Metric)
- UcumReader (reader of the XML essence of UCUM)
 

TODO:
- UcumReader doesn't implement special cases (like fahrenheid/celcius conversion formulas)
