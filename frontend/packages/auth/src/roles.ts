/**
 * Rol de dominio (persona) y su espacio de trabajo.
 * El rol de seguridad (`Admin`/`Employee`) es distinto: ver `rolDominio`.
 * Fuente única compartida por los apps de administración y servicio.
 */

/** Ruta inicial de cada rol dentro de su app. */
export const INICIO_POR_ROL: Record<string, string> = {
  Administrador: '/',
  Cajero: '/pos',
  Host: '/reservas',
  EditorContenido: '/contenido',
  Mesero: '/mesonero',
  Barra: '/kds',
  Cocina: '/kds',
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

/** Roles que operan en el app de servicio (mesonero/barra/cocina). */
export const ROLES_SERVICIO = ['Mesero', 'Barra', 'Cocina'] as const;

/** Ruta inicial sugerida para un rol de dominio. */
export function inicioDeRol(rolDominio?: string): string {
  return (rolDominio && INICIO_POR_ROL[rolDominio]) || '/';
}

/** Etiqueta legible del rol (dominio si existe; si no, el de seguridad). */
export function etiquetaRol(rolDominio?: string, rolSeguridad?: string): string {
  if (rolDominio && ETIQUETA_ROL[rolDominio]) return ETIQUETA_ROL[rolDominio];
  return rolSeguridad ?? '—';
}

/** ¿El rol pertenece al personal de servicio (Mesero/Barra/Cocina)? */
export function esRolServicio(rolDominio?: string): boolean {
  return Boolean(rolDominio && ROLES_SERVICIO.includes(rolDominio as (typeof ROLES_SERVICIO)[number]));
}

/** URL del app de servicio, configurable por entorno. */
export function urlServicio(): string {
  return (import.meta.env.VITE_SERVICIO_URL as string | undefined) ?? 'http://localhost:5175';
}

/** URL del app de administración, configurable por entorno. */
export function urlAdmin(): string {
  return (import.meta.env.VITE_ADMIN_URL as string | undefined) ?? 'http://localhost:5173';
}