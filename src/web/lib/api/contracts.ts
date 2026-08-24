export type Country = {
  id: number;
  isoAlpha2: string;
  isoAlpha3: string;
  name: string;
};

export type Department = {
  id: number;
  countryId: number;
  code: string;
  name: string;
};

export type Municipality = {
  id: number;
  countryId: number;
  departmentId: number;
  code: string;
  name: string;
};

export type User = {
  id: number;
  name: string;
  phone: string;
  countryId: number;
  departmentId: number;
  municipalityId: number;
  address: string;
  createdAt: string;
  updatedAt: string;
};

export type UserInput = Pick<
  User,
  "name" | "phone" | "countryId" | "departmentId" | "municipalityId" | "address"
>;

export type UserPage = {
  items: User[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

export type ProblemDetails = {
  status?: number;
  title?: string;
  detail?: string;
  code?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
};
