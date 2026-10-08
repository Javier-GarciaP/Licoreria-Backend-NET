import { useState } from 'react';
import { useFieldArray, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { CheckCircle2, Eye, Plus, Send, Truck, XCircle } from 'lucide-react';
import {
  ActionMenu,
  Button,
  Card,
  CardBody,
  CardHeader,
  CardTitle,
  DataTable,
  Input,
  Modal,
  ModalSection,
  Pagination,
  Pill,
  Select,
  type ActionMenuOption,
} from '@licoreria/ui';
import type { EstadoOrdenCompra, OrdenCompra } from '@licoreria/types';
import { comprasApi, inventarioApi, proveedoresApi } from '@licoreria/api-client';
import { ComprasTabs } from '../components/ComprasTabs';
import { FolderPanel } from '../components/FolderTabs';
import { InlineForm } from '../components/InlineForm';
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
    <div className="mx-auto flex max-w-page flex-col gap-4">
      <ComprasTabs />
      <div className="flex flex-col">
      <FolderPanel className="flex flex-col gap-4">
        {creando && (
          <InlineForm title="Nueva orden de compra" onCancel={() => setCreando(false)}>
            <form
              className="flex flex-col gap-3"
              onSubmit={ordenForm.handleSubmit((d) => crear.mutate(d))}
              noValidate
            >
              <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                <Select label="Proveedor" error={ordenForm.formState.errors.proveedorId?.message} {...ordenForm.register('proveedorId')}>
                  <option value="">Selecciona…</option>
                  {proveedores.data?.map((proveedor) => (
                    <option key={proveedor.id} value={proveedor.id}>
                      {proveedor.nombre}
                    </option>
                  ))}
                </Select>
                <Input label="Observaciones" {...ordenForm.register('observaciones')} />
              </div>

              <div className="rounded-inner border border-border bg-muted/30 p-3">
                <div className="mb-2 flex items-center justify-between">
                  <p className="text-xs font-medium uppercase tracking-tighter2 text-muted-foreground">Líneas</p>
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
                  <p className="mb-2 text-xs text-destructive-fg">{ordenForm.formState.errors.detalles.message}</p>
                )}
                <div className="flex flex-col gap-2">
                  {fields.map((field, indice) => (
                    <div key={field.id} className="grid grid-cols-2 gap-2 rounded-inner bg-card p-3 sm:grid-cols-4">
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

              <div className="flex justify-end gap-2">
                <Button variant="ghost" onClick={() => setCreando(false)}>
                  Cancelar
                </Button>
                <Button type="submit" loading={crear.isPending}>
                  Crear orden
                </Button>
              </div>
            </form>
          </InlineForm>
        )}

        <Card className="border-0 bg-transparent shadow-none">
          <CardHeader>
            <CardTitle>Órdenes</CardTitle>
            <div className="flex flex-wrap items-center gap-2">
              <Select aria-label="Filtrar por estado" value={estado} onChange={(evento) => { setEstado(evento.target.value); setPage(1); }}>
                <option value="">Todos los estados</option>
                {ESTADOS.map((valor) => (
                  <option key={valor} value={valor}>
                    {valor}
                  </option>
                ))}
              </Select>
              <Button
                size="sm"
                leftIcon={<Plus size={15} />}
                onClick={() => {
                  ordenForm.reset(VACIO);
                  setCreando(true);
                }}
              >
                Nueva orden
              </Button>
            </div>
          </CardHeader>
          <CardBody>
            <DataTable<OrdenCompra>
              rows={ordenes.data?.items ?? []}
              loading={ordenes.isLoading}
              rowKey={(orden) => orden.id}
              empty="No hay órdenes."
              expandirKey={recepcionando?.id ?? null}
              expandedRow={
                recepcionando
                  ? (orden) =>
                      orden.id === recepcionando.id ? (
                        <InlineForm title={`Recepcionar · ${orden.numero}`} onCancel={() => setRecepcionando(null)}>
                          <div className="flex flex-col gap-3">
                            <Input
                              label="Observaciones"
                              value={recepcionObs}
                              onChange={(evento) => setRecepcionObs(evento.target.value)}
                            />
                            <div className="flex flex-col gap-2">
                              {recepcionando.detalles.map((item) => {
                                const pendiente = Math.max(0, item.cantidad - item.cantidadRecibida);
                                return (
                                  <div key={item.id} className="flex items-center justify-between gap-3 rounded-inner bg-muted/40 px-3 py-2">
                                    <div>
                                      <p className="text-sm text-foreground">
                                        {item.nombre} <span className="text-xs text-muted-foreground">{item.sku}</span>
                                      </p>
                                      <p className="text-xs text-muted-foreground">Pendiente: {formatNumber(pendiente)}</p>
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
                                      className="num h-9 w-24 rounded-control border border-border bg-card px-3 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-ring/50"
                                    />
                                  </div>
                                );
                              })}
                            </div>
                            <div className="flex justify-end gap-2">
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
                            </div>
                          </div>
                        </InlineForm>
                      ) : null
                  : undefined
              }
              columns={[
                { key: 'numero', header: 'Número', render: (orden) => <span className="text-foreground">{orden.numero}</span> },
                { key: 'proveedor', header: 'Proveedor', render: (orden) => orden.proveedorNombre },
                { key: 'fecha', header: 'Fecha', render: (orden) => formatDateTime(orden.fecha) },
                { key: 'estado', header: 'Estado', render: (orden) => <Pill tone={tono[orden.estado] ?? 'neutral'}>{orden.estado}</Pill> },
                { key: 'total', header: 'Total', align: 'right', render: (orden) => formatUSD(orden.totalUSD) },
                {
                  key: 'acciones',
                  header: '',
                  align: 'right',
                  render: (orden) => {
                    const opciones: ActionMenuOption[] = [
                      { label: 'Ver detalle', icon: <Eye size={15} />, onClick: () => setDetalle(orden) },
                    ];
                    if (orden.estado === 'Borrador') {
                      opciones.push({ label: 'Aprobar', icon: <CheckCircle2 size={15} />, onClick: () => accion.mutate({ id: orden.id, op: 'aprobar' }) });
                      opciones.push({ label: 'Cancelar', icon: <XCircle size={15} />, danger: true, onClick: () => accion.mutate({ id: orden.id, op: 'cancelar' }) });
                    } else if (orden.estado === 'Aprobada') {
                      opciones.push({ label: 'Enviar', icon: <Send size={15} />, onClick: () => accion.mutate({ id: orden.id, op: 'enviar' }) });
                      opciones.push({ label: 'Cancelar', icon: <XCircle size={15} />, danger: true, onClick: () => accion.mutate({ id: orden.id, op: 'cancelar' }) });
                    } else if (orden.estado === 'Enviada' || orden.estado === 'RecibidaParcial') {
                      opciones.push({ label: 'Recepcionar', icon: <Truck size={15} />, onClick: () => abrirRecepcion(orden) });
                      if (orden.estado === 'Enviada') {
                        opciones.push({ label: 'Cancelar', icon: <XCircle size={15} />, danger: true, onClick: () => accion.mutate({ id: orden.id, op: 'cancelar' }) });
                      }
                    }
                    return <ActionMenu label={`Acciones de ${orden.numero}`} options={opciones} />;
                  },
                },
              ]}
            />
            <Pagination page={page} totalPages={ordenes.data?.totalPages ?? 1} onPageChange={setPage} />
          </CardBody>
        </Card>
      </FolderPanel>
      </div>

      <Modal
        open={Boolean(detalle)}
        onClose={() => setDetalle(null)}
        title={`Orden ${detalle?.numero ?? ''}`}
        size="lg"
        backdrop="none"
      >
        {detalle && (
          <div className="flex flex-col gap-5">
            <div className="flex flex-wrap items-center gap-3">
              <Pill tone={tono[detalle.estado] ?? 'neutral'}>{detalle.estado}</Pill>
              <span className="text-sm text-muted-foreground">{detalle.proveedorNombre}</span>
              <span className="text-sm text-muted-foreground">{formatDateTime(detalle.fecha)}</span>
            </div>
            <ModalSection title="Líneas">
              <div className="overflow-x-auto rounded-inner border border-border">
                <table className="w-full min-w-[520px] text-sm">
                  <thead>
                    <tr className="bg-muted/60 text-left text-xs uppercase tracking-tighter2 text-muted-foreground">
                      <th scope="col" className="px-4 py-2.5">SKU</th>
                      <th scope="col" className="px-4 py-2.5 text-right">Cantidad</th>
                      <th scope="col" className="px-4 py-2.5 text-right">Recibida</th>
                      <th scope="col" className="px-4 py-2.5 text-right">Costo</th>
                      <th scope="col" className="px-4 py-2.5 text-right">Subtotal</th>
                    </tr>
                  </thead>
                  <tbody>
                    {detalle.detalles.map((item) => (
                      <tr key={item.id} className="border-t border-border">
                        <td className="px-4 py-2.5 text-foreground">
                          {item.nombre}
                          <span className="ml-2 text-xs text-muted-foreground">{item.sku}</span>
                        </td>
                        <td className="num px-4 py-2.5 text-right text-muted-foreground">{formatNumber(item.cantidad)}</td>
                        <td className="num px-4 py-2.5 text-right text-muted-foreground">{formatNumber(item.cantidadRecibida)}</td>
                        <td className="num px-4 py-2.5 text-right text-muted-foreground">{formatUSD(item.costoUnitarioUSD)}</td>
                        <td className="num px-4 py-2.5 text-right text-foreground">{formatUSD(item.subtotalUSD)}</td>
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