import { describe, expect, it } from 'vitest';
import { filtrarGrupos, puedeAcceder, itemDeRuta, type Modo } from './navigation';

const esAdmin = true;
const sinPermiso = () => false;
const conPermiso = () => true;

function rutasVisibles(modo?: Modo): string[] {
  return filtrarGrupos(esAdmin, conPermiso, undefined, modo).flatMap((g) => g.items.map((i) => i.to));
}

describe('filtrarGrupos por modo de operación', () => {
  it('modo licorería oculta salón, mesas y cuentas', () => {
    const rutas = rutasVisibles('licoreria');
    expect(rutas).not.toContain('/reservas');
    expect(rutas).not.toContain('/eventos');
    expect(rutas).not.toContain('/salon');
    expect(rutas).not.toContain('/plano');
    expect(rutas).not.toContain('/cuentas');
  });

  it('modo licorería conserva dashboard, POS y venta de mostrador', () => {
    const rutas = rutasVisibles('licoreria');
    expect(rutas).toContain('/');
    expect(rutas).toContain('/pos');
    expect(rutas).toContain('/ventas');
  });

  it('modo discoteca muestra la navegación completa', () => {
    const rutas = rutasVisibles('discoteca');
    expect(rutas).toContain('/reservas');
    expect(rutas).toContain('/plano');
    expect(rutas).toContain('/cuentas');
    expect(rutas).toContain('/pos');
    expect(rutas).toContain('/');
  });

  it('sin modo no aplica filtro de modo (compatibilidad)', () => {
    expect(rutasVisibles()).toEqual(rutasVisibles('discoteca'));
  });

  it('no deja grupos vacíos tras filtrar', () => {
    for (const modo of ['licoreria', 'discoteca'] as const) {
      for (const grupo of filtrarGrupos(esAdmin, conPermiso, undefined, modo)) {
        expect(grupo.items.length).toBeGreaterThan(0);
      }
    }
  });
});

describe('puedeAcceder por modo', () => {
  it('redirige rutas de salón en modo licorería aunque sea admin', () => {
    const reservas = itemDeRuta('/reservas');
    expect(puedeAcceder(reservas, esAdmin, conPermiso, undefined, 'licoreria')).toBe(false);
    expect(puedeAcceder(reservas, esAdmin, conPermiso, undefined, 'discoteca')).toBe(true);
  });

  it('permite el dashboard y el POS en ambos modos', () => {
    for (const modo of ['licoreria', 'discoteca'] as const) {
      expect(puedeAcceder(itemDeRuta('/'), esAdmin, conPermiso, undefined, modo)).toBe(true);
      expect(puedeAcceder(itemDeRuta('/pos'), esAdmin, conPermiso, undefined, modo)).toBe(true);
    }
  });

  it('sin modo no considera la restricción de modo', () => {
    expect(puedeAcceder(itemDeRuta('/reservas'), esAdmin, conPermiso, undefined)).toBe(true);
  });

  it('rutas sin ítem de nav se permiten', () => {
    expect(puedeAcceder(undefined, esAdmin, sinPermiso, undefined, 'licoreria')).toBe(true);
  });
});
