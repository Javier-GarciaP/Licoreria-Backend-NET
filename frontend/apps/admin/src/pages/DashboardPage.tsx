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
  Coins,
  Download,
  Package,
  ShoppingCart,
  Users,
  CalendarClock,
  CalendarX2,
  LayoutGrid,
  Receipt,
  Star,
  Trash2,
  TrendingUp,
  Wallet,
} from 'lucide-react';
import { Button, Card, CardHeader, cn, Pill, Skeleton } from '@licoreria/ui';
import type { Dashboard, DiagnosticoInventario, ProductoVendido, ReporteHeatmap } from '@licoreria/types';
import { ApiError, reportesApi } from '@licoreria/api-client';
import { SelectorPeriodo } from '../components/SelectorPeriodo';
import { usePeriodo } from '../hooks/usePeriodo';
import { formatNumber, formatPercent, formatUSD } from '../lib/format';
import { fechaLocal } from '../lib/periodo';
import { exportarDashboardPdf, type SeccionPdf } from '../lib/dashboardPdf';

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
        'border border-border bg-card shadow-sm flex h-full flex-col overflow-hidden rounded-xl p-5 transition-colors hover:border-primary/40',
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
        'border border-border bg-card shadow-sm flex h-full flex-col overflow-hidden rounded-xl p-5 transition-colors hover:border-primary/40',
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

  const propinas = useQuery({
    queryKey: ['reportes', 'propinas', desde, hasta],
    queryFn: () => reportesApi.propinas(desde, hasta),
    enabled: habilitada,
    retry: false,
  });
  const compras = useQuery({
    queryKey: ['reportes', 'compras', desde, hasta],
    queryFn: () => reportesApi.compras(desde, hasta),
    enabled: habilitada,
    retry: false,
  });
  const inventario = useQuery({
    queryKey: ['reportes', 'inventario'],
    queryFn: reportesApi.inventario,
    retry: false,
  });
  const mesoneros = useQuery({
    queryKey: ['reportes', 'mesoneros', desde, hasta],
    queryFn: () => reportesApi.mesoneros(desde, hasta),
    enabled: habilitada,
    retry: false,
  });
  const topIngresos = useQuery({
    queryKey: ['reportes', 'productos', desde, hasta, 'Ingresos', 10],
    queryFn: () => reportesApi.productos(desde, hasta, 'Ingresos', 10),
    enabled: habilitada,
    retry: false,
  });

  const datos: Dashboard | undefined = dashboard.data;

  const serie = useMemo(
    () => (ventas.data ? serieDelPeriodo(ventas.data.porDia, desde, hasta) : []),
    [ventas.data, desde, hasta],
  );

  const etiquetaPeriodo = `${desde} al ${hasta}`;

  const diasPeriodo = Math.max(1, serie.length);
  const mejorDia = serie.reduce<(typeof serie)[number] | null>(
    (mejor, dia) => (dia.totalUSD > (mejor?.totalUSD ?? 0) ? dia : mejor),
    null,
  );
  const inv = inventario.data;
  const margenInventario = inv ? inv.valorVentaTotalUSD - inv.costoTotalUSD : 0;
  const margenPct = inv && inv.costoTotalUSD > 0 ? (margenInventario / inv.costoTotalUSD) * 100 : 0;
  const metodos = ventas.data?.porMetodoPago ?? [];
  const maxMetodo = Math.max(1, ...metodos.map((m) => m.monto));
  const topMesonero = mesoneros.data?.mesoneros?.[0];

  const exportarPdf = () => {
    const secciones: SeccionPdf[] = [
      {
        titulo: 'Ventas',
        kpis: ventas.data
          ? [
              ['Ventas del periodo (USD)', formatUSD(ventas.data.totalUSD)],
              ['Ventas del periodo (Bs)', formatNumber(ventas.data.totalBS)],
              ['Número de ventas', formatNumber(ventas.data.cantidad)],
              ['Ticket promedio', formatUSD(ventas.data.ticketPromedioUSD)],
              ['Promedio diario', formatUSD(ventas.data.totalUSD / diasPeriodo)],
              ...(mejorDia ? ([['Mejor día', `${mejorDia.etiqueta} (${formatUSD(mejorDia.totalUSD)})`]] as [string, string][]) : []),
              ...(datos
                ? ([
                    ['Ventas de hoy', formatUSD(datos.ventasHoyUSD)],
                    ['Ventas del mes', formatUSD(datos.ventasMesUSD)],
                  ] as [string, string][])
                : []),
            ]
          : undefined,
        tabla: metodos.length
          ? {
              cabecera: ['Método de pago', 'Moneda', 'Pagos', 'Monto'],
              filas: metodos.map((m) => [m.metodoPagoNombre, String(m.moneda), formatNumber(m.pagos), formatNumber(m.monto)]),
            }
          : undefined,
      },
      {
        titulo: 'Ventas por usuario',
        tabla: ventas.data?.porUsuario.length
          ? {
              cabecera: ['Usuario', 'Ventas', 'Total USD'],
              filas: ventas.data.porUsuario.map((u) => [u.usuarioNombre, formatNumber(u.cantidad), formatUSD(u.totalUSD)]),
            }
          : undefined,
      },
      {
        titulo: 'Productos más vendidos (por ingresos)',
        tabla: topIngresos.data?.top.length
          ? {
              cabecera: ['Producto', 'Unidades', 'Ingreso USD', '% ingreso'],
              filas: topIngresos.data.top.map((p) => [
                `${p.productoNombre}${p.varianteNombre ? ` · ${p.varianteNombre}` : ''}`,
                formatNumber(p.unidades),
                formatUSD(p.ingresoUSD),
                formatPercent(p.porcentajeIngreso),
              ]),
            }
          : undefined,
      },
      {
        titulo: 'Inventario',
        kpis: inv
          ? [
              ['Variantes en inventario', formatNumber(inv.variantes)],
              ['Unidades totales', formatNumber(inv.unidadesTotales)],
              ['Costo total del inventario', formatUSD(inv.costoTotalUSD)],
              ['Valor de venta del inventario', formatUSD(inv.valorVentaTotalUSD)],
              ['Margen potencial', `${formatUSD(margenInventario)} (${formatPercent(margenPct)})`],
              ['Variantes bajo mínimo', formatNumber(inv.bajoMinimo)],
              ...(salud.data
                ? ([
                    ['Sin stock', formatNumber(salud.data.sinStock)],
                    ['Riesgo crítico', formatNumber(salud.data.riesgoCritico)],
                    ['Sobreabastecido / excesivo', formatNumber(salud.data.sobreabastecido + salud.data.excesivo)],
                  ] as [string, string][])
                : []),
            ]
          : undefined,
      },
      {
        titulo: 'Compras y propinas',
        kpis: [
          ...(compras.data
            ? ([
                ['Recepciones', formatNumber(compras.data.recepciones)],
                ['Total comprado', formatUSD(compras.data.totalCompradoUSD)],
                ['Por pagar a proveedores', formatUSD(compras.data.totalPorPagarUSD)],
                ['Cuentas pendientes', formatNumber(compras.data.cuentasPendientes)],
              ] as [string, string][])
            : []),
          ...(propinas.data ? ([['Propinas del periodo', formatUSD(propinas.data.totalPropinaUSD)]] as [string, string][]) : []),
        ],
      },
      {
        titulo: 'Operación del salón',
        kpis: [
          ...(ocupacion.data
            ? ([
                ['Mesas ocupadas ahora', `${ocupacion.data.mesasOcupadas} de ${ocupacion.data.mesasActivas} (${formatPercent(ocupacion.data.porcentajeOcupacion, 0)})`],
                ['Sesiones del periodo', formatNumber(ocupacion.data.sesiones)],
                ['Permanencia promedio (min)', ocupacion.data.permanenciaPromedioMinutos === null ? '—' : formatNumber(Math.round(ocupacion.data.permanenciaPromedioMinutos))],
              ] as [string, string][])
            : []),
          ...(asistencia.data
            ? ([
                ['Reservas del periodo', formatNumber(asistencia.data.total)],
                ['No asistencia', asistencia.data.tasaNoAsistencia === null ? '—' : formatPercent(asistencia.data.tasaNoAsistencia, 1)],
                ['Reservas canceladas', formatNumber(asistencia.data.canceladas)],
              ] as [string, string][])
            : []),
          ...(caja.data
            ? ([
                ['Turnos de caja cerrados', formatNumber(caja.data.turnos)],
                ['Descuadre total', formatUSD(caja.data.descuadreTotal)],
              ] as [string, string][])
            : []),
          ...(mermas.data
            ? ([
                ['Mermas valorizadas', formatUSD(mermas.data.mermaValorizadaUSD)],
                ['Mermas sobre ventas', formatPercent(mermas.data.porcentaje, 1)],
              ] as [string, string][])
            : []),
          ...(datos
            ? ([
                ['Cuentas abiertas', formatNumber(datos.cuentasAbiertas)],
                ['Clientes registrados', formatNumber(datos.clientes)],
                ['Productos activos', formatNumber(datos.productosActivos)],
              ] as [string, string][])
            : []),
        ],
        tabla: ocupacion.data?.porZona.length
          ? {
              cabecera: ['Zona', 'Mesas activas', 'Ocupadas', '% ocupación'],
              filas: ocupacion.data.porZona.map((z) => [z.zona, formatNumber(z.mesasActivas), formatNumber(z.mesasOcupadas), formatPercent(z.porcentajeOcupacion, 0)]),
            }
          : undefined,
      },
      {
        titulo: 'Desempeño de mesoneros',
        tabla: mesoneros.data?.mesoneros.length
          ? {
              cabecera: ['Mesonero', 'Cuentas', 'Ventas USD', 'Ticket/cuenta', 'Cancelación'],
              filas: mesoneros.data.mesoneros.map((m) => [
                m.nombre,
                formatNumber(m.cuentas),
                formatUSD(m.ventasGeneradasUSD),
                formatUSD(m.ticketPorCuentaUSD),
                formatPercent(m.tasaCancelacion, 1),
              ]),
            }
          : undefined,
      },
    ];
    exportarDashboardPdf({
      local: 'Licorería',
      periodo: etiquetaPeriodo,
      secciones,
      nombreArchivo: `dashboard-${desde}_${hasta}.pdf`,
    });
  };
  const cargaVentas = ventas.isLoading || (!ventas.data && habilitada && !ventas.error);

  return (
    <MotionConfig reducedMotion="user">
      <div className="flex flex-col gap-6">
        <Card>
          <CardHeader className="flex flex-wrap items-end gap-3">
            <div className="flex flex-col gap-1">
              <span className="text-xs font-medium text-foreground">Periodo</span>
              <SelectorPeriodo periodo={periodo} onElegir={periodo.elegir} onRango={periodo.fijarRango} />
            </div>
            <p className="pb-2 text-xs text-muted-foreground">
              {valido ? etiquetaPeriodo : 'Elige un periodo válido para ver los indicadores.'}
            </p>
            <div className="ml-auto">
              <Button leftIcon={<Download size={15} />} onClick={exportarPdf} disabled={!valido || !ventas.data}>
                Exportar PDF
              </Button>
            </div>
          </CardHeader>
        </Card>

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

        <h2 className="text-sm font-medium text-foreground">Analítica ampliada</h2>
        <motion.div
          key={`extra-${desde}-${hasta}`}
          variants={contenedor}
          initial="oculto"
          animate="visible"
          className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4"
          aria-label="Indicadores adicionales"
        >
          <Tile tone="mint" icon={<TrendingUp size={16} />} label="Promedio diario" value={ventas.data ? formatUSD(ventas.data.totalUSD / diasPeriodo) : textoDeEstado(ventas) ?? ESQUELETO_TILE} hint={`${diasPeriodo} días en el periodo`} />
          <Tile tone="lavender" icon={<Star size={16} />} label="Mejor día" value={mejorDia ? formatUSD(mejorDia.totalUSD) : <span className="text-base text-muted-foreground">Sin datos</span>} hint={mejorDia ? `Día ${mejorDia.etiqueta} · ${mejorDia.cantidad} ventas` : undefined} />
          <Tile tone="sky" icon={<Wallet size={16} />} label="Ventas en Bs" value={ventas.data ? `Bs ${formatNumber(ventas.data.totalBS)}` : textoDeEstado(ventas) ?? ESQUELETO_TILE} hint="equivalente del periodo" />
          <Tile tone="butter" icon={<Coins size={16} />} label="Propinas" value={propinas.data ? formatUSD(propinas.data.totalPropinaUSD) : textoDeEstado(propinas) ?? ESQUELETO_TILE} hint="del periodo" />
          <Tile tone="sky" icon={<Package size={16} />} label="Inventario a costo" value={inv ? formatUSD(inv.costoTotalUSD) : textoDeEstado(inventario) ?? ESQUELETO_TILE} hint={inv ? `${formatNumber(inv.unidadesTotales)} unidades · ${formatNumber(inv.variantes)} variantes` : undefined} />
          <Tile tone="mint" icon={<Package size={16} />} label="Margen potencial" value={inv ? formatUSD(margenInventario) : textoDeEstado(inventario) ?? ESQUELETO_TILE} hint={inv ? `${formatPercent(margenPct)} sobre el costo` : undefined} />
          <Tile tone="peach" icon={<ShoppingCart size={16} />} label="Compras del periodo" value={compras.data ? formatUSD(compras.data.totalCompradoUSD) : textoDeEstado(compras) ?? ESQUELETO_TILE} hint={compras.data ? `${compras.data.recepciones} recepciones` : undefined} />
          <Tile tone="rose" icon={<Receipt size={16} />} label="Por pagar a proveedores" value={compras.data ? formatUSD(compras.data.totalPorPagarUSD) : textoDeEstado(compras) ?? ESQUELETO_TILE} hint={compras.data ? `${compras.data.cuentasPendientes} cuentas pendientes` : undefined} />
          <Tile tone="lavender" icon={<Users size={16} />} label="Clientes" value={datos ? formatNumber(datos.clientes) : ESQUELETO_TILE} hint="registrados" />
          <Tile tone="mint" icon={<Boxes size={16} />} label="Productos activos" value={datos ? formatNumber(datos.productosActivos) : ESQUELETO_TILE} hint="en catálogo" />
          <Tile tone="sky" icon={<Armchair size={16} />} label="Permanencia en mesa" value={ocupacion.data?.permanenciaPromedioMinutos != null ? `${formatNumber(Math.round(ocupacion.data.permanenciaPromedioMinutos))} min` : <span className="text-base text-muted-foreground">Sin datos</span>} hint={ocupacion.data ? `${formatNumber(ocupacion.data.sesiones)} sesiones` : undefined} />
          <Tile tone="peach" icon={<CalendarX2 size={16} />} label="Reservas del periodo" value={asistencia.data ? formatNumber(asistencia.data.total) : textoDeEstado(asistencia) ?? ESQUELETO_TILE} hint={asistencia.data ? `${asistencia.data.canceladas} canceladas` : undefined} />
        </motion.div>

        <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
          <Panel title="Ventas por método de pago" aside={valido ? etiquetaPeriodo : '—'}>
            {metodos.length > 0 ? (
              <div className="flex h-full flex-col justify-center gap-3">
                {metodos.map((m) => (
                  <div key={`${m.metodoPagoId}-${m.moneda}`} className="flex flex-col gap-1">
                    <div className="flex items-center justify-between gap-2 text-xs">
                      <span className="truncate text-foreground">{m.metodoPagoNombre} <span className="text-muted-foreground">· {String(m.moneda)} · {m.pagos} pagos</span></span>
                      <span className="num shrink-0 text-muted-foreground">{formatNumber(m.monto)}</span>
                    </div>
                    <div className="h-1.5 w-full overflow-hidden rounded-full bg-muted">
                      <div className="h-full rounded-full bg-info" style={{ width: `${(m.monto / maxMetodo) * 100}%` }} />
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <p className="py-6 text-center text-sm text-muted-foreground">Sin pagos en el periodo.</p>
            )}
          </Panel>

          <Panel title="Top productos por ingresos" aside="Top 10">
            {topIngresos.data && topIngresos.data.top.length > 0 ? (
              <div className="flex flex-col gap-2 text-xs">
                {topIngresos.data.top.map((p) => (
                  <div key={p.varianteId} className="flex items-center justify-between gap-2">
                    <span className="truncate text-foreground">{p.productoNombre}{p.varianteNombre ? <span className="text-muted-foreground"> · {p.varianteNombre}</span> : null}</span>
                    <span className="num shrink-0 text-muted-foreground">{formatUSD(p.ingresoUSD)} · {formatPercent(p.porcentajeIngreso)}</span>
                  </div>
                ))}
              </div>
            ) : (
              <p className="py-6 text-center text-sm text-muted-foreground">Sin ingresos por producto.</p>
            )}
          </Panel>

          <Panel title="Ventas por usuario" aside="Cajeros / vendedores">
            {ventas.data && ventas.data.porUsuario.length > 0 ? (
              <div className="flex flex-col gap-2 text-xs">
                {ventas.data.porUsuario.map((u) => (
                  <div key={u.usuarioId} className="flex items-center justify-between gap-2">
                    <span className="truncate text-foreground">{u.usuarioNombre}</span>
                    <span className="num shrink-0 text-muted-foreground">{formatUSD(u.totalUSD)} · {u.cantidad} ventas</span>
                  </div>
                ))}
              </div>
            ) : (
              <p className="py-6 text-center text-sm text-muted-foreground">Sin ventas por usuario.</p>
            )}
          </Panel>

          <Panel title="Desempeño de mesoneros" aside={topMesonero ? `Líder: ${topMesonero.nombre}` : '—'}>
            {mesoneros.data && mesoneros.data.mesoneros.length > 0 ? (
              <div className="flex flex-col gap-2 text-xs">
                {mesoneros.data.mesoneros.slice(0, 8).map((m) => (
                  <div key={m.usuarioId ?? m.nombre} className="flex items-center justify-between gap-2">
                    <span className="truncate text-foreground">{m.nombre}</span>
                    <span className="num shrink-0 text-muted-foreground">{formatUSD(m.ventasGeneradasUSD)} · {m.cuentas} cuentas · {formatPercent(m.tasaCancelacion, 1)} canc.</span>
                  </div>
                ))}
              </div>
            ) : (
              <p className="py-6 text-center text-sm text-muted-foreground">{mesoneros.error ? 'Disponible solo para administradores.' : 'Sin datos de mesoneros.'}</p>
            )}
          </Panel>

          <Panel title="Ocupación por zona" aside="Ahora">
            {ocupacion.data && ocupacion.data.porZona.length > 0 ? (
              <div className="flex flex-col gap-3">
                {ocupacion.data.porZona.map((z) => (
                  <div key={z.zonaId ?? z.zona} className="flex flex-col gap-1">
                    <div className="flex items-center justify-between gap-2 text-xs">
                      <span className="truncate text-foreground">{z.zona}</span>
                      <span className="num shrink-0 text-muted-foreground">{z.mesasOcupadas}/{z.mesasActivas} · {formatPercent(z.porcentajeOcupacion, 0)}</span>
                    </div>
                    <div className="h-1.5 w-full overflow-hidden rounded-full bg-muted">
                      <div className="h-full rounded-full bg-primary" style={{ width: `${Math.min(100, z.porcentajeOcupacion)}%` }} />
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <p className="py-6 text-center text-sm text-muted-foreground">Sin zonas con mesas activas.</p>
            )}
          </Panel>
        </div>
      </div>
    </MotionConfig>
  );
}
