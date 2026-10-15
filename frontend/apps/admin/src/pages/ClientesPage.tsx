import { useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Coins, Eye, Pencil, Plus, Trash2 } from 'lucide-react';
import {
  ActionMenu,
  Buscador,
  Button,
  Card,
  CardBody,
  CardHeader,
  DataTable,
  Input,
  Modal,
  Pagination,
  Pill,
} from '@licoreria/ui';
import type { Cliente } from '@licoreria/types';
import { clientesApi, cuentasPorCobrarApi } from '@licoreria/api-client';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { mensajeDeError } from '../lib/api';
import { formatDateTime, formatUSD } from '../lib/format';

const esquemaCliente = z.object({
  nombre: z.string().min(2, 'Ingresa el nombre'),
  rif: z.string().optional(),
  ci: z.string().optional(),
  email: z.string().email('Correo inválido').or(z.literal('')).optional(),
  telefono: z.string().optional(),
  direccion: z.string().optional(),
  activo: z.boolean(),
});

const esquemaPuntos = z.object({
  puntos: z.coerce.number({ invalid_type_error: 'Ingresa los puntos' }).int().positive('Debe ser mayor que 0'),
  motivo: z.string().min(3, 'Describe el motivo'),
});

type FormularioCliente = z.infer<typeof esquemaCliente>;
type FormularioPuntos = z.infer<typeof esquemaPuntos>;

const VACIO: FormularioCliente = {
  nombre: '',
  rif: '',
  ci: '',
  email: '',
  telefono: '',
  direccion: '',
  activo: true,
};

export function ClientesPage() {
  const [page, setPage] = useState(1);
  const [params] = useSearchParams();
  const [busqueda, setBusqueda] = useState(params.get('busqueda') ?? '');
  const [creando, setCreando] = useState(false);
  const [editando, setEditando] = useState<Cliente | null>(null);
  const [puntosDe, setPuntosDe] = useState<Cliente | null>(null);
  const [porEliminar, setPorEliminar] = useState<Cliente | null>(null);
  const [detalleDe, setDetalleDe] = useState<Cliente | null>(null);
  const queryClient = useQueryClient();

  const clienteForm = useForm<FormularioCliente>({ resolver: zodResolver(esquemaCliente), defaultValues: VACIO });
  const puntosForm = useForm<FormularioPuntos>({
    resolver: zodResolver(esquemaPuntos),
    defaultValues: { puntos: 0, motivo: '' },
  });

  const clientes = useQuery({
    queryKey: ['clientes', page, busqueda],
    queryFn: () => clientesApi.listar({ page, pageSize: 15, busqueda }),
  });

  const detalleId = detalleDe?.id ?? '';
  const ventasCliente = useQuery({
    queryKey: ['clientes', 'detalle', detalleId, 'ventas'],
    queryFn: () => clientesApi.ventas(detalleId, { pageSize: 10 }),
    enabled: Boolean(detalleId),
  });
  const puntosCliente = useQuery({
    queryKey: ['clientes', 'detalle', detalleId, 'puntos'],
    queryFn: () => clientesApi.puntos(detalleId),
    enabled: Boolean(detalleId),
  });
  const cuentasCliente = useQuery({
    queryKey: ['clientes', 'detalle', detalleId, 'cuentas'],
    queryFn: () => cuentasPorCobrarApi.listar({ clienteId: detalleId, pageSize: 20 }),
    enabled: Boolean(detalleId),
  });

  const invalidar = () => queryClient.invalidateQueries({ queryKey: ['clientes'] });

  const guardar = useMutation({
    mutationFn: (datos: FormularioCliente) =>
      editando
        ? clientesApi.actualizar(editando.id, { id: editando.id, ...datos })
        : clientesApi.crear(datos),
    onSuccess: () => {
      toast.success(editando ? 'Cliente actualizado' : 'Cliente creado');
      setCreando(false);
      setEditando(null);
      clienteForm.reset(VACIO);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo guardar', { description: mensajeDeError(error) }),
  });

  const acumular = useMutation({
    mutationFn: ({ id, datos }: { id: string; datos: FormularioPuntos }) =>
      clientesApi.acumularPuntos(id, datos),
    onSuccess: () => {
      toast.success('Puntos acumulados');
      setPuntosDe(null);
      puntosForm.reset({ puntos: 0, motivo: '' });
      invalidar();
    },
    onError: (error) => toast.error('No se pudo acumular', { description: mensajeDeError(error) }),
  });

  const canjear = useMutation({
    mutationFn: ({ id, datos }: { id: string; datos: FormularioPuntos }) => clientesApi.canjearPuntos(id, datos),
    onSuccess: () => {
      toast.success('Puntos canjeados');
      setPuntosDe(null);
      puntosForm.reset({ puntos: 0, motivo: '' });
      invalidar();
    },
    onError: (error) => toast.error('No se pudo canjear', { description: mensajeDeError(error) }),
  });

  const eliminar = useMutation({
    mutationFn: (id: string) => clientesApi.eliminar(id),
    onSuccess: () => {
      toast.success('Cliente eliminado');
      setPorEliminar(null);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo eliminar', { description: mensajeDeError(error) }),
  });

  const abrirCrear = () => {
    clienteForm.reset(VACIO);
    setCreando(true);
  };

  const abrirEditar = (cliente: Cliente) => {
    clienteForm.reset({
      nombre: cliente.nombre,
      rif: cliente.rif ?? '',
      ci: cliente.ci ?? '',
      email: cliente.email ?? '',
      telefono: cliente.telefono ?? '',
      direccion: cliente.direccion ?? '',
      activo: cliente.activo,
    });
    setEditando(cliente);
  };

  const formularioAbierto = creando || Boolean(editando);

  return (
    <div className="mx-auto flex max-w-page flex-col gap-4">
      <Card>
        <CardHeader className="flex flex-col items-stretch gap-3">
          <div className="flex flex-wrap items-center gap-2">
            <Buscador
              placeholder="Buscar nombre, RIF, CI o teléfono…"
              value={busqueda}
              onCambio={(valor) => {
                setBusqueda(valor);
                setPage(1);
              }}
            />
            <div className="ml-auto">
              <Button leftIcon={<Plus size={15} />} onClick={abrirCrear}>
                Nuevo cliente
              </Button>
            </div>
          </div>
        </CardHeader>
        <CardBody>
          <DataTable<Cliente>
            rows={clientes.data?.items ?? []}
            loading={clientes.isLoading}
            rowKey={(cliente) => cliente.id}
            empty="No hay clientes."
            columns={[
              {
                key: 'nombre',
                header: 'Cliente',
                render: (cliente) => (
                  <div>
                    <p className="text-foreground">{cliente.nombre}</p>
                    <p className="text-xs text-muted-foreground">{cliente.telefono ?? cliente.email ?? '—'}</p>
                  </div>
                ),
              },
              { key: 'rif', header: 'RIF/CI', render: (cliente) => cliente.rif ?? cliente.ci ?? '—' },
              { key: 'puntos', header: 'Puntos', align: 'center', render: (cliente) => <Pill tone="accent">{cliente.puntos}</Pill> },
              {
                key: 'activo',
                header: 'Estado',
                render: (cliente) => (
                  <Pill tone={cliente.activo ? 'success' : 'danger'}>{cliente.activo ? 'Activo' : 'Inactivo'}</Pill>
                ),
              },
              {
                key: 'acciones',
                header: '',
                align: 'right',
                render: (cliente) => (
                  <ActionMenu
                    label={`Acciones de ${cliente.nombre}`}
                    options={[
                      { label: 'Ver detalle', icon: <Eye size={15} />, onClick: () => setDetalleDe(cliente) },
                      { label: 'Puntos', icon: <Coins size={15} />, onClick: () => setPuntosDe(cliente) },
                      { label: 'Editar', icon: <Pencil size={15} />, onClick: () => abrirEditar(cliente) },
                      {
                        label: 'Eliminar',
                        icon: <Trash2 size={15} />,
                        danger: true,
                        onClick: () => setPorEliminar(cliente),
                      },
                    ]}
                  />
                ),
              },
            ]}
          />
          <Pagination page={page} totalPages={clientes.data?.totalPages ?? 1} onPageChange={setPage} />
        </CardBody>
      </Card>

      <Modal
        open={formularioAbierto}
        onClose={() => {
          setCreando(false);
          setEditando(null);
        }}
        title={editando ? 'Editar cliente' : 'Nuevo cliente'}
        footer={
          <>
            <Button
              variant="ghost"
              onClick={() => {
                setCreando(false);
                setEditando(null);
              }}
            >
              Cancelar
            </Button>
            <Button type="submit" form="form-cliente" loading={guardar.isPending}>
              Guardar
            </Button>
          </>
        }
      >
        <form
          id="form-cliente"
          className="flex flex-col gap-3"
          onSubmit={clienteForm.handleSubmit((datos) => guardar.mutate(datos))}
          noValidate
        >
          <Input label="Nombre" error={clienteForm.formState.errors.nombre?.message} {...clienteForm.register('nombre')} />
          <div className="grid grid-cols-2 gap-3">
            <Input label="RIF" {...clienteForm.register('rif')} />
            <Input label="Cédula" {...clienteForm.register('ci')} />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <Input label="Correo" type="email" error={clienteForm.formState.errors.email?.message} {...clienteForm.register('email')} />
            <Input label="Teléfono" {...clienteForm.register('telefono')} />
          </div>
          <Input label="Dirección" {...clienteForm.register('direccion')} />
          <label className="flex items-center gap-2 text-sm text-muted-foreground">
            <input type="checkbox" {...clienteForm.register('activo')} />
            Activo
          </label>
        </form>
      </Modal>

      <Modal
        open={Boolean(detalleDe)}
        onClose={() => setDetalleDe(null)}
        title={`Cliente · ${detalleDe?.nombre ?? ''}`}
        size="lg"
        footer={
          <Button variant="ghost" onClick={() => setDetalleDe(null)}>
            Cerrar
          </Button>
        }
      >
        <div className="flex flex-col gap-5">
          <div className="flex items-center justify-between rounded-xl border border-border bg-muted/40 px-4 py-3">
            <span className="text-sm text-muted-foreground">Puntos</span>
            <span className="num text-xl font-medium text-foreground">{detalleDe?.puntos ?? 0}</span>
          </div>

          <section aria-label="Ventas del cliente" className="flex flex-col gap-2">
            <h3 className="text-sm font-medium text-foreground">Ventas recientes</h3>
            {ventasCliente.isError && <p className="text-sm text-destructive-fg">No se pudieron cargar las ventas.</p>}
            {!ventasCliente.isError && (ventasCliente.data?.items.length ?? 0) === 0 && !ventasCliente.isLoading && (
              <p className="text-sm text-muted-foreground">Sin ventas asociadas.</p>
            )}
            {(ventasCliente.data?.items ?? []).map((venta) => (
              <div key={venta.id} className="flex items-center justify-between gap-3 text-sm">
                <span className="text-muted-foreground">
                  {formatDateTime(venta.fecha)} · {venta.numeroComprobante ?? venta.id.slice(0, 8)}
                </span>
                <span className="num text-foreground">{formatUSD(venta.totalUSD)}</span>
              </div>
            ))}
          </section>

          <section aria-label="Cuentas por cobrar del cliente" className="flex flex-col gap-2">
            <h3 className="text-sm font-medium text-foreground">Cuentas por cobrar</h3>
            {cuentasCliente.isError && (
              <p className="text-sm text-destructive-fg">No se pudieron cargar las cuentas por cobrar.</p>
            )}
            {!cuentasCliente.isError && (cuentasCliente.data?.items.length ?? 0) === 0 && !cuentasCliente.isLoading && (
              <p className="text-sm text-muted-foreground">Sin cuentas por cobrar.</p>
            )}
            {(cuentasCliente.data?.items ?? []).map((cuenta) => (
              <div key={cuenta.id} className="flex items-center justify-between gap-3 text-sm">
                <span className="text-muted-foreground">Vence {cuenta.vencimiento.slice(0, 10)}</span>
                <span className="flex items-center gap-2">
                  <span className="num text-foreground">{formatUSD(cuenta.saldoUSD)}</span>
                  <Pill tone={cuenta.estado === 'Vencida' ? 'danger' : cuenta.estado === 'Pagada' ? 'success' : 'warning'}>
                    {cuenta.estado}
                  </Pill>
                </span>
              </div>
            ))}
          </section>

          <section aria-label="Movimientos de puntos" className="flex flex-col gap-2">
            <h3 className="text-sm font-medium text-foreground">Movimientos de puntos</h3>
            {puntosCliente.isError && <p className="text-sm text-destructive-fg">No se pudieron cargar los puntos.</p>}
            {!puntosCliente.isError && (puntosCliente.data?.length ?? 0) === 0 && !puntosCliente.isLoading && (
              <p className="text-sm text-muted-foreground">Sin movimientos de puntos.</p>
            )}
            {(puntosCliente.data ?? []).slice(0, 10).map((movimiento) => (
              <div key={movimiento.id} className="flex items-center justify-between gap-3 text-sm">
                <span className="text-muted-foreground">
                  {movimiento.motivo} · {formatDateTime(movimiento.fecha)}
                </span>
                <span className="num text-foreground">
                  {movimiento.tipo === 'Acumulacion' ? '+' : '−'}
                  {movimiento.puntos}
                </span>
              </div>
            ))}
          </section>
        </div>
      </Modal>

      <Modal
        open={Boolean(puntosDe)}
        onClose={() => setPuntosDe(null)}
        title={`Puntos · ${puntosDe?.nombre ?? ''}`}
        footer={
          <>
            <Button
              variant="ghost"
              loading={canjear.isPending}
              onClick={() => puntosDe && puntosForm.handleSubmit((datos) => canjear.mutate({ id: puntosDe.id, datos }))()}
            >
              Canjear
            </Button>
            <Button
              type="submit"
              form="form-puntos"
              loading={acumular.isPending}
            >
              Acumular
            </Button>
          </>
        }
      >
        <form
          id="form-puntos"
          className="flex flex-col gap-3"
          onSubmit={puntosForm.handleSubmit((datos) => puntosDe && acumular.mutate({ id: puntosDe.id, datos }))}
          noValidate
        >
          <Input
            label="Puntos"
            type="number"
            error={puntosForm.formState.errors.puntos?.message}
            {...puntosForm.register('puntos')}
          />
          <Input label="Motivo" error={puntosForm.formState.errors.motivo?.message} {...puntosForm.register('motivo')} />
        </form>
      </Modal>

      <ConfirmDialog
        open={Boolean(porEliminar)}
        title="Eliminar cliente"
        description={`¿Seguro que deseas eliminar a ${porEliminar?.nombre ?? ''}?`}
        confirmLabel="Eliminar"
        loading={eliminar.isPending}
        onClose={() => setPorEliminar(null)}
        onConfirm={() => porEliminar && eliminar.mutate(porEliminar.id)}
      />
    </div>
  );
}
