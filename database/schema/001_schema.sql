create schema if not exists app;

create table if not exists app.countries (
    id smallint primary key,
    iso_alpha2 varchar(2) not null,
    iso_alpha3 varchar(3) not null,
    name varchar(100) not null,
    constraint uq_countries_iso_alpha2 unique (iso_alpha2),
    constraint uq_countries_iso_alpha3 unique (iso_alpha3),
    constraint ck_countries_id_positive check (id > 0),
    constraint ck_countries_iso_alpha2_format check (iso_alpha2 ~ '^[A-Z]{2}$'),
    constraint ck_countries_iso_alpha3_format check (iso_alpha3 ~ '^[A-Z]{3}$'),
    constraint ck_countries_name_not_blank check (name = btrim(name) and name <> '')
);

create table if not exists app.departments (
    id smallint primary key,
    country_id smallint not null,
    code varchar(2) not null,
    name varchar(100) not null,
    constraint uq_departments_country_id_id unique (country_id, id),
    constraint uq_departments_country_id_code unique (country_id, code),
    constraint fk_departments_country
        foreign key (country_id) references app.countries (id)
        on update restrict on delete restrict,
    constraint ck_departments_id_matches_code
        check (code ~ '^[0-9]{2}$' and code = lpad(id::text, 2, '0')),
    constraint ck_departments_name_not_blank check (name = btrim(name) and name <> '')
);

create table if not exists app.municipalities (
    id integer primary key,
    country_id smallint not null,
    department_id smallint not null,
    code varchar(5) not null,
    name varchar(120) not null,
    constraint uq_municipalities_country_department_id
        unique (country_id, department_id, id),
    constraint uq_municipalities_country_code unique (country_id, code),
    constraint fk_municipalities_department
        foreign key (country_id, department_id)
        references app.departments (country_id, id)
        on update restrict on delete restrict,
    constraint ck_municipalities_id_matches_code
        check (code ~ '^[0-9]{5}$' and code = lpad(id::text, 5, '0')),
    constraint ck_municipalities_code_matches_department
        check (left(code, 2) = lpad(department_id::text, 2, '0')),
    constraint ck_municipalities_name_not_blank check (name = btrim(name) and name <> '')
);

create table if not exists app.users (
    id bigint generated always as identity primary key,
    name varchar(150) not null,
    phone varchar(16) not null,
    country_id smallint not null,
    department_id smallint not null,
    municipality_id integer not null,
    address varchar(250) not null,
    created_at timestamp with time zone not null default clock_timestamp(),
    updated_at timestamp with time zone not null default clock_timestamp(),
    constraint fk_users_geography
        foreign key (country_id, department_id, municipality_id)
        references app.municipalities (country_id, department_id, id)
        on update restrict on delete restrict,
    constraint ck_users_name_not_blank check (name = btrim(name) and name <> ''),
    constraint ck_users_phone_format check (phone ~ '^\+?[0-9]{7,15}$'),
    constraint ck_users_address_not_blank check (address = btrim(address) and address <> ''),
    constraint ck_users_updated_after_created check (updated_at >= created_at)
);

create index if not exists ix_departments_country_name_id
    on app.departments (country_id, lower(name), id);

create index if not exists ix_municipalities_department_name_id
    on app.municipalities (department_id, lower(name), id);

create index if not exists ix_users_name_id
    on app.users (lower(name), id);

create index if not exists ix_users_phone
    on app.users (phone);

create index if not exists ix_users_geography
    on app.users (country_id, department_id, municipality_id);
