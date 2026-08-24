# COINK Technical Assessment - Repository Operating Rules

## 1. Purpose and scope

This monorepo implements the COINK technical assessment as a professional,
maintainable, secure, testable, and documented solution. The assessed baseline
is a relational user-registration system with Colombian geographic reference
data, a C# API, meaningful validation, PostgreSQL Stored Procedure data access,
consistent HTTP behavior, Docker support, and database scripts.

The repository also includes explicitly approved value-added engineering:
complete user CRUD, a small Next.js user-management UI, API versioning,
pagination/search, the complete Colombia geographic catalog, broader automated
tests, CI/CD, Postman assets, ADRs, and repository-quality tooling.

Always distinguish these categories in documentation and interview material:

- **COINK requirement:** explicitly supported by the source assessment.
- **Value-added decision:** deliberately added for maintainability, security,
  UX, testability, reviewability, or developer experience, with a concise
  rationale.

Never present a value-added feature as an explicit COINK requirement. Do not
invent business rules beyond the assessment or an explicit user approval.

## 2. Approved architecture

Use a pragmatic modular monolith with a layered backend:

```text
API -> Application -> Domain
                       ^
API -> Infrastructure -|
Infrastructure -> Dapper/Npgsql -> PostgreSQL Stored Procedures
```

Dependency direction and responsibilities:

- **API:** HTTP transport, versioned routes, request/response mapping, OpenAPI,
  middleware, rate limiting, and dependency-composition root. No business or
  SQL logic.
- **Application:** use cases/application services, DTOs, validation orchestration,
  repository abstractions, result types where useful, pagination contracts, and
  transaction-independent business flow.
- **Domain:** small, purposeful business concepts and invariants only. It must
  not depend on infrastructure or web frameworks.
- **Infrastructure:** Dapper/Npgsql repository implementations, Stored Procedure
  invocation, database-specific mapping, and external technical concerns.
- **Database:** relational integrity, schema, indexes, immutable geographic
  reference data, deterministic seed data, and documented Stored Procedures.

Use Dependency Injection, Repository, Application Service/Use Case, DTO, and a
lightweight Result pattern where they solve a concrete concern. Do not add CQRS,
MediatR, Event Sourcing, microservices, generic repositories, artificial Unit of
Work abstractions, unnecessary factories, or distributed infrastructure without
a demonstrated need and explicit approval.

## 3. Target repository structure

```text
/
|-- AGENTS.md
|-- README.md
|-- PROJECT_STATE.md
|-- COINK.slnx
|-- Directory.Build.props
|-- Directory.Packages.props
|-- compose.yaml
|-- .editorconfig
|-- .gitattributes
|-- .gitignore
|-- .dockerignore
|-- src/
|   |-- backend/
|   |   |-- Coink.Api/
|   |   |-- Coink.Application/
|   |   |-- Coink.Domain/
|   |   `-- Coink.Infrastructure/
|   `-- web/
|-- tests/
|   |-- Coink.UnitTests/
|   |-- Coink.IntegrationTests/
|   `-- e2e/
|-- database/
|   |-- init/
|   |-- schema/
|   |-- procedures/
|   `-- seed/
|-- docs/
|   |-- adr/
|   |-- postman/
|   `-- compliance/
`-- .github/workflows/
```

Adjust a path only when the established project tooling requires it; preserve
the layer boundaries and keep one obvious location for each concern.

## 4. Technology stack

- .NET 10 LTS, ASP.NET Core Web API, and C# with nullable reference types.
- FluentValidation for request/application validation.
- Dapper and Npgsql for all normal application database access.
- PostgreSQL with Stored Procedures for normal application operations.
- Swagger/OpenAPI and API routes rooted under `/api/v1`.
- Next.js, React, TypeScript in strict mode, and Tailwind CSS.
- React Hook Form and Zod for frontend form state and UX validation.
- xUnit-compatible backend unit/integration tests; use real PostgreSQL for
  database integration coverage.
- Frontend unit/component tests where they add useful confidence.
- Playwright for browser E2E tests.
- Dockerfiles and Docker Compose services named `web`, `api`, and `postgres`.
- GitHub Actions for continuous integration.

Pin reproducible SDK/package versions using the native lock/version mechanisms.
Introduce a dependency only when it has a clear benefit and no adequate native
capability already exists.

## 5. Coding and naming conventions

### General

- Prefer clear, small, cohesive units and intention-revealing names.
- Keep business behavior independent of HTTP, SQL, and UI frameworks.
- Avoid duplication, speculative abstractions, magic values, and hidden global
  state. Extract shared code only after a real repeated concept exists.
- Comments explain intent, constraints, tradeoffs, or non-obvious behavior; they
  must not narrate obvious syntax.
- Use UTF-8, LF line endings, final newlines, and repository `.editorconfig`.
- Treat compiler, formatter, linter, and static-analysis warnings as actionable.

### C#

- Use current idiomatic C# supported by .NET 10, file-scoped namespaces, nullable
  annotations, async I/O, and cancellation tokens on asynchronous boundaries.
- Use PascalCase for types, methods, properties, constants, and public members;
  camelCase for parameters and locals; `_camelCase` for private fields; prefix
  interfaces with `I`.
- Append `Async` to methods that return `Task`/`ValueTask`, except framework-
  prescribed signatures.
- Prefer records for immutable transport/value data when appropriate. Avoid
  primitive obsession only when a domain type adds meaningful invariants.
- Never use exceptions for expected validation/not-found/conflict flow. Use a
  focused result/error representation where it clarifies application behavior.
- Do not expose database models or domain internals directly as API contracts.

### C# XML documentation

- Add `///` XML documentation to public types and members whose intent, contract,
  side effects, units, constraints, exceptions, or lifecycle are not obvious.
- Public application interfaces and cross-layer contracts require summaries and
  meaningful `<param>`, `<returns>`, and `<exception>` tags when applicable.
- Do not add boilerplate such as "Gets or sets" when the identifier and type are
  self-explanatory. Document why and contract semantics, not syntax.
- Keep documentation accurate when behavior changes; stale comments are defects.

### TypeScript and React

- Enable strict TypeScript. Do not use `any`; prefer `unknown` plus narrowing.
- Use PascalCase for React components and exported types, camelCase for values
  and functions, `useX` for hooks, and descriptive lowercase route/file names
  following Next.js conventions.
- Keep server/client boundaries explicit. Minimize client components and effects;
  colocate UI behavior with the smallest responsible component.
- Separate API contracts/client functions, form schemas, UI components, and page
  composition. Do not duplicate backend validation as a security assumption.
- Ensure interactive elements are keyboard-accessible, labeled, and expose useful
  loading, disabled, validation, empty, and error states.

### TSDoc/JSDoc

- Document exported functions, hooks, and types when behavior, error semantics,
  side effects, invariants, or parameter meaning is not self-evident.
- Use TSDoc/JSDoc for reusable public contracts, including `@param`, `@returns`,
  and `@throws` only when they add information.
- Do not annotate obvious components, trivial accessors, or code already made
  clear by strong types and naming.

### SQL and Stored Procedures

- Use `snake_case` for schemas, tables, columns, indexes, constraints, parameters,
  and procedure names. Qualify database objects with their schema.
- Use plural table names consistently and explicit column lists; never rely on
  `SELECT *` in application procedures.
- Each Stored Procedure must include a nearby SQL comment block describing its
  purpose, parameters, returned columns/result shape, business/relational
  validation, expected error behavior, and concurrency or ordering assumptions.
- Comments must explain contract and non-obvious choices, not restate each SQL
  clause. Keep procedures focused and parameterized.

## 6. API conventions

- Expose only these approved routes unless a later phase is explicitly approved:
  `POST/GET /api/v1/users`, `GET/PUT/DELETE /api/v1/users/{id}`,
  `GET /api/v1/countries`,
  `GET /api/v1/countries/{countryId}/departments`, and
  `GET /api/v1/departments/{departmentId}/municipalities`.
- `GET /api/v1/users` supports bounded pagination, basic search/filtering, and a
  deterministic tie-broken order. It is not a generic query API.
- Use JSON consistently, explicit request/response DTOs, and stable property
  naming. Do not leak internal exception, SQL, connection, or stack details.
- Return status codes consistently: `200` reads/updates, `201` creation with a
  `Location`, `204` deletion, `400` malformed/validation requests, `404` missing
  resources, `409` true conflicts, `429` rate-limit rejection, and `500` only for
  unexpected failures. Refine only when the API contract justifies it.
- Represent errors with RFC 9457 Problem Details-compatible responses, including
  validation details and a correlation/trace identifier where useful.
- Keep OpenAPI accurate and verify representative success/error responses.

## 7. Database and data-access conventions

- Normal application reads and writes must call PostgreSQL Stored Procedures via
  Dapper + Npgsql. Do not add an ORM or inline repository CRUD SQL.
- Use parameters for every external value. Never concatenate values into SQL.
- Database initialization must be deterministic, ordered, repeatable, and fail
  visibly. Do not download data during normal container startup.
- Use primary/foreign keys, uniqueness, check constraints, appropriate nullability,
  and indexes justified by queries. Prefer database-enforced integrity over
  application-only assumptions.
- Treat country, department, and municipality data as read-only reference data.
  Seed a complete, traceable Colombia dataset in-repository.
- Validate the complete country -> department -> municipality relationship, not
  merely the independent existence of each identifier. Enforce the relationship
  in database constraints/procedures and provide clear application errors.
- Procedures must define deterministic ordering and pagination behavior. Avoid
  unbounded list queries and unstable offset results.
- Repository code owns database mapping and translates known PostgreSQL outcomes
  into application errors without hiding unexpected failures.
- Use transactions only around a concrete atomic operation; do not add an
  artificial Unit of Work abstraction.

## 8. Validation strategy

Apply complementary validation at each boundary:

- **Frontend/Zod:** fast UX feedback and dependent selector consistency; never a
  security boundary.
- **API/Application/FluentValidation:** required fields, formats, lengths, allowed
  ranges, and request-level rules with stable, useful messages.
- **Database:** relational integrity, uniqueness, required data, and complete
  geographic hierarchy validation.

Normalize only when the business meaning is unambiguous. Reject silently truncated
or ambiguous data. Test boundary values and cross-field/geographic failures.

## 9. Security rules

- Do not implement login, JWT, roles, permissions, or an authentication system
  unless the user explicitly approves a future scope change.
- Never commit secrets, credentials, connection strings containing passwords,
  production endpoints, local `.env` files, or generated secret material. Supply
  safe examples and environment-variable configuration.
- Use ASP.NET Core's native rate limiting
  (`Microsoft.AspNetCore.RateLimiting`/built-in middleware). Do not add a
  third-party rate-limiting library without a concrete unmet requirement and
  explicit approval.
- Prefer native ASP.NET Core configuration/middleware or a small project-owned
  middleware for security headers. Do not add a convenience-only security-header
  dependency.
- Validate all untrusted inputs, parameterize SQL, constrain request/body sizes,
  configure only required CORS origins, and avoid sensitive data in logs/errors.
- Use HTTPS-aware production settings, least-privilege database credentials,
  non-root containers where practical, and pinned/minimal runtime images.
- Do not expose Swagger in production unless explicitly and safely configured.
- Review dependencies, container inputs, logs, OpenAPI exposure, error bodies,
  rate-limit behavior, and injection risks before final completion.

## 10. Error handling and logging

- Use centralized ASP.NET Core exception handling and Problem Details mapping.
- Distinguish validation, not found, conflict, transient infrastructure, and
  unexpected failures. Do not blanket-catch exceptions or return success for an
  unsuccessful operation.
- Preserve cancellation. Log unexpected failures once at the handling boundary
  with structured properties and trace context; avoid duplicate noisy logging.
- Never log secrets, full connection strings, or unnecessary personal data such
  as full phone/address payloads.
- The frontend presents safe, actionable messages, preserves recoverable form
  state, and handles network/unexpected responses without exposing internals.

## 11. Testing strategy

- Unit-test meaningful domain/application behavior, validators, mappings, result
  translation, and frontend logic/components. Avoid tests that only repeat
  framework implementation.
- Run backend integration tests against a real PostgreSQL instance initialized
  from production-equivalent schema, procedures, and seed scripts. Cover CRUD,
  geographic hierarchy enforcement, constraints, pagination, search, ordering,
  not-found/conflict behavior, and rollback/isolation expectations.
- Use Playwright for critical reviewer journeys: list/search/paginate, create with
  dependent geography, view, edit, delete confirmation, validation, and useful
  error/empty/loading behavior.
- Keep tests deterministic and isolated. Do not depend on execution order,
  uncontrolled clocks, public networks, or mutable shared data.
- Do not optimize for artificial 100% coverage. Use coverage to find consequential
  gaps and require regression tests for fixed defects.
- CI Playwright setup must include exactly the functional sequence:
  `npm ci`, `npx playwright install --with-deps`, `npx playwright test` (with paths
  or scripts added only as necessary). Use conservative CI workers and upload
  useful reports/traces/screenshots on failure.

## 12. Formatting, linting, and static analysis

- C#: `dotnet format --verify-no-changes`, build with warnings enabled, nullable
  analysis enabled, and analyzers configured centrally. Do not suppress warnings
  without a documented reason.
- TypeScript: Next.js/ESLint checks, `tsc --noEmit`, unit tests, and the repository
  formatter. Keep generated outputs out of source lint inputs.
- SQL: consistent repository formatting, lower-case identifiers, explicit clauses,
  and reviewable migration/init ordering. Validate scripts against real PostgreSQL.
- Run the smallest relevant checks during iteration and all affected quality gates
  before a phase is complete.

## 13. Git conventions

- Work on the current approved branch and preserve unrelated user changes.
- Inspect `git status` before work and `git diff --check`, `git diff`, and
  `git status` before each phase commit.
- Each approved phase ends in exactly one coherent repository commit unless the
  user approves another strategy. Never commit the next phase early.
- Use Conventional Commits: `type(optional-scope): imperative summary`, e.g.
  `chore(repo): establish monorepo foundation`.
- Typical types are `feat`, `fix`, `test`, `docs`, `chore`, `ci`, and `refactor`.
- Do not amend, force-push, rewrite history, delete branches, or push remotely
  unless explicitly requested. Never commit secrets or generated build outputs.

## 14. Docker and local development conventions

- The complete solution must start through Docker Compose with `web`, `api`, and
  `postgres` services and one prominently documented command.
- Use multi-stage, cache-friendly Dockerfiles; copy dependency manifests before
  source; run as non-root where supported; keep runtime images minimal; define
  health checks; and add only necessary build context via `.dockerignore`.
- Compose startup must wait on meaningful health/readiness conditions where
  dependencies require them. Do not encode secrets in images or Compose files.
- Database initialization must be repeatable from repository scripts. A reset is
  explicitly destructive to local container data and must be a separate, clearly
  documented command.

## 15. CI/CD conventions

- GitHub Actions runs restore/install, formatting/lint/static checks, backend unit
  and real-PostgreSQL integration tests, frontend tests/build, and Playwright E2E.
- Pin major action versions, grant minimal permissions, use deterministic installs
  (`dotnet restore`, `npm ci`), cache only safe reproducible inputs, and avoid
  secrets on untrusted code paths.
- For Playwright, always run `npx playwright install --with-deps` before tests.
  Favor reliability over maximum parallelism and publish diagnostic artifacts on
  failures.
- CI must not silently pass skipped required tests. Keep job names and failure
  output useful to reviewers.
- Automated deployment is outside the current assessment scope unless explicitly
  approved; CI validates build/test quality only.

## 16. Documentation requirements

- README must give prerequisites, one-command startup, configuration, architecture,
  API/UI usage, database reset, tests/quality commands, troubleshooting, and links
  to OpenAPI, Postman, ADRs, and compliance material.
- Maintain concise ADRs for consequential decisions and their context/tradeoffs.
- Provide a Postman collection/environment without real secrets.
- Final compliance documentation must provide an interview-ready matrix:
  COINK requirement, faithful wording, implementation location, validation/test
  evidence, and status; plus value-added capability, rationale, benefit, and why
  it is proportionate rather than overengineering.
- `PROJECT_STATE.md`, when present, stays concise: current phase, completed phases,
  blockers, validation/commit evidence, and the next approval boundary.
- Documentation comments prioritize useful intent and contracts over narration.
- The final README must end with `Future Improvements / TODO` and explicitly frame
  all entries as potential evolution beyond the current assessment, never as
  skipped requirements, defects, or unfinished current work.
- That roadmap must cover: authentication/authorization; user lifecycle and audit;
  observability; development/staging/production promotion; managed secrets and
  rotation; multi-country/versioned catalogs; API evolution (PATCH, advanced
  filters/sorting, retirement policy, justified idempotency); load/stress/capacity
  testing; dependency/security automation; profiling-justified caching; business-
  justified background/event processing; and Infrastructure as Code. Explicitly
  state that Redis, brokers, and distributed architecture require measured or
  concrete future needs before introduction.

## 17. Forbidden overengineering

Do not add features or infrastructure because they are fashionable. Specifically,
without explicit approval do not add authentication, soft delete, audit systems,
caching, message brokers, background jobs, Kubernetes, cloud deployment, IaC,
multi-country import pipelines, generic filter/query languages, multiple API
versions, service meshes, custom frameworks, or alternate databases.

Prefer the smallest design that fully satisfies the approved requirements and is
easy for an assessor to understand, run, test, and discuss.

## 18. Phase approval and STOP protocol

Work in controlled phases:

1. Inspect current repository state and the approved phase.
2. Present/refine a concise phase plan when needed.
3. **STOP until explicit approval** (`OK`, `Aprobado`, `Continua`, `Proceed`, or
   an unambiguous equivalent).
4. Implement only the approved phase.
5. Run relevant validation/tests.
6. Review `git diff --check`, full diff, and status; fix discovered issues.
7. Create the phase's Conventional Commit.
8. Report implemented work, validation/tests, commit hash/message, and limitations.
9. **STOP and wait for approval of the next phase. Never continue automatically.**

If a requirement or architecture decision is materially ambiguous, ask before
implementing it. Model or agent changes never authorize skipping this protocol.

## 19. Definition of done

A phase is complete only when:

- its approved scope and acceptance behavior are implemented, with no unrelated
  scope creep;
- affected validation, tests, builds, formatters, linters, and static analysis pass;
- security, error handling, accessibility, and documentation are addressed in
  proportion to the phase;
- database changes are deterministic and integration-tested where applicable;
- no secret, generated artifact, debug code, or unexplained warning is introduced;
- the full diff and status have been reviewed and `git diff --check` passes;
- relevant README/ADR/PROJECT_STATE/compliance evidence is updated;
- the phase is committed with a meaningful Conventional Commit; and
- limitations or blocked validations are explicitly reported.

The project is finally done only when the clone-to-running workflow works, all
required deliverables exist, the final requirement classification is accurate,
and the complete quality/security/compliance review passes.

## 20. Canonical commands

These are target commands; establish the referenced scripts during the relevant
approved foundation/testing phases. On Windows PowerShell, use `npm.cmd` and
`npx.cmd` when execution policy blocks the PowerShell shims.

```powershell
# Restore and build backend
dotnet restore COINK.slnx
dotnet build COINK.slnx --no-restore

# Backend tests
dotnet test COINK.slnx --no-build

# C# formatting
dotnet format COINK.slnx
dotnet format COINK.slnx --verify-no-changes

# Frontend install, development, lint, typecheck, test, build, and format
npm.cmd --prefix src/web ci
npm.cmd --prefix src/web run dev
npm.cmd --prefix src/web run lint
npm.cmd --prefix src/web run typecheck
npm.cmd --prefix src/web test
npm.cmd --prefix src/web run build
npm.cmd --prefix src/web run format
npm.cmd --prefix src/web run format:check

# E2E
npm.cmd --prefix tests/e2e ci
npx.cmd --prefix tests/e2e playwright install --with-deps
npx.cmd --prefix tests/e2e playwright test

# Complete local stack
docker compose up --build
docker compose down

# Explicit local database reset (destructive to the Compose postgres volume)
docker compose down --volumes
docker compose up --build

# Inspect OpenAPI after the API is healthy
Start-Process 'http://localhost:8080/swagger'
```

Prefer repository scripts that compose these primitives without hiding failures.
Update this section whenever the actual stable command surface changes.
