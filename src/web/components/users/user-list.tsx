import Link from "next/link";

import { MoreIcon, UsersIcon } from "@/components/ui/icons";
import type { UserPage } from "@/lib/api/contracts";
import { formatDateTime, initials } from "@/lib/format";

type UserListProps = {
  page: UserPage;
  search: string;
};

export function UserList({ page, search }: UserListProps) {
  if (page.items.length === 0) {
    const pageOutsideResults = page.totalCount > 0;
    return (
      <section className="empty-state">
        <div className="empty-state-icon">
          <UsersIcon className="size-8" />
        </div>
        <h2>
          {pageOutsideResults
            ? "Esta página no contiene usuarios"
            : search
              ? "No encontramos coincidencias"
              : "Aún no hay usuarios"}
        </h2>
        <p>
          {pageOutsideResults
            ? "Vuelve a la primera página para consultar los resultados disponibles."
            : search
              ? `No hay resultados para “${search}”. Prueba con otro nombre o teléfono.`
              : "Registra la primera persona para comenzar a administrar el directorio."}
        </p>
        {pageOutsideResults ? (
          <Link
            className="button button-secondary mt-5"
            href={search ? `/?${new URLSearchParams({ search })}` : "/"}
          >
            Ir a la primera página
          </Link>
        ) : search ? (
          <Link className="button button-secondary mt-5" href="/">
            Limpiar búsqueda
          </Link>
        ) : (
          <Link className="button button-primary mt-5" href="/users/new">
            Crear primer usuario
          </Link>
        )}
      </section>
    );
  }

  return (
    <section aria-label="Usuarios registrados" className="table-card">
      <div className="overflow-x-auto">
        <table>
          <thead>
            <tr>
              <th>Usuario</th>
              <th>Teléfono</th>
              <th>Ubicación</th>
              <th>Actualización</th>
              <th>
                <span className="sr-only">Acciones</span>
              </th>
            </tr>
          </thead>
          <tbody>
            {page.items.map((user) => (
              <tr key={user.id}>
                <td>
                  <Link className="user-cell" href={`/users/${user.id}`}>
                    <span className="avatar">{initials(user.name)}</span>
                    <span>
                      <strong>{user.name}</strong>
                      <small>ID {user.id}</small>
                    </span>
                  </Link>
                </td>
                <td className="tabular-nums">{user.phone}</td>
                <td>
                  <span className="location-chip">
                    Municipio {user.municipalityId}
                  </span>
                </td>
                <td>
                  <time dateTime={user.updatedAt}>
                    {formatDateTime(user.updatedAt)}
                  </time>
                </td>
                <td>
                  <Link
                    aria-label={`Ver detalles de ${user.name}`}
                    className="icon-button"
                    href={`/users/${user.id}`}
                  >
                    <MoreIcon className="size-5" />
                  </Link>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <Pagination page={page} search={search} />
    </section>
  );
}

function Pagination({ page, search }: UserListProps) {
  const from = (page.page - 1) * page.pageSize + 1;
  const to = Math.min(page.page * page.pageSize, page.totalCount);

  return (
    <nav aria-label="Paginación de usuarios" className="pagination">
      <p>
        Mostrando <strong>{from}</strong>–<strong>{to}</strong> de{" "}
        <strong>{page.totalCount}</strong>
      </p>
      <div>
        <PaginationLink
          disabled={page.page <= 1}
          label="Anterior"
          page={page.page - 1}
          search={search}
        />
        <span aria-current="page" className="current-page">
          {page.page}
        </span>
        <PaginationLink
          disabled={page.page >= page.totalPages}
          label="Siguiente"
          page={page.page + 1}
          search={search}
        />
      </div>
    </nav>
  );
}

type PaginationLinkProps = {
  disabled: boolean;
  label: string;
  page: number;
  search: string;
};

function PaginationLink({
  disabled,
  label,
  page,
  search,
}: PaginationLinkProps) {
  if (disabled) {
    return (
      <span aria-disabled="true" className="pagination-link disabled">
        {label}
      </span>
    );
  }

  const query = new URLSearchParams({ page: page.toString() });
  if (search) {
    query.set("search", search);
  }

  return (
    <Link className="pagination-link" href={`/?${query}`}>
      {label}
    </Link>
  );
}
