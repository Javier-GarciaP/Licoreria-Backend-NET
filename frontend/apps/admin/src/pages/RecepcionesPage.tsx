import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Button, Card, CardBody, CardHeader, CardTitle, DataTable, Modal, PageHeader, Pagination } from '@licoreria/ui';
import type { Recepcion } from '@licoreria/types';
import { comprasApi } from '@licoreria/api-client';
import { ComprasTabs } from '../components/ComprasTabs';
import { formatDateTime, formatNumber, formatUSD } from '../lib/format';

export function RecepcionesPage() {
  const [page, setPage] = useState(1);
  const [detalle, setDetalle] = useState<Recepcion | null>(null);

  const recepciones = useQuery({
    queryKey: ['recepciones', page],
    queryFn: () => comprasApi.recepciones({ page, pageSize: 15 }),
  });

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader title="Compras" subtitle="Recepciones de mercancía." />
      <ComprasTabs />

      <Card>
        <CardHeader>
          <CardTitle>Recepciones</CardTitle>
        </CardHeader>
        <CardBody>
          <DataTable<Recepcion>
            rows={recepciones.data?.items ?? []}
            loading={recepciones.isLoading}
            rowKey={(recepcion) => recepcion.id}
            empty="No hay recepciones."
            columns={[
              { key: 'orden', header: 'Orden', render: (recepcion) => <span className="text-ink">{recepcion.numeroOrden}</span> },
              { key: 'fecha', header: 'Fecha', render: (recepcion) => formatDateTime(recepcion.fecha) },
              { key: 'lineas', header: 'Líneas', align: 'center', render: (recepcion) => recepcion.detalles.length },
              { key: 'total', header: 'Total', align: 'right', render: (recepcion) => formatUSD(recepcion.totalUSD) },
              {
                key: 'acciones',
                header: '',
                align: 'right',
                render: (recepcion) => (
                  <Button size="sm" variant="ghost" onClick={() => setDetalle(recepcion)}>
                    Ver
                  </Button>
                ),
              },
            ]}
          />
          <Pagination page={page} totalPages={recepciones.data?.totalPages ?? 1} onPageChange={setPage} />
        </CardBody>
      </Card>

      <Modal open={Boolean(detalle)} onClose={() => setDetalle(null)} title={`Recepción · ${detalle?.numeroOrden ?? ''}`}>
        {detalle && (
          <div className="flex flex-col gap-3">
            <p className="text-xs text-muted">{formatDateTime(detalle.fecha)}</p>
            {detalle.observaciones && <p className="text-sm text-muted">{detalle.observaciones}</p>}
            <div className="overflow-x-auto">
              <table className="w-full min-w-[420px] text-sm">
                <thead>
                  <tr className="text-left text-xs uppercase tracking-tighter2 text-muted">
                    <th scope="col" className="py-2">SKU</th>
                    <th scope="col" className="py-2 text-right">Cantidad</th>
                    <th scope="col" className="py-2 text-right">Costo unit.</th>
                  </tr>
                </thead>
                <tbody>
                  {detalle.detalles.map((item) => (
                    <tr key={item.id} className="border-t border-hairline">
                      <td className="py-2 text-ink">{item.sku}</td>
                      <td className="py-2 text-right text-muted">{formatNumber(item.cantidad)}</td>
                      <td className="py-2 text-right text-muted">{formatUSD(item.costoUnitarioUSD)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}
