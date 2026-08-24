"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useForm } from "react-hook-form";

import type {
  Country,
  Department,
  Municipality,
  User,
} from "@/lib/api/contracts";
import {
  createUser,
  getDepartments,
  getMunicipalities,
  updateUser,
} from "@/lib/api/client";
import { ApiError } from "@/lib/api/request";
import { userSchema, type UserFormValues } from "@/lib/validation/user-schema";

type UserFormProps = {
  countries: Country[];
  initialDepartments?: Department[];
  initialMunicipalities?: Municipality[];
  user?: User;
};

const editableFields = new Set<keyof UserFormValues>([
  "name",
  "phone",
  "countryId",
  "departmentId",
  "municipalityId",
  "address",
]);

function isEditableField(value: string): value is keyof UserFormValues {
  return editableFields.has(value as keyof UserFormValues);
}

export function UserForm({
  countries,
  initialDepartments = [],
  initialMunicipalities = [],
  user,
}: UserFormProps) {
  const router = useRouter();
  const [departments, setDepartments] = useState(initialDepartments);
  const [municipalities, setMunicipalities] = useState(initialMunicipalities);
  const [loadingDepartments, setLoadingDepartments] = useState(false);
  const [loadingMunicipalities, setLoadingMunicipalities] = useState(false);
  const [locationError, setLocationError] = useState<string | null>(null);
  const [submitError, setSubmitError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    setError,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<UserFormValues>({
    resolver: zodResolver(userSchema),
    defaultValues: {
      name: user?.name ?? "",
      phone: user?.phone ?? "",
      countryId: user?.countryId ?? 0,
      departmentId: user?.departmentId ?? 0,
      municipalityId: user?.municipalityId ?? 0,
      address: user?.address ?? "",
    },
  });

  const countryField = register("countryId", { valueAsNumber: true });
  const departmentField = register("departmentId", { valueAsNumber: true });
  const municipalityField = register("municipalityId", { valueAsNumber: true });

  async function changeCountry(event: React.ChangeEvent<HTMLSelectElement>) {
    await countryField.onChange(event);
    const countryId = Number(event.target.value);
    setValue("departmentId", 0, { shouldValidate: true });
    setValue("municipalityId", 0, { shouldValidate: true });
    setDepartments([]);
    setMunicipalities([]);
    setLocationError(null);

    if (countryId < 1) {
      return;
    }

    setLoadingDepartments(true);
    try {
      setDepartments(await getDepartments(countryId));
    } catch {
      setLocationError(
        "No fue posible cargar los departamentos. Intenta nuevamente.",
      );
    } finally {
      setLoadingDepartments(false);
    }
  }

  async function changeDepartment(event: React.ChangeEvent<HTMLSelectElement>) {
    await departmentField.onChange(event);
    const departmentId = Number(event.target.value);
    setValue("municipalityId", 0, { shouldValidate: true });
    setMunicipalities([]);
    setLocationError(null);

    if (departmentId < 1) {
      return;
    }

    setLoadingMunicipalities(true);
    try {
      setMunicipalities(await getMunicipalities(departmentId));
    } catch {
      setLocationError(
        "No fue posible cargar los municipios. Intenta nuevamente.",
      );
    } finally {
      setLoadingMunicipalities(false);
    }
  }

  function applyApiValidation(error: ApiError): boolean {
    const validationErrors = error.problem.errors;
    if (!validationErrors) {
      return false;
    }

    let applied = false;
    for (const [field, messages] of Object.entries(validationErrors)) {
      if (isEditableField(field) && messages[0]) {
        setError(field, { type: "server", message: messages[0] });
        applied = true;
      }
    }

    return applied;
  }

  async function submit(values: UserFormValues) {
    setSubmitError(null);
    try {
      const savedUser = user
        ? await updateUser(user.id, values)
        : await createUser(values);
      router.push(`/users/${savedUser.id}`);
      router.refresh();
    } catch (error) {
      if (error instanceof ApiError) {
        if (applyApiValidation(error)) {
          setSubmitError("Revisa los campos marcados antes de continuar.");
          return;
        }
        setSubmitError(
          error.problem.detail ??
            "La API rechazó la solicitud. Verifica los datos e intenta de nuevo.",
        );
        return;
      }
      setSubmitError(
        "No fue posible comunicarse con la API. Comprueba que esté disponible.",
      );
    }
  }

  return (
    <form className="form-card" noValidate onSubmit={handleSubmit(submit)}>
      <div className="form-section-heading">
        <span>01</span>
        <div>
          <h2>Información personal</h2>
          <p>Datos básicos para identificar y contactar al usuario.</p>
        </div>
      </div>

      <div className="form-grid">
        <Field
          label="Nombre completo"
          error={errors.name?.message}
          htmlFor="name"
        >
          <input
            aria-describedby={errors.name ? "name-error" : undefined}
            aria-invalid={Boolean(errors.name)}
            autoComplete="name"
            id="name"
            maxLength={150}
            placeholder="Ej. María Rodríguez"
            {...register("name")}
          />
        </Field>

        <Field label="Teléfono" error={errors.phone?.message} htmlFor="phone">
          <input
            aria-describedby={errors.phone ? "phone-error" : "phone-hint"}
            aria-invalid={Boolean(errors.phone)}
            autoComplete="tel"
            id="phone"
            inputMode="tel"
            placeholder="Ej. +573001234567"
            type="tel"
            {...register("phone")}
          />
          {!errors.phone ? (
            <span className="field-hint" id="phone-hint">
              De 7 a 15 dígitos; puedes iniciar con +.
            </span>
          ) : null}
        </Field>
      </div>

      <div className="form-divider" />

      <div className="form-section-heading">
        <span>02</span>
        <div>
          <h2>Ubicación</h2>
          <p>La jerarquía se valida desde país hasta municipio.</p>
        </div>
      </div>

      {locationError ? (
        <p className="inline-alert" role="alert">
          {locationError}
        </p>
      ) : null}

      <div className="form-grid form-grid-three">
        <Field
          label="País"
          error={errors.countryId?.message}
          htmlFor="countryId"
        >
          <select
            aria-describedby={errors.countryId ? "countryId-error" : undefined}
            aria-invalid={Boolean(errors.countryId)}
            id="countryId"
            {...countryField}
            onChange={changeCountry}
          >
            <option value={0}>Selecciona un país</option>
            {countries.map((country) => (
              <option key={country.id} value={country.id}>
                {country.name}
              </option>
            ))}
          </select>
        </Field>

        <Field
          label="Departamento"
          error={errors.departmentId?.message}
          htmlFor="departmentId"
        >
          <select
            aria-describedby={
              errors.departmentId ? "departmentId-error" : undefined
            }
            aria-invalid={Boolean(errors.departmentId)}
            disabled={loadingDepartments || departments.length === 0}
            id="departmentId"
            {...departmentField}
            onChange={changeDepartment}
          >
            <option value={0}>
              {loadingDepartments ? "Cargando…" : "Selecciona un departamento"}
            </option>
            {departments.map((department) => (
              <option key={department.id} value={department.id}>
                {department.name}
              </option>
            ))}
          </select>
        </Field>

        <Field
          label="Municipio"
          error={errors.municipalityId?.message}
          htmlFor="municipalityId"
        >
          <select
            aria-describedby={
              errors.municipalityId ? "municipalityId-error" : undefined
            }
            aria-invalid={Boolean(errors.municipalityId)}
            disabled={loadingMunicipalities || municipalities.length === 0}
            id="municipalityId"
            {...municipalityField}
          >
            <option value={0}>
              {loadingMunicipalities ? "Cargando…" : "Selecciona un municipio"}
            </option>
            {municipalities.map((municipality) => (
              <option key={municipality.id} value={municipality.id}>
                {municipality.name}
              </option>
            ))}
          </select>
        </Field>
      </div>

      <div className="mt-5">
        <Field
          label="Dirección"
          error={errors.address?.message}
          htmlFor="address"
        >
          <textarea
            aria-describedby={errors.address ? "address-error" : undefined}
            aria-invalid={Boolean(errors.address)}
            autoComplete="street-address"
            id="address"
            maxLength={250}
            placeholder="Ej. Calle 10 # 20-30, apartamento 401"
            rows={3}
            {...register("address")}
          />
        </Field>
      </div>

      {submitError ? (
        <p className="submit-error" role="alert">
          {submitError}
        </p>
      ) : null}

      <div className="form-actions">
        <Link
          className="button button-secondary"
          href={user ? `/users/${user.id}` : "/"}
        >
          Cancelar
        </Link>
        <button
          className="button button-primary"
          disabled={isSubmitting}
          type="submit"
        >
          {isSubmitting
            ? "Guardando…"
            : user
              ? "Guardar cambios"
              : "Crear usuario"}
        </button>
      </div>
    </form>
  );
}

type FieldProps = {
  label: string;
  htmlFor: keyof UserFormValues;
  error?: string;
  children: React.ReactNode;
};

function Field({ label, htmlFor, error, children }: FieldProps) {
  return (
    <div className="field">
      <label htmlFor={htmlFor}>{label}</label>
      {children}
      {error ? (
        <span className="field-error" id={`${htmlFor}-error`} role="alert">
          {error}
        </span>
      ) : null}
    </div>
  );
}
