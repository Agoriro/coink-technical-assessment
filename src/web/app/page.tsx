import Link from "next/link";

import { UserList } from "@/components/users/user-list";
import { ErrorNotice } from "@/components/ui/error-notice";
import { PlusIcon, SearchIcon, UsersIcon } from "@/components/ui/icons";
import { PageHeading } from "@/components/ui/page-heading";
import { getUsers } from "@/lib/api/server";

export const dynamic = "force-dynamic";

const pageSize = 10;

type HomePageProps = {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
};

function readPage(value: string | string[] | undefined): number {
  const candidate = Array.isArray(value) ? value[0] : value;
  if (!candidate || !/^\d+$/.test(candidate)) {
    return 1;
  }
  return Math.max(1, Number(candidate));
}

function readSearch(value: string | string[] | undefined): string {
  const candidate = Array.isArray(value) ? value[0] : value;
  return candidate?.trim().slice(0, 100) ?? "";
}

export default async function HomePage({ searchParams }: HomePageProps) {
  const parameters = await searchParams;
  const requestedPage = readPage(parameters.page);
  const search = readSearch(parameters.search);

  let users;
  try {
    users = await getUsers(requestedPage, pageSize, search);
  } catch {
    return (
      <>
        <PageHeading
          description="Consulta, registra y actualiza usuarios desde un solo lugar."
          eyebrow="Directorio"
          title="Usuarios"
        />
        <ErrorNotice
          message="Comprueba que la API esté disponible en el puerto configurado y vuelve a intentarlo."
          retryHref="/"
        />
      </>
    );
  }

  return (
    <>
      <PageHeading
        actions={
          <Link className="button button-primary" href="/users/new">
            <PlusIcon className="size-5" />
            Nuevo usuario
          </Link>
        }
        description="Consulta, registra y actualiza usuarios desde un solo lugar."
        eyebrow="Directorio"
        title="Usuarios"
      />

      <section className="summary-strip" aria-label="Resumen del directorio">
        <div className="summary-icon">
          <UsersIcon className="size-6" />
        </div>
        <div>
          <strong>{users.totalCount}</strong>
          <span>
            {users.totalCount === 1
              ? "usuario registrado"
              : "usuarios registrados"}
          </span>
        </div>
        <span className="summary-separator" />
        <p>Datos ordenados por nombre para una consulta consistente.</p>
      </section>

      <div className="list-toolbar">
        <form action="/" className="search-form" method="get" role="search">
          <SearchIcon className="search-form-icon" />
          <label className="sr-only" htmlFor="search">
            Buscar por nombre o teléfono
          </label>
          <input
            defaultValue={search}
            id="search"
            maxLength={100}
            name="search"
            placeholder="Buscar por nombre o teléfono"
            type="search"
          />
          <button className="button button-secondary" type="submit">
            Buscar
          </button>
        </form>
        {search ? (
          <Link className="clear-search" href="/">
            Limpiar filtro
          </Link>
        ) : null}
      </div>

      <UserList page={users} search={search} />
    </>
  );
}
