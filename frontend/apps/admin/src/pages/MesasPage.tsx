import { useEffect, useMemo, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import {
  Activity,
  Armchair,
  ArrowRight,
  Brush,
  CalendarClock,
  CircleCheck,
  Clock,
  DoorOpen,
  Eye,
  LayoutGrid,
  Map as MapIcon,
  Maximize,
  Sparkles,
  User,
  UtensilsCrossed,
  Wallet,
} from 'lucide-react';
import { Button, cn, Skeleton } from '@licoreria/ui';
import type { Cuenta, Mesa, Plano, PlanoElemento } from '@licoreria/types';

import { clubApi, cuentasApi, usuariosApi } from '@licoreria/api-client';
import { MapaView, type EstadoMesaPlano } from '../components/mapa/MapaView';
import { UNIT } from '../components/mapa/elementos';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { useRealtime } from '../hooks/useRealtime';
import { estadoDeMesa, planoVisible } from '../lib/plano';
import { formatUSD, haceCuanto } from '../lib/format';
import { mensajeDeError } from '../lib/api';

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

const ETIQUETA_ESTADO: Record<EstadoMesaPlano, string> = {
  Libre: 'Libre',
  Ocupada: 'Ocupada',
  Reservada: 'Reservada',
  EnLimpieza: 'En limpieza',
};

type TipoEvento = 'apertura' | 'comanda' | 'item' | 'liberada' | 'reservada' | 'limpieza';

interface EventoMesa {
  id: string;
  cuando: Date;
  tipo: TipoEvento;
  mesa?: string;
  zona?: string;
  texto: string;
}

const EVENTO_CHIP: Record<TipoEvento, string> = {
  apertura: 'bg-success/25 text-success-fg',
  comanda: 'bg-primary/25 text-foreground',
  item: 'bg-info/25 text-info-fg',
  liberada: 'bg-muted/25 text-muted-foreground',
  reservada: 'bg-warning/25 text-warning-fg',
  limpieza: 'bg-info/25 text-info-fg',
};

let contador = 0;
const nuevoEventoId = () => `ev-${Date.now()}-${contador++}`;

function KpiTile({
  label,
  value,
  icon,
  chip,
}: {
  label: string;
  value: React.ReactNode;
  icon: React.ReactNode;
  chip: string;
}) {
  return (
    <div className="border border-border bg-card flex items-center gap-3 rounded-xl px-4 py-3">
      <span className={cn('flex h-9 w-9 shrink-0 items-center justify-center rounded-full', chip)}>{icon}</span>
      <div className="min-w-0">
        <p className="num truncate text-lg font-medium leading-tight text-foreground">{value}</p>
        <p className="truncate text-[11px] text-muted-foreground">{label}</p>
      </div>
    </div>
  );
}

/** Supervisión del salón en tiempo real para el Administrador. */
export function MesasPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const [seleccionadaId, setSeleccionadaId] = useState<string | null>(null);
  const [zonaFeed, setZonaFeed] = useState<string | null>(null);
  const [eventos, setEventos] = useState<EventoMesa[]>([]);
  const [zoom, setZoom] = useState(1);
  const [mesaLiberar, setMesaLiberar] = useState<Mesa | null>(null);
  const wrapper = useRef<HTMLDivElement>(null);

  const planos = useQuery({ queryKey: ['planos'], queryFn: clubApi.planos });
  const zonas = useQuery({ queryKey: ['zonas'], queryFn: clubApi.zonas });
  const mesas = useQuery({ queryKey: ['mesas'], queryFn: () => clubApi.mesas() });
  const cuentas = useQuery({
    queryKey: ['cuentas', 'abiertas'],
    queryFn: () => cuentasApi.listar({ estado: 'Abierta', pageSize: 100 }),
  });
  const usuarios = useQuery({ queryKey: ['usuarios'], queryFn: () => usuariosApi.listar({ page: 1, pageSize: 100 }) });

  const listaMesas = useMemo(() => mesas.data ?? [], [mesas.data]);
  const cuentasAbiertas = useMemo(() => cuentas.data?.items ?? [], [cuentas.data]);
  const plano = useMemo<Plano>(() => planoVisible(planos.data ?? [], listaMesas), [planos.data, listaMesas]);

  const cuentasPorId = useMemo(() => {
    const mapa: Record<string, Cuenta> = {};
    for (const cuenta of cuentasAbiertas) mapa[cuenta.id] = cuenta;
    return mapa;
  }, [cuentasAbiertas]);

  const usuariosPorId = useMemo(() => {
    const mapa: Record<string, string> = {};
    for (const usuario of usuarios.data?.items ?? []) mapa[usuario.id] = usuario.nombreCompleto || usuario.email;
    return mapa;
  }, [usuarios.data]);

  const estadoPorMesa = useMemo(
    () => Object.fromEntries(listaMesas.map((m) => [m.id, estadoDeMesa(m)])) as Record<string, EstadoMesaPlano>,
    [listaMesas],
  );
  const numeroPorMesa = useMemo(() => Object.fromEntries(listaMesas.map((m) => [m.id, m.numero])), [listaMesas]);

  const porEstado = useMemo(() => {
    const res: Record<EstadoMesaPlano, number> = { Libre: 0, Ocupada: 0, Reservada: 0, EnLimpieza: 0 };
    for (const mesa of listaMesas) res[estadoDeMesa(mesa)] += 1;
    return res;
  }, [listaMesas]);

  const saldoTotal = useMemo(() => cuentasAbiertas.reduce((acc, c) => acc + c.saldo, 0), [cuentasAbiertas]);

  const invalidar = () => {
    queryClient.invalidateQueries({ queryKey: ['mesas'] });
    queryClient.invalidateQueries({ queryKey: ['cuentas'] });
  };

  const agregarEvento = (tipo: TipoEvento, mesaNumero: string, zona: string, texto: string) => {
    setEventos((prev) => [{ id: nuevoEventoId(), cuando: new Date(), tipo, mesa: mesaNumero, zona, texto }, ...prev].slice(0, 40));
  };

  useRealtime('meseros', {
    'mesa:actualizada': (payload) => {
      const p = payload as { mesaId: string; estado: string };
      const mesa = listaMesas.find((m) => m.id === p.mesaId);
      if (!mesa) return;
      let tipo: TipoEvento;
      let texto: string;
      switch (p.estado) {
        case 'Ocupada':
          tipo = 'apertura';
          texto = 'Se abrió la mesa';
          break;
        case 'Libre':
          tipo = 'liberada';
          texto = 'Mesa liberada';
          break;
        case 'Reservada':
          tipo = 'reservada';
          texto = 'Marcada como reservada';
          break;
        default:
          tipo = 'limpieza';
          texto = `Estado: ${p.estado}`;
      }
      agregarEvento(tipo, mesa.numero, mesa.zonaNombre, texto);
      invalidar();
    },
    'comanda:creada': (payload) => {
      const p = payload as { cuentaId: string; area?: string };
      const mesa = listaMesas.find((m) => m.cuentaId === p.cuentaId);
      if (mesa) agregarEvento('comanda', mesa.numero, mesa.zonaNombre, `Comanda enviada${p.area ? ` (${p.area})` : ''}`);
      invalidar();
    },
    'comanda:actualizada': invalidar,
    'item:actualizado': (payload) => {
      const p = payload as { cuentaId: string; estado?: string };
      const mesa = listaMesas.find((m) => m.cuentaId === p.cuentaId);
      if (mesa) agregarEvento('item', mesa.numero, mesa.zonaNombre, `Ítem ${p.estado ?? 'servido'}`);
      invalidar();
    },
  });

  const ajustar = () => {
    const w = wrapper.current?.clientWidth ?? 0;
    const h = wrapper.current?.clientHeight ?? 0;
    if (w === 0 || h === 0) return;
    const anchoMapa = plano.anchoFondo * UNIT;
    const altoMapa = plano.altoFondo * UNIT;
    const z = Math.min(1.4, Math.max(0.3, Math.min((w - 40) / anchoMapa, (h - 40) / altoMapa)));
    setZoom(+z.toFixed(2));
  };

  useEffect(() => {
    ajustar();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [plano.anchoFondo, plano.altoFondo, planos.data, mesas.data]);

  useEffect(() => {
    window.addEventListener('resize', ajustar);
    return () => window.removeEventListener('resize', ajustar);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const liberar = useMutation({
    mutationFn: (mesaId: string) => clubApi.desalojar(mesaId),
    onSuccess: () => {
      toast.success('Mesa liberada');
      setMesaLiberar(null);
      setSeleccionadaId(null);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo liberar', { description: mensajeDeError(error) }),
  });

  const manejarElemento = (_evento: unknown, elemento: PlanoElemento) => {
    if (!elemento.mesaId) return;
    const mesa = listaMesas.find((m) => m.id === elemento.mesaId);
    if (mesa) setSeleccionadaId(mesa.id);
  };

  const mesaSeleccionada = listaMesas.find((m) => m.id === seleccionadaId) ?? null;
  const cuentaSeleccionada = mesaSeleccionada?.cuentaId ? cuentasPorId[mesaSeleccionada.cuentaId] ?? null : null;
  const atendidaPor = cuentaSeleccionada?.abiertaPorId ? usuariosPorId[cuentaSeleccionada.abiertaPorId] ?? '—' : '—';

  const zonasConMesas = useMemo(
    () => (zonas.data ?? []).filter((z) => listaMesas.some((m) => m.zonaId === z.id)),
    [zonas.data, listaMesas],
  );
  const eventosFiltrados = useMemo(
    () => (zonaFeed ? eventos.filter((e) => e.zona === zonaFeed) : eventos),
    [eventos, zonaFeed],
  );

  return (
    <div className="absolute inset-0 flex min-h-0 flex-col overflow-hidden p-3 pb-24 lg:p-4 lg:pb-4">
      {/* ===== Franja superior ===== */}
      <div className="flex flex-wrap items-center gap-2 border-b border-border pb-3">
        <span className="flex h-9 w-9 items-center justify-center rounded-full bg-primary/20 text-foreground">
          <MapIcon size={17} />
        </span>
        <div className="mr-2">
          <p className="text-sm font-medium text-foreground">Mesas · Supervisión</p>
          <p className="text-[11px] text-muted-foreground">Movimiento del salón en tiempo real</p>
        </div>

        <div className="mx-1 hidden h-6 w-px bg-hairline md:block" />
        <div className="hidden items-center gap-3 md:flex">
          {LEYENDA.map((item) => (
            <span key={item.estado} className="inline-flex items-center gap-1.5 text-[11px] text-muted-foreground">
              <span className="h-2.5 w-2.5 rounded-full" style={{ backgroundColor: item.color }} />
              {item.label}
            </span>
          ))}
        </div>

        <div className="ml-auto flex items-center gap-2">
          <span className="inline-flex items-center gap-1.5 rounded-full border border-border bg-card/60 px-2.5 py-1 text-[11px] text-success-fg">
            <span className="relative flex h-2 w-2">
              <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-success opacity-60" />
              <span className="relative inline-flex h-2 w-2 rounded-full bg-success" />
            </span>
            En vivo
          </span>
          <Button size="sm" variant="ghost" onClick={ajustar} aria-label="Ajustar mapa">
            <Maximize size={15} />
          </Button>
        </div>
      </div>

      {/* ===== Tira de KPIs ===== */}
      <div className="grid grid-cols-2 gap-2 pt-3 sm:grid-cols-3 xl:grid-cols-6">
        <KpiTile label="Ocupadas" value={porEstado.Ocupada} chip="bg-danger/25 text-destructive-fg" icon={<Armchair size={16} />} />
        <KpiTile label="Libres" value={porEstado.Libre} chip="bg-success/25 text-success-fg" icon={<CircleCheck size={16} />} />
        <KpiTile label="Reservadas" value={porEstado.Reservada} chip="bg-warning/25 text-warning-fg" icon={<CalendarClock size={16} />} />
        <KpiTile label="En limpieza" value={porEstado.EnLimpieza} chip="bg-info/25 text-info-fg" icon={<Brush size={16} />} />
        <KpiTile label="Cuentas abiertas" value={cuentasAbiertas.length} chip="bg-primary/25 text-foreground" icon={<LayoutGrid size={16} />} />
        <KpiTile label="Saldo total" value={formatUSD(saldoTotal)} chip="bg-butter/25 text-butter-fg" icon={<Wallet size={16} />} />
      </div>

      {/* ===== Cuerpo: mapa + panel ===== */}
      <div className="flex min-h-0 flex-1 gap-3 pt-3">
        {/* Mapa */}
        <div className="relative min-h-0 min-w-0 flex-1 overflow-hidden rounded-lg border border-border bg-muted/30">
          {planos.isLoading || mesas.isLoading ? (
            <div className="absolute inset-0 flex items-center justify-center">
              <Skeleton className="h-64 w-full max-w-xl" />
            </div>
          ) : (
            <div ref={wrapper} className="absolute inset-0 overflow-hidden">
              <div style={{ transform: `scale(${zoom})`, transformOrigin: '0 0' }} className="absolute left-4 top-4">
                <MapaView
                  plano={plano}
                  zonas={zonas.data ?? []}
                  modo="operacion"
                  estadoPorMesa={estadoPorMesa}
                  numeroPorMesa={numeroPorMesa}
                  onElementoPointerDown={manejarElemento}
                />
              </div>
            </div>
          )}
        </div>

        {/* Panel derecho */}
        <div className="flex w-72 min-h-0 shrink-0 flex-col gap-3 sm:w-80 lg:w-[22rem]">
          {/* Feed de actividad */}
          <div className="border border-border bg-card flex min-h-0 flex-1 flex-col rounded-xl p-3">
            <div className="flex items-center justify-between gap-2 pb-2">
              <p className="text-xs font-medium text-foreground">Actividad en vivo</p>
              <Activity size={14} className="text-success-fg" />
            </div>
            {zonasConMesas.length > 1 && (
              <div className="app-scroll -mx-1 flex gap-1.5 overflow-x-auto px-1 pb-2">
                <ChipFeed activo={zonaFeed === null} onClick={() => setZonaFeed(null)}>
                  Todas
                </ChipFeed>
                {zonasConMesas.map((zona) => (
                  <ChipFeed key={zona.id} activo={zonaFeed === zona.id} onClick={() => setZonaFeed(zona.id)}>
                    {zona.nombre}
                  </ChipFeed>
                ))}
              </div>
            )}
            <div className="app-scroll min-h-0 flex-1 overflow-y-auto pr-1">
              {eventosFiltrados.length === 0 ? (
                <p className="py-8 text-center text-xs text-muted-foreground">Sin movimientos todavía. La actividad aparecerá aquí en tiempo real.</p>
              ) : (
                <ul className="flex flex-col gap-2">
                  {eventosFiltrados.map((evento) => (
                    <li key={evento.id} className="flex items-start gap-2.5">
                      <span className={cn('mt-0.5 flex h-6 w-6 shrink-0 items-center justify-center rounded-full', EVENTO_CHIP[evento.tipo])}>
                        <IconoEvento tipo={evento.tipo} />
                      </span>
                      <div className="min-w-0">
                        <p className="text-xs leading-snug text-foreground">
                          {evento.mesa ? <span className="font-medium">Mesa {evento.mesa} · </span> : null}
                          {evento.texto}
                        </p>
                        <p className="text-[10px] text-muted-foreground">
                          {haceCuanto(evento.cuando.toISOString())}
                          {evento.zona ? ` · ${evento.zona}` : ''}
                        </p>
                      </div>
                    </li>
                  ))}
                </ul>
              )}
            </div>
          </div>

          {/* Detalle de la mesa seleccionada */}
          {mesaSeleccionada ? (
            <div className="border border-border bg-card shrink-0 rounded-xl p-4">
              <div className="flex items-center justify-between gap-2">
                <p className="text-sm font-medium text-foreground">Mesa {mesaSeleccionada.numero}</p>
                <span
                  className="inline-flex items-center gap-1.5 rounded-full px-2 py-0.5 text-[11px] text-foreground"
                  style={{ backgroundColor: `${COLOR_ESTADO[estadoDeMesa(mesaSeleccionada)]}22`, color: COLOR_ESTADO[estadoDeMesa(mesaSeleccionada)] }}
                >
                  <span className="h-2 w-2 rounded-full" style={{ backgroundColor: COLOR_ESTADO[estadoDeMesa(mesaSeleccionada)] }} />
                  {ETIQUETA_ESTADO[estadoDeMesa(mesaSeleccionada)]}
                </span>
              </div>
              <p className="mt-0.5 text-xs text-muted-foreground">{mesaSeleccionada.zonaNombre}</p>

              <div className="mt-3 flex flex-col gap-1.5 text-xs">
                <Detalle icon={<User size={13} />} label="Atendida por" value={atendidaPor} />
                {cuentaSeleccionada && (
                  <>
                    <Detalle icon={<Clock size={13} />} label="Abierta" value={haceCuanto(cuentaSeleccionada.abiertaEn)} />
                    <Detalle icon={<Wallet size={13} />} label="Saldo" value={<span className="num text-foreground">{formatUSD(cuentaSeleccionada.saldo)}</span>} />
                    <Detalle icon={<UtensilsCrossed size={13} />} label="Comandas" value={`${cuentaSeleccionada.comandas.length}`} />
                  </>
                )}
              </div>

              {estadoDeMesa(mesaSeleccionada) === 'Ocupada' && cuentaSeleccionada && (
                <div className="mt-3 flex gap-2">
                  <Button size="sm" variant="ghost" className="flex-1" onClick={() => navigate(`/cuentas/${cuentaSeleccionada.id}`)}>
                    <Eye size={14} /> Ver cuenta
                  </Button>
                  <Button size="sm" variant="danger" className="flex-1" onClick={() => setMesaLiberar(mesaSeleccionada)}>
                    <DoorOpen size={14} /> Liberar
                  </Button>
                </div>
              )}
            </div>
          ) : (
            <div className="border border-border bg-card flex shrink-0 items-center justify-center gap-2 rounded-xl px-4 py-6 text-center">
              <ArrowRight size={14} className="text-muted-foreground" />
              <p className="text-xs text-muted-foreground">Toca una mesa en el mapa para ver su detalle</p>
            </div>
          )}
        </div>
      </div>

      <ConfirmDialog
        open={Boolean(mesaLiberar)}
        title="Liberar mesa"
        description={`¿Liberar la mesa ${mesaLiberar?.numero ?? ''} sin cerrar la cuenta? El saldo quedará por cobrar.`}
        confirmLabel="Liberar"
        loading={liberar.isPending}
        onClose={() => setMesaLiberar(null)}
        onConfirm={() => mesaLiberar && liberar.mutate(mesaLiberar.id)}
      />
    </div>
  );
}

function IconoEvento({ tipo }: { tipo: TipoEvento }) {
  const size = 12;
  switch (tipo) {
    case 'apertura':
      return <Armchair size={size} />;
    case 'comanda':
      return <UtensilsCrossed size={size} />;
    case 'item':
      return <Sparkles size={size} />;
    case 'reservada':
      return <CalendarClock size={size} />;
    case 'limpieza':
      return <Brush size={size} />;
    default:
      return <DoorOpen size={size} />;
  }
}

function ChipFeed({ activo, onClick, children }: { activo: boolean; onClick: () => void; children: React.ReactNode }) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={cn(
        'flex h-7 shrink-0 items-center whitespace-nowrap rounded-full border px-2.5 text-[11px] transition',
        activo ? 'border-primary bg-primary/15 text-foreground' : 'border-border text-muted-foreground hover:text-foreground',
      )}
    >
      {children}
    </button>
  );
}

function Detalle({ icon, label, value }: { icon: React.ReactNode; label: string; value: React.ReactNode }) {
  return (
    <div className="flex items-center gap-2 text-muted-foreground">
      <span className="text-muted-foreground">{icon}</span>
      <span className="w-24 shrink-0">{label}</span>
      <span className="ml-auto text-right text-foreground">{value}</span>
    </div>
  );
}
