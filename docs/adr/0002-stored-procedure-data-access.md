# ADR 0002: PostgreSQL Stored Procedures through Dapper/Npgsql

- Status: Accepted
- Date: 2026-08-24
- Classification: COINK requires Stored Procedure consumption; Dapper/Npgsql and
  refcursor transaction handling are value-added implementation choices.

## Context

COINK explicitly requires database queries through Stored Procedures and prefers
PostgreSQL. PostgreSQL procedures do not directly return row sets like functions;
refcursors exist only inside a transaction.

## Decision

Normal application reads and writes call schema-qualified `app.*` procedures
through parameterized Dapper commands over Npgsql. A small executor opens one
connection and transaction, calls the procedure, fetches its refcursor, materializes
rows, and commits. Repositories map rows and only translate known SQLSTATEs;
unexpected failures propagate to centralized handling.

## Consequences

The database contract stays explicit and testable against real PostgreSQL. No ORM
or inline repository CRUD SQL exists. Each request currently pays one short
transaction and cursor round trip; this is acceptable for assessment scale and
should be measured before considering a different procedure result convention.
