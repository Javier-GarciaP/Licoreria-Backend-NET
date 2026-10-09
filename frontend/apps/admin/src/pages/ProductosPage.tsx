import { useEffect, useMemo, useState } from 'react';
import { useLocation, useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { motion, LayoutGroup } from 'framer-motion';
import { Pencil, Plus, Trash2, Wine } from 'lucide-react';
import {
  ActionMenu,
  Buscador,
  Button,
  Card,
  CardBody,
  CardHeader,
  DataTable,
  FiltroDropdown,
  FiltroRango,
  LimpiarFiltros,
  Pagination,
  Pill,
  Skeleton,
} from '@licoreria/ui';
import type { Producto } from '@licoreria/types';
import { catalogoApi } from '@licoreria/api-client';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { Can } from '../components/Rbac';
import { FormularioProducto, type DatosProductoForm } from '../components/catalogo/FormularioProducto';
import { INSUMO_BASE } from '../components/catalogo/EditorConsumo';
import { PanelDetalleProducto } from '../components/catalogo/PanelDetalleProducto';
import { mensajeDeError } from '../lib/api';
import { formatNumber, formatUSD } from '../lib/format';
import { urlDeImagen } from '../lib/imagenProducto';
import { paginarEnMemoria, PAGE_SIZE_FILTRO_LOCAL } from '../lib/filtros';

const imagenIdDe = (producto: Producto) => `prod-img-${producto.id}`;

function ImagenMiniatura({ producto }: { producto: Producto }) {
  const imagen = urlDeImagen(producto.imagenUrl);
  const layoutId = imagenIdDe(producto);
  if (imagen) {
    return (
      <motion.img
        layoutId={layoutId}
        layout
        src={imagen}
        alt=""
        className="h-10 w-10 shrink-0 rounded-lg border border-border object-cover"
      />
    );
  }
  return (
    <motion.div
      layoutId={layoutId}
      layout
      className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg border border-dashed border-border bg-muted/30 text-muted-foreground"
    >
      <Wine size={16} />
    </motion.div>
  );
}

export function ProductosPage() {
  const [page, setPage] = useState(1);
  const [params] = useSearchParams();
  const [busqueda, setBusqueda] = useState(params.get('busqueda') ?? '');
  const [categoria, setCategoria] = useState('');
  const [marca, setMarca] = useState('');
  const [tipo, setTipo] = useState('');
  const [soloActivos, setSoloActivos] = useState('');
  const [precioMin, setPrecioMin] = useState('');
  const [precioMax, setPrecioMax] = useState('');
  const [expandido, setExpandido] = useState<Producto | null>(null);
  const [porEliminar, setPorEliminar] = useState<Producto | null>(null);
  const navigate = useNavigate();
  const { pathname } = useLocation();
  const { id } = useParams();
  const queryClient = useQueryClient();

  const esCrear = pathname === '/productos/nuevo';
  const esEditar = /^\/productos\/[^/]+\/editar$/.test(pathname);
  const esEditor = esCrear || esEditar;

  useEffect(() => {
    setExpandido(null);
  }, [pathname]);

  const hayFiltroLocal = Boolean(marca || tipo || soloActivos || precioMin || precioMax);
  const hayFiltros = hayFiltroLocal || Boolean(busqueda || categoria);

  const limpiarFiltros = () => {
    setBusqueda('');
    setCategoria('');
    setMarca('');
    setTipo('');
    setSoloActivos('');
    setPrecioMin('');
    setPrecioMax('');
    setPage(1);
  };

  const productos = useQuery({
    queryKey: ['productos', page, busqueda, categoria, hayFiltroLocal ? PAGE_SIZE_FILTRO_LOCAL : 12],
    queryFn: () =>
      catalogoApi.productos({
        page: hayFiltroLocal ? 1 : page,
        pageSize: hayFiltroLocal ? PAGE_SIZE_FILTRO_LOCAL : 12,
        busqueda,
        categoriaId: categoria || undefined,
        activo: true,
      }),
  });
  const categorias = useQuery({ queryKey: ['categorias'], queryFn: catalogoApi.categorias });
  const marcas = useQuery({ queryKey: ['marcas'], queryFn: catalogoApi.marcas });
  const unidades = useQuery({ queryKey: ['unidades'], queryFn: catalogoApi.unidades });
  const detalle = useQuery({
    queryKey: ['producto', id],
    queryFn: () => catalogoApi.producto(id as string),
    enabled: esEditar,
  });
  const recetasPorVariante = useQuery({
    queryKey: ['recetas', 'producto', id],
    queryFn: async () => {
      const producto = detalle.data!;
      const entradas = await Promise.all(
        producto.variantes.map(async (variante) => [variante.id, await catalogoApi.recetas(variante.id)] as const),
      );
      return Object.fromEntries(entradas);
    },
    enabled: esEditar && Boolean(detalle.data),
  });

  const invalidar = () => {
    queryClient.invalidateQueries({ queryKey: ['productos'] });
    queryClient.invalidateQueries({ queryKey: ['stock'] });
  };

  const opcionesTipo = [
    { valor: 'Simple', etiqueta: 'Simple' },
    { valor: 'Preparado', etiqueta: 'Preparado' },
  ];

  const { items: filas, totalPages } = useMemo(() => {
    const precioMinN = Number(precioMin);
    const precioMaxN = Number(precioMax);
    const filtradas = (productos.data?.items ?? []).filter((producto) => {
      if (marca && producto.marcaNombre !== marca) return false;
      if (tipo && producto.tipo !== tipo) return false;
      if (soloActivos && (soloActivos === 'activos') !== producto.activo) return false;
      if (precioMinN || precioMaxN) {
        const menor = Math.min(...producto.variantes.map((variante) => variante.precioVentaUSD));
        if (precioMinN && menor < precioMinN) return false;
        if (precioMaxN && menor > precioMaxN) return false;
      }
      return true;
    });
    return hayFiltroLocal
      ? paginarEnMemoria(filtradas, page, 12)
      : { items: filtradas, totalPages: productos.data?.totalPages ?? 1 };
  }, [productos.data, marca, tipo, soloActivos, precioMin, precioMax, hayFiltroLocal, page]);

  const editando = esEditar ? detalle.data : null;

  const sincronizarConsumos = async (datos: DatosProductoForm, guardado: Producto) => {
    const baseId = guardado.variantes.find((variante) => variante.esBase)?.id;
    for (let indice = 0; indice < datos.variantes.length; indice += 1) {
      const variante = datos.variantes[indice];
      const guardada =
        guardado.variantes.find((v) => v.sku.trim() === (variante.sku ?? '').trim()) ?? guardado.variantes[indice];
      if (!guardada) continue;
      const actuales = await catalogoApi.recetas(guardada.id);
      for (const receta of actuales) await catalogoApi.eliminarReceta(guardada.id, receta.id);
      for (const consumo of variante.recetas ?? []) {
        const insumoId = consumo.varianteInsumoId === INSUMO_BASE ? baseId : consumo.varianteInsumoId;
        const cantidad = Number(consumo.cantidad);
        if (insumoId && cantidad > 0) {
          await catalogoApi.agregarReceta(guardada.id, { varianteInsumoId: insumoId, cantidad });
        }
      }
    }
  };

  const guardar = useMutation({
    mutationFn: async (datos: DatosProductoForm) => {
      // Cocina y preparados no llevan stock propio: no se ordena al proveedor.
      const sinStock = datos.areaDestino === 'Cocina' || datos.tipo === 'Preparado';
      const body = {
        nombre: datos.nombre,
        descripcion: datos.descripcion ?? null,
        categoriaId: datos.categoriaId,
        marcaId: datos.marcaId || null,
        impuestoId: null,
        tipo: datos.tipo,
        areaDestino: datos.areaDestino,
        gradoAlcoholico: datos.gradoAlcoholico || null,
        imagenUrl: datos.imagenUrl || null,
        variantes: datos.variantes.map((variante) => ({
          id: variante.id ?? null,
          nombre: variante.nombre,
          sku: variante.sku,
          unidadMedidaId: variante.unidadMedidaId,
          precioCompraUSD: variante.precioCompraUSD === '' ? 0 : variante.precioCompraUSD,
          precioVentaUSD: variante.precioVentaUSD === '' ? 0 : variante.precioVentaUSD,
          codigosBarras: variante.codigoBarras ? [variante.codigoBarras] : [],
          esBase: datos.areaDestino === 'Barra' && datos.tipo !== 'Preparado' && variante.esBase,
          stockInicial: sinStock ? null : (variante.stockInicial === '' ? null : variante.stockInicial),
          stockMinimo: sinStock ? null : (variante.stockMinimo === '' ? 0 : variante.stockMinimo),
          stockMaximo: sinStock ? null : (variante.stockMaximo === '' ? 0 : variante.stockMaximo),
        })),
      };

      const guardado = editando
        ? await catalogoApi.actualizar(editando.id, { id: editando.id, activo: datos.activo, ...body })
        : await catalogoApi.crear(body);

      await sincronizarConsumos(datos, guardado);
      return guardado;
    },
    onSuccess: () => {
      toast.success(editando ? 'Producto actualizado' : 'Producto creado');
      queryClient.invalidateQueries({ queryKey: ['recetas'] });
      navigate('/productos');
      invalidar();
    },
    onError: (error) => toast.error('No se pudo guardar', { description: mensajeDeError(error) }),
  });

  const eliminar = useMutation({
    mutationFn: (id: string) => catalogoApi.eliminar(id),
    onSuccess: () => {
      toast.success('Producto eliminado');
      setPorEliminar(null);
      setExpandido(null);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo eliminar', { description: mensajeDeError(error) }),
  });

  const cerrarFormulario = () => navigate('/productos');

  if (esEditor) {
    if (esEditar && (detalle.isLoading || recetasPorVariante.isLoading)) {
      return (
        <div className="absolute inset-0 flex items-center justify-center">
          <Skeleton className="h-64 w-full max-w-2xl" />
        </div>
      );
    }
    if (esEditar && !detalle.data) {
      return (
        <div className="absolute inset-0 flex flex-col items-center justify-center gap-4 text-center">
          <p className="text-sm text-muted-foreground">Este producto no existe o fue eliminado.</p>
          <Button variant="ghost" onClick={() => navigate('/productos')}>
            Volver a productos
          </Button>
        </div>
      );
    }
    return (
      <div className="absolute inset-0 flex min-h-0 flex-col overflow-hidden p-3 lg:p-4">
        <FormularioProducto
          producto={editando}
          categorias={categorias.data ?? []}
          marcas={marcas.data ?? []}
          unidades={unidades.data ?? []}
          recetasPorVariante={recetasPorVariante.data ?? {}}
          guardando={guardar.isPending}
          onCancelar={cerrarFormulario}
          onGuardar={(datos) => guardar.mutate(datos)}
        />
      </div>
    );
  }

  return (
    <div className="mx-auto flex max-w-page flex-col gap-4">
        <Card>
          <CardHeader className="flex flex-col items-stretch gap-3">
            <div className="flex flex-wrap items-center gap-2">
              <Buscador
                placeholder="Buscar nombre o SKU…"
                value={busqueda}
                onCambio={(valor) => {
                  setBusqueda(valor);
                  setPage(1);
                }}
              />
              <FiltroDropdown
                label="Tipo"
                opciones={opcionesTipo}
                valor={tipo}
                onChange={(valor) => {
                  setTipo(valor);
                  setPage(1);
                }}
              />
              <div className="ml-auto">
                <Can permiso="catalog:write">
                  <Button leftIcon={<Plus size={15} />} onClick={() => navigate('/productos/nuevo')}>
                    Nuevo producto
                  </Button>
                </Can>
              </div>
            </div>
            <div className="border-b border-border" />
            <div className="flex flex-wrap items-center gap-2">
              <FiltroDropdown
                label="Categoría"
                opciones={(categorias.data ?? []).map((c) => ({ valor: c.id, etiqueta: c.nombre }))}
                valor={categoria}
                onChange={(valor) => {
                  setCategoria(valor);
                  setPage(1);
                }}
              />
              <FiltroDropdown
                label="Marca"
                opciones={(marcas.data ?? []).map((m) => ({ valor: m.nombre, etiqueta: m.nombre }))}
                valor={marca}
                onChange={(valor) => {
                  setMarca(valor);
                  setPage(1);
                }}
              />
              <FiltroDropdown
                label="Estado"
                opciones={[
                  { valor: 'activos', etiqueta: 'Activos' },
                  { valor: 'inactivos', etiqueta: 'Inactivos' },
                ]}
                valor={soloActivos}
                onChange={(valor) => {
                  setSoloActivos(valor);
                  setPage(1);
                }}
              />
              <FiltroRango
                label="Precio USD"
                minimo={precioMin}
                maximo={precioMax}
                onMinimo={(valor) => {
                  setPrecioMin(valor);
                  setPage(1);
                }}
                onMaximo={(valor) => {
                  setPrecioMax(valor);
                  setPage(1);
                }}
              />
              <div className="ml-auto">
                <LimpiarFiltros activo={hayFiltros} onClick={limpiarFiltros} />
              </div>
            </div>
          </CardHeader>
              <CardBody>
                <LayoutGroup>
                  <DataTable<Producto>
                    rows={filas}
                    loading={productos.isLoading}
                    rowKey={(producto) => producto.id}
                    empty="No hay productos."
                    onRowClick={(producto) => setExpandido((actual) => (actual?.id === producto.id ? null : producto))}
                    expandirKey={expandido?.id ?? null}
                    expandedRow={
                      expandido
                        ? (producto) =>
                            producto.id === expandido.id ? (
                              <PanelDetalleProducto
                                producto={producto}
                                imagenId={imagenIdDe(producto)}
                                onCerrar={() => setExpandido(null)}
                              />
                            ) : null
                        : undefined
                    }
                    columns={[
                      {
                        key: 'nombre',
                        header: 'Producto',
                        render: (producto) => (
                          <div className="flex items-center gap-3">
                            <ImagenMiniatura producto={producto} />
                            <div>
                              <p className="text-foreground">{producto.nombre}</p>
                              <p className="text-xs text-muted-foreground">{producto.categoriaNombre}</p>
                            </div>
                          </div>
                        ),
                      },
                      { key: 'tipo', header: 'Tipo', render: (producto) => <Pill>{producto.tipo}</Pill> },
                      { key: 'area', header: 'Área', render: (producto) => <Pill tone={producto.areaDestino === 'Barra' ? 'accent' : 'info'}>{producto.areaDestino}</Pill> },
                      { key: 'marca', header: 'Marca', render: (producto) => producto.marcaNombre ?? '—' },
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
                        key: 'stock',
                        header: 'Stock',
                        align: 'right',
                        render: (producto) => formatNumber(producto.variantes.reduce((suma, v) => suma + v.cantidad, 0)),
                      },
                      {
                        key: 'reservado',
                        header: 'Reservado',
                        align: 'right',
                        render: (producto) => formatNumber(producto.variantes.reduce((suma, v) => suma + v.cantidadReservada, 0)),
                      },
                      {
                        key: 'estado',
                        header: 'Estado',
                        render: (producto) => (producto.activo ? <Pill tone="success">Activo</Pill> : <Pill tone="neutral">Inactivo</Pill>),
                      },
                      {
                        key: 'acciones',
                        header: '',
                        align: 'right',
                        className: 'w-12 pr-1',
                        render: (producto) => (
                          <div onClick={(evento) => evento.stopPropagation()} className="flex justify-end">
                            <ActionMenu
                              label={`Acciones de ${producto.nombre}`}
                              options={[
                                {
                                  label: 'Editar',
                                  icon: <Pencil size={15} />,
                                  onClick: () => navigate(`/productos/${producto.id}/editar`),
                                },
                                {
                                  label: 'Eliminar',
                                  icon: <Trash2 size={15} />,
                                  danger: true,
                                  onClick: () => setPorEliminar(producto),
                                },
                              ]}
                            />
                          </div>
                        ),
                      },
                    ]}
                  />
                </LayoutGroup>
                <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
              </CardBody>
        </Card>

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
