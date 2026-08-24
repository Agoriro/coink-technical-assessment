# COINK Technical Assessment

Professional implementation of the COINK user-registration assessment with a
layered .NET API, PostgreSQL Stored Procedures, Colombia geographic reference
data, and a Next.js management interface.

## Run with Docker

Prerequisite: Docker Desktop with Linux containers.

1. Copy [`.env.example`](.env.example) to `.env` and replace the local database
   password placeholder. The `.env` file is ignored by Git.
2. Build and start the complete stack:

   ```powershell
   docker compose up --build
   ```

The UI is available at `http://localhost:3000`, the API at
`http://localhost:8080`, Swagger at `http://localhost:8080/swagger`, and
PostgreSQL on local port `5432`. All published ports bind only to loopback.

Optional `WEB_PORT`, `API_PORT`, and `POSTGRES_PORT` values in `.env` avoid
local port conflicts.

Stop containers while preserving local database data:

```powershell
docker compose down
```

Reset the local database. This permanently deletes the Compose PostgreSQL
volume before rebuilding the stack:

```powershell
docker compose down --volumes
docker compose up --build
```

## Automated tests

Backend unit tests and real-PostgreSQL integration tests are part of the
solution. The integration fixture starts an ephemeral PostgreSQL 16 container,
loads `database/init/001_initialize.sql` in its canonical include order, and
removes the container after the run:

```powershell
dotnet restore COINK.slnx
dotnet build COINK.slnx --no-restore
dotnet test COINK.slnx --no-build
```

Run the frontend unit/component suite:

```powershell
npm.cmd --prefix src/web ci
npm.cmd --prefix src/web test
```

Browser E2E tests require the complete stack to be running. With the default
ports, install the Playwright browser once and execute Chromium headlessly:

```powershell
npm.cmd --prefix tests/e2e ci
npx.cmd --prefix tests/e2e playwright install --with-deps chromium
npx.cmd --prefix tests/e2e playwright test --config tests/e2e/playwright.config.ts
```

Set `E2E_BASE_URL` and `E2E_API_BASE_URL` when Compose uses non-default web or
API ports. Playwright uses one worker for deterministic shared-database flows
and keeps traces, screenshots, and videos only as configured for failure
diagnosis; generated reports are ignored by Git.

## Continuous integration

GitHub Actions runs on pushes and pull requests targeting `develop` or `main`,
and can also be started manually. The workflow enforces three ordered quality
gates:

- .NET restore, formatting, release build, unit tests, and integration tests
  against an ephemeral real PostgreSQL instance;
- deterministic frontend install, formatting, lint, strict type checking, unit
  and component tests, and a production build;
- complete Docker Compose image build/start followed by the Playwright Chromium
  journeys.

The E2E gate uses the required Playwright browser/system-dependency installation,
generates a masked disposable database credential, runs with one worker, uploads
reports and failure evidence for seven days, and always removes its disposable
containers and database volume. This workflow is validation-only; automated
deployment is intentionally outside the assessment scope.

Expanded architecture, API, testing, troubleshooting, decision, and compliance
documentation will be consolidated in the final documentation phase.
