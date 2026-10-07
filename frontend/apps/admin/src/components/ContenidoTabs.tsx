import { NavLink } from 'react-router-dom';
import { cn } from '@licoreria/ui';

const TABS = [
  { to: '/contenido', label: 'Páginas', end: true },
  { to: '/contenido/eventos', label: 'Eventos', end: false },
  { to: '/contenido/local', label: 'Horarios y local', end: false },
  { to: '/contenido/menu', label: 'Menú y archivos', end: false },
];

export function ContenidoTabs() {
  return (
    <nav aria-label="Secciones de contenido" className="flex flex-wrap gap-2">
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
