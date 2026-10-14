import { useEffect, useRef, useState } from 'react';
import { Bell, CheckCheck, CookingPot, Trash2, X } from 'lucide-react';
import { cn } from '@licoreria/ui';
import type { Notificacion } from '../../lib/notificaciones';

function haceCuanto(ts: number): string {
  const segundos = Math.max(0, Math.floor((Date.now() - ts) / 1000));
  if (segundos < 10) return 'ahora';
  if (segundos < 60) return `hace ${segundos} s`;
  const minutos = Math.floor(segundos / 60);
  if (minutos < 60) return `hace ${minutos} min`;
  const horas = Math.floor(minutos / 60);
  return `hace ${horas} h`;
}

/**
 * Campana de notificaciones del mesonero: badge de no leídas, panel desplegable
 * que se abre/cierra, cierre al tocar fuera, descartar individual y limpiar todo.
 */
export function Notificaciones({
  notificaciones,
  onMarcarLeidas,
  onDescartar,
  onLimpiar,
}: {
  notificaciones: Notificacion[];
  onMarcarLeidas: () => void;
  onDescartar: (id: string) => void;
  onLimpiar: () => void;
}) {
  const [abierto, setAbierto] = useState(false);
  const raizRef = useRef<HTMLDivElement>(null);

  const noLeidas = notificaciones.filter((n) => !n.leida).length;

  useEffect(() => {
    const alTocarFuera = (evento: MouseEvent) => {
      if (abierto && !raizRef.current?.contains(evento.target as Node)) {
        setAbierto(false);
      }
    };
    document.addEventListener('mousedown', alTocarFuera);
    return () => document.removeEventListener('mousedown', alTocarFuera);
  }, [abierto]);

  const alternar = () => {
    const siguiente = !abierto;
    setAbierto(siguiente);
    if (siguiente) onMarcarLeidas();
  };

  return (
    <div ref={raizRef} className="relative">
      <button
        type="button"
        aria-label={`Notificaciones (${noLeidas} no leídas)`}
        onClick={alternar}
        className={cn(
          'relative flex h-10 w-10 items-center justify-center rounded-full border text-foreground transition hover:bg-accent/10',
          abierto ? 'border-primary bg-primary/10' : 'border-border',
        )}
      >
        <Bell size={17} />
        {noLeidas > 0 && (
          <span className="absolute -right-0.5 -top-0.5 flex h-5 min-w-5 items-center justify-center rounded-full bg-danger px-1 num text-[10px] text-primary-foreground">
            {noLeidas}
          </span>
        )}
      </button>

      {abierto && (
        <div className="absolute right-0 top-12 z-50 flex w-80 flex-col overflow-hidden rounded-xl border border-border bg-card/95 shadow-card backdrop-blur">
          <div className="flex items-center justify-between gap-2 border-b border-border px-3 py-2.5">
            <p className="text-sm font-medium text-foreground">Notificaciones</p>
            <div className="flex items-center gap-1">
              {notificaciones.length > 0 && (
                <button
                  type="button"
                  onClick={onLimpiar}
                  className="flex items-center gap-1 rounded-full px-2 py-1 text-[11px] text-muted-foreground transition hover:bg-destructive/10 hover:text-destructive-fg"
                >
                  <Trash2 size={12} /> Limpiar
                </button>
              )}
              <button
                type="button"
                aria-label="Cerrar"
                onClick={() => setAbierto(false)}
                className="flex h-7 w-7 items-center justify-center rounded-full text-muted-foreground transition hover:bg-accent/10 hover:text-foreground"
              >
                <X size={14} />
              </button>
            </div>
          </div>

          <div className="app-scroll max-h-80 min-h-0 overflow-y-auto p-2">
            {notificaciones.length === 0 ? (
              <p className="px-3 py-8 text-center text-xs text-muted-foreground">No tienes notificaciones.</p>
            ) : (
              <ul className="flex flex-col gap-1">
                {notificaciones.map((notificacion) => (
                  <li
                    key={notificacion.id}
                    className={cn(
                      'group flex items-start gap-2 rounded-lg px-2.5 py-2 transition',
                      notificacion.leida ? 'opacity-70' : 'bg-primary/10',
                    )}
                  >
                    <span
                      className={cn(
                        'mt-0.5 flex h-6 w-6 shrink-0 items-center justify-center rounded-full',
                        notificacion.tipo === 'exito' ? 'bg-success/15 text-success-fg' : 'bg-warning/15 text-warning-fg',
                      )}
                    >
                      {notificacion.tipo === 'exito' ? <CheckCheck size={13} /> : <CookingPot size={13} />}
                    </span>
                    <div className="min-w-0 flex-1">
                      <p className="text-xs leading-snug text-foreground">{notificacion.texto}</p>
                      <p className="text-[10px] text-muted-foreground">{haceCuanto(notificacion.cuando)}</p>
                    </div>
                    <button
                      type="button"
                      aria-label="Descartar"
                      onClick={() => onDescartar(notificacion.id)}
                      className="text-muted-foreground opacity-0 transition group-hover:opacity-100 hover:text-destructive-fg"
                    >
                      <X size={13} />
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
      )}
    </div>
  );
}