import type { ComandaDetalle, Cuenta, EstadoItemComanda } from '@licoreria/types';

export interface DetalleConPago {
  detalle: ComandaDetalle;
  comandaId: string;
  area: string;
  /** Monto ya cubierto por abonos. */
  pagado: number;
  /** Monto pendiente por cubrir. */
  pendiente: number;
  /** `true` si el monto proviene del backend (`pagadoUSD`). */
  origenBackend: boolean;
}

const CANCELADOS: EstadoItemComanda[] = ['Cancelado'];

/**
 * Aplana los consumos de la cuenta y calcula cuánto está pagado por ítem.
 * Si el backend aún no expone `pagadoUSD`, se reparte el total abonado en FIFO.
 */
export function detallesConPago(cuenta: Cuenta): DetalleConPago[] {
  const base: Omit<DetalleConPago, 'pagado' | 'pendiente' | 'origenBackend'>[] = [];
  for (const comanda of cuenta.comandas) {
    for (const detalle of comanda.detalles) {
      if (CANCELADOS.includes(detalle.estado)) continue;
      base.push({ detalle, comandaId: comanda.id, area: comanda.area });
    }
  }

  const hayPagoBackend = base.some((item) => typeof item.detalle.pagadoUSD === 'number');
  if (hayPagoBackend) {
    return base.map((item) => {
      const pagado = Math.min(item.detalle.subtotalUSD, Math.max(0, item.detalle.pagadoUSD ?? 0));
      return { ...item, pagado, pendiente: Math.max(0, item.detalle.subtotalUSD - pagado), origenBackend: true };
    });
  }

  let restante = Math.max(0, cuenta.totalAbonado);
  return base.map((item) => {
    const pagado = Math.min(item.detalle.subtotalUSD, restante);
    restante -= pagado;
    return { ...item, pagado, pendiente: Math.max(0, item.detalle.subtotalUSD - pagado), origenBackend: false };
  });
}

export const saldoPendienteSeleccion = (items: DetalleConPago[]) =>
  items.reduce((total, item) => total + item.pendiente, 0);
