export const formatUSD = (value: number) =>
  new Intl.NumberFormat('es-VE', { style: 'currency', currency: 'USD', minimumFractionDigits: 2 }).format(
    Number(value ?? 0),
  );

export const formatBS = (value: number) =>
  `Bs ${new Intl.NumberFormat('es-VE', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(
    Number(value ?? 0),
  )}`;

export const formatFecha = (iso: string) => {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return '—';
  return date.toLocaleDateString('es-VE', { dateStyle: 'long' });
};
