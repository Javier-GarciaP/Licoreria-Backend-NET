import { forwardRef, type InputHTMLAttributes, type ReactNode } from 'react';
import { cn } from '../lib/cn';
import { Input as InputPrimitivo } from './ui/input';

export interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  hint?: string;
  /** Icono/acción a la izquierda del input (p. ej. lupa de búsqueda). */
  leftSlot?: ReactNode;
  rightSlot?: ReactNode;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(function Input(
  { label, error, hint, leftSlot, rightSlot, className, id, ...props },
  ref,
) {
  const inputId = id ?? props.name;
  return (
    <label className="flex w-full flex-col gap-1.5" htmlFor={inputId}>
      {label && <span className="text-sm font-medium text-foreground">{label}</span>}
      <span className="relative flex items-center">
        {leftSlot && <span className="absolute left-3 flex items-center text-muted-foreground">{leftSlot}</span>}
        <InputPrimitivo
          id={inputId}
          ref={ref}
          aria-invalid={error ? true : undefined}
          className={cn(error && 'border-destructive', leftSlot && 'pl-9', rightSlot && 'pr-9', className)}
          {...props}
        />
        {rightSlot && <span className="absolute right-3 flex items-center text-muted-foreground">{rightSlot}</span>}
      </span>
      {error ? (
        <span className="text-sm text-destructive-fg">{error}</span>
      ) : hint ? (
        <span className="text-sm text-muted-foreground">{hint}</span>
      ) : null}
    </label>
  );
});