import { API_BASE_URL } from './api';

/**
 * Resuelve una URL de imagen: las relativas (p. ej. `/uploads/eventos/x.jpg`)
 * se anteponen al origen del API; las absolutas se dejan intactas.
 */
export function urlDeImagen(url: string | null | undefined): string | undefined {
  if (!url) return undefined;
  if (/^https?:\/\//i.test(url) || url.startsWith('data:')) return url;
  const base = API_BASE_URL.replace(/\/+$/, '');
  return `${base}${url.startsWith('/') ? url : `/${url}`}`;
}