import type { AuditDetalle, AuditLog, AuditValores } from '@licoreria/types';

export const ACCIONES_CAMBIO = ['crear', 'editar', 'eliminar', 'restaurar'] as const;

export const ACCIONES_EVENTO = [
  'abrir',
  'cerrar',
  'registrar',
  'devolucion',
  'recepcion',
  'destapar',
  'descartar',
  'cambiar-password',
  'revocar-sesiones',
  'iniciar-sesion',
  'cerrar-sesion',
  'intento-fallido',
] as const;

export interface FilaDetalle {
  campo: string;
  antes: string;
  despues: string;
}

export function formatearValor(valor: unknown): string {
  if (valor === null || valor === undefined) return '—';
  if (typeof valor === 'boolean') return valor ? 'Sí' : 'No';
  if (typeof valor === 'object') return JSON.stringify(valor);
  return String(valor);
}

/** Interpreta `datos`: JSON de cambio (`antes`/`despues`), JSON libre de un evento o texto no JSON. */
export function interpretarDatos(datos: string | null): { detalle: AuditDetalle | null; libre: AuditValores | null } {
  if (!datos) return { detalle: null, libre: null };
  try {
    const valor: unknown = JSON.parse(datos);
    if (valor === null || typeof valor !== 'object' || Array.isArray(valor)) return { detalle: null, libre: null };
    const objeto = valor as Record<string, unknown>;
    if ('antes' in objeto || 'despues' in objeto) {
      return { detalle: objeto as AuditDetalle, libre: null };
    }
    return { detalle: null, libre: objeto };
  } catch {
    return { detalle: null, libre: null };
  }
}

/** Filas campo / antes / después de una entrada de cambio (altas sin «antes», bajas sin «después»). */
export function filasDeDetalle(detalle: AuditDetalle): FilaDetalle[] {
  const campos = [...new Set([...Object.keys(detalle.antes ?? {}), ...Object.keys(detalle.despues ?? {})])];
  return campos.map((campo) => ({
    campo,
    antes: detalle.antes ? formatearValor(detalle.antes[campo]) : '',
    despues: detalle.despues ? formatearValor(detalle.despues[campo]) : '',
  }));
}

export function esCambioDeEdicion(entrada: Pick<AuditLog, 'accion' | 'tipo'>): boolean {
  return entrada.tipo === 'Cambio' && entrada.accion === 'editar';
}

/** Rango de fechas local (YYYY-MM-DD) → instantes ISO que cubren el día completo. */
export function rangoIso(desde: string, hasta: string): { desde?: string; hasta?: string } {
  return {
    desde: desde ? new Date(`${desde}T00:00:00`).toISOString() : undefined,
    hasta: hasta ? new Date(`${hasta}T23:59:59.999`).toISOString() : undefined,
  };
}
