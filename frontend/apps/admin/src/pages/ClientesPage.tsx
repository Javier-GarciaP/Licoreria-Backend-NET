import { useState } from 'react';
import { useSearchParams } from 'react-router-dom';
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
  Pill,
} from '@licoreria/ui';
import type { Cliente } from '@licoreria/types';
import { clientesApi } from '@licoreria/api-client';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { mensajeDeError } from '../lib/api';

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
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader
        title="Clientes"
        subtitle="CRM y programa de fidelidad."
        actions={<Button onClick={abrirCrear}>Nuevo cliente</Button>}
      />
      <Card>
        <CardHeader>
          <CardTitle>Clientes</CardTitle>
          <div className="w-56">
            <Input
              placeholder="Buscar…"
              value={busqueda}
              onChange={(evento) => {
                setBusqueda(evento.target.value);
                setPage(1);
              }}
            />
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
                  <div className="flex justify-end gap-2">
                    <Button size="sm" variant="ghost" onClick={() => setPuntosDe(cliente)}>
                      Puntos
                    </Button>
                    <Button size="sm" variant="ghost" onClick={() => abrirEditar(cliente)}>
                      Editar
                    </Button>
                    <Button size="sm" variant="ghost" onClick={() => setPorEliminar(cliente)}>
                      Eliminar
                    </Button>
                  </div>
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
