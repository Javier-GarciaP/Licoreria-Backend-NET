import { useMemo, useState } from 'react';
import { Ban, Check, Divide, DoorOpen, HandCoins, Plus, Receipt } from 'lucide-react';
import { Button, Pill, StatusBadge, cn } from '@licoreria/ui';
import type { Cuenta, EstadoItemComanda, Mesa } from '@licoreria/types';
import { formatBS, formatUSD, haceCuanto } from '../../lib/format';
import { detallesConPago, saldoPendienteSeleccion, type DetalleConPago } from './pago';

const SIGUIENTE_ESTADO: Record<string, EstadoItemComanda | null> = {
  Recibido: 'Preparado',
  Preparado: 'Entregado',
  Entregado: null,
};

export function CuentaPanel({
  cuenta,
  mesa,
  registrando,
  puedeAbonar,
  puedeCobrar,
  onAgregarConsumo,
  onAbonar,
  onAbonarMonto,
  onDividir,
  onCobrar,
  onLiberar,
  onCambiarEstado,
  onCancelar,
}: {
  cuenta: Cuenta;
  mesa?: Mesa;
  registrando: boolean;
  puedeAbonar: boolean;
  puedeCobrar: boolean;
  onAgregarConsumo: () => void;
  onAbonar: (detalles: DetalleConPago[]) => void;
  onAbonarMonto: () => void;
  onDividir: () => void;
  onCobrar: () => void;
  onLiberar: () => void;
  onCambiarEstado: (comandaId: string, detalleId: string, estado: EstadoItemComanda) => void;
  onCancelar: (comandaId: string, detalleId: string) => void;
}) {
  const [tab, setTab] = useState<'consumo' | 'cuenta'>('consumo');
  const [seleccion, setSeleccion] = useState<Set<string>>(new Set());

  const detalles = useMemo(() => detallesConPago(cuenta), [cuenta]);
  const seleccionados = useMemo(
    () => detalles.filter((item) => seleccion.has(item.detalle.id) && item.pendiente > 0),
    [detalles, seleccion],
  );
  const totalSeleccion = saldoPendienteSeleccion(seleccionados);

  const alternar = (id: string) =>
    setSeleccion((actuales) => {
      const siguiente = new Set(actuales);
      if (siguiente.has(id)) siguiente.delete(id);
      else siguiente.add(id);
      return siguiente;
    });

  const comandas = cuenta.comandas.filter((comanda) => comanda.detalles.length > 0);

  return (
    <div className="flex h-full min-h-0 flex-col">
      <header className="flex flex-col gap-2 border-b border-hairline pb-3">
        <div className="flex items-start justify-between gap-3">
          <div className="min-w-0">
            <p className="truncate text-lg font-medium text-ink">
              {mesa ? `Mesa ${mesa.numero}` : cuenta.nombreMesa || 'Cuenta'}
            </p>
            <p className="text-xs text-muted">
              {mesa?.zonaNombre ? `${mesa.zonaNombre} · ` : ''}Abierta {haceCuanto(cuenta.abiertaEn)}
            </p>
          </div>
          <StatusBadge status={cuenta.estado} />
        </div>
        <div className="grid grid-cols-3 gap-2 text-center">
          <Totals etiqueta="Total" valor={formatUSD(cuenta.total)} />
          <Totals etiqueta="Abonado" valor={formatUSD(cuenta.totalAbonado)} tono="success" />
          <Totals etiqueta="Saldo" valor={formatUSD(cuenta.saldo)} tono="accent" />
        </div>
      </header>

      <div className="flex gap-1 pt-3">
        {(['consumo', 'cuenta'] as const).map((valor) => (
          <button
            key={valor}
            type="button"
            onClick={() => setTab(valor)}
            className={cn(
              'flex-1 rounded-pill px-3 py-2 text-sm capitalize transition',
              tab === valor ? 'bg-accent/20 font-medium text-accent-ink' : 'text-muted hover:text-ink',
            )}
          >
            {valor === 'consumo' ? 'Consumo' : 'Cuenta'}
          </button>
        ))}
      </div>

      <div className="app-scroll mt-3 min-h-0 flex-1 overflow-y-auto pr-1">
        {tab === 'consumo' ? (
          comandas.length === 0 ? (
            <div className="flex h-full flex-col items-center justify-center gap-1 py-10 text-center">
              <p className="text-sm text-muted">Sin consumos todavía.</p>
              <p className="text-xs text-stone">Toca "Agregar consumo" para empezar.</p>
            </div>
          ) : (
            <div className="flex flex-col gap-3">
              {seleccionados.length > 0 && (
                <div className="flex items-center justify-between gap-3 rounded-inner border border-accent/40 bg-accent/10 px-3 py-2">
                  <span className="text-sm text-accent-ink">
                    {seleccionados.length} seleccionados · <span className="num">{formatUSD(totalSeleccion)}</span>
                  </span>
                  <Button size="sm" disabled={!puedeAbonar} onClick={() => onAbonar(seleccionados)}>
                    Abonar selección
                  </Button>
                </div>
              )}
              {comandas.map((comanda) => (
                <section key={comanda.id} className="rounded-card border border-hairline p-3">
                  <div className="mb-2 flex items-center justify-between">
                    <Pill tone={comanda.area === 'Barra' ? 'accent' : 'info'}>{comanda.area}</Pill>
                    <span className="text-[11px] text-muted">{haceCuanto(comanda.fecha)}</span>
                  </div>
                  <ul className="flex flex-col gap-1.5">
                    {comanda.detalles.map((detalle) => {
                      const pago = detalles.find((item) => item.detalle.id === detalle.id);
                      const pagado = pago ? pago.pendiente <= 0 : false;
                      const seleccionado = seleccion.has(detalle.id);
                      const siguiente = SIGUIENTE_ESTADO[detalle.estado];
                      const cancelado = detalle.estado === 'Cancelado';
                      return (
                        <li
                          key={detalle.id}
                          className={cn(
                            'flex items-center gap-3 rounded-inner border px-3 py-2 transition',
                            seleccionado ? 'border-accent/60 bg-accent/10' : 'border-transparent bg-elevated/40',
                            cancelado && 'opacity-50',
                          )}
                        >
                          {!cancelado && pago && pago.pendiente > 0 && (
                            <input
                              type="checkbox"
                              aria-label={`Seleccionar ${detalle.nombre}`}
                              checked={seleccionado}
                              onChange={() => alternar(detalle.id)}
                              className="h-5 w-5 shrink-0 accent-[rgb(var(--color-accent))]"
                            />
                          )}
                          <div className="min-w-0 flex-1">
                            <p className={cn('truncate text-sm text-ink', pagado && 'line-through decoration-success/70')}>
                              <span className="num mr-1.5 text-muted">{detalle.cantidad}×</span>
                              {detalle.nombre}
                              {detalle.esCortesia && <span className="ml-2 text-[11px] text-warning-ink">cortesía</span>}
                            </p>
                            <p className="num text-[11px] text-muted">
                              {formatUSD(detalle.subtotalUSD)}
                              {pago && pago.pagado > 0 && !pagado && (
                                <span className="text-success-ink"> · pagado {formatUSD(pago.pagado)}</span>
                              )}
                            </p>
                          </div>
                          {pagado && <Pill tone="success">Pagado</Pill>}
                          {!cancelado && (
                            <div className="flex shrink-0 items-center gap-1">
                              <StatusBadge status={detalle.estado} />
                              {siguiente && (
                                <button
                                  type="button"
                                  aria-label={`Marcar ${siguiente}`}
                                  onClick={() => onCambiarEstado(comanda.id, detalle.id, siguiente)}
                                  className="flex h-8 w-8 items-center justify-center rounded-full border border-hairline text-muted transition hover:text-ink"
                                >
                                  <Check size={14} />
                                </button>
                              )}
                              <button
                                type="button"
                                aria-label={`Cancelar ${detalle.nombre}`}
                                onClick={() => onCancelar(comanda.id, detalle.id)}
                                className="flex h-8 w-8 items-center justify-center rounded-full border border-hairline text-muted transition hover:text-danger-ink"
                              >
                                <Ban size={14} />
                              </button>
                            </div>
                          )}
                        </li>
                      );
                    })}
                  </ul>
                </section>
              ))}
            </div>
          )
        ) : (
          <div className="flex flex-col gap-4">
            {cuenta.abonos.length > 0 && (
              <section className="rounded-card border border-hairline p-3">
                <p className="mb-2 text-xs font-medium uppercase tracking-tighter2 text-muted">Abonos</p>
                <ul className="flex flex-col gap-1">
                  {cuenta.abonos.map((abono) => (
                    <li key={abono.id} className="flex justify-between text-sm text-muted">
                      <span>{abono.metodoPago}</span>
                      <span className="num text-ink">
                        {abono.moneda === 'USD' ? formatUSD(abono.monto) : formatBS(abono.monto)}
                      </span>
                    </li>
                  ))}
                </ul>
              </section>
            )}
            <section className="rounded-card border border-hairline p-3">
              <p className="mb-2 text-xs font-medium uppercase tracking-tighter2 text-muted">Divisiones</p>
              {cuenta.divisiones.length === 0 ? (
                <p className="text-sm text-muted">Sin dividir. Usa "Dividir" para repartir el saldo.</p>
              ) : (
                <ul className="flex flex-col gap-1">
                  {cuenta.divisiones.map((division) => (
                    <li key={division.id} className="flex justify-between text-sm">
                      <span className="text-muted">Parte {division.indice}</span>
                      <span className={cn('num', division.pagada ? 'text-success-ink' : 'text-ink')}>
                        {formatUSD(division.monto)}
                      </span>
                    </li>
                  ))}
                </ul>
              )}
            </section>
          </div>
        )}
      </div>

      <div className="mt-3 flex flex-col gap-2 border-t border-hairline pt-3">
        <Button className="w-full" leftIcon={<Plus size={16} />} onClick={onAgregarConsumo} disabled={registrando}>
          Agregar consumo
        </Button>
        <div className="grid grid-cols-2 gap-2">
          <Button variant="ghost" leftIcon={<HandCoins size={15} />} disabled={!puedeAbonar} onClick={onAbonarMonto}>
            Abonar
          </Button>
          <Button variant="ghost" leftIcon={<Divide size={15} />} onClick={onDividir}>
            Dividir
          </Button>
          <Button variant="ghost" leftIcon={<DoorOpen size={15} />} onClick={onLiberar}>
            Liberar
          </Button>
          <Button
            leftIcon={<Receipt size={15} />}
            disabled={!puedeCobrar || registrando}
            onClick={onCobrar}
          >
            Cobrar {formatUSD(cuenta.saldo)}
          </Button>
        </div>
      </div>
    </div>
  );
}

function Totals({ etiqueta, valor, tono }: { etiqueta: string; valor: string; tono?: 'success' | 'accent' }) {
  return (
    <div className="rounded-inner bg-elevated/40 px-2 py-2">
      <p className="text-[10px] uppercase tracking-tighter2 text-muted">{etiqueta}</p>
      <p
        className={cn(
          'num text-sm font-medium',
          tono === 'success' ? 'text-success-ink' : tono === 'accent' ? 'text-accent-ink' : 'text-ink',
        )}
      >
        {valor}
      </p>
    </div>
  );
}
