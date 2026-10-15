// Pruebas del verificador de umbral. Ejecutar con: node --test scripts/coverage
import assert from 'node:assert/strict';
import { spawnSync } from 'node:child_process';
import { mkdtempSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { test } from 'node:test';
import { fileURLToPath } from 'node:url';
import { medirCobertura, verificar } from './verificar-umbral.mjs';

const ensamblado = (name, coveredlines, coverablelines) => ({ name, coveredlines, coverablelines });
const resumen = (...assemblies) => ({ coverage: { assemblies } });
const umbral = { lineas: 70, ensamblados: ['Licoreria.Domain', 'Licoreria.Application'] };

test('suma solo los ensamblados indicados', () => {
  const r = resumen(
    ensamblado('Licoreria.Domain', 50, 100),
    ensamblado('Licoreria.Application', 90, 100),
    ensamblado('Licoreria.Infrastructure', 0, 1000),
  );
  assert.deepEqual(medirCobertura(r, umbral.ensamblados), { cubiertas: 140, cubribles: 200, porcentaje: 70 });
});

test('por debajo del umbral falla indicando medido y exigido', () => {
  const r = resumen(ensamblado('Licoreria.Domain', 65, 100), ensamblado('Licoreria.Application', 65, 100));
  const { ok, mensaje } = verificar(r, umbral);
  assert.equal(ok, false);
  assert.match(mensaje, /65\.0 % medido frente a 70\.0 % exigido/);
});

test('por encima del umbral pasa', () => {
  const r = resumen(ensamblado('Licoreria.Domain', 80, 100), ensamblado('Licoreria.Application', 75, 100));
  const { ok, mensaje } = verificar(r, umbral);
  assert.equal(ok, true);
  assert.match(mensaje, /77\.5 % medido frente a 70\.0 % exigido/);
});

test('justo en el umbral pasa', () => {
  const r = resumen(ensamblado('Licoreria.Domain', 70, 100), ensamblado('Licoreria.Application', 70, 100));
  assert.equal(verificar(r, umbral).ok, true);
});

test('un ensamblado ausente es un error y no un 100 %', () => {
  assert.throws(() => medirCobertura(resumen(ensamblado('Licoreria.Domain', 1, 1)), umbral.ensamblados), /Licoreria\.Application/);
});

test('la línea de órdenes sale con 1 bajo el umbral y con 0 sobre él', () => {
  const dir = mkdtempSync(join(tmpdir(), 'umbral-'));
  const archivoUmbral = join(dir, 'umbral.json');
  writeFileSync(archivoUmbral, JSON.stringify(umbral));
  const script = fileURLToPath(new URL('./verificar-umbral.mjs', import.meta.url));
  const ejecutar = (assemblies) => {
    const archivoResumen = join(dir, 'Summary.json');
    writeFileSync(archivoResumen, JSON.stringify(resumen(...assemblies)));
    return spawnSync(process.execPath, [script, archivoResumen, archivoUmbral], { encoding: 'utf8' });
  };

  const bajo = ejecutar([ensamblado('Licoreria.Domain', 10, 100), ensamblado('Licoreria.Application', 10, 100)]);
  assert.equal(bajo.status, 1);
  assert.match(bajo.stderr, /10\.0 % medido frente a 70\.0 % exigido/);

  const alto = ejecutar([ensamblado('Licoreria.Domain', 90, 100), ensamblado('Licoreria.Application', 90, 100)]);
  assert.equal(alto.status, 0);
});
