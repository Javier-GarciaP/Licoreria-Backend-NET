#!/usr/bin/env node
// Verifica que el contrato OpenAPI (openapi/licoreria.yaml) esté sincronizado
// con los controladores de la API. Falla si la cantidad de operaciones declaradas
// en el contrato no coincide con los atributos [Http*] de los controladores.
//
// Uso: node scripts/openapi/verificar-paridad.mjs

import { createRequire } from 'node:module';
import { readFile, readdir } from 'node:fs/promises';
import { existsSync } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const repoRoot = path.resolve(__dirname, '../..');
const controladoresDir = path.join(repoRoot, 'backend/src/Licoreria.WebAPI/Controllers');
const contratoPath = path.join(repoRoot, 'openapi/licoreria.yaml');

const require = createRequire(import.meta.url);

function cargarYaml() {
  try {
    return require('js-yaml');
  } catch {
    const fallback = path.join(repoRoot, 'docs/site/node_modules/js-yaml');
    if (existsSync(fallback)) {
      return require(fallback);
    }
    throw new Error('No se encontró "js-yaml". Ejecuta "npm install" en docs/site.');
  }
}

async function contarOperacionesControladores() {
  const archivos = (await readdir(controladoresDir)).filter((f) => f.endsWith('.cs'));
  const patron = /\[Http(?:Get|Post|Put|Patch|Delete)\b/g;
  let total = 0;

  for (const archivo of archivos) {
    const contenido = await readFile(path.join(controladoresDir, archivo), 'utf8');
    total += (contenido.match(patron) ?? []).length;
  }

  return total;
}

async function main() {
  if (!existsSync(contratoPath)) {
    throw new Error('No existe openapi/licoreria.yaml.');
  }

  const yaml = cargarYaml();
  const documento = yaml.load(await readFile(contratoPath, 'utf8'));
  const operacionesContrato = Object.values(documento.paths ?? {}).reduce(
    (total, item) => total + Object.keys(item).length,
    0,
  );
  const operacionesControladores = await contarOperacionesControladores();

  if (operacionesContrato !== operacionesControladores) {
    console.error(
      `[openapi] Contrato desincronizado: ${operacionesContrato} operaciones en el YAML ` +
        `vs ${operacionesControladores} en los controladores. ` +
        'Ejecuta "task backend:openapi".',
    );
    process.exit(1);
  }

  console.log(
    `[openapi] Paridad correcta: ${operacionesContrato} operaciones coinciden con los controladores.`,
  );
}

main().catch((err) => {
  console.error('[openapi] Error:', err.message);
  process.exit(1);
});
