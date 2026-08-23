-- Purpose: Return the read-only country catalog.
-- Parameters: p_result is the transaction-scoped cursor opened by the procedure.
-- Result: id, ISO alpha-2/alpha-3 codes, and name, ordered deterministically.
-- Validation/errors: no external values are accepted; unexpected database errors propagate.
-- Concurrency/order: a cursor observes the caller transaction snapshot and orders by name, then id.
create or replace procedure app.get_countries(
    inout p_result refcursor default 'countries_result'
)
language plpgsql
as $$
begin
    open p_result for
        select
            country.id,
            country.iso_alpha2,
            country.iso_alpha3,
            country.name
        from app.countries as country
        order by lower(country.name), country.id;
end;
$$;

-- Purpose: Return departments belonging to one country.
-- Parameters: p_country_id identifies the parent; p_result is the transaction-scoped cursor.
-- Result: id, country_id, DIVIPOLA code, and name, ordered deterministically.
-- Validation/errors: raises SQLSTATE P0002 when the parent country does not exist.
-- Concurrency/order: reference data is immutable to the application; order is name, then id.
create or replace procedure app.get_departments(
    in p_country_id smallint,
    inout p_result refcursor default 'departments_result'
)
language plpgsql
as $$
begin
    if not exists (
        select 1
        from app.countries as country
        where country.id = p_country_id
    ) then
        raise exception
            using message = format('Country %s was not found.', p_country_id),
            errcode = 'P0002';
    end if;

    open p_result for
        select
            department.id,
            department.country_id,
            department.code,
            department.name
        from app.departments as department
        where department.country_id = p_country_id
        order by lower(department.name), department.id;
end;
$$;

-- Purpose: Return municipality-level DIVIPOLA units belonging to one department.
-- Parameters: p_department_id identifies the parent; p_result is the transaction-scoped cursor.
-- Result: id, country_id, department_id, DIVIPOLA code, and name.
-- Validation/errors: raises SQLSTATE P0002 when the parent department does not exist.
-- Concurrency/order: reference data is immutable to the application; order is name, then id.
create or replace procedure app.get_municipalities(
    in p_department_id smallint,
    inout p_result refcursor default 'municipalities_result'
)
language plpgsql
as $$
begin
    if not exists (
        select 1
        from app.departments as department
        where department.id = p_department_id
    ) then
        raise exception
            using message = format('Department %s was not found.', p_department_id),
            errcode = 'P0002';
    end if;

    open p_result for
        select
            municipality.id,
            municipality.country_id,
            municipality.department_id,
            municipality.code,
            municipality.name
        from app.municipalities as municipality
        where municipality.department_id = p_department_id
        order by lower(municipality.name), municipality.id;
end;
$$;
