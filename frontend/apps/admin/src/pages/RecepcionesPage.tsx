import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Eye } from 'lucide-react';
import { ActionMenu, Card, CardBody, CardHeader, CardTitle, DataTable, Modal, ModalSection, Pagination } from '@licoreria/ui';
import type { Recepcion } from '@licoreria/types';
import { comprasApi } from '@licoreria/api-client';
import { ComprasTabs } from '../components/ComprasTabs';
import { FolderPanel } from '../components/FolderTabs';
import { formatDateTime, formatNumber, formatUSD } from '../lib/format';

export function RecepcionesPage() {
  const [page, setPage] = useState(1);
  const [detalle, setDetalle] = useState<Recepcion | null>(null);

  const recepciones = useQuery({
    queryKey: ['recepciones', page],
    queryFn: () => comprasApi.recepciones({ page, pageSize: 15 }),
  });

  return (
    <div className="mx-auto flex max-w-page flex-col gap-4">
      <ComprasTabs />

      <div className="flex flex-col">
      <FolderPanel className="flex flex-col gap-4">
        <Card className="border-0 bg-transparent shadow-none">
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
                { key: 'orden', header: 'Orden', render: (recepcion) => <span className="text-foreground">{recepcion.numeroOrden}</span> },
                { key: 'fecha', header: 'Fecha', render: (recepcion) => formatDateTime(recepcion.fecha) },
                { key: 'lineas', header: 'Líneas', align: 'center', render: (recepcion) => recepcion.detalles.length },
                { key: 'total', header: 'Total', align: 'right', render: (recepcion) => formatUSD(recepcion.totalUSD) },
                {
                  key: 'acciones',
                  header: '',
                  align: 'right',
                  render: (recepcion) => (
                    <ActionMenu
                      label={`Acciones de recepción ${recepcion.numeroOrden}`}
                      options={[{ label: 'Ver detalle', icon: <Eye size={15} />, onClick: () => setDetalle(recepcion) }]}
                    />
                  ),
                },
              ]}
            />
            <Pagination page={page} totalPages={recepciones.data?.totalPages ?? 1} onPageChange={setPage} />
          </CardBody>
        </Card>
      </FolderPanel>
      </div>

      <Modal
        open={Boolean(detalle)}
        onClose={() => setDetalle(null)}
        title={`Recepción · ${detalle?.numeroOrden ?? ''}`}
        size="lg"
        backdrop="none"
      >
        {detalle && (
          <div className="flex flex-col gap-5">
            <p className="text-sm text-muted-foreground">{formatDateTime(detalle.fecha)}</p>
            {detalle.observaciones && <p className="text-sm text-muted-foreground">{detalle.observaciones}</p>}
            <ModalSection title="Líneas recibidas">
              <div className="overflow-x-auto rounded-inner border border-border">
                <table className="w-full min-w-[420px] text-sm">
                  <thead>
                    <tr className="bg-muted/60 text-left text-xs uppercase tracking-tighter2 text-muted-foreground">
                      <th scope="col" className="px-4 py-2.5">SKU</th>
                      <th scope="col" className="px-4 py-2.5 text-right">Cantidad</th>
                      <th scope="col" className="px-4 py-2.5 text-right">Costo unit.</th>
                      <th scope="col" className="px-4 py-2.5 text-right">Subtotal</th>
                    </tr>
                  </thead>
                  <tbody>
                    {detalle.detalles.map((item) => (
                      <tr key={item.id} className="border-t border-border">
                        <td className="px-4 py-2.5 text-foreground">{item.sku}</td>
                        <td className="num px-4 py-2.5 text-right text-muted-foreground">{formatNumber(item.cantidad)}</td>
                        <td className="num px-4 py-2.5 text-right text-muted-foreground">{formatUSD(item.costoUnitarioUSD)}</td>
                        <td className="num px-4 py-2.5 text-right text-foreground">
                          {formatUSD(Number(item.cantidad) * Number(item.costoUnitarioUSD))}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </ModalSection>
            <div className="flex justify-end border-t border-border pt-3">
              <div className="flex items-baseline gap-3">
                <span className="text-sm text-muted-foreground">Total</span>
                <span className="num text-lg font-medium text-foreground">{formatUSD(detalle.totalUSD)}</span>
              </div>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}