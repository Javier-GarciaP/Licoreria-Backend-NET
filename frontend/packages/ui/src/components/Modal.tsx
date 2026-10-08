import type { ReactNode } from 'react';
import { cn } from '../lib/cn';
import { Dialog, DialogClose, DialogContent, DialogOverlay, DialogPortal, DialogTitle } from './ui/dialog';

const anchos: Record<NonNullable<ModalProps['size']>, string> = {
  sm: 'max-w-md',
  md: 'max-w-lg',
  lg: 'max-w-2xl',
  xl: 'max-w-4xl',
};

const fondos: Record<NonNullable<ModalProps['backdrop']>, string> = {
  dim: 'bg-black/60',
  soft: 'bg-black/25',
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
  /** Fondo sobre el contenido. `none` deja el diálogo flotar sin opacar (estilo POS). */
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
  return (
    <Dialog open={open} onOpenChange={(abierto) => { if (!abierto) onClose(); }}>
      <DialogPortal>
        <DialogOverlay className={cn('fixed inset-0 z-50', fondos[backdrop])} />
        <DialogContent
          aria-modal="true"
          className="fixed inset-0 z-50 flex items-end justify-center p-0 sm:items-center sm:p-6"
        >
          <div
            className={cn(
              'flex max-h-[92dvh] w-full flex-col overflow-y-auto app-scroll rounded-t-xl bg-card p-6 shadow-lg sm:rounded-xl sm:p-8',
              anchos[size],
              className,
            )}
          >
            <div className="flex items-center justify-between gap-4">
              <DialogTitle className="text-lg font-medium tracking-tighter2 text-foreground">{title}</DialogTitle>
              <DialogClose
                aria-label="Cerrar"
                className={cn(
                  'flex h-8 w-8 shrink-0 items-center justify-center rounded-full text-muted-foreground transition',
                  'hover:bg-accent/10 hover:text-foreground',
                  'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2',
                )}
              >
                &#x2715;
              </DialogClose>
            </div>
            <div className="mt-5">{children}</div>
            {footer && <div className="mt-7 flex flex-wrap justify-end gap-3">{footer}</div>}
          </div>
        </DialogContent>
      </DialogPortal>
    </Dialog>
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
      <p className="text-xs font-medium text-muted-foreground">{title}</p>
      {children}
    </section>
  );
}