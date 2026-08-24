# ADR 0003: Fixed Colombia catalog and composite hierarchy

- Status: Accepted
- Date: 2026-08-24
- Classification: Relational country/department/municipality data and coherent
  references are COINK requirements; the complete traceable DIVIPOLA snapshot is
  value-added.

## Context

Valid independent identifiers do not prove that a municipality belongs to the
submitted department and country. Network-fetched seed data would also make
startup nondeterministic and unreviewable.

## Decision

Check in one fixed catalog: Colombia, 33 department-level divisions, and 1,119
municipality-level DIVIPOLA units. Preserve source provenance and SHA-256. Use
composite foreign keys so a user references one valid country-department-
municipality tuple. Treat the catalog as read-only application data.

## Consequences

Hierarchy integrity is atomic even if application validation is bypassed, and
container startup works offline. The snapshot can age independently of DANE;
updating it is an explicit reviewed data-version change, not an automatic merge.
