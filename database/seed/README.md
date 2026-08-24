# Colombia geographic seed

The checked-in SQL seed contains one country, 33 department-level divisions,
and 1,119 municipality-level territorial units identified by five-digit
DIVIPOLA codes. The `municipalities` table name follows the assessment's API
language; the source catalog also includes municipality-equivalent territorial
units used by DIVIPOLA.

## Provenance

- Supplied source: `Tabla de Municipios.xls`, exported as `subregiones.pdf` and
  provided with the assessment material on 2026-08-23.
- Source SHA-256:
  `eeff1128f15a9f723e34adc001dd6c4c18681600a623600e6c8487dd3b8898fd`.
- Source total: 1,119 municipality-level rows.
- Derived artifact: `001_colombia.sql`.
- Reproduction tool: `generate-colombia-seed.mjs`, using only Node.js built-ins.

The DANE Geoportal's
[DIVIPOLA download page](https://geoportal.dane.gov.co/servicios/descarga-y-metadatos/descarga-divipola/)
and its published 2025 feature-service schema were consulted independently to
confirm the five-digit concatenated-code semantics. Their live catalog is not
silently merged into this fixed assessment snapshot.

The accompanying converted `subregiones.md` was used as a human-readable
cross-check, not as the generation input: its table conversion splits several
codes and names. The generator reads the original PDF streams, reconstructs
wide names, and refuses to write a seed unless all 1,119 codes are unique and
all 33 department prefixes are present.

Current public DANE/DIVIPOLA publications can evolve independently of this
assessment snapshot. Updating the catalog is an explicit data-version change:
record the new official source URL, cutoff date, checksum, row counts, and
reviewed differences before regenerating this file.

## Regeneration

From the repository root:

```powershell
node database/seed/generate-colombia-seed.mjs `
  C:\path\to\subregiones.pdf `
  database/seed/001_colombia.sql
```

Database initialization never downloads reference data. It consumes only the
reviewed SQL checked into the repository.

Validate the generated artifact without a database:

```powershell
node database/seed/validate-colombia-seed.mjs
```

After initialization, run the transactional PostgreSQL smoke checks with:

```powershell
psql --file database/init/verify_database.sql
```
