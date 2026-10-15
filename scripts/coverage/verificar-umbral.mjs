// Verifica la cobertura de líneas combinada de los ensamblados de negocio (Domain y Application)
// contra el umbral versionado en umbral.json. Lee el Summary.json que genera ReportGenerator.
//
// Uso: node scripts/coverage/verificar-umbral.mjs [ruta/Summary.json] [ruta/umbral.json]
// Salida: código 0 si se alcanza el umbral; 1 si queda por debajo; 2 si faltan datos o hay un error de uso.
import { readFileSync } from 'node:fs';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const aquí = dirname(fileURLToPath(import.meta.url));
const RESUMEN_POR_DEFECTO = resolve(aquí, '../../artifacts/coverage/report/Summary.json');
const UMBRAL_POR_DEFECTO = resolve(aquí, 'umbral.json');

/** Suma líneas cubiertas y cubribles de los ensamblados indicados. */
export function medirCobertura(resumen, ensamblados) {
  const presentes = resumen?.coverage?.assemblies ?? [];
  const faltantes = ensamblados.filter((nombre) => !presentes.some((a) => a.name === nombre));
  if (faltantes.length > 0) {
    throw new Error(`El resumen no incluye los ensamblados: ${faltantes.join(', ')}.`);
  }

  const elegidos = presentes.filter((a) => ensamblados.includes(a.name));
  const cubiertas = elegidos.reduce((suma, a) => suma + a.coveredlines, 0);
  const cubribles = elegidos.reduce((suma, a) => suma + a.coverablelines, 0);
  if (cubribles === 0) {
    throw new Error('Los ensamblados medidos no tienen líneas cubribles.');
  }

  return { cubiertas, cubribles, porcentaje: (cubiertas / cubribles) * 100 };
}

/** Compara con el umbral y devuelve el resultado y el mensaje a mostrar. */
export function verificar(resumen, umbral) {
  const { cubiertas, cubribles, porcentaje } = medirCobertura(resumen, umbral.ensamblados);
  const medido = porcentaje.toFixed(1);
  const exigido = Number(umbral.lineas).toFixed(1);
  const detalle = `${cubiertas}/${cubribles} líneas de ${umbral.ensamblados.join(' + ')}`;

  return porcentaje + 1e-9 >= umbral.lineas
    ? { ok: true, mensaje: `Cobertura suficiente: ${medido} % medido frente a ${exigido} % exigido (${detalle}).` }
    : { ok: false, mensaje: `Cobertura insuficiente: ${medido} % medido frente a ${exigido} % exigido (${detalle}).` };
}

function principal() {
  const [rutaResumen = RESUMEN_POR_DEFECTO, rutaUmbral = UMBRAL_POR_DEFECTO] = process.argv.slice(2);
  try {
    const resumen = JSON.parse(readFileSync(rutaResumen, 'utf8'));
    const umbral = JSON.parse(readFileSync(rutaUmbral, 'utf8'));
    const { ok, mensaje } = verificar(resumen, umbral);
    (ok ? console.log : console.error)(mensaje);
    process.exit(ok ? 0 : 1);
  } catch (error) {
    console.error(`No se pudo verificar el umbral: ${error.message}`);
    process.exit(2);
  }
}

if (process.argv[1] && resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  principal();
}
