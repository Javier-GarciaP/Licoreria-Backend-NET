import type { Plano, Zona } from '@licoreria/types';
import { clubApi } from '@licoreria/api-client';

/** Formas operativas de una mesa (valor que guarda la entidad `Mesa`). */
export const FORMAS_MESA = ['redonda', 'cuadrada', 'rectangular'] as const;
export type FormaMesa = (typeof FORMAS_MESA)[number];

export const ETIQUETA_FORMA: Record<FormaMesa, string> = {
  redonda: 'Redonda',
  cuadrada: 'Cuadrada',
  rectangular: 'Rectangular',
};

export function etiquetaForma(forma: string): string {
  return (ETIQUETA_FORMA as Record<string, string>)[forma] ?? forma;
}

/** Forma del elemento del plano a partir de la forma operativa de la mesa. */
export function formaElementoDeMesa(forma: string): string {
  switch (forma) {
    case 'cuadrada':
      return 'mesa_cuadrada';
    case 'rectangular':
      return 'mesa_rectangular';
    default:
      return 'mesa_redonda';
  }
}

/** Forma operativa de la mesa a partir de la forma del elemento del plano. */
export function formaMesaDeElemento(forma: string | null | undefined): FormaMesa {
  switch (forma) {
    case 'mesa_cuadrada':
      return 'cuadrada';
    case 'mesa_rectangular':
      return 'rectangular';
    default:
      return 'redonda';
  }
}

/**
 * Zona por defecto para las mesas. Si no existe ninguna zona, crea "General"
 * con ancho/alto 0 para que no dibuje una región en el mapa.
 */
export async function zonaPorDefecto(): Promise<Zona> {
  const zonas = await clubApi.zonas();
  if (zonas.length > 0) return zonas[0];
  return clubApi.crearZona({ nombre: 'General', tipo: 'Mesas', activo: true, color: null, posX: 0, posY: 0, ancho: 0, alto: 0 });
}

/** Planos que contienen un elemento enlazado a la mesa dada. */
export function planosDeMesa(planos: Plano[], mesaId: string): Plano[] {
  return planos.filter((p) => p.elementos.some((e) => e.mesaId === mesaId));
}