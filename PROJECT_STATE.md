# Project State

- Current branch: `develop`
- Last completed phase: Phase 4 - versioned Colombian geography read endpoints
- Current status: awaiting explicit approval for Phase 5
- Next phase: user registration and complete user CRUD API
- Assessment source: the supplied PDF/Markdown confirms the registration, relational geography, Stored Procedure, error-handling, Docker, and OpenAPI requirements
- Data source: fixed 1,119-row DIVIPOLA snapshot supplied as `subregiones.pdf`; provenance, checksum, deterministic generation, and catalog-version caveat are documented in `database/seed/README.md`
- Environment limitation: none for the completed phases; Docker-backed PostgreSQL and local .NET runtime checks pass

## Completed phases

- Phase 0: discovery, skills review, operating rules, and phased plan
- Phase 1: solution/SDK policy, centralized .NET settings, repository hygiene, and target directory skeleton
- Phase 2: relational constraints, deterministic Colombia seed, true PostgreSQL procedures with refcursor result contracts, and transactional verification SQL
- Phase 3: layered .NET projects, dependency composition, centralized Problem Details, exception handling, CORS, rate limiting, security headers, request-size limits, and development-only OpenAPI
- Phase 4: versioned country, department, and municipality read endpoints backed exclusively by PostgreSQL Stored Procedures through Dapper/Npgsql

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

## Approval boundary

Do not begin Phase 5 until the user explicitly approves it.
