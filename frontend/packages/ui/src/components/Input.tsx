import { forwardRef, type InputHTMLAttributes, type ReactNode } from 'react';
import { cn } from '../lib/cn';

export interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  hint?: string;
  rightSlot?: ReactNode;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(function Input(
  { label, error, hint, rightSlot, className, id, ...props },
  ref,
) {
  const inputId = id ?? props.name;
  return (
    <label className="flex w-full flex-col gap-1.5" htmlFor={inputId}>
      {label && <span className="text-xs font-medium tracking-tighter2 text-muted">{label}</span>}
      <span className="relative flex items-center">
        <input
          id={inputId}
          ref={ref}
          className={cn(
            'h-10 w-full rounded-pill border bg-surface px-4 text-sm text-ink transition',
            'placeholder:text-stone focus:outline-none focus:ring-2 focus:ring-accent/50',
            rightSlot && 'pr-11',
            error ? 'border-danger' : 'border-hairline',
            className,
          )}
          {...props}
        />
        {rightSlot && <span className="absolute right-3 flex items-center">{rightSlot}</span>}
      </span>
      {error ? (
        <span className="text-xs text-danger">{error}</span>
      ) : hint ? (
        <span className="text-xs text-muted">{hint}</span>
      ) : null}
    </label>
  );
});
