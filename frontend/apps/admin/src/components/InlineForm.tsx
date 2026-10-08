import type { ReactNode } from 'react';
import { X } from 'lucide-react';
import { cn } from '@licoreria/ui';

/** Panel de formulario inline (alta/edición) que se expande dentro de un módulo. */
export function InlineForm({
  title,
  onCancel,
  children,
  footer,
  className,
}: {
  title: string;
  onCancel: () => void;
  children: ReactNode;
  footer?: ReactNode;
  className?: string;
}) {
  return (
    <div className={cn('rounded-2xl border border-accent/40 bg-surface p-4 shadow-soft', className)}>
      <div className="mb-3 flex items-center justify-between gap-3">
        <p className="text-sm font-medium text-ink">{title}</p>
        <button
          type="button"
          onClick={onCancel}
          className="flex h-7 w-7 items-center justify-center rounded-full text-muted transition hover:bg-ink/5 hover:text-ink focus:outline-none focus-visible:ring-2 focus-visible:ring-accent/60"
          aria-label="Cerrar formulario"
        >
          <X size={14} />
        </button>
      </div>
      {children}
      {footer && <div className="mt-4 flex flex-wrap justify-end gap-2">{footer}</div>}
    </div>
  );
}