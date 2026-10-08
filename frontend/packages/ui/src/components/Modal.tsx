import type { ReactNode } from 'react';
import { createPortal } from 'react-dom';
import { cn } from '../lib/cn';
import { useFocusTrap } from '../lib/useFocusTrap';

const anchos: Record<NonNullable<ModalProps['size']>, string> = {
  sm: 'max-w-md',
  md: 'max-w-lg',
  lg: 'max-w-2xl',
  xl: 'max-w-4xl',
};

const fondos: Record<NonNullable<ModalProps['backdrop']>, string> = {
  dim: 'bg-black/60',
  soft: 'bg-black/20 backdrop-blur-[2px]',
  none: 'bg-transparent',
};

export interface ModalProps {
  open: boolean;
  onClose: () => void;
  title: string;
  children: ReactNode;
  footer?: ReactNode;
  className?: string;
  /** Ancho máximo del diálogo. Por defecto `md` (max-w-lg). */
  size?: 'sm' | 'md' | 'lg' | 'xl';
  /** Fondo sobre el contenido. `none` deja el vidrio flotar sin opacar (estilo POS). */
  backdrop?: 'dim' | 'soft' | 'none';
}

export function Modal({
  open,
  onClose,
  title,
  children,
  footer,
  className,
  size = 'md',
  backdrop = 'dim',
}: ModalProps) {
  const dialogRef = useFocusTrap<HTMLDivElement>(open, onClose);

  if (!open) return null;

  return createPortal(
    <div
      className={cn(
        'fixed inset-0 z-50 flex items-end justify-center p-0 sm:items-center sm:p-6',
        fondos[backdrop],
      )}
      onClick={onClose}
      role="presentation"
    >
      <div
        ref={dialogRef}
        role="dialog"
        aria-modal="true"
        aria-labelledby="modal-titulo"
        tabIndex={-1}
        className={cn(
          'w-full max-h-[92dvh] overflow-y-auto app-scroll rounded-t-card glass-card shadow-card focus:outline-none sm:rounded-card',
          'p-6 sm:p-8',
          anchos[size],
          className,
        )}
        onClick={(event) => event.stopPropagation()}
      >
        <div className="flex items-center justify-between gap-4">
          <h2 id="modal-titulo" className="text-lg font-medium tracking-tighter2 text-ink">
            {title}
          </h2>
          <button
            onClick={onClose}
            className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full text-muted transition hover:bg-ink/5 hover:text-ink focus:outline-none focus-visible:ring-2 focus-visible:ring-accent/60"
            aria-label="Cerrar"
          >
            &#x2715;
          </button>
        </div>
        <div className="mt-5">{children}</div>
        {footer && <div className="mt-7 flex flex-wrap justify-end gap-3">{footer}</div>}
      </div>
    </div>,
    document.body,
  );
}

/** Sección de un modal de detalle: micro-título + contenido ordenado. */
export function ModalSection({
  title,
  children,
  className,
}: {
  title: string;
  children: ReactNode;
  className?: string;
}) {
  return (
    <section className={cn('flex flex-col gap-2', className)}>
      <p className="text-xs font-medium uppercase tracking-tighter2 text-muted">{title}</p>
      {children}
    </section>
  );
}