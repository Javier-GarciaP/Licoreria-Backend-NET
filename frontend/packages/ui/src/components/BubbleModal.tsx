import type { ReactNode } from 'react';
import { createPortal } from 'react-dom';
import { cn } from '../lib/cn';

const ANCHO = 340;
const MARGEN = 16;

/**
 * Burbuja de texto con cola apuntando al elemento que la abrió (estilo juegos).
 * Para formularios breves (≤ 3 campos). Sin fondo oscuro.
 */
export function BubbleModal({
  open,
  onClose,
  title,
  anchor,
  children,
  footer,
  width = ANCHO,
}: {
  open: boolean;
  onClose: () => void;
  title: string;
  /** Elemento (botón) al que apunta la cola. */
  anchor: HTMLElement | null;
  children: ReactNode;
  footer?: ReactNode;
  width?: number;
}) {
  if (!open || !anchor) return null;

  const rect = anchor.getBoundingClientRect();
  const abajo = window.innerHeight - rect.bottom > 240;
  const cx = rect.left + rect.width / 2;
  const left = Math.max(MARGEN, Math.min(cx - width / 2, window.innerWidth - width - MARGEN));
  const colaX = Math.max(18, Math.min(cx - left, width - 18));

  return createPortal(
    <div
      role="dialog"
      aria-label={title}
      style={abajo ? { top: rect.bottom + 12, left } : { bottom: window.innerHeight - rect.top + 12, left }}
      className="fixed z-50 w-[20rem] rounded-xl border border-border bg-popover shadow-md"
    >
      {/* Cola */}
      <span
        aria-hidden
        className="absolute h-3 w-3 rotate-45 border border-border bg-popover"
        style={abajo ? { top: -6, left: colaX - 6 } : { bottom: -6, left: colaX - 6 }}
      />

      <div className="p-4">
        <div className="mb-3 flex items-center justify-between gap-2">
          <p className={cn('text-sm font-medium tracking-tighter2 text-foreground', 'flex-1 truncate')}>{title}</p>
          <button
            type="button"
            onClick={onClose}
            className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full text-muted-foreground transition hover:bg-accent/10 hover:text-foreground focus:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
            aria-label="Cerrar"
          >
            &#x2715;
          </button>
        </div>
        {children}
        {footer && <div className="mt-4 flex flex-wrap justify-end gap-2">{footer}</div>}
      </div>
    </div>,
    document.body,
  );
}