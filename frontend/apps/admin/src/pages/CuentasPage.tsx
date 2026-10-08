import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { Card, CardBody, CardHeader, CardTitle, DataTable, Pagination, StatusBadge } from '@licoreria/ui';
import type { Cuenta } from '@licoreria/types';
import { cuentasApi } from '@licoreria/api-client';
import { formatUSD, haceCuanto } from '../lib/format';

export function CuentasPage() {
  const [page, setPage] = useState(1);
  const navigate = useNavigate();
  const cuentas = useQuery({
    queryKey: ['cuentas', 'lista', page],
    queryFn: () => cuentasApi.listar({ page, pageSize: 15 }),
  });

  return (
    <div className="mx-auto flex max-w-page flex-col gap-4">
      <Card>
        <CardHeader>
          <CardTitle>Listado</CardTitle>
        </CardHeader>
        <CardBody>
          <DataTable<Cuenta>
            rows={cuentas.data?.items ?? []}
            loading={cuentas.isLoading}
            rowKey={(cuenta) => cuenta.id}
            onRowClick={(cuenta) => navigate(`/cuentas/${cuenta.id}`)}
            empty="No hay cuentas."
            columns={[
              { key: 'mesa', header: 'Mesa', render: (cuenta) => <span className="text-ink">{cuenta.nombreMesa}</span> },
              { key: 'estado', header: 'Estado', render: (cuenta) => <StatusBadge status={cuenta.estado} /> },
              { key: 'total', header: 'Total', align: 'right', render: (cuenta) => formatUSD(cuenta.total) },
              { key: 'abonado', header: 'Abonado', align: 'right', render: (cuenta) => formatUSD(cuenta.totalAbonado) },
              {
                key: 'saldo',
                header: 'Saldo',
                align: 'right',
                render: (cuenta) => <span className="font-medium text-accent-ink">{formatUSD(cuenta.saldo)}</span>,
              },
              { key: 'tiempo', header: 'Abierta', align: 'right', render: (cuenta) => haceCuanto(cuenta.abiertaEn) },
            ]}
          />
          <Pagination page={page} totalPages={cuentas.data?.totalPages ?? 1} onPageChange={setPage} />
        </CardBody>
      </Card>
    </div>
  );
}
