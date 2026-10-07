import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Card, CardBody, CardHeader, CardTitle, DataTable, Input, PageHeader, Pagination, Pill } from '@licoreria/ui';
import type { MovimientoKardex, TipoMovimientoInventario } from '@licoreria/types';
import { inventarioApi } from '@licoreria/api-client';
import { InventarioTabs } from '../components/InventarioTabs';
import { formatDateTime, formatNumber, formatUSD } from '../lib/format';

const TIPOS: TipoMovimientoInventario[] = ['Compra', 'Venta', 'Ajuste', 'Merma', 'Cortesia', 'ConsumoInterno'];

const tono: Record<string, 'success' | 'danger' | 'warning' | 'info' | 'accent' | 'neutral'> = {
  Compra: 'success',
  Venta: 'accent',
  Ajuste: 'info',
  Merma: 'danger',
  Cortesia: 'warning',
  ConsumoInterno: 'neutral',
};

export function KardexPage() {
  const [page, setPage] = useState(1);
  const [tipo, setTipo] = useState('');
  const [desde, setDesde] = useState('');
  const [hasta, setHasta] = useState('');

  const kardex = useQuery({
    queryKey: ['kardex', page, tipo, desde, hasta],
    queryFn: () => inventarioApi.kardex({ page, pageSize: 20, tipo: tipo || undefined, desde: desde || undefined, hasta: hasta || undefined }),
  });

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader title="Inventario" subtitle="Kardex inmutable de movimientos." />
      <InventarioTabs />

      <Card>
        <CardHeader>
          <CardTitle>Movimientos</CardTitle>
          <div className="flex flex-wrap items-end gap-2">
            <label className="flex flex-col gap-1.5">
              <span className="text-xs text-muted">Tipo</span>
              <select
                className="h-10 rounded-control border border-hairline bg-surface px-3 text-sm text-ink"
                value={tipo}
                onChange={(evento) => {
                  setTipo(evento.target.value);
                  setPage(1);
                }}
              >
                <option value="">Todos</option>
                {TIPOS.map((valor) => (
                  <option key={valor} value={valor}>
                    {valor}
                  </option>
                ))}
              </select>
            </label>
            <div className="w-40">
              <Input
                label="Desde"
                type="date"
                value={desde}
                onChange={(evento) => {
                  setDesde(evento.target.value);
                  setPage(1);
                }}
              />
            </div>
            <div className="w-40">
              <Input
                label="Hasta"
                type="date"
                value={hasta}
                onChange={(evento) => {
                  setHasta(evento.target.value);
                  setPage(1);
                }}
              />
            </div>
          </div>
        </CardHeader>
        <CardBody>
          <DataTable<MovimientoKardex>
            rows={kardex.data?.items ?? []}
            loading={kardex.isLoading}
            rowKey={(movimiento) => movimiento.id}
            empty="Sin movimientos."
            columns={[
              { key: 'fecha', header: 'Fecha', render: (movimiento) => formatDateTime(movimiento.fecha) },
              { key: 'sku', header: 'SKU', render: (movimiento) => movimiento.sku },
              { key: 'tipo', header: 'Tipo', render: (movimiento) => <Pill tone={tono[movimiento.tipo] ?? 'neutral'}>{movimiento.tipo}</Pill> },
              {
                key: 'cantidad',
                header: 'Cantidad',
                align: 'right',
                render: (movimiento) => (
                  <span className={movimiento.cantidad < 0 ? 'text-danger-ink' : 'text-success-ink'}>
                    {movimiento.cantidad > 0 ? '+' : ''}
                    {formatNumber(movimiento.cantidad)}
                  </span>
                ),
              },
              {
                key: 'costo',
                header: 'Costo unit.',
                align: 'right',
                render: (movimiento) => (movimiento.costoUnitario != null ? formatUSD(movimiento.costoUnitario) : '—'),
              },
              { key: 'motivo', header: 'Motivo', render: (movimiento) => movimiento.motivo ?? movimiento.referenciaTipo ?? '—' },
            ]}
          />
          <Pagination page={page} totalPages={kardex.data?.totalPages ?? 1} onPageChange={setPage} />
        </CardBody>
      </Card>
    </div>
  );
}
