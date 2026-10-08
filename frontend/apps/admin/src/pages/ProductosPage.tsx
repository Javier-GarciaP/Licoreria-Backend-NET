import { useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { ArrowLeft, Eye, Pencil, Plus, Trash2 } from 'lucide-react';
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
  ModalSection,
  Pagination,
  Pill,
} from '@licoreria/ui';
import type { Producto } from '@licoreria/types';
import { catalogoApi } from '@licoreria/api-client';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { CatalogoTabs } from '../components/CatalogoTabs';
import { FolderPanel } from '../components/FolderTabs';
import { Can } from '../components/Rbac';
import { FormularioProducto, type DatosProductoForm } from '../components/catalogo/FormularioProducto';
import { mensajeDeError } from '../lib/api';
import { formatUSD } from '../lib/format';

export function ProductosPage() {
  const [page, setPage] = useState(1);
  const [params] = useSearchParams();
  const [busqueda, setBusqueda] = useState(params.get('busqueda') ?? '');
  const [editando, setEditando] = useState<Producto | null>(null);
  const [creando, setCreando] = useState(false);
  const [detalle, setDetalle] = useState<Producto | null>(null);
  const [porEliminar, setPorEliminar] = useState<Producto | null>(null);
  const queryClient = useQueryClient();

  const productos = useQuery({
    queryKey: ['productos', page, busqueda],
    queryFn: () => catalogoApi.productos({ page, pageSize: 12, busqueda, activo: true }),
  });
  const categorias = useQuery({ queryKey: ['categorias'], queryFn: catalogoApi.categorias });
  const marcas = useQuery({ queryKey: ['marcas'], queryFn: catalogoApi.marcas });
  const unidades = useQuery({ queryKey: ['unidades'], queryFn: catalogoApi.unidades });

  const invalidar = () => queryClient.invalidateQueries({ queryKey: ['productos'] });

  const guardar = useMutation({
    mutationFn: (datos: DatosProductoForm) => {
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

  const cerrarFormulario = () => {
    setCreando(false);
    setEditando(null);
  };

  return (
    <div className="mx-auto flex max-w-page flex-col gap-4">
      <CatalogoTabs />

      <div className="flex flex-col">
        <FolderPanel className="flex flex-col gap-4">
          {creando || editando ? (
            /* Panel independiente de producto (muchos campos, estilo editor). */
            <section className="rounded-xl border border-primary/40 bg-card p-5 lg:p-6">
              <div className="flex flex-wrap items-center gap-3">
                <Button variant="ghost" size="sm" onClick={cerrarFormulario} aria-label="Volver">
                  <ArrowLeft size={16} />
                </Button>
                <div>
                  <p className="text-base font-medium tracking-tighter2 text-foreground">
                    {editando ? `Editar · ${editando.nombre}` : 'Nuevo producto'}
                  </p>
                  <p className="text-xs text-muted-foreground">
                    Define el producto, su presentación y precios. La existencia parte de cero.
                  </p>
                </div>
              </div>
              <div className="mt-5">
                <FormularioProducto
                  producto={editando}
                  categorias={categorias.data ?? []}
                  marcas={marcas.data ?? []}
                  unidades={unidades.data ?? []}
                  guardando={guardar.isPending}
                  onCancelar={cerrarFormulario}
                  onGuardar={(datos) => guardar.mutate(datos)}
                />
              </div>
            </section>
          ) : (
            <Card className="border-0 bg-transparent shadow-none">
              <CardHeader>
                <CardTitle>Productos</CardTitle>
                <div className="flex flex-wrap items-center gap-3">
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
                  <Can permiso="catalog:write">
                    <Button
                      leftIcon={<Plus size={15} />}
                      onClick={() => {
                        setEditando(null);
                        setCreando(true);
                      }}
                    >
                      Nuevo producto
                    </Button>
                  </Can>
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
                          <p className="text-foreground">{producto.nombre}</p>
                          <p className="text-xs text-muted-foreground">{producto.categoriaNombre}</p>
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
                        <ActionMenu
                          label={`Acciones de ${producto.nombre}`}
                          options={[
                            { label: 'Ver detalle', icon: <Eye size={15} />, onClick: () => setDetalle(producto) },
                            {
                              label: 'Editar',
                              icon: <Pencil size={15} />,
                              onClick: () => {
                                setEditando(producto);
                                setCreando(false);
                              },
                            },
                            {
                              label: 'Eliminar',
                              icon: <Trash2 size={15} />,
                              danger: true,
                              onClick: () => setPorEliminar(producto),
                            },
                          ]}
                        />
                      ),
                    },
                  ]}
                />
                <Pagination page={page} totalPages={productos.data?.totalPages ?? 1} onPageChange={setPage} />
              </CardBody>
            </Card>
          )}
        </FolderPanel>
      </div>

      <Modal
        open={Boolean(detalle)}
        onClose={() => setDetalle(null)}
        title={detalle?.nombre ?? ''}
        size="lg"
        backdrop="none"
      >
        {detalle && (
          <div className="flex flex-col gap-6">
            <div className="flex flex-wrap gap-2">
              <Pill>{detalle.tipo}</Pill>
              {detalle.categoriaNombre && <Pill tone="accent">{detalle.categoriaNombre}</Pill>}
              {detalle.marcaNombre && <Pill tone="info">{detalle.marcaNombre}</Pill>}
              {detalle.activo ? <Pill tone="success">Activo</Pill> : <Pill tone="danger">Inactivo</Pill>}
            </div>
            {detalle.descripcion && <p className="text-sm text-muted-foreground">{detalle.descripcion}</p>}
            <ModalSection title="Variantes">
              <div className="overflow-x-auto rounded-inner border border-border">
                <table className="w-full min-w-[480px] text-sm">
                  <thead>
                    <tr className="bg-muted/60 text-left text-xs uppercase tracking-tighter2 text-muted-foreground">
                      <th scope="col" className="px-4 py-2.5">Variante</th>
                      <th scope="col" className="px-4 py-2.5">SKU</th>
                      <th scope="col" className="px-4 py-2.5 text-right">Costo</th>
                      <th scope="col" className="px-4 py-2.5 text-right">Venta</th>
                    </tr>
                  </thead>
                  <tbody>
                    {detalle.variantes.map((variante) => (
                      <tr key={variante.id} className="border-t border-border">
                        <td className="px-4 py-2.5 text-foreground">{variante.nombre}</td>
                        <td className="num px-4 py-2.5 text-muted-foreground">{variante.sku}</td>
                        <td className="num px-4 py-2.5 text-right text-muted-foreground">{formatUSD(variante.precioCompraUSD)}</td>
                        <td className="num px-4 py-2.5 text-right text-foreground">{formatUSD(variante.precioVentaUSD)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </ModalSection>
          </div>
        )}
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