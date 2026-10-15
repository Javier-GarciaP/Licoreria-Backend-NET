import { describe, expect, it } from 'vitest';
import { fechaLocal, leerPeriodo, resolverPeriodo, validarRango } from './periodo';

// 14 de octubre de 2026 (hora local).
const HOY = new Date(2026, 9, 14, 15, 30);

describe('resolverPeriodo', () => {
  it('hoy: el mismo día', () => {
    expect(resolverPeriodo('hoy', HOY)).toMatchObject({ desde: '2026-10-14', hasta: '2026-10-14', valido: true });
  });

  it('últimos 7 días: incluye hoy y los 6 anteriores', () => {
    expect(resolverPeriodo('7d', HOY)).toMatchObject({ desde: '2026-10-08', hasta: '2026-10-14', valido: true });
  });

  it('últimos 30 días: incluye hoy y los 29 anteriores, cruzando de mes', () => {
    expect(resolverPeriodo('30d', HOY)).toMatchObject({ desde: '2026-09-15', hasta: '2026-10-14', valido: true });
  });

  it('mes actual: del primero del mes a hoy', () => {
    expect(resolverPeriodo('mes', HOY)).toMatchObject({ desde: '2026-10-01', hasta: '2026-10-14', valido: true });
  });

  it('personalizado válido conserva el rango', () => {
    expect(resolverPeriodo('personalizado', HOY, { desde: '2026-10-01', hasta: '2026-10-13' })).toMatchObject({
      desde: '2026-10-01',
      hasta: '2026-10-13',
      valido: true,
      motivo: null,
    });
  });

  it('personalizado con fecha final anterior a la inicial es inválido y explica el motivo', () => {
    const periodo = resolverPeriodo('personalizado', HOY, { desde: '2026-10-13', hasta: '2026-10-01' });

    expect(periodo.valido).toBe(false);
    expect(periodo.motivo).toMatch(/final no puede ser anterior/);
  });

  it('personalizado incompleto es inválido', () => {
    const periodo = resolverPeriodo('personalizado', HOY, { desde: '2026-10-01', hasta: '' });

    expect(periodo.valido).toBe(false);
    expect(periodo.motivo).toMatch(/Elige la fecha/);
  });

  it('personalizado de más de 366 días es inválido', () => {
    expect(validarRango('2025-01-01', '2026-10-14')).toMatch(/366/);
    expect(validarRango('2025-10-15', '2026-10-14')).toBeNull();
  });
});

describe('leerPeriodo', () => {
  it('sin parámetros usa los últimos 30 días', () => {
    expect(leerPeriodo(new URLSearchParams(), HOY).id).toBe('30d');
  });

  it('ignora un periodo desconocido', () => {
    expect(leerPeriodo(new URLSearchParams('periodo=ayer'), HOY).id).toBe('30d');
  });

  it('lee el rango personalizado de la URL', () => {
    const periodo = leerPeriodo(new URLSearchParams('periodo=personalizado&desde=2026-10-01&hasta=2026-10-05'), HOY);

    expect(periodo).toMatchObject({ id: 'personalizado', desde: '2026-10-01', hasta: '2026-10-05', valido: true });
  });
});

describe('fechaLocal', () => {
  it('no corre el día por la zona horaria', () => {
    expect(fechaLocal(new Date(2026, 0, 1, 0, 5))).toBe('2026-01-01');
    expect(fechaLocal(new Date(2026, 11, 31, 23, 55))).toBe('2026-12-31');
  });
});
