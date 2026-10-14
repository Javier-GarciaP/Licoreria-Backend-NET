import type { Mesa, Plano, PlanoElemento } from '@licoreria/types';
import type { EstadoMesaPlano } from '../components/mapa/MapaView';

/** Plano sintético a partir de las mesas cuando aún no hay un mapa diseñado. */
export function planoDesdeMesas(mesas: Mesa[]): Plano {
  const maxX = Math.max(6, ...mesas.map((m) => m.posX + m.ancho + 1));
  const maxY = Math.max(4, ...mesas.map((m) => m.posY + m.alto + 1));
  const elementos: PlanoElemento[] = mesas.map((m, i) => ({
    id: `mesa-${m.id}`,
    zonaId: m.zonaId,
    mesaId: m.id,
    tipo: 'mesa',
    forma: m.forma === 'cuadrada' ? 'mesa_cuadrada' : m.forma === 'rectangular' ? 'mesa_rectangular' : 'mesa_redonda',
    color: null,
    etiqueta: m.numero,
    z: i,
    posX: m.posX,
    posY: m.posY,
    ancho: Math.max(1, m.ancho),
    alto: Math.max(1, m.alto),
    rotacion: 0,
  }));
  return {
    id: 'sintetico',
    nombre: 'Salón',
    version: 1,
    activo: true,
    anchoFondo: maxX,
    altoFondo: maxY,
    rejilla: 0.5,
    piso: 'madera',
    elementos,
  };
}

/** El plano activo; si no hay diseñado, uno sintético desde las mesas. */
export function planoVisible(planos: Plano[], mesas: Mesa[]): Plano {
  const disenado = planos.find((p) => p.activo) ?? planos[0];
  if (disenado) return disenado;
  return planoDesdeMesas(mesas);
}

/**
 * Estado visual de una mesa para la web pública, considerando la selección del cliente.
 * Cuando se filtra por una fecha futura, la ocupación actual (cuenta abierta) no debe
 * bloquear la mesa: puede reservarse para esa noche.
 */
export function estadoMesaPublic(
  mesa: Mesa,
  seleccionadas: ReadonlySet<string>,
  ignoraOcupacionActual = false,
): EstadoMesaPlano {
  if (!mesa.activa) return 'EnLimpieza';
  if (!ignoraOcupacionActual && mesa.cuentaId) return 'Ocupada';
  if (mesa.reservada) return 'Reservada';
  return seleccionadas.has(mesa.id) ? 'Seleccionada' : 'Libre';
}