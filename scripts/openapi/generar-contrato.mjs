#!/usr/bin/env node
// Convierte el documento OpenAPI generado por la API (openapi/generated/*.json)
// al contrato YAML versionado (openapi/licoreria.yaml).
//
// Requiere haber ejecutado antes:
//   dotnet build backend/src/Licoreria.WebAPI/Licoreria.WebAPI.csproj -t:GenerateOpenApiDocuments
//
// Uso: node scripts/openapi/generar-contrato.mjs

import { createRequire } from 'node:module';
import { readFile, writeFile, readdir, mkdir } from 'node:fs/promises';
import { existsSync } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const repoRoot = path.resolve(__dirname, '../..');
const generatedDir = path.join(repoRoot, 'openapi/generated');
const outputPath = path.join(repoRoot, 'openapi/licoreria.yaml');

const require = createRequire(import.meta.url);

function cargarYaml() {
  try {
    return require('js-yaml');
  } catch {
    // js-yaml vive como dependencia del sitio de documentación.
    const fallback = path.join(repoRoot, 'docs/site/node_modules/js-yaml');
    if (existsSync(fallback)) {
      return require(fallback);
    }
    throw new Error(
      'No se encontró "js-yaml". Ejecuta "npm install" en docs/site o instala js-yaml.',
    );
  }
}

async function main() {
  if (!existsSync(generatedDir)) {
    throw new Error(`No existe ${generatedDir}. Ejecuta primero la tarea de generación.`);
  }

  const archivos = (await readdir(generatedDir)).filter((f) => f.endsWith('.json'));
  if (archivos.length === 0) {
    throw new Error(`No hay documentos generados en ${generatedDir}.`);
  }

  const jsonPath = path.join(generatedDir, archivos[0]);
  const documento = JSON.parse(await readFile(jsonPath, 'utf8'));

  const yaml = cargarYaml();
  const contenido = yaml.dump(documento, { noRefs: true, lineWidth: 120, sortKeys: false });

  await mkdir(path.dirname(outputPath), { recursive: true });
  await writeFile(outputPath, contenido, 'utf8');

  const operaciones = Object.values(documento.paths ?? {}).reduce(
    (total, item) => total + Object.keys(item).length,
    0,
  );

  console.log(
    `[openapi] Contrato escrito en ${path.relative(repoRoot, outputPath)} ` +
      `(${Object.keys(documento.paths ?? {}).length} rutas, ${operaciones} operaciones).`,
  );
}

main().catch((err) => {
  console.error('[openapi] Error:', err.message);
  process.exit(1);
});
