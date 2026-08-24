import { z } from "zod";

const unpaddedText = (label: string, maximumLength: number) =>
  z
    .string()
    .min(1, `${label} es obligatorio.`)
    .max(
      maximumLength,
      `${label} no puede superar ${maximumLength} caracteres.`,
    )
    .refine(
      (value) => value === value.trim(),
      `${label} no debe iniciar ni terminar con espacios.`,
    );

export const userSchema = z.object({
  name: unpaddedText("El nombre", 150),
  phone: z
    .string()
    .min(1, "El teléfono es obligatorio.")
    .regex(
      /^\+?[0-9]{7,15}$/,
      "Usa entre 7 y 15 dígitos y un signo + opcional al inicio.",
    ),
  countryId: z.number().int().positive("Selecciona un país."),
  departmentId: z.number().int().positive("Selecciona un departamento."),
  municipalityId: z.number().int().positive("Selecciona un municipio."),
  address: unpaddedText("La dirección", 250),
});

export type UserFormValues = z.infer<typeof userSchema>;
