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

Expanded architecture, API, testing, troubleshooting, decision, and compliance
documentation will be consolidated in the final documentation phase.
