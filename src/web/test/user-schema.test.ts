import { describe, expect, it } from "vitest";

import { userSchema } from "@/lib/validation/user-schema";

const validUser = {
  name: "Ada Lovelace",
  phone: "+573001234567",
  countryId: 1,
  departmentId: 5,
  municipalityId: 5001,
  address: "Calle 1 # 2-3",
};

describe("userSchema", () => {
  it("accepts the documented phone boundaries and a complete geography", () => {
    expect(userSchema.safeParse(validUser).success).toBe(true);
    expect(
      userSchema.safeParse({ ...validUser, phone: "1234567" }).success,
    ).toBe(true);
    expect(
      userSchema.safeParse({ ...validUser, phone: "123456789012345" }).success,
    ).toBe(true);
  });

  it("rejects padded text, malformed phones, and missing geography", () => {
    const result = userSchema.safeParse({
      ...validUser,
      name: " Ada Lovelace ",
      phone: "300-123",
      departmentId: 0,
      address: "",
    });

    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.issues.map((issue) => issue.path[0])).toEqual(
        expect.arrayContaining(["name", "phone", "departmentId", "address"]),
      );
    }
  });
});
