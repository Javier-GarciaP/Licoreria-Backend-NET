import { describe, expect, it } from 'vitest';
import { filasDeDetalle, interpretarDatos, rangoIso } from './auditoria';

describe('interpretarDatos', () => {
  it('reconoce el detalle antes/después de un cambio', () => {
    const { detalle, libre } = interpretarDatos('{"antes":{"PrecioVentaUSD":10},"despues":{"PrecioVentaUSD":12}}');
    expect(detalle?.antes).toEqual({ PrecioVentaUSD: 10 });
    expect(libre).toBeNull();
  });

  it('trata un JSON sin antes/después como datos libres de un evento', () => {
    const { detalle, libre } = interpretarDatos('{"esperado":100,"contado":95}');
    expect(detalle).toBeNull();
    expect(libre).toEqual({ esperado: 100, contado: 95 });
  });

  it('tolera datos vacíos o inválidos', () => {
    expect(interpretarDatos(null)).toEqual({ detalle: null, libre: null });
    expect(interpretarDatos('no es json')).toEqual({ detalle: null, libre: null });
  });
});

describe('filasDeDetalle', () => {
  it('muestra antes y después de cada campo editado', () => {
    expect(filasDeDetalle({ antes: { Precio: 10 }, despues: { Precio: 12 } })).toEqual([
      { campo: 'Precio', antes: '10', despues: '12' },
    ]);
  });

  it('un alta solo tiene valores nuevos y una baja solo anteriores', () => {
    expect(filasDeDetalle({ despues: { Nombre: 'Ron', Activo: true } })).toEqual([
      { campo: 'Nombre', antes: '', despues: 'Ron' },
      { campo: 'Activo', antes: '', despues: 'Sí' },
    ]);
    expect(filasDeDetalle({ antes: { Nombre: null } })).toEqual([{ campo: 'Nombre', antes: '—', despues: '' }]);
  });
});

describe('rangoIso', () => {
  it('cubre el día completo en hora local', () => {
    const { desde, hasta } = rangoIso('2026-10-14', '2026-10-14');
    expect(new Date(desde!).getTime()).toBe(new Date('2026-10-14T00:00:00').getTime());
    expect(new Date(hasta!).getTime()).toBe(new Date('2026-10-14T23:59:59.999').getTime());
  });

  it('omite los extremos vacíos', () => {
    expect(rangoIso('', '')).toEqual({ desde: undefined, hasta: undefined });
  });
});
