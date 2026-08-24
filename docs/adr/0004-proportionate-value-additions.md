# ADR 0004: Proportionate reviewer-facing additions

- Status: Accepted
- Date: 2026-08-24
- Classification: Value-added decision.

## Context

COINK explicitly asks for registration. Reviewability and maintainability benefit
from demonstrating the complete lifecycle and validating the solution through
real consumer journeys, but unnecessary platform scope would obscure the task.

## Decision

Add bounded list/search, read, update, and delete endpoints; a small Next.js UI;
versioned `/api/v1` routes; complete automated unit, real-PostgreSQL integration,
component, and Playwright tests; Docker Compose; CI; Postman; ADRs; and compliance
evidence. Keep one API version and one deterministic query surface.

Do not add authentication, soft delete, auditing, caching, brokers, background
jobs, cloud deployment, IaC, or generic filter languages without future approval
and a concrete need.

## Consequences

Reviewers can exercise a realistic workflow and see failure behavior without
inflating runtime architecture. Added code and test maintenance are deliberate
costs. These capabilities must never be presented as explicit COINK requirements.
