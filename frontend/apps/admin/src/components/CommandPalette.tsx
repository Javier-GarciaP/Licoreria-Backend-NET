import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Package, Zap } from 'lucide-react';
import { cn, Command, CommandDialog, CommandEmpty, CommandInput, CommandItem, CommandList } from '@licoreria/ui';
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
  const [consulta, setConsulta] = useState('');
  const [debounced, setDebounced] = useState('');

  useEffect(() => {
    const t = setTimeout(() => setDebounced(consulta.trim()), 250);
    return () => clearTimeout(t);
  }, [consulta, setDebounced]);

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

  const ejecutar = (comando: Comando) => {
    navigate(comando.to);
    setConsulta('');
    onClose();
  };

  return (
    <Command shouldFilter={false} loop>
      <CommandDialog
        open={open}
        onOpenChange={(abierto) => {
          if (!abierto) {
            setConsulta('');
            onClose();
          }
        }}
        label="Paleta de comandos"
        overlayClassName="fixed inset-0 z-50 bg-black/40"
        contentClassName="fixed left-1/2 top-[12vh] z-50 w-full max-w-lg -translate-x-1/2 overflow-hidden rounded-xl border border-border bg-card shadow-lg focus:outline-none"
      >
        <CommandInput
          value={consulta}
          onValueChange={setConsulta}
          placeholder="Buscar módulo o producto…"
          className="h-11 w-full border-0 bg-card px-4 text-sm text-foreground placeholder:text-muted-foreground shadow-none focus-visible:outline-none focus-visible:ring-0"
        />
        <CommandList className="app-scroll max-h-[50vh] overflow-y-auto border-t border-border p-2">
          <CommandEmpty className="px-3 py-6 text-center text-sm text-muted-foreground">
            Sin resultados.
          </CommandEmpty>
          {filtrados.map((comando) => {
            const Icono = comando.icon;
            return (
              <CommandItem
                key={`${comando.sublabel}-${comando.label}-${comando.to}`}
                value={`${comando.sublabel} ${comando.label}`}
                onSelect={() => ejecutar(comando)}
                className={cn(
                  'flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-left text-sm transition-colors',
                  'text-muted-foreground hover:bg-accent/10 hover:text-foreground',
                  'data-[selected=true]:bg-primary/15 data-[selected=true]:text-foreground',
                  'focus-visible:outline-none',
                )}
              >
                <Icono className="h-4 w-4" />
                <span className="flex-1 truncate text-foreground">{comando.label}</span>
                <span className="text-xs text-muted-foreground">{comando.sublabel}</span>
              </CommandItem>
            );
          })}
        </CommandList>
      </CommandDialog>
    </Command>
  );
}