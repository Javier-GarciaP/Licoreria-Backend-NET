import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Button, Card, CardBody, CardHeader, CardTitle, Input, Modal, PageHeader, Pill, Skeleton } from '@licoreria/ui';
import type { Categoria, Marca } from '@licoreria/types';
import { catalogoApi } from '@licoreria/api-client';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { CatalogoTabs } from '../components/CatalogoTabs';
import { mensajeDeError } from '../lib/api';

const esquemaSimple = z.object({
  nombre: z.string().min(2, 'Ingresa el nombre'),
  descripcion: z.string().optional(),
  activo: z.boolean(),
});

type FormularioSimple = z.infer<typeof esquemaSimple>;
const VACIO: FormularioSimple = { nombre: '', descripcion: '', activo: true };

export function CatalogosPage() {
  const queryClient = useQueryClient();
  const [categoriaEditando, setCategoriaEditando] = useState<Categoria | null>(null);
  const [categoriaCreando, setCategoriaCreando] = useState(false);
  const [categoriaEliminar, setCategoriaEliminar] = useState<Categoria | null>(null);
  const [marcaEditando, setMarcaEditando] = useState<Marca | null>(null);
  const [marcaCreando, setMarcaCreando] = useState(false);
  const [marcaEliminar, setMarcaEliminar] = useState<Marca | null>(null);

  const categoriaForm = useForm<FormularioSimple>({ resolver: zodResolver(esquemaSimple), defaultValues: VACIO });
  const marcaForm = useForm<FormularioSimple>({ resolver: zodResolver(esquemaSimple), defaultValues: VACIO });

  const categorias = useQuery({ queryKey: ['categorias'], queryFn: catalogoApi.categorias });
  const marcas = useQuery({ queryKey: ['marcas'], queryFn: catalogoApi.marcas });

  const invalidar = () => {
    queryClient.invalidateQueries({ queryKey: ['categorias'] });
    queryClient.invalidateQueries({ queryKey: ['marcas'] });
  };

  const guardarCategoria = useMutation({
    mutationFn: (datos: FormularioSimple) =>
      categoriaEditando
        ? catalogoApi.actualizarCategoria(categoriaEditando.id, {
            id: categoriaEditando.id,
            nombre: datos.nombre,
            descripcion: datos.descripcion ?? null,
            categoriaPadreId: categoriaEditando.categoriaPadreId,
            activo: datos.activo,
          })
        : catalogoApi.crearCategoria({
            nombre: datos.nombre,
            descripcion: datos.descripcion ?? null,
            categoriaPadreId: null,
            activo: datos.activo,
          }),
    onSuccess: () => {
      toast.success(categoriaEditando ? 'Categoría actualizada' : 'Categoría creada');
      setCategoriaCreando(false);
      setCategoriaEditando(null);
      categoriaForm.reset(VACIO);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo guardar', { description: mensajeDeError(error) }),
  });

  const eliminarCategoria = useMutation({
    mutationFn: (id: string) => catalogoApi.eliminarCategoria(id),
    onSuccess: () => {
      toast.success('Categoría eliminada');
      setCategoriaEliminar(null);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo eliminar', { description: mensajeDeError(error) }),
  });

  const guardarMarca = useMutation({
    mutationFn: (datos: FormularioSimple) =>
      marcaEditando
        ? catalogoApi.actualizarMarca(marcaEditando.id, {
            id: marcaEditando.id,
            nombre: datos.nombre,
            descripcion: datos.descripcion ?? null,
            activo: datos.activo,
          })
        : catalogoApi.crearMarca({
            nombre: datos.nombre,
            descripcion: datos.descripcion ?? null,
            activo: datos.activo,
          }),
    onSuccess: () => {
      toast.success(marcaEditando ? 'Marca actualizada' : 'Marca creada');
      setMarcaCreando(false);
      setMarcaEditando(null);
      marcaForm.reset(VACIO);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo guardar', { description: mensajeDeError(error) }),
  });

  const eliminarMarca = useMutation({
    mutationFn: (id: string) => catalogoApi.eliminarMarca(id),
    onSuccess: () => {
      toast.success('Marca eliminada');
      setMarcaEliminar(null);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo eliminar', { description: mensajeDeError(error) }),
  });

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader title="Catálogo" subtitle="Productos, categorías, marcas y precios." />
      <CatalogoTabs />

      <div className="grid grid-cols-1 gap-6 xl:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Categorías</CardTitle>
            <Button
              size="sm"
              onClick={() => {
                categoriaForm.reset(VACIO);
                setCategoriaCreando(true);
              }}
            >
              Nueva
            </Button>
          </CardHeader>
          <CardBody className="flex flex-col gap-2">
            {categorias.isLoading ? (
              <Skeleton className="h-24 w-full" />
            ) : (
              categorias.data?.map((categoria) => (
                <div key={categoria.id} className="flex items-center justify-between gap-2 rounded-2xl bg-elevated/40 px-3 py-2">
                  <div>
                    <p className="text-sm text-ink">{categoria.nombre}</p>
                    <p className="text-xs text-muted">{categoria.descripcion ?? '—'}</p>
                  </div>
                  <div className="flex items-center gap-2">
                    <Pill tone={categoria.activo ? 'success' : 'danger'}>{categoria.activo ? 'Activa' : 'Inactiva'}</Pill>
                    <Button
                      size="sm"
                      variant="ghost"
                      onClick={() => {
                        categoriaForm.reset({
                          nombre: categoria.nombre,
                          descripcion: categoria.descripcion ?? '',
                          activo: categoria.activo,
                        });
                        setCategoriaEditando(categoria);
                      }}
                    >
                      Editar
                    </Button>
                    <Button size="sm" variant="ghost" onClick={() => setCategoriaEliminar(categoria)}>
                      Eliminar
                    </Button>
                  </div>
                </div>
              ))
            )}
          </CardBody>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Marcas</CardTitle>
            <Button
              size="sm"
              onClick={() => {
                marcaForm.reset(VACIO);
                setMarcaCreando(true);
              }}
            >
              Nueva
            </Button>
          </CardHeader>
          <CardBody className="flex flex-col gap-2">
            {marcas.isLoading ? (
              <Skeleton className="h-24 w-full" />
            ) : (
              marcas.data?.map((marca) => (
                <div key={marca.id} className="flex items-center justify-between gap-2 rounded-2xl bg-elevated/40 px-3 py-2">
                  <div>
                    <p className="text-sm text-ink">{marca.nombre}</p>
                    <p className="text-xs text-muted">{marca.descripcion ?? '—'}</p>
                  </div>
                  <div className="flex items-center gap-2">
                    <Pill tone={marca.activo ? 'success' : 'danger'}>{marca.activo ? 'Activa' : 'Inactiva'}</Pill>
                    <Button
                      size="sm"
                      variant="ghost"
                      onClick={() => {
                        marcaForm.reset({
                          nombre: marca.nombre,
                          descripcion: marca.descripcion ?? '',
                          activo: marca.activo,
                        });
                        setMarcaEditando(marca);
                      }}
                    >
                      Editar
                    </Button>
                    <Button size="sm" variant="ghost" onClick={() => setMarcaEliminar(marca)}>
                      Eliminar
                    </Button>
                  </div>
                </div>
              ))
            )}
          </CardBody>
        </Card>
      </div>

      <Modal
        open={categoriaCreando || Boolean(categoriaEditando)}
        onClose={() => {
          setCategoriaCreando(false);
          setCategoriaEditando(null);
        }}
        title={categoriaEditando ? 'Editar categoría' : 'Nueva categoría'}
        footer={
          <>
            <Button
              variant="ghost"
              onClick={() => {
                setCategoriaCreando(false);
                setCategoriaEditando(null);
              }}
            >
              Cancelar
            </Button>
            <Button type="submit" form="form-categoria" loading={guardarCategoria.isPending}>
              Guardar
            </Button>
          </>
        }
      >
        <form id="form-categoria" className="flex flex-col gap-3" onSubmit={categoriaForm.handleSubmit((d) => guardarCategoria.mutate(d))} noValidate>
          <Input label="Nombre" error={categoriaForm.formState.errors.nombre?.message} {...categoriaForm.register('nombre')} />
          <Input label="Descripción" {...categoriaForm.register('descripcion')} />
          <label className="flex items-center gap-2 text-sm text-muted">
            <input type="checkbox" {...categoriaForm.register('activo')} />
            Activa
          </label>
        </form>
      </Modal>

      <Modal
        open={marcaCreando || Boolean(marcaEditando)}
        onClose={() => {
          setMarcaCreando(false);
          setMarcaEditando(null);
        }}
        title={marcaEditando ? 'Editar marca' : 'Nueva marca'}
        footer={
          <>
            <Button
              variant="ghost"
              onClick={() => {
                setMarcaCreando(false);
                setMarcaEditando(null);
              }}
            >
              Cancelar
            </Button>
            <Button type="submit" form="form-marca" loading={guardarMarca.isPending}>
              Guardar
            </Button>
          </>
        }
      >
        <form id="form-marca" className="flex flex-col gap-3" onSubmit={marcaForm.handleSubmit((d) => guardarMarca.mutate(d))} noValidate>
          <Input label="Nombre" error={marcaForm.formState.errors.nombre?.message} {...marcaForm.register('nombre')} />
          <Input label="Descripción" {...marcaForm.register('descripcion')} />
          <label className="flex items-center gap-2 text-sm text-muted">
            <input type="checkbox" {...marcaForm.register('activo')} />
            Activa
          </label>
        </form>
      </Modal>

      <ConfirmDialog
        open={Boolean(categoriaEliminar)}
        title="Eliminar categoría"
        description={`¿Seguro que deseas eliminar "${categoriaEliminar?.nombre ?? ''}"?`}
        confirmLabel="Eliminar"
        loading={eliminarCategoria.isPending}
        onClose={() => setCategoriaEliminar(null)}
        onConfirm={() => categoriaEliminar && eliminarCategoria.mutate(categoriaEliminar.id)}
      />
      <ConfirmDialog
        open={Boolean(marcaEliminar)}
        title="Eliminar marca"
        description={`¿Seguro que deseas eliminar "${marcaEliminar?.nombre ?? ''}"?`}
        confirmLabel="Eliminar"
        loading={eliminarMarca.isPending}
        onClose={() => setMarcaEliminar(null)}
        onConfirm={() => marcaEliminar && eliminarMarca.mutate(marcaEliminar.id)}
      />
    </div>
  );
}
