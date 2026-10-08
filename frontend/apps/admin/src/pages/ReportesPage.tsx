import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Card, CardBody, CardHeader, CardTitle, cn, DataTable, PageHeader, Pill, Skeleton } from '@licoreria/ui';
import { reportesApi } from '@licoreria/api-client';
import { formatDateTime, formatNumber, formatUSD } from '../lib/format';

type Pestana = 'ventas' | 'inventario' | 'compras' | 'propinas';

const PESTANAS: { id: Pestana; label: string }[] = [
  { id: 'ventas', label: 'Ventas' },
  { id: 'inventario', label: 'Inventario' },
  { id: 'compras', label: 'Compras' },
  { id: 'propinas', label: 'Propinas' },
];

function rangoPorDefecto() {
  const hasta = new Date();
  const desde = new Date();
  desde.setDate(desde.getDate() - 30);
  return { desde: desde.toISOString().slice(0, 10), hasta: hasta.toISOString().slice(0, 10) };
}

export function ReportesPage() {
  const [pestana, setPestana] = useState<Pestana>('ventas');
  const [rango, setRango] = useState(rangoPorDefecto);

  const ventas = useQuery({
    queryKey: ['reportes', 'ventas', rango],
    queryFn: () => reportesApi.ventas(rango.desde, rango.hasta),
    enabled: pestana === 'ventas',
  });
  const inventario = useQuery({
    queryKey: ['reportes', 'inventario'],
    queryFn: () => reportesApi.inventario(),
    enabled: pestana === 'inventario',
  });
  const compras = useQuery({
    queryKey: ['reportes', 'compras', rango],
    queryFn: () => reportesApi.compras(rango.desde, rango.hasta),
    enabled: pestana === 'compras',
  });
  const propinas = useQuery({
    queryKey: ['reportes', 'propinas', rango],
    queryFn: () => reportesApi.propinas(rango.desde, rango.hasta),
    enabled: pestana === 'propinas',
  });

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader
        title="Reportes"
        subtitle="Ventas, inventario valorizado, compras y propinas."
        actions={
          (pestana === 'ventas' || pestana === 'compras' || pestana === 'propinas') && (
            <div className="flex items-center gap-2">
              <input
                type="date"
                value={rango.desde}
                onChange={(evento) => setRango((r) => ({ ...r, desde: evento.target.value }))}
                className="h-9 rounded-control border border-border bg-card px-3 text-sm text-foreground"
              />
              <span className="text-xs text-muted-foreground">a</span>
              <input
                type="date"
                value={rango.hasta}
                onChange={(evento) => setRango((r) => ({ ...r, hasta: evento.target.value }))}
                className="h-9 rounded-control border border-border bg-card px-3 text-sm text-foreground"
              />
            </div>
          )
        }
      />

      <nav aria-label="Secciones de reportes" className="flex flex-wrap gap-2">
        {PESTANAS.map((item) => (
          <button
            key={item.id}
            type="button"
            onClick={() => setPestana(item.id)}
            className={cn(
              'rounded-full border px-4 py-1.5 text-sm transition',
              pestana === item.id ? 'border-primary text-foreground' : 'border-border text-muted-foreground hover:text-foreground',
            )}
          >
            {item.label}
          </button>
        ))}
      </nav>

      {pestana === 'ventas' && (
        <Card>
          <CardHeader>
            <CardTitle>Ventas del período</CardTitle>
            {ventas.data && <Pill tone="success">{ventas.data.cantidad} ventas</Pill>}
          </CardHeader>
          <CardBody className="flex flex-col gap-5">
            {ventas.isLoading || !ventas.data ? (
              <Skeleton className="h-64 w-full" />
            ) : (
              <>
                <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
                  <div className="rounded-xl border border-border p-4">
                    <p className="text-xs text-muted-foreground">Total USD</p>
                    <p className="num mt-1 text-xl font-medium text-foreground">{formatUSD(ventas.data.totalUSD)}</p>
                  </div>
                  <div className="rounded-xl border border-border p-4">
                    <p className="text-xs text-muted-foreground">Total Bs</p>
                    <p className="num mt-1 text-xl font-medium text-foreground">{formatNumber(ventas.data.totalBS)}</p>
                  </div>
                  <div className="rounded-xl border border-border p-4">
                    <p className="text-xs text-muted-foreground">Ticket promedio</p>
                    <p className="num mt-1 text-xl font-medium text-foreground">{formatUSD(ventas.data.ticketPromedioUSD)}</p>
                  </div>
                  <div className="rounded-xl border border-border p-4">
                    <p className="text-xs text-muted-foreground">Ventas</p>
                    <p className="num mt-1 text-xl font-medium text-foreground">{formatNumber(ventas.data.cantidad)}</p>
                  </div>
                </div>

                <div>
                  <p className="mb-2 text-sm font-medium text-foreground">Por día</p>
                  <DataTable
                    rows={ventas.data.porDia}
                    rowKey={(fila) => fila.fecha}
                    columns={[
                      { key: 'fecha', header: 'Fecha', render: (fila) => formatDateTime(fila.fecha) },
                      { key: 'cantidad', header: 'Ventas', align: 'right', render: (fila) => formatNumber(fila.cantidad) },
                      { key: 'total', header: 'Total', align: 'right', render: (fila) => formatUSD(fila.totalUSD) },
                    ]}
                  />
                </div>

                <div className="grid grid-cols-1 gap-6 xl:grid-cols-2">
                  <div>
                    <p className="mb-2 text-sm font-medium text-foreground">Por usuario</p>
                    <DataTable
                      rows={ventas.data.porUsuario}
                      rowKey={(fila) => fila.usuarioId}
                      empty="Sin datos."
                      columns={[
                        { key: 'usuario', header: 'Usuario', render: (fila) => fila.usuarioNombre },
                        { key: 'cantidad', header: 'Ventas', align: 'right', render: (fila) => formatNumber(fila.cantidad) },
                        { key: 'total', header: 'Total', align: 'right', render: (fila) => formatUSD(fila.totalUSD) },
                      ]}
                    />
                  </div>
                  <div>
                    <p className="mb-2 text-sm font-medium text-foreground">Por método de pago</p>
                    <DataTable
                      rows={ventas.data.porMetodoPago}
                      rowKey={(fila) => fila.metodoPagoId}
                      empty="Sin datos."
                      columns={[
                        { key: 'metodo', header: 'Método', render: (fila) => fila.metodoPagoNombre },
                        { key: 'moneda', header: 'Moneda', render: (fila) => fila.moneda },
                        { key: 'monto', header: 'Monto', align: 'right', render: (fila) => formatNumber(fila.monto) },
                      ]}
                    />
                  </div>
                </div>
              </>
            )}
          </CardBody>
        </Card>
      )}

      {pestana === 'inventario' && (
        <Card>
          <CardHeader>
            <CardTitle>Inventario valorizado</CardTitle>
            {inventario.data && <Pill tone="accent">{inventario.data.variantes} variantes</Pill>}
          </CardHeader>
          <CardBody className="flex flex-col gap-5">
            {inventario.isLoading || !inventario.data ? (
              <Skeleton className="h-64 w-full" />
            ) : (
              <>
                <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
                  <div className="rounded-xl border border-border p-4">
                    <p className="text-xs text-muted-foreground">Unidades</p>
                    <p className="num mt-1 text-xl font-medium text-foreground">{formatNumber(inventario.data.unidadesTotales)}</p>
                  </div>
                  <div className="rounded-xl border border-border p-4">
                    <p className="text-xs text-muted-foreground">Costo total</p>
                    <p className="num mt-1 text-xl font-medium text-foreground">{formatUSD(inventario.data.costoTotalUSD)}</p>
                  </div>
                  <div className="rounded-xl border border-border p-4">
                    <p className="text-xs text-muted-foreground">Valor de venta</p>
                    <p className="num mt-1 text-xl font-medium text-foreground">{formatUSD(inventario.data.valorVentaTotalUSD)}</p>
                  </div>
                  <div className="rounded-xl border border-border p-4">
                    <p className="text-xs text-muted-foreground">Bajo mínimo</p>
                    <p className="num mt-1 text-xl font-medium text-foreground">{formatNumber(inventario.data.bajoMinimo)}</p>
                  </div>
                </div>
                <DataTable
                  rows={inventario.data.items}
                  rowKey={(fila) => fila.varianteId}
                  columns={[
                    {
                      key: 'producto',
                      header: 'Producto',
                      render: (fila) => (
                        <span className="text-foreground">
                          {fila.productoNombre} <span className="text-xs text-muted-foreground">{fila.sku}</span>
                        </span>
                      ),
                    },
                    { key: 'cantidad', header: 'Cantidad', align: 'right', render: (fila) => formatNumber(fila.cantidad) },
                    { key: 'costo', header: 'Costo total', align: 'right', render: (fila) => formatUSD(fila.costoTotalUSD) },
                    { key: 'venta', header: 'Valor venta', align: 'right', render: (fila) => formatUSD(fila.valorVentaUSD) },
                  ]}
                />
              </>
            )}
          </CardBody>
        </Card>
      )}

      {pestana === 'compras' && (
        <Card>
          <CardHeader>
            <CardTitle>Compras del período</CardTitle>
            {compras.data && <Pill tone="accent">{compras.data.recepciones} recepciones</Pill>}
          </CardHeader>
          <CardBody>
            {compras.isLoading || !compras.data ? (
              <Skeleton className="h-40 w-full" />
            ) : (
              <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
                <div className="rounded-xl border border-border p-4">
                  <p className="text-xs text-muted-foreground">Comprado</p>
                  <p className="num mt-1 text-xl font-medium text-foreground">{formatUSD(compras.data.totalCompradoUSD)}</p>
                </div>
                <div className="rounded-xl border border-border p-4">
                  <p className="text-xs text-muted-foreground">Por pagar</p>
                  <p className="num mt-1 text-xl font-medium text-foreground">{formatUSD(compras.data.totalPorPagarUSD)}</p>
                </div>
                <div className="rounded-xl border border-border p-4">
                  <p className="text-xs text-muted-foreground">Pagado</p>
                  <p className="num mt-1 text-xl font-medium text-foreground">{formatUSD(compras.data.totalPagadoUSD)}</p>
                </div>
                <div className="rounded-xl border border-border p-4">
                  <p className="text-xs text-muted-foreground">Cuentas pendientes</p>
                  <p className="num mt-1 text-xl font-medium text-foreground">{formatNumber(compras.data.cuentasPendientes)}</p>
                </div>
              </div>
            )}
          </CardBody>
        </Card>
      )}

      {pestana === 'propinas' && (
        <Card>
          <CardHeader>
            <CardTitle>Propinas del período</CardTitle>
            {propinas.data && <Pill tone="success">{formatUSD(propinas.data.totalPropinaUSD)}</Pill>}
          </CardHeader>
          <CardBody>
            {propinas.isLoading || !propinas.data ? (
              <Skeleton className="h-40 w-full" />
            ) : (
              <DataTable
                rows={propinas.data.porUsuario}
                rowKey={(fila) => fila.usuarioId}
                empty="Sin propinas registradas."
                columns={[
                  { key: 'usuario', header: 'Usuario', render: (fila) => fila.usuarioNombre },
                  { key: 'ventas', header: 'Ventas', align: 'right', render: (fila) => formatNumber(fila.ventas) },
                  { key: 'total', header: 'Propina', align: 'right', render: (fila) => formatUSD(fila.totalPropinaUSD) },
                ]}
              />
            )}
          </CardBody>
        </Card>
      )}
    </div>
  );
}
