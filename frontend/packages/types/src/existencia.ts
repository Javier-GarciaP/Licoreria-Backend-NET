import type { Producto, ProductoVariante } from './index';

export const AVISO_SIN_ABRIR = 'Pide a barra que destape una botella';

/** Cómo se muestra la existencia de una variante y si se puede pedir ahora. */
export interface EstadoExistencia {
  /** Texto corto junto a la variante («Quedan 5», «3 porciones abiertas»…); null si no aplica. */
  etiqueta: string | null;
  /** Sin unidades ni forma de conseguirlas. */
  agotada: boolean;
  /** Porción sin envase abierto, pero con presentaciones cerradas que barra puede destapar. */
  sinAbrir: boolean;
  /** Indicación para el personal cuando no se puede pedir. */
  aviso: string | null;
  /** No se puede agregar al pedido ahora. */
  bloqueada: boolean;
}

/**
 * Unidades que se pueden pedir ahora: en una presentación, lo que hay menos lo reservado; en una porción,
 * las porciones de envases abiertos menos las reservadas. Devuelve `null` si la variante no lleva
 * inventario propio (receta heredada o cocina), para no mostrar un «agotado» falso.
 */
export function disponibleDeVariante(producto: Producto, variante: ProductoVariante): number | null {
  if (producto.areaDestino === 'Cocina') return null;
  if (variante.naturaleza === 'Porcion') return Math.max(0, variante.porcionesDisponibles ?? 0);
  if (!variante.esBase) return null;
  return Math.max(0, variante.cantidad - variante.cantidadReservada);
}

export function estadoExistencia(producto: Producto, variante: ProductoVariante): EstadoExistencia {
  const disponible = disponibleDeVariante(producto, variante);
  if (disponible === null) return { etiqueta: null, agotada: false, sinAbrir: false, aviso: null, bloqueada: false };

  if (variante.naturaleza === 'Porcion') {
    if (disponible > 0) {
      const etiqueta = `${disponible} ${disponible === 1 ? 'porción abierta' : 'porciones abiertas'}`;
      return { etiqueta, agotada: false, sinAbrir: false, aviso: null, bloqueada: false };
    }
    const cerradas = variante.envasesCerradosAbribles ?? 0;
    if (cerradas > 0) {
      return {
        etiqueta: `Sin abrir · ${cerradas} ${cerradas === 1 ? 'botella' : 'botellas'}`,
        agotada: false,
        sinAbrir: true,
        aviso: AVISO_SIN_ABRIR,
        bloqueada: true,
      };
    }
    return { etiqueta: 'Agotado', agotada: true, sinAbrir: false, aviso: null, bloqueada: true };
  }

  return disponible <= 0
    ? { etiqueta: 'Agotado', agotada: true, sinAbrir: false, aviso: null, bloqueada: true }
    : { etiqueta: `Quedan ${disponible}`, agotada: false, sinAbrir: false, aviso: null, bloqueada: false };
}

/** Variantes que se pueden vender (activas), en el orden en que las define el catálogo. */
export function variantesVendibles(producto: Producto): ProductoVariante[] {
  return producto.variantes.filter((variante) => variante.activo);
}

/** Texto de existencia para mostrar junto a una variante, o null si no aplica. */
export function etiquetaExistencia(producto: Producto, variante: ProductoVariante): string | null {
  return estadoExistencia(producto, variante).etiqueta;
}

/**
 * Agotado solo si todas las variantes vendibles son agotables y ninguna tiene unidades ni forma de
 * conseguirlas; si alguna no lleva inventario propio, no se puede afirmar.
 */
export function productoAgotado(producto: Producto): boolean {
  const vendibles = variantesVendibles(producto);
  if (vendibles.length === 0) return false;
  return vendibles.every((variante) => estadoExistencia(producto, variante).agotada);
}

/** Ninguna variante vendible se puede pedir ahora (agotado o a la espera de que barra destape). */
export function productoBloqueado(producto: Producto): boolean {
  const vendibles = variantesVendibles(producto);
  if (vendibles.length === 0) return false;
  return vendibles.every((variante) => estadoExistencia(producto, variante).bloqueada);
}
