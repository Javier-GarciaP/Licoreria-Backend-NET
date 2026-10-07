import { useEffect, useId, useMemo, type ReactNode } from 'react';
import { animate, motion, MotionConfig, useMotionValue, useReducedMotion, useTransform, type Variants } from 'framer-motion';
import { useQuery } from '@tanstack/react-query';
import { Area, AreaChart, Bar, BarChart, Cell, ResponsiveContainer, Tooltip, XAxis } from 'recharts';
import {
  Boxes,
  CalendarClock,
  LayoutGrid,
  Package,
  Receipt,
  Trash2,
  TrendingUp,
  Users,
  Wallet,
} from 'lucide-react';
import { cn, Pill, Skeleton } from '@licoreria/ui';
import type { Dashboard, DiagnosticoInventario, ReporteHeatmap } from '@licoreria/types';
import { reportesApi } from '@licoreria/api-client';
import { formatNumber, formatUSD } from '../lib/format';

const DIAS = ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb'];
const HORAS = Array.from({ length: 24 }, (_, hora) => hora);

type Tone = 'mint' | 'sky' | 'lavender' | 'peach' | 'rose' | 'butter';

const TONE: Record<Tone, { chip: string; wash: string }> = {
  mint: { chip: 'bg-success/25 text-success-ink', wash: 'bg-success/25' },
  sky: { chip: 'bg-info/25 text-info-ink', wash: 'bg-info/25' },
  lavender: { chip: 'bg-accent/25 text-accent-ink', wash: 'bg-accent/25' },
  peach: { chip: 'bg-warning/25 text-warning-ink', wash: 'bg-warning/25' },
  rose: { chip: 'bg-danger/25 text-danger-ink', wash: 'bg-danger/25' },
  butter: { chip: 'bg-butter/25 text-butter-ink', wash: 'bg-butter/25' },
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

/** Tramas de carga del bento de KPIs (10 tiles: héroe 2 + caja 2 + 8 sencillos). */
const KPI_ESQUELETOS = [
  'col-span-2',
  'col-span-2',
  '',
  '',
  '',
  '',
  '',
  '',
  '',
  '',
];

function claveLocal(fecha: Date): string {
  return `${fecha.getFullYear()}-${String(fecha.getMonth() + 1).padStart(2, '0')}-${String(fecha.getDate()).padStart(2, '0')}`;
}

/**
 * Rango que cubre el mes en curso y, a la vez, los últimos 7 días.
 * Se envían fechas ISO con zona horaria (Z): el API rechaza fechas sin
 * zona al compararlas contra columnas `timestamp with time zone`.
 */
function rangoDashboard(): { desde: string; hasta: string } {
  const hoy = new Date();
  const inicioMes = new Date(hoy.getFullYear(), hoy.getMonth(), 1);
  const hace6 = new Date(hoy);
  hace6.setDate(hace6.getDate() - 6);
  const desde = inicioMes < hace6 ? inicioMes : hace6;
  return { desde: `${claveLocal(desde)}T00:00:00Z`, hasta: `${claveLocal(hoy)}T23:59:59Z` };
}

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

/** Barras compactas (por ejemplo, ventas por día del mes), resaltando el día de hoy. */
function MiniBars({ data, tone = 'mint' }: { data: Punto[]; tone?: Tone }) {
  const color = `rgb(var(${TONE_VAR[tone]}))`;
  const hoy = new Date().getDate();
  return (
    <ResponsiveContainer width="100%" height="100%">
      <BarChart data={data} margin={{ top: 2, right: 0, bottom: 0, left: 0 }}>
        <XAxis dataKey="etiqueta" hide />
        <Tooltip
          cursor={{ fill: 'rgb(var(--color-ink) / 0.05)' }}
          contentStyle={TOOLTIP_STYLE}
          formatter={(value: number) => formatUSD(value)}
          labelFormatter={(etiqueta) => `Día ${etiqueta}`}
        />
        <Bar dataKey="totalUSD" radius={[3, 3, 0, 0]} isAnimationActive animationDuration={800}>
          {data.map((punto) => (
            <Cell
              key={punto.etiqueta}
              fill={Number(punto.etiqueta) === hoy ? 'rgb(var(--color-accent-ink))' : color}
            />
          ))}
        </Bar>
      </BarChart>
    </ResponsiveContainer>
  );
}

function BarrasMotivo({ items }: { items: { motivo: string; valorUSD: number }[] }) {
  const max = Math.max(1, ...items.map((entrada) => entrada.valorUSD));
  if (items.length === 0) {
    return <p className="py-6 text-center text-sm text-muted">Sin mermas registradas.</p>;
  }
  return (
    <div className="flex h-full flex-col justify-center gap-3">
      {items.slice(0, 5).map((entrada) => (
        <div key={entrada.motivo} className="flex flex-col gap-1">
          <div className="flex items-center justify-between gap-2 text-xs">
            <span className="truncate capitalize text-muted">{entrada.motivo}</span>
            <span className="num shrink-0 text-ink">{formatUSD(entrada.valorUSD)}</span>
          </div>
          <div className="h-1.5 w-full overflow-hidden rounded-pill bg-elevated">
            <div
              className="h-full rounded-pill bg-warning"
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
      whileHover={{ y: -3 }}
      className={cn(
        'glass-card flex h-full flex-col overflow-hidden rounded-card p-5 transition-colors hover:border-accent/40',
        !children && 'justify-center',
        className,
      )}
    >
      <div className="flex items-center justify-between gap-3">
        <p className="text-xs font-medium text-muted">{label}</p>
        {icon && (
          <span className={cn('flex h-8 w-8 items-center justify-center rounded-full', TONE[tone].chip)}>{icon}</span>
        )}
      </div>
      <div className="relative mt-3 flex flex-wrap items-end justify-between gap-2">
        <p
          className={cn(
            'font-medium tracking-tightest text-ink',
            hero ? 'text-3xl' : 'text-2xl',
          )}
        >
          {value}
        </p>
        {badge}
      </div>
      {hint && <p className="mt-1 text-xs text-muted">{hint}</p>}
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
      whileHover={{ y: -3 }}
      className={cn(
        'glass-card flex h-full flex-col overflow-hidden rounded-card p-5 transition-colors hover:border-accent/40',
        className,
      )}
    >
      <div className="mb-3 flex shrink-0 items-center justify-between gap-3">
        <h2 className="text-sm font-medium tracking-tighter2 text-ink">{title}</h2>
        {aside && <span className="text-xs text-muted">{aside}</span>}
      </div>
      <div className="min-h-0 flex-1">{children}</div>
    </motion.div>
  );
}

function Heatmap({ data }: { data?: ReporteHeatmap }) {
  if (!data) {
    return <Skeleton className="h-44 w-full" />;
  }

  const mapa = new Map(data.franjas.map((franja) => [`${franja.diaSemana}-${franja.hora}`, franja.totalUSD]));
  const max = Math.max(1, ...data.franjas.map((franja) => franja.totalUSD));
  const pico = data.franjas.reduce(
    (mejor, franja) => (franja.totalUSD > mejor.totalUSD ? franja : mejor),
    { diaSemana: 0, hora: 0, totalUSD: 0, cantidad: 0 },
  );
  const hayPico = pico.totalUSD > 0;

  return (
    <div className="flex h-full flex-col justify-between gap-4">
      <motion.div
        variants={contenedor}
        initial="oculto"
        animate="visible"
        className="flex flex-col gap-1"
      >
        <div className="flex gap-1">
          <div className="w-7 shrink-0" />
          <div className="flex flex-1 gap-1">
            {HORAS.map((hora) => (
              <div key={hora} className="flex-1 text-center text-[9px] text-muted num">
                {hora % 3 === 0 ? hora : ''}
              </div>
            ))}
          </div>
        </div>
        {DIAS.map((dia, indiceDia) => (
          <motion.div key={dia} variants={item} className="flex items-center gap-1">
            <div className="w-7 shrink-0 text-[10px] text-muted">{dia}</div>
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
                      'h-5 flex-1 rounded-[4px] border border-hairline/60',
                      esPico && 'ring-2 ring-accent-ink',
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

      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex items-center gap-2 text-xs text-muted">
          <span>Menos</span>
          {[0.2, 0.4, 0.6, 0.8, 1].map((alpha) => (
            <span
              key={alpha}
              className="h-3 w-3 rounded-sm border border-hairline/60"
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
  if (!data) return <Skeleton className="h-24 w-full" />;

  const total = Math.max(1, data.total);
  const criticos = Number(data.sinStock) + Number(data.riesgoCritico);

  return (
    <div className="flex h-full flex-col justify-center gap-3">
      <div className="flex items-center justify-between gap-3">
        <span className="num text-2xl font-medium text-ink">{formatNumber(data.total)}</span>
        <Pill tone={criticos > 0 ? 'danger' : 'success'}>{formatNumber(criticos)} críticos</Pill>
      </div>

      <div className="flex h-3 w-full overflow-hidden rounded-pill border border-hairline">
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
          <span key={estado.clave} className="flex items-center gap-1.5 text-xs text-muted">
            <span className={cn('h-2.5 w-2.5 shrink-0 rounded-full', estado.color)} />
            <span className="truncate">{estado.etiqueta}</span>
            <span className="ml-auto font-medium text-ink tabular-nums">{formatNumber(Number(data[estado.clave] ?? 0))}</span>
          </span>
        ))}
      </div>
    </div>
  );
}

export function DashboardPage() {
  const dashboard = useQuery({ queryKey: ['reportes', 'dashboard'], queryFn: reportesApi.dashboard });
  const heatmap = useQuery({ queryKey: ['reportes', 'heatmap'], queryFn: () => reportesApi.heatmap() });
  const salud = useQuery({ queryKey: ['reportes', 'inventario-salud'], queryFn: reportesApi.inventarioSalud });
  const mermas = useQuery({ queryKey: ['reportes', 'mermas-vs-ventas'], queryFn: () => reportesApi.mermasVsVentas() });

  const rango = useMemo(() => rangoDashboard(), []);
  const ventas = useQuery({
    queryKey: ['reportes', 'ventas', rango.desde, rango.hasta],
    queryFn: () => reportesApi.ventas(rango.desde, rango.hasta),
  });

  const datos: Dashboard | undefined = dashboard.data;
  const cargando = dashboard.isLoading || !datos;

  const serie = useMemo(() => {
    const porDia = ventas.data?.porDia ?? [];
    const mapa = new Map(porDia.map((dia) => [dia.fecha.slice(0, 10), dia]));
    const hoy = new Date();
    const filas: (Punto & { cantidad: number })[] = [];
    for (let indice = 6; indice >= 0; indice -= 1) {
      const fecha = new Date(hoy);
      fecha.setDate(fecha.getDate() - indice);
      const fila = mapa.get(claveLocal(fecha));
      filas.push({
        etiqueta: `${fecha.getDate()}/${fecha.getMonth() + 1}`,
        totalUSD: fila?.totalUSD ?? 0,
        cantidad: fila?.cantidad ?? 0,
      });
    }
    return filas;
  }, [ventas.data]);

  const total7 = serie.reduce((acumulado, fila) => acumulado + fila.totalUSD, 0);
  const ayer = serie[5]?.totalUSD ?? 0;
  const ventasHoy = datos?.ventasHoyUSD ?? 0;
  const variacion = ayer > 0 ? ((ventasHoy - ayer) / ayer) * 100 : null;

  const serieMes = useMemo(() => {
    const porDia = ventas.data?.porDia ?? [];
    const mapa = new Map(porDia.map((dia) => [dia.fecha.slice(0, 10), dia]));
    const hoy = new Date();
    const filas: Punto[] = [];
    for (let dia = 1; dia <= hoy.getDate(); dia += 1) {
      const fecha = new Date(hoy.getFullYear(), hoy.getMonth(), dia);
      const fila = mapa.get(claveLocal(fecha));
      filas.push({ etiqueta: String(dia), totalUSD: fila?.totalUSD ?? 0 });
    }
    return filas;
  }, [ventas.data]);

  const ticketSerie = useMemo(
    () => serie.map((fila) => ({ etiqueta: fila.etiqueta, totalUSD: fila.cantidad > 0 ? fila.totalUSD / fila.cantidad : 0 })),
    [serie],
  );

  return (
    <MotionConfig reducedMotion="user">
      <div className="flex flex-col gap-6">
        <motion.div
          key={cargando ? 'carga' : 'datos'}
          variants={contenedor}
          initial="oculto"
          animate="visible"
          className="grid grid-cols-2 gap-4 lg:grid-cols-4"
        >
          {cargando || !datos ? (
            KPI_ESQUELETOS.map((clase, indice) => (
              <motion.div key={indice} variants={item} className={cn('min-h-28', clase)}>
                <Skeleton className="h-full w-full" />
              </motion.div>
            ))
          ) : (
            <>
              <Tile
                hero
                tone="mint"
                icon={<TrendingUp size={16} />}
                label="Ventas de hoy"
                value={<Contador valor={datos.ventasHoyUSD} formato={formatUSD} />}
                hint={`${formatNumber(datos.ventasHoyCantidad)} ventas · ticket ${formatUSD(datos.ticketPromedioUSD)}`}
                badge={
                  variacion !== null ? (
                    <Pill tone={variacion >= 0 ? 'success' : 'danger'}>
                      {variacion >= 0 ? '▲' : '▼'} {Math.abs(variacion).toFixed(0)}%
                    </Pill>
                  ) : undefined
                }
                className="col-span-2"
              >
                <div className="flex items-center justify-between text-xs text-muted">
                  <span>Últimos 7 días</span>
                  <span className="num text-ink">{formatUSD(total7)}</span>
                </div>
                <div className="mt-2 h-36">
                  {ventas.data ? <AreaSpark data={serie} tone="mint" /> : <Skeleton className="h-full w-full" />}
                </div>
              </Tile>

              <Tile
                tone={datos.cajaAbierta ? 'mint' : 'rose'}
                icon={<Wallet size={16} />}
                label="Caja del turno"
                value={datos.cajaAbierta ? 'Abierta' : 'Cerrada'}
                badge={
                  <Pill tone={datos.cajaAbierta ? 'success' : 'danger'}>
                    {datos.cajaAbierta ? 'Turno en curso' : 'Sin turno'}
                  </Pill>
                }
                className="col-span-2"
              >
                <div className="flex h-full flex-col justify-center gap-1.5 text-xs text-muted">
                  <p>
                    {datos.cajaAbierta
                      ? 'Caja operativa: los cobros de hoy se registran sobre esta caja.'
                      : 'No hay caja abierta: inicia el turno para poder registrar cobros.'}
                  </p>
                  <p>
                    El turno se abrió con un fondo inicial de{' '}
                    <span className="num text-ink">{formatUSD(datos.cajaFondoInicial)}</span>
                    {' '}· al cerrar se hace arqueo desde el módulo Caja.
                  </p>
                </div>
              </Tile>

              <Tile
                tone="sky"
                icon={<LayoutGrid size={16} />}
                label="Cuentas abiertas"
                value={<Contador valor={datos.cuentasAbiertas} formato={formatNumber} />}
                hint="en curso"
              />

              <Tile
                tone="lavender"
                icon={<CalendarClock size={16} />}
                label="Reservas próximas"
                value={<Contador valor={datos.reservasProximas} formato={formatNumber} />}
                hint="próximos 7 días"
              />

              <Tile
                tone="mint"
                icon={<Receipt size={16} />}
                label="Ventas del mes"
                value={<Contador valor={datos.ventasMesUSD} formato={formatUSD} />}
                hint={`${formatNumber(datos.ventasMesCantidad)} ventas`}
              >
                <div className="h-24">
                  {ventas.data ? <MiniBars data={serieMes} tone="mint" /> : <Skeleton className="h-full w-full" />}
                </div>
              </Tile>

              <Tile
                tone="mint"
                icon={<TrendingUp size={16} />}
                label="Ticket promedio"
                value={<Contador valor={datos.ticketPromedioUSD} formato={formatUSD} />}
                hint="tendencia diaria"
              >
                <div className="h-24">
                  {ventas.data ? <AreaSpark data={ticketSerie} tone="lavender" /> : <Skeleton className="h-full w-full" />}
                </div>
              </Tile>

              <Tile
                tone="rose"
                icon={<Boxes size={16} />}
                label="Stock bajo"
                value={<Contador valor={datos.productosStockBajo} formato={formatNumber} />}
                hint="productos bajo mínimo"
              />

              <Tile
                tone="peach"
                icon={<Trash2 size={16} />}
                label="Mermas del mes"
                value={<Contador valor={datos.mermasMesCantidad} formato={formatNumber} />}
                hint={`${formatNumber(datos.mermasMesUnidades)} unidades`}
              />

              <Tile
                tone="lavender"
                icon={<Users size={16} />}
                label="Clientes"
                value={<Contador valor={datos.clientes} formato={formatNumber} />}
              />

              <Tile
                tone="sky"
                icon={<Package size={16} />}
                label="Productos activos"
                value={<Contador valor={datos.productosActivos} formato={formatNumber} />}
              />
            </>
          )}
        </motion.div>

        <motion.div
          variants={contenedor}
          initial="oculto"
          animate="visible"
          className="grid grid-cols-1 gap-6 lg:grid-cols-2 xl:grid-cols-3"
        >
          <Panel title="Horas pico" aside="USD por día y hora">
            <Heatmap data={heatmap.data} />
          </Panel>

          <Panel
            title="Mermas por motivo"
            aside={mermas.data ? `${mermas.data.porcentaje}% de ventas` : '—'}
          >
            {mermas.data ? <BarrasMotivo items={mermas.data.porMotivo} /> : <Skeleton className="h-40 w-full" />}
          </Panel>

          <Panel title="Salud del inventario" aside={salud.data ? `${formatNumber(salud.data.total)} variantes` : '—'}>
            <SaludInventario data={salud.data} />
          </Panel>
        </motion.div>
      </div>
    </MotionConfig>
  );
}