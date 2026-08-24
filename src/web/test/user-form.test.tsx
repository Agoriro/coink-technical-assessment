import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { UserForm } from "@/components/users/user-form";

const router = vi.hoisted(() => ({ push: vi.fn(), refresh: vi.fn() }));

vi.mock("next/navigation", () => ({ useRouter: () => router }));

const countries = [
  { id: 1, isoAlpha2: "CO", isoAlpha3: "COL", name: "Colombia" },
];
const department = { id: 5, countryId: 1, code: "05", name: "Antioquia" };
const municipality = {
  id: 5001,
  countryId: 1,
  departmentId: 5,
  code: "05001",
  name: "Medellín",
};

describe("UserForm", () => {
  const fetchMock = vi.fn<typeof fetch>();

  beforeEach(() => {
    router.push.mockReset();
    router.refresh.mockReset();
    fetchMock.mockReset();
    vi.stubGlobal("fetch", fetchMock);
  });

  it("loads dependent geography and submits a valid registration", async () => {
    fetchMock.mockImplementation(async (input, init) => {
      const url = input.toString();
      if (url.endsWith("/api/v1/countries/1/departments")) {
        return jsonResponse([department]);
      }
      if (url.endsWith("/api/v1/departments/5/municipalities")) {
        return jsonResponse([municipality]);
      }
      if (url.endsWith("/api/v1/users") && init?.method === "POST") {
        return jsonResponse({
          id: 9,
          name: "Ada Lovelace",
          phone: "+573001234567",
          countryId: 1,
          departmentId: 5,
          municipalityId: 5001,
          address: "Calle 1 # 2-3",
          createdAt: "2026-01-01T00:00:00Z",
          updatedAt: "2026-01-01T00:00:00Z",
        });
      }
      throw new Error(`Unexpected request: ${url}`);
    });
    const user = userEvent.setup();
    render(<UserForm countries={countries} />);

    await user.type(screen.getByLabelText(/nombre completo/i), "Ada Lovelace");
    await user.type(screen.getByLabelText(/teléfono/i), "+573001234567");
    await user.selectOptions(screen.getByLabelText(/^país$/i), "1");
    await user.selectOptions(
      await screen.findByLabelText(/departamento/i, { selector: "select" }),
      "5",
    );
    await user.selectOptions(
      await screen.findByLabelText(/municipio/i, { selector: "select" }),
      "5001",
    );
    await user.type(screen.getByLabelText(/dirección/i), "Calle 1 # 2-3");
    await user.click(screen.getByRole("button", { name: /crear usuario/i }));

    await waitFor(() => expect(router.push).toHaveBeenCalledWith("/users/9"));
    expect(router.refresh).toHaveBeenCalledOnce();
    const createCall = fetchMock.mock.calls.find(
      ([, init]) => init?.method === "POST",
    );
    expect(createCall?.[1]?.body).toContain('"municipalityId":5001');
  });

  it("exposes loading and safe location error states", async () => {
    let rejectRequest: ((reason: Error) => void) | undefined;
    fetchMock.mockReturnValue(
      new Promise<Response>((_, reject) => {
        rejectRequest = reject;
      }),
    );
    const user = userEvent.setup();
    render(<UserForm countries={countries} />);

    await user.selectOptions(screen.getByLabelText(/^país$/i), "1");
    expect(screen.getByRole("option", { name: /cargando/i })).toBeVisible();
    rejectRequest?.(new Error("network unavailable"));

    expect(
      await screen.findByText(/no fue posible cargar los departamentos/i),
    ).toHaveAttribute("role", "alert");
  });
});

function jsonResponse(value: unknown): Response {
  return new Response(JSON.stringify(value), {
    status: 200,
    headers: { "Content-Type": "application/json" },
  });
}
