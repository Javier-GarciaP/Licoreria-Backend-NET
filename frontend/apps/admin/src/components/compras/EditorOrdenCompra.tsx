import { useEffect, useMemo, useRef, useState } from 'react';
import { useFieldArray, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { ArrowLeft, Package, Save, Search, Trash2 } from 'lucide-react';
import {
  Button,
  Card,
  CardBody,
  CardHeader,
  CardTitle,
  Command,
  CommandEmpty,
  CommandInput,
  CommandItem,
  CommandList,
  Input,
  Select,
  UiEmpty,
} from '@licoreria/ui';
import type { Producto } from '@licoreria/types';
import { catalogoApi, comprasApi, proveedoresApi } from '@licoreria/api-client';
import { mensajeDeError } from '../../lib/api';
import { formatNumber, formatUSD } from '../../lib/format';
import { urlDeImagen } from '../../lib/imagenProducto';

const esquemaDetalle = z.object({
  varianteId: z.string().min(1, 'Selecciona la variante'),
  cantidad: z.coerce.number({ invalid_type_error: 'Cantidad inválida' }).positive('Debe ser mayor que 0'),
  costoUnitarioUSD: z.coerce.number({ invalid_type_error: 'Costo inválido' }).min(0, 'No puede ser negativo'),
});

const esquemaOrden = z.object({
  proveedorId: z.string().min(1, 'Selecciona el proveedor'),
  observaciones: z.string().optional(),
  detalles: z.array(esquemaDetalle).min(1, 'Agrega al menos una línea'),
});

type FormularioOrden = z.infer<typeof esquemaOrden>;

interface ItemOrdenable {
  varianteId: string;
  productoNombre: string;
  varianteNombre: string;
  sku: string;
  unidad: string;
  imagenUrl: string | null;
  precioCompraUSD: number;
  cantidad: number;
  cantidadReservada: number;
  stockMinimo: number;
  stockMaximo: number;
  sugerido: number;
}

/** Solo se ordenan presentaciones base de productos de Barra. */
function aItemsOrdenables(producto: Producto): ItemOrdenable[] {
  if (producto.areaDestino !== 'Barra') return [];
  return producto.variantes
    .filter((v) => v.esBase)
    .map((variante) => ({
      varianteId: variante.id,
      productoNombre: producto.nombre,
      varianteNombre: variante.nombre,
      sku: variante.sku,
      unidad: variante.unidadMedidaNombre,
      imagenUrl: producto.imagenUrl,
      precioCompraUSD: variante.precioCompraUSD,
      cantidad: variante.cantidad,
      cantidadReservada: variante.cantidadReservada,
      stockMinimo: variante.stockMinimo,
      stockMaximo: variante.stockMaximo,
      sugerido: variante.stockMaximo > 0 ? Math.max(0, variante.stockMaximo - variante.cantidad) : 0,
    }));
}

function MiniaturaProducto({ imagenUrl, nombre }: { imagenUrl: string | null; nombre: string }) {
  const imagen = urlDeImagen(imagenUrl);
  if (imagen) {
    return <img src={imagen} alt={nombre} className="h-10 w-10 shrink-0 rounded-md object-cover" />;
  }
  return (
    <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-md bg-muted/40 text-muted-foreground">
      <Package size={16} />
    </span>
  );
}

/** Etiqueta solo visible en móvil (en escritorio la nombra la cabecera de la tabla). */
function EtiquetaMovil({ children }: { children: string }) {
  return (
    <span className="text-xs font-medium text-muted-foreground lg:hidden">{children}</span>
  );
}

/** Panel dedicado de nueva orden de compra (estilo editor de producto/plano). */
export function EditorOrdenCompra() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [busqueda, setBusqueda] = useState('');
  const [busquedaDebounced, setBusquedaDebounced] = useState('');
  const [buscadorAbierto, setBuscadorAbierto] = useState(false);
  const [indiceAEnfocar, setIndiceAEnfocar] = useState<number | null>(null);
  const busquedaRef = useRef<HTMLInputElement | null>(null);

  useEffect(() => {
    const temporizador = setTimeout(() => setBusquedaDebounced(busqueda.trim()), 250);
    return () => clearTimeout(temporizador);
  }, [busqueda]);

  const proveedores = useQuery({ queryKey: ['proveedores'], queryFn: () => proveedoresApi.listar() });
  // Se busca en el servidor (el listado está paginado a 100).
  const productos = useQuery({
    queryKey: ['productos', 'ordenables', busquedaDebounced],
    queryFn: () =>
      catalogoApi.productos({ page: 1, pageSize: 100, busqueda: busquedaDebounced || undefined, activo: true }),
  });

  const ordenForm = useForm<FormularioOrden>({
    resolver: zodResolver(esquemaOrden),
    defaultValues: { proveedorId: '', observaciones: '', detalles: [] },
  });
  const { fields, append, remove } = useFieldArray({ control: ordenForm.control, name: 'detalles' });

  // Al agregar una línea (Enter o clic en el buscador), el foco pasa a su cantidad.
  useEffect(() => {
    if (indiceAEnfocar === null) return;
    const input = document.querySelector<HTMLInputElement>(`input[name="detalles.${indiceAEnfocar}.cantidad"]`);
    if (input) {
      input.focus();
      input.select();
      setIndiceAEnfocar(null);
    }
  }, [indiceAEnfocar, fields.length]);

  const lista = useMemo(() => productos.data?.items ?? [], [productos.data]);
  // Solo se ordenan presentaciones base de productos de Barra activos.
  const todos = useMemo(() => lista.flatMap(aItemsOrdenables), [lista]);
  const porVariante = useMemo(() => new Map(todos.map((item) => [item.varianteId, item])), [todos]);
  const catalogo = useMemo(() => {
    const filtro = busquedaDebounced.trim().toLowerCase();
    return filtro
      ? todos.filter((item) =>
          `${item.productoNombre} ${item.varianteNombre} ${item.sku}`.toLowerCase().includes(filtro),
        )
      : [];
  }, [todos, busquedaDebounced]);

  const total = useMemo(
    () =>
      fields.reduce(
        (suma, linea) => suma + Number(linea.cantidad || 0) * Number(linea.costoUnitarioUSD || 0),
        0,
      ),
    [fields],
  );

  const guardar = useMutation({
    mutationFn: (datos: FormularioOrden) => comprasApi.crear(datos),
    onSuccess: () => {
      toast.success('Orden creada');
      queryClient.invalidateQueries({ queryKey: ['ordenes'] });
      navigate('/compras');
    },
    onError: (error) => toast.error('No se pudo crear la orden', { description: mensajeDeError(error) }),
  });

  const agregar = (item: ItemOrdenable) => {
    if (fields.some((linea) => linea.varianteId === item.varianteId)) {
      toast.info(`${item.productoNombre} ya está en la orden`);
      return;
    }
    append({
      varianteId: item.varianteId,
      cantidad: Math.max(1, item.sugerido || 1),
      costoUnitarioUSD: item.precioCompraUSD,
    });
    toast.success(`${item.productoNombre} agregado`);
    setBusqueda('');
    setBuscadorAbierto(false);
    setIndiceAEnfocar(fields.length);
  };

  /** Flujo rápido: Enter avanza de la cantidad al costo y del costo al buscador. */
  const avanzarConEnter = (evento: React.KeyboardEvent<HTMLFormElement>) => {
    if (evento.key !== 'Enter' || evento.shiftKey) return;
    const objetivo = evento.target as HTMLInputElement;
    if (objetivo.tagName !== 'INPUT') return;
    const fila = objetivo.closest<HTMLElement>('[data-fila-linea]');
    if (!fila) return;
    evento.preventDefault();
    if (objetivo.name.endsWith('.cantidad')) {
      fila.querySelector<HTMLInputElement>('input[name$=".costoUnitarioUSD"]')?.focus();
    } else if (objetivo.name.endsWith('.costoUnitarioUSD')) {
      busquedaRef.current?.focus();
    }
  };

  const columnas = 'grid-cols-1 lg:grid-cols-[minmax(0,1fr)_6rem_7rem_6rem_6rem]';

  return (
    <div className="absolute inset-0 flex min-h-0 flex-col overflow-y-auto p-3 lg:overflow-hidden lg:p-4">
      <form
        className="flex min-h-0 flex-1 flex-col gap-4"
        onSubmit={ordenForm.handleSubmit((datos) => guardar.mutate(datos))}
        onKeyDown={avanzarConEnter}
        noValidate
      >
        <div className="flex flex-wrap items-center gap-2 border-b border-border pb-2">
          <Button
            variant="ghost"
            size="sm"
            type="button"
            onClick={() => navigate('/compras')}
            aria-label="Volver"
          >
            <ArrowLeft size={16} />
          </Button>
          <div>
            <p className="text-base font-medium tracking-tighter2 text-foreground">Nueva orden de compra</p>
            <p className="text-xs text-muted-foreground">
              Elige los productos de barra que repondrás a tu proveedor.
            </p>
          </div>
        </div>

        <div className="flex min-h-0 flex-1 flex-col gap-4 lg:grid lg:grid-cols-[minmax(0,1fr)_320px]">
          <Card className="flex min-h-0 flex-col">
            <CardHeader className="flex flex-col items-stretch gap-3 px-4 pt-4 lg:px-5 lg:pt-5">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <CardTitle className="text-base">Líneas de la orden</CardTitle>
                {fields.length > 0 && (
                  <span className="num text-xs text-muted-foreground">
                    {fields.length} {fields.length === 1 ? 'línea' : 'líneas'}
                  </span>
                )}
              </div>

              <div
                className="relative"
                onFocus={() => setBuscadorAbierto(true)}
                onBlur={(evento) => {
                  if (!evento.currentTarget.contains(evento.relatedTarget)) setBuscadorAbierto(false);
                }}
              >
                <Command shouldFilter={false}>
                  <span className="relative">
                    <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                    <CommandInput
                      ref={busquedaRef}
                      value={busqueda}
                      onValueChange={setBusqueda}
                      placeholder="Buscar producto por nombre, variante o SKU…"
                      aria-label="Buscar producto"
                      className="h-9 w-full rounded-md border border-input bg-background pl-9 pr-3 text-sm shadow-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2"
                    />
                  </span>

                  {buscadorAbierto && busqueda.trim().length > 0 && (
                    <CommandList className="app-scroll absolute left-0 right-0 z-30 mt-1 max-h-72 overflow-y-auto rounded-md border border-border bg-card p-1 shadow-lg">
                      {catalogo.length === 0 ? (
                        <CommandEmpty className="px-3 py-6 text-center text-sm text-muted-foreground">
                          {productos.isLoading ? 'Cargando…' : 'Sin resultados. Prueba con otro término.'}
                        </CommandEmpty>
                      ) : (
                        catalogo.map((item) => {
                          const imagen = urlDeImagen(item.imagenUrl);
                          return (
                            <CommandItem
                              key={item.varianteId}
                              value={`${item.productoNombre} ${item.varianteNombre} ${item.sku}`}
                              onSelect={() => agregar(item)}
                              className="flex w-full items-center gap-2.5 rounded-md px-2 py-2 text-left text-sm transition-colors data-[selected=true]:bg-primary/15 focus-visible:outline-none"
                            >
                              {imagen ? (
                                <img src={imagen} alt="" className="h-8 w-8 shrink-0 rounded-md object-cover" />
                              ) : (
                                <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-md bg-muted/40 text-muted-foreground">
                                  <Package size={14} />
                                </span>
                              )}
                              <span className="min-w-0 flex-1">
                                <span className="block truncate text-sm text-foreground">
                                  {item.productoNombre}
                                </span>
                                <span className="block truncate text-xs text-muted-foreground">
                                  {item.varianteNombre} <span className="num">· {item.sku}</span>
                                </span>
                              </span>
                              <span className="num text-right text-xs text-muted-foreground">
                                <span className="block">Disp {formatNumber(item.cantidad)}</span>
                                <span className="block text-foreground">Sug {formatNumber(item.sugerido)}</span>
                              </span>
                            </CommandItem>
                          );
                        })
                      )}
                    </CommandList>
                  )}
                </Command>
              </div>
            </CardHeader>

            <CardBody className="flex min-h-0 flex-1 flex-col gap-2 px-4 pb-4 pt-2 lg:px-5 lg:pb-5">
              {ordenForm.formState.errors.detalles?.message && (
                <p className="text-xs text-destructive-fg">{ordenForm.formState.errors.detalles.message}</p>
              )}

              {fields.length === 0 ? (
                <div className="flex flex-1 flex-col justify-center">
                  <UiEmpty
                    icon={<Package size={22} />}
                    title="Aún no hay líneas"
                    description="Busca un producto arriba y haz clic para agregarlo a la orden. Se sugiere la cantidad según el stock máximo."
                    className="py-10"
                  />
                </div>
              ) : (
                <div className="flex min-h-0 flex-1 flex-col">
                  <div className="app-scroll min-h-0 flex-1 overflow-y-auto lg:pr-1">
                    <div className="flex flex-col">
                      <div
                        className={`sticky top-0 z-10 hidden gap-2 border-b border-border bg-card pb-2 lg:grid ${columnas}`}
                      >
                        <span className="px-2 text-xs font-medium text-muted-foreground">Producto</span>
                        <span className="text-xs font-medium text-muted-foreground">Cantidad</span>
                        <span className="text-xs font-medium text-muted-foreground">Costo USD</span>
                        <span className="text-right text-xs font-medium text-muted-foreground">Subtotal</span>
                        <span />
                      </div>
                      {fields.map((field, indice) => {
                        const item = porVariante.get(field.varianteId);
                        const subtotal = Number(field.cantidad || 0) * Number(field.costoUnitarioUSD || 0);
                        return (
                          <div
                            key={field.id}
                            data-fila-linea
                            className={`grid ${columnas} gap-x-2 gap-y-1.5 border-b border-border py-2.5 transition-colors last:border-b-0 hover:bg-muted/50 lg:items-center`}
                          >
                            <div className="col-span-2 flex min-w-0 items-center gap-2.5 lg:col-span-1 lg:px-2">
                              {item && (
                                <MiniaturaProducto imagenUrl={item.imagenUrl} nombre={item.productoNombre} />
                              )}
                              <div className="min-w-0">
                                <p className="truncate text-sm font-medium text-foreground">
                                  {item?.productoNombre ?? 'Producto'}
                                </p>
                                <p className="truncate text-xs text-muted-foreground">
                                  {item?.varianteNombre ?? ''} <span className="num">{item?.sku ?? ''}</span>
                                  <span className="num"> · {item?.unidad ?? ''}</span>
                                </p>
                              </div>
                            </div>
                            <div className="flex flex-col gap-1.5">
                              <EtiquetaMovil>Cantidad</EtiquetaMovil>
                              <Input
                                type="number"
                                min={1}
                                placeholder="Cant."
                                className="num"
                                {...ordenForm.register(`detalles.${indice}.cantidad`)}
                              />
                            </div>
                            <div className="flex flex-col gap-1.5">
                              <EtiquetaMovil>Costo USD</EtiquetaMovil>
                              <Input
                                type="number"
                                min={0}
                                step="0.01"
                                placeholder="0.00"
                                className="num"
                                {...ordenForm.register(`detalles.${indice}.costoUnitarioUSD`)}
                              />
                            </div>
                            <div className="flex items-baseline justify-between lg:block lg:text-right">
                              <span className="text-xs text-muted-foreground lg:hidden">Subtotal</span>
                              <span className="num text-sm font-medium text-foreground">
                                {formatUSD(subtotal)}
                              </span>
                            </div>
                            <div className="flex items-center justify-end lg:justify-center">
                              <Button
                                type="button"
                                variant="ghost"
                                size="sm"
                                leftIcon={<Trash2 size={13} />}
                                onClick={() => remove(indice)}
                                className="text-muted-foreground hover:text-destructive-fg"
                              >
                                Quitar
                              </Button>
                            </div>
                          </div>
                        );
                      })}
                    </div>
                  </div>
                </div>
              )}
            </CardBody>
          </Card>

          <Card className="flex min-h-0 flex-col gap-4 p-5">
            <div>
              <p className="text-sm font-medium tracking-tighter2 text-foreground">Resumen de la orden</p>
              <p className="text-xs text-muted-foreground">Se guarda como Borrador hasta aprobarla.</p>
            </div>

            <Select
              label="Proveedor"
              error={ordenForm.formState.errors.proveedorId?.message}
              {...ordenForm.register('proveedorId')}
            >
              <option value="">Selecciona…</option>
              {proveedores.data?.map((proveedor) => (
                <option key={proveedor.id} value={proveedor.id}>
                  {proveedor.nombre}
                </option>
              ))}
            </Select>

            <Input label="Observaciones" {...ordenForm.register('observaciones')} />

            <div className="mt-auto flex flex-col gap-3 border-t border-border pt-3">
              <div className="flex items-baseline justify-between gap-3">
                <span className="text-sm text-muted-foreground">Líneas</span>
                <span className="num text-sm text-foreground">{fields.length}</span>
              </div>
              <div className="flex items-baseline justify-between gap-3">
                <span className="text-sm text-muted-foreground">Total</span>
                <span className="num text-lg font-medium text-foreground">{formatUSD(total)}</span>
              </div>
              <Button type="submit" className="w-full" leftIcon={<Save size={15} />} loading={guardar.isPending}>
                Crear orden
              </Button>
            </div>
          </Card>
        </div>
      </form>
    </div>
  );
}