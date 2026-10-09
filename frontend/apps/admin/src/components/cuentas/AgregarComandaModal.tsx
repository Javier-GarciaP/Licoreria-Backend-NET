import { useEffect, useRef, useState } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import { toast } from 'sonner';
import { ArrowLeft, ArrowRight, Minus, Package, Plus, Sparkles, UtensilsCrossed, Wine, X } from 'lucide-react';
import { Button, cn, Input, Modal, Skeleton } from '@licoreria/ui';
import type { AreaDestino, Producto, ProductoVariante } from '@licoreria/types';
import { catalogoApi, cuentasApi } from '@licoreria/api-client';
import { mensajeDeError } from '../../lib/api';
import { urlDeImagen } from '../../lib/imagenProducto';
import { formatUSD } from '../../lib/format';

const AREAS: AreaDestino[] = ['Barra', 'Cocina'];
const OPCIONES_AREA: ('Auto' | AreaDestino)[] = ['Auto', 'Barra', 'Cocina'];

interface ItemPendiente {
  varianteId: string;
  nombre: string;
  precioUSD: number;
  cantidad: number;
  area: AreaDestino;
}

const rangoPrecio = (producto: Producto) => {
  const precios = producto.variantes.map((variante) => variante.precioVentaUSD).filter((valor) => valor > 0);
  if (precios.length === 0) return '—';
  const minimo = Math.min(...precios);
  const maximo = Math.max(...precios);
  return minimo === maximo ? formatUSD(minimo) : `${formatUSD(minimo)} – ${formatUSD(maximo)}`;
};

function StepperBtn({
  onClick,
  label,
  children,
}: {
  onClick: () => void;
  label: string;
  children: React.ReactNode;
}) {
  return (
    <button
      type="button"
      aria-label={label}
      title={label}
      onClick={onClick}
      className="flex h-6 w-6 items-center justify-center rounded-full text-muted-foreground transition hover:bg-muted hover:text-foreground"
    >
      {children}
    </button>
  );
}

/** Picker de productos para agregar comandas: cada ítem queda en su área (Barra/Cocina) y se envían comandas separadas por área. */
export function AgregarComandaModal({
  cuentaId,
  abierto,
  onCerrar,
  onEnviado,
}: {
  cuentaId: string;
  abierto: boolean;
  onCerrar: () => void;
  onEnviado: () => void;
}) {
  const [busqueda, setBusqueda] = useState('');
  const [categoriaId, setCategoriaId] = useState<string | null>(null);
  const [areaActual, setAreaActual] = useState<'Auto' | AreaDestino>('Auto');
  const [pendientes, setPendientes] = useState<ItemPendiente[]>([]);
  const [varianteDe, setVarianteDe] = useState<Producto | null>(null);
  const busquedaRef = useRef<HTMLInputElement>(null);

  const productos = useQuery({
    queryKey: ['cuenta', 'productos', busqueda, categoriaId],
    queryFn: () => catalogoApi.productos({ busqueda, categoriaId: categoriaId ?? undefined, pageSize: 50, activo: true }),
    enabled: abierto,
  });
  const categorias = useQuery({ queryKey: ['cuenta', 'categorias'], queryFn: catalogoApi.categorias, enabled: abierto });

  useEffect(() => {
    if (!abierto) return;
    setBusqueda('');
    setCategoriaId(null);
    setPendientes([]);
    setAreaActual('Auto');
    setVarianteDe(null);
    const temporizador = setTimeout(() => busquedaRef.current?.focus(), 60);
    return () => clearTimeout(temporizador);
  }, [abierto]);

  const agregarNuevo = (producto: Producto, variante: ProductoVariante) => {
    const area = areaActual === 'Auto' ? (producto.areaDestino ?? 'Barra') : areaActual;
    setPendientes((actuales) => {
      const existente = actuales.find((item) => item.varianteId === variante.id);
      if (existente) {
        return actuales.map((item) => (item.varianteId === variante.id ? { ...item, cantidad: item.cantidad + 1 } : item));
      }
      return [...actuales, { varianteId: variante.id, nombre: producto.nombre, precioUSD: variante.precioVentaUSD, cantidad: 1, area }];
    });
  };

  const seleccionarProducto = (producto: Producto) => {
    if (producto.variantes.length === 1) {
      agregarNuevo(producto, producto.variantes[0]);
      return;
    }
    setVarianteDe(producto);
  };

  const seleccionarVariante = (variante: ProductoVariante) => {
    if (varianteDe) agregarNuevo(varianteDe, variante);
    setVarianteDe(null);
  };

  const cambiarCantidad = (varianteId: string, delta: number) =>
    setPendientes((actuales) =>
      actuales
        .map((item) => (item.varianteId === varianteId ? { ...item, cantidad: item.cantidad + delta } : item))
        .filter((item) => item.cantidad > 0),
    );

  const moverArea = (varianteId: string) =>
    setPendientes((actuales) =>
      actuales.map((item) =>
        item.varianteId === varianteId ? { ...item, area: item.area === 'Barra' ? 'Cocina' : 'Barra' } : item,
      ),
    );

  const quitar = (varianteId: string) => setPendientes((actuales) => actuales.filter((item) => item.varianteId !== varianteId));

  const barra = pendientes.filter((item) => item.area === 'Barra');
  const cocina = pendientes.filter((item) => item.area === 'Cocina');
  const resumen = [
    barra.length > 0 ? `${barra.length} Barra` : null,
    cocina.length > 0 ? `${cocina.length} Cocina` : null,
  ]
    .filter(Boolean)
    .join(' · ');

  const enviar = useMutation({
    mutationFn: async () => {
      const agrupados: Record<AreaDestino, ItemPendiente[]> = { Barra: barra, Cocina: cocina };
      const llamadas = AREAS.filter((area) => agrupados[area].length > 0).map((area) =>
        cuentasApi.agregarComanda(cuentaId, {
          area,
          items: agrupados[area].map((item) => ({ varianteId: item.varianteId, cantidad: item.cantidad })),
        }),
      );
      await Promise.all(llamadas);
    },
    onSuccess: () => {
      toast.success('Comandas enviadas');
      onEnviado();
      onCerrar();
    },
    onError: (error) => toast.error('No se pudo enviar la comanda', { description: mensajeDeError(error) }),
  });

  return (
    <>
      <Modal
        open={abierto}
        onClose={onCerrar}
        title="Agregar a la comanda"
        size="xl"
        className="max-w-5xl"
        footer={
          <>
            <Button variant="ghost" onClick={onCerrar}>
              Cancelar
            </Button>
            <Button disabled={pendientes.length === 0} loading={enviar.isPending} onClick={() => enviar.mutate()}>
              Enviar comandas{resumen ? ` · ${resumen}` : ''}
            </Button>
          </>
        }
      >
        <div className="flex flex-col gap-5 lg:flex-row">
          <div className="flex min-w-0 flex-1 flex-col gap-3">
            <Input ref={busquedaRef} placeholder="Buscar producto…" value={busqueda} onChange={(evento) => setBusqueda(evento.target.value)} />

            <div className="app-scroll -mx-1 flex gap-1.5 overflow-x-auto px-1 pb-1">
              <button
                type="button"
                onClick={() => setCategoriaId(null)}
                className={cn(
                  'flex h-7 shrink-0 items-center whitespace-nowrap rounded-full border px-2.5 text-[11px] transition',
                  categoriaId === null ? 'border-primary bg-primary/15 text-foreground' : 'border-border text-muted-foreground hover:text-foreground',
                )}
              >
                Todas
              </button>
              {(categorias.data ?? []).map((categoria) => (
                <button
                  key={categoria.id}
                  type="button"
                  onClick={() => setCategoriaId(categoria.id)}
                  className={cn(
                    'flex h-7 shrink-0 items-center whitespace-nowrap rounded-full border px-2.5 text-[11px] transition',
                    categoriaId === categoria.id ? 'border-primary bg-primary/15 text-foreground' : 'border-border text-muted-foreground hover:text-foreground',
                  )}
                >
                  {categoria.nombre}
                </button>
              ))}
            </div>

            <div className="flex gap-2">
              {OPCIONES_AREA.map((valor) => (
                <button
                  key={valor}
                  type="button"
                  onClick={() => setAreaActual(valor)}
                  className={cn(
                    'flex h-9 flex-1 items-center justify-center gap-1.5 rounded-full border px-3 text-sm transition',
                    areaActual === valor ? 'border-primary bg-primary/15 text-foreground' : 'border-border text-muted-foreground hover:text-foreground',
                  )}
                >
                  {valor === 'Barra' ? <Wine size={14} /> : valor === 'Cocina' ? <UtensilsCrossed size={14} /> : <Sparkles size={14} />}
                  {valor === 'Auto' ? 'Auto' : valor}
                </button>
              ))}
            </div>
            <p className="-mt-1 text-[11px] text-muted-foreground">
              {areaActual === 'Auto' ? (
                <>Cada producto irá al área que tiene registrada (Barra/Cocina).</>
              ) : (
                <>
                  Lo que agregues irá a <span className="font-medium text-foreground">{areaActual}</span>.
                </>
              )}
            </p>

            <div className="grid max-h-[26rem] grid-cols-2 gap-2 overflow-y-auto sm:grid-cols-3 2xl:grid-cols-5">
              {productos.isLoading ? (
                Array.from({ length: 6 }).map((_, indice) => <Skeleton key={indice} className="h-32 w-full" />)
              ) : (productos.data?.items ?? []).length === 0 ? (
                <p className="col-span-full py-8 text-center text-sm text-muted-foreground">Sin productos para agregar.</p>
              ) : (
                (productos.data?.items ?? []).map((producto) => {
                  const imagen = urlDeImagen(producto.imagenUrl);
                  return (
                    <button
                      key={producto.id}
                      type="button"
                      onClick={() => seleccionarProducto(producto)}
                      className="flex flex-col overflow-hidden rounded-xl border border-border text-left transition hover:border-primary/50"
                    >
                      {imagen ? (
                        <img src={imagen} alt={producto.nombre} className="aspect-[4/3] w-full object-cover" />
                      ) : (
                        <div className="flex aspect-[4/3] w-full items-center justify-center bg-muted/40">
                          <Package size={20} className="text-muted-foreground" />
                        </div>
                      )}
<div className="flex flex-col gap-0.5 p-2.5">
                      <span className="line-clamp-2 text-sm text-foreground">{producto.nombre}</span>
                      <span className="flex items-center justify-between gap-1">
                        <span className="num text-[11px] text-muted-foreground">{rangoPrecio(producto)}</span>
                        <span
                          className={cn(
                            'inline-flex shrink-0 items-center gap-1 text-[10px] font-medium',
                            producto.areaDestino === 'Cocina' ? 'text-info-fg' : 'text-foreground',
                          )}
                        >
                          {producto.areaDestino === 'Cocina' ? <UtensilsCrossed size={11} /> : <Wine size={11} />}
                          {producto.areaDestino ?? 'Barra'}
                        </span>
                      </span>
                    </div>
                    </button>
                  );
                })
              )}
            </div>
          </div>

          <div className="flex w-full shrink-0 flex-col gap-3 lg:w-80">
            {pendientes.length === 0 ? (
              <div className="flex min-h-40 flex-col items-center justify-center gap-2 rounded-xl border border-dashed border-border px-4 py-8 text-center">
                <p className="text-sm font-medium text-foreground">Comanda vacía</p>
                <p className="text-xs text-muted-foreground">Selecciona productos a la izquierda; cada uno queda en su área (Barra/Cocina).</p>
              </div>
            ) : (
              AREAS.map((area) => {
                const items = area === 'Barra' ? barra : cocina;
                if (items.length === 0) return null;
                return (
                  <div key={area} className="rounded-xl border border-border p-3">
                    <div className="mb-2 flex items-center justify-between">
                      <span className="inline-flex items-center gap-1.5 text-xs font-medium text-foreground">
                        {area === 'Barra' ? <Wine size={13} /> : <UtensilsCrossed size={13} />}
                        {area}
                      </span>
                      <span className="num text-xs text-muted-foreground">
                        {formatUSD(items.reduce((suma, item) => suma + item.precioUSD * item.cantidad, 0))}
                      </span>
                    </div>
                    <div className="flex flex-col gap-1">
                      {items.map((item) => (
                        <div key={item.varianteId} className="flex items-center justify-between gap-2 rounded-lg bg-muted/40 px-2.5 py-1.5">
                          <div className="min-w-0">
                            <p className="truncate text-sm text-foreground">
                              {item.cantidad} × {item.nombre}
                            </p>
                            <p className="num text-[11px] text-muted-foreground">{formatUSD(item.precioUSD * item.cantidad)}</p>
                          </div>
                          <div className="flex items-center gap-1">
                            <StepperBtn onClick={() => cambiarCantidad(item.varianteId, -1)} label="Quitar uno">
                              <Minus size={12} />
                            </StepperBtn>
                            <StepperBtn onClick={() => cambiarCantidad(item.varianteId, 1)} label="Añadir uno">
                              <Plus size={12} />
                            </StepperBtn>
                            <StepperBtn onClick={() => moverArea(item.varianteId)} label={`Mover a ${item.area === 'Barra' ? 'Cocina' : 'Barra'}`}>
                              {item.area === 'Barra' ? <ArrowRight size={12} /> : <ArrowLeft size={12} />}
                            </StepperBtn>
                            <StepperBtn onClick={() => quitar(item.varianteId)} label="Quitar">
                              <X size={12} />
                            </StepperBtn>
                          </div>
                        </div>
                      ))}
                    </div>
                  </div>
                );
              })
            )}
          </div>
        </div>
      </Modal>

      <Modal
        open={Boolean(varianteDe)}
        onClose={() => setVarianteDe(null)}
        title={varianteDe?.nombre ?? 'Seleccionar variante'}
        size="sm"
        footer={
          <Button variant="ghost" onClick={() => setVarianteDe(null)}>
            Cancelar
          </Button>
        }
      >
        <div className="flex flex-col gap-2">
          {varianteDe?.variantes.map((variante) => (
            <button
              key={variante.id}
              type="button"
              onClick={() => seleccionarVariante(variante)}
              className="flex items-center justify-between rounded-xl border border-border px-3 py-2 text-left text-sm transition hover:border-primary/50"
            >
              <span className="text-foreground">{variante.nombre}</span>
              <span className="num text-muted-foreground">{formatUSD(variante.precioVentaUSD)}</span>
            </button>
          ))}
        </div>
      </Modal>
    </>
  );
}