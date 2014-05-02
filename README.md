Fhir.Metrics
============

WORK IN PROGRESS

Unified Code for Units of Measure (UCUM) implementation for FHIR.

Classes:

- Exponential (Normalized Scientific Notation number, with error propagation)
- Unit (measurable dimensions like gram, meter, Kelvin etc. = UCUM Base Unit)
- Metric (a prefixed unit like kg, mm, = UCUM Unit)
- Quantity (a measurement; Exponential + Metric)
- UcumReader (reader of the XML essence of UCUM)

Not yet finished:
Conversions of Quantities from one metric to another.
