import { useEffect, useRef, type ReactNode } from 'react';
import { cn } from '../lib/cn';

export function Modal({
  open,
  onClose,
  title,
  children,
  footer,
  className,
}: {
  open: boolean;
  onClose: () => void;
  title: string;
  children: ReactNode;
  footer?: ReactNode;
  className?: string;
}) {
  const dialogRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!open) return;

    const alPresionar = (evento: KeyboardEvent) => {
      if (evento.key === 'Escape') {
        onClose();
        return;
      }

      if (evento.key !== 'Tab') return;

      const contenedor = dialogRef.current;
      if (!contenedor) return;

      const focusables = contenedor.querySelectorAll<HTMLElement>(
        'a[href], button:not([disabled]), textarea, input, select, [tabindex]:not([tabindex="-1"])',
      );

      if (focusables.length === 0) {
        evento.preventDefault();
        return;
      }

      const primero = focusables[0];
      const ultimo = focusables[focusables.length - 1];

      if (evento.shiftKey && document.activeElement === primero) {
        evento.preventDefault();
        ultimo.focus();
      } else if (!evento.shiftKey && document.activeElement === ultimo) {
        evento.preventDefault();
        primero.focus();
      }
    };

    const elementoPrevio = document.activeElement as HTMLElement | null;
    document.addEventListener('keydown', alPresionar);
    dialogRef.current?.focus();

    return () => {
      document.removeEventListener('keydown', alPresionar);
      elementoPrevio?.focus?.();
    };
  }, [open, onClose]);

  if (!open) return null;

  return (
    <div
      className="fixed inset-0 z-50 flex items-end justify-center bg-black/60 p-0 sm:items-center sm:p-4"
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
          'w-full max-w-lg rounded-t-card bg-elevated p-6 shadow-card focus:outline-none sm:rounded-card',
          className,
        )}
        onClick={(event) => event.stopPropagation()}
      >
        <div className="flex items-center justify-between gap-4">
          <h2 id="modal-titulo" className="text-base font-semibold text-ink">
            {title}
          </h2>
          <button onClick={onClose} className="text-muted transition hover:text-ink" aria-label="Cerrar">
            &#x2715;
          </button>
        </div>
        <div className="mt-4">{children}</div>
        {footer && <div className="mt-6 flex justify-end gap-2">{footer}</div>}
      </div>
    </div>
  );
}
