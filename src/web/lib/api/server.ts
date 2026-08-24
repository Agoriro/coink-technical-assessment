import type {
  Country,
  Department,
  Municipality,
  User,
  UserPage,
} from "@/lib/api/contracts";
import { requestJson } from "@/lib/api/request";

const apiBaseUrl = (
  process.env.API_BASE_URL ??
  process.env.NEXT_PUBLIC_API_BASE_URL ??
  "http://localhost:8080"
).replace(/\/$/, "");

const serverRequest: RequestInit = { cache: "no-store" };

export function getUsers(
  page: number,
  pageSize: number,
  search: string,
): Promise<UserPage> {
  const query = new URLSearchParams({
    page: page.toString(),
    pageSize: pageSize.toString(),
  });
  if (search) {
    query.set("search", search);
  }

  return requestJson<UserPage>(
    apiBaseUrl,
    `/api/v1/users?${query}`,
    serverRequest,
  );
}

export function getUser(userId: number): Promise<User> {
  return requestJson<User>(
    apiBaseUrl,
    `/api/v1/users/${userId}`,
    serverRequest,
  );
}

export function getCountries(): Promise<Country[]> {
  return requestJson<Country[]>(apiBaseUrl, "/api/v1/countries", serverRequest);
}

export function getDepartments(countryId: number): Promise<Department[]> {
  return requestJson<Department[]>(
    apiBaseUrl,
    `/api/v1/countries/${countryId}/departments`,
    serverRequest,
  );
}

export function getMunicipalities(
  departmentId: number,
): Promise<Municipality[]> {
  return requestJson<Municipality[]>(
    apiBaseUrl,
    `/api/v1/departments/${departmentId}/municipalities`,
    serverRequest,
  );
}
