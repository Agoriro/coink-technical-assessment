# COINK Technical Assessment

Production-minded implementation of COINK's user-registration assessment. The
repository combines a layered .NET 10 API, PostgreSQL Stored Procedures, a fixed
Colombia DIVIPOLA catalog, a small Next.js management UI, automated tests, and
CI quality gates.

## Quick start

### Prerequisites

- Docker Desktop using Linux containers and Docker Compose v2.
- Git. .NET 10 SDK and Node.js 24 are needed only for running quality checks
  outside containers.
- Available loopback ports `3000`, `8080`, and `5432`, or custom ports in `.env`.

Copy [`.env.example`](.env.example) to `.env`, replace
`replace_with_a_local_password` with a local alphanumeric password, then start
the complete stack from the repository root:

```powershell
Copy-Item .env.example .env
docker compose up --build
```

This one Compose command builds and starts `postgres`, `api`, and `web`; waits
for meaningful dependency health checks; and initializes the database from the
versioned repository scripts. Startup never downloads reference data.

| Service    | Default local URL               | Purpose                               |
| ---------- | ------------------------------- | ------------------------------------- |
| Web        | <http://localhost:3000>         | User-management interface             |
| API        | <http://localhost:8080>         | Versioned JSON API                    |
| Swagger UI | <http://localhost:8080/swagger> | Interactive development documentation |
| PostgreSQL | `localhost:5432`                | Local database connection             |

Published ports bind to loopback only. Swagger is intentionally enabled only
when the API runs in `Development`, as configured by local Compose.

Stop containers while preserving local database data:

```powershell
docker compose down
```

## Configuration

Do not commit `.env`; it is ignored by Git. Repository examples contain no real
secrets.

| Variable                      | Default/example         | Consumer       | Meaning                                      |
| ----------------------------- | ----------------------- | -------------- | -------------------------------------------- |
| `POSTGRES_PASSWORD`           | required placeholder    | Compose        | Local PostgreSQL and API credential          |
| `POSTGRES_PORT`               | `5432`                  | Compose        | Host-only PostgreSQL port                    |
| `API_PORT`                    | `8080`                  | Compose        | Host-only API port and browser API URL       |
| `WEB_PORT`                    | `3000`                  | Compose        | Host-only UI port and allowed CORS origin    |
| `ConnectionStrings__Postgres` | environment-specific    | API            | PostgreSQL connection string outside Compose |
| `Cors__AllowedOrigins__0`     | `http://localhost:3000` | API            | Explicit browser origin outside Compose      |
| `API_BASE_URL`                | `http://localhost:8080` | Next.js server | Server-side API URL                          |
| `NEXT_PUBLIC_API_BASE_URL`    | `http://localhost:8080` | Browser        | Public API URL embedded at build time        |
| `E2E_BASE_URL`                | `http://localhost:3000` | Playwright     | UI under test                                |
| `E2E_API_BASE_URL`            | `http://localhost:8080` | Playwright     | API used for test setup                      |

The API defaults to a limit of 100 requests per 60-second fixed window. Change
`RateLimiting__PermitLimit` and `RateLimiting__WindowSeconds` through trusted
environment configuration when needed.

## Architecture

The solution is a pragmatic modular monolith. Dependencies point inward and
each layer owns one concern:

```text
HTTP / UI
   |
Coink.Api ---------------- composition, transport, OpenAPI, middleware
   |
Coink.Application -------- use cases, validation, contracts, result mapping
   |
Coink.Domain ------------- framework-independent domain boundary
   ^
Coink.Infrastructure ----- Dapper/Npgsql repositories
   |
PostgreSQL --------------- constraints, immutable geography, Stored Procedures
```

All normal application database reads and writes call schema-qualified
PostgreSQL Stored Procedures through Dapper/Npgsql. Composite foreign keys
enforce the complete country-department-municipality relationship atomically.
Expected validation, not-found, and geography failures become safe Problem
Details responses; unexpected errors are logged once at the API boundary.

Key decisions and tradeoffs are recorded in [architecture decision records](docs/adr/README.md).
The fixed geographic dataset provenance, checksum, regeneration, and versioning
caveat are documented in [the seed notes](database/seed/README.md).

## API usage

Routes are rooted at `/api/v1`:

| Method   | Route                                        | Behavior                                     |
| -------- | -------------------------------------------- | -------------------------------------------- |
| `POST`   | `/users`                                     | Register user; returns `201` and `Location`  |
| `GET`    | `/users?page=1&pageSize=20&search=`          | Bounded deterministic list/search            |
| `GET`    | `/users/{id}`                                | Read one user                                |
| `PUT`    | `/users/{id}`                                | Replace editable user fields                 |
| `DELETE` | `/users/{id}`                                | Permanently delete one user                  |
| `GET`    | `/countries`                                 | Read country catalog                         |
| `GET`    | `/countries/{countryId}/departments`         | Read departments for country                 |
| `GET`    | `/departments/{departmentId}/municipalities` | Read municipality-level units for department |

Example registration using the seeded Colombia, Antioquia, and Medellín IDs:

```powershell
$body = @{
  name = 'Ada Lovelace'
  phone = '+573001234567'
  countryId = 170
  departmentId = 5
  municipalityId = 5001
  address = 'Calle 10 # 20-30'
} | ConvertTo-Json

Invoke-RestMethod `
  -Method Post `
  -Uri 'http://localhost:8080/api/v1/users' `
  -ContentType 'application/json' `
  -Body $body
```

Names and addresses must be nonblank and no longer than 150 and 250 characters
respectively, without surrounding whitespace. Phones accept 7-15 digits with
an optional leading `+`. Pagination is one-based, `pageSize` is 1-100, and
`search` is an optional case-insensitive name/phone fragment up to 100
characters. Phone uniqueness is not an assessment business rule and is not
invented here.

Errors use RFC 9457-compatible `application/problem+json`, stable error codes,
field errors where applicable, and a trace identifier. Expected status codes
are `200`, `201`, `204`, `400`, `404`, `429`, and `500` for unexpected failures.

Use [Swagger UI](http://localhost:8080/swagger) while the local stack runs, or
import the [Postman collection](docs/postman/COINK.postman_collection.json) and
[local environment](docs/postman/COINK.local.postman_environment.json).

## UI usage

Open <http://localhost:3000>. The UI supports listing, literal name/phone
search, pagination, registration, detail, editing, and confirmed permanent
deletion. Country, department, and municipality selectors load dependently so
users cannot accidentally keep a stale child selection. Client-side Zod checks
provide fast feedback; API and database validation remain authoritative.

## Database lifecycle

Initialization order is deterministic:

1. relational schema and indexes;
2. checked-in Colombia seed;
3. geography Stored Procedures;
4. user Stored Procedures.

The Compose volume preserves records across normal stops. To recreate local
data from scratch, run the following explicitly destructive local command:

```powershell
docker compose down --volumes
docker compose up --build
```

This removes the Compose `postgres_data` volume and all users stored there.
Repository files are not deleted. SQL-only initialization and transactional
verification entry points live under [`database/init`](database/init).

## Tests and quality gates

Backend unit tests and integration tests are part of the solution. Integration
tests start a disposable real PostgreSQL 16 container and load production-
equivalent schema, procedures, and seed scripts.

```powershell
dotnet restore COINK.slnx
dotnet format COINK.slnx --no-restore --verify-no-changes
dotnet build COINK.slnx --configuration Release --no-restore
dotnet test COINK.slnx --configuration Release --no-build
```

Frontend checks:

```powershell
npm.cmd --prefix src/web ci
npm.cmd --prefix src/web run format:check
npm.cmd --prefix src/web run lint
npm.cmd --prefix src/web run typecheck
npm.cmd --prefix src/web test
npm.cmd --prefix src/web run build
```

Browser E2E checks require the complete default-port stack to be healthy:

```powershell
npm.cmd --prefix tests/e2e ci
npm.cmd --prefix tests/e2e run typecheck
npx.cmd --prefix tests/e2e playwright install --with-deps
npm.cmd --prefix tests/e2e test
```

Set `E2E_BASE_URL` and `E2E_API_BASE_URL` for non-default ports. Playwright uses
one worker to keep shared-database journeys deterministic and retains failure
diagnostics. GitHub Actions runs these backend, frontend, Compose, and Playwright
gates on pushes and pull requests for `develop` and `main`; deployment remains
outside current scope.

## Troubleshooting

- **Compose says `POSTGRES_PASSWORD` is missing:** create `.env` from
  `.env.example` and replace the placeholder.
- **A default port is busy:** set unused `POSTGRES_PORT`, `API_PORT`, and
  `WEB_PORT` values in `.env`, rebuild, and use matching URLs.
- **API waits or reports database connection errors:** inspect
  `docker compose ps` and `docker compose logs postgres api`; API starts only
  after PostgreSQL health succeeds.
- **Swagger returns 404:** expected outside `Development`; local Compose sets
  `Development` explicitly.
- **Browser calls fail CORS:** use the same `localhost` host and configured web
  port as `Cors__AllowedOrigins__0`; `localhost` and `127.0.0.1` are different
  origins.
- **Changed SQL does not run against an old volume:** initialization scripts run
  only for a fresh PostgreSQL data directory. Use the documented destructive
  local reset after preserving any data you need.
- **Playwright browser is absent:** run
  `npx.cmd --prefix tests/e2e playwright install --with-deps`.

## Reviewer guide

- [Assessment compliance matrix](docs/compliance/assessment-matrix.md): faithful
  COINK wording, implementation, evidence, and explicit value-added scope.
- [Final security and quality review](docs/compliance/security-review.md):
  controls checked, residual limitations, and reproducible evidence.
- [Postman assets](docs/postman/README.md): import order and safe local variables.
- [ADRs](docs/adr/README.md): architecture, Stored Procedure access, geography,
  and proportional value-added decisions.
- [`PROJECT_STATE.md`](PROJECT_STATE.md): phase and commit evidence.

## Future Improvements / TODO

Everything below is potential evolution beyond this assessment, not a skipped
COINK requirement, current defect, or incomplete deliverable. Add only after a
measured or concrete need:

- Add authentication and authorization with login, short-lived JWTs, refresh
  token rotation, roles, and route/resource protection after identity and access
  policies are defined.
- Add user lifecycle and audit capabilities such as status, soft deletion,
  reactivation, actor attribution, and immutable change history when retention
  and compliance requirements exist.
- Add OpenTelemetry logs, metrics, traces, dashboards, service-level objectives,
  and alerts after operational targets and telemetry destinations are chosen.
- Define development, staging, and production environments with reviewed
  promotion, approval, rollback, and release policies.
- Move secrets to a managed store with least-privilege access, rotation,
  expiration, and incident procedures.
- Version geographic catalogs and support multiple countries through reviewed,
  traceable imports when product scope expands beyond the fixed Colombia
  snapshot.
- Evolve API contracts only from client needs: consider `PATCH`, advanced
  filters/sorting, a deprecation and retirement policy, and idempotency keys for
  operations where retries create a demonstrated risk.
- Add load, stress, soak, baseline, and capacity tests before setting scaling or
  performance guarantees.
- Add Dependabot or equivalent update automation, vulnerability policy gates,
  SAST, secret scanning, container scanning, and SBOM publication.
- Introduce caching only after profiling identifies a useful target and defines
  invalidation; Redis requires measured latency or load evidence.
- Introduce background or event processing only for a concrete business flow;
  message brokers require delivery, retry, ordering, or decoupling needs that
  synchronous processing cannot meet.
- Add Infrastructure as Code after a target hosting platform and environment
  model are selected. Distributed architecture likewise requires measured scale,
  team-boundary, resilience, or deployment needs before introduction.
