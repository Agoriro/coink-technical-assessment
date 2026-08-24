import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";

import { DeleteUserButton } from "@/components/users/delete-user-button";
import { ErrorNotice } from "@/components/ui/error-notice";
import { MapPinIcon, PhoneIcon } from "@/components/ui/icons";
import { PageHeading } from "@/components/ui/page-heading";
import { ApiError } from "@/lib/api/request";
import {
  getCountries,
  getDepartments,
  getMunicipalities,
  getUser,
} from "@/lib/api/server";
import { formatDateTime, initials } from "@/lib/format";

export const metadata: Metadata = { title: "Detalle de usuario" };
export const dynamic = "force-dynamic";

type UserDetailPageProps = {
  params: Promise<{ id: string }>;
};

export default async function UserDetailPage({ params }: UserDetailPageProps) {
  const { id } = await params;
  if (!/^\d+$/.test(id) || Number(id) < 1) {
    notFound();
  }

  const userId = Number(id);
  let user;
  try {
    user = await getUser(userId);
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) {
      notFound();
    }
    return (
      <ErrorNotice
        message="No fue posible consultar el registro. Comprueba la conexión con la API."
        retryHref={`/users/${id}`}
      />
    );
  }

  let countryName = `País ${user.countryId}`;
  let departmentName = `Departamento ${user.departmentId}`;
  let municipalityName = `Municipio ${user.municipalityId}`;
  try {
    const [countries, departments, municipalities] = await Promise.all([
      getCountries(),
      getDepartments(user.countryId),
      getMunicipalities(user.departmentId),
    ]);
    countryName =
      countries.find((country) => country.id === user.countryId)?.name ??
      countryName;
    departmentName =
      departments.find((department) => department.id === user.departmentId)
        ?.name ?? departmentName;
    municipalityName =
      municipalities.find(
        (municipality) => municipality.id === user.municipalityId,
      )?.name ?? municipalityName;
  } catch {
    // Numeric references remain visible when the read-only catalog is temporarily unavailable.
  }

  return (
    <>
      <PageHeading
        actions={
          <>
            <Link
              className="button button-secondary"
              href={`/users/${user.id}/edit`}
            >
              Editar
            </Link>
            <DeleteUserButton userId={user.id} userName={user.name} />
          </>
        }
        backHref="/"
        description={`Registro #${user.id}`}
        eyebrow="Perfil de usuario"
        title={user.name}
      />

      <section className="profile-card">
        <div className="profile-identity">
          <span className="profile-avatar">{initials(user.name)}</span>
          <div>
            <p className="profile-label">Usuario activo</p>
            <h2>{user.name}</h2>
            <p>Registrado el {formatDateTime(user.createdAt)}</p>
          </div>
        </div>

        <div className="detail-grid">
          <article className="detail-item">
            <span className="detail-icon">
              <PhoneIcon className="size-5" />
            </span>
            <div>
              <p>Teléfono</p>
              <strong className="tabular-nums">{user.phone}</strong>
            </div>
          </article>
          <article className="detail-item">
            <span className="detail-icon">
              <MapPinIcon className="size-5" />
            </span>
            <div>
              <p>Ubicación</p>
              <strong>
                {municipalityName}, {departmentName}
              </strong>
              <small>{countryName}</small>
            </div>
          </article>
          <article className="detail-item detail-item-wide">
            <span className="detail-index">A</span>
            <div>
              <p>Dirección</p>
              <strong>{user.address}</strong>
            </div>
          </article>
        </div>

        <div className="audit-row">
          <span>Creado: {formatDateTime(user.createdAt)}</span>
          <span>Última actualización: {formatDateTime(user.updatedAt)}</span>
        </div>
      </section>
    </>
  );
}
