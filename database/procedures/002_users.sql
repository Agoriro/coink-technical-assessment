-- Purpose: Create a user and return the stored representation.
-- Parameters: the user fields and three geographic identifiers; p_result is the output cursor.
-- Result: one row containing the created user and timestamps.
-- Validation/errors: table checks reject blank/invalid values; fk_users_geography atomically
-- verifies that municipality, department, and country form one valid hierarchy (SQLSTATE 23503).
-- Concurrency/order: one atomic insert; no application-level uniqueness rule is assumed.
create or replace procedure app.create_user(
    in p_name varchar(150),
    in p_phone varchar(16),
    in p_country_id smallint,
    in p_department_id smallint,
    in p_municipality_id integer,
    in p_address varchar(250),
    inout p_result refcursor default 'create_user_result'
)
language plpgsql
as $$
declare
    created_user app.users%rowtype;
begin
    insert into app.users (
        name,
        phone,
        country_id,
        department_id,
        municipality_id,
        address
    )
    values (
        p_name,
        p_phone,
        p_country_id,
        p_department_id,
        p_municipality_id,
        p_address
    )
    returning
        id,
        name,
        phone,
        country_id,
        department_id,
        municipality_id,
        address,
        created_at,
        updated_at
    into created_user;

    open p_result for
        select
            created_user.id as id,
            created_user.name as name,
            created_user.phone as phone,
            created_user.country_id as country_id,
            created_user.department_id as department_id,
            created_user.municipality_id as municipality_id,
            created_user.address as address,
            created_user.created_at as created_at,
            created_user.updated_at as updated_at;
end;
$$;

-- Purpose: Return one user by identifier.
-- Parameters: p_user_id is the stable identity; p_result is the output cursor.
-- Result: zero rows when absent, otherwise one complete user row.
-- Validation/errors: absence is represented by an empty cursor for application-level 404 mapping.
-- Concurrency/order: reads the caller transaction snapshot; a primary-key lookup needs no ordering.
create or replace procedure app.get_user(
    in p_user_id bigint,
    inout p_result refcursor default 'get_user_result'
)
language plpgsql
as $$
begin
    open p_result for
        select
            app_user.id,
            app_user.name,
            app_user.phone,
            app_user.country_id,
            app_user.department_id,
            app_user.municipality_id,
            app_user.address,
            app_user.created_at,
            app_user.updated_at
        from app.users as app_user
        where app_user.id = p_user_id;
end;
$$;

-- Purpose: Return a bounded, searchable page of users plus the filtered total count.
-- Parameters: p_page is one-based, p_page_size is 1..100, and blank p_search means no filter.
-- Result: user columns plus total_count repeated per returned row. An empty page returns one
-- metadata row with nullable user columns so callers still receive the filtered total count.
-- Validation/errors: invalid pagination raises SQLSTATE 22023; search is literal and case-insensitive.
-- Concurrency/order: offset pagination observes one snapshot and orders by lower(name), then id.
create or replace procedure app.list_users(
    in p_page integer,
    in p_page_size integer,
    in p_search text,
    inout p_result refcursor default 'list_users_result'
)
language plpgsql
as $$
begin
    if p_page < 1 then
        raise exception
            using message = 'Page must be greater than or equal to 1.',
            errcode = '22023';
    end if;

    if p_page_size < 1 or p_page_size > 100 then
        raise exception
            using message = 'Page size must be between 1 and 100.',
            errcode = '22023';
    end if;

    open p_result for
        with filtered_users as (
            select
                app_user.id,
                app_user.name,
                app_user.phone,
                app_user.country_id,
                app_user.department_id,
                app_user.municipality_id,
                app_user.address,
                app_user.created_at,
                app_user.updated_at
            from app.users as app_user
            where p_search is null
                or btrim(p_search) = ''
                or position(lower(btrim(p_search)) in lower(app_user.name)) > 0
                or position(lower(btrim(p_search)) in lower(app_user.phone)) > 0
        ),
        paged_users as (
            select
                filtered_user.id,
                filtered_user.name,
                filtered_user.phone,
                filtered_user.country_id,
                filtered_user.department_id,
                filtered_user.municipality_id,
                filtered_user.address,
                filtered_user.created_at,
                filtered_user.updated_at
            from filtered_users as filtered_user
            order by lower(filtered_user.name), filtered_user.id
            limit p_page_size
            offset ((p_page::bigint - 1) * p_page_size::bigint)
        ),
        filtered_count as (
            select count(*) as total_count
            from filtered_users
        )
        select
            paged_user.id,
            paged_user.name,
            paged_user.phone,
            paged_user.country_id,
            paged_user.department_id,
            paged_user.municipality_id,
            paged_user.address,
            paged_user.created_at,
            paged_user.updated_at,
            filtered_count.total_count
        from filtered_count
        left join paged_users as paged_user on true
        order by lower(paged_user.name), paged_user.id;
end;
$$;

-- Purpose: Replace the editable fields of an existing user and return the stored representation.
-- Parameters: p_user_id plus all user fields; p_result is the output cursor.
-- Result: zero rows when absent, otherwise one updated user row.
-- Validation/errors: table checks validate values and fk_users_geography validates the full hierarchy.
-- Concurrency/order: one atomic update; PostgreSQL row locking serializes concurrent writes.
create or replace procedure app.update_user(
    in p_user_id bigint,
    in p_name varchar(150),
    in p_phone varchar(16),
    in p_country_id smallint,
    in p_department_id smallint,
    in p_municipality_id integer,
    in p_address varchar(250),
    inout p_result refcursor default 'update_user_result'
)
language plpgsql
as $$
declare
    updated_user app.users%rowtype;
begin
    update app.users as app_user
    set name = p_name,
        phone = p_phone,
        country_id = p_country_id,
        department_id = p_department_id,
        municipality_id = p_municipality_id,
        address = p_address,
        updated_at = clock_timestamp()
    where app_user.id = p_user_id
    returning
        app_user.id,
        app_user.name,
        app_user.phone,
        app_user.country_id,
        app_user.department_id,
        app_user.municipality_id,
        app_user.address,
        app_user.created_at,
        app_user.updated_at
    into updated_user;

    if found then
        open p_result for
            select
                updated_user.id as id,
                updated_user.name as name,
                updated_user.phone as phone,
                updated_user.country_id as country_id,
                updated_user.department_id as department_id,
                updated_user.municipality_id as municipality_id,
                updated_user.address as address,
                updated_user.created_at as created_at,
                updated_user.updated_at as updated_at;
    else
        open p_result for
            select
                app_user.id,
                app_user.name,
                app_user.phone,
                app_user.country_id,
                app_user.department_id,
                app_user.municipality_id,
                app_user.address,
                app_user.created_at,
                app_user.updated_at
            from app.users as app_user
            where false;
    end if;
end;
$$;

-- Purpose: Permanently delete one user and return the deleted identifier.
-- Parameters: p_user_id identifies the target; p_result is the output cursor.
-- Result: zero rows when absent, otherwise one row with id.
-- Validation/errors: absence is represented by an empty cursor for application-level 404 mapping.
-- Concurrency/order: one atomic hard delete; soft deletion/auditing is outside approved scope.
create or replace procedure app.delete_user(
    in p_user_id bigint,
    inout p_result refcursor default 'delete_user_result'
)
language plpgsql
as $$
declare
    deleted_user_id bigint;
begin
    delete from app.users as app_user
    where app_user.id = p_user_id
    returning app_user.id into deleted_user_id;

    open p_result for
        select deleted_user_id as id
        where deleted_user_id is not null;
end;
$$;
