# Final security and quality review

Review date: 2026-08-24. Scope: repository baseline for a local technical
assessment, not a production authorization or penetration-test report.

## Security review

| Area                            | Evidence reviewed                                                                                                                                     | Result           |
| ------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------- |
| Secrets and configuration       | `.env` ignored; examples use placeholders; Compose requires injected password; CI creates and masks a disposable credential                           | Pass             |
| SQL injection and data boundary | Repositories use Dapper parameters for every external value; normal operations call only fixed `app.*` procedures; search is literal, not dynamic SQL | Pass             |
| Relational integrity            | Composite foreign key validates complete geography; check constraints cover formats/nonblank values; reference rows are application read-only         | Pass             |
| Input validation                | Kestrel body limit is 1 MiB; FluentValidation bounds text, phone, identifiers, pagination, and search; Zod is UX only                                 | Pass             |
| Error disclosure                | Expected outcomes map to Problem Details with stable code and trace ID; unexpected errors log once without SQL/connection/stack details in response   | Pass             |
| Browser boundary                | Explicit CORS origins; CSP and defensive security headers; no wildcard origin; client renders values as React text                                    | Pass             |
| Abuse resistance                | Native fixed-window ASP.NET Core rate limiting returns `429` with `Retry-After`; list queries are bounded                                             | Pass             |
| API exposure                    | Swagger is development-only; production runtime check returned safe `404`; published Compose ports bind only to loopback                              | Pass             |
| Containers                      | Runtime images are digest-pinned and run dedicated non-root API/web users; build stages keep SDK/tooling out of runtime images                        | Pass             |
| Dependency posture              | Deterministic .NET central versions and npm lockfiles; current npm audits report zero known vulnerabilities; CI uses minimal read-only permissions    | Pass with caveat |
| Authentication                  | Explicitly forbidden current scope; no misleading partial authentication exists                                                                       | Out of scope     |

Dependency caveat: ESLint 9 remains pinned because current Next.js React plugins
fail at runtime on ESLint 10. npm reports its upstream support warning, while the
project audit, lint, and build are clean. Upgrade after the plugin ecosystem is
compatible; do not suppress the warning or force an incompatible major.

## Quality and compliance review

- Repository source matches approved API surface: five user operations and three
  geography reads under `/api/v1`.
- Normal database access is exclusively parameterized Stored Procedure calls via
  Dapper/Npgsql; no ORM or inline repository CRUD SQL was found.
- COINK requirements and value-added decisions are separated in the assessment
  matrix and ADR metadata.
- Seed source, SHA-256, row counts, deterministic generator, and catalog aging
  caveat are recorded; normal startup has no public-network dependency.
- Backend formatting and Release build pass with zero warnings/errors.
- Backend suite passes 8 unit and 8 real-PostgreSQL integration tests.
- Frontend formatting, ESLint, strict TypeScript, 6 Vitest tests, production
  build, and npm audit pass.
- E2E strict TypeScript and 3 Playwright Chromium journeys pass against the full
  health-gated Compose stack.
- `actionlint` and Prettier validate CI. Workflow performs deterministic installs,
  includes required Playwright browser/system dependency installation, uploads
  failure evidence, and always destroys its disposable data volume.
- Git diff whitespace validation and final status review are required immediately
  before the Phase 10 commit; commit evidence is recorded in `PROJECT_STATE.md`.

## Known boundaries

- No authentication or authorization: deliberate assessment scope, so do not
  expose this baseline directly to an untrusted network.
- Compose uses `Development` to provide reviewer Swagger; production deployment
  configuration, TLS termination, managed secrets, and environment promotion are
  intentionally not supplied.
- The Colombia catalog is a fixed supplied snapshot, not an automatically current
  DANE feed. Updates require provenance and reviewed version changes.
- Hard delete is deliberate current CRUD behavior; retention and audit policies
  were not provided.
- Local and CI test evidence is comprehensive, but no hosted workflow run can be
  claimed until commits are pushed to GitHub. No push is performed automatically.

These boundaries are disclosed scope choices, not hidden controls. Potential
production evolution is listed only in `README.md` under
`Future Improvements / TODO`.
