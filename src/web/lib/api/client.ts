import type {
  Department,
  Municipality,
  User,
  UserInput,
} from "@/lib/api/contracts";
import { requestJson, requestNoContent } from "@/lib/api/request";

const apiBaseUrl = (
  process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:8080"
).replace(/\/$/, "");

const jsonHeaders = { "Content-Type": "application/json" };

export function createUser(input: UserInput): Promise<User> {
  return requestJson<User>(apiBaseUrl, "/api/v1/users", {
    method: "POST",
    headers: jsonHeaders,
    body: JSON.stringify(input),
  });
}

export function updateUser(userId: number, input: UserInput): Promise<User> {
  return requestJson<User>(apiBaseUrl, `/api/v1/users/${userId}`, {
    method: "PUT",
    headers: jsonHeaders,
    body: JSON.stringify(input),
  });
}

export function deleteUser(userId: number): Promise<void> {
  return requestNoContent(apiBaseUrl, `/api/v1/users/${userId}`, {
    method: "DELETE",
  });
}

export function getDepartments(countryId: number): Promise<Department[]> {
  return requestJson<Department[]>(
    apiBaseUrl,
    `/api/v1/countries/${countryId}/departments`,
  );
}

export function getMunicipalities(
  departmentId: number,
): Promise<Municipality[]> {
  return requestJson<Municipality[]>(
    apiBaseUrl,
    `/api/v1/departments/${departmentId}/municipalities`,
  );
}
