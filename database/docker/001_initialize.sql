\set ON_ERROR_STOP on

-- Docker entrypoint wrapper. The canonical script keeps relative includes
-- portable for direct psql execution and container initialization.
\ir /database/init/001_initialize.sql
