/**
 * Rol de dominio (persona) y su espacio de trabajo.
 * El rol de seguridad (`Admin`/`Employee`) es distinto: ver `rolDominio`.
 */

export const INICIO_POR_ROL: Record<string, string> = {
  Administrador: '/',
  Cajero: '/pos',
  Mesero: '/mesonero',
  Barra: '/kds',
  Cocina: '/kds',
  Host: '/reservas',
  EditorContenido: '/contenido',
};

export const ETIQUETA_ROL: Record<string, string> = {
  Administrador: 'Administrador',
  Cajero: 'Cajero',
  Mesero: 'Mesero',
  Barra: 'Barra',
  Cocina: 'Cocina',
  Host: 'Host',
  EditorContenido: 'Editor de contenido',
};

/** Ruta inicial sugerida para un rol de dominio. */
export function inicioDeRol(rolDominio?: string): string {
  return (rolDominio && INICIO_POR_ROL[rolDominio]) || '/';
}

/** Etiqueta legible del rol (dominio si existe; si no, el de seguridad). */
export function etiquetaRol(rolDominio?: string, rolSeguridad?: string): string {
  if (rolDominio && ETIQUETA_ROL[rolDominio]) return ETIQUETA_ROL[rolDominio];
  return rolSeguridad ?? '—';
}

/** Cuentas sembradas para probar cada rol. */
export const CUENTAS_DEMO = [
  { rol: 'Administrador', email: 'admin@licoreria.com', password: 'admin123' },
  { rol: 'Cajero', email: 'cajero1@licoreria.com', password: 'cajero123' },
  { rol: 'Mesero', email: 'mesero1@licoreria.com', password: 'demo123' },
  { rol: 'Barra', email: 'barra1@licoreria.com', password: 'demo123' },
  { rol: 'Cocina', email: 'cocina1@licoreria.com', password: 'demo123' },
  { rol: 'Host', email: 'host1@licoreria.com', password: 'demo123' },
  { rol: 'Editor de contenido', email: 'editor1@licoreria.com', password: 'demo123' },
] as const;
