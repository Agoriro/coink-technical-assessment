\set ON_ERROR_STOP on

-- Destructive local-only reset: removes all application data and database objects.
drop schema if exists app cascade;

\ir 001_initialize.sql
