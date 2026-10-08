import { useCallback, useEffect, useRef, useState, type ReactNode } from 'react';
import { createPortal } from 'react-dom';
import { MoreHorizontal } from 'lucide-react';
import { cn } from '../lib/cn';

export interface ActionMenuOption {
  label: string;
  icon?: ReactNode;
  onClick: () => void;
  danger?: boolean;
  disabled?: boolean;
}

/** Menú de acciones "…" alineado al botón, en un popover de vidrio. */
export function ActionMenu({
  options,
  label,
  className,
}: {
  options: ActionMenuOption[];
  /** Etiqueta accesible del botón. */
  label: string;
  className?: string;
}) {
  const [abierto, setAbierto] = useState(false);
  const [pos, setPos] = useState<{ top: number; right: number; abajo: boolean } | null>(null);
  const botonRef = useRef<HTMLButtonElement>(null);

  const medir = useCallback(() => {
    const rect = botonRef.current?.getBoundingClientRect();
    if (!rect) return;
    const abajo = window.innerHeight - rect.bottom > 240;
    setPos({
      top: abajo ? rect.bottom + 8 : rect.top - 8,
      right: window.innerWidth - rect.right,
      abajo,
    });
  }, []);

  const alternar = () => {
    if (!abierto) medir();
    setAbierto((actual) => !actual);
  };

  useEffect(() => {
    if (!abierto) return;
    const alPresionar = (evento: KeyboardEvent) => {
      if (evento.key === 'Escape') setAbierto(false);
    };
    const alClic = (evento: MouseEvent) => {
      const objetivo = evento.target as Node;
      if (!botonRef.current?.contains(objetivo)) setAbierto(false);
    };
    const alRedimensionar = () => medir();
    document.addEventListener('keydown', alPresionar);
    document.addEventListener('mousedown', alClic);
    window.addEventListener('resize', alRedimensionar);
    return () => {
      document.removeEventListener('keydown', alPresionar);
      document.removeEventListener('mousedown', alClic);
      window.removeEventListener('resize', alRedimensionar);
    };
  }, [abierto, medir]);

  return (
    <>
      <button
        ref={botonRef}
        type="button"
        aria-label={label}
        aria-haspopup="menu"
        aria-expanded={abierto}
        onClick={(evento) => {
          evento.stopPropagation();
          alternar();
        }}
        className={cn(
          'flex h-8 w-8 items-center justify-center rounded-full text-muted transition',
          'hover:bg-ink/5 hover:text-ink focus:outline-none focus-visible:ring-2 focus-visible:ring-accent/60',
          abierto && 'bg-ink/5 text-ink',
          className,
        )}
      >
        <MoreHorizontal size={16} />
      </button>

      {abierto &&
        pos &&
        createPortal(
          <div
            role="menu"
            aria-label={label}
            style={{ top: pos.top, right: pos.right }}
            className={cn(
              'fixed z-[60] flex min-w-[11rem] flex-col rounded-2xl glass-card border border-hairline p-1.5 shadow-card',
              pos.abajo ? 'translate-y-0' : '-translate-y-full',
            )}
          >
            {options.map((opcion) => (
              <button
                key={opcion.label}
                type="button"
                role="menuitem"
                disabled={opcion.disabled}
                onClick={(evento) => {
                  evento.stopPropagation();
                  setAbierto(false);
                  opcion.onClick();
                }}
                className={cn(
                  'flex w-full items-center gap-2.5 rounded-xl px-3 py-2 text-left text-sm transition',
                  'focus:outline-none focus-visible:ring-2 focus-visible:ring-accent/60',
                  'disabled:cursor-not-allowed disabled:opacity-40',
                  opcion.danger
                    ? 'text-danger-ink hover:bg-danger/15'
                    : 'text-ink hover:bg-ink/5',
                )}
              >
                {opcion.icon && <span className="shrink-0 text-current opacity-80">{opcion.icon}</span>}
                <span className="flex-1">{opcion.label}</span>
              </button>
            ))}
          </div>,
          document.body,
        )}
    </>
  );
}