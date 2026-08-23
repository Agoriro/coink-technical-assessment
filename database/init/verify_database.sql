\set ON_ERROR_STOP on

begin;

do $$
declare
    department_count integer;
    municipality_count integer;
begin
    select count(*) into department_count
    from app.departments
    where country_id = 170;

    select count(*) into municipality_count
    from app.municipalities
    where country_id = 170;

    if department_count <> 33 or municipality_count <> 1119 then
        raise exception 'Reference-data counts are invalid.';
    end if;

    if exists (
        select 1
        from app.municipalities as municipality
        left join app.departments as department
            on department.country_id = municipality.country_id
            and department.id = municipality.department_id
        where department.id is null
            or left(municipality.code, 2) <> department.code
    ) then
        raise exception 'The geographic hierarchy contains an orphan or mismatched code.';
    end if;

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
            'Invalid Geography Probe',
            '+573000000001',
            170,
            5,
            8001,
            'Transient verification row'
        );

        raise exception 'The composite geographic foreign key did not reject a mismatch.';
    exception
        when foreign_key_violation then
            null;
    end;
end;
$$;

call app.get_countries('verify_countries');
fetch all from verify_countries;

call app.get_departments(170::smallint, 'verify_departments');
fetch all from verify_departments;

call app.get_municipalities(5::smallint, 'verify_municipalities');
fetch all from verify_municipalities;

call app.create_user(
    'Phase 2 Verification',
    '+573000000000',
    170::smallint,
    5::smallint,
    5002,
    'Transient verification row',
    'verify_create_user'
);
fetch all from verify_create_user;

select max(id) as verify_user_id
from app.users
where name = 'Phase 2 Verification'
\gset

call app.get_user(:verify_user_id, 'verify_get_user');
fetch all from verify_get_user;

call app.list_users(1, 20, 'Phase 2', 'verify_list_users');
fetch all from verify_list_users;

call app.update_user(
    :verify_user_id,
    'Phase 2 Verification Updated',
    '+573000000000',
    170::smallint,
    5::smallint,
    5002,
    'Transient verification row',
    'verify_update_user'
);
fetch all from verify_update_user;

call app.delete_user(:verify_user_id, 'verify_delete_user');
fetch all from verify_delete_user;

rollback;
