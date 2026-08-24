import type { Metadata } from "next";
import { notFound } from "next/navigation";

import { UserForm } from "@/components/users/user-form";
import { ErrorNotice } from "@/components/ui/error-notice";
import { PageHeading } from "@/components/ui/page-heading";
import { ApiError } from "@/lib/api/request";
import {
  getCountries,
  getDepartments,
  getMunicipalities,
  getUser,
} from "@/lib/api/server";

export const metadata: Metadata = { title: "Editar usuario" };
export const dynamic = "force-dynamic";

type EditUserPageProps = {
  params: Promise<{ id: string }>;
};

export default async function EditUserPage({ params }: EditUserPageProps) {
  const { id } = await params;
  if (!/^\d+$/.test(id) || Number(id) < 1) {
    notFound();
  }

  let data;
  try {
    const user = await getUser(Number(id));
    const [countries, departments, municipalities] = await Promise.all([
      getCountries(),
      getDepartments(user.countryId),
      getMunicipalities(user.departmentId),
    ]);
    data = { user, countries, departments, municipalities };
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) {
      notFound();
    }
    return (
      <ErrorNotice
        message="No pudimos cargar el usuario o su catálogo geográfico. Comprueba la API."
        retryHref={`/users/${id}/edit`}
      />
    );
  }

  return (
    <>
      <PageHeading
        backHref={`/users/${data.user.id}`}
        description="Actualiza los campos necesarios y conserva la relación geográfica válida."
        eyebrow={`Registro #${data.user.id}`}
        title="Editar usuario"
      />
      <UserForm
        countries={data.countries}
        initialDepartments={data.departments}
        initialMunicipalities={data.municipalities}
        user={data.user}
      />
    </>
  );
}
