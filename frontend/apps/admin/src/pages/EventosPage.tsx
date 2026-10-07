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
import type { Evento } from '@licoreria/types';
import { contenidoApi } from '@licoreria/api-client';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { ContenidoTabs } from '../components/ContenidoTabs';
import { mensajeDeError } from '../lib/api';
import { formatDateTime } from '../lib/format';

const esquema = z.object({
  titulo: z.string().min(2, 'Ingresa el título'),
  descripcion: z.string().min(2, 'Ingresa la descripción'),
  fechaInicio: z.string().min(1, 'Selecciona la fecha de inicio'),
  fechaFin: z.string().optional(),
  imagenUrl: z.string().optional(),
  publicado: z.boolean(),
  activo: z.boolean(),
});

type Formulario = z.infer<typeof esquema>;

const VACIO: Formulario = {
  titulo: '',
  descripcion: '',
  fechaInicio: '',
  fechaFin: '',
  imagenUrl: '',
  publicado: false,
  activo: true,
};

export function EventosPage() {
  const [creando, setCreando] = useState(false);
  const [editando, setEditando] = useState<Evento | null>(null);
  const [porEliminar, setPorEliminar] = useState<Evento | null>(null);
  const queryClient = useQueryClient();

  const form = useForm<Formulario>({ resolver: zodResolver(esquema), defaultValues: VACIO });
  const eventos = useQuery({ queryKey: ['eventos-todos'], queryFn: contenidoApi.eventos });

  const invalidar = () => {
    queryClient.invalidateQueries({ queryKey: ['eventos-todos'] });
    queryClient.invalidateQueries({ queryKey: ['eventos'] });
  };

  const guardar = useMutation({
    mutationFn: (datos: Formulario) => {
      const body = {
        titulo: datos.titulo,
        descripcion: datos.descripcion,
        fechaInicio: new Date(datos.fechaInicio).toISOString(),
        fechaFin: datos.fechaFin ? new Date(datos.fechaFin).toISOString() : null,
        imagenUrl: datos.imagenUrl || null,
        publicado: datos.publicado,
      };
      return editando
        ? contenidoApi.actualizarEvento(editando.id, { id: editando.id, activo: datos.activo, ...body })
        : contenidoApi.crearEvento(body);
    },
    onSuccess: () => {
      toast.success(editando ? 'Evento actualizado' : 'Evento creado');
      setCreando(false);
      setEditando(null);
      form.reset(VACIO);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo guardar', { description: mensajeDeError(error) }),
  });

  const eliminar = useMutation({
    mutationFn: (id: string) => contenidoApi.eliminarEvento(id),
    onSuccess: () => {
      toast.success('Evento eliminado');
      setPorEliminar(null);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo eliminar', { description: mensajeDeError(error) }),
  });

  const abrirEditar = (evento: Evento) => {
    form.reset({
      titulo: evento.titulo,
      descripcion: evento.descripcion,
      fechaInicio: evento.fechaInicio.slice(0, 16),
      fechaFin: evento.fechaFin ? evento.fechaFin.slice(0, 16) : '',
      imagenUrl: evento.imagenUrl ?? '',
      publicado: evento.publicado,
      activo: evento.activo,
    });
    setEditando(evento);
  };

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader
        title="Contenido"
        subtitle="Eventos publicables en la web."
        actions={
          <Button
            onClick={() => {
              form.reset(VACIO);
              setCreando(true);
            }}
          >
            Nuevo evento
          </Button>
        }
      />
      <ContenidoTabs />

      <Card>
        <CardHeader>
          <CardTitle>Eventos</CardTitle>
        </CardHeader>
        <CardBody>
          <DataTable<Evento>
            rows={eventos.data ?? []}
            loading={eventos.isLoading}
            rowKey={(evento) => evento.id}
            empty="No hay eventos."
            columns={[
              { key: 'titulo', header: 'Título', render: (evento) => <span className="text-ink">{evento.titulo}</span> },
              { key: 'inicio', header: 'Inicio', render: (evento) => formatDateTime(evento.fechaInicio) },
              {
                key: 'publicado',
                header: 'Publicado',
                render: (evento) => (
                  <Pill tone={evento.publicado ? 'success' : 'neutral'}>{evento.publicado ? 'Sí' : 'No'}</Pill>
                ),
              },
              {
                key: 'activo',
                header: 'Activo',
                render: (evento) => <Pill tone={evento.activo ? 'success' : 'danger'}>{evento.activo ? 'Sí' : 'No'}</Pill>,
              },
              {
                key: 'acciones',
                header: '',
                align: 'right',
                render: (evento) => (
                  <div className="flex justify-end gap-2">
                    <Button size="sm" variant="ghost" onClick={() => abrirEditar(evento)}>
                      Editar
                    </Button>
                    <Button size="sm" variant="ghost" onClick={() => setPorEliminar(evento)}>
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
        title={editando ? 'Editar evento' : 'Nuevo evento'}
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
            <Button type="submit" form="form-evento" loading={guardar.isPending}>
              Guardar
            </Button>
          </>
        }
      >
        <form id="form-evento" className="flex flex-col gap-3" onSubmit={form.handleSubmit((d) => guardar.mutate(d))} noValidate>
          <Input label="Título" error={form.formState.errors.titulo?.message} {...form.register('titulo')} />
          <Input label="Descripción" error={form.formState.errors.descripcion?.message} {...form.register('descripcion')} />
          <div className="grid grid-cols-2 gap-3">
            <Input label="Inicio" type="datetime-local" error={form.formState.errors.fechaInicio?.message} {...form.register('fechaInicio')} />
            <Input label="Fin (opcional)" type="datetime-local" {...form.register('fechaFin')} />
          </div>
          <Input label="Imagen (URL)" {...form.register('imagenUrl')} />
          <div className="flex gap-6">
            <label className="flex items-center gap-2 text-sm text-muted">
              <input type="checkbox" {...form.register('publicado')} />
              Publicado
            </label>
            {editando && (
              <label className="flex items-center gap-2 text-sm text-muted">
                <input type="checkbox" {...form.register('activo')} />
                Activo
              </label>
            )}
          </div>
        </form>
      </Modal>

      <ConfirmDialog
        open={Boolean(porEliminar)}
        title="Eliminar evento"
        description={`¿Seguro que deseas eliminar "${porEliminar?.titulo ?? ''}"?`}
        confirmLabel="Eliminar"
        loading={eliminar.isPending}
        onClose={() => setPorEliminar(null)}
        onConfirm={() => porEliminar && eliminar.mutate(porEliminar.id)}
      />
    </div>
  );
}
