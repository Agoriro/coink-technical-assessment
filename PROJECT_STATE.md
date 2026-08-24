# Project State

- Current branch: `develop`
- Last completed phase: Phase 8 - automated test pyramid
- Current status: awaiting explicit approval for Phase 9
- Next phase: CI/CD quality validation
- Assessment source: the supplied PDF/Markdown confirms the registration, relational geography, Stored Procedure, error-handling, Docker, and OpenAPI requirements
- Data source: fixed 1,119-row DIVIPOLA snapshot supplied as `subregiones.pdf`; provenance, checksum, deterministic generation, and catalog-version caveat are documented in `database/seed/README.md`
- Environment note: the in-app browser remains unavailable, but Playwright-managed Chromium now passes the complete reviewer journeys headlessly
- Toolchain caveat: ESLint 9 is pinned because the current Next.js React plugins fail at runtime on ESLint 10; npm flags ESLint 9 as upstream end-of-support, while the final audit remains clean

## Completed phases

- Phase 0: discovery, skills review, operating rules, and phased plan
- Phase 1: solution/SDK policy, centralized .NET settings, repository hygiene, and target directory skeleton
- Phase 2: relational constraints, deterministic Colombia seed, true PostgreSQL procedures with refcursor result contracts, and transactional verification SQL
- Phase 3: layered .NET projects, dependency composition, centralized Problem Details, exception handling, CORS, rate limiting, security headers, request-size limits, and development-only OpenAPI
- Phase 4: versioned country, department, and municipality read endpoints backed exclusively by PostgreSQL Stored Procedures through Dapper/Npgsql
- Phase 5: validated registration and complete user CRUD, bounded pagination/search, stable Problem Details, and correct empty-page metadata
- Phase 6: responsive Next.js user directory with search/pagination, create/detail/edit/delete journeys, dependent Colombia selectors, accessible states, and layered API clients
- Phase 7: digest-pinned multi-stage images, non-root API/web runtimes, deterministic database initialization, health-gated Compose startup, local-only ports, and explicit persistent/reset workflows
- Phase 8: backend unit tests, real-PostgreSQL API/database integration tests, frontend unit/component tests, and Playwright reviewer journeys

## Phase 2 validation evidence

- Seed generator and validator pass Node.js syntax checks.
- Regeneration from the supplied PDF is byte-for-byte deterministic.
- Seed validation confirms 1,119 unique five-digit codes across 33 department prefixes.
- PostgreSQL 16 initialization, repeat initialization, explicit reset, and post-reset initialization pass in Docker.
- Transactional verification passes for geographic reads, hierarchy rejection, and complete user create/get/list/update/delete procedure flow.

## Phase 3 validation evidence

- `dotnet restore COINK.slnx` passes with pinned central package versions.
- `dotnet build COINK.slnx --no-restore` passes with zero warnings and zero errors.
- `dotnet format COINK.slnx --no-restore --verify-no-changes` passes.
- Development runtime smoke checks pass for Swagger JSON/UI, allowed-origin CORS, security headers, RFC-compatible `404`/`429` Problem Details with trace identifiers, and `Retry-After`.
- Production runtime smoke check confirms Swagger is unavailable and the response remains a safe Problem Details `404`.

## Phase 4 validation evidence

- `dotnet build COINK.slnx --no-restore` passes with zero warnings and zero errors.
- `dotnet format COINK.slnx --no-restore --verify-no-changes` passes.
- PostgreSQL 16 runtime checks exercise the repository through the production schema, seed, and Stored Procedures.
- API smoke checks return 1 country, 33 Colombian departments, and the expected 125 municipality-level units for Antioquia with correct parent identifiers and DIVIPOLA codes.
- Repeated reads preserve deterministic payload ordering; missing parents return safe `404` Problem Details and invalid, out-of-range, or malformed identifiers return safe `400` Problem Details.
- OpenAPI exposes exactly the three approved geography paths; the isolated validation container was removed after the checks.

## Phase 5 validation evidence

- `dotnet build COINK.slnx --no-restore` passes with zero warnings and zero errors.
- `dotnet format COINK.slnx --no-restore --verify-no-changes` passes.
- PostgreSQL 16 initialization and transactional database verification pass after the list procedure update; repeat initialization remains idempotent.
- Real API checks cover `201` creation with `Location`, get/list/search/update/delete success, deterministic ordering, and empty-page totals.
- Validation checks cover phone format, pagination bounds, malformed and non-positive identifiers, and full country-department-municipality mismatch rejection.
- Missing get/update/delete operations return safe `404` Problem Details; OpenAPI contains all five user CRUD operations alongside the three geography operations.
- The isolated PostgreSQL validation container and its non-persistent test data were removed after checks.

## Phase 6 validation evidence

- Deterministic `npm ci` dependencies are pinned in `package-lock.json`; `npm audit` reports zero known vulnerabilities.
- Prettier verification, ESLint with zero warnings, strict TypeScript checking, and the optimized Next.js production build pass.
- Real runtime checks use PostgreSQL 16 initialized from repository scripts and the actual ASP.NET Core API.
- Server-rendered list, create, detail, and edit routes return their expected content; API-backed create, update, and delete complete successfully.
- Detail rendering resolves the complete geographic hierarchy; empty, loading, validation, API-error, not-found, and delete-confirmation states are implemented.
- The isolated PostgreSQL container and its non-persistent test data were removed after checks.

## Phase 7 validation evidence

- `docker compose config --quiet` validates the three-service `web`, `api`, and `postgres` model with environment-supplied credentials.
- Digest-pinned .NET, Node.js, and PostgreSQL Alpine images build successfully; API and web runtime processes use dedicated non-root users.
- `docker compose up --build --detach --wait` passes from an empty volume with dependency-gated health checks for all services.
- Initialization produces 1 country, 33 departments, and 1,119 municipality-level records using the canonical repository scripts.
- Real containerized HTTP checks cover server-rendered UI data, complete user CRUD, geographic names, configured CORS, and security headers.
- A normal `docker compose down` and subsequent startup preserve user data; the separately documented `--volumes` reset recreates the catalog from scratch.
- Validation used alternate loopback ports to preserve unrelated running containers; the temporary COINK containers, network, volume, and test user were removed afterward.

## Phase 8 validation evidence

- The solution builds with zero warnings/errors and runs 8 backend unit tests covering application validation, persistence outcome mapping, missing resources, and geography service semantics.
- Eight integration tests start a digest-pinned PostgreSQL 16 container, execute the canonical repository initializer, and cover the complete catalog, CRUD, hierarchy enforcement, constraints, deterministic pagination/search, missing resources, the explicit absence of a duplicate-phone conflict rule, and transaction rollback.
- Six Vitest tests cover Zod boundaries, list empty/search/pagination behavior, dependent geographic loading, registration submission, and safe client errors.
- Three Playwright Chromium journeys cover validation and dependent selectors, complete create/view/edit/search/delete behavior, deterministic pagination/filtering, and loading/error feedback.
- Deterministic `npm ci`, strict frontend/E2E type checking, ESLint, Prettier, Next.js production build, .NET formatting, and package audits pass.
- The E2E stack used alternate loopback web/PostgreSQL ports; its containers, network, database volume, browser test users, and Testcontainers resources were removed after validation.

## Approval boundary

Do not begin Phase 9 until the user explicitly approves it.
