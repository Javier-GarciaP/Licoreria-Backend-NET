import { useMemo, useRef, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Pencil, Plus, Trash2 } from 'lucide-react';
import {
  ActionMenu,
  BubbleModal,
  Buscador,
  Button,
  Card,
  CardBody,
  CardHeader,
  FiltroDropdown,
  Input,
  LimpiarFiltros,
  Pill,
  Skeleton,
} from '@licoreria/ui';
import type { Categoria, Marca } from '@licoreria/types';
import { catalogoApi } from '@licoreria/api-client';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { mensajeDeError } from '../lib/api';
import { contiene } from '../lib/filtros';

const esquemaSimple = z.object({
  nombre: z.string().min(2, 'Ingresa el nombre'),
  descripcion: z.string().optional(),
  activo: z.boolean(),
});

type FormularioSimple = z.infer<typeof esquemaSimple>;
const VACIO: FormularioSimple = { nombre: '', descripcion: '', activo: true };

function FormularioSimple({
  inicial,
  guardando,
  onGuardar,
}: {
  inicial: FormularioSimple;
  guardando: boolean;
  onGuardar: (datos: FormularioSimple) => void;
}) {
  const form = useForm<FormularioSimple>({ resolver: zodResolver(esquemaSimple), defaultValues: inicial });
  return (
    <form className="flex flex-col gap-3" onSubmit={form.handleSubmit(onGuardar)} noValidate>
      <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
        <Input label="Nombre" error={form.formState.errors.nombre?.message} {...form.register('nombre')} />
        <Input label="Descripción" {...form.register('descripcion')} />
      </div>
      <label className="flex items-center gap-2 text-sm text-muted-foreground">
        <input type="checkbox" {...form.register('activo')} />
        Activa
      </label>
      <div className="mt-1 flex justify-end">
        <Button type="submit" loading={guardando}>
          Guardar
        </Button>
      </div>
    </form>
  );
}

export function CatalogosPage() {
  const queryClient = useQueryClient();
  const botonCategoriaRef = useRef<HTMLSpanElement>(null);
  const botonMarcaRef = useRef<HTMLSpanElement>(null);
  const [categoriaCreando, setCategoriaCreando] = useState(false);
  const [categoriaEditando, setCategoriaEditando] = useState<Categoria | null>(null);
  const [categoriaEliminar, setCategoriaEliminar] = useState<Categoria | null>(null);
  const [marcaCreando, setMarcaCreando] = useState(false);
  const [marcaEditando, setMarcaEditando] = useState<Marca | null>(null);
  const [marcaEliminar, setMarcaEliminar] = useState<Marca | null>(null);
  const [busqueda, setBusqueda] = useState('');
  const [estado, setEstado] = useState('');

  const categorias = useQuery({ queryKey: ['categorias'], queryFn: catalogoApi.categorias });
  const marcas = useQuery({ queryKey: ['marcas'], queryFn: catalogoApi.marcas });

  const categoriasVisibles = useMemo(
    () =>
      (categorias.data ?? []).filter((categoria) => {
        if (!contiene(categoria.nombre, busqueda)) return false;
        if (estado === 'activos' && !categoria.activo) return false;
        if (estado === 'inactivos' && categoria.activo) return false;
        return true;
      }),
    [categorias.data, busqueda, estado],
  );

  const marcasVisibles = useMemo(
    () =>
      (marcas.data ?? []).filter((marca) => {
        if (!contiene(marca.nombre, busqueda)) return false;
        if (estado === 'activos' && !marca.activo) return false;
        if (estado === 'inactivos' && marca.activo) return false;
        return true;
      }),
    [marcas.data, busqueda, estado],
  );

  const hayFiltros = Boolean(busqueda || estado);

  const limpiarFiltros = () => {
    setBusqueda('');
    setEstado('');
  };

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

  const editarCategoria = (categoria: Categoria) => {
    setCategoriaEditando(categoria);
    setCategoriaCreando(false);
  };

  const editarMarca = (marca: Marca) => {
    setMarcaEditando(marca);
    setMarcaCreando(false);
  };

  return (
    <div className="mx-auto flex max-w-page flex-col gap-4">
      <Card>
        <CardHeader className="flex flex-col items-stretch gap-3">
          <div className="flex flex-wrap items-center gap-2">
            <Buscador
              placeholder="Buscar categoría o marca…"
              value={busqueda}
              onCambio={setBusqueda}
            />
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
        <CardBody className="grid grid-cols-1 gap-6 xl:grid-cols-2">
        <section className="flex flex-col gap-2">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <h3 className="text-lg font-medium tracking-tighter2 text-card-foreground">Categorías</h3>
            <span ref={botonCategoriaRef}>
              <Button
                size="sm"
                leftIcon={<Plus size={15} />}
                onClick={() => {
                  setCategoriaEditando(null);
                  setCategoriaCreando(true);
                }}
              >
                Nueva
              </Button>
            </span>
          </div>
          <div className="flex flex-col gap-2">
            {categorias.isLoading ? (
              <Skeleton className="h-24 w-full" />
            ) : (
              categoriasVisibles.map((categoria) => (
                <div
                  key={categoria.id}
                  className={`flex items-center justify-between gap-2 rounded-lg bg-muted/40 px-3 py-2 ${categoriaEditando?.id === categoria.id ? 'ring-1 ring-ring/50' : ''}`}
                >
                  <div>
                    <p className="text-sm text-foreground">{categoria.nombre}</p>
                    <p className="text-xs text-muted-foreground">{categoria.descripcion ?? '—'}</p>
                  </div>
                  <div className="flex items-center gap-2">
                    <Pill tone={categoria.activo ? 'success' : 'danger'}>{categoria.activo ? 'Activa' : 'Inactiva'}</Pill>
                    <ActionMenu
                      label={`Acciones de ${categoria.nombre}`}
                      options={[
                        { label: 'Editar', icon: <Pencil size={15} />, onClick: () => editarCategoria(categoria) },
                        { label: 'Eliminar', icon: <Trash2 size={15} />, danger: true, onClick: () => setCategoriaEliminar(categoria) },
                      ]}
                    />
                  </div>
                </div>
              ))
            )}
          </div>
        </section>

        <section className="flex flex-col gap-2">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <h3 className="text-lg font-medium tracking-tighter2 text-card-foreground">Marcas</h3>
            <span ref={botonMarcaRef}>
              <Button
                size="sm"
                leftIcon={<Plus size={15} />}
                onClick={() => {
                  setMarcaEditando(null);
                  setMarcaCreando(true);
                }}
              >
                Nueva
              </Button>
            </span>
          </div>
          <div className="flex flex-col gap-2">
            {marcas.isLoading ? (
              <Skeleton className="h-24 w-full" />
            ) : (
              marcasVisibles.map((marca) => (
                <div
                  key={marca.id}
                  className={`flex items-center justify-between gap-2 rounded-lg bg-muted/40 px-3 py-2 ${marcaEditando?.id === marca.id ? 'ring-1 ring-ring/50' : ''}`}
                >
                  <div>
                    <p className="text-sm text-foreground">{marca.nombre}</p>
                    <p className="text-xs text-muted-foreground">{marca.descripcion ?? '—'}</p>
                  </div>
                  <div className="flex items-center gap-2">
                    <Pill tone={marca.activo ? 'success' : 'danger'}>{marca.activo ? 'Activa' : 'Inactiva'}</Pill>
                    <ActionMenu
                      label={`Acciones de ${marca.nombre}`}
                      options={[
                        { label: 'Editar', icon: <Pencil size={15} />, onClick: () => editarMarca(marca) },
                        { label: 'Eliminar', icon: <Trash2 size={15} />, danger: true, onClick: () => setMarcaEliminar(marca) },
                      ]}
                    />
                  </div>
                </div>
              ))
            )}
          </div>
        </section>
        </CardBody>
      </Card>

      <BubbleModal
        open={categoriaCreando || Boolean(categoriaEditando)}
        onClose={() => {
          setCategoriaCreando(false);
          setCategoriaEditando(null);
        }}
        title={categoriaEditando ? `Editar · ${categoriaEditando.nombre}` : 'Nueva categoría'}
        anchor={botonCategoriaRef.current}
      >
        <FormularioSimple
          inicial={
            categoriaEditando
              ? { nombre: categoriaEditando.nombre, descripcion: categoriaEditando.descripcion ?? '', activo: categoriaEditando.activo }
              : VACIO
          }
          guardando={guardarCategoria.isPending}
          onGuardar={(datos) => guardarCategoria.mutate(datos)}
        />
      </BubbleModal>

      <BubbleModal
        open={marcaCreando || Boolean(marcaEditando)}
        onClose={() => {
          setMarcaCreando(false);
          setMarcaEditando(null);
        }}
        title={marcaEditando ? `Editar · ${marcaEditando.nombre}` : 'Nueva marca'}
        anchor={botonMarcaRef.current}
      >
        <FormularioSimple
          inicial={
            marcaEditando
              ? { nombre: marcaEditando.nombre, descripcion: marcaEditando.descripcion ?? '', activo: marcaEditando.activo }
              : VACIO
          }
          guardando={guardarMarca.isPending}
          onGuardar={(datos) => guardarMarca.mutate(datos)}
        />
      </BubbleModal>

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