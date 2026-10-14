const usdFormatter = new Intl.NumberFormat('es-VE', {
  style: 'currency',
  currency: 'USD',
  minimumFractionDigits: 2,
});

const bsFormatter = new Intl.NumberFormat('es-VE', {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

export const formatUSD = (value: number) => usdFormatter.format(Number(value ?? 0));
export const formatBS = (value: number) => `Bs ${bsFormatter.format(Number(value ?? 0))}`;
export const formatNumber = (value: number) => new Intl.NumberFormat('es-VE').format(Number(value ?? 0));

export function formatDateTime(iso: string): string {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return '—';
  return date.toLocaleString('es-VE', { dateStyle: 'short', timeStyle: 'short' });
}

export function formatTime(iso: string): string {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return '—';
  return date.toLocaleTimeString('es-VE', { hour: '2-digit', minute: '2-digit' });
}

/** Minutos transcurridos desde una fecha ISO. */
export function minutosTranscurridos(iso: string): number {
  const inicio = new Date(iso).getTime();
  if (Number.isNaN(inicio)) return 0;
  return Math.max(0, Math.floor((Date.now() - inicio) / 60000));
}

export function haceCuanto(iso: string): string {
  const minutos = minutosTranscurridos(iso);
  if (minutos < 1) return 'ahora';
  if (minutos < 60) return `hace ${minutos} min`;
  const horas = Math.floor(minutos / 60);
  return `hace ${horas} h`;
}
