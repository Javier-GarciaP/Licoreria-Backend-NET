import type { RefObject } from 'react';
import { Button } from '@licoreria/ui';
import type { Promocion } from '@licoreria/types';
import { formatUSD } from '../../lib/format';
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
}: {
  pos: PosApi;
  promociones: Promocion[];
  registrando: boolean;
  onCobrar: () => void;
  descuentoRef: RefObject<HTMLInputElement>;
}) {
  const { activa } = pos;
  const subtotal = subtotalOrden(activa);
  const total = totalOrden(activa);
  const totalPagar = totalConPropina(activa);
  const puedeCobrar = activa.lineas.length > 0;

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

      <div className="flex flex-col gap-3 border-t border-hairline pt-3">
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
            className={`num h-10 w-full rounded-control border border-hairline bg-surface px-3 text-sm text-ink focus:outline-none focus:ring-2 focus:ring-accent/50 ${sinSpinners}`}
          />
          <input
            aria-label="Propina USD"
            type="number"
            min={0}
            step="0.01"
            value={activa.propinaUSD || ''}
            placeholder="Propina"
            onChange={(evento) => pos.propina(Number(evento.target.value) || 0)}
            className={`num h-10 w-full rounded-control border border-hairline bg-surface px-3 text-sm text-ink focus:outline-none focus:ring-2 focus:ring-accent/50 ${sinSpinners}`}
          />
        </div>

        {promociones.length > 0 && (
          <select
            aria-label="Promoción"
            value={activa.promocionId}
            onChange={(evento) => pos.promocion(evento.target.value)}
            className="h-10 w-full rounded-control border border-hairline bg-surface px-3 text-sm text-ink focus:outline-none focus:ring-2 focus:ring-accent/50"
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
          <div className="flex items-center justify-between text-sm text-muted">
            <span>Subtotal</span>
            <span className="num">{formatUSD(subtotal)}</span>
          </div>
          {activa.descuentoUSD > 0 && (
            <div className="flex items-center justify-between text-sm text-danger-ink">
              <span>Descuento</span>
              <span className="num">−{formatUSD(activa.descuentoUSD)}</span>
            </div>
          )}
          <div className="flex items-center justify-between text-sm text-muted">
            <span>Total</span>
            <span className="num">{formatUSD(total)}</span>
          </div>
          {activa.propinaUSD > 0 && (
            <div className="flex items-center justify-between text-sm text-muted">
              <span>Propina</span>
              <span className="num">{formatUSD(activa.propinaUSD)}</span>
            </div>
          )}
          <div className="flex items-center justify-between text-base font-medium text-ink">
            <span>Total a cobrar</span>
            <span className="num">{formatUSD(totalPagar)}</span>
          </div>
        </div>

        <Button size="lg" className="w-full" loading={registrando} disabled={!puedeCobrar || registrando} onClick={onCobrar}>
          Cobrar total <span className="num">{formatUSD(totalPagar)}</span>
        </Button>
      </div>
    </div>
  );
}
