\set ON_ERROR_STOP on

begin;

\ir ../schema/001_schema.sql
\ir ../seed/001_colombia.sql
\ir ../procedures/001_geography.sql
\ir ../procedures/002_users.sql

commit;
