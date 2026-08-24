import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { UserList } from "@/components/users/user-list";
import type { UserPage } from "@/lib/api/contracts";

const emptyPage: UserPage = {
  items: [],
  page: 1,
  pageSize: 10,
  totalCount: 0,
  totalPages: 0,
};

describe("UserList", () => {
  it("distinguishes empty catalog and empty search states", () => {
    const { rerender } = render(<UserList page={emptyPage} search="" />);
    expect(
      screen.getByRole("heading", { name: /aún no hay usuarios/i }),
    ).toBeVisible();
    expect(
      screen.getByRole("link", { name: /crear primer usuario/i }),
    ).toHaveAttribute("href", "/users/new");

    rerender(<UserList page={emptyPage} search="Ada" />);
    expect(
      screen.getByRole("heading", { name: /no encontramos coincidencias/i }),
    ).toBeVisible();
    expect(
      screen.getByRole("link", { name: /limpiar búsqueda/i }),
    ).toHaveAttribute("href", "/");
  });

  it("renders a deterministic page and preserves search in pagination", () => {
    render(
      <UserList
        page={{
          items: [
            {
              id: 7,
              name: "Ada Lovelace",
              phone: "3001234567",
              countryId: 1,
              departmentId: 5,
              municipalityId: 5001,
              address: "Calle 1",
              createdAt: "2026-01-01T00:00:00Z",
              updatedAt: "2026-01-01T00:00:00Z",
            },
          ],
          page: 1,
          pageSize: 1,
          totalCount: 2,
          totalPages: 2,
        }}
        search="Ada"
      />,
    );

    expect(screen.getByText("Ada Lovelace").closest("a")).toHaveAttribute(
      "href",
      "/users/7",
    );
    expect(screen.getByRole("link", { name: /siguiente/i })).toHaveAttribute(
      "href",
      "/?page=2&search=Ada",
    );
  });
});
