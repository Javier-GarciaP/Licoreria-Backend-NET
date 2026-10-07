import { useQuery } from '@tanstack/react-query';
import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import { Card, CardBody, CardHeader, CardTitle, Pill, Skeleton, StatusBadge } from '@licoreria/ui';
import type { Dashboard, DiagnosticoInventario, ReporteHeatmap } from '@licoreria/types';
import { reportesApi } from '@licoreria/api-client';
import { formatNumber, formatUSD } from '../lib/format';
import { PageHeader } from '@licoreria/ui';

const DIAS = ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb'];
const HORAS = Array.from({ length: 24 }, (_, hora) => hora);

function KpiCard({ label, value, hint }: { label: string; value: string; hint?: string }) {
  return (
    <Card className="p-5">
      <p className="text-xs font-medium uppercase tracking-tighter2 text-muted">{label}</p>
      <p className="mt-2 text-2xl font-semibold tracking-tightest text-ink">{value}</p>
      {hint && <p className="mt-1 text-xs text-muted">{hint}</p>}
    </Card>
  );
}

function Heatmap({ data }: { data?: ReporteHeatmap }) {
  if (!data) {
    return <Skeleton className="h-64 w-full" />;
  }

  const mapa = new Map(data.franjas.map((franja) => [`${franja.diaSemana}-${franja.hora}`, franja.totalUSD]));
  const max = Math.max(1, ...data.franjas.map((franja) => franja.totalUSD));

  return (
    <div className="overflow-x-auto pb-2">
      <div className="inline-block min-w-full">
        <div className="flex">
          <div className="w-10" />
          <div className="flex gap-1">
            {HORAS.map((hora) => (
              <div key={hora} className="w-6 text-center text-[9px] text-muted">
                {hora % 3 === 0 ? hora : ''}
              </div>
            ))}
          </div>
        </div>
        {DIAS.map((dia, indiceDia) => (
          <div key={dia} className="flex items-center">
            <div className="w-10 text-[10px] text-muted">{dia}</div>
            <div className="flex gap-1 py-0.5">
              {HORAS.map((hora) => {
                const valor = mapa.get(`${indiceDia}-${hora}`) ?? 0;
                const intensidad = valor / max;
                return (
                  <div
                    key={hora}
                    title={`${dia} ${hora}:00 · ${formatUSD(valor)}`}
                    className="h-6 w-6 rounded-md border border-hairline/60"
                    style={{
                      backgroundColor:
                        valor > 0 ? `rgba(var(--color-accent), ${0.15 + intensidad * 0.85})` : 'transparent',
                    }}
                  />
                );
              })}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

function InventarioSalud({ data }: { data?: DiagnosticoInventario }) {
  if (!data) return <Skeleton className="h-52 w-full" />;

  const criticos = data.items
    .filter((item) => item.estado !== 'Optimo')
    .sort((a, b) => a.cantidad - b.cantidad)
    .slice(0, 7);

  return (
    <div className="flex flex-col gap-4">
      <div className="flex flex-wrap gap-2">
        <Pill tone="danger">Sin stock: {data.sinStock}</Pill>
        <Pill tone="danger">Crítico: {data.riesgoCritico}</Pill>
        <Pill tone="warning">Subabastecido: {data.subabastecido}</Pill>
        <Pill tone="success">Óptimo: {data.optimo}</Pill>
        <Pill tone="warning">Sobreabastecido: {data.sobreabastecido}</Pill>
        <Pill tone="danger">Excesivo: {data.excesivo}</Pill>
      </div>

      <div className="overflow-x-auto">
        <table className="w-full min-w-[520px] text-sm">
          <thead>
            <tr className="text-left text-xs uppercase tracking-tighter2 text-muted">
              <th className="py-2">Producto</th>
              <th className="py-2 text-right">Actual</th>
              <th className="py-2 text-right">Mín</th>
              <th className="py-2 text-right">Máx</th>
              <th className="py-2 text-right">Comprar</th>
              <th className="py-2 text-right">Estado</th>
            </tr>
          </thead>
          <tbody>
            {criticos.map((item) => (
              <tr key={item.varianteId} className="border-t border-hairline">
                <td className="py-2 text-ink">
                  {item.productoNombre}
                  <span className="ml-2 text-xs text-muted">{item.sku}</span>
                </td>
                <td className="py-2 text-right text-ink">{formatNumber(item.cantidad)}</td>
                <td className="py-2 text-right text-muted">{formatNumber(item.stockMinimo)}</td>
                <td className="py-2 text-right text-muted">{formatNumber(item.stockMaximo)}</td>
                <td className="py-2 text-right text-ink">{formatNumber(item.unidadesCompraSugeridas)}</td>
                <td className="py-2 text-right">
                  <StatusBadge status={item.estado} />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

export function DashboardPage() {
  const dashboard = useQuery({ queryKey: ['reportes', 'dashboard'], queryFn: reportesApi.dashboard });
  const heatmap = useQuery({ queryKey: ['reportes', 'heatmap'], queryFn: () => reportesApi.heatmap() });
  const salud = useQuery({ queryKey: ['reportes', 'inventario-salud'], queryFn: reportesApi.inventarioSalud });
  const mermas = useQuery({ queryKey: ['reportes', 'mermas-vs-ventas'], queryFn: () => reportesApi.mermasVsVentas() });

  const datos: Dashboard | undefined = dashboard.data;

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader title="Dashboard" subtitle="Indicadores del negocio, horas pico y salud del inventario." />

      {dashboard.isLoading || !datos ? (
        <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
          {Array.from({ length: 8 }).map((_, indice) => (
            <Skeleton key={indice} className="h-28 w-full" />
          ))}
        </div>
      ) : (
        <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
          <KpiCard label="Ventas hoy" value={formatUSD(datos.ventasHoyUSD)} hint={`${datos.ventasHoyCantidad} ventas`} />
          <KpiCard label="Ventas del mes" value={formatUSD(datos.ventasMesUSD)} hint={`${datos.ventasMesCantidad} ventas`} />
          <KpiCard label="Ticket promedio" value={formatUSD(datos.ticketPromedioUSD)} />
          <KpiCard label="Cuentas abiertas" value={formatNumber(datos.cuentasAbiertas)} />
          <KpiCard label="Stock bajo" value={formatNumber(datos.productosStockBajo)} hint="productos bajo mínimo" />
          <KpiCard label="Reservas próximas" value={formatNumber(datos.reservasProximas)} hint="próximos 7 días" />
          <KpiCard label="Mermas del mes" value={formatNumber(datos.mermasMesCantidad)} hint={`${formatNumber(datos.mermasMesUnidades)} unidades`} />
          <KpiCard label="Caja" value={datos.cajaAbierta ? 'Abierta' : 'Cerrada'} hint={formatUSD(datos.cajaFondoInicial)} />
        </div>
      )}

      <Card>
        <CardHeader>
          <CardTitle>Mapa de calor · horas pico</CardTitle>
          <span className="text-xs text-muted">Consumo por día y hora (USD)</span>
        </CardHeader>
        <CardBody>
          <Heatmap data={heatmap.data} />
        </CardBody>
      </Card>

      <div className="grid grid-cols-1 gap-6 xl:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Salud del inventario</CardTitle>
            <span className="text-xs text-muted">Mínimos y máximos</span>
          </CardHeader>
          <CardBody>
            <InventarioSalud data={salud.data} />
          </CardBody>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Mermas vs ventas</CardTitle>
            <span className="text-xs text-muted">
              {mermas.data ? `${mermas.data.porcentaje}% de las ventas` : '—'}
            </span>
          </CardHeader>
          <CardBody>
            {mermas.data ? (
              <div className="flex flex-col gap-4">
                <div className="flex gap-3">
                  <Pill tone="accent">Ventas {formatUSD(mermas.data.ventasUSD)}</Pill>
                  <Pill tone="danger">Mermas {formatUSD(mermas.data.mermaValorizadaUSD)}</Pill>
                </div>
                <div className="h-52 w-full">
                  <ResponsiveContainer width="100%" height="100%">
                    <BarChart data={mermas.data.porMotivo}>
                      <CartesianGrid strokeDasharray="3 3" stroke="rgb(var(--color-hairline))" />
                      <XAxis dataKey="motivo" tick={{ fill: 'rgb(var(--color-muted))', fontSize: 11 }} />
                      <YAxis tick={{ fill: 'rgb(var(--color-muted))', fontSize: 11 }} />
                      <Tooltip
                        contentStyle={{
                          background: 'rgb(var(--color-elevated))',
                          border: '1px solid rgb(var(--color-hairline))',
                          borderRadius: 14,
                          color: 'rgb(var(--color-ink))',
                        }}
                        formatter={(value: number) => formatUSD(value)}
                      />
                      <Bar dataKey="valorUSD" fill="rgb(var(--color-accent))" radius={[8, 8, 0, 0]} />
                    </BarChart>
                  </ResponsiveContainer>
                </div>
              </div>
            ) : (
              <Skeleton className="h-52 w-full" />
            )}
          </CardBody>
        </Card>
      </div>
    </div>
  );
}
