import { createHash } from "node:crypto";
import { readFileSync, writeFileSync } from "node:fs";
import { inflateSync } from "node:zlib";

const departments = new Map([
  ["05", "Antioquia"],
  ["08", "Atlántico"],
  ["11", "Bogotá, D.C."],
  ["13", "Bolívar"],
  ["15", "Boyacá"],
  ["17", "Caldas"],
  ["18", "Caquetá"],
  ["19", "Cauca"],
  ["20", "Cesar"],
  ["23", "Córdoba"],
  ["25", "Cundinamarca"],
  ["27", "Chocó"],
  ["41", "Huila"],
  ["44", "La Guajira"],
  ["47", "Magdalena"],
  ["50", "Meta"],
  ["52", "Nariño"],
  ["54", "Norte de Santander"],
  ["63", "Quindío"],
  ["66", "Risaralda"],
  ["68", "Santander"],
  ["70", "Sucre"],
  ["73", "Tolima"],
  ["76", "Valle del Cauca"],
  ["81", "Arauca"],
  ["85", "Casanare"],
  ["86", "Putumayo"],
  ["88", "Archipiélago de San Andrés, Providencia y Santa Catalina"],
  ["91", "Amazonas"],
  ["94", "Guainía"],
  ["95", "Guaviare"],
  ["97", "Vaupés"],
  ["99", "Vichada"],
]);

const [, , sourcePath, outputPath = "001_colombia.sql"] = process.argv;

if (!sourcePath) {
  throw new Error(
    "Usage: node generate-colombia-seed.mjs <subregiones.pdf> [output.sql]",
  );
}

const source = readFileSync(sourcePath);
const sourceText = source.toString("latin1");
const pageStreams = [];
let streamPosition = 0;

while ((streamPosition = sourceText.indexOf("stream", streamPosition)) >= 0) {
  let streamStart = streamPosition + "stream".length;
  if (sourceText[streamStart] === "\r") streamStart += 1;
  if (sourceText[streamStart] === "\n") streamStart += 1;

  const streamEndMarker = sourceText.indexOf("endstream", streamStart);
  if (streamEndMarker < 0) break;

  let streamEnd = streamEndMarker;
  if (source[streamEnd - 1] === 0x0a) streamEnd -= 1;
  if (source[streamEnd - 1] === 0x0d) streamEnd -= 1;

  try {
    const inflated = inflateSync(source.subarray(streamStart, streamEnd)).toString(
      "latin1",
    );
    if (inflated.includes("CODIGO_MUNICIPIO")) pageStreams.push(inflated);
  } catch {
    // PDF resources such as fonts and images are not relevant to the table.
  }

  streamPosition = streamEndMarker + "endstream".length;
}

const tableText = pageStreams.join("\n");
const rowStarts = [...tableText.matchAll(/\[\((\d{5})\)/g)];
const literalPattern = /\((?:\\.|[^\\()])*\)/g;

function decodePdfLiteral(literal) {
  return literal
    .slice(1, -1)
    .replace(/\\([\\()])/g, "$1")
    .replace(/\\n/g, "\n")
    .replace(/\\r/g, "\r")
    .replace(/\\t/g, "\t")
    .trim();
}

function literalsFrom(text) {
  return [...text.matchAll(literalPattern)].map((match) =>
    decodePdfLiteral(match[0]),
  );
}

const municipalities = rowStarts.map((rowStart, index) => {
  const code = rowStart[1];
  const nextStart = rowStarts[index + 1]?.index ?? tableText.length;
  const rowChunk = tableText.slice(rowStart.index, nextStart);
  const firstArrayEnd = rowChunk.indexOf("]TJ");
  const firstArray = rowChunk.slice(0, firstArrayEnd + 3);
  let literals = literalsFrom(firstArray);

  // Wide names are split into multiple positioned text operators by the PDF.
  // The display name is the final literal before the row's first TD operator.
  if (literals.length < 3) {
    const firstRowAdvance = rowChunk.indexOf(" TD", firstArrayEnd);
    literals = literalsFrom(rowChunk.slice(0, firstRowAdvance));
  }

  const name = literals.at(-1);
  const departmentCode = code.slice(0, 2);

  if (!name || name === code || !departments.has(departmentCode)) {
    throw new Error(`Could not extract a valid row for DIVIPOLA code ${code}.`);
  }

  return { code, departmentCode, name };
});

const uniqueCodes = new Set(municipalities.map(({ code }) => code));
if (municipalities.length !== 1119 || uniqueCodes.size !== 1119) {
  throw new Error(
    `Expected 1,119 unique municipality-level rows; extracted ${municipalities.length} rows and ${uniqueCodes.size} unique codes.`,
  );
}

const observedDepartments = new Set(
  municipalities.map(({ departmentCode }) => departmentCode),
);
if (
  observedDepartments.size !== departments.size ||
  [...departments.keys()].some((code) => !observedDepartments.has(code))
) {
  throw new Error("The extracted department hierarchy is incomplete.");
}

const sqlLiteral = (value) => `'${value.replaceAll("'", "''")}'`;
const sourceSha256 = createHash("sha256").update(source).digest("hex");
const sourceReceivedOn = "2026-08-23";

const departmentValues = [...departments.entries()]
  .map(
    ([code, name]) =>
      `    (${Number(code)}, 170, '${code}', ${sqlLiteral(name)})`,
  )
  .join(",\n");

const municipalityValues = municipalities
  .sort((left, right) =>
    left.code < right.code ? -1 : left.code > right.code ? 1 : 0,
  )
  .map(
    ({ code, departmentCode, name }) =>
      `    (${Number(code)}, 170, ${Number(departmentCode)}, '${code}', ${sqlLiteral(name)})`,
  )
  .join(",\n");

const sql = `-- Colombia geographic reference seed.
-- Source artifact: Tabla de Municipios.xls exported as subregiones.pdf.
-- Source SHA-256: ${sourceSha256}
-- Source received: ${sourceReceivedOn}
-- Scope: 1 country, 33 department-level divisions, and 1,119 DIVIPOLA
-- municipality-level territorial units. See database/seed/README.md.

insert into app.countries (id, iso_alpha2, iso_alpha3, name)
values (170, 'CO', 'COL', 'Colombia')
on conflict (id) do update
set iso_alpha2 = excluded.iso_alpha2,
    iso_alpha3 = excluded.iso_alpha3,
    name = excluded.name;

insert into app.departments (id, country_id, code, name)
values
${departmentValues}
on conflict (id) do update
set country_id = excluded.country_id,
    code = excluded.code,
    name = excluded.name;

insert into app.municipalities (id, country_id, department_id, code, name)
values
${municipalityValues}
on conflict (id) do update
set country_id = excluded.country_id,
    department_id = excluded.department_id,
    code = excluded.code,
    name = excluded.name;

do $$
declare
    department_count integer;
    municipality_count integer;
begin
    select count(*)
    into department_count
    from app.departments
    where country_id = 170;

    select count(*)
    into municipality_count
    from app.municipalities
    where country_id = 170;

    if department_count <> 33 or municipality_count <> 1119 then
        raise exception
            using message = format(
                'Unexpected Colombia seed counts: %s departments, %s municipality-level units',
                department_count,
                municipality_count
            ),
            errcode = '23514';
    end if;
end;
$$;
`;

writeFileSync(outputPath, sql, "utf8");
console.log(
  `Generated ${outputPath} with ${departments.size} departments and ${municipalities.length} municipality-level rows from SHA-256 ${sourceSha256}.`,
);
