import { Search, X } from 'lucide-react';
import type { InputHTMLAttributes } from 'react';
import { cn } from '../lib/cn';
import { Input } from './Input';

export interface BuscadorProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'onChange'> {
  placeholder?: string;
  /** Devuelve el valor filtrado ('' al limpiar). */
  onCambio?: (valor: string) => void;
}

/** Input de búsqueda con lupa a la izquierda y botón para limpiar. Patrón estándar de listados. */
export function Buscador({ value = '', onCambio, placeholder = 'Buscar…', className, ...props }: BuscadorProps) {
  return (
    <div className={cn('w-80 shrink-0', className)}>
      <Input
        placeholder={placeholder}
        value={value}
        onChange={(evento) => onCambio?.(evento.target.value)}
        leftSlot={<Search className="h-4 w-4" />}
        rightSlot={
          value ? (
            <button
              type="button"
              aria-label="Limpiar búsqueda"
              onClick={() => onCambio?.('')}
              className="rounded-full text-muted-foreground transition hover:bg-accent/10 hover:text-foreground focus:outline-none focus-visible:ring-2 focus-visible:ring-ring"
            >
              <X className="h-4 w-4" />
            </button>
          ) : null
        }
        {...props}
      />
    </div>
  );
}