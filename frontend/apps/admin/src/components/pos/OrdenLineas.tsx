import { Trash2 } from 'lucide-react';
import { cn } from '@licoreria/ui';
import { formatUSD } from '../../lib/format';
import { precioUnitario, type LineaOrden } from '../../hooks/usePos';

export function OrdenLineas({
  lineas,
  indiceSeleccionado,
  onSeleccionar,
  onQuitar,
}: {
  lineas: LineaOrden[];
  indiceSeleccionado: number;
  onSeleccionar: (indice: number) => void;
  onQuitar: (key: string) => void;
}) {
  if (lineas.length === 0) {
    return (
      <div className="flex flex-1 flex-col items-center justify-center gap-2 rounded-card border border-dashed border-hairline px-4 py-10 text-center">
        <p className="text-sm text-muted">Agrega productos para iniciar la venta.</p>
        <p className="text-xs text-stone">F2 busca · Enter agrega</p>
      </div>
    );
  }

  return (
    <ul className="app-scroll flex min-h-0 flex-1 flex-col gap-1.5 overflow-y-auto pr-1">
      {lineas.map((linea, indice) => {
        const seleccionada = indice === indiceSeleccionado;
        return (
          <li key={linea.key}>
            <div
              role="button"
              tabIndex={-1}
              onMouseDown={() => onSeleccionar(indice)}
              className={cn(
                'flex items-center gap-3 rounded-inner border px-3 py-2 transition',
                seleccionada ? 'border-accent/60 bg-accent/10' : 'border-transparent bg-elevated/40',
              )}
            >
              <div className="min-w-0 flex-1">
                <p className="truncate text-sm text-ink">
                  <span className="num mr-1.5 text-muted">{linea.cantidad}×</span>
                  {linea.nombre}
                  {linea.varianteNombre && <span className="text-muted"> · {linea.varianteNombre}</span>}
                </p>
                {linea.modificadores.length > 0 && (
                  <p className="truncate text-[11px] text-accent-ink">
                    + {linea.modificadores.map((mod) => mod.nombre).join(', ')}
                  </p>
                )}
                <p className="num text-xs text-muted">{formatUSD(precioUnitario(linea))} c/u</p>
              </div>
              <button
                type="button"
                aria-label={`Eliminar ${linea.nombre}`}
                onClick={() => onQuitar(linea.key)}
                className="text-muted transition hover:text-danger-ink"
              >
                <Trash2 size={15} />
              </button>
            </div>
          </li>
        );
      })}
    </ul>
  );
}
