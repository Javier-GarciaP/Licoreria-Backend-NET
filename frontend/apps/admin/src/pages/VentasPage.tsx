import { useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
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
import type { Venta } from '@licoreria/types';
import { ventasApi } from '@licoreria/api-client';
import { Can } from '../components/Rbac';
import { mensajeDeError } from '../lib/api';
import { formatDateTime, formatUSD } from '../lib/format';

const esquemaDevolucion = z.object({
  motivo: z.string().min(3, 'Describe el motivo (mínimo 3 caracteres)'),
});

type FormularioDevolucion = z.infer<typeof esquemaDevolucion>;

export function VentasPage() {
  const [page, setPage] = useState(1);
  const [detalle, setDetalle] = useState<Venta | null>(null);
  const [devolver, setDevolver] = useState<Venta | null>(null);
  const [reintegrar, setReintegrar] = useState(true);
  const [cantidades, setCantidades] = useState<Record<string, number>>({});
  const queryClient = useQueryClient();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormularioDevolucion>({ resolver: zodResolver(esquemaDevolucion), defaultValues: { motivo: '' } });

  const ventas = useQuery({ queryKey: ['ventas', page], queryFn: () => ventasApi.listar({ page, pageSize: 15 }) });

  const devolucion = useMutation({
    mutationFn: ({ venta, motivo }: { venta: Venta; motivo: string }) =>
      ventasApi.devolver(venta.id, {
        motivo,
        reintegrarInventario: reintegrar,
        detalles: Object.entries(cantidades)
          .filter(([, cantidad]) => cantidad > 0)
          .map(([varianteId, cantidad]) => ({ varianteId, cantidad })),
      }),
    onSuccess: () => {
      toast.success('Devolución registrada');
      setDevolver(null);
      reset({ motivo: '' });
      setCantidades({});
      queryClient.invalidateQueries({ queryKey: ['ventas'] });
    },
    onError: (error) => toast.error('No se pudo registrar la devolución', { description: mensajeDeError(error) }),
  });

  const totalDevolver = useMemo(
    () => (devolver ? devolver.detalles.filter((d) => (cantidades[d.varianteId] ?? 0) > 0).length : 0),
    [devolver, cantidades],
  );

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader title="Ventas" subtitle="Historial de ventas, comprobantes y devoluciones." />
      <Card>
        <CardHeader>
          <CardTitle>Historial</CardTitle>
        </CardHeader>
        <CardBody>
          <DataTable<Venta>
            rows={ventas.data?.items ?? []}
            loading={ventas.isLoading}
            rowKey={(venta) => venta.id}
            onRowClick={(venta) => setDetalle(venta)}
            empty="No hay ventas."
            columns={[
              { key: 'fecha', header: 'Fecha', render: (venta) => formatDateTime(venta.fecha) },
              { key: 'comprobante', header: 'Comprobante', render: (venta) => venta.numeroComprobante ?? venta.id.slice(0, 8) },
              { key: 'estado', header: 'Estado', render: (venta) => <StatusBadge status={venta.estado} /> },
              { key: 'total', header: 'Total', align: 'right', render: (venta) => formatUSD(venta.totalUSD) },
              { key: 'bs', header: 'Total Bs', align: 'right', render: (venta) => `Bs ${venta.totalBS.toFixed(2)}` },
              {
                key: 'acciones',
                header: '',
                align: 'right',
                render: (venta) => (
                  <Can permiso="sales:void">
                    <Button
                      size="sm"
                      variant="ghost"
                      onClick={() => {
                        setDevolver(venta);
                        reset({ motivo: '' });
                        setCantidades({});
                      }}
                    >
                      Devolución
                    </Button>
                  </Can>
                ),
              },
            ]}
          />
          <Pagination page={page} totalPages={ventas.data?.totalPages ?? 1} onPageChange={setPage} />
        </CardBody>
      </Card>

      <Modal open={Boolean(detalle)} onClose={() => setDetalle(null)} title={`Venta ${detalle?.numeroComprobante ?? ''}`}>
        {detalle && (
          <div className="flex flex-col gap-4">
            <div className="flex flex-col gap-1">
              {detalle.detalles.map((linea) => (
                <div key={linea.id} className="flex justify-between text-sm">
                  <span className="text-ink">
                    {linea.cantidad} × {linea.nombre} {linea.esCortesia && <span className="text-warning-ink">(cortesía)</span>}
                  </span>
                  <span className="text-muted">{formatUSD(linea.subtotalUSD)}</span>
                </div>
              ))}
            </div>
            <div className="border-t border-hairline pt-3 text-sm">
              <div className="flex justify-between text-muted">
                <span>Subtotal</span>
                <span>{formatUSD(detalle.subtotalUSD)}</span>
              </div>
              <div className="flex justify-between text-muted">
                <span>Descuento</span>
                <span>{formatUSD(detalle.descuentoUSD)}</span>
              </div>
              <div className="flex justify-between font-medium text-ink">
                <span>Total</span>
                <span>{formatUSD(detalle.totalUSD)}</span>
              </div>
            </div>
          </div>
        )}
      </Modal>

      <Modal
        open={Boolean(devolver)}
        onClose={() => setDevolver(null)}
        title="Registrar devolución"
        footer={
          <>
            <Button variant="ghost" onClick={() => setDevolver(null)}>
              Cancelar
            </Button>
            <Button
              type="submit"
              form="form-devolucion"
              disabled={totalDevolver === 0}
              loading={devolucion.isPending}
            >
              Registrar devolución
            </Button>
          </>
        }
      >
        {devolver && (
          <form
            id="form-devolucion"
            className="flex flex-col gap-4"
            onSubmit={handleSubmit((datos) => devolucion.mutate({ venta: devolver, motivo: datos.motivo }))}
            noValidate
          >
            <div className="flex flex-col gap-2">
              {devolver.detalles.map((linea) => (
                <div key={linea.id} className="flex items-center justify-between gap-3">
                  <span className="text-sm text-ink">{linea.nombre}</span>
                  <div className="flex items-center gap-2">
                    <span className="text-xs text-muted">de {linea.cantidad}</span>
                    <input
                      type="number"
                      min={0}
                      max={linea.cantidad}
                      aria-label={`Cantidad a devolver de ${linea.nombre}`}
                      value={cantidades[linea.varianteId] ?? 0}
                      onChange={(evento) =>
                        setCantidades((actuales) => ({
                          ...actuales,
                          [linea.varianteId]: Math.min(linea.cantidad, Math.max(0, Number(evento.target.value))),
                        }))
                      }
                      className="h-9 w-20 rounded-control border border-hairline bg-surface px-3 text-sm text-ink"
                    />
                  </div>
                </div>
              ))}
              {totalDevolver === 0 && (
                <span className="text-xs text-muted">Indica al menos una cantidad a devolver.</span>
              )}
            </div>
            <Input
              label="Motivo"
              placeholder="Producto dañado, error de cobro…"
              error={errors.motivo?.message}
              {...register('motivo')}
            />
            <label className="flex items-center gap-2 text-sm text-muted">
              <input type="checkbox" checked={reintegrar} onChange={(evento) => setReintegrar(evento.target.checked)} />
              Reintegrar al inventario
            </label>
          </form>
        )}
      </Modal>
    </div>
  );
}
