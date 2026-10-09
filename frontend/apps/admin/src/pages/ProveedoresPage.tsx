import { useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Pencil, Plus, Trash2 } from 'lucide-react';
import {
  ActionMenu,
  Buscador,
  Button,
  Card,
  CardBody,
  CardHeader,
  DataTable,
  FiltroDropdown,
  Input,
  LimpiarFiltros,
  Modal,
  Pill,
} from '@licoreria/ui';
import type { Proveedor } from '@licoreria/types';
import { proveedoresApi } from '@licoreria/api-client';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { mensajeDeError } from '../lib/api';
import { contiene } from '../lib/filtros';

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
  const [estado, setEstado] = useState('');
  const [credito, setCredito] = useState('');
  const [creando, setCreando] = useState(false);
  const [editando, setEditando] = useState<Proveedor | null>(null);
  const [porEliminar, setPorEliminar] = useState<Proveedor | null>(null);
  const queryClient = useQueryClient();

  const proveedores = useQuery({ queryKey: ['proveedores'], queryFn: () => proveedoresApi.listar() });

  const filas = useMemo(
    () =>
      (proveedores.data ?? []).filter((proveedor) => {
        if (!contiene(`${proveedor.nombre} ${proveedor.rif ?? ''} ${proveedor.contacto ?? ''}`, busqueda)) return false;
        if (estado === 'activos' && !proveedor.activo) return false;
        if (estado === 'inactivos' && proveedor.activo) return false;
        if (credito === 'sin' && proveedor.diasCredito !== 0) return false;
        if (credito === 'con' && proveedor.diasCredito === 0) return false;
        return true;
      }),
    [proveedores.data, busqueda, estado, credito],
  );

  const hayFiltros = Boolean(busqueda || estado || credito);

  const limpiarFiltros = () => {
    setBusqueda('');
    setEstado('');
    setCredito('');
  };

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

  const abrirCrear = () => {
    setEditando(null);
    setCreando(true);
  };

  const abrirEditar = (proveedor: Proveedor) => {
    setEditando(proveedor);
    setCreando(false);
  };

  const modalAbierto = creando || Boolean(editando);
  const formInicial = editando
    ? {
        nombre: editando.nombre,
        rif: editando.rif ?? '',
        contacto: editando.contacto ?? '',
        telefono: editando.telefono ?? '',
        email: editando.email ?? '',
        direccion: editando.direccion ?? '',
        diasCredito: editando.diasCredito,
        activo: editando.activo,
      }
    : VACIO;

  return (
    <div className="mx-auto flex max-w-page flex-col gap-4">
      <Card>
        <CardHeader className="flex flex-col items-stretch gap-3">
          <div className="flex flex-wrap items-center gap-2">
            <Buscador
              placeholder="Buscar nombre, RIF o contacto…"
              value={busqueda}
              onCambio={setBusqueda}
            />
            <div className="ml-auto">
              <Button leftIcon={<Plus size={15} />} onClick={abrirCrear}>
                Nuevo proveedor
              </Button>
            </div>
          </div>
          <div className="border-b border-border" />
          <div className="flex flex-wrap items-center gap-2">
            <FiltroDropdown
              label="Estado"
              opciones={[
                { valor: 'activos', etiqueta: 'Activos' },
                { valor: 'inactivos', etiqueta: 'Inactivos' },
              ]}
              valor={estado}
              onChange={setEstado}
            />
            <FiltroDropdown
              label="Crédito"
              opciones={[
                { valor: 'sin', etiqueta: 'Sin crédito' },
                { valor: 'con', etiqueta: 'Con crédito' },
              ]}
              valor={credito}
              onChange={setCredito}
            />
            <div className="ml-auto">
              <LimpiarFiltros activo={hayFiltros} onClick={limpiarFiltros} />
            </div>
          </div>
        </CardHeader>
        <CardBody>
          <DataTable<Proveedor>
            rows={filas}
            loading={proveedores.isLoading}
            rowKey={(proveedor) => proveedor.id}
            empty="No hay proveedores."
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
                      { label: 'Editar', icon: <Pencil size={15} />, onClick: () => abrirEditar(proveedor) },
                      { label: 'Eliminar', icon: <Trash2 size={15} />, danger: true, onClick: () => setPorEliminar(proveedor) },
                    ]}
                  />
                ),
              },
            ]}
          />
        </CardBody>
      </Card>

      <Modal
        open={modalAbierto}
        onClose={() => {
          setCreando(false);
          setEditando(null);
        }}
        title={editando ? `Editar · ${editando.nombre}` : 'Nuevo proveedor'}
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
          </>
        }
      >
        <FormularioProveedor
          inicial={formInicial}
          guardando={guardar.isPending}
          onGuardar={(datos) => guardar.mutate(datos)}
        />
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