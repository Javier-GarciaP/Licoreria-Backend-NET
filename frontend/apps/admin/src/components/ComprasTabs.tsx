import { NavLink } from 'react-router-dom';
import { cn } from '@licoreria/ui';

const TABS = [
  { to: '/compras', label: 'Órdenes', end: true },
  { to: '/compras/proveedores', label: 'Proveedores', end: false },
  { to: '/compras/recepciones', label: 'Recepciones', end: false },
  { to: '/compras/cuentas', label: 'Cuentas por pagar', end: false },
];

export function ComprasTabs() {
  return (
    <nav aria-label="Secciones de compras" className="flex flex-wrap gap-2">
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
