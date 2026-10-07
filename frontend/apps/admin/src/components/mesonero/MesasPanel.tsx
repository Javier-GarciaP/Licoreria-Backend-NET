import { useMemo, useState, type ReactNode } from 'react';
import { LayoutGrid, Map as MapIcon } from 'lucide-react';
import { cn } from '@licoreria/ui';
import type { Cuenta, Mesa, Plano, PlanoElemento, Zona } from '@licoreria/types';
import { MapaView, type EstadoMesaPlano } from '../mapa/MapaView';
import { formatUSD, haceCuanto } from '../../lib/format';
import { estadoDeMesa, planoVisible } from '../../lib/plano';

const LEYENDA: { estado: EstadoMesaPlano; label: string; color: string }[] = [
  { estado: 'Libre', label: 'Libre', color: '#2fbf71' },
  { estado: 'Ocupada', label: 'Ocupada', color: '#ef4444' },
  { estado: 'Reservada', label: 'Reservada', color: '#f5a524' },
  { estado: 'EnLimpieza', label: 'En limpieza', color: '#3b82f6' },
];

const COLOR_ESTADO: Record<EstadoMesaPlano, string> = {
  Libre: '#2fbf71',
  Ocupada: '#ef4444',
  Reservada: '#f5a524',
  EnLimpieza: '#3b82f6',
};

export function MesasPanel({
  mesas,
  zonas,
  planos,
  cuentasPorId,
  seleccionadaId,
  onSeleccionar,
}: {
  mesas: Mesa[];
  zonas: Zona[];
  planos: Plano[];
  cuentasPorId: Record<string, Cuenta>;
  seleccionadaId: string | null;
  onSeleccionar: (mesa: Mesa) => void;
}) {
  const [vista, setVista] = useState<'mapa' | 'grilla'>('mapa');
  const [zonaId, setZonaId] = useState<string | null>(null);

  const plano = useMemo<Plano>(() => planoVisible(planos, mesas), [planos, mesas]);

  const estadoPorMesa = useMemo(
    () => Object.fromEntries(mesas.map((m) => [m.id, estadoDeMesa(m)])) as Record<string, EstadoMesaPlano>,
    [mesas],
  );
  const numeroPorMesa = useMemo(() => Object.fromEntries(mesas.map((m) => [m.id, m.numero])), [mesas]);

  const zonasConMesas = useMemo(
    () => zonas.filter((z) => mesas.some((m) => m.zonaId === z.id)),
    [zonas, mesas],
  );

  const mesasFiltradas = useMemo(
    () => (zonaId ? mesas.filter((m) => m.zonaId === zonaId) : mesas),
    [mesas, zonaId],
  );

  const manejarElemento = (_evento: unknown, elemento: PlanoElemento) => {
    if (!elemento.mesaId) return;
    const mesa = mesas.find((m) => m.id === elemento.mesaId);
    if (mesa) onSeleccionar(mesa);
  };

  return (
    <div className="flex h-full min-h-0 flex-col gap-3">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <div className="flex flex-wrap items-center gap-3">
          {LEYENDA.map((item) => (
            <span key={item.estado} className="inline-flex items-center gap-1.5 text-[11px] text-muted">
              <span className="h-2.5 w-2.5 rounded-full" style={{ backgroundColor: item.color }} />
              {item.label}
            </span>
          ))}
        </div>
        <div className="flex rounded-pill border border-hairline p-0.5">
          {([
            ['mapa', MapIcon],
            ['grilla', LayoutGrid],
          ] as const).map(([valor, Icono]) => (
            <button
              key={valor}
              type="button"
              aria-label={`Vista ${valor}`}
              aria-pressed={vista === valor}
              onClick={() => setVista(valor)}
              className={cn(
                'flex h-8 w-8 items-center justify-center rounded-pill transition',
                vista === valor ? 'bg-accent/20 text-accent-ink' : 'text-muted hover:text-ink',
              )}
            >
              <Icono size={15} />
            </button>
          ))}
        </div>
      </div>

      {zonasConMesas.length > 1 && (
        <div className="app-scroll -mx-1 flex gap-2 overflow-x-auto px-1 pb-1">
          <ChipZona activo={zonaId === null} onClick={() => setZonaId(null)}>
            Todas
          </ChipZona>
          {zonasConMesas.map((zona) => (
            <ChipZona
              key={zona.id}
              activo={zonaId === zona.id}
              color={zona.color}
              onClick={() => {
                setZonaId(zona.id);
                setVista('grilla');
              }}
            >
              {zona.nombre}
            </ChipZona>
          ))}
        </div>
      )}

      <div className="min-h-0 flex-1 overflow-auto rounded-card border border-hairline bg-elevated/30 p-2">
        {vista === 'mapa' ? (
          <MapaView
            plano={plano}
            zonas={zonas}
            modo="operacion"
            estadoPorMesa={estadoPorMesa}
            numeroPorMesa={numeroPorMesa}
            onElementoPointerDown={manejarElemento}
          />
        ) : (
          <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 xl:grid-cols-4">
            {mesasFiltradas.map((mesa) => (
              <TarjetaMesa
                key={mesa.id}
                mesa={mesa}
                cuenta={mesa.cuentaId ? cuentasPorId[mesa.cuentaId] : undefined}
                seleccionada={mesa.id === seleccionadaId}
                onClick={() => onSeleccionar(mesa)}
              />
            ))}
            {mesasFiltradas.length === 0 && (
              <p className="col-span-full py-10 text-center text-sm text-muted">No hay mesas en esta zona.</p>
            )}
          </div>
        )}
      </div>
    </div>
  );
}

function ChipZona({
  children,
  activo,
  color,
  onClick,
}: {
  children: ReactNode;
  activo: boolean;
  color?: string | null;
  onClick: () => void;
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={cn(
        'flex h-9 shrink-0 items-center gap-2 whitespace-nowrap rounded-pill border px-3 text-sm transition',
        activo ? 'border-accent bg-accent/15 text-accent-ink' : 'border-hairline text-muted hover:text-ink',
      )}
    >
      {color && <span className="h-2.5 w-2.5 rounded-full" style={{ backgroundColor: color }} />}
      {children}
    </button>
  );
}

function TarjetaMesa({
  mesa,
  cuenta,
  seleccionada,
  onClick,
}: {
  mesa: Mesa;
  cuenta?: Cuenta;
  seleccionada: boolean;
  onClick: () => void;
}) {
  const estado = estadoDeMesa(mesa);
  return (
    <button
      type="button"
      onClick={onClick}
      className={cn(
        'flex min-h-[7rem] flex-col items-start gap-1 rounded-card border p-4 text-left outline-none transition',
        seleccionada ? 'border-accent bg-accent/10' : 'border-hairline bg-surface/60 hover:border-accent/40',
      )}
    >
      <div className="flex w-full items-center justify-between gap-2">
        <span className="text-base font-medium text-ink">Mesa {mesa.numero}</span>
        <span className="h-2.5 w-2.5 rounded-full" style={{ backgroundColor: COLOR_ESTADO[estado] }} />
      </div>
      <span className="text-[11px] text-muted">{mesa.zonaNombre}</span>
      <div className="mt-auto flex w-full items-end justify-between pt-2">
        {cuenta ? (
          <>
            <span className="num text-sm text-accent-ink">{formatUSD(cuenta.total)}</span>
            <span className="text-[11px] text-muted">{haceCuanto(cuenta.abiertaEn)}</span>
          </>
        ) : (
          <span className="text-[11px] text-muted">{EstadoLabel[estado]}</span>
        )}
      </div>
    </button>
  );
}

const EstadoLabel: Record<EstadoMesaPlano, string> = {
  Libre: 'Libre',
  Ocupada: 'Ocupada',
  Reservada: 'Reservada',
  EnLimpieza: 'En limpieza',
};
