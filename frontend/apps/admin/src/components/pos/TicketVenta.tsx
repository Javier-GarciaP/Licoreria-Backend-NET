import { useEffect } from 'react';
import { createPortal } from 'react-dom';
import { Printer, X } from 'lucide-react';
import { useQuery } from '@tanstack/react-query';
import type { Venta } from '@licoreria/types';
import { contenidoApi } from '@licoreria/api-client';
import { formatBS, formatDateTime, formatUSD } from '../../lib/format';

const codigoBarras =
  'repeating-linear-gradient(90deg,#000 0 1.5px,transparent 1.5px 3.5px,#000 3.5px 5px,transparent 5px 5.5px,#000 5.5px 8px,transparent 8px 11px,#000 11px 12px,transparent 12px 15px)';

/* Borde troquelado: 26 picos que simulan el corte del ticket térmico. */
const DIENTES = (() => {
  const total = 26;
  const paso = 100 / total;
  let puntos = '0,0 ';
  for (let i = 0; i < total; i += 1) {
    puntos += `${((i + 0.5) * paso).toFixed(2)},10 ${((i + 1) * paso).toFixed(2)},0 `;
  }
  return puntos.trim();
})();

function Separador() {
  return <div className="my-3 border-t border-dashed border-black/20" />;
}

function Fila({ etiqueta, valor, fuerte = false }: { etiqueta: string; valor: string; fuerte?: boolean }) {
  return (
    <div
      className={`flex items-baseline justify-between gap-3 ${
        fuerte ? 'text-sm font-semibold text-black' : 'text-black/70'
      }`}
    >
      <span className="min-w-0 break-words">{etiqueta}</span>
      <span className="num shrink-0 text-right">{valor}</span>
    </div>
  );
}

export function TicketVenta({ venta, onCerrar }: { venta: Venta | null; onCerrar: () => void }) {
  const local = useQuery({
    queryKey: ['pos', 'local-info'],
    queryFn: contenidoApi.localInfo,
    enabled: venta !== null,
    staleTime: 5 * 60 * 1000,
  });

  useEffect(() => {
    if (!venta) return;
    const previo = document.activeElement as HTMLElement | null;
    const alPresionar = (evento: KeyboardEvent) => {
      if (evento.key === 'Escape') onCerrar();
    };
    document.addEventListener('keydown', alPresionar);
    return () => {
      document.removeEventListener('keydown', alPresionar);
      previo?.focus?.();
    };
  }, [venta, onCerrar]);

  if (!venta) return null;

  const propina = venta.pagos.reduce((total, pago) => total + (pago.propina ?? 0), 0);
  const unidades = venta.detalles.reduce((total, detalle) => total + detalle.cantidad, 0);
  const numero = venta.numeroComprobante ?? venta.id.slice(0, 8);

  return createPortal(
    <div
      role="dialog"
      aria-modal="true"
      aria-label="Venta registrada"
      onClick={onCerrar}
      className="fixed inset-0 z-50 flex items-center justify-center overflow-y-auto bg-black/40 p-4"
    >
      <div className="relative my-auto" onClick={(evento) => evento.stopPropagation()}>
        <button
          type="button"
          autoFocus
          aria-label="Cerrar"
          onClick={onCerrar}
          className="no-print absolute -right-3 -top-3 z-10 flex h-8 w-8 items-center justify-center rounded-full border border-hairline bg-surface text-muted shadow-card transition hover:text-ink focus:outline-none focus-visible:ring-2 focus-visible:ring-accent/60"
        >
          <X size={15} />
        </button>

        <div className="ticket-print w-[20rem] max-w-[85vw] drop-shadow-[0_6px_18px_rgba(0,0,0,0.18)]">
          <div className="rounded-t-sm bg-white px-6 pb-4 pt-6 font-mono text-[11px] leading-relaxed text-neutral-900">
            <header className="flex flex-col items-center gap-1 text-center">
              {local.data?.logoUrl ? (
                <img src={local.data.logoUrl} alt="" className="mb-1 h-10 object-contain" />
              ) : null}
              <p className="max-w-full break-words text-sm font-semibold uppercase tracking-[0.18em] text-black">
                {local.data?.nombre?.trim() || 'Licorería'}
              </p>
              {local.data?.direccion && (
                <p className="max-w-full break-words text-[10px] text-black/55">{local.data.direccion}</p>
              )}
              {(local.data?.telefono || local.data?.email) && (
                <p className="max-w-full break-words text-[10px] text-black/55">
                  {[local.data?.telefono, local.data?.email].filter(Boolean).join(' · ')}
                </p>
              )}
              <p className="mt-1 text-[9px] uppercase tracking-[0.2em] text-black/45">Comprobante de venta</p>
            </header>

            <Separador />

            <div className="flex flex-col gap-1">
              <Fila etiqueta="Comprobante" valor={numero} fuerte />
              <Fila etiqueta="Fecha" valor={formatDateTime(venta.fecha)} />
              <Fila etiqueta="Estado" valor={venta.estado} />
              <Fila etiqueta="Tasa" valor={formatBS(venta.tasaCambio)} />
              <Fila etiqueta="Artículos" valor={String(unidades)} />
            </div>

            <Separador />

            <div className="flex items-center justify-between pb-1 text-[9px] uppercase tracking-[0.14em] text-black/40">
              <span>Descripción</span>
              <span>Importe</span>
            </div>

            <ul className="flex flex-col">
              {venta.detalles.map((detalle) => (
                <li key={detalle.id} className="border-b border-dashed border-black/10 py-1.5 last:border-0">
                  <p className="break-words font-medium uppercase text-black">{detalle.nombre}</p>
                  <div className="flex items-baseline justify-between gap-3 text-[10px] text-black/55">
                    <span className="num min-w-0 break-words">
                      {detalle.cantidad} × {formatUSD(detalle.precioUnitarioUSD)}
                      {detalle.sku && <span className="ml-1 tracking-wide">· {detalle.sku}</span>}
                    </span>
                    <span className="num shrink-0 text-right font-medium text-black">
                      {formatUSD(detalle.subtotalUSD)}
                    </span>
                  </div>
                  {detalle.esCortesia && (
                    <p className="text-[9px] uppercase tracking-wider text-emerald-600">Cortesía</p>
                  )}
                </li>
              ))}
            </ul>

            <Separador />

            <div className="flex flex-col gap-1">
              <Fila etiqueta="Subtotal" valor={formatUSD(venta.subtotalUSD)} />
              {venta.descuentoUSD > 0 && <Fila etiqueta="Descuento" valor={`−${formatUSD(venta.descuentoUSD)}`} />}
              {propina > 0 && <Fila etiqueta="Propina" valor={formatUSD(propina)} />}
              <div className="mt-1 flex items-baseline justify-between gap-3 border-t border-black/70 pt-1.5 text-sm font-semibold text-black">
                <span>TOTAL USD</span>
                <span className="num shrink-0 text-right">{formatUSD(venta.totalUSD + propina)}</span>
              </div>
              <Fila etiqueta="Total Bs" valor={formatBS(venta.totalBS + propina * venta.tasaCambio)} />
            </div>

            <Separador />

            <div className="flex flex-col gap-1">
              <p className="text-[9px] uppercase tracking-[0.14em] text-black/40">Pagos</p>
              {venta.pagos.length === 0 && <p className="text-[10px] text-black/55">Sin pagos registrados</p>}
              {venta.pagos.map((pago) => (
                <Fila
                  key={pago.id}
                  etiqueta={pago.metodoPago}
                  valor={pago.moneda === 'USD' ? formatUSD(pago.monto) : formatBS(pago.monto)}
                />
              ))}
            </div>

            <Separador />

            <footer className="flex flex-col items-center gap-2 text-center">
              <p className="text-[10px] uppercase tracking-[0.14em] text-black/55">¡Gracias por su compra!</p>
              <div className="h-9 w-44 max-w-full" style={{ backgroundImage: codigoBarras }} />
              <p className="num text-[10px] tracking-[0.3em] text-black/45">{numero}</p>
            </footer>

            <div className="no-print mt-4 flex justify-center">
              <button
                type="button"
                aria-label="Imprimir ticket"
                onClick={() => window.print()}
                className="flex h-9 w-9 items-center justify-center rounded-full border border-black/10 text-black/55 transition hover:bg-black/5 hover:text-black focus:outline-none focus-visible:ring-2 focus-visible:ring-black/20"
              >
                <Printer size={16} />
              </button>
            </div>
          </div>

          <svg
            className="-mt-px block h-2.5 w-full"
            viewBox="0 0 100 10"
            preserveAspectRatio="none"
            aria-hidden="true"
            focusable="false"
          >
            <polygon points={DIENTES} fill="#ffffff" />
          </svg>
        </div>
      </div>
    </div>,
    document.body,
  );
}
