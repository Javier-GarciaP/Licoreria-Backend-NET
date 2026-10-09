import { Check, ChevronDown } from 'lucide-react';
import { cn } from '../lib/cn';
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuPortal, DropdownMenuTrigger } from './ui/dropdown-menu';

export interface FiltroOpcion {
  valor: string;
  etiqueta: string;
}

export interface FiltroDropdownProps {
  /** Etiqueta fija que identifica el filtro (p. ej. "Estado", "Categoría"). Se muestra arriba del botón. */
  label: string;
  opciones: FiltroOpcion[];
  valor?: string;
  onChange?: (valor: string) => void;
  /** Mostrar el ítem "Limpiar" (activa siempre que haya selección). */
  limpiable?: boolean;
}

/** Filtro con label arriba y botón desplegable con el valor seleccionado. Patrón estándar de listados. */
export function FiltroDropdown({ label, opciones, valor, onChange, limpiable = true }: FiltroDropdownProps) {
  const seleccionada = opciones.find((opcion) => opcion.valor === valor);

  return (
    <div className="flex flex-col gap-1">
      <span className="text-xs font-medium text-foreground">{label}</span>
      <DropdownMenu>
        <DropdownMenuTrigger
          className={cn(
            'inline-flex items-center gap-1.5 rounded border px-3 py-2 text-sm transition',
            seleccionada ? 'border-primary bg-primary/10 text-foreground' : 'border-border bg-card text-muted-foreground hover:text-foreground',
          )}
        >
          <span className="flex-1 text-left">{seleccionada ? seleccionada.etiqueta : 'Todos'}</span>
          <ChevronDown className="h-3.5 w-3.5" />
        </DropdownMenuTrigger>
        <DropdownMenuPortal>
          <DropdownMenuContent
            align="start"
            sideOffset={4}
            className="w-56 rounded-xl border border-border bg-popover p-1.5 text-sm shadow-md"
          >
            {limpiable && (
              <DropdownMenuItem
                onSelect={() => onChange?.('')}
                className="flex cursor-pointer items-center rounded-md px-2.5 py-2 text-sm text-muted-foreground outline-none focus:bg-accent/10 focus:text-foreground data-[highlighted]:bg-accent/10 data-[highlighted]:text-foreground"
              >
                <span className="flex-1">Todos</span>
                {!valor && <Check className="h-4 w-4 text-foreground" />}
              </DropdownMenuItem>
            )}
            {opciones.map((opcion) => (
              <DropdownMenuItem
                key={opcion.valor}
                onSelect={() => onChange?.(opcion.valor)}
                className="flex cursor-pointer items-center rounded-md px-2.5 py-2 text-sm text-foreground outline-none focus:bg-accent/10 data-[highlighted]:bg-accent/10"
              >
                <span className="flex-1">{opcion.etiqueta}</span>
                {valor === opcion.valor && <Check className="h-4 w-4 text-foreground" />}
              </DropdownMenuItem>
            ))}
          </DropdownMenuContent>
        </DropdownMenuPortal>
      </DropdownMenu>
    </div>
  );
}