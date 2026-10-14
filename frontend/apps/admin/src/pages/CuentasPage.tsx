import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { Buscador, Card, CardBody, CardHeader, DataTable, FiltroDropdown, FiltroRango, LimpiarFiltros, PageHeader, Pagination, StatusBadge } from '@licoreria/ui';
import type { Cuenta } from '@licoreria/types';
import { cuentasApi } from '@licoreria/api-client';
import { formatUSD, haceCuanto } from '../lib/format';
import { contiene, paginarEnMemoria, PAGE_SIZE_FILTRO_LOCAL } from '../lib/filtros';

const OPCIONES_ESTADO = [
  { valor: '', etiqueta: 'Todas' },
  { valor: 'Abierta', etiqueta: 'Abiertas' },
  { valor: 'PorCobrar', etiqueta: 'Por cobrar' },
  { valor: 'Cerrada', etiqueta: 'Cerradas' },
];

export function CuentasPage() {
  const [estado, setEstado] = useState('Abierta');
  const [busqueda, setBusqueda] = useState('');
  const [saldoMin, setSaldoMin] = useState('');
  const [saldoMax, setSaldoMax] = useState('');
  const [page, setPage] = useState(1);
  const navigate = useNavigate();

  const hayFiltroLocal = Boolean(busqueda || saldoMin || saldoMax);
  const hayFiltros = hayFiltroLocal || Boolean(estado);

  const limpiarFiltros = () => {
    setEstado('');
    setBusqueda('');
    setSaldoMin('');
    setSaldoMax('');
    setPage(1);
  };

  const cuentas = useQuery({
    queryKey: ['cuentas', 'lista', estado, page, hayFiltroLocal ? PAGE_SIZE_FILTRO_LOCAL : 15],
    queryFn: () =>
      cuentasApi.listar({
        estado: estado || undefined,
        page: hayFiltroLocal ? 1 : page,
        pageSize: hayFiltroLocal ? PAGE_SIZE_FILTRO_LOCAL : 15,
      }),
  });

  const { items: filas, totalPages } = useMemo(() => {
    const saldoMinN = Number(saldoMin);
    const saldoMaxN = Number(saldoMax);
    const filtradas = (cuentas.data?.items ?? []).filter((cuenta) => {
      if (!contiene(`${cuenta.nombreMesa} ${cuenta.cliente ?? ''}`, busqueda)) return false;
      if (saldoMinN && cuenta.saldo < saldoMinN) return false;
      if (saldoMaxN && cuenta.saldo > saldoMaxN) return false;
      return true;
    });
    return hayFiltroLocal
      ? paginarEnMemoria(filtradas, page, 15)
      : { items: filtradas, totalPages: cuentas.data?.totalPages ?? 1 };
  }, [cuentas.data, busqueda, saldoMin, saldoMax, hayFiltroLocal, page]);

  return (
    <div className="mx-auto flex max-w-page flex-col gap-4">
      <PageHeader title="Cuentas" subtitle="Monitoreo y cobro de las cuentas del salón." />

      <Card>
        <CardHeader className="flex flex-col items-stretch gap-3">
          <div className="flex flex-wrap items-center gap-2">
            <Buscador
              placeholder="Buscar mesa o cliente…"
              value={busqueda}
              onCambio={(valor) => {
                setBusqueda(valor);
                setPage(1);
              }}
            />
            <FiltroDropdown
              label="Estado"
              opciones={OPCIONES_ESTADO}
              valor={estado}
              onChange={(valor) => {
                setEstado(valor);
                setPage(1);
              }}
            />
          </div>
          <div className="border-b border-border" />
          <div className="flex flex-wrap items-center gap-2">
            <FiltroRango
              label="Saldo USD"
              minimo={saldoMin}
              maximo={saldoMax}
              onMinimo={(valor) => {
                setSaldoMin(valor);
                setPage(1);
              }}
              onMaximo={(valor) => {
                setSaldoMax(valor);
                setPage(1);
              }}
            />
            <div className="ml-auto">
              <LimpiarFiltros activo={hayFiltros} onClick={limpiarFiltros} />
            </div>
          </div>
        </CardHeader>
        <CardBody>
          <DataTable<Cuenta>
            rows={filas}
            loading={cuentas.isLoading}
            rowKey={(cuenta) => cuenta.id}
            onRowClick={(cuenta) => navigate(`/cuentas/${cuenta.id}`)}
            empty="No hay cuentas."
            columns={[
              { key: 'mesa', header: 'Mesa', render: (cuenta) => <span className="text-foreground">{cuenta.nombreMesa}</span> },
              { key: 'estado', header: 'Estado', render: (cuenta) => <StatusBadge status={cuenta.estado} /> },
              { key: 'consumos', header: 'Consumos', align: 'right', render: (cuenta) => cuenta.comandas.reduce((total, comanda) => total + comanda.detalles.length, 0) },
              { key: 'total', header: 'Total', align: 'right', render: (cuenta) => formatUSD(cuenta.total) },
              { key: 'abonado', header: 'Abonado', align: 'right', render: (cuenta) => formatUSD(cuenta.totalAbonado) },
              {
                key: 'saldo',
                header: 'Saldo',
                align: 'right',
                render: (cuenta) => <span className="num font-medium text-foreground">{formatUSD(cuenta.saldo)}</span>,
              },
              { key: 'tiempo', header: 'Abierta', align: 'right', render: (cuenta) => haceCuanto(cuenta.abiertaEn) },
            ]}
          />
          <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
        </CardBody>
      </Card>
    </div>
  );
}