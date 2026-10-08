import { useState } from 'react';
import { Plus, X } from 'lucide-react';
import { cn } from '@licoreria/ui';
import type { Orden } from '../../hooks/usePos';

export function OrdenTabs({
  ordenes,
  activaId,
  onActivar,
  onNueva,
  onCerrar,
  onRenombrar,
}: {
  ordenes: Orden[];
  activaId: string;
  onActivar: (id: string) => void;
  onNueva: () => void;
  onCerrar: (id: string) => void;
  onRenombrar: (id: string, nombre: string) => void;
}) {
  const [editando, setEditando] = useState<string | null>(null);
  const [borrador, setBorrador] = useState('');

  const empezarEdicion = (orden: Orden) => {
    setEditando(orden.id);
    setBorrador(orden.nombre);
  };

  const guardar = (id: string) => {
    const nombre = borrador.trim();
    if (nombre) onRenombrar(id, nombre);
    setEditando(null);
  };

  return (
    <div className="flex items-center gap-2">
      <div role="tablist" aria-label="Órdenes en espera" className="app-scroll flex flex-1 gap-1.5 overflow-x-auto">
        {ordenes.map((orden, indice) => {
          const activa = orden.id === activaId;
          return (
            <div
              key={orden.id}
              className={cn(
                'group flex h-10 shrink-0 items-center gap-1.5 rounded-full border px-3 text-sm transition',
                activa ? 'border-primary bg-primary/15 text-foreground' : 'border-border text-muted-foreground hover:text-foreground',
              )}
            >
              {editando === orden.id ? (
                <input
                  autoFocus
                  value={borrador}
                  onChange={(evento) => setBorrador(evento.target.value)}
                  onBlur={() => guardar(orden.id)}
                  onKeyDown={(evento) => {
                    if (evento.key === 'Enter') guardar(orden.id);
                    if (evento.key === 'Escape') setEditando(null);
                  }}
                  className="w-24 bg-transparent text-sm text-foreground focus:outline-none"
                />
              ) : (
                <button
                  type="button"
                  role="tab"
                  aria-selected={activa}
                  onClick={() => onActivar(orden.id)}
                  onDoubleClick={() => empezarEdicion(orden)}
                  className="flex items-center gap-1.5"
                >
                  {indice < 9 && <kbd className="text-[10px] opacity-60">⌥{indice + 1}</kbd>}
                  <span className="max-w-[7rem] truncate">{orden.nombre}</span>
                  {orden.lineas.length > 0 && <span className="num text-[11px]">({orden.lineas.length})</span>}
                </button>
              )}
              {ordenes.length > 1 && (
                <button
                  type="button"
                  aria-label={`Cerrar ${orden.nombre}`}
                  onClick={() => onCerrar(orden.id)}
                  className="text-muted-foreground transition hover:text-destructive-fg"
                >
                  <X size={13} />
                </button>
              )}
            </div>
          );
        })}
      </div>
      <button
        type="button"
        aria-label="Nueva orden"
        onClick={onNueva}
        className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full border border-border text-muted-foreground transition hover:text-foreground"
      >
        <Plus size={16} />
      </button>
    </div>
  );
}
