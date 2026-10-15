/** Periodos del Dashboard y de Reportes: se resuelven a fechas `YYYY-MM-DD` que viajan al API. */

export type PeriodoId = 'hoy' | '7d' | '30d' | 'mes' | 'personalizado';

export const PERIODOS: { id: PeriodoId; label: string }[] = [
  { id: 'hoy', label: 'Hoy' },
  { id: '7d', label: 'Últimos 7 días' },
  { id: '30d', label: 'Últimos 30 días' },
  { id: 'mes', label: 'Mes actual' },
  { id: 'personalizado', label: 'Personalizado' },
];

/** Días máximos que acepta el API en un periodo. */
export const DIAS_MAXIMOS = 366;

export const PERIODO_POR_DEFECTO: PeriodoId = '30d';

export interface PeriodoResuelto {
  id: PeriodoId;
  desde: string;
  hasta: string;
  /** false cuando el rango personalizado no se puede consultar (la interfaz no consulta y muestra el motivo). */
  valido: boolean;
  motivo: string | null;
}

/** Fecha local como `YYYY-MM-DD` (sin pasar por UTC, para no correr el día). */
export function fechaLocal(fecha: Date): string {
  return `${fecha.getFullYear()}-${String(fecha.getMonth() + 1).padStart(2, '0')}-${String(fecha.getDate()).padStart(2, '0')}`;
}

function restarDias(fecha: Date, dias: number): Date {
  const copia = new Date(fecha.getFullYear(), fecha.getMonth(), fecha.getDate());
  copia.setDate(copia.getDate() - dias);
  return copia;
}

const FECHA = /^\d{4}-\d{2}-\d{2}$/;

function diasEntre(desde: string, hasta: string): number {
  const [ya, ma, da] = desde.split('-').map(Number);
  const [yb, mb, db] = hasta.split('-').map(Number);
  return Math.round((Date.UTC(yb, mb - 1, db) - Date.UTC(ya, ma - 1, da)) / 86_400_000);
}

export function validarRango(desde: string, hasta: string): string | null {
  if (!FECHA.test(desde) || !FECHA.test(hasta)) return 'Elige la fecha inicial y la fecha final.';
  if (desde > hasta) return 'La fecha final no puede ser anterior a la inicial.';
  if (diasEntre(desde, hasta) + 1 > DIAS_MAXIMOS) return `El periodo no puede superar ${DIAS_MAXIMOS} días.`;
  return null;
}

export function esPeriodoId(valor: string | null): valor is PeriodoId {
  return PERIODOS.some((p) => p.id === valor);
}

/** Resuelve un periodo predefinido (o el rango personalizado indicado) a fechas concretas. */
export function resolverPeriodo(
  id: PeriodoId,
  hoy: Date,
  personalizado: { desde?: string | null; hasta?: string | null } = {},
): PeriodoResuelto {
  const fin = fechaLocal(hoy);

  switch (id) {
    case 'hoy':
      return { id, desde: fin, hasta: fin, valido: true, motivo: null };
    case '7d':
      return { id, desde: fechaLocal(restarDias(hoy, 6)), hasta: fin, valido: true, motivo: null };
    case '30d':
      return { id, desde: fechaLocal(restarDias(hoy, 29)), hasta: fin, valido: true, motivo: null };
    case 'mes':
      return {
        id,
        desde: fechaLocal(new Date(hoy.getFullYear(), hoy.getMonth(), 1)),
        hasta: fin,
        valido: true,
        motivo: null,
      };
    case 'personalizado': {
      const desde = personalizado.desde ?? '';
      const hasta = personalizado.hasta ?? '';
      const motivo = validarRango(desde, hasta);
      return { id, desde, hasta, valido: motivo === null, motivo };
    }
  }
}

/** Lee el periodo del estado de la URL (`periodo`, `desde`, `hasta`). */
export function leerPeriodo(params: URLSearchParams, hoy: Date = new Date()): PeriodoResuelto {
  const id = params.get('periodo');
  return resolverPeriodo(esPeriodoId(id) ? id : PERIODO_POR_DEFECTO, hoy, {
    desde: params.get('desde'),
    hasta: params.get('hasta'),
  });
}
