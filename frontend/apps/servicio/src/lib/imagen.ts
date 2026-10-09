import { API_BASE_URL } from './api';

/**
 * Resuelve la URL de la imagen de un producto.
 * Las rutas relativas (`/uploads/productos/x.jpg`) se anteponen al origen del API,
 * porque en el navegador resolverían contra el puerto de Vite y devolverían el HTML
 * de la SPA en lugar de la imagen.
 */
export function urlDeImagen(url: string | null | undefined): string | null {
  if (!url) return null;
  if (/^https?:\/\//i.test(url) || url.startsWith('data:')) return url;
  const base = API_BASE_URL.replace(/\/+$/, '');
  return `${base}${url.startsWith('/') ? url : `/${url}`}`;
}
