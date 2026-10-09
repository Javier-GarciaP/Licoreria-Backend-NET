import type { Producto } from '@licoreria/types';
import { API_BASE_URL } from './api';
import { formatUSD } from './format';

/** Convierte una URL de archivo (posiblemente relativa `/uploads/...`) en una URL absoluta. */
export function urlDeImagen(url: string | null | undefined): string | null {
  if (!url) return null;
  if (/^https?:\/\//i.test(url)) return url;
  return `${API_BASE_URL}${url.startsWith('/') ? '' : '/'}${url}`;
}

export interface MetricasProducto {
  variantes: number;
  precioMinUSD: number;
  precioMaxUSD: number;
  costoMinUSD: number;
  costoMaxUSD: number;
  margenPromedioUSD: number;
}

export function metricasDeProducto(producto: Producto): MetricasProducto {
  const precios = producto.variantes.map((v) => v.precioVentaUSD);
  const costos = producto.variantes.map((v) => v.precioCompraUSD);
  const precioMin = precios.length > 0 ? Math.min(...precios) : 0;
  const precioMax = precios.length > 0 ? Math.max(...precios) : 0;
  const costoMin = costos.length > 0 ? Math.min(...costos) : 0;
  const costoMax = costos.length > 0 ? Math.max(...costos) : 0;
  const margen =
    producto.variantes.length > 0
      ? producto.variantes.reduce((suma, v) => suma + (v.precioVentaUSD - v.precioCompraUSD), 0) /
        producto.variantes.length
      : 0;
  return { variantes: producto.variantes.length, precioMinUSD: precioMin, precioMaxUSD: precioMax, costoMinUSD: costoMin, costoMaxUSD: costoMax, margenPromedioUSD: margen };
}

export function formatoRango(min: number, max: number): string {
  if (min === max) return formatUSD(min);
  return `${formatUSD(min)} – ${formatUSD(max)}`;
}
