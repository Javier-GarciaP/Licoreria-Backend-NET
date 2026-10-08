import { useEffect, useRef } from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import { cn } from '@licoreria/ui';

export interface FolderTab {
  to: string;
  label: string;
  end?: boolean;
}

/** ¿La ruta actual pertenece a esta pestaña (respetando límites de segmento)? */
function coincide(tab: FolderTab, pathname: string): boolean {
  if (tab.end) return pathname === tab.to;
  return pathname === tab.to || pathname.startsWith(`${tab.to}/`);
}

/** Pestañas tipo gestor de carpetas (estilo Salón) con salto al cambiar de carpeta. */
export function FolderTabs({ tabs, ariaLabel }: { tabs: FolderTab[]; ariaLabel: string }) {
  const { pathname } = useLocation();
  const idx = Math.max(0, tabs.findIndex((tab) => coincide(tab, pathname)));
  const previo = useRef(idx);
  const direccion = idx > previo.current ? 'salto-der' : idx < previo.current ? 'salto-izq' : '';

  useEffect(() => {
    previo.current = idx;
  }, [idx]);

  return (
    <nav aria-label={ariaLabel} className="flex gap-1 pl-2">
      {tabs.map((tab) => {
        const activo = coincide(tab, pathname);
        return (
          <NavLink
            key={tab.to}
            to={tab.to}
            end={tab.end}
            className={cn(
              'rounded-t-2xl border border-b-0 px-5 py-2.5 text-sm transition',
              activo ? cn('border-hairline bg-surface text-ink', direccion) : 'border-transparent text-muted hover:bg-surface/40 hover:text-ink',
            )}
          >
            {tab.label}
          </NavLink>
        );
      })}
    </nav>
  );
}

/** Panel que engloba el contenido bajo las pestañas carpeta. */
export function FolderPanel({ children, className }: { children: React.ReactNode; className?: string }) {
  return (
    <div className={cn('rounded-b-2xl rounded-tr-2xl border border-hairline bg-surface/50 p-4 lg:p-6', className)}>
      {children}
    </div>
  );
}