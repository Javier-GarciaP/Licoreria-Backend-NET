import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { CheckCircle2, ClipboardList, HandCoins, Plus, XCircle } from 'lucide-react';
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
  Pagination,
  Pill,
  Select,
  StatusBadge,
} from '@licoreria/ui';
import type { Mesa, Reserva } from '@licoreria/types';
import { clubApi, inventarioApi, ventasApi } from '@licoreria/api-client';
import { mensajeDeError } from '../lib/api';
import { formatDateTime, formatUSD } from '../lib/format';

const ORIGENES = ['Web', 'Whatsapp', 'Presencial'] as const;

const esquema = z
  .object({
    fechaHora: z.string().min(1, 'Selecciona fecha y hora'),
    personas: z.coerce.number({ invalid_type_error: 'Ingresa el número de personas' }).int().min(1, 'Al menos 1 persona'),
    nombreContacto: z.string().min(2, 'Ingresa el nombre'),
    telefono: z.string().min(6, 'Teléfono inválido'),
    origen: z.enum(ORIGENES),
    notas: z.string().optional(),
    mesas: z.array(z.string()),
    metodoPagoId: z.string(),
    monto: z.coerce.number().min(0),
  })
  .refine((datos) => !datos.metodoPagoId || datos.monto > 0, {
    message: 'Ingresa el monto de la seña',
    path: ['monto'],
  });

type Formulario = z.infer<typeof esquema>;

const VALORES_INICIALES: Formulario = {
  fechaHora: '',
  personas: 2,
  nombreContacto: '',
  telefono: '',
  origen: 'Web',
  notas: '',
  mesas: [],
  metodoPagoId: '',
  monto: 0,
};

export function ReservasPage() {
  const [page, setPage] = useState(1);
  const [crearOpen, setCrearOpen] = useState(false);
  const [senasDe, setSenasDe] = useState<Reserva | null>(null);
  const [pedidosDe, setPedidosDe] = useState<Reserva | null>(null);
  const queryClient = useQueryClient();

  const [pedidoVarianteId, setPedidoVarianteId] = useState('');
  const [pedidoCantidad, setPedidoCantidad] = useState('1');

  const {
    register,
    handleSubmit,
    reset,
    setValue,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<Formulario>({ resolver: zodResolver(esquema), defaultValues: VALORES_INICIALES });

  const mesasSeleccionadas = watch('mesas');

  const reservas = useQuery({ queryKey: ['reservas', page], queryFn: () => clubApi.reservas({ page, pageSize: 15 }) });
  const mesas = useQuery({ queryKey: ['mesas'], queryFn: () => clubApi.mesas(), enabled: crearOpen });
  const metodos = useQuery({ queryKey: ['metodos-pago'], queryFn: ventasApi.metodosPago, enabled: crearOpen });
  const variantes = useQuery({ queryKey: ['stock'], queryFn: () => inventarioApi.stock(), enabled: Boolean(pedidosDe) });
  const pedidos = useQuery({
    queryKey: ['pedidos', pedidosDe?.id],
    queryFn: () => clubApi.pedidos(pedidosDe!.id),
    enabled: Boolean(pedidosDe),
  });

  const invalidar = () => queryClient.invalidateQueries({ queryKey: ['reservas'] });

  const crear = useMutation({
    mutationFn: async (datos: Formulario) => {
      const reserva = await clubApi.crearReserva({
        fechaHora: new Date(datos.fechaHora).toISOString(),
        personas: datos.personas,
        mesas: datos.mesas,
        nombreContacto: datos.nombreContacto,
        telefono: datos.telefono,
        origen: datos.origen,
        notas: datos.notas || null,
      });
      if (datos.metodoPagoId && datos.monto > 0) {
        await clubApi.registrarPagoReserva(reserva.id, {
          metodoPagoId: datos.metodoPagoId,
          monto: datos.monto,
          moneda: 'USD',
        });
      }
      return reserva;
    },
    onSuccess: () => {
      toast.success('Reserva creada');
      setCrearOpen(false);
      reset(VALORES_INICIALES);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo crear la reserva', { description: mensajeDeError(error) }),
  });

  const cambiar = useMutation({
    mutationFn: ({ id, estado }: { id: string; estado: string }) => clubApi.cambiarEstadoReserva(id, estado),
    onSuccess: () => {
      toast.success('Reserva actualizada');
      invalidar();
    },
    onError: (error) => toast.error('No se pudo actualizar', { description: mensajeDeError(error) }),
  });

  const validar = useMutation({
    mutationFn: ({ id, pagoId, aprobar }: { id: string; pagoId: string; aprobar: boolean }) =>
      clubApi.validarPagoReserva(id, pagoId, aprobar),
    onSuccess: (_data, variables) => {
      toast.success(variables.aprobar ? 'Seña validada' : 'Seña rechazada');
      queryClient.invalidateQueries({ queryKey: ['reservas'] });
      setSenasDe(null);
    },
    onError: (error) => toast.error('No se pudo validar la seña', { description: mensajeDeError(error) }),
  });

  const agregarPedido = useMutation({
    mutationFn: () =>
      clubApi.agregarPedido(pedidosDe!.id, { varianteId: pedidoVarianteId, cantidad: Number(pedidoCantidad) }),
    onSuccess: () => {
      toast.success('Pedido agregado');
      setPedidoVarianteId('');
      setPedidoCantidad('1');
      queryClient.invalidateQueries({ queryKey: ['pedidos', pedidosDe?.id] });
    },
    onError: (error) => toast.error('No se pudo agregar el pedido', { description: mensajeDeError(error) }),
  });

  const quitarPedido = useMutation({
    mutationFn: (pedidoId: string) => clubApi.eliminarPedido(pedidosDe!.id, pedidoId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['pedidos', pedidosDe?.id] }),
    onError: (error) => toast.error('No se pudo quitar', { description: mensajeDeError(error) }),
  });

  const toggleMesa = (mesaId: string) => {
    const siguiente = mesasSeleccionadas.includes(mesaId)
      ? mesasSeleccionadas.filter((id) => id !== mesaId)
      : [...mesasSeleccionadas, mesaId];
    setValue('mesas', siguiente, { shouldValidate: true });
  };

  return (
    <div className="mx-auto flex max-w-page flex-col gap-4">
      <Card>
        <CardHeader>
          <CardTitle>Agenda</CardTitle>
          <Button size="sm" leftIcon={<Plus size={15} />} onClick={() => setCrearOpen(true)}>
            Nueva reserva
          </Button>
        </CardHeader>
        <CardBody>
          <DataTable<Reserva>
            rows={reservas.data?.items ?? []}
            loading={reservas.isLoading}
            rowKey={(reserva) => reserva.id}
            empty="No hay reservas."
            columns={[
              { key: 'fecha', header: 'Fecha', render: (reserva) => formatDateTime(reserva.fechaHora) },
              {
                key: 'contacto',
                header: 'Contacto',
                render: (reserva) => (
                  <div>
                    <p className="text-foreground">{reserva.nombreContacto}</p>
                    <p className="text-xs text-muted-foreground">{reserva.telefono}</p>
                  </div>
                ),
              },
              { key: 'personas', header: 'Personas', align: 'center', render: (reserva) => reserva.personas },
              {
                key: 'mesas',
                header: 'Mesas',
                render: (reserva) => reserva.mesas.map((mesa) => mesa.numero).join(', ') || '—',
              },
              {
                key: 'senas',
                header: 'Señas',
                render: (reserva) =>
                  reserva.pagos.length === 0 ? (
                    <span className="text-xs text-muted-foreground">—</span>
                  ) : (
                    <Pill tone={reserva.pagos.some((pago) => pago.estado === 'Pendiente') ? 'warning' : 'success'}>
                      {reserva.pagos.length} pago(s)
                    </Pill>
                  ),
              },
              { key: 'estado', header: 'Estado', render: (reserva) => <StatusBadge status={reserva.estado} /> },
              {
                key: 'acciones',
                header: '',
                align: 'right',
                render: (reserva) => {
                  const opciones = [];
                  if (reserva.pagos.some((pago) => pago.estado === 'Pendiente')) {
                    opciones.push({ label: 'Validar señas', icon: <HandCoins size={15} />, onClick: () => setSenasDe(reserva) });
                  }
                  opciones.push({ label: 'Pedidos', icon: <ClipboardList size={15} />, onClick: () => setPedidosDe(reserva) });
                  if (reserva.estado === 'Pendiente') {
                    opciones.push({
                      label: 'Confirmar',
                      icon: <CheckCircle2 size={15} />,
                      onClick: () => cambiar.mutate({ id: reserva.id, estado: 'Confirmada' }),
                    });
                  }
                  if (reserva.estado === 'Pendiente' || reserva.estado === 'Confirmada') {
                    opciones.push({
                      label: 'Cancelar',
                      icon: <XCircle size={15} />,
                      danger: true,
                      onClick: () => cambiar.mutate({ id: reserva.id, estado: 'Cancelada' }),
                    });
                  }
                  return <ActionMenu label={`Acciones de reserva ${reserva.nombreContacto}`} options={opciones} />;
                },
              },
            ]}
          />
          <Pagination page={page} totalPages={reservas.data?.totalPages ?? 1} onPageChange={setPage} />
        </CardBody>
      </Card>

      <Modal
        open={crearOpen}
        onClose={() => setCrearOpen(false)}
        title="Nueva reserva"
        footer={
          <>
            <Button variant="ghost" onClick={() => setCrearOpen(false)}>
              Cancelar
            </Button>
            <Button type="submit" form="form-reserva" loading={isSubmitting || crear.isPending}>
              Crear reserva
            </Button>
          </>
        }
      >
        <form
          id="form-reserva"
          className="flex flex-col gap-3"
          onSubmit={handleSubmit((datos) => crear.mutate(datos))}
          noValidate
        >
          <Input
            label="Fecha y hora"
            type="datetime-local"
            error={errors.fechaHora?.message}
            {...register('fechaHora')}
          />
          <div className="grid grid-cols-2 gap-3">
            <Input
              label="Personas"
              type="number"
              error={errors.personas?.message}
              {...register('personas')}
            />
            <Select label="Origen" error={errors.origen?.message} {...register('origen')}>
              {ORIGENES.map((origen) => (
                <option key={origen} value={origen}>
                  {origen}
                </option>
              ))}
            </Select>
          </div>
          <Input label="Nombre" error={errors.nombreContacto?.message} {...register('nombreContacto')} />
          <Input label="Teléfono" error={errors.telefono?.message} {...register('telefono')} />
          <div>
            <p className="mb-1 text-xs font-medium text-muted-foreground">Mesas</p>
            <div className="flex flex-wrap gap-2">
              {mesas.data?.map((mesa: Mesa) => (
                <button
                  key={mesa.id}
                  type="button"
                  aria-pressed={mesasSeleccionadas.includes(mesa.id)}
                  onClick={() => toggleMesa(mesa.id)}
                  className={`rounded-full border px-3 py-1.5 text-xs transition ${
                    mesasSeleccionadas.includes(mesa.id)
                      ? 'border-primary text-foreground'
                      : 'border-border text-muted-foreground'
                  }`}
                >
                  {mesa.numero}
                </button>
              ))}
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <Select label="Seña (método, opcional)" error={errors.metodoPagoId?.message} {...register('metodoPagoId')}>
              <option value="">Sin seña</option>
              {metodos.data?.map((metodo) => (
                <option key={metodo.id} value={metodo.id}>
                  {metodo.nombre}
                </option>
              ))}
            </Select>
            <Input label="Monto seña (USD)" type="number" error={errors.monto?.message} {...register('monto')} />
          </div>
        </form>
      </Modal>

      <Modal open={Boolean(senasDe)} onClose={() => setSenasDe(null)} title="Validar señas">
        <div className="flex flex-col gap-3">
          {senasDe?.pagos.map((pago) => (
            <div key={pago.id} className="flex items-center justify-between gap-3 rounded-lg bg-muted/40 px-3 py-2">
              <div>
                <p className="text-sm text-foreground">{pago.metodoPago}</p>
                <p className="text-xs text-muted-foreground">{formatUSD(pago.monto)}</p>
              </div>
              <div className="flex items-center gap-2">
                <StatusBadge status={pago.estado} />
                {pago.estado === 'Pendiente' && (
                  <>
                    <Button
                      size="sm"
                      onClick={() => validar.mutate({ id: senasDe.id, pagoId: pago.id, aprobar: true })}
                    >
                      Validar
                    </Button>
                    <Button
                      size="sm"
                      variant="ghost"
                      onClick={() => validar.mutate({ id: senasDe.id, pagoId: pago.id, aprobar: false })}
                    >
                      Rechazar
                    </Button>
                  </>
                )}
              </div>
            </div>
          ))}
        </div>
      </Modal>

      <Modal open={Boolean(pedidosDe)} onClose={() => setPedidosDe(null)} title="Pedidos anticipados">
        <div className="flex flex-col gap-3">
          <div className="flex flex-col gap-1">
            {pedidos.data?.map((pedido) => (
              <div key={pedido.id} className="flex items-center justify-between text-sm">
                <span className="text-foreground">
                  {pedido.cantidad} × {pedido.nombre}
                </span>
                <div className="flex items-center gap-2">
                  <span className="text-muted-foreground">{formatUSD(pedido.subtotalUSD)}</span>
                  <button className="text-destructive-fg" onClick={() => quitarPedido.mutate(pedido.id)}>
                    Quitar
                  </button>
                </div>
              </div>
            ))}
            {pedidos.data?.length === 0 && <p className="text-sm text-muted-foreground">Sin pedidos.</p>}
          </div>
          <div className="flex items-end gap-2">
            <Select
              label="Producto"
              className="flex-1"
              value={pedidoVarianteId}
              onChange={(evento) => setPedidoVarianteId(evento.target.value)}
            >
              <option value="">Selecciona…</option>
              {variantes.data?.items.map((item) => (
                <option key={item.varianteId} value={item.varianteId}>
                  {item.productoNombre} · {item.sku}
                </option>
              ))}
            </Select>
            <Input
              label="Cant."
              type="number"
              value={pedidoCantidad}
              onChange={(evento) => setPedidoCantidad(evento.target.value)}
            />
            <Button
              disabled={!pedidoVarianteId || Number(pedidoCantidad) <= 0}
              loading={agregarPedido.isPending}
              onClick={() => agregarPedido.mutate()}
            >
              Añadir
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
