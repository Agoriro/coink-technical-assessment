import type { Metadata } from "next";

import { UserForm } from "@/components/users/user-form";
import { ErrorNotice } from "@/components/ui/error-notice";
import { PageHeading } from "@/components/ui/page-heading";
import { getCountries } from "@/lib/api/server";

export const metadata: Metadata = { title: "Nuevo usuario" };
export const dynamic = "force-dynamic";

export default async function NewUserPage() {
  let countries;
  try {
    countries = await getCountries();
  } catch {
    return (
      <>
        <PageHeading
          backHref="/"
          description="Completa los datos requeridos para registrar una persona."
          eyebrow="Nuevo registro"
          title="Crear usuario"
        />
        <ErrorNotice
          message="No pudimos cargar el catálogo geográfico. Comprueba que la API esté disponible."
          retryHref="/users/new"
        />
      </>
    );
  }

  return (
    <>
      <PageHeading
        backHref="/"
        description="Completa los datos requeridos. La ubicación se valida como una jerarquía completa."
        eyebrow="Nuevo registro"
        title="Crear usuario"
      />
      <UserForm countries={countries} />
    </>
  );
}
