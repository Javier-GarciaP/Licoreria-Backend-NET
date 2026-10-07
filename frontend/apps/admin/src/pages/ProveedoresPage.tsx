import { useState } from 'react';
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
  Pill,
} from '@licoreria/ui';
import type { Proveedor } from '@licoreria/types';
import { proveedoresApi } from '@licoreria/api-client';
import { ComprasTabs } from '../components/ComprasTabs';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { mensajeDeError } from '../lib/api';

const esquema = z.object({
  nombre: z.string().min(2, 'Ingresa el nombre'),
  rif: z.string().optional(),
  contacto: z.string().optional(),
  telefono: z.string().optional(),
  email: z.string().email('Correo inválido').or(z.literal('')).optional(),
  direccion: z.string().optional(),
  diasCredito: z.coerce.number().int().min(0, 'No puede ser negativo'),
  activo: z.boolean(),
});

type Formulario = z.infer<typeof esquema>;

const VACIO: Formulario = {
  nombre: '',
  rif: '',
  contacto: '',
  telefono: '',
  email: '',
  direccion: '',
  diasCredito: 0,
  activo: true,
};

export function ProveedoresPage() {
  const [busqueda, setBusqueda] = useState('');
  const [creando, setCreando] = useState(false);
  const [editando, setEditando] = useState<Proveedor | null>(null);
  const [porEliminar, setPorEliminar] = useState<Proveedor | null>(null);
  const queryClient = useQueryClient();

  const form = useForm<Formulario>({ resolver: zodResolver(esquema), defaultValues: VACIO });
  const proveedores = useQuery({ queryKey: ['proveedores', busqueda], queryFn: () => proveedoresApi.listar(busqueda) });

  const invalidar = () => queryClient.invalidateQueries({ queryKey: ['proveedores'] });

  const guardar = useMutation({
    mutationFn: (datos: Formulario) =>
      editando
        ? proveedoresApi.actualizar(editando.id, { id: editando.id, ...datos })
        : proveedoresApi.crear(datos),
    onSuccess: () => {
      toast.success(editando ? 'Proveedor actualizado' : 'Proveedor creado');
      setCreando(false);
      setEditando(null);
      form.reset(VACIO);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo guardar', { description: mensajeDeError(error) }),
  });

  const eliminar = useMutation({
    mutationFn: (id: string) => proveedoresApi.eliminar(id),
    onSuccess: () => {
      toast.success('Proveedor eliminado');
      setPorEliminar(null);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo eliminar', { description: mensajeDeError(error) }),
  });

  const abrirEditar = (proveedor: Proveedor) => {
    form.reset({
      nombre: proveedor.nombre,
      rif: proveedor.rif ?? '',
      contacto: proveedor.contacto ?? '',
      telefono: proveedor.telefono ?? '',
      email: proveedor.email ?? '',
      direccion: proveedor.direccion ?? '',
      diasCredito: proveedor.diasCredito,
      activo: proveedor.activo,
    });
    setEditando(proveedor);
  };

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader
        title="Compras"
        subtitle="Proveedores del local."
        actions={
          <Button
            onClick={() => {
              form.reset(VACIO);
              setCreando(true);
            }}
          >
            Nuevo proveedor
          </Button>
        }
      />
      <ComprasTabs />

      <Card>
        <CardHeader>
          <CardTitle>Proveedores</CardTitle>
          <div className="w-56">
            <Input placeholder="Buscar…" value={busqueda} onChange={(evento) => setBusqueda(evento.target.value)} />
          </div>
        </CardHeader>
        <CardBody>
          <DataTable<Proveedor>
            rows={proveedores.data ?? []}
            loading={proveedores.isLoading}
            rowKey={(proveedor) => proveedor.id}
            empty="No hay proveedores."
            columns={[
              {
                key: 'nombre',
                header: 'Proveedor',
                render: (proveedor) => (
                  <div>
                    <p className="text-ink">{proveedor.nombre}</p>
                    <p className="text-xs text-muted">{proveedor.contacto ?? proveedor.telefono ?? '—'}</p>
                  </div>
                ),
              },
              { key: 'rif', header: 'RIF', render: (proveedor) => proveedor.rif ?? '—' },
              { key: 'credito', header: 'Crédito', align: 'center', render: (proveedor) => `${proveedor.diasCredito} d` },
              {
                key: 'activo',
                header: 'Estado',
                render: (proveedor) => (
                  <Pill tone={proveedor.activo ? 'success' : 'danger'}>{proveedor.activo ? 'Activo' : 'Inactivo'}</Pill>
                ),
              },
              {
                key: 'acciones',
                header: '',
                align: 'right',
                render: (proveedor) => (
                  <div className="flex justify-end gap-2">
                    <Button size="sm" variant="ghost" onClick={() => abrirEditar(proveedor)}>
                      Editar
                    </Button>
                    <Button size="sm" variant="ghost" onClick={() => setPorEliminar(proveedor)}>
                      Eliminar
                    </Button>
                  </div>
                ),
              },
            ]}
          />
        </CardBody>
      </Card>

      <Modal
        open={creando || Boolean(editando)}
        onClose={() => {
          setCreando(false);
          setEditando(null);
        }}
        title={editando ? 'Editar proveedor' : 'Nuevo proveedor'}
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
            <Button type="submit" form="form-proveedor" loading={guardar.isPending}>
              Guardar
            </Button>
          </>
        }
      >
        <form id="form-proveedor" className="flex flex-col gap-3" onSubmit={form.handleSubmit((d) => guardar.mutate(d))} noValidate>
          <Input label="Nombre" error={form.formState.errors.nombre?.message} {...form.register('nombre')} />
          <div className="grid grid-cols-2 gap-3">
            <Input label="RIF" {...form.register('rif')} />
            <Input label="Contacto" {...form.register('contacto')} />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <Input label="Teléfono" {...form.register('telefono')} />
            <Input label="Correo" type="email" error={form.formState.errors.email?.message} {...form.register('email')} />
          </div>
          <Input label="Dirección" {...form.register('direccion')} />
          <Input
            label="Días de crédito"
            type="number"
            error={form.formState.errors.diasCredito?.message}
            {...form.register('diasCredito')}
          />
          <label className="flex items-center gap-2 text-sm text-muted">
            <input type="checkbox" {...form.register('activo')} />
            Activo
          </label>
        </form>
      </Modal>

      <ConfirmDialog
        open={Boolean(porEliminar)}
        title="Eliminar proveedor"
        description={`¿Seguro que deseas eliminar a ${porEliminar?.nombre ?? ''}?`}
        confirmLabel="Eliminar"
        loading={eliminar.isPending}
        onClose={() => setPorEliminar(null)}
        onConfirm={() => porEliminar && eliminar.mutate(porEliminar.id)}
      />
    </div>
  );
}
