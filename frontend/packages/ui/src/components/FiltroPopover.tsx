import { useEffect, useRef, useState, type ReactNode } from 'react';
import { createPortal } from 'react-dom';
import { ChevronDown } from 'lucide-react';
import { cn } from '../lib/cn';

export interface FiltroPopoverProps {
  /** Etiqueta del botón (p. ej. "Total USD", "Fecha"). Se muestra arriba del botón. */
  label: string;
  /** Valor seleccionado (p. ej. "5 – 20"). Si está vacío, el botón muestra "Todos". */
  valor?: string;
  children: ReactNode;
}

/** Filtro con label arriba y panel posicionado bajo el botón. Patrón estándar de listados. */
export function FiltroPopover({ label, valor = '', children }: FiltroPopoverProps) {
  const [abierto, setAbierto] = useState(false);
  const botonRef = useRef<HTMLButtonElement>(null);
  const panelRef = useRef<HTMLDivElement>(null);

  const cerrar = () => setAbierto(false);

  useEffect(() => {
    if (!abierto) return;
    const alTeclado = (evento: KeyboardEvent) => {
      if (evento.key === 'Escape') cerrar();
    };
    const alClic = (evento: MouseEvent) => {
      const objetivo = evento.target as Node;
      const boton = botonRef.current;
      const panel = panelRef.current;
      if (boton && boton.contains(objetivo)) return;
      if (panel && panel.contains(objetivo)) return;
      cerrar();
    };
    document.addEventListener('keydown', alTeclado);
    document.addEventListener('pointerdown', alClic);
    return () => {
      document.removeEventListener('keydown', alTeclado);
      document.removeEventListener('pointerdown', alClic);
    };
  }, [abierto]);

  const rect = botonRef.current?.getBoundingClientRect();
  const abrirAbajo = rect && window.innerHeight - rect.bottom > 240;

  return (
    <div className="flex flex-col gap-1">
      <span className="text-xs font-medium text-foreground">{label}</span>
      <button
        ref={botonRef}
        type="button"
        aria-haspopup="dialog"
        aria-expanded={abierto}
        onClick={() => setAbierto((actual) => !actual)}
        className={cn(
          'inline-flex items-center gap-1.5 rounded border px-3 py-2 text-sm transition',
          valor ? 'border-primary bg-primary/10 text-foreground' : 'border-border bg-card text-muted-foreground hover:text-foreground',
        )}
      >
        <span className="flex-1 text-left">{valor || 'Todos'}</span>
        <ChevronDown className="h-3.5 w-3.5" />
      </button>

      {abierto && rect && createPortal(
        <div
          ref={panelRef}
          role="dialog"
          aria-label={label}
          style={abrirAbajo ? { top: rect.bottom + 8, left: rect.left } : { bottom: window.innerHeight - rect.top + 8, left: rect.left }}
          className="fixed z-50 rounded-xl border border-border bg-popover p-4 shadow-md"
        >
          {children}
        </div>,
        document.body,
      )}
    </div>
  );
}