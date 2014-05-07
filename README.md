Fhir.Metrics
============

WORK IN PROGRESS

Unified Code for Units of Measure (UCUM) implementation for FHIR.

UcumReader now reads UCM essence xml set. And parses basic conversions.
Complex conversions (formula's with mixes of units with numbers) not yet implemented. 

Classes:
- Exponential (Normalized Scientific Notation number, with error propagation)
- Unit (measurable dimensions like gram, meter, Kelvin etc. = UCUM Base Unit)
- Prefix (kilo, mega, mili, micro etc.)
- Metric (a prefixed unit like kg, mm, = UCUM Unit)
- Quantity (a measurement: Exponential + Metric)
- UcumReader (reader of the XML essence of UCUM)

