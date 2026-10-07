import type { ReactNode } from 'react';
import { createPortal } from 'react-dom';
import { cn } from '../lib/cn';
import { useFocusTrap } from '../lib/useFocusTrap';

export function Modal({
  open,
  onClose,
  title,
  children,
  footer,
  className,
  backdrop = 'dim',
}: {
  open: boolean;
  onClose: () => void;
  title: string;
  children: ReactNode;
  footer?: ReactNode;
  className?: string;
  backdrop?: 'dim' | 'none';
}) {
  const dialogRef = useFocusTrap<HTMLDivElement>(open, onClose);

  if (!open) return null;

  return createPortal(
    <div
      className={cn(
        'fixed inset-0 z-50 flex items-end justify-center p-0 sm:items-center sm:p-4',
        backdrop === 'dim' ? 'bg-black/60' : 'bg-transparent',
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
          'w-full max-w-lg rounded-t-card glass-card p-6 shadow-card focus:outline-none sm:rounded-card',
          className,
        )}
        onClick={(event) => event.stopPropagation()}
      >
        <div className="flex items-center justify-between gap-4">
          <h2 id="modal-titulo" className="text-base font-medium text-ink">
            {title}
          </h2>
          <button onClick={onClose} className="text-muted transition hover:text-ink" aria-label="Cerrar">
            &#x2715;
          </button>
        </div>
        <div className="mt-4">{children}</div>
        {footer && <div className="mt-6 flex justify-end gap-2">{footer}</div>}
      </div>
    </div>,
    document.body,
  );
}
