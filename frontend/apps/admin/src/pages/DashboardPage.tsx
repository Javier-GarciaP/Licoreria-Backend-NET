import { useEffect, useId, useMemo, type ReactNode } from 'react';
import {
  animate,
  motion,
  MotionConfig,
  useMotionValue,
  useReducedMotion,
  useTransform,
  type Variants,
} from 'framer-motion';
import { useQuery } from '@tanstack/react-query';
import { Area, AreaChart, Bar, BarChart, ResponsiveContainer, Tooltip, XAxis } from 'recharts';
import {
  Armchair,
  Boxes,
  CalendarClock,
  CalendarX2,
  LayoutGrid,
  Receipt,
  Star,
  Trash2,
  TrendingUp,
  Wallet,
} from 'lucide-react';
import { cn, Pill, Skeleton } from '@licoreria/ui';
import type { Dashboard, DiagnosticoInventario, ProductoVendido, ReporteHeatmap } from '@licoreria/types';
import { ApiError, reportesApi } from '@licoreria/api-client';
import { SelectorPeriodo } from '../components/SelectorPeriodo';
import { usePeriodo } from '../hooks/usePeriodo';
import { formatNumber, formatPercent, formatUSD } from '../lib/format';
import { fechaLocal } from '../lib/periodo';

const DIAS = ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb'];
const HORAS = Array.from({ length: 24 }, (_, hora) => hora);

/** Alto uniforme del contenido de cada panel (móvil / escritorio) para que las gráficas se lean parejas. */
const ALTURA_GRAFICA = 'h-52 md:h-64';

type Tone = 'mint' | 'sky' | 'lavender' | 'peach' | 'rose' | 'butter';

const TONE: Record<Tone, { chip: string; wash: string }> = {
  mint: { chip: 'bg-success text-success-fg', wash: 'bg-success/25' },
  sky: { chip: 'bg-info text-info-fg', wash: 'bg-info/25' },
  lavender: { chip: 'bg-primary text-primary-foreground', wash: 'bg-primary/25' },
  peach: { chip: 'bg-warning text-warning-fg', wash: 'bg-warning/25' },
  rose: { chip: 'bg-destructive text-destructive-fg', wash: 'bg-destructive/25' },
  butter: { chip: 'bg-butter text-butter-fg', wash: 'bg-butter/25' },
};

const TONE_VAR: Record<Tone, string> = {
  mint: '--color-success',
  sky: '--color-info',
  lavender: '--color-accent',
  peach: '--color-warning',
  rose: '--color-danger',
  butter: '--color-butter',
};

const TOOLTIP_STYLE = {
  background: 'rgb(var(--color-surface))',
  border: '1px solid rgb(var(--color-hairline))',
  borderRadius: 12,
  color: 'rgb(var(--color-ink))',
  fontSize: 12,
};

/** Entrada escalonada de los bloques del panel. `MotionConfig` respeta reduced-motion. */
const contenedor: Variants = {
  oculto: {},
  visible: { transition: { staggerChildren: 0.06, delayChildren: 0.05 } },
};

const item: Variants = {
  oculto: { opacity: 0, y: 22, scale: 0.96 },
  visible: { opacity: 1, y: 0, scale: 1, transition: { duration: 0.5, ease: [0.22, 1, 0.36, 1] } },
};

/** Cifra que se cuenta desde 0 hasta su valor al aparecer. */
function Contador({ valor, formato }: { valor: number; formato: (numero: number) => string }) {
  const reduce = useReducedMotion();
  const vector = useMotionValue(reduce ? valor : 0);
  const texto = useTransform(vector, (numero: number) => formato(numero));

  useEffect(() => {
    if (reduce) return;
    const control = animate(vector, valor, { duration: 0.9, ease: 'easeOut' });
    return () => control.stop();
  }, [valor, reduce, vector]);

  return <motion.span className="num">{texto}</motion.span>;
}

type Punto = { etiqueta: string; totalUSD: number };

/** Área pequeña con degradado de acento, sin ejes, integrada al tile. */
function AreaSpark({ data, tone = 'mint' }: { data: Punto[]; tone?: Tone }) {
  const gradiente = useId().replace(/[:]/g, '');
  const color = `rgb(var(${TONE_VAR[tone]}))`;
  return (
    <ResponsiveContainer width="100%" height="100%">
      <AreaChart data={data} margin={{ top: 6, right: 2, bottom: 0, left: 2 }}>
        <defs>
          <linearGradient id={gradiente} x1="0" y1="0" x2="0" y2="1">
            <stop offset="0%" stopColor={color} stopOpacity={0.45} />
            <stop offset="100%" stopColor={color} stopOpacity={0} />
          </linearGradient>
        </defs>
        <Tooltip contentStyle={TOOLTIP_STYLE} formatter={(value: number) => formatUSD(value)} />
        <Area
          type="monotone"
          dataKey="totalUSD"
          stroke={color}
          strokeWidth={2}
          fill={`url(#${gradiente})`}
          dot={false}
          isAnimationActive
          animationDuration={900}
          animationBegin={150}
        />
      </AreaChart>
    </ResponsiveContainer>
  );
}

/** Barras de ventas por día del periodo, con eje de fechas y tooltip. */
function BarrasDia({ data }: { data: Punto[] }) {
  const color = `rgb(var(${TONE_VAR.mint}))`;
  return (
    <ResponsiveContainer width="100%" height="100%">
      <BarChart data={data} margin={{ top: 4, right: 4, bottom: 0, left: 4 }}>
        <XAxis
          dataKey="etiqueta"
          tickLine={false}
          axisLine={false}
          interval="preserveStartEnd"
          minTickGap={24}
          tick={{ fontSize: 10, fill: 'rgb(var(--color-ink) / 0.55)' }}
        />
        <Tooltip
          cursor={{ fill: 'rgb(var(--color-ink) / 0.05)' }}
          contentStyle={TOOLTIP_STYLE}
          formatter={(value: number) => [formatUSD(value), 'Ventas']}
          labelFormatter={(etiqueta) => `Día ${etiqueta}`}
        />
        <Bar dataKey="totalUSD" fill={color} radius={[3, 3, 0, 0]} isAnimationActive animationDuration={800} />
      </BarChart>
    </ResponsiveContainer>
  );
}

/** Ranking horizontal de los productos más vendidos. */
function BarrasProductos({ items }: { items: ProductoVendido[] }) {
  const max = Math.max(1, ...items.map((producto) => producto.unidades));
  return (
    <div className="flex h-full flex-col justify-center gap-3">
      {items.map((producto) => (
        <div key={producto.varianteId} className="flex flex-col gap-1">
          <div className="flex items-center justify-between gap-2 text-xs">
            <span className="truncate text-foreground">
              {producto.productoNombre}
              {producto.varianteNombre ? <span className="text-muted-foreground"> · {producto.varianteNombre}</span> : null}
            </span>
            <span className="num shrink-0 text-muted-foreground">{formatNumber(producto.unidades)} u</span>
          </div>
          <div className="h-1.5 w-full overflow-hidden rounded-full bg-muted">
            <div className="h-full rounded-full bg-primary" style={{ width: `${(producto.unidades / max) * 100}%` }} />
          </div>
        </div>
      ))}
    </div>
  );
}

function BarrasMotivo({ items }: { items: { motivo: string; valorUSD: number }[] }) {
  const max = Math.max(1, ...items.map((entrada) => entrada.valorUSD));
  if (items.length === 0) {
    return <p className="py-6 text-center text-sm text-muted-foreground">Sin mermas registradas.</p>;
  }
  return (
    <div className="flex h-full flex-col justify-center gap-3">
      {items.slice(0, 5).map((entrada) => (
        <div key={entrada.motivo} className="flex flex-col gap-1">
          <div className="flex items-center justify-between gap-2 text-xs">
            <span className="truncate capitalize text-muted-foreground">{entrada.motivo}</span>
            <span className="num shrink-0 text-foreground">{formatUSD(entrada.valorUSD)}</span>
          </div>
          <div className="h-1.5 w-full overflow-hidden rounded-full bg-muted">
            <div
              className="h-full rounded-full bg-warning"
              style={{ width: `${(entrada.valorUSD / max) * 100}%` }}
            />
          </div>
        </div>
      ))}
    </div>
  );
}

/** Tile de KPI: vidrio con ícono en chip pastel, cifra tabular y hover sutil. */
function Tile({
  label,
  value,
  hint,
  icon,
  tone = 'lavender',
  hero = false,
  badge,
  className,
  children,
}: {
  label: string;
  value: ReactNode;
  hint?: ReactNode;
  icon?: ReactNode;
  tone?: Tone;
  hero?: boolean;
  badge?: ReactNode;
  className?: string;
  children?: ReactNode;
}) {
  return (
    <motion.div
      variants={item}
      className={cn(
        'border border-border bg-card flex h-full flex-col overflow-hidden rounded-xl p-5 transition-colors hover:border-primary/40',
        !children && 'justify-center',
        className,
      )}
    >
      <div className="flex items-center justify-between gap-3">
        <p className="text-xs font-medium text-muted-foreground">{label}</p>
        {icon && (
          <span className={cn('flex h-8 w-8 items-center justify-center rounded-full', TONE[tone].chip)}>
            {icon}
          </span>
        )}
      </div>
      <div className="relative mt-3 flex flex-wrap items-end justify-between gap-2">
        <p className={cn('font-medium tracking-tightest text-foreground', hero ? 'text-3xl' : 'text-2xl')}>
          {value}
        </p>
        {badge}
      </div>
      {hint && <p className="mt-1 text-xs text-muted-foreground">{hint}</p>}
      {children && <div className="mt-3 min-h-0 flex-1">{children}</div>}
    </motion.div>
  );
}

function Panel({
  title,
  aside,
  className,
  children,
}: {
  title: string;
  aside?: ReactNode;
  className?: string;
  children: ReactNode;
}) {
  return (
    <motion.div
      variants={item}
      className={cn(
        'border border-border bg-card flex h-full flex-col overflow-hidden rounded-xl p-5 transition-colors hover:border-primary/40',
        className,
      )}
    >
      <div className="mb-3 flex shrink-0 items-center justify-between gap-3">
        <h2 className="text-sm font-medium tracking-tighter2 text-foreground">{title}</h2>
        {aside && <span className="text-xs text-muted-foreground">{aside}</span>}
      </div>
      <div className={cn(ALTURA_GRAFICA, 'min-h-0 overflow-y-auto overflow-x-hidden')}>{children}</div>
    </motion.div>
  );
}

function Heatmap({ data }: { data?: ReporteHeatmap }) {
  if (!data) {
    return <Skeleton className="h-full w-full" />;
  }

  const mapa = new Map(data.franjas.map((franja) => [`${franja.diaSemana}-${franja.hora}`, franja.totalUSD]));
  const max = Math.max(1, ...data.franjas.map((franja) => franja.totalUSD));
  const pico = data.franjas.reduce((mejor, franja) => (franja.totalUSD > mejor.totalUSD ? franja : mejor), {
    diaSemana: 0,
    hora: 0,
    totalUSD: 0,
    cantidad: 0,
  });
  const hayPico = pico.totalUSD > 0;

  return (
    <div className="flex h-full flex-col justify-between gap-4">
      <div className="overflow-x-auto">
        <motion.div
          variants={contenedor}
          initial="oculto"
          animate="visible"
          className="flex min-w-[560px] flex-col gap-1"
        >
          <div className="flex gap-1">
            <div className="w-7 shrink-0" />
            <div className="flex flex-1 gap-1">
              {HORAS.map((hora) => (
                <div key={hora} className="flex-1 text-center text-[9px] text-muted-foreground num">
                  {hora % 3 === 0 ? hora : ''}
                </div>
              ))}
            </div>
          </div>
          {DIAS.map((dia, indiceDia) => (
            <motion.div key={dia} variants={item} className="flex items-center gap-1">
              <div className="w-7 shrink-0 text-[10px] text-muted-foreground">{dia}</div>
              <div className="flex flex-1 gap-1">
                {HORAS.map((hora) => {
                  const valor = mapa.get(`${indiceDia}-${hora}`) ?? 0;
                  const intensidad = valor / max;
                  const esPico = hayPico && pico.diaSemana === indiceDia && pico.hora === hora;
                  return (
                    <div
                      key={hora}
                      title={`${dia} ${hora}:00 · ${formatUSD(valor)}`}
                      className={cn(
                        'h-5 flex-1 rounded-[4px] border border-border/60',
                        esPico && 'ring-2 ring-ring-ink',
                      )}
                      style={{
                        backgroundColor:
                          valor > 0 ? `rgb(var(--color-accent) / ${0.15 + intensidad * 0.8})` : 'transparent',
                      }}
                    />
                  );
                })}
              </div>
            </motion.div>
          ))}
        </motion.div>
      </div>

      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex items-center gap-2 text-xs text-muted-foreground">
          <span>Menos</span>
          {[0.2, 0.4, 0.6, 0.8, 1].map((alpha) => (
            <span
              key={alpha}
              className="h-3 w-3 rounded-sm border border-border/60"
              style={{ backgroundColor: `rgb(var(--color-accent) / ${alpha})` }}
            />
          ))}
          <span>Más</span>
        </div>
        {hayPico && (
          <Pill tone="accent">
            Pico · {DIAS[pico.diaSemana]} {pico.hora}:00 — {formatUSD(pico.totalUSD)}
          </Pill>
        )}
      </div>
    </div>
  );
}

const ESTADOS_INVENTARIO: { clave: keyof DiagnosticoInventario; etiqueta: string; color: string }[] = [
  { clave: 'sinStock', etiqueta: 'Sin stock', color: 'bg-danger' },
  { clave: 'riesgoCritico', etiqueta: 'Crítico', color: 'bg-danger/70' },
  { clave: 'subabastecido', etiqueta: 'Subabastecido', color: 'bg-warning' },
  { clave: 'optimo', etiqueta: 'Óptimo', color: 'bg-success' },
  { clave: 'sobreabastecido', etiqueta: 'Sobreabastecido', color: 'bg-butter' },
  { clave: 'excesivo', etiqueta: 'Excesivo', color: 'bg-warning/60' },
];

function SaludInventario({ data }: { data?: DiagnosticoInventario }) {
  if (!data) return <Skeleton className="h-full w-full" />;

  const total = Math.max(1, data.total);
  const criticos = Number(data.sinStock) + Number(data.riesgoCritico);

  return (
    <div className="flex h-full flex-col justify-center gap-3">
      <div className="flex items-center justify-between gap-3">
        <span className="num text-2xl font-medium text-foreground">{formatNumber(data.total)}</span>
        <Pill tone={criticos > 0 ? 'danger' : 'success'}>{formatNumber(criticos)} críticos</Pill>
      </div>

      <div className="flex h-3 w-full overflow-hidden rounded-full border border-border">
        {ESTADOS_INVENTARIO.map((estado) => {
          const cantidad = Number(data[estado.clave] ?? 0);
          if (cantidad <= 0) return null;
          return (
            <div
              key={estado.clave}
              className={estado.color}
              style={{ width: `${(cantidad / total) * 100}%` }}
              title={`${estado.etiqueta}: ${cantidad}`}
            />
          );
        })}
      </div>

      <div className="grid grid-cols-2 gap-x-4 gap-y-1.5">
        {ESTADOS_INVENTARIO.map((estado) => (
          <span key={estado.clave} className="flex items-center gap-1.5 text-xs text-muted-foreground">
            <span className={cn('h-2.5 w-2.5 shrink-0 rounded-full', estado.color)} />
            <span className="truncate">{estado.etiqueta}</span>
            <span className="ml-auto font-medium text-foreground tabular-nums">
              {formatNumber(Number(data[estado.clave] ?? 0))}
            </span>
          </span>
        ))}
      </div>
    </div>
  );
}

/** Tipo mínimo de una consulta de TanStack Query que usa el Dashboard. */
interface Consulta<T> {
  data: T | undefined;
  isLoading: boolean;
  error: unknown;
}

function sinAcceso(error: unknown): boolean {
  return error instanceof ApiError && error.status === 403;
}

/** Texto del tile cuando la consulta no devolvió datos (cargando, sin acceso o con error). */
function textoDeEstado(consulta: Consulta<unknown>): string | null {
  if (consulta.data) return null;
  if (consulta.isLoading) return null;
  return sinAcceso(consulta.error) ? 'Sin acceso' : 'No disponible';
}

const ESQUELETO_TILE = <Skeleton className="h-full min-h-20 w-full" />;

function TileVacio({ label, icon, tone, mensaje, className }: {
  label: string;
  icon: ReactNode;
  tone: Tone;
  mensaje: string;
  className?: string;
}) {
  return (
    <Tile label={label} icon={icon} tone={tone} value={<span className="text-base text-muted-foreground">Sin datos</span>} hint={mensaje} className={className} />
  );
}

/** Serie diaria completa del periodo (los días sin ventas cuentan 0). */
function serieDelPeriodo(
  porDia: { fecha: string; totalUSD: number; cantidad: number }[],
  desde: string,
  hasta: string,
): (Punto & { cantidad: number })[] {
  const mapa = new Map(porDia.map((dia) => [dia.fecha.slice(0, 10), dia]));
  const filas: (Punto & { cantidad: number })[] = [];
  const [ay, am, ad] = desde.split('-').map(Number);
  const cursor = new Date(ay, am - 1, ad);
  const [hy, hm, hd] = hasta.split('-').map(Number);
  const fin = new Date(hy, hm - 1, hd);
  while (cursor <= fin && filas.length < 400) {
    const fila = mapa.get(fechaLocal(cursor));
    filas.push({
      etiqueta: `${cursor.getDate()}/${cursor.getMonth() + 1}`,
      totalUSD: fila?.totalUSD ?? 0,
      cantidad: fila?.cantidad ?? 0,
    });
    cursor.setDate(cursor.getDate() + 1);
  }
  return filas;
}

export function DashboardPage() {
  const periodo = usePeriodo();
  const { desde, hasta, valido } = periodo;
  const habilitada = valido;

  // Estado en vivo (no depende del periodo).
  const dashboard = useQuery({ queryKey: ['reportes', 'dashboard'], queryFn: reportesApi.dashboard });
  const salud = useQuery({ queryKey: ['reportes', 'inventario-salud'], queryFn: reportesApi.inventarioSalud });

  // Indicadores del periodo elegido: cada clave incluye el periodo para recalcular al cambiarlo.
  const ventas = useQuery({
    queryKey: ['reportes', 'ventas', desde, hasta],
    queryFn: () => reportesApi.ventas(desde, hasta),
    enabled: habilitada,
    retry: false,
  });
  const productos = useQuery({
    queryKey: ['reportes', 'productos', desde, hasta, 'Unidades', 5],
    queryFn: () => reportesApi.productos(desde, hasta, 'Unidades', 5),
    enabled: habilitada,
    retry: false,
  });
  const ocupacion = useQuery({
    queryKey: ['reportes', 'ocupacion', desde, hasta],
    queryFn: () => reportesApi.ocupacion(desde, hasta),
    enabled: habilitada,
    retry: false,
  });
  const asistencia = useQuery({
    queryKey: ['reportes', 'reservas-asistencia', desde, hasta],
    queryFn: () => reportesApi.reservasAsistencia(desde, hasta),
    enabled: habilitada,
    retry: false,
  });
  const caja = useQuery({
    queryKey: ['reportes', 'caja', desde, hasta],
    queryFn: () => reportesApi.caja(desde, hasta),
    enabled: habilitada,
    retry: false,
  });
  const mermas = useQuery({
    queryKey: ['reportes', 'mermas-vs-ventas', desde, hasta],
    queryFn: () => reportesApi.mermasVsVentas(desde, hasta),
    enabled: habilitada,
    retry: false,
  });
  const heatmap = useQuery({
    queryKey: ['reportes', 'heatmap', desde, hasta],
    queryFn: () => reportesApi.heatmap(desde, hasta),
    enabled: habilitada,
    retry: false,
  });

  const datos: Dashboard | undefined = dashboard.data;

  const serie = useMemo(
    () => (ventas.data ? serieDelPeriodo(ventas.data.porDia, desde, hasta) : []),
    [ventas.data, desde, hasta],
  );

  const etiquetaPeriodo = `${desde} al ${hasta}`;
  const cargaVentas = ventas.isLoading || (!ventas.data && habilitada && !ventas.error);

  return (
    <MotionConfig reducedMotion="user">
      <div className="flex flex-col gap-6">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h1 className="text-lg font-medium text-foreground">Dashboard</h1>
            <p className="text-xs text-muted-foreground">
              {valido ? `Periodo: ${etiquetaPeriodo}` : 'Elige un periodo válido para ver los indicadores.'}
            </p>
          </div>
          <SelectorPeriodo periodo={periodo} onElegir={periodo.elegir} onRango={periodo.fijarRango} />
        </div>

        <motion.div
          key={`${desde}-${hasta}`}
          variants={contenedor}
          initial="oculto"
          animate="visible"
          className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4"
          aria-label="Indicadores del periodo"
        >
          {/* Ventas del periodo */}
          <Tile
            hero
            tone="mint"
            icon={<TrendingUp size={16} />}
            label="Ventas del periodo"
            value={
              ventas.data ? (
                <Contador valor={ventas.data.totalUSD} formato={formatUSD} />
              ) : (
                <span className="text-base text-muted-foreground">{textoDeEstado(ventas) ?? '…'}</span>
              )
            }
            hint={ventas.data ? `${formatNumber(ventas.data.cantidad)} ventas` : undefined}
            className="sm:col-span-2"
          >
            <div className="mt-2 h-28">
              {cargaVentas ? (
                <Skeleton className="h-full w-full" />
              ) : ventas.data && serie.length > 0 ? (
                <AreaSpark data={serie} tone="mint" />
              ) : null}
            </div>
          </Tile>

          {/* Ticket promedio */}
          {ventas.data ? (
            ventas.data.cantidad > 0 ? (
              <Tile
                tone="mint"
                icon={<Receipt size={16} />}
                label="Ticket promedio"
                value={<Contador valor={ventas.data.ticketPromedioUSD} formato={formatUSD} />}
                hint={`${formatNumber(ventas.data.cantidad)} ventas completadas`}
              />
            ) : (
              <TileVacio label="Ticket promedio" icon={<Receipt size={16} />} tone="mint" mensaje="No hubo ventas en el periodo." />
            )
          ) : (
            <Tile label="Ticket promedio" icon={<Receipt size={16} />} tone="mint" value={textoDeEstado(ventas) ?? ESQUELETO_TILE} />
          )}

          {/* Producto más vendido */}
          {productos.data ? (
            productos.data.top.length > 0 ? (
              <Tile
                tone="lavender"
                icon={<Star size={16} />}
                label="Más vendido"
                value={<span className="text-lg">{productos.data.top[0].productoNombre}</span>}
                hint={`${formatNumber(productos.data.top[0].unidades)} unidades · ${formatPercent(productos.data.top[0].porcentajeUnidades)} del total`}
              />
            ) : (
              <TileVacio label="Más vendido" icon={<Star size={16} />} tone="lavender" mensaje="No hubo productos vendidos en el periodo." />
            )
          ) : (
            <Tile label="Más vendido" icon={<Star size={16} />} tone="lavender" value={textoDeEstado(productos) ?? ESQUELETO_TILE} />
          )}

          {/* Ocupación de mesas */}
          {ocupacion.data ? (
            ocupacion.data.mesasActivas > 0 ? (
              <Tile
                tone="sky"
                icon={<Armchair size={16} />}
                label="Ocupación de mesas"
                value={<Contador valor={ocupacion.data.porcentajeOcupacion} formato={(n) => formatPercent(n, 0)} />}
                hint={`${ocupacion.data.mesasOcupadas} de ${ocupacion.data.mesasActivas} mesas ocupadas ahora`}
              />
            ) : (
              <TileVacio label="Ocupación de mesas" icon={<Armchair size={16} />} tone="sky" mensaje="No hay mesas activas." />
            )
          ) : (
            <Tile label="Ocupación de mesas" icon={<Armchair size={16} />} tone="sky" value={textoDeEstado(ocupacion) ?? ESQUELETO_TILE} />
          )}

          {/* Tasa de no asistencia */}
          {asistencia.data ? (
            asistencia.data.tasaNoAsistencia !== null ? (
              <Tile
                tone="peach"
                icon={<CalendarX2 size={16} />}
                label="No asistencia a reservas"
                value={<Contador valor={asistencia.data.tasaNoAsistencia} formato={(n) => formatPercent(n, 1)} />}
                hint={`${asistencia.data.noAsistieron} de ${asistencia.data.asistieron + asistencia.data.noAsistieron} reservas no asistieron`}
              />
            ) : (
              <TileVacio
                label="No asistencia a reservas"
                icon={<CalendarX2 size={16} />}
                tone="peach"
                mensaje="Sin reservas que debieran concretarse."
              />
            )
          ) : (
            <Tile label="No asistencia a reservas" icon={<CalendarX2 size={16} />} tone="peach" value={textoDeEstado(asistencia) ?? ESQUELETO_TILE} />
          )}

          {/* Descuadre de caja */}
          {caja.data ? (
            caja.data.turnos > 0 ? (
              <Tile
                tone={caja.data.descuadreTotal < 0 ? 'rose' : 'mint'}
                icon={<Wallet size={16} />}
                label="Descuadre de caja"
                value={<Contador valor={caja.data.descuadreTotal} formato={formatUSD} />}
                hint={`${caja.data.turnos} turnos cerrados · mayor ${formatUSD(caja.data.mayorDescuadre)}`}
              />
            ) : (
              <TileVacio label="Descuadre de caja" icon={<Wallet size={16} />} tone="mint" mensaje="Sin turnos cerrados en el periodo." />
            )
          ) : (
            <Tile label="Descuadre de caja" icon={<Wallet size={16} />} tone="mint" value={textoDeEstado(caja) ?? ESQUELETO_TILE} />
          )}

          {/* Mermas */}
          {mermas.data ? (
            mermas.data.porMotivo.length > 0 ? (
              <Tile
                tone="peach"
                icon={<Trash2 size={16} />}
                label="Mermas del periodo"
                value={<Contador valor={mermas.data.mermaValorizadaUSD} formato={formatUSD} />}
                hint={`${formatPercent(mermas.data.porcentaje, 1)} de las ventas`}
              />
            ) : (
              <TileVacio label="Mermas del periodo" icon={<Trash2 size={16} />} tone="peach" mensaje="Sin mermas registradas." />
            )
          ) : (
            <Tile label="Mermas del periodo" icon={<Trash2 size={16} />} tone="peach" value={textoDeEstado(mermas) ?? ESQUELETO_TILE} />
          )}

          {/* Estado en vivo */}
          {datos ? (
            <>
              <Tile
                tone={datos.cajaAbierta ? 'mint' : 'rose'}
                icon={<Wallet size={16} />}
                label="Caja del turno"
                value={datos.cajaAbierta ? 'Abierta' : 'Cerrada'}
                hint={datos.cajaAbierta ? `Fondo inicial ${formatUSD(datos.cajaFondoInicial)}` : 'Sin turno en curso'}
              />
              <Tile
                tone="sky"
                icon={<LayoutGrid size={16} />}
                label="Cuentas abiertas"
                value={<Contador valor={datos.cuentasAbiertas} formato={formatNumber} />}
                hint="en curso ahora"
              />
              <Tile
                tone="lavender"
                icon={<CalendarClock size={16} />}
                label="Reservas próximas"
                value={<Contador valor={datos.reservasProximas} formato={formatNumber} />}
                hint="próximos 7 días"
              />
              <Tile
                tone="rose"
                icon={<Boxes size={16} />}
                label="Stock bajo"
                value={<Contador valor={datos.productosStockBajo} formato={formatNumber} />}
                hint="productos bajo mínimo"
              />
            </>
          ) : (
            dashboard.isLoading &&
            [0, 1, 2, 3].map((indice) => (
              <motion.div key={indice} variants={item} className="min-h-28">
                <Skeleton className="h-full w-full" />
              </motion.div>
            ))
          )}
        </motion.div>

        <motion.div
          key={`graficas-${desde}-${hasta}`}
          variants={contenedor}
          initial="oculto"
          animate="visible"
          className="grid grid-cols-1 gap-6 lg:grid-cols-2"
        >
          <Panel title="Ventas por día" aside={valido ? etiquetaPeriodo : '—'}>
            {cargaVentas ? (
              <Skeleton className="h-full w-full" />
            ) : serie.length > 0 && ventas.data && ventas.data.cantidad > 0 ? (
              <BarrasDia data={serie} />
            ) : (
              <p className="py-6 text-center text-sm text-muted-foreground">Sin ventas en el periodo.</p>
            )}
          </Panel>

          <Panel title="Productos más vendidos" aside="Top 5 por unidades">
            {productos.isLoading ? (
              <Skeleton className="h-full w-full" />
            ) : productos.data && productos.data.top.length > 0 ? (
              <BarrasProductos items={productos.data.top} />
            ) : (
              <p className="py-6 text-center text-sm text-muted-foreground">Sin productos vendidos en el periodo.</p>
            )}
          </Panel>

          <Panel title="Horas pico" aside="USD por día y hora">
            <Heatmap data={heatmap.data} />
          </Panel>

          <Panel title="Mermas por motivo" aside={mermas.data ? `${mermas.data.porcentaje}% de ventas` : '—'}>
            {mermas.data ? <BarrasMotivo items={mermas.data.porMotivo} /> : <Skeleton className="h-full w-full" />}
          </Panel>

          <Panel
            title="Salud del inventario"
            aside={salud.data ? `${formatNumber(salud.data.total)} variantes` : '—'}
          >
            <SaludInventario data={salud.data} />
          </Panel>
        </motion.div>
      </div>
    </MotionConfig>
  );
}
