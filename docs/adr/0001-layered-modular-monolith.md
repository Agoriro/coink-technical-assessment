# ADR 0001: Layered modular monolith

- Status: Accepted
- Date: 2026-08-24
- Classification: Value-added decision; design patterns were appreciated, but
  this exact architecture was not prescribed by COINK.

## Context

The assessment needs one registration API, relational data, validation, and
Stored Procedure access. It also benefits from obvious ownership boundaries
without distributed-system cost.

## Decision

Use one deployable API with `Api`, `Application`, `Domain`, and `Infrastructure`
projects. API owns HTTP and composition; Application owns use cases, validation,
and repository contracts; Domain remains framework-independent; Infrastructure
owns Dapper/Npgsql and database outcome translation. Dependencies point inward.

## Consequences

Business flow can be tested without HTTP or PostgreSQL, database details do not
leak into API contracts, and reviewers can navigate one coherent solution.
There is some mapping code between layers. CQRS, MediatR, generic repositories,
Unit of Work wrappers, microservices, and distributed infrastructure are omitted
because current scope gives them no concrete benefit.
