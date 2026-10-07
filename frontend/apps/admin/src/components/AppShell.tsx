import type { ReactNode } from 'react';
import { NavLink, useNavigate } from 'react-router-dom';
import {
  BarChart3,
  Boxes,
  CalendarClock,
  ChefHat,
  CircleUser,
  Grid2x2,
  LayoutGrid,
  LogOut,
  Moon,
  Package,
  Receipt,
  ShoppingCart,
  Sun,
  Tags,
  Trash2,
  Users,
  Wallet,
  Wine,
} from 'lucide-react';
import { cn } from '@licoreria/ui';
import { useAuth } from '../context/AuthContext';
import { useTheme } from '../context/ThemeContext';
import { useModo } from '../context/ModoContext';

interface NavItem {
  to: string;
  label: string;
  icon: ReactNode;
  permiso?: string;
  admin?: boolean;
}

const NAV_ITEMS: NavItem[] = [
  { to: '/', label: 'Dashboard', icon: <BarChart3 size={20} />, permiso: 'sales:read' },
  { to: '/pos', label: 'POS', icon: <ShoppingCart size={20} />, permiso: 'sales:write' },
  { to: '/plano', label: 'Mesas', icon: <Grid2x2 size={20} />, permiso: 'club:read' },
  { to: '/kds', label: 'KDS Barra', icon: <ChefHat size={20} />, permiso: 'catalog:read' },
  { to: '/cuentas', label: 'Cuentas', icon: <LayoutGrid size={20} />, permiso: 'sales:read' },
  { to: '/ventas', label: 'Ventas', icon: <Receipt size={20} />, permiso: 'sales:read' },
  { to: '/reservas', label: 'Reservas', icon: <CalendarClock size={20} />, permiso: 'reservation:manage' },
  { to: '/mermas', label: 'Mermas', icon: <Trash2 size={20} />, permiso: 'inventory:merma' },
  { to: '/inventario', label: 'Inventario', icon: <Boxes size={20} />, permiso: 'inventory:read' },
  { to: '/productos', label: 'Catálogo', icon: <Package size={20} />, permiso: 'catalog:read' },
  { to: '/catalogos', label: 'Catálogo base', icon: <Tags size={20} />, permiso: 'catalog:read' },
  { to: '/caja', label: 'Caja', icon: <Wallet size={20} />, permiso: 'cash:movement' },
  { to: '/clientes', label: 'Clientes', icon: <Users size={20} />, permiso: 'crm:read' },
  { to: '/usuarios', label: 'Usuarios', icon: <CircleUser size={20} />, admin: true },
];

export function AppShell({ children }: { children: ReactNode }) {
  const { usuario, logout, esAdmin, tienePermiso } = useAuth() as {
    usuario: { nombreCompleto: string; rol: string } | null;
    logout: () => Promise<void>;
    esAdmin: boolean;
    tienePermiso: (clave: string) => boolean;
  };
  const { esOscuro, alternarTema } = useTheme() as { esOscuro: boolean; alternarTema: () => void };
  const { esDiscoteca, alternarModo } = useModo() as { esDiscoteca: boolean; alternarModo: () => void };
  const navigate = useNavigate();

  const visibles = NAV_ITEMS.filter(
    (item) => (item.admin ? esAdmin : !item.permiso || esAdmin || tienePermiso(item.permiso)),
  );

  const cerrarSesion = async () => {
    await logout();
    navigate('/login', { replace: true });
  };

  return (
    <div className="flex min-h-dvh bg-canvas">
      <a
        href="#contenido"
        className="sr-only focus:not-sr-only focus:absolute focus:left-4 focus:top-4 focus:z-50 focus:rounded-pill focus:bg-accent focus:px-4 focus:py-2 focus:text-sm focus:text-white"
      >
        Saltar al contenido
      </a>
      <aside
        aria-label="Navegación principal"
        className="sticky top-0 hidden h-dvh w-64 flex-col border-r border-hairline bg-surface px-4 py-6 lg:flex"
      >
        <div className="flex items-center gap-2 px-2">
          <span className="flex h-9 w-9 items-center justify-center rounded-full bg-accent text-white shadow-glow">
            <Wine size={18} />
          </span>
          <div>
            <p className="text-sm font-semibold tracking-tighter2 text-ink">Licorería</p>
            <p className="text-[11px] text-muted">{esDiscoteca ? 'Modo Discoteca' : 'Modo Licorería'}</p>
          </div>
        </div>

        <nav aria-label="Secciones" className="app-scroll mt-8 flex flex-1 flex-col gap-1 overflow-y-auto">
          {visibles.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.to === '/'}
              className={({ isActive }) =>
                cn(
                  'flex items-center gap-3 rounded-2xl px-3 py-2.5 text-sm text-muted transition',
                  isActive ? 'bg-elevated text-ink shadow-soft' : 'hover:bg-elevated/60 hover:text-ink',
                )
              }
            >
              {item.icon}
              {item.label}
            </NavLink>
          ))}
        </nav>

        <div className="mt-4 flex flex-col gap-2 border-t border-hairline pt-4">
          <button
            onClick={alternarModo}
            className="flex items-center gap-3 rounded-2xl px-3 py-2.5 text-sm text-muted transition hover:bg-elevated/60 hover:text-ink"
          >
            <Wine size={18} />
            Cambiar a {esDiscoteca ? 'Licorería' : 'Discoteca'}
          </button>
          <button
            onClick={alternarTema}
            className="flex items-center gap-3 rounded-2xl px-3 py-2.5 text-sm text-muted transition hover:bg-elevated/60 hover:text-ink"
          >
            {esOscuro ? <Sun size={18} /> : <Moon size={18} />}
            Tema {esOscuro ? 'Azul UNET' : 'Oscuro'}
          </button>
          <div className="flex items-center justify-between rounded-2xl bg-elevated px-3 py-2.5">
            <div className="min-w-0">
              <p className="truncate text-xs font-medium text-ink">{usuario?.nombreCompleto}</p>
              <p className="truncate text-[11px] text-muted">{usuario?.rol}</p>
            </div>
            <button onClick={cerrarSesion} className="text-muted transition hover:text-danger" aria-label="Cerrar sesión">
              <LogOut size={16} />
            </button>
          </div>
        </div>
      </aside>

      <div className="flex min-w-0 flex-1 flex-col">
        <header className="sticky top-0 z-20 flex items-center justify-between gap-3 border-b border-hairline bg-canvas/80 px-4 py-3 backdrop-blur lg:px-8">
          <div className="flex items-center gap-2">
            <span className="flex h-8 w-8 items-center justify-center rounded-full bg-accent text-white lg:hidden">
              <Wine size={16} />
            </span>
            <p className="text-sm font-semibold text-ink lg:hidden">Licorería</p>
          </div>
          <div className="flex items-center gap-2">
            <button
              onClick={alternarModo}
              className="rounded-pill border border-hairline px-3 py-1.5 text-xs text-muted transition hover:text-ink"
            >
              {esDiscoteca ? 'Discoteca' : 'Licorería'}
            </button>
            <button
              onClick={alternarTema}
              className="rounded-pill border border-hairline p-2 text-muted transition hover:text-ink"
              aria-label="Cambiar tema"
            >
              {esOscuro ? <Sun size={16} /> : <Moon size={16} />}
            </button>
            <button
              onClick={cerrarSesion}
              className="rounded-pill border border-hairline p-2 text-muted transition hover:text-danger lg:hidden"
              aria-label="Cerrar sesión"
            >
              <LogOut size={16} />
            </button>
          </div>
        </header>

        <main id="contenido" className="app-scroll flex-1 px-4 py-6 pb-24 lg:px-8 lg:pb-8">{children}</main>
      </div>

      <nav
        aria-label="Navegación rápida"
        className="app-scroll fixed inset-x-0 bottom-0 z-30 flex items-center gap-1 overflow-x-auto border-t border-hairline bg-surface/95 px-2 py-2 backdrop-blur lg:hidden"
      >
        {visibles.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            end={item.to === '/'}
            className={({ isActive }) =>
              cn(
                'flex min-w-[3.75rem] flex-col items-center gap-1 rounded-2xl px-2 py-1.5 text-[10px]',
                isActive ? 'bg-elevated text-ink' : 'text-muted',
              )
            }
          >
            {item.icon}
            {item.label}
          </NavLink>
        ))}
      </nav>
    </div>
  );
}
