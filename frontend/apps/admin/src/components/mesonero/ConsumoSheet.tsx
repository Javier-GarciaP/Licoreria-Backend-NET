import { useEffect, useMemo, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import { useQuery } from '@tanstack/react-query';
import { Minus, Plus, Search, Trash2, X } from 'lucide-react';
import { Button, cn } from '@licoreria/ui';
import type { Producto, ProductoVariante } from '@licoreria/types';
import { catalogoApi } from '@licoreria/api-client';
import { formatUSD } from '../../lib/format';
import { CategoriaBar } from '../pos/CategoriaBar';
import { ProductoGrid } from '../pos/ProductoGrid';
import { SelectorProductoModal, type SeleccionProducto } from '../pos/SelectorProductoModal';

export interface LineaComanda {
  key: string;
  varianteId: string;
  nombre: string;
  varianteNombre: string;
  sku: string;
  precioUSD: number;
  cantidad: number;
  esCortesia: boolean;
}

let secuencia = 0;
const nuevoKey = () => `lc-${Date.now().toString(36)}-${secuencia++}`;

export function ConsumoSheet({
  abierto,
  cuentaNombre,
  enviando,
  onCerrar,
  onEnviar,
}: {
  abierto: boolean;
  cuentaNombre: string;
  enviando: boolean;
  onCerrar: () => void;
  onEnviar: (area: 'Barra' | 'Cocina', items: { varianteId: string; cantidad: number; esCortesia: boolean }[]) => void;
}) {
  const [busqueda, setBusqueda] = useState('');
  const [busquedaDebounced, setBusquedaDebounced] = useState('');
  const [categoriaId, setCategoriaId] = useState<string | null>(null);
  const [area, setArea] = useState<'Barra' | 'Cocina'>('Barra');
  const [lineas, setLineas] = useState<LineaComanda[]>([]);
  const [productoModal, setProductoModal] = useState<Producto | null>(null);
  const busquedaRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (!abierto) return;
    setBusqueda('');
    setBusquedaDebounced('');
    setCategoriaId(null);
    setLineas([]);
    const previo = document.activeElement as HTMLElement | null;
    const alPresionar = (evento: KeyboardEvent) => {
      if (evento.key === 'Escape') onCerrar();
    };
    document.addEventListener('keydown', alPresionar);
    return () => {
      document.removeEventListener('keydown', alPresionar);
      previo?.focus?.();
    };
  }, [abierto, onCerrar]);

  useEffect(() => {
    const temporizador = setTimeout(() => setBusquedaDebounced(busqueda.trim()), 250);
    return () => clearTimeout(temporizador);
  }, [busqueda]);

  useEffect(() => {
    if (!abierto || productoModal) return;
    const alEscribir = (evento: KeyboardEvent) => {
      if (evento.ctrlKey || evento.metaKey || evento.altKey) return;
      const objetivo = evento.target as HTMLElement | null;
      if (objetivo && ['INPUT', 'TEXTAREA', 'SELECT'].includes(objetivo.tagName)) return;
      if (evento.key.length !== 1 || /[+\-\s]/.test(evento.key)) return;
      evento.preventDefault();
      busquedaRef.current?.focus();
      setBusqueda(evento.key);
    };
    window.addEventListener('keydown', alEscribir);
    return () => window.removeEventListener('keydown', alEscribir);
  }, [abierto, productoModal]);

  const categorias = useQuery({ queryKey: ['mesonero', 'categorias'], queryFn: catalogoApi.categorias, enabled: abierto });
  const productos = useQuery({
    queryKey: ['mesonero', 'productos', categoriaId, busquedaDebounced],
    queryFn: () =>
      catalogoApi.productos({ busqueda: busquedaDebounced, categoriaId: categoriaId ?? undefined, pageSize: 100, activo: true }),
    enabled: abierto,
  });

  const total = useMemo(
    () => lineas.reduce((acumulado, linea) => acumulado + linea.precioUSD * linea.cantidad, 0),
    [lineas],
  );

  const agregarVariante = (nombre: string, variante: ProductoVariante) =>
    setLineas((actuales) => {
      const existente = actuales.find((linea) => linea.varianteId === variante.id && !linea.esCortesia);
      if (existente) {
        return actuales.map((linea) => (linea.key === existente.key ? { ...linea, cantidad: linea.cantidad + 1 } : linea));
      }
      return [
        ...actuales,
        {
          key: nuevoKey(),
          varianteId: variante.id,
          nombre,
          varianteNombre: variante.nombre,
          sku: variante.sku,
          precioUSD: variante.precioVentaUSD,
          cantidad: 1,
          esCortesia: false,
        },
      ];
    });

  const seleccionarProducto = (producto: Producto) => {
    if (producto.variantes.length === 1 && producto.tipo !== 'Preparado') {
      agregarVariante(producto.nombre, producto.variantes[0]);
      return;
    }
    setProductoModal(producto);
  };

  const confirmarSeleccion = (seleccion: SeleccionProducto) => {
    if (!productoModal) return;
    setLineas((actuales) => [
      ...actuales,
      {
        key: nuevoKey(),
        varianteId: seleccion.variante.id,
        nombre: productoModal.nombre,
        varianteNombre: seleccion.variante.nombre,
        sku: seleccion.variante.sku,
        precioUSD: seleccion.variante.precioVentaUSD,
        cantidad: seleccion.cantidad,
        esCortesia: false,
      },
    ]);
    setProductoModal(null);
  };

  const cambiarCantidad = (key: string, delta: number) =>
    setLineas((actuales) =>
      actuales
        .map((linea) => (linea.key === key ? { ...linea, cantidad: linea.cantidad + delta } : linea))
        .filter((linea) => linea.cantidad > 0),
    );

  const alternarCortesia = (key: string) =>
    setLineas((actuales) => actuales.map((linea) => (linea.key === key ? { ...linea, esCortesia: !linea.esCortesia } : linea)));

  const quitar = (key: string) => setLineas((actuales) => actuales.filter((linea) => linea.key !== key));

  const enviar = () =>
    onEnviar(
      area,
      lineas.map((linea) => ({ varianteId: linea.varianteId, cantidad: linea.cantidad, esCortesia: linea.esCortesia })),
    );

  if (!abierto) return null;

  return createPortal(
    <div
      role="dialog"
      aria-modal="true"
      aria-label="Agregar consumo"
      className="fixed inset-0 z-50 flex flex-col bg-canvas/95 backdrop-blur-sm"
    >
      <header className="flex items-center justify-between gap-3 border-b border-hairline px-4 py-3">
        <div className="min-w-0">
          <p className="truncate text-base font-medium text-ink">Agregar consumo</p>
          <p className="truncate text-xs text-muted">{cuentaNombre}</p>
        </div>
        <div className="flex items-center gap-2">
          <div className="flex rounded-pill border border-hairline p-0.5">
            {(['Barra', 'Cocina'] as const).map((valor) => (
              <button
                key={valor}
                type="button"
                onClick={() => setArea(valor)}
                className={cn(
                  'rounded-pill px-4 py-1.5 text-sm transition',
                  area === valor ? 'bg-accent/20 font-medium text-accent-ink' : 'text-muted hover:text-ink',
                )}
              >
                {valor}
              </button>
            ))}
          </div>
          <button
            type="button"
            aria-label="Cerrar"
            onClick={onCerrar}
            className="flex h-10 w-10 items-center justify-center rounded-full border border-hairline text-muted transition hover:text-ink"
          >
            <X size={18} />
          </button>
        </div>
      </header>

      <div className="grid min-h-0 flex-1 grid-cols-1 lg:grid-cols-[minmax(0,1fr)_22rem]">
        <div className="flex min-h-0 flex-col gap-3 border-hairline p-4 lg:border-r">
          <CategoriaBar categorias={categorias.data ?? []} activa={categoriaId} onCambiar={setCategoriaId} />
          <label className="relative flex items-center">
            <Search size={16} className="absolute left-3 text-muted" />
            <input
              ref={busquedaRef}
              aria-label="Buscar producto"
              placeholder="Buscar por nombre o SKU…"
              value={busqueda}
              onChange={(evento) => setBusqueda(evento.target.value)}
              className="h-11 w-full rounded-control border border-hairline bg-surface pl-10 pr-4 text-sm text-ink focus:outline-none focus:ring-2 focus:ring-accent/50"
            />
          </label>
          <div className="app-scroll min-h-0 flex-1 overflow-y-auto pr-1">
            <ProductoGrid
              productos={productos.data?.items ?? []}
              cargando={productos.isLoading}
              onSeleccionar={seleccionarProducto}
            />
          </div>
        </div>

        <aside className="flex min-h-0 flex-col gap-3 p-4">
          <p className="text-xs font-medium uppercase tracking-tighter2 text-muted">Comanda · {area}</p>
          <ul className="app-scroll flex min-h-0 flex-1 flex-col gap-1.5 overflow-y-auto pr-1">
            {lineas.length === 0 && <li className="py-10 text-center text-sm text-muted">Agrega productos.</li>}
            {lineas.map((linea) => (
              <li key={linea.key} className="rounded-inner bg-elevated/40 px-3 py-2">
                <div className="flex items-start justify-between gap-2">
                  <div className="min-w-0">
                    <p className="truncate text-sm text-ink">
                      {linea.nombre}
                      <span className="text-muted"> · {linea.varianteNombre}</span>
                    </p>
                    <p className="num text-[11px] text-muted">{formatUSD(linea.precioUSD)} c/u</p>
                  </div>
                  <button
                    type="button"
                    aria-label={`Quitar ${linea.nombre}`}
                    onClick={() => quitar(linea.key)}
                    className="text-muted transition hover:text-danger-ink"
                  >
                    <Trash2 size={14} />
                  </button>
                </div>
                <div className="mt-2 flex items-center justify-between">
                  <label className="flex items-center gap-1.5 text-[11px] text-muted">
                    <input
                      type="checkbox"
                      checked={linea.esCortesia}
                      onChange={() => alternarCortesia(linea.key)}
                      className="h-4 w-4 accent-[rgb(var(--color-accent))]"
                    />
                    Cortesía
                  </label>
                  <div className="flex items-center gap-2">
                    <button
                      type="button"
                      aria-label="Quitar una unidad"
                      onClick={() => cambiarCantidad(linea.key, -1)}
                      className="flex h-8 w-8 items-center justify-center rounded-full border border-hairline text-ink"
                    >
                      <Minus size={13} />
                    </button>
                    <span className="num w-6 text-center text-sm text-ink">{linea.cantidad}</span>
                    <button
                      type="button"
                      aria-label="Agregar una unidad"
                      onClick={() => cambiarCantidad(linea.key, 1)}
                      className="flex h-8 w-8 items-center justify-center rounded-full border border-hairline text-ink"
                    >
                      <Plus size={13} />
                    </button>
                  </div>
                </div>
              </li>
            ))}
          </ul>
          <div className="flex items-center justify-between text-sm">
            <span className="text-muted">Total</span>
            <span className="num text-base font-medium text-ink">{formatUSD(total)}</span>
          </div>
          <Button size="lg" className="w-full" loading={enviando} disabled={lineas.length === 0 || enviando} onClick={enviar}>
            Enviar a {area}
          </Button>
        </aside>
      </div>

      <SelectorProductoModal producto={productoModal} onCerrar={() => setProductoModal(null)} onConfirmar={confirmarSeleccion} />
    </div>,
    document.body,
  );
}
