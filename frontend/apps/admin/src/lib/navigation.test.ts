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

describe('Auditoría en la navegación', () => {
  const rutasDe = (admin: boolean, modo?: Modo) =>
    filtrarGrupos(admin, admin ? conPermiso : sinPermiso, admin ? 'Administrador' : 'Cajero', modo).flatMap((g) =>
      g.items.map((i) => i.to),
    );

  it('el administrador ve Auditoría en el menú de Analítica, sin Reportes', () => {
    for (const modo of ['licoreria', 'discoteca'] as const) {
      const rutas = rutasDe(true, modo);
      expect(rutas).toContain('/auditoria');
      expect(rutas).not.toContain('/reportes');
    }
    const analitica = filtrarGrupos(true, conPermiso, 'Administrador').find((g) => g.id === 'analitica');
    expect(analitica?.items.map((i) => i.to)).toEqual(['/auditoria']);
  });

  it('el cajero no ve Auditoría ni puede acceder a su ruta', () => {
    expect(rutasDe(false)).not.toContain('/auditoria');
    expect(puedeAcceder(itemDeRuta('/auditoria'), false, conPermiso, 'Cajero')).toBe(false);
  });
});

describe('Módulos complementarios por modo y rol', () => {
  // Permisos efectivos por rol (reflejan la matriz del backend para estos módulos).
  const PERMISOS: Record<string, string[]> = {
    Administrador: [],
    Cajero: ['sales:read', 'sales:write', 'inventory:read', 'crm:read', 'crm:write', 'catalog:read'],
    Mesero: ['sales:read', 'sales:write', 'inventory:read', 'club:read'],
    Host: ['club:read', 'club:manage', 'reservation:manage', 'content:read'],
  };

  const rutas = (rol: string, modo: Modo) => {
    const admin = rol === 'Administrador';
    return filtrarGrupos(admin, (c) => PERMISOS[rol].includes(c), rol, modo).flatMap((g) => g.items.map((i) => i.to));
  };

  it('el cajero en licorería ve Existencias, Clientes y Cuentas por cobrar, y no Compras ni Promociones', () => {
    const r = rutas('Cajero', 'licoreria');
    expect(r).toEqual(expect.arrayContaining(['/inventario', '/clientes', '/cxc']));
    expect(r).not.toContain('/compras');
    expect(r).not.toContain('/promociones');
  });

  it('el administrador ve en ambos modos Promociones, Existencias, Clientes, Cuentas por cobrar y Compras', () => {
    for (const modo of ['licoreria', 'discoteca'] as const) {
      expect(rutas('Administrador', modo)).toEqual(
        expect.arrayContaining(['/promociones', '/inventario', '/clientes', '/cxc', '/compras']),
      );
    }
  });

  it('Entradas y Lista VIP solo aparecen en discoteca para administrador y host', () => {
    for (const rol of ['Administrador', 'Host']) {
      expect(rutas(rol, 'discoteca')).toEqual(expect.arrayContaining(['/entradas', '/vip']));
      expect(rutas(rol, 'licoreria')).not.toContain('/entradas');
      expect(rutas(rol, 'licoreria')).not.toContain('/vip');
    }
    expect(puedeAcceder(itemDeRuta('/vip'), true, conPermiso, undefined, 'licoreria')).toBe(false);
    expect(rutas('Cajero', 'discoteca')).not.toContain('/vip');
  });

  it('el mesero no ve Clientes ni Promociones y no accede por dirección directa', () => {
    const r = rutas('Mesero', 'discoteca');
    expect(r).not.toContain('/clientes');
    expect(r).not.toContain('/promociones');
    expect(puedeAcceder(itemDeRuta('/clientes'), false, (c) => PERMISOS.Mesero.includes(c), 'Mesero', 'discoteca')).toBe(false);
  });

  it('los lotes, las tomas físicas y el contenido siguen fuera del menú', () => {
    for (const modo of ['licoreria', 'discoteca'] as const) {
      const r = rutas('Administrador', modo);
      expect(r).not.toContain('/inventario/lotes');
      expect(r).not.toContain('/inventario/tomas');
      expect(r).not.toContain('/contenido');
    }
  });
});
