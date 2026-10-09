export interface Notificacion {
  id: string;
  texto: string;
  cuando: number;
  leida: boolean;
  tipo: 'info' | 'exito';
}

const MÁXIMO = 50;

function claveDe(usuarioId: string): string {
  return `licoreria.notificaciones.${usuarioId}`;
}

/** Carga las notificaciones persistidas de un mesonero desde localStorage. */
export function cargarNotificaciones(usuarioId: string): Notificacion[] {
  try {
    const crudo = localStorage.getItem(claveDe(usuarioId));
    if (!crudo) return [];
    const parsed = JSON.parse(crudo) as unknown;
    return Array.isArray(parsed) ? (parsed as Notificacion[]).slice(0, MÁXIMO) : [];
  } catch {
    return [];
  }
}

/** Persiste las notificaciones de un mesonero en localStorage (máximo MÁXIMO). */
export function guardarNotificaciones(usuarioId: string, notificaciones: Notificacion[]): void {
  try {
    localStorage.setItem(claveDe(usuarioId), JSON.stringify(notificaciones.slice(0, MÁXIMO)));
  } catch {
    /* Almacenamiento no disponible; las notificaciones siguen en memoria. */
  }
}