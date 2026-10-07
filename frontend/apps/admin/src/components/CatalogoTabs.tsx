import { NavLink } from 'react-router-dom';
import { cn } from '@licoreria/ui';

const TABS = [
  { to: '/productos', label: 'Productos', end: true },
  { to: '/catalogos', label: 'Categorías y marcas', end: false },
  { to: '/catalogos-avanzado', label: 'Unidades, impuestos y listas', end: false },
];

/** Sub-navegación del catálogo unificado. */
export function CatalogoTabs() {
  return (
    <nav aria-label="Secciones de catálogo" className="flex flex-wrap gap-2">
      {TABS.map((tab) => (
        <NavLink
          key={tab.to}
          to={tab.to}
          end={tab.end}
          className={({ isActive }) =>
            cn(
              'rounded-pill border px-4 py-1.5 text-sm transition',
              isActive ? 'border-accent text-accent-ink' : 'border-hairline text-muted hover:text-ink',
            )
          }
        >
          {tab.label}
        </NavLink>
      ))}
    </nav>
  );
}
