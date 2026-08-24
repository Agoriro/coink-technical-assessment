import { readFileSync } from "node:fs";

const [, , seedPath = "database/seed/001_colombia.sql"] = process.argv;
const seed = readFileSync(seedPath, "utf8");
const rowPattern =
  /^    \((\d+), 170, (\d+), '(\d{5})', '((?:''|[^'])*)'\)[,]?$/gm;
const rows = [...seed.matchAll(rowPattern)].map((match) => ({
  id: Number(match[1]),
  departmentId: Number(match[2]),
  code: match[3],
  name: match[4].replaceAll("''", "'"),
}));

const codes = new Set(rows.map(({ code }) => code));
const departmentIds = new Set(rows.map(({ departmentId }) => departmentId));
const invalidRows = rows.filter(
  ({ id, departmentId, code, name }) =>
    id !== Number(code) ||
    String(departmentId).padStart(2, "0") !== code.slice(0, 2) ||
    name.length < 2 ||
    name !== name.trim() ||
    /[\u0000-\u001f\ufffd]/u.test(name),
);

if (
  rows.length !== 1119 ||
  codes.size !== 1119 ||
  departmentIds.size !== 33 ||
  invalidRows.length > 0
) {
  throw new Error(
    JSON.stringify(
      {
        rows: rows.length,
        uniqueCodes: codes.size,
        departments: departmentIds.size,
        invalidRows,
      },
      null,
      2,
    ),
  );
}

console.log(
  `Validated ${rows.length} unique municipality-level rows across ${departmentIds.size} departments.`,
);
