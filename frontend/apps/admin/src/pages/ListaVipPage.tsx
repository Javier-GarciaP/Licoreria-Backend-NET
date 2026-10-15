import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Pencil, Plus, Trash2 } from 'lucide-react';
import {
  ActionMenu,
  Buscador,
  FiltroDropdown,
  LimpiarFiltros,
  Button,
  Card,
  CardBody,
  CardHeader,
  DataTable,
  Input,
  Modal,
  Pill,
} from '@licoreria/ui';
import type { ListaVip } from '@licoreria/types';
import { listaVipApi } from '@licoreria/api-client';
import { mensajeDeError } from '../lib/api';
import { contiene } from '../lib/filtros';

const esquema = z.object({
  nombre: z.string().min(1, 'Ingresa el nombre'),
  documento: z.string().optional(),
  telefono: z.string().optional(),
  notas: z.string().optional(),
  activo: z.boolean().default(true),
});

type Formulario = z.infer<typeof esquema>;

export function ListaVipPage() {
  const queryClient = useQueryClient();
  const [editando, setEditando] = useState<ListaVip | null>(null);
  const [abierto, setAbierto] = useState(false);
  const [busqueda, setBusqueda] = useState('');
  const [estado, setEstado] = useState('');
  const [eliminar, setEliminar] = useState<ListaVip | null>(null);

  const invitados = useQuery({ queryKey: ['lista-vip'], queryFn: listaVipApi.listar });

  const filas = (invitados.data ?? []).filter((fila) => {
    if (!contiene(`${fila.nombre} ${fila.documento ?? ''} ${fila.telefono ?? ''}`, busqueda)) return false;
    if (estado === 'activos' && !fila.activo) return false;
    if (estado === 'inactivos' && fila.activo) return false;
    return true;
  });
  const hayFiltros = Boolean(busqueda || estado);
  const limpiarFiltros = () => {
    setBusqueda('');
    setEstado('');
  };
  const form = useForm<Formulario>({
    resolver: zodResolver(esquema),
    defaultValues: { nombre: '', documento: '', telefono: '', notas: '', activo: true },
  });

  const invalidar = () => queryClient.invalidateQueries({ queryKey: ['lista-vip'] });

  const guardar = useMutation({
    mutationFn: (datos: Formulario) =>
      editando
        ? listaVipApi.actualizar(editando.id, { ...datos, id: editando.id })
        : listaVipApi.crear(datos),
    onSuccess: () => {
      toast.success(editando ? 'Invitado actualizado' : 'Invitado agregado');
      cerrar();
      invalidar();
    },
    onError: (error) => toast.error('No se pudo guardar', { description: mensajeDeError(error) }),
  });

  const borrar = useMutation({
    mutationFn: (id: string) => listaVipApi.eliminar(id),
    onSuccess: () => {
      toast.success('Invitado eliminado');
      setEliminar(null);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo eliminar', { description: mensajeDeError(error) }),
  });

  const cerrar = () => {
    setAbierto(false);
    setEditando(null);
    form.reset({ nombre: '', documento: '', telefono: '', notas: '', activo: true });
  };

  const abrirCrear = () => {
    form.reset({ nombre: '', documento: '', telefono: '', notas: '', activo: true });
    setEditando(null);
    setAbierto(true);
  };

  const abrirEditar = (invitado: ListaVip) => {
    form.reset({
      nombre: invitado.nombre,
      documento: invitado.documento ?? '',
      telefono: invitado.telefono ?? '',
      notas: invitado.notas ?? '',
      activo: invitado.activo,
    });
    setEditando(invitado);
    setAbierto(true);
  };

  return (
    <div className="mx-auto flex max-w-page flex-col gap-4">
      <Card>
        <CardHeader className="flex flex-col items-stretch gap-3">
          <div className="flex flex-wrap items-center gap-2">
            <Buscador placeholder="Buscar nombre, documento o teléfono…" value={busqueda} onCambio={setBusqueda} />
            <div className="ml-auto">
              <Button leftIcon={<Plus size={15} />} onClick={abrirCrear}>
                Agregar invitado
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
            <div className="ml-auto">
              <LimpiarFiltros activo={hayFiltros} onClick={limpiarFiltros} />
            </div>
          </div>
        </CardHeader>
        <CardBody>
          <DataTable<ListaVip>
            rows={filas}
            loading={invitados.isLoading}
            rowKey={(fila) => fila.id}
            empty={hayFiltros ? 'Sin invitados con esos filtros.' : 'La lista VIP está vacía.'}
            columns={[
              { key: 'nombre', header: 'Nombre', render: (fila) => fila.nombre },
              { key: 'documento', header: 'Documento', render: (fila) => fila.documento ?? '—' },
              { key: 'telefono', header: 'Teléfono', render: (fila) => fila.telefono ?? '—' },
              { key: 'estado', header: 'Estado', render: (fila) => <Pill tone={fila.activo ? 'success' : 'neutral'}>{fila.activo ? 'Activo' : 'Inactivo'}</Pill> },
              {
                key: 'acciones',
                header: '',
                align: 'right',
                render: (fila) => (
                  <ActionMenu
                    label={`Acciones de ${fila.nombre}`}
                    options={[
                      { label: 'Editar', icon: <Pencil size={15} />, onClick: () => abrirEditar(fila) },
                      { label: 'Quitar', icon: <Trash2 size={15} />, danger: true, onClick: () => setEliminar(fila) },
                    ]}
                  />
                ),
              },
            ]}
          />
        </CardBody>
      </Card>

      <Modal
        open={abierto}
        onClose={cerrar}
        title={editando ? 'Editar invitado' : 'Agregar invitado'}
        footer={
          <>
            <Button variant="ghost" onClick={cerrar}>
              Cancelar
            </Button>
            <Button type="submit" form="form-vip" loading={guardar.isPending}>
              {editando ? 'Guardar cambios' : 'Agregar'}
            </Button>
          </>
        }
      >
        <form id="form-vip" className="flex flex-col gap-3" onSubmit={form.handleSubmit((d) => guardar.mutate(d))} noValidate>
          <Input label="Nombre" error={form.formState.errors.nombre?.message} {...form.register('nombre')} />
          <div className="grid grid-cols-2 gap-3">
            <Input label="Documento" {...form.register('documento')} />
            <Input label="Teléfono" {...form.register('telefono')} />
          </div>
          <Input label="Notas" {...form.register('notas')} />
          <label className="flex items-center gap-2 text-sm text-foreground">
            <input type="checkbox" className="h-4 w-4 rounded-control border-border" {...form.register('activo')} />
            Activo
          </label>
        </form>
      </Modal>

      <Modal
        open={Boolean(eliminar)}
        onClose={() => setEliminar(null)}
        title="Quitar de la lista VIP"
        footer={
          <>
            <Button variant="ghost" onClick={() => setEliminar(null)}>
              Cancelar
            </Button>
            <Button variant="danger" loading={borrar.isPending} onClick={() => eliminar && borrar.mutate(eliminar.id)}>
              Quitar
            </Button>
          </>
        }
      >
        <p className="text-sm text-muted-foreground">
          ¿Quitar a <span className="text-foreground">{eliminar?.nombre}</span> de la lista VIP?
        </p>
      </Modal>
    </div>
  );
}
