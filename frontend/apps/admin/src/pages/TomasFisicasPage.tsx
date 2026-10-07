import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import {
  Button,
  Card,
  CardBody,
  CardHeader,
  CardTitle,
  DataTable,
  Input,
  Modal,
  PageHeader,
  Pagination,
  StatusBadge,
} from '@licoreria/ui';
import type { TomaFisica } from '@licoreria/types';
import { inventarioApi } from '@licoreria/api-client';
import { InventarioTabs } from '../components/InventarioTabs';
import { mensajeDeError } from '../lib/api';
import { formatDateTime, formatNumber } from '../lib/format';

export function TomasFisicasPage() {
  const [page, setPage] = useState(1);
  const [creando, setCreando] = useState(false);
  const [detalle, setDetalle] = useState<TomaFisica | null>(null);
  const [observaciones, setObservaciones] = useState('');
  const [conteos, setConteos] = useState<Record<string, string>>({});
  const queryClient = useQueryClient();

  const tomas = useQuery({ queryKey: ['tomas', page], queryFn: () => inventarioApi.tomas({ page, pageSize: 15 }) });
  const stock = useQuery({ queryKey: ['stock'], queryFn: () => inventarioApi.stock(), enabled: creando });

  const detallesCreados = useMemo(
    () =>
      Object.entries(conteos)
        .filter(([, valor]) => valor !== '')
        .map(([varianteId, valor]) => ({ varianteId, cantidadContada: Number(valor) })),
    [conteos],
  );

  const registrar = useMutation({
    mutationFn: () => inventarioApi.registrarToma({ detalles: detallesCreados, observaciones: observaciones || null }),
    onSuccess: () => {
      toast.success('Toma física registrada');
      setCreando(false);
      setObservaciones('');
      setConteos({});
      queryClient.invalidateQueries({ queryKey: ['tomas'] });
      queryClient.invalidateQueries({ queryKey: ['stock'] });
    },
    onError: (error) => toast.error('No se pudo registrar la toma', { description: mensajeDeError(error) }),
  });

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader
        title="Inventario"
        subtitle="Tomas físicas y ajustes por diferencia."
        actions={<Button onClick={() => setCreando(true)}>Nueva toma</Button>}
      />
      <InventarioTabs />

      <Card>
        <CardHeader>
          <CardTitle>Tomas registradas</CardTitle>
        </CardHeader>
        <CardBody>
          <DataTable<TomaFisica>
            rows={tomas.data?.items ?? []}
            loading={tomas.isLoading}
            rowKey={(toma) => toma.id}
            empty="Sin tomas registradas."
            columns={[
              { key: 'fecha', header: 'Fecha', render: (toma) => formatDateTime(toma.fecha) },
              { key: 'estado', header: 'Estado', render: (toma) => <StatusBadge status={toma.estado} /> },
              { key: 'lineas', header: 'Líneas', align: 'center', render: (toma) => toma.detalles.length },
              {
                key: 'diferencias',
                header: 'Con diferencia',
                align: 'center',
                render: (toma) => toma.detalles.filter((item) => item.diferencia !== 0).length,
              },
              { key: 'observaciones', header: 'Observaciones', render: (toma) => toma.observaciones ?? '—' },
              {
                key: 'acciones',
                header: '',
                align: 'right',
                render: (toma) => (
                  <Button size="sm" variant="ghost" onClick={() => setDetalle(toma)}>
                    Ver
                  </Button>
                ),
              },
            ]}
          />
          <Pagination page={page} totalPages={tomas.data?.totalPages ?? 1} onPageChange={setPage} />
        </CardBody>
      </Card>

      <Modal
        open={creando}
        onClose={() => setCreando(false)}
        title="Nueva toma física"
        className="max-w-3xl"
        footer={
          <>
            <Button variant="ghost" onClick={() => setCreando(false)}>
              Cancelar
            </Button>
            <Button
              disabled={detallesCreados.length === 0}
              loading={registrar.isPending}
              onClick={() => registrar.mutate()}
            >
              Registrar toma
            </Button>
          </>
        }
      >
        <div className="flex flex-col gap-3">
          <Input label="Observaciones" value={observaciones} onChange={(evento) => setObservaciones(evento.target.value)} />
          <div className="max-h-80 overflow-y-auto rounded-2xl border border-hairline">
            <table className="w-full min-w-[520px] text-sm">
              <thead className="sticky top-0 bg-elevated">
                <tr className="text-left text-xs uppercase tracking-tighter2 text-muted">
                  <th scope="col" className="px-3 py-2">Producto</th>
                  <th scope="col" className="px-3 py-2 text-right">Sistema</th>
                  <th scope="col" className="px-3 py-2 text-right">Contado</th>
                </tr>
              </thead>
              <tbody>
                {stock.data?.items.map((item) => (
                  <tr key={item.varianteId} className="border-t border-hairline">
                    <td className="px-3 py-2 text-ink">
                      {item.productoNombre}
                      <span className="ml-2 text-xs text-muted">{item.sku}</span>
                    </td>
                    <td className="px-3 py-2 text-right text-muted">{formatNumber(item.cantidad)}</td>
                    <td className="px-3 py-2 text-right">
                      <input
                        type="number"
                        min={0}
                        aria-label={`Contado ${item.sku}`}
                        value={conteos[item.varianteId] ?? ''}
                        onChange={(evento) => setConteos((actuales) => ({ ...actuales, [item.varianteId]: evento.target.value }))}
                        className="h-9 w-24 rounded-control border border-hairline bg-surface px-3 text-sm text-ink"
                      />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </Modal>

      <Modal open={Boolean(detalle)} onClose={() => setDetalle(null)} title="Detalle de toma" className="max-w-2xl">
        {detalle && (
          <div className="flex flex-col gap-3">
            <div className="flex items-center gap-3">
              <StatusBadge status={detalle.estado} />
              <span className="text-xs text-muted">{formatDateTime(detalle.fecha)}</span>
            </div>
            {detalle.observaciones && <p className="text-sm text-muted">{detalle.observaciones}</p>}
            <div className="overflow-x-auto">
              <table className="w-full min-w-[520px] text-sm">
                <thead>
                  <tr className="text-left text-xs uppercase tracking-tighter2 text-muted">
                    <th scope="col" className="py-2">SKU</th>
                    <th scope="col" className="py-2 text-right">Sistema</th>
                    <th scope="col" className="py-2 text-right">Contado</th>
                    <th scope="col" className="py-2 text-right">Diferencia</th>
                  </tr>
                </thead>
                <tbody>
                  {detalle.detalles.map((item) => (
                    <tr key={item.varianteId} className="border-t border-hairline">
                      <td className="py-2 text-ink">{item.sku}</td>
                      <td className="py-2 text-right text-muted">{formatNumber(item.cantidadSistema)}</td>
                      <td className="py-2 text-right text-muted">{formatNumber(item.cantidadContada)}</td>
                      <td
                        className={`py-2 text-right ${
                          item.diferencia === 0 ? 'text-muted' : item.diferencia > 0 ? 'text-success-ink' : 'text-danger-ink'
                        }`}
                      >
                        {item.diferencia > 0 ? '+' : ''}
                        {formatNumber(item.diferencia)}
                      </td>
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
