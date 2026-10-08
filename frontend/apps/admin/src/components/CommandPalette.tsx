import { useEffect, useMemo, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { ArrowRight, Package, Search, Zap } from 'lucide-react';
import { cn, useFocusTrap } from '@licoreria/ui';
import { catalogoApi } from '@licoreria/api-client';
import type { NavGroup, NavItem } from '../lib/navigation';

interface Comando {
  label: string;
  sublabel: string;
  to: string;
  icon: NavItem['icon'];
}

/** Paleta de comandos (⌘K / Ctrl+K): módulos, acciones rápidas y búsqueda de datos. */
export function CommandPalette({
  open,
  onClose,
  grupos,
}: {
  open: boolean;
  onClose: () => void;
  grupos: NavGroup[];
}) {
  const navigate = useNavigate();
  const ref = useFocusTrap<HTMLDivElement>(open, onClose);
  const inputRef = useRef<HTMLInputElement>(null);
  const [consulta, setConsulta] = useState('');
  const [activo, setActivo] = useState(0);
  const [debounced, setDebounced] = useState('');

  useEffect(() => {
    const t = setTimeout(() => setDebounced(consulta.trim()), 250);
    return () => clearTimeout(t);
  }, [consulta]);

  const productos = useQuery({
    queryKey: ['palette', 'productos', debounced],
    queryFn: () => catalogoApi.productos({ busqueda: debounced, pageSize: 5, activo: true }),
    enabled: open && debounced.length >= 2,
  });

  const comandos = useMemo<Comando[]>(() => {
    const deNav = grupos.flatMap((grupo) =>
      grupo.items.map((item) => ({ label: item.label, sublabel: grupo.label, to: item.to, icon: item.icon })),
    );
    const acciones: Comando[] = [
      { label: 'Nueva venta', sublabel: 'Acciones', to: '/pos', icon: Zap },
      { label: 'Abrir mesa', sublabel: 'Acciones', to: '/plano', icon: Zap },
      { label: 'Registrar merma', sublabel: 'Acciones', to: '/mermas', icon: Zap },
      { label: 'Abrir caja', sublabel: 'Acciones', to: '/caja', icon: Zap },
    ];
    const recursos: Comando[] = [];
    if (debounced.length >= 2) {
      for (const p of productos.data?.items ?? []) {
        recursos.push({
          label: p.nombre,
          sublabel: 'Producto',
          to: `/pos?busqueda=${encodeURIComponent(p.nombre)}`,
          icon: Package,
        });
      }
    }
    return [...acciones, ...recursos, ...deNav];
  }, [grupos, debounced, productos.data]);

  const filtrados = useMemo(() => {
    const q = consulta.trim().toLowerCase();
    if (!q) return comandos;
    return comandos.filter((c) => `${c.label} ${c.sublabel}`.toLowerCase().includes(q));
  }, [comandos, consulta]);

  useEffect(() => {
    setActivo(0);
  }, [consulta, open]);

  useEffect(() => {
    if (open) inputRef.current?.focus();
  }, [open]);

  const cerrar = () => {
    onClose();
    setConsulta('');
  };

  const ejecutar = (comando?: Comando) => {
    if (!comando) return;
    navigate(comando.to);
    cerrar();
  };

  const alTeclear = (evento: React.KeyboardEvent) => {
    if (evento.key === 'ArrowDown') {
      evento.preventDefault();
      setActivo((i) => Math.min(i + 1, filtrados.length - 1));
    } else if (evento.key === 'ArrowUp') {
      evento.preventDefault();
      setActivo((i) => Math.max(i - 1, 0));
    } else if (evento.key === 'Enter') {
      evento.preventDefault();
      ejecutar(filtrados[activo]);
    }
  };

  if (!open) return null;

  return (
    <div
      className="fixed inset-0 z-50 flex items-start justify-center bg-black/40 px-4 pt-[12vh]"
      role="presentation"
      onClick={cerrar}
    >
      <div
        ref={ref}
        role="dialog"
        aria-modal="true"
        aria-label="Paleta de comandos"
        tabIndex={-1}
        onClick={(evento) => evento.stopPropagation()}
        className="glass-panel w-full max-w-lg overflow-hidden rounded-2xl shadow-card focus:outline-none"
      >
        <div className="flex items-center gap-2 border-b border-hairline px-4 py-3">
          <Search size={16} className="text-muted" />
          <input
            ref={inputRef}
            value={consulta}
            onChange={(evento) => setConsulta(evento.target.value)}
            onKeyDown={alTeclear}
            placeholder="Buscar módulo o producto…"
            className="w-full bg-transparent text-sm text-ink placeholder:text-stone focus:outline-none"
            aria-label="Buscar"
          />
          <kbd className="rounded border border-hairline px-1.5 py-0.5 text-[10px] text-muted">Esc</kbd>
        </div>

        <div className="app-scroll max-h-[50vh] overflow-y-auto p-2">
          {filtrados.length === 0 && <p className="px-3 py-6 text-center text-sm text-muted">Sin resultados.</p>}
          {filtrados.map((comando, indice) => {
            const Icono = comando.icon;
            return (
              <button
                key={`${comando.sublabel}-${comando.label}-${comando.to}`}
                type="button"
                onMouseEnter={() => setActivo(indice)}
                onClick={() => ejecutar(comando)}
                className={cn(
                  'flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-left text-sm transition',
                  indice === activo ? 'bg-accent/20 text-accent-ink' : 'text-muted hover:bg-ink/5 hover:text-ink',
                )}
              >
                <Icono size={16} />
                <span className="flex-1 truncate text-ink">{comando.label}</span>
                <span className="text-[11px] text-muted">{comando.sublabel}</span>
                {indice === activo && <ArrowRight size={14} />}
              </button>
            );
          })}
        </div>
      </div>
    </div>
  );
}
