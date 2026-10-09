import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Buscador, Card, CardBody, CardHeader, DataTable, FiltroDropdown, FiltroFechas, LimpiarFiltros, Pagination, Pill } from '@licoreria/ui';
import type { MovimientoKardex, TipoMovimientoInventario } from '@licoreria/types';
import { inventarioApi } from '@licoreria/api-client';
import { formatDateTime, formatNumber, formatUSD } from '../lib/format';
import { contiene, paginarEnMemoria, PAGE_SIZE_FILTRO_LOCAL } from '../lib/filtros';

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
  const [busqueda, setBusqueda] = useState('');

  const hayFiltroLocal = Boolean(busqueda);
  const hayFiltros = hayFiltroLocal || Boolean(tipo || desde || hasta);

  const limpiarFiltros = () => {
    setBusqueda('');
    setTipo('');
    setDesde('');
    setHasta('');
    setPage(1);
  };

  const kardex = useQuery({
    queryKey: ['kardex', page, tipo, desde, hasta, hayFiltroLocal ? PAGE_SIZE_FILTRO_LOCAL : 20],
    queryFn: () =>
      inventarioApi.kardex({
        page: hayFiltroLocal ? 1 : page,
        pageSize: hayFiltroLocal ? PAGE_SIZE_FILTRO_LOCAL : 20,
        tipo: tipo || undefined,
        desde: desde || undefined,
        hasta: hasta || undefined,
      }),
  });

  const { items: filas, totalPages } = useMemo(() => {
    const filtradas = (kardex.data?.items ?? []).filter((movimiento) => contiene(movimiento.sku, busqueda));
    return hayFiltroLocal
      ? paginarEnMemoria(filtradas, page, 20)
      : { items: filtradas, totalPages: kardex.data?.totalPages ?? 1 };
  }, [kardex.data, busqueda, hayFiltroLocal, page]);

  return (
    <div className="mx-auto flex max-w-page flex-col gap-4">
      <Card>
        <CardHeader className="flex flex-col items-stretch gap-3">
          <div className="flex flex-wrap items-center gap-2">
            <Buscador
              placeholder="Buscar SKU…"
              value={busqueda}
              onCambio={(valor) => {
                setBusqueda(valor);
                setPage(1);
              }}
            />
            <FiltroDropdown
              label="Tipo"
              opciones={TIPOS.map((valor) => ({ valor, etiqueta: valor }))}
              valor={tipo}
              onChange={(valor) => {
                setTipo(valor);
                setPage(1);
              }}
            />
            <FiltroFechas
              desde={desde}
              hasta={hasta}
              onDesde={(valor) => { setDesde(valor); setPage(1); }}
              onHasta={(valor) => { setHasta(valor); setPage(1); }}
            />
            <div className="ml-auto">
              <LimpiarFiltros activo={hayFiltros} onClick={limpiarFiltros} />
            </div>
          </div>
        </CardHeader>
        <CardBody>
          <DataTable<MovimientoKardex>
            rows={filas}
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
                  <span className={movimiento.cantidad < 0 ? 'text-destructive-fg' : 'text-success-fg'}>
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
          <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
        </CardBody>
      </Card>
    </div>
  );
}