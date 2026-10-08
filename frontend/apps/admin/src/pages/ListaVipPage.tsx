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
import type { ListaVip } from '@licoreria/types';
import { listaVipApi } from '@licoreria/api-client';
import { mensajeDeError } from '../lib/api';

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
  const [eliminar, setEliminar] = useState<ListaVip | null>(null);

  const invitados = useQuery({ queryKey: ['lista-vip'], queryFn: listaVipApi.listar });
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
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader
        title="Lista VIP"
        subtitle="Invitados y control de acceso preferente."
        actions={<Button onClick={abrirCrear}>Agregar invitado</Button>}
      />

      <Card>
        <CardHeader>
          <CardTitle>Invitados</CardTitle>
          <span className="text-xs text-muted-foreground">{invitados.data?.length ?? 0} en la lista</span>
        </CardHeader>
        <CardBody>
          <DataTable<ListaVip>
            rows={invitados.data ?? []}
            loading={invitados.isLoading}
            rowKey={(fila) => fila.id}
            empty="La lista VIP está vacía."
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
                  <div className="flex justify-end gap-2">
                    <Button size="sm" variant="ghost" onClick={() => abrirEditar(fila)}>
                      Editar
                    </Button>
                    <Button size="sm" variant="ghost" onClick={() => setEliminar(fila)}>
                      Quitar
                    </Button>
                  </div>
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
