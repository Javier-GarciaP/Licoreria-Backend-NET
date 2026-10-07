import { NavLink } from 'react-router-dom';
import { cn } from '@licoreria/ui';

const TABS = [
  { to: '/inventario', label: 'Existencias', end: true },
  { to: '/inventario/kardex', label: 'Kardex', end: false },
  { to: '/inventario/lotes', label: 'Lotes', end: false },
  { to: '/inventario/tomas', label: 'Tomas físicas', end: false },
];

export function InventarioTabs() {
  return (
    <nav aria-label="Secciones de inventario" className="flex flex-wrap gap-2">
      {TABS.map((tab) => (
        <NavLink
          key={tab.to}
          to={tab.to}
          end={tab.end}
          className={({ isActive }) =>
            cn(
              'rounded-pill border px-4 py-1.5 text-sm transition',
              isActive ? 'border-accent text-accent-soft' : 'border-hairline text-muted hover:text-ink',
            )
          }
        >
          {tab.label}
        </NavLink>
      ))}
    </nav>
  );
}
