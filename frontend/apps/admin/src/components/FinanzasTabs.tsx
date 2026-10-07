import { NavLink } from 'react-router-dom';
import { cn } from '@licoreria/ui';

const TABS = [
  { to: '/finanzas', label: 'Tasas de cambio', end: true },
  { to: '/finanzas/tesoreria', label: 'Tesorería', end: false },
];

export function FinanzasTabs() {
  return (
    <nav aria-label="Secciones de finanzas" className="flex flex-wrap gap-2">
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
