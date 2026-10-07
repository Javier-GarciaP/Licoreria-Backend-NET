import type { Mesa, Plano, PlanoElemento } from '@licoreria/types';
import type { EstadoMesaPlano } from '../components/mapa/MapaView';

const UUID_RE = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

/** ¿El id del elemento es un UUID real del servidor (y no uno temporal del editor)? */
export const esIdServidor = (id: string) => UUID_RE.test(id);

export function estadoDeMesa(mesa: Mesa): EstadoMesaPlano {
  if (!mesa.activa) return 'EnLimpieza';
  if (mesa.cuentaId) return 'Ocupada';
  if (mesa.reservada) return 'Reservada';
  return 'Libre';
}

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

/**
 * Plano que se muestra en mesas y mesonero: el marcado como activo; si ninguno
 * lo está, el primero de la lista; si no hay planos, uno sintético desde mesas.
 */
export function planoVisible(planos: Plano[], mesas: Mesa[]): Plano {
  const disenado = planos.find((p) => p.activo) ?? planos[0];
  if (disenado) return disenado;
  return planoDesdeMesas(mesas);
}

interface PlanoEditable {
  id: string;
  nombre: string;
  activo: boolean;
  anchoFondo: number;
  altoFondo: number;
  rejilla: number;
  piso: string;
  elementos: PlanoElemento[];
}

/** Cuerpo de `PUT /planos/:id` a partir de un plano (o borrador del editor). */
export function planoAPayload(plano: PlanoEditable) {
  return {
    id: plano.id,
    nombre: plano.nombre,
    activo: plano.activo,
    anchoFondo: plano.anchoFondo,
    altoFondo: plano.altoFondo,
    rejilla: plano.rejilla,
    piso: plano.piso,
    elementos: plano.elementos.map((e) => ({
      id: esIdServidor(e.id) ? e.id : undefined,
      zonaId: e.zonaId,
      mesaId: e.mesaId,
      tipo: e.tipo,
      forma: e.forma,
      color: e.color,
      etiqueta: e.etiqueta,
      z: e.z,
      posX: e.posX,
      posY: e.posY,
      ancho: e.ancho,
      alto: e.alto,
      rotacion: e.rotacion,
    })),
  };
}