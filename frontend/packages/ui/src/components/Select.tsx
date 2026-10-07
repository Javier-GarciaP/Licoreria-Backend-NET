import { forwardRef, type ReactNode, type SelectHTMLAttributes } from 'react';
import { cn } from '../lib/cn';

export interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label?: string;
  error?: string;
  hint?: string;
  children: ReactNode;
}

export const Select = forwardRef<HTMLSelectElement, SelectProps>(function Select(
  { label, error, hint, className, id, children, ...props },
  ref,
) {
  const selectId = id ?? props.name;
  return (
    <label className="flex w-full flex-col gap-1.5" htmlFor={selectId}>
      {label && <span className="text-xs font-medium tracking-tighter2 text-muted">{label}</span>}
      <select
        id={selectId}
        ref={ref}
        className={cn(
          'h-10 w-full rounded-control border bg-surface px-4 text-sm text-ink transition',
          'focus:outline-none focus:ring-2 focus:ring-accent/50',
          error ? 'border-danger' : 'border-hairline',
          className,
        )}
        {...props}
      >
        {children}
      </select>
      {error ? (
        <span className="text-xs text-danger-ink">{error}</span>
      ) : hint ? (
        <span className="text-xs text-muted">{hint}</span>
      ) : null}
    </label>
  );
});
