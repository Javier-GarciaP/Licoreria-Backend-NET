import type { Mesa } from '@licoreria/types';

export type EstadoMesaPlano = 'Libre' | 'Ocupada' | 'Reservada' | 'EnLimpieza';

export function estadoDeMesa(mesa: Mesa): EstadoMesaPlano {
  if (!mesa.activa) return 'EnLimpieza';
  if (mesa.cuentaId) return 'Ocupada';
  if (mesa.reservada) return 'Reservada';
  return 'Libre';
}
