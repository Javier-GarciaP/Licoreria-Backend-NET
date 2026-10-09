import { motion } from 'framer-motion';
import { X } from 'lucide-react';
import { Pill } from '@licoreria/ui';
import type { Producto } from '@licoreria/types';
import { formatoRango, metricasDeProducto, urlDeImagen } from '../../lib/imagenProducto';
import { formatNumber, formatUSD } from '../../lib/format';
import { CodigoBarras } from './CodigoBarras';
import { ConsumoVariante } from './ConsumoVariante';

function Metrica({ etiqueta, valor }: { etiqueta: string; valor: string }) {
  return (
    <div className="rounded-inner border border-border bg-muted/30 px-3 py-2">
      <p className="text-[11px] uppercase tracking-tighter2 text-muted-foreground">{etiqueta}</p>
      <p className="num mt-0.5 text-sm text-foreground">{valor}</p>
    </div>
  );
}

/** Panel expandido de producto al hacer clic en una fila (sin recuadro, directo en la fila). */
export function PanelDetalleProducto({ producto, imagenId, onCerrar }: { producto: Producto; imagenId: string; onCerrar: () => void }) {
  const metricas = metricasDeProducto(producto);
  const imagen = urlDeImagen(producto.imagenUrl);

  return (
    <div className="flex flex-col gap-5">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <div className="flex flex-wrap items-center gap-2">
          <Pill>{producto.tipo}</Pill>
          <Pill tone={producto.areaDestino === 'Barra' ? 'accent' : 'info'}>{producto.areaDestino}</Pill>
          {producto.categoriaNombre && <Pill tone="accent">{producto.categoriaNombre}</Pill>}
          {producto.marcaNombre && <Pill tone="info">{producto.marcaNombre}</Pill>}
          {producto.activo ? <Pill tone="success">Activo</Pill> : <Pill tone="danger">Inactivo</Pill>}
        </div>
        <button
          type="button"
          onClick={onCerrar}
          aria-label="Cerrar detalle"
          className="flex h-7 w-7 items-center justify-center rounded-md text-muted-foreground transition hover:bg-accent/10 hover:text-foreground focus:outline-none focus-visible:ring-2 focus-visible:ring-ring/60"
        >
          <X size={14} />
        </button>
      </div>

      <div className="grid grid-cols-1 gap-6 md:grid-cols-[220px_1fr]">
        {imagen ? (
          <motion.img
            layoutId={imagenId}
            src={imagen}
            alt={producto.nombre}
            className="h-[194px] w-full rounded-xl border border-border object-cover md:h-[229px]"
          />
        ) : (
          <motion.div
            layoutId={imagenId}
            className="flex h-[194px] w-full items-center justify-center rounded-xl border border-dashed border-border bg-muted/30 text-sm text-muted-foreground md:h-[229px]"
          >
            Sin imagen
          </motion.div>
        )}

        <div className="flex min-w-0 flex-col gap-4">
          {producto.descripcion && <p className="text-sm text-muted-foreground">{producto.descripcion}</p>}

          <div className="grid grid-cols-2 gap-2 sm:grid-cols-3 xl:grid-cols-6">
            <Metrica etiqueta="Variantes" valor={String(metricas.variantes)} />
            <Metrica etiqueta="Precio venta" valor={formatoRango(metricas.precioMinUSD, metricas.precioMaxUSD)} />
            <Metrica etiqueta="Costo" valor={formatoRango(metricas.costoMinUSD, metricas.costoMaxUSD)} />
            <Metrica etiqueta="Margen prom." valor={formatoRango(metricas.margenPromedioUSD, metricas.margenPromedioUSD)} />
            <Metrica etiqueta="Stock" valor={formatNumber(producto.variantes.reduce((suma, v) => suma + v.cantidad, 0))} />
            <Metrica etiqueta="Reservado" valor={formatNumber(producto.variantes.reduce((suma, v) => suma + v.cantidadReservada, 0))} />
          </div>

          {producto.tipo === 'Preparado' && (
            <p className="rounded-inner border border-border bg-info/15 p-2.5 text-xs text-info-fg">
              Se prepara en el local; consume de sus ingredientes (no se ordena a proveedor).
            </p>
          )}

          {producto.variantes.length > 0 && (
            <div className="overflow-x-auto rounded-inner border border-border">
              <table className="w-full min-w-[860px] text-sm">
                <thead>
                  <tr className="bg-muted/60 text-left text-xs uppercase tracking-tighter2 text-muted-foreground">
                    <th scope="col" className="px-4 py-2.5">Variante</th>
                    <th scope="col" className="px-4 py-2.5">SKU</th>
                    <th scope="col" className="px-4 py-2.5">Código</th>
                    <th scope="col" className="px-4 py-2.5">Consume</th>
                    <th scope="col" className="px-4 py-2.5 text-right">Disponible</th>
                    <th scope="col" className="px-4 py-2.5 text-right">Reservada</th>
                    <th scope="col" className="px-4 py-2.5 text-right">Costo</th>
                    <th scope="col" className="px-4 py-2.5 text-right">Venta</th>
                  </tr>
                </thead>
                <tbody>
                  {producto.variantes.map((variante) => (
                    <tr key={variante.id} className="border-t border-border">
                      <td className="px-4 py-2.5 text-foreground">
                        {variante.nombre}
                        {variante.esBase && <Pill tone="accent" className="ml-2">Base</Pill>}
                      </td>
                      <td className="num px-4 py-2.5 text-muted-foreground">{variante.sku}</td>
                      <td className="px-4 py-2.5">
                        {variante.codigosBarras?.[0] ? <CodigoBarras valor={variante.codigosBarras[0]} /> : <span className="text-muted-foreground">—</span>}
                      </td>
                      <td className="px-4 py-2.5">
                        {variante.esBase ? <span className="text-xs text-muted-foreground">—</span> : <ConsumoVariante varianteId={variante.id} />}
                      </td>
                      <td className="num px-4 py-2.5 text-right text-foreground">{formatNumber(variante.cantidad)}</td>
                      <td className="num px-4 py-2.5 text-right text-muted-foreground">{formatNumber(variante.cantidadReservada)}</td>
                      <td className="num px-4 py-2.5 text-right text-muted-foreground">{formatUSD(variante.precioCompraUSD)}</td>
                      <td className="num px-4 py-2.5 text-right text-foreground">{formatUSD(variante.precioVentaUSD)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
