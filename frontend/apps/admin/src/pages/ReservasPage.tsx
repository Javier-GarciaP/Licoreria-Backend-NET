import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { CheckCircle2, ClipboardList, Plus, XCircle } from 'lucide-react';
import {
  ActionMenu,
  Buscador,
  Button,
  Card,
  CardBody,
  CardHeader,
  DataTable,
  FiltroDropdown,
  FiltroRango,
  Input,
  LimpiarFiltros,
  Modal,
  Pagination,
  Pill,
  Select,
  StatusBadge,
} from '@licoreria/ui';
import type { Mesa, Reserva } from '@licoreria/types';
import { clubApi, ventasApi } from '@licoreria/api-client';
import { mensajeDeError } from '../lib/api';
import { formatDateTime } from '../lib/format';
import { contiene, paginarEnMemoria, PAGE_SIZE_FILTRO_LOCAL } from '../lib/filtros';

const ORIGENES = ['Web', 'Whatsapp', 'Presencial'] as const;
const ESTADOS_RESERVA = ['Pendiente', 'Confirmada', 'Cancelada'] as const;

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
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [crearOpen, setCrearOpen] = useState(false);
  const [estado, setEstado] = useState('');
  const [origen, setOrigen] = useState('');
  const [busqueda, setBusqueda] = useState('');
  const [personasMin, setPersonasMin] = useState('');
  const queryClient = useQueryClient();

  const {
    register,
    handleSubmit,
    reset,
    setValue,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<Formulario>({ resolver: zodResolver(esquema), defaultValues: VALORES_INICIALES });

  const mesasSeleccionadas = watch('mesas');

  const hayFiltroLocal = Boolean(busqueda || origen || personasMin);
  const hayFiltros = hayFiltroLocal || Boolean(estado);

  const limpiarFiltros = () => {
    setEstado('');
    setOrigen('');
    setBusqueda('');
    setPersonasMin('');
    setPage(1);
  };

  const reservas = useQuery({
    queryKey: ['reservas', page, estado, hayFiltroLocal ? PAGE_SIZE_FILTRO_LOCAL : 15],
    queryFn: () =>
      clubApi.reservas({
        estado: estado || undefined,
        page: hayFiltroLocal ? 1 : page,
        pageSize: hayFiltroLocal ? PAGE_SIZE_FILTRO_LOCAL : 15,
      }),
  });
  const mesas = useQuery({ queryKey: ['mesas'], queryFn: () => clubApi.mesas(), enabled: crearOpen });
  const metodos = useQuery({ queryKey: ['metodos-pago'], queryFn: ventasApi.metodosPago, enabled: crearOpen });

  const { items: filas, totalPages } = useMemo(() => {
    const personasMinN = Number(personasMin);
    const filtradas = (reservas.data?.items ?? []).filter((reserva) => {
      if (!contiene(`${reserva.nombreContacto} ${reserva.telefono}`, busqueda)) return false;
      if (origen && reserva.origen !== origen) return false;
      if (personasMinN && reserva.personas < personasMinN) return false;
      return true;
    });
    return hayFiltroLocal
      ? paginarEnMemoria(filtradas, page, 15)
      : { items: filtradas, totalPages: reservas.data?.totalPages ?? 1 };
  }, [reservas.data, busqueda, origen, personasMin, hayFiltroLocal, page]);

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

  const toggleMesa = (mesaId: string) => {
    const siguiente = mesasSeleccionadas.includes(mesaId)
      ? mesasSeleccionadas.filter((id) => id !== mesaId)
      : [...mesasSeleccionadas, mesaId];
    setValue('mesas', siguiente, { shouldValidate: true });
  };

  return (
    <div className="mx-auto flex max-w-page flex-col gap-4">
      <Card>
        <CardHeader className="flex flex-col items-stretch gap-3">
          <div className="flex flex-wrap items-center gap-2">
            <Buscador
              placeholder="Buscar nombre o teléfono…"
              value={busqueda}
              onCambio={(valor) => {
                setBusqueda(valor);
                setPage(1);
              }}
            />
            <FiltroDropdown
              label="Estado"
              opciones={ESTADOS_RESERVA.map((valor) => ({ valor, etiqueta: valor }))}
              valor={estado}
              onChange={(valor) => {
                setEstado(valor);
                setPage(1);
              }}
            />
            <div className="ml-auto">
              <Button size="sm" leftIcon={<Plus size={15} />} onClick={() => setCrearOpen(true)}>
                Nueva reserva
              </Button>
            </div>
          </div>
          <div className="border-b border-border" />
          <div className="flex flex-wrap items-center gap-2">
            <FiltroDropdown
              label="Origen"
              opciones={ORIGENES.map((valor) => ({ valor, etiqueta: valor }))}
              valor={origen}
              onChange={(valor) => {
                setOrigen(valor);
                setPage(1);
              }}
            />
            <FiltroRango
              label="Personas"
              minimo={personasMin}
              onMinimo={(valor) => {
                setPersonasMin(valor);
                setPage(1);
              }}
              step="1"
            />
            <div className="ml-auto">
              <LimpiarFiltros activo={hayFiltros} onClick={limpiarFiltros} />
            </div>
          </div>
        </CardHeader>
        <CardBody>
          <DataTable<Reserva>
            rows={filas}
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
                  opciones.push({ label: 'Ver detalle y pedidos', icon: <ClipboardList size={15} />, onClick: () => navigate(`/reservas/${reserva.id}`) });
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
          <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
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
    </div>
  );
}