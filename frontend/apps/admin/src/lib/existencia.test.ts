import { describe, expect, it } from 'vitest';
import { estadoExistencia, productoAgotado, productoBloqueado } from '@licoreria/types';
import type { Producto, ProductoVariante } from '@licoreria/types';

const variante = (extra: Partial<ProductoVariante>): ProductoVariante =>
  ({
    id: 'v',
    nombre: 'V',
    activo: true,
    esBase: false,
    naturaleza: 'SinDefinir',
    cantidad: 0,
    cantidadReservada: 0,
    porcionesDisponibles: 0,
    porcionesPotenciales: 0,
    envasesCerradosAbribles: 0,
    ...extra,
  }) as ProductoVariante;

const producto = (variantes: ProductoVariante[]) => ({ areaDestino: 'Barra', variantes }) as unknown as Producto;

describe('estadoExistencia', () => {
  it('presentación con existencia: «Quedan N» descontando lo reservado', () => {
    const v = variante({ esBase: true, naturaleza: 'Presentacion', cantidad: 7, cantidadReservada: 2 });
    expect(estadoExistencia(producto([v]), v)).toMatchObject({ etiqueta: 'Quedan 5', bloqueada: false });
  });

  it('presentación sin unidades: Agotado y bloqueada', () => {
    const v = variante({ esBase: true, naturaleza: 'Presentacion', cantidad: 2, cantidadReservada: 2 });
    expect(estadoExistencia(producto([v]), v)).toMatchObject({ etiqueta: 'Agotado', agotada: true, bloqueada: true });
  });

  it('vaso con envase abierto: porciones abiertas', () => {
    const v = variante({ naturaleza: 'Porcion', porcionesDisponibles: 3 });
    expect(estadoExistencia(producto([v]), v)).toMatchObject({ etiqueta: '3 porciones abiertas', bloqueada: false });
  });

  it('vaso sin abrir pero con botellas cerradas: aviso para barra y no se puede pedir', () => {
    const v = variante({ naturaleza: 'Porcion', envasesCerradosAbribles: 2 });
    expect(estadoExistencia(producto([v]), v)).toMatchObject({
      etiqueta: 'Sin abrir · 2 botellas',
      sinAbrir: true,
      aviso: 'Pide a barra que destape una botella',
      bloqueada: true,
      agotada: false,
    });
  });

  it('vaso sin envases ni botellas: Agotado', () => {
    const v = variante({ naturaleza: 'Porcion' });
    expect(estadoExistencia(producto([v]), v)).toMatchObject({ etiqueta: 'Agotado', agotada: true });
  });

  it('receta heredada no muestra existencia', () => {
    const v = variante({ naturaleza: 'Receta' });
    expect(estadoExistencia(producto([v]), v)).toMatchObject({ etiqueta: null, bloqueada: false });
  });

  it('un producto solo con vasos sin abrir está bloqueado pero no agotado', () => {
    const p = producto([variante({ naturaleza: 'Porcion', envasesCerradosAbribles: 1 })]);
    expect(productoBloqueado(p)).toBe(true);
    expect(productoAgotado(p)).toBe(false);
  });
});
