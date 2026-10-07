import { useEffect, useRef } from 'react';
import { NavLink, Outlet, useLocation } from 'react-router-dom';
import { cn } from '@licoreria/ui';

const TABS = [
  { to: '/salon', label: 'Planos' },
  { to: '/salon/zonas', label: 'Zonas y mesas' },
];

/** Salón como sistema de carpetas: pestañas sobre un panel que contiene el contenido. */
export function SalonLayout() {
  const { pathname } = useLocation();
  const idx = Math.max(0, TABS.findIndex((tab) => tab.to === pathname));
  const previo = useRef(idx);
  const direccion = idx > previo.current ? 'salto-der' : idx < previo.current ? 'salto-izq' : '';

  useEffect(() => {
    previo.current = idx;
  }, [idx]);

  return (
    <div className="mx-auto flex max-w-page flex-col">
      {/* Pestañas tipo carpeta */}
      <nav aria-label="Secciones del salón" className="flex gap-1 pl-2">
        {TABS.map((tab) => {
          const activo = pathname === tab.to;
          return (
            <NavLink
              key={tab.to}
              to={tab.to}
              end
              className={cn(
                'rounded-t-2xl border border-b-0 px-5 py-2.5 text-sm transition',
                activo
                  ? cn('border-hairline bg-surface text-ink', direccion)
                  : 'border-transparent text-muted hover:bg-surface/40 hover:text-ink',
              )}
            >
              {tab.label}
            </NavLink>
          );
        })}
      </nav>

      {/* Panel que engloba el contenido */}
      <div className="rounded-b-2xl rounded-tr-2xl border border-hairline bg-surface/50 p-4 lg:p-6">
        <Outlet />
      </div>
    </div>
  );
}
