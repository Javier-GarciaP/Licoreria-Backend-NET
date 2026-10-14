import type { MetodoPago } from '@licoreria/types';

/** Código del método de pago que genera una cuenta por cobrar (fiado). */
export const CODIGO_METODO_CREDITO = 'Credito';

/** Plazo que propone la interfaz; el servidor usa su propio valor si no se envía fecha. */
export const DIAS_CREDITO_PREDETERMINADO = 30;

export const esMetodoCredito = (metodo?: Pick<MetodoPago, 'codigo'> | null): boolean =>
  metodo?.codigo === CODIGO_METODO_CREDITO;

/** Fecha (AAAA-MM-DD, hora local) a `dias` días de hoy. */
export function fechaEnDias(dias: number, desde: Date = new Date()): string {
  const fecha = new Date(desde.getFullYear(), desde.getMonth(), desde.getDate() + dias);
  const mes = String(fecha.getMonth() + 1).padStart(2, '0');
  const dia = String(fecha.getDate()).padStart(2, '0');
  return `${fecha.getFullYear()}-${mes}-${dia}`;
}

/** Convierte una fecha de calendario en el instante ISO con que se envía el vencimiento (fin del día local). */
export function vencimientoIso(fecha: string): string {
  return new Date(`${fecha}T23:59:59`).toISOString();
}

/** Método de efectivo por defecto según la moneda del pago; si no existe, el primero disponible. */
export function metodoPredeterminado(metodos: MetodoPago[], moneda: 'USD' | 'BS'): MetodoPago | null {
  const codigo = moneda === 'BS' ? 'EfectivoBS' : 'EfectivoUSD';
  return metodos.find((metodo) => metodo.codigo === codigo) ?? metodos[0] ?? null;
}
