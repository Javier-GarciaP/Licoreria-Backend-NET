import { useState } from 'react';
import { useFieldArray, useForm } from 'react-hook-form';
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
  Pill,
  Select,
} from '@licoreria/ui';
import type { EstadoOrdenCompra, OrdenCompra } from '@licoreria/types';
import { comprasApi, inventarioApi, proveedoresApi } from '@licoreria/api-client';
import { ComprasTabs } from '../components/ComprasTabs';
import { mensajeDeError } from '../lib/api';
import { formatDateTime, formatNumber, formatUSD } from '../lib/format';

const ESTADOS: EstadoOrdenCompra[] = ['Borrador', 'Aprobada', 'Enviada', 'RecibidaParcial', 'Recibida', 'Cancelada'];

const tono: Record<string, 'success' | 'danger' | 'warning' | 'info' | 'accent' | 'neutral'> = {
  Borrador: 'neutral',
  Aprobada: 'info',
  Enviada: 'accent',
  RecibidaParcial: 'warning',
  Recibida: 'success',
  Cancelada: 'danger',
};

const esquemaDetalle = z.object({
  varianteId: z.string().min(1, 'Selecciona la variante'),
  cantidad: z.coerce.number({ invalid_type_error: 'Cantidad inválida' }).positive('Debe ser mayor que 0'),
  costoUnitarioUSD: z.coerce.number({ invalid_type_error: 'Costo inválido' }).min(0, 'No puede ser negativo'),
});

const esquemaOrden = z.object({
  proveedorId: z.string().min(1, 'Selecciona el proveedor'),
  observaciones: z.string().optional(),
  detalles: z.array(esquemaDetalle).min(1, 'Agrega al menos una línea'),
});

type FormularioOrden = z.infer<typeof esquemaOrden>;

const VACIO: FormularioOrden = {
  proveedorId: '',
  observaciones: '',
  detalles: [{ varianteId: '', cantidad: 1, costoUnitarioUSD: 0 }],
};

export function ComprasPage() {
  const [page, setPage] = useState(1);
  const [estado, setEstado] = useState('');
  const [creando, setCreando] = useState(false);
  const [detalle, setDetalle] = useState<OrdenCompra | null>(null);
  const [recepcionando, setRecepcionando] = useState<OrdenCompra | null>(null);
  const [recepcionQty, setRecepcionQty] = useState<Record<string, string>>({});
  const [recepcionObs, setRecepcionObs] = useState('');
  const queryClient = useQueryClient();

  const ordenForm = useForm<FormularioOrden>({ resolver: zodResolver(esquemaOrden), defaultValues: VACIO });
  const { fields, append, remove } = useFieldArray({ control: ordenForm.control, name: 'detalles' });

  const ordenes = useQuery({
    queryKey: ['ordenes', page, estado],
    queryFn: () => comprasApi.ordenes({ page, pageSize: 15, estado: estado || undefined }),
  });
  const proveedores = useQuery({ queryKey: ['proveedores'], queryFn: () => proveedoresApi.listar() });
  const stock = useQuery({ queryKey: ['stock'], queryFn: () => inventarioApi.stock(), enabled: creando });

  const invalidar = () => {
    queryClient.invalidateQueries({ queryKey: ['ordenes'] });
    queryClient.invalidateQueries({ queryKey: ['recepciones'] });
    queryClient.invalidateQueries({ queryKey: ['cuentas-pagar'] });
  };

  const crear = useMutation({
    mutationFn: (datos: FormularioOrden) => comprasApi.crear(datos),
    onSuccess: () => {
      toast.success('Orden creada');
      setCreando(false);
      ordenForm.reset(VACIO);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo crear la orden', { description: mensajeDeError(error) }),
  });

  const accion = useMutation({
    mutationFn: ({ id, op }: { id: string; op: 'aprobar' | 'enviar' | 'cancelar' }) => comprasApi[op](id),
    onSuccess: (_data, variables) => {
      toast.success(`Orden ${variables.op === 'aprobar' ? 'aprobada' : variables.op === 'enviar' ? 'enviada' : 'cancelada'}`);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo actualizar la orden', { description: mensajeDeError(error) }),
  });

  const recepcionar = useMutation({
    mutationFn: (orden: OrdenCompra) =>
      comprasApi.registrarRecepcion({
        ordenCompraId: orden.id,
        observaciones: recepcionObs || null,
        detalles: Object.entries(recepcionQty)
          .filter(([, valor]) => Number(valor) > 0)
          .map(([ordenCompraDetalleId, valor]) => ({ ordenCompraDetalleId, cantidad: Number(valor) })),
      }),
    onSuccess: () => {
      toast.success('Recepción registrada');
      setRecepcionando(null);
      setRecepcionQty({});
      setRecepcionObs('');
      invalidar();
    },
    onError: (error) => toast.error('No se pudo recepcionar', { description: mensajeDeError(error) }),
  });

  const abrirRecepcion = (orden: OrdenCompra) => {
    setRecepcionQty(
      Object.fromEntries(orden.detalles.map((item) => [item.id, String(Math.max(0, item.cantidad - item.cantidadRecibida))])),
    );
    setRecepcionObs('');
    setRecepcionando(orden);
  };

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader
        title="Compras"
        subtitle="Órdenes de compra y su ciclo de aprobación."
        actions={
          <Button
            onClick={() => {
              ordenForm.reset(VACIO);
              setCreando(true);
            }}
          >
            Nueva orden
          </Button>
        }
      />
      <ComprasTabs />

      <Card>
        <CardHeader>
          <CardTitle>Órdenes</CardTitle>
          <Select aria-label="Filtrar por estado" value={estado} onChange={(evento) => { setEstado(evento.target.value); setPage(1); }}>
            <option value="">Todos los estados</option>
            {ESTADOS.map((valor) => (
              <option key={valor} value={valor}>
                {valor}
              </option>
            ))}
          </Select>
        </CardHeader>
        <CardBody>
          <DataTable<OrdenCompra>
            rows={ordenes.data?.items ?? []}
            loading={ordenes.isLoading}
            rowKey={(orden) => orden.id}
            empty="No hay órdenes."
            columns={[
              { key: 'numero', header: 'Número', render: (orden) => <span className="text-ink">{orden.numero}</span> },
              { key: 'proveedor', header: 'Proveedor', render: (orden) => orden.proveedorNombre },
              { key: 'fecha', header: 'Fecha', render: (orden) => formatDateTime(orden.fecha) },
              { key: 'estado', header: 'Estado', render: (orden) => <Pill tone={tono[orden.estado] ?? 'neutral'}>{orden.estado}</Pill> },
              { key: 'total', header: 'Total', align: 'right', render: (orden) => formatUSD(orden.totalUSD) },
              {
                key: 'acciones',
                header: '',
                align: 'right',
                render: (orden) => (
                  <div className="flex justify-end gap-2">
                    <Button size="sm" variant="ghost" onClick={() => setDetalle(orden)}>
                      Ver
                    </Button>
                    {orden.estado === 'Borrador' && (
                      <Button size="sm" variant="ghost" onClick={() => accion.mutate({ id: orden.id, op: 'aprobar' })}>
                        Aprobar
                      </Button>
                    )}
                    {orden.estado === 'Aprobada' && (
                      <Button size="sm" variant="ghost" onClick={() => accion.mutate({ id: orden.id, op: 'enviar' })}>
                        Enviar
                      </Button>
                    )}
                    {(orden.estado === 'Enviada' || orden.estado === 'RecibidaParcial') && (
                      <Button size="sm" onClick={() => abrirRecepcion(orden)}>
                        Recepcionar
                      </Button>
                    )}
                    {(orden.estado === 'Borrador' || orden.estado === 'Aprobada' || orden.estado === 'Enviada') && (
                      <Button size="sm" variant="ghost" onClick={() => accion.mutate({ id: orden.id, op: 'cancelar' })}>
                        Cancelar
                      </Button>
                    )}
                  </div>
                ),
              },
            ]}
          />
          <Pagination page={page} totalPages={ordenes.data?.totalPages ?? 1} onPageChange={setPage} />
        </CardBody>
      </Card>

      <Modal
        open={creando}
        onClose={() => setCreando(false)}
        title="Nueva orden de compra"
        className="max-w-3xl"
        footer={
          <>
            <Button variant="ghost" onClick={() => setCreando(false)}>
              Cancelar
            </Button>
            <Button type="submit" form="form-orden" loading={crear.isPending}>
              Crear orden
            </Button>
          </>
        }
      >
        <form id="form-orden" className="flex flex-col gap-3" onSubmit={ordenForm.handleSubmit((d) => crear.mutate(d))} noValidate>
          <Select label="Proveedor" error={ordenForm.formState.errors.proveedorId?.message} {...ordenForm.register('proveedorId')}>
            <option value="">Selecciona…</option>
            {proveedores.data?.map((proveedor) => (
              <option key={proveedor.id} value={proveedor.id}>
                {proveedor.nombre}
              </option>
            ))}
          </Select>
          <Input label="Observaciones" {...ordenForm.register('observaciones')} />

          <div className="rounded-2xl border border-hairline p-3">
            <div className="mb-2 flex items-center justify-between">
              <p className="text-xs font-medium uppercase tracking-tighter2 text-muted">Líneas</p>
              <Button
                type="button"
                size="sm"
                variant="ghost"
                onClick={() => append({ varianteId: '', cantidad: 1, costoUnitarioUSD: 0 })}
              >
                Agregar
              </Button>
            </div>
            {ordenForm.formState.errors.detalles?.message && (
              <p className="mb-2 text-xs text-danger-ink">{ordenForm.formState.errors.detalles.message}</p>
            )}
            <div className="flex flex-col gap-2">
              {fields.map((field, indice) => (
                <div key={field.id} className="grid grid-cols-2 gap-2 rounded-2xl bg-elevated/30 p-3 sm:grid-cols-4">
                  <Select aria-label="Variante" {...ordenForm.register(`detalles.${indice}.varianteId`)}>
                    <option value="">Producto…</option>
                    {stock.data?.items.map((item) => (
                      <option key={item.varianteId} value={item.varianteId}>
                        {item.productoNombre} · {item.sku}
                      </option>
                    ))}
                  </Select>
                  <Input type="number" placeholder="Cantidad" {...ordenForm.register(`detalles.${indice}.cantidad`)} />
                  <Input type="number" placeholder="Costo USD" {...ordenForm.register(`detalles.${indice}.costoUnitarioUSD`)} />
                  <Button type="button" variant="ghost" size="sm" onClick={() => remove(indice)}>
                    Quitar
                  </Button>
                </div>
              ))}
            </div>
          </div>
        </form>
      </Modal>

      <Modal open={Boolean(detalle)} onClose={() => setDetalle(null)} title={`Orden ${detalle?.numero ?? ''}`} className="max-w-2xl">
        {detalle && (
          <div className="flex flex-col gap-3">
            <div className="flex items-center gap-3">
              <Pill tone={tono[detalle.estado] ?? 'neutral'}>{detalle.estado}</Pill>
              <span className="text-sm text-muted">{detalle.proveedorNombre}</span>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full min-w-[520px] text-sm">
                <thead>
                  <tr className="text-left text-xs uppercase tracking-tighter2 text-muted">
                    <th scope="col" className="py-2">SKU</th>
                    <th scope="col" className="py-2 text-right">Cantidad</th>
                    <th scope="col" className="py-2 text-right">Recibida</th>
                    <th scope="col" className="py-2 text-right">Costo</th>
                    <th scope="col" className="py-2 text-right">Subtotal</th>
                  </tr>
                </thead>
                <tbody>
                  {detalle.detalles.map((item) => (
                    <tr key={item.id} className="border-t border-hairline">
                      <td className="py-2 text-ink">
                        {item.nombre}
                        <span className="ml-2 text-xs text-muted">{item.sku}</span>
                      </td>
                      <td className="py-2 text-right text-muted">{formatNumber(item.cantidad)}</td>
                      <td className="py-2 text-right text-muted">{formatNumber(item.cantidadRecibida)}</td>
                      <td className="py-2 text-right text-muted">{formatUSD(item.costoUnitarioUSD)}</td>
                      <td className="py-2 text-right text-ink">{formatUSD(item.subtotalUSD)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </Modal>

      <Modal
        open={Boolean(recepcionando)}
        onClose={() => setRecepcionando(null)}
        title={`Recepcionar orden ${recepcionando?.numero ?? ''}`}
        className="max-w-2xl"
        footer={
          <>
            <Button variant="ghost" onClick={() => setRecepcionando(null)}>
              Cancelar
            </Button>
            <Button
              disabled={Object.values(recepcionQty).every((valor) => Number(valor) <= 0)}
              loading={recepcionar.isPending}
              onClick={() => recepcionando && recepcionar.mutate(recepcionando)}
            >
              Registrar recepción
            </Button>
          </>
        }
      >
        {recepcionando && (
          <div className="flex flex-col gap-3">
            <Input label="Observaciones" value={recepcionObs} onChange={(evento) => setRecepcionObs(evento.target.value)} />
            <div className="flex flex-col gap-2">
              {recepcionando.detalles.map((item) => {
                const pendiente = Math.max(0, item.cantidad - item.cantidadRecibida);
                return (
                  <div key={item.id} className="flex items-center justify-between gap-3 rounded-2xl bg-elevated/40 px-3 py-2">
                    <div>
                      <p className="text-sm text-ink">
                        {item.nombre} <span className="text-xs text-muted">{item.sku}</span>
                      </p>
                      <p className="text-xs text-muted">Pendiente: {formatNumber(pendiente)}</p>
                    </div>
                    <input
                      type="number"
                      min={0}
                      max={pendiente}
                      aria-label={`Recibir ${item.sku}`}
                      value={recepcionQty[item.id] ?? ''}
                      onChange={(evento) =>
                        setRecepcionQty((actuales) => ({
                          ...actuales,
                          [item.id]: String(Math.min(pendiente, Math.max(0, Number(evento.target.value)))),
                        }))
                      }
                      className="h-9 w-24 rounded-control border border-hairline bg-surface px-3 text-sm text-ink"
                    />
                  </div>
                );
              })}
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}
