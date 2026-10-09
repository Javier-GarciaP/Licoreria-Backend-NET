import { useState } from 'react';
import { useFieldArray, useForm, type Control, type UseFormRegister } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Pencil, Trash2 } from 'lucide-react';
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
  PageHeader,
  Pill,
} from '@licoreria/ui';
import type { Pagina } from '@licoreria/types';
import { contenidoApi } from '@licoreria/api-client';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { ContenidoTabs } from '../components/ContenidoTabs';
import { mensajeDeError } from '../lib/api';

const esquemaBloque = z.object({
  tipo: z.string().min(1, 'Tipo requerido'),
  orden: z.coerce.number().int().min(0),
  contenido: z.string().min(1, 'Contenido requerido'),
  activo: z.boolean(),
});

const esquemaSeccion = z.object({
  titulo: z.string().min(1, 'Título requerido'),
  tipo: z.string().min(1, 'Tipo requerido'),
  orden: z.coerce.number().int().min(0),
  activa: z.boolean(),
  bloques: z.array(esquemaBloque),
});

const esquemaPagina = z.object({
  titulo: z.string().min(2, 'Ingresa el título'),
  slug: z.string().min(1, 'Ingresa el slug'),
  publicada: z.boolean(),
  activo: z.boolean(),
  secciones: z.array(esquemaSeccion),
});

type FormularioPagina = z.infer<typeof esquemaPagina>;

const VACIO: FormularioPagina = { titulo: '', slug: '', publicada: false, activo: true, secciones: [] };

function SeccionEditor({
  control,
  register,
  index,
  onRemove,
}: {
  control: Control<FormularioPagina>;
  register: UseFormRegister<FormularioPagina>;
  index: number;
  onRemove: () => void;
}) {
  const { fields, append, remove } = useFieldArray({ control, name: `secciones.${index}.bloques` });

  return (
    <div className="rounded-lg border border-border p-3">
      <div className="grid grid-cols-2 gap-2 sm:grid-cols-4">
        <Input placeholder="Título" {...register(`secciones.${index}.titulo`)} />
        <Input placeholder="Tipo" {...register(`secciones.${index}.tipo`)} />
        <Input type="number" placeholder="Orden" {...register(`secciones.${index}.orden`)} />
        <Button type="button" variant="ghost" size="sm" onClick={onRemove}>
          Quitar sección
        </Button>
      </div>
      <label className="mt-2 flex items-center gap-2 text-xs text-muted-foreground">
        <input type="checkbox" {...register(`secciones.${index}.activa`)} />
        Activa
      </label>

      <div className="mt-3 flex flex-col gap-2">
        {fields.map((field, bloqueIndice) => (
          <div key={field.id} className="grid grid-cols-2 gap-2 sm:grid-cols-4">
            <Input placeholder="Tipo bloque" {...register(`secciones.${index}.bloques.${bloqueIndice}.tipo`)} />
            <Input placeholder="Contenido" {...register(`secciones.${index}.bloques.${bloqueIndice}.contenido`)} />
            <Input type="number" placeholder="Orden" {...register(`secciones.${index}.bloques.${bloqueIndice}.orden`)} />
            <Button type="button" variant="ghost" size="sm" onClick={() => remove(bloqueIndice)}>
              Quitar
            </Button>
          </div>
        ))}
        <Button
          type="button"
          size="sm"
          variant="ghost"
          onClick={() => append({ tipo: 'texto', orden: 0, contenido: '', activo: true })}
        >
          Agregar bloque
        </Button>
      </div>
    </div>
  );
}

export function PaginasPage() {
  const [creando, setCreando] = useState(false);
  const [editando, setEditando] = useState<Pagina | null>(null);
  const [porEliminar, setPorEliminar] = useState<Pagina | null>(null);
  const queryClient = useQueryClient();

  const form = useForm<FormularioPagina>({ resolver: zodResolver(esquemaPagina), defaultValues: VACIO });
  const seccionesArray = useFieldArray({ control: form.control, name: 'secciones' });

  const paginas = useQuery({ queryKey: ['paginas'], queryFn: contenidoApi.paginas });

  const invalidar = () => {
    queryClient.invalidateQueries({ queryKey: ['paginas'] });
    queryClient.invalidateQueries({ queryKey: ['paginas-publicas'] });
  };

  const guardar = useMutation({
    mutationFn: (datos: FormularioPagina) => {
      const secciones = datos.secciones.map((seccion) => ({
        titulo: seccion.titulo,
        tipo: seccion.tipo,
        orden: seccion.orden,
        activa: seccion.activa,
        bloques: seccion.bloques.map((bloque) => ({
          tipo: bloque.tipo,
          orden: bloque.orden,
          contenido: bloque.contenido,
          activo: bloque.activo,
        })),
      }));
      return editando
        ? contenidoApi.actualizarPagina(editando.id, {
            id: editando.id,
            titulo: datos.titulo,
            slug: datos.slug,
            publicada: datos.publicada,
            activo: datos.activo,
            secciones,
          })
        : contenidoApi.crearPagina({ titulo: datos.titulo, slug: datos.slug, publicada: datos.publicada, secciones });
    },
    onSuccess: () => {
      toast.success(editando ? 'Página actualizada' : 'Página creada');
      setCreando(false);
      setEditando(null);
      form.reset(VACIO);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo guardar', { description: mensajeDeError(error) }),
  });

  const eliminar = useMutation({
    mutationFn: (id: string) => contenidoApi.eliminarPagina(id),
    onSuccess: () => {
      toast.success('Página eliminada');
      setPorEliminar(null);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo eliminar', { description: mensajeDeError(error) }),
  });

  const abrirEditar = (pagina: Pagina) => {
    form.reset({
      titulo: pagina.titulo,
      slug: pagina.slug,
      publicada: pagina.publicada,
      activo: pagina.activo,
      secciones: pagina.secciones.map((seccion) => ({
        titulo: seccion.titulo,
        tipo: seccion.tipo,
        orden: seccion.orden,
        activa: seccion.activa,
        bloques: seccion.bloques.map((bloque) => ({
          tipo: bloque.tipo,
          orden: bloque.orden,
          contenido: bloque.contenido,
          activo: bloque.activo,
        })),
      })),
    });
    setEditando(pagina);
  };

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader
        title="Contenido"
        subtitle="Páginas, secciones y bloques de la web."
        actions={
          <Button
            onClick={() => {
              form.reset(VACIO);
              setCreando(true);
            }}
          >
            Nueva página
          </Button>
        }
      />
      <ContenidoTabs />

      <Card>
        <CardHeader>
          <CardTitle>Páginas</CardTitle>
        </CardHeader>
        <CardBody>
          <DataTable<Pagina>
            rows={paginas.data ?? []}
            loading={paginas.isLoading}
            rowKey={(pagina) => pagina.id}
            empty="No hay páginas."
            columns={[
              {
                key: 'titulo',
                header: 'Título',
                render: (pagina) => (
                  <div>
                    <p className="text-foreground">{pagina.titulo}</p>
                    <p className="text-xs text-muted-foreground">/{pagina.slug}</p>
                  </div>
                ),
              },
              { key: 'secciones', header: 'Secciones', align: 'center', render: (pagina) => pagina.secciones.length },
              {
                key: 'publicada',
                header: 'Publicada',
                render: (pagina) => <Pill tone={pagina.publicada ? 'success' : 'neutral'}>{pagina.publicada ? 'Sí' : 'No'}</Pill>,
              },
              {
                key: 'acciones',
                header: '',
                align: 'right',
                render: (pagina) => (
                  <ActionMenu
                    label={`Acciones de ${pagina.titulo}`}
                    options={[
                      { label: 'Editar', icon: <Pencil size={15} />, onClick: () => abrirEditar(pagina) },
                      {
                        label: 'Eliminar',
                        icon: <Trash2 size={15} />,
                        danger: true,
                        onClick: () => setPorEliminar(pagina),
                      },
                    ]}
                  />
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
        title={editando ? 'Editar página' : 'Nueva página'}
        className="max-w-3xl"
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
            <Button type="submit" form="form-pagina" loading={guardar.isPending}>
              Guardar
            </Button>
          </>
        }
      >
        <form id="form-pagina" className="flex flex-col gap-3" onSubmit={form.handleSubmit((d) => guardar.mutate(d))} noValidate>
          <div className="grid grid-cols-2 gap-3">
            <Input label="Título" error={form.formState.errors.titulo?.message} {...form.register('titulo')} />
            <Input label="Slug" error={form.formState.errors.slug?.message} {...form.register('slug')} />
          </div>
          <div className="flex gap-6">
            <label className="flex items-center gap-2 text-sm text-muted-foreground">
              <input type="checkbox" {...form.register('publicada')} />
              Publicada
            </label>
            {editando && (
              <label className="flex items-center gap-2 text-sm text-muted-foreground">
                <input type="checkbox" {...form.register('activo')} />
                Activa
              </label>
            )}
          </div>

          <div className="flex flex-col gap-2">
            <div className="flex items-center justify-between">
              <p className="text-xs font-medium uppercase tracking-tighter2 text-muted-foreground">Secciones</p>
              <Button
                type="button"
                size="sm"
                variant="ghost"
                onClick={() => seccionesArray.append({ titulo: '', tipo: 'texto', orden: 0, activa: true, bloques: [] })}
              >
                Agregar sección
              </Button>
            </div>
            {seccionesArray.fields.map((field, indice) => (
              <SeccionEditor
                key={field.id}
                control={form.control}
                register={form.register}
                index={indice}
                onRemove={() => seccionesArray.remove(indice)}
              />
            ))}
            {seccionesArray.fields.length === 0 && (
              <p className="text-xs text-muted-foreground">Sin secciones. Agrega al menos una.</p>
            )}
          </div>
        </form>
      </Modal>

      <ConfirmDialog
        open={Boolean(porEliminar)}
        title="Eliminar página"
        description={`¿Seguro que deseas eliminar "${porEliminar?.titulo ?? ''}"?`}
        confirmLabel="Eliminar"
        loading={eliminar.isPending}
        onClose={() => setPorEliminar(null)}
        onConfirm={() => porEliminar && eliminar.mutate(porEliminar.id)}
      />
    </div>
  );
}
