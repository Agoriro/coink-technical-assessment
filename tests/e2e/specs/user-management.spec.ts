import { expect, test, type APIRequestContext } from "@playwright/test";

const apiBaseUrl = process.env.E2E_API_BASE_URL ?? "http://localhost:8080";

type Country = { id: number; name: string };
type Department = { id: number; code: string; name: string };
type Municipality = { id: number; name: string };
type User = { id: number; name: string };
type UserPage = { items: User[] };

test.describe("user management", () => {
  test.beforeEach(async ({ request }) => clearUsers(request));
  test.afterAll(async ({ request }) => clearUsers(request));

  test("creates, views, edits, searches, and deletes a user", async ({
    page,
    request,
  }) => {
    const geography = await getGeography(request);
    const name = "Margaret Hamilton";

    await page.goto("/");
    await expect(
      page.getByRole("heading", { name: /aún no hay usuarios/i }),
    ).toBeVisible();
    await page.getByRole("link", { name: /crear primer usuario/i }).click();

    await page.getByRole("button", { name: /crear usuario/i }).click();
    await expect(page.getByText(/el nombre es obligatorio/i)).toBeVisible();
    await expect(
      page.getByText("Selecciona un país.", { exact: true }),
    ).toBeVisible();

    await page.getByLabel(/nombre completo/i).fill(name);
    await page.getByLabel(/teléfono/i).fill("+573001234567");
    await page
      .getByLabel(/^país$/i)
      .selectOption(geography.country.id.toString());
    await expect(page.getByLabel(/departamento/i)).toBeEnabled();
    await page
      .getByLabel(/departamento/i)
      .selectOption(geography.department.id.toString());
    await expect(page.getByLabel(/municipio/i)).toBeEnabled();
    await page
      .getByLabel(/municipio/i)
      .selectOption(geography.municipality.id.toString());
    await page.getByLabel(/dirección/i).fill("Calle 10 # 20-30");
    await page.getByRole("button", { name: /crear usuario/i }).click();

    await expect(page).toHaveURL(/\/users\/\d+$/);
    await expect(page.getByRole("heading", { name, level: 1 })).toBeVisible();
    await expect(page.getByText("Calle 10 # 20-30")).toBeVisible();

    await page.getByRole("link", { name: /editar/i }).click();
    await page.getByLabel(/dirección/i).fill("Carrera 7 # 8-90");
    await page.getByRole("button", { name: /guardar cambios/i }).click();
    await expect(page.getByText("Carrera 7 # 8-90")).toBeVisible();

    await page.goto("/");
    await page
      .getByRole("searchbox", { name: /buscar por nombre/i })
      .fill("Hamilton");
    await page.getByRole("button", { name: /buscar/i }).click();
    const userLink = page.locator("a.user-cell").filter({ hasText: name });
    await expect(userLink).toBeVisible();
    await userLink.click();

    page.once("dialog", async (dialog) => dialog.accept());
    await page.getByRole("button", { name: /^eliminar$/i }).click();
    await expect(page).toHaveURL(/\/$/);
    await expect(
      page.getByRole("heading", { name: /aún no hay usuarios/i }),
    ).toBeVisible();
  });

  test("paginates and filters deterministic results", async ({
    page,
    request,
  }) => {
    const geography = await getGeography(request);
    for (let index = 1; index <= 11; index++) {
      await createUser(request, geography, {
        name: `Persona ${index.toString().padStart(2, "0")}`,
        phone: `30012345${index.toString().padStart(2, "0")}`,
      });
    }

    await page.goto("/");
    const summary = page.getByRole("region", {
      name: /resumen del directorio/i,
    });
    await expect(summary.getByText("11", { exact: true })).toBeVisible();
    await expect(summary).toContainText("usuarios registrados");
    await expect(page.locator("tbody tr")).toHaveCount(10);
    await page.getByRole("link", { name: /siguiente/i }).click();
    await expect(page).toHaveURL(/page=2/);
    await expect(page.locator("tbody tr")).toHaveCount(1);

    await page
      .getByRole("searchbox", { name: /buscar por nombre/i })
      .fill("Persona 11");
    await page.getByRole("button", { name: /buscar/i }).click();
    await expect(page.locator("tbody tr")).toHaveCount(1);
    await expect(
      page.locator("a.user-cell").filter({ hasText: "Persona 11" }),
    ).toBeVisible();
  });

  test("shows dependent-selector loading and safe API errors", async ({
    page,
    request,
  }) => {
    const geography = await getGeography(request);
    await page.route("**/api/v1/countries/*/departments", async (route) => {
      await new Promise((resolve) => setTimeout(resolve, 250));
      await route.fulfill({
        status: 503,
        contentType: "application/problem+json",
        body: JSON.stringify({ title: "Unavailable" }),
      });
    });

    await page.goto("/users/new");
    await page
      .getByLabel(/^país$/i)
      .selectOption(geography.country.id.toString());
    const departmentSelect = page.getByLabel(/departamento/i);
    await expect(departmentSelect).toBeDisabled();
    await expect(departmentSelect).toContainText(/cargando/i);
    await expect(
      page
        .getByRole("alert")
        .filter({ hasText: /no fue posible cargar los departamentos/i }),
    ).toBeVisible();
  });
});

async function clearUsers(request: APIRequestContext): Promise<void> {
  const response = await request.get(
    `${apiBaseUrl}/api/v1/users?page=1&pageSize=100`,
  );
  expect(response.ok()).toBeTruthy();
  const users = (await response.json()) as UserPage;
  for (const user of users.items) {
    const deletion = await request.delete(
      `${apiBaseUrl}/api/v1/users/${user.id}`,
    );
    expect(deletion.status()).toBe(204);
  }
}

async function getGeography(request: APIRequestContext) {
  const countriesResponse = await request.get(`${apiBaseUrl}/api/v1/countries`);
  expect(countriesResponse.ok()).toBeTruthy();
  const country = ((await countriesResponse.json()) as Country[])[0];

  const departmentsResponse = await request.get(
    `${apiBaseUrl}/api/v1/countries/${country.id}/departments`,
  );
  expect(departmentsResponse.ok()).toBeTruthy();
  const departments = (await departmentsResponse.json()) as Department[];
  const department = departments.find((candidate) => candidate.code === "05")!;

  const municipalitiesResponse = await request.get(
    `${apiBaseUrl}/api/v1/departments/${department.id}/municipalities`,
  );
  expect(municipalitiesResponse.ok()).toBeTruthy();
  const municipality = (
    (await municipalitiesResponse.json()) as Municipality[]
  )[0];
  return { country, department, municipality };
}

async function createUser(
  request: APIRequestContext,
  geography: Awaited<ReturnType<typeof getGeography>>,
  identity: { name: string; phone: string },
): Promise<void> {
  const response = await request.post(`${apiBaseUrl}/api/v1/users`, {
    data: {
      ...identity,
      countryId: geography.country.id,
      departmentId: geography.department.id,
      municipalityId: geography.municipality.id,
      address: "Calle de prueba 1",
    },
  });
  expect(response.status()).toBe(201);
}
