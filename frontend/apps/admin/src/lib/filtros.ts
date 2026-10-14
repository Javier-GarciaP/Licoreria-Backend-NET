/** Tamaño de página para filtros client-side: trae el universo y pagina en memoria. */
export const PAGE_SIZE_FILTRO_LOCAL = 500;

export function contiene(texto: string | null | undefined, busqueda: string): boolean {
  return !busqueda || (texto ?? '').toLowerCase().includes(busqueda.toLowerCase());
}

export function paginarEnMemoria<T>(items: T[], page: number, pageSize: number): { items: T[]; totalPages: number } {
  const desde = (page - 1) * pageSize;
  return { items: items.slice(desde, desde + pageSize), totalPages: Math.max(1, Math.ceil(items.length / pageSize)) };
}