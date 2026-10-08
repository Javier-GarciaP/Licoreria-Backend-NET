/** Roles de dominio que operan en el app de servicio. */
export const ROLES_SERVICIO = ['Mesero', 'Barra', 'Cocina'] as const;

export const ETIQUETA_ROL: Record<string, string> = {
  Mesero: 'Mesero',
  Barra: 'Barra',
  Cocina: 'Cocina',
};

/** Ruta de aterrizaje según el rol de dominio dentro del app de servicio. */
export const INICIO_POR_ROL: Record<string, string> = {
  Mesero: '/mesonero',
  Barra: '/kds',
  Cocina: '/kds',
};

export function inicioDeRol(rolDominio?: string): string {
  return (rolDominio && INICIO_POR_ROL[rolDominio]) || '/';
}

export function etiquetaRol(rolDominio?: string): string {
  return (rolDominio && ETIQUETA_ROL[rolDominio]) || 'Personal';
}

/** ¿El rol opera en este app? (Mesero/Barra/Cocina) */
export function esRolServicio(rolDominio?: string): boolean {
  return Boolean(rolDominio && ROLES_SERVICIO.includes(rolDominio as (typeof ROLES_SERVICIO)[number]));
}

export const CUENTAS_DEMO = [
  { rol: 'Mesero', email: 'mesero1@licoreria.com', password: 'demo123' },
  { rol: 'Barra', email: 'barra1@licoreria.com', password: 'demo123' },
  { rol: 'Cocina', email: 'cocina1@licoreria.com', password: 'demo123' },
] as const;
