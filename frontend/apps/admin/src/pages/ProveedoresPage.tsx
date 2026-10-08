import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Pencil, Plus, Trash2 } from 'lucide-react';
import {
  ActionMenu,
  Button,
  Card,
  CardBody,
  CardHeader,
  CardTitle,
  DataTable,
  Input,
  Pill,
} from '@licoreria/ui';
import type { Proveedor } from '@licoreria/types';
import { proveedoresApi } from '@licoreria/api-client';
import { ComprasTabs } from '../components/ComprasTabs';
import { FolderPanel } from '../components/FolderTabs';
import { InlineForm } from '../components/InlineForm';
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

function FormularioProveedor({
  inicial,
  guardando,
  onGuardar,
}: {
  inicial: Formulario;
  guardando: boolean;
  onGuardar: (datos: Formulario) => void;
}) {
  const form = useForm<Formulario>({ resolver: zodResolver(esquema), defaultValues: inicial });
  return (
    <form className="flex flex-col gap-3" onSubmit={form.handleSubmit(onGuardar)} noValidate>
      <Input label="Nombre" error={form.formState.errors.nombre?.message} {...form.register('nombre')} />
      <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
        <Input label="RIF" {...form.register('rif')} />
        <Input label="Contacto" {...form.register('contacto')} />
      </div>
      <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
        <Input label="Teléfono" {...form.register('telefono')} />
        <Input label="Correo" type="email" error={form.formState.errors.email?.message} {...form.register('email')} />
      </div>
      <Input label="Dirección" {...form.register('direccion')} />
      <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
        <Input
          label="Días de crédito"
          type="number"
          error={form.formState.errors.diasCredito?.message}
          {...form.register('diasCredito')}
        />
        <label className="flex items-end gap-2 pb-2.5 text-sm text-muted-foreground">
          <input type="checkbox" {...form.register('activo')} />
          Activo
        </label>
      </div>
      <div className="flex justify-end">
        <Button type="submit" loading={guardando}>
          Guardar
        </Button>
      </div>
    </form>
  );
}

export function ProveedoresPage() {
  const [busqueda, setBusqueda] = useState('');
  const [creando, setCreando] = useState(false);
  const [editando, setEditando] = useState<Proveedor | null>(null);
  const [porEliminar, setPorEliminar] = useState<Proveedor | null>(null);
  const queryClient = useQueryClient();

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

  const editar = (proveedor: Proveedor) => {
    setEditando(proveedor);
    setCreando(false);
  };

  return (
    <div className="mx-auto flex max-w-page flex-col gap-4">
      <ComprasTabs />

      <div className="flex flex-col">
      <FolderPanel className="flex flex-col gap-4">
        {creando && (
          <InlineForm title="Nuevo proveedor" onCancel={() => setCreando(false)}>
            <FormularioProveedor inicial={VACIO} guardando={guardar.isPending} onGuardar={(datos) => guardar.mutate(datos)} />
          </InlineForm>
        )}

        <Card className="border-0 bg-transparent shadow-none">
          <CardHeader>
            <CardTitle>Proveedores</CardTitle>
            <div className="flex flex-wrap items-center gap-3">
              <div className="w-56">
                <Input placeholder="Buscar…" value={busqueda} onChange={(evento) => setBusqueda(evento.target.value)} />
              </div>
              <Button
                size="sm"
                leftIcon={<Plus size={15} />}
                onClick={() => {
                  setEditando(null);
                  setCreando(true);
                }}
              >
                Nuevo proveedor
              </Button>
            </div>
          </CardHeader>
          <CardBody>
            <DataTable<Proveedor>
              rows={proveedores.data ?? []}
              loading={proveedores.isLoading}
              rowKey={(proveedor) => proveedor.id}
              empty="No hay proveedores."
              expandirKey={editando?.id ?? null}
              expandedRow={
                editando
                  ? (proveedor) =>
                      proveedor.id === editando.id ? (
                        <InlineForm title={`Editar · ${proveedor.nombre}`} onCancel={() => setEditando(null)}>
                          <FormularioProveedor
                            inicial={{
                              nombre: proveedor.nombre,
                              rif: proveedor.rif ?? '',
                              contacto: proveedor.contacto ?? '',
                              telefono: proveedor.telefono ?? '',
                              email: proveedor.email ?? '',
                              direccion: proveedor.direccion ?? '',
                              diasCredito: proveedor.diasCredito,
                              activo: proveedor.activo,
                            }}
                            guardando={guardar.isPending}
                            onGuardar={(datos) => guardar.mutate(datos)}
                          />
                        </InlineForm>
                      ) : null
                  : undefined
              }
              columns={[
                {
                  key: 'nombre',
                  header: 'Proveedor',
                  render: (proveedor) => (
                    <div>
                      <p className="text-foreground">{proveedor.nombre}</p>
                      <p className="text-xs text-muted-foreground">{proveedor.contacto ?? proveedor.telefono ?? '—'}</p>
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
                    <ActionMenu
                      label={`Acciones de ${proveedor.nombre}`}
                      options={[
                        { label: 'Editar', icon: <Pencil size={15} />, onClick: () => editar(proveedor) },
                        { label: 'Eliminar', icon: <Trash2 size={15} />, danger: true, onClick: () => setPorEliminar(proveedor) },
                      ]}
                    />
                  ),
                },
              ]}
            />
          </CardBody>
        </Card>
      </FolderPanel>
      </div>

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