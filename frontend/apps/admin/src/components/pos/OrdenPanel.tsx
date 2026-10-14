import type { RefObject } from 'react';
import { Button, cn } from '@licoreria/ui';
import type { Cliente, MetodoPago, Moneda, Promocion } from '@licoreria/types';
import { formatBS, formatUSD } from '../../lib/format';
import { ClienteVentaPicker } from './ClienteVentaPicker';
import { OrdenLineas } from './OrdenLineas';
import { OrdenTabs } from './OrdenTabs';
import { subtotalOrden, totalConPropina, totalOrden, type PosApi } from '../../hooks/usePos';

const sinSpinners =
  '[appearance:textfield] [&::-webkit-outer-spin-button]:appearance-none [&::-webkit-inner-spin-button]:appearance-none';

export function OrdenPanel({
  pos,
  promociones,
  registrando,
  onCobrar,
  descuentoRef,
  moneda,
  onMonedaChange,
  tasa,
  metodos,
  metodoPagoId,
  onMetodoChange,
  cliente,
  onClienteChange,
  esCredito,
  vencimiento,
  onVencimientoChange,
  vencimientoMinimo,
}: {
  pos: PosApi;
  promociones: Promocion[];
  registrando: boolean;
  onCobrar: () => void;
  descuentoRef: RefObject<HTMLInputElement>;
  /** Moneda en la que se registrará el pago de la venta. */
  moneda: Moneda;
  onMonedaChange: (moneda: Moneda) => void;
  /** Tasa de cambio vigente (Bs por USD). */
  tasa: number;
  /** Métodos de pago disponibles. */
  metodos: MetodoPago[];
  /** Método elegido para cobrar. */
  metodoPagoId: string;
  onMetodoChange: (id: string) => void;
  /** Cliente asociado a la venta (opcional salvo con Crédito). */
  cliente: Cliente | null;
  onClienteChange: (cliente: Cliente | null) => void;
  /** El método elegido es Crédito: exige cliente y fecha de vencimiento. */
  esCredito: boolean;
  /** Vencimiento previsto del crédito (AAAA-MM-DD). */
  vencimiento: string;
  onVencimientoChange: (fecha: string) => void;
  vencimientoMinimo: string;
}) {
  const { activa } = pos;
  const subtotal = subtotalOrden(activa);
  const total = totalOrden(activa);
  const totalPagar = totalConPropina(activa);
  const faltaCliente = esCredito && !cliente;
  const puedeCobrar = activa.lineas.length > 0 && !faltaCliente;

  return (
    <div className="flex h-full min-h-0 flex-col gap-3">
      <OrdenTabs
        ordenes={pos.ordenes}
        activaId={activa.id}
        onActivar={pos.activar}
        onNueva={() => pos.nueva(`Venta ${pos.ordenes.length + 1}`)}
        onCerrar={pos.cerrar}
        onRenombrar={pos.renombrar}
      />

      <OrdenLineas
        lineas={activa.lineas}
        indiceSeleccionado={pos.indiceLinea}
        onSeleccionar={pos.setIndiceLinea}
        onQuitar={pos.quitarLinea}
      />

      <div className="flex flex-col gap-3 border-t border-border pt-3">
        <div className="grid grid-cols-2 gap-2">
          <input
            ref={descuentoRef}
            aria-label="Descuento USD"
            type="number"
            min={0}
            step="0.01"
            value={activa.descuentoUSD || ''}
            placeholder="Descuento"
            onChange={(evento) => pos.descuento(Number(evento.target.value) || 0)}
            className={`num h-10 w-full rounded-control border border-border bg-card px-3 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-ring/50 ${sinSpinners}`}
          />
          <input
            aria-label="Propina USD"
            type="number"
            min={0}
            step="0.01"
            value={activa.propinaUSD || ''}
            placeholder="Propina"
            onChange={(evento) => pos.propina(Number(evento.target.value) || 0)}
            className={`num h-10 w-full rounded-control border border-border bg-card px-3 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-ring/50 ${sinSpinners}`}
          />
        </div>

        {promociones.length > 0 && (
          <select
            aria-label="Promoción"
            value={activa.promocionId}
            onChange={(evento) => pos.promocion(evento.target.value)}
            className="h-10 w-full rounded-control border border-border bg-card px-3 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-ring/50"
          >
            <option value="">Sin promoción</option>
            {promociones.map((promocion) => (
              <option key={promocion.id} value={promocion.id}>
                {promocion.nombre}
              </option>
            ))}
          </select>
        )}

        <div className="flex flex-col gap-1.5">
          <div className="flex items-center justify-between text-sm text-muted-foreground">
            <span>Subtotal</span>
            <span className="num">{formatUSD(subtotal)}</span>
          </div>
          {activa.descuentoUSD > 0 && (
            <div className="flex items-center justify-between text-sm text-destructive-fg">
              <span>Descuento</span>
              <span className="num">−{formatUSD(activa.descuentoUSD)}</span>
            </div>
          )}
          <div className="flex items-center justify-between text-sm text-muted-foreground">
            <span>Total</span>
            <span className="num">{formatUSD(total)}</span>
          </div>
          {activa.propinaUSD > 0 && (
            <div className="flex items-center justify-between text-sm text-muted-foreground">
              <span>Propina</span>
              <span className="num">{formatUSD(activa.propinaUSD)}</span>
            </div>
          )}
          <div className="flex items-center justify-between text-base font-medium text-foreground">
            <span>Total a cobrar</span>
            <span className="num">
              {moneda === 'BS' ? formatBS(tasa > 0 ? totalPagar * tasa : 0) : formatUSD(totalPagar)}
            </span>
          </div>
        </div>

        <ClienteVentaPicker cliente={cliente} onChange={onClienteChange} />

        <select
          aria-label="Método de pago"
          value={metodoPagoId}
          onChange={(evento) => onMetodoChange(evento.target.value)}
          className="h-10 w-full rounded-control border border-border bg-card px-3 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-ring/50"
        >
          {metodos.map((metodo) => (
            <option key={metodo.id} value={metodo.id}>
              {metodo.nombre}
            </option>
          ))}
        </select>

        {esCredito && (
          <div className="flex flex-col gap-1.5 rounded-control border border-border bg-muted/40 p-2">
            <label className="flex items-center justify-between gap-2 text-xs text-muted-foreground">
              Vence el
              <input
                aria-label="Vencimiento del crédito"
                type="date"
                min={vencimientoMinimo}
                value={vencimiento}
                onChange={(evento) => onVencimientoChange(evento.target.value)}
                className="h-8 rounded-control border border-border bg-card px-2 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-ring/50"
              />
            </label>
            {faltaCliente && (
              <p role="alert" className="text-xs text-destructive-fg">
                Elige un cliente para vender a crédito.
              </p>
            )}
          </div>
        )}

        <div className="flex items-center justify-between gap-2">
          <div className="flex items-center gap-0.5 rounded-lg border border-border p-0.5" role="group" aria-label="Moneda del pago">
            {(['USD', 'BS'] as const).map((opcion) => (
              <button
                key={opcion}
                type="button"
                onClick={() => onMonedaChange(opcion)}
                disabled={opcion === 'BS' && tasa <= 0}
                title={opcion === 'BS' && tasa <= 0 ? 'Sin tasa de cambio vigente' : undefined}
                className={cn(
                  'h-8 rounded-md px-2.5 text-xs font-medium transition disabled:cursor-not-allowed disabled:opacity-50',
                  moneda === opcion ? 'bg-primary/15 text-foreground' : 'text-muted-foreground hover:text-foreground',
                )}
                aria-pressed={moneda === opcion}
              >
                {opcion === 'USD' ? 'USD $' : 'Bs'}
              </button>
            ))}
          </div>
          {moneda === 'BS' && tasa > 0 && (
            <span className="num text-[11px] text-muted-foreground">1 USD = Bs {tasa.toFixed(2)}</span>
          )}
        </div>

        <Button size="lg" className="w-full" loading={registrando} disabled={!puedeCobrar || registrando} onClick={onCobrar}>
          Cobrar total{' '}
          <span className="num">
            {moneda === 'BS' ? formatBS(tasa > 0 ? totalPagar * tasa : 0) : formatUSD(totalPagar)}
          </span>
        </Button>
      </div>
    </div>
  );
}
