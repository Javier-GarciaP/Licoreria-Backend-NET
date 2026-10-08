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
      <div className="flex flex-1 flex-col items-center justify-center gap-2 rounded-xl border border-dashed border-border px-4 py-10 text-center">
        <p className="text-sm text-muted-foreground">Agrega productos para iniciar la venta.</p>
        <p className="text-xs text-muted-foreground">F2 busca · Enter agrega</p>
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
                seleccionada ? 'border-primary/60 bg-primary/10' : 'border-transparent bg-muted/40',
              )}
            >
              <div className="min-w-0 flex-1">
                <p className="truncate text-sm text-foreground">
                  <span className="num mr-1.5 text-muted-foreground">{linea.cantidad}×</span>
                  {linea.nombre}
                  {linea.varianteNombre && <span className="text-muted-foreground"> · {linea.varianteNombre}</span>}
                </p>
                {linea.modificadores.length > 0 && (
                  <p className="truncate text-[11px] text-foreground">
                    + {linea.modificadores.map((mod) => mod.nombre).join(', ')}
                  </p>
                )}
                <p className="num text-xs text-muted-foreground">{formatUSD(precioUnitario(linea))} c/u</p>
              </div>
              <button
                type="button"
                aria-label={`Eliminar ${linea.nombre}`}
                onClick={() => onQuitar(linea.key)}
                className="text-muted-foreground transition hover:text-destructive-fg"
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
