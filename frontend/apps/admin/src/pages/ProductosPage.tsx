import { useState } from 'react';
import { useFieldArray, useForm } from 'react-hook-form';
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
  Select,
} from '@licoreria/ui';
import type { Producto } from '@licoreria/types';
import { catalogoApi } from '@licoreria/api-client';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { mensajeDeError } from '../lib/api';
import { formatUSD } from '../lib/format';

const esquemaVariante = z.object({
  id: z.string().optional(),
  nombre: z.string().min(1, 'Nombre de la variante'),
  sku: z.string().min(1, 'SKU requerido'),
  unidadMedidaId: z.string().min(1, 'Selecciona la unidad'),
  precioCompraUSD: z.coerce.number({ invalid_type_error: 'Precio inválido' }).min(0, 'No puede ser negativo'),
  precioVentaUSD: z.coerce.number({ invalid_type_error: 'Precio inválido' }).min(0, 'No puede ser negativo'),
});

const esquemaProducto = z.object({
  nombre: z.string().min(2, 'Ingresa el nombre'),
  descripcion: z.string().optional(),
  categoriaId: z.string().min(1, 'Selecciona una categoría'),
  marcaId: z.string(),
  tipo: z.enum(['Simple', 'Preparado']),
  gradoAlcoholico: z.coerce.number().min(0, 'Inválido').max(100, 'Inválido'),
  imagenUrl: z.string().optional(),
  activo: z.boolean(),
  variantes: z.array(esquemaVariante).min(1, 'Agrega al menos una variante'),
});

type FormularioProducto = z.infer<typeof esquemaProducto>;

const VACIO: FormularioProducto = {
  nombre: '',
  descripcion: '',
  categoriaId: '',
  marcaId: '',
  tipo: 'Simple',
  gradoAlcoholico: 0,
  imagenUrl: '',
  activo: true,
  variantes: [{ nombre: '', sku: '', unidadMedidaId: '', precioCompraUSD: 0, precioVentaUSD: 0 }],
};

export function ProductosPage() {
  const [page, setPage] = useState(1);
  const [busqueda, setBusqueda] = useState('');
  const [editando, setEditando] = useState<Producto | null>(null);
  const [creando, setCreando] = useState(false);
  const [porEliminar, setPorEliminar] = useState<Producto | null>(null);
  const queryClient = useQueryClient();

  const productoForm = useForm<FormularioProducto>({ resolver: zodResolver(esquemaProducto), defaultValues: VACIO });
  const { fields, append, remove } = useFieldArray({ control: productoForm.control, name: 'variantes' });

  const productos = useQuery({
    queryKey: ['productos', page, busqueda],
    queryFn: () => catalogoApi.productos({ page, pageSize: 12, busqueda, activo: true }),
  });
  const categorias = useQuery({ queryKey: ['categorias'], queryFn: catalogoApi.categorias });
  const marcas = useQuery({ queryKey: ['marcas'], queryFn: catalogoApi.marcas });
  const unidades = useQuery({ queryKey: ['unidades'], queryFn: catalogoApi.unidades });

  const invalidar = () => queryClient.invalidateQueries({ queryKey: ['productos'] });

  const guardar = useMutation({
    mutationFn: (datos: FormularioProducto) => {
      const body = {
        nombre: datos.nombre,
        descripcion: datos.descripcion ?? null,
        categoriaId: datos.categoriaId,
        marcaId: datos.marcaId || null,
        impuestoId: null,
        tipo: datos.tipo,
        gradoAlcoholico: datos.gradoAlcoholico || null,
        imagenUrl: datos.imagenUrl || null,
        variantes: datos.variantes.map((variante) => ({
          id: variante.id ?? null,
          nombre: variante.nombre,
          sku: variante.sku,
          unidadMedidaId: variante.unidadMedidaId,
          precioCompraUSD: variante.precioCompraUSD,
          precioVentaUSD: variante.precioVentaUSD,
          codigosBarras: [],
        })),
      };

      return editando
        ? catalogoApi.actualizar(editando.id, { id: editando.id, activo: datos.activo, ...body })
        : catalogoApi.crear(body);
    },
    onSuccess: () => {
      toast.success(editando ? 'Producto actualizado' : 'Producto creado');
      setCreando(false);
      setEditando(null);
      productoForm.reset(VACIO);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo guardar', { description: mensajeDeError(error) }),
  });

  const eliminar = useMutation({
    mutationFn: (id: string) => catalogoApi.eliminar(id),
    onSuccess: () => {
      toast.success('Producto eliminado');
      setPorEliminar(null);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo eliminar', { description: mensajeDeError(error) }),
  });

  const abrirCrear = () => {
    productoForm.reset(VACIO);
    setCreando(true);
  };

  const abrirEditar = (producto: Producto) => {
    productoForm.reset({
      nombre: producto.nombre,
      descripcion: producto.descripcion ?? '',
      categoriaId: producto.categoriaId,
      marcaId: producto.marcaId ?? '',
      tipo: producto.tipo,
      gradoAlcoholico: producto.gradoAlcoholico ?? 0,
      imagenUrl: producto.imagenUrl ?? '',
      activo: producto.activo,
      variantes: producto.variantes.map((variante) => ({
        id: variante.id,
        nombre: variante.nombre,
        sku: variante.sku,
        unidadMedidaId: variante.unidadMedidaId,
        precioCompraUSD: variante.precioCompraUSD,
        precioVentaUSD: variante.precioVentaUSD,
      })),
    });
    setEditando(producto);
  };

  const formularioAbierto = creando || Boolean(editando);

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader
        title="Catálogo"
        subtitle="Productos y variantes con precios."
        actions={<Button onClick={abrirCrear}>Nuevo producto</Button>}
      />
      <Card>
        <CardHeader>
          <CardTitle>Productos</CardTitle>
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
          <DataTable<Producto>
            rows={productos.data?.items ?? []}
            loading={productos.isLoading}
            rowKey={(producto) => producto.id}
            empty="No hay productos."
            columns={[
              {
                key: 'nombre',
                header: 'Producto',
                render: (producto) => (
                  <div>
                    <p className="text-ink">{producto.nombre}</p>
                    <p className="text-xs text-muted">{producto.categoriaNombre}</p>
                  </div>
                ),
              },
              { key: 'tipo', header: 'Tipo', render: (producto) => <Pill>{producto.tipo}</Pill> },
              { key: 'variantes', header: 'Variantes', align: 'center', render: (producto) => producto.variantes.length },
              {
                key: 'precio',
                header: 'Precio venta',
                align: 'right',
                render: (producto) => {
                  const precio = Math.min(...producto.variantes.map((variante) => variante.precioVentaUSD));
                  return Number.isFinite(precio) ? formatUSD(precio) : '—';
                },
              },
              {
                key: 'acciones',
                header: '',
                align: 'right',
                render: (producto) => (
                  <div className="flex justify-end gap-2">
                    <Button size="sm" variant="ghost" onClick={() => abrirEditar(producto)}>
                      Editar
                    </Button>
                    <Button size="sm" variant="ghost" onClick={() => setPorEliminar(producto)}>
                      Eliminar
                    </Button>
                  </div>
                ),
              },
            ]}
          />
          <Pagination page={page} totalPages={productos.data?.totalPages ?? 1} onPageChange={setPage} />
        </CardBody>
      </Card>

      <Modal
        open={formularioAbierto}
        onClose={() => {
          setCreando(false);
          setEditando(null);
        }}
        title={editando ? 'Editar producto' : 'Nuevo producto'}
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
            <Button type="submit" form="form-producto" loading={guardar.isPending}>
              Guardar
            </Button>
          </>
        }
      >
        <form
          id="form-producto"
          className="flex flex-col gap-3"
          onSubmit={productoForm.handleSubmit((datos) => guardar.mutate(datos))}
          noValidate
        >
          <Input label="Nombre" error={productoForm.formState.errors.nombre?.message} {...productoForm.register('nombre')} />
          <Input label="Descripción" {...productoForm.register('descripcion')} />
          <div className="grid grid-cols-2 gap-3">
            <Select label="Categoría" error={productoForm.formState.errors.categoriaId?.message} {...productoForm.register('categoriaId')}>
              <option value="">Selecciona…</option>
              {categorias.data?.map((categoria) => (
                <option key={categoria.id} value={categoria.id}>
                  {categoria.nombre}
                </option>
              ))}
            </Select>
            <Select label="Marca" {...productoForm.register('marcaId')}>
              <option value="">Sin marca</option>
              {marcas.data?.map((marca) => (
                <option key={marca.id} value={marca.id}>
                  {marca.nombre}
                </option>
              ))}
            </Select>
          </div>
          <div className="grid grid-cols-3 gap-3">
            <Select label="Tipo" {...productoForm.register('tipo')}>
              <option value="Simple">Simple</option>
              <option value="Preparado">Preparado</option>
            </Select>
            <Input label="Grado alcohólico" type="number" {...productoForm.register('gradoAlcoholico')} />
            <Input label="Imagen (URL)" {...productoForm.register('imagenUrl')} />
          </div>
          <label className="flex items-center gap-2 text-sm text-muted">
            <input type="checkbox" {...productoForm.register('activo')} />
            Activo
          </label>

          <div className="rounded-2xl border border-hairline p-3">
            <div className="mb-2 flex items-center justify-between">
              <p className="text-xs font-semibold uppercase tracking-tighter2 text-muted">Variantes</p>
              <Button
                type="button"
                size="sm"
                variant="ghost"
                onClick={() => append({ nombre: '', sku: '', unidadMedidaId: '', precioCompraUSD: 0, precioVentaUSD: 0 })}
              >
                Agregar
              </Button>
            </div>
            {productoForm.formState.errors.variantes?.message && (
              <p className="mb-2 text-xs text-danger">{productoForm.formState.errors.variantes.message}</p>
            )}
            <div className="flex flex-col gap-3">
              {fields.map((field, indice) => (
                <div key={field.id} className="grid grid-cols-2 gap-2 rounded-2xl bg-elevated/30 p-3 sm:grid-cols-6">
                  <Input placeholder="Nombre" {...productoForm.register(`variantes.${indice}.nombre`)} />
                  <Input placeholder="SKU" {...productoForm.register(`variantes.${indice}.sku`)} />
                  <Select aria-label="Unidad" {...productoForm.register(`variantes.${indice}.unidadMedidaId`)}>
                    <option value="">Unidad…</option>
                    {unidades.data?.map((unidad) => (
                      <option key={unidad.id} value={unidad.id}>
                        {unidad.abreviatura}
                      </option>
                    ))}
                  </Select>
                  <Input type="number" placeholder="Costo USD" {...productoForm.register(`variantes.${indice}.precioCompraUSD`)} />
                  <Input type="number" placeholder="Venta USD" {...productoForm.register(`variantes.${indice}.precioVentaUSD`)} />
                  <Button type="button" variant="ghost" size="sm" onClick={() => remove(indice)}>
                    Quitar
                  </Button>
                </div>
              ))}
            </div>
          </div>
        </form>
      </Modal>

      <ConfirmDialog
        open={Boolean(porEliminar)}
        title="Eliminar producto"
        description={`¿Seguro que deseas eliminar "${porEliminar?.nombre ?? ''}"?`}
        confirmLabel="Eliminar"
        loading={eliminar.isPending}
        onClose={() => setPorEliminar(null)}
        onConfirm={() => porEliminar && eliminar.mutate(porEliminar.id)}
      />
    </div>
  );
}
