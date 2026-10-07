import {
  forwardRef,
  useCallback,
  useEffect,
  useImperativeHandle,
  useRef,
  useState,
  type KeyboardEvent,
} from 'react';
import type { Producto } from '@licoreria/types';
import { cn } from '@licoreria/ui';
import { formatUSD } from '../../lib/format';

const rangoPrecio = (producto: Producto) => {
  const precios = producto.variantes.map((variante) => variante.precioVentaUSD).filter((valor) => valor > 0);
  if (precios.length === 0) return '—';
  const minimo = Math.min(...precios);
  const maximo = Math.max(...precios);
  return minimo === maximo ? formatUSD(minimo) : `${formatUSD(minimo)} – ${formatUSD(maximo)}`;
};

export interface ProductoGridHandle {
  enfocar: (indice?: number) => void;
}

export const ProductoGrid = forwardRef<
  ProductoGridHandle,
  {
    productos: Producto[];
    cargando: boolean;
    onSeleccionar: (producto: Producto) => void;
  }
>(function ProductoGrid({ productos, cargando, onSeleccionar }, ref) {
  const [indice, setIndice] = useState(0);
  const refs = useRef<(HTMLButtonElement | null)[]>([]);

  useEffect(() => {
    setIndice(0);
  }, [productos]);

  useEffect(() => {
    refs.current[indice]?.scrollIntoView?.({ block: 'nearest' });
  }, [indice]);

  const enfocar = useCallback(
    (destino = 0) => {
      const limite = Math.max(0, productos.length - 1);
      const siguiente = Math.min(Math.max(0, destino), limite);
      setIndice(siguiente);
      refs.current[siguiente]?.focus({ preventScroll: true });
    },
    [productos.length],
  );

  useImperativeHandle(ref, () => ({ enfocar }), [enfocar]);

  const alPresionar = (evento: KeyboardEvent<HTMLButtonElement>, posicion: number) => {
    if (!['ArrowRight', 'ArrowDown', 'ArrowLeft', 'ArrowUp', 'Home', 'End'].includes(evento.key)) return;
    evento.preventDefault();
    evento.stopPropagation();
    switch (evento.key) {
      case 'ArrowRight':
      case 'ArrowDown':
        enfocar(posicion + 1);
        break;
      case 'ArrowLeft':
      case 'ArrowUp':
        enfocar(posicion - 1);
        break;
      case 'Home':
        enfocar(0);
        break;
      case 'End':
        enfocar(productos.length - 1);
        break;
      default:
        break;
    }
  };

  if (cargando) {
    return (
      <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 xl:grid-cols-4">
        {Array.from({ length: 12 }).map((_, posicion) => (
          <div key={posicion} className="h-28 animate-pulse rounded-card bg-elevated" />
        ))}
      </div>
    );
  }

  if (productos.length === 0) {
    return (
      <div className="flex flex-col items-center gap-2 rounded-card border border-hairline bg-surface/60 px-6 py-14 text-center">
        <p className="text-base font-medium text-ink">Sin productos</p>
        <p className="text-sm text-muted">Ajusta la búsqueda o cambia de categoría.</p>
      </div>
    );
  }

  return (
    <div role="grid" aria-label="Productos" className="grid grid-cols-2 gap-3 sm:grid-cols-3 xl:grid-cols-4">
      {productos.map((producto, posicion) => {
        const activo = posicion === indice;
        const preparado = producto.tipo === 'Preparado';
        return (
          <button
            key={producto.id}
            ref={(elemento) => {
              refs.current[posicion] = elemento;
            }}
            type="button"
            role="gridcell"
            aria-selected={activo}
            tabIndex={activo ? 0 : -1}
            onClick={() => {
              setIndice(posicion);
              onSeleccionar(producto);
            }}
            onFocus={() => setIndice(posicion)}
            onKeyDown={(evento) => alPresionar(evento, posicion)}
            className={cn(
              'flex min-h-[7rem] flex-col items-start gap-1 rounded-card border p-4 text-left outline-none transition',
              'focus:outline-none focus-visible:outline-none',
              activo ? 'border-accent bg-accent/10' : 'border-hairline bg-surface/60 hover:border-accent/40',
            )}
          >
            <div className="flex w-full items-start justify-between gap-2">
              <span className="line-clamp-2 text-sm font-medium text-ink">{producto.nombre}</span>
              {preparado && (
                <span className="shrink-0 rounded-pill bg-info/25 px-2 py-0.5 text-[10px] font-medium text-info-ink">
                  Receta
                </span>
              )}
            </div>
            <span className="text-[11px] text-muted">{producto.categoriaNombre}</span>
            <span className="mt-auto flex w-full items-center justify-between pt-2">
              <span className="num text-sm text-accent-ink">{rangoPrecio(producto)}</span>
              <span className="text-[11px] text-muted">
                {producto.variantes.length === 1 ? '1 variante' : `${producto.variantes.length} variantes`}
              </span>
            </span>
          </button>
        );
      })}
    </div>
  );
});
