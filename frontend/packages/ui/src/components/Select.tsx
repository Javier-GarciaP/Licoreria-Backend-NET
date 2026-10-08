import { forwardRef, type ReactNode, type SelectHTMLAttributes } from 'react';
import { ChevronDown } from 'lucide-react';
import { cn } from '../lib/cn';

export interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label?: string;
  error?: string;
  hint?: string;
  children: ReactNode;
}

/**
 * Select nativo estilizado al mismo lenguaje que `Input` (shadcn/ui):
 * borde, radio y anillo de foco idénticos. Se conserva el `<select>`
 * nativo para integrar con `react-hook-form` (register) y accesibilidad.
 */
export const Select = forwardRef<HTMLSelectElement, SelectProps>(function Select(
  { label, error, hint, className, id, children, ...props },
  ref,
) {
  const selectId = id ?? props.name;
  return (
    <label className="flex w-full flex-col gap-1.5" htmlFor={selectId}>
      {label && <span className="text-sm font-medium text-foreground">{label}</span>}
      <span className="relative block">
        <select
          id={selectId}
          ref={ref}
          aria-invalid={error ? true : undefined}
          className={cn(
            'h-9 w-full appearance-none rounded-md border border-input bg-background py-1 pl-3 pr-9 text-sm shadow-sm transition-colors',
            'focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2',
            'disabled:cursor-not-allowed disabled:opacity-50',
            error ? 'border-destructive' : 'border-input',
            className,
          )}
          {...props}
        >
          {children}
        </select>
        <ChevronDown className="pointer-events-none absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
      </span>
      {error ? (
        <span className="text-sm text-destructive-fg">{error}</span>
      ) : hint ? (
        <span className="text-sm text-muted-foreground">{hint}</span>
      ) : null}
    </label>
  );
});