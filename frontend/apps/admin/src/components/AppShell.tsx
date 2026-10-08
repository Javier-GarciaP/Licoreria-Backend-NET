import { useEffect, useMemo, useState, type ReactNode } from 'react';
import { NavLink, useLocation, useNavigate } from 'react-router-dom';
import { ChevronRight, LogOut, Menu, Moon, Search, Sun, Wine } from 'lucide-react';
import { cn } from '@licoreria/ui';
import { useAuth } from '../context/AuthContext';
import { useTheme } from '../context/ThemeContext';
import { esRutaActiva, filtrarGrupos } from '../lib/navigation';
import { etiquetaRol } from '../lib/roles';
import { Breadcrumbs } from './Breadcrumbs';
import { CommandPalette } from './CommandPalette';
import { MobileNavSheet } from './MobileNavSheet';

const GRUPOS_KEY = 'licoreria.nav.grupos';

export function AppShell({ children }: { children: ReactNode }) {
  const { usuario, logout, esAdmin, tienePermiso, rolDominio } = useAuth() as {
    usuario: { nombreCompleto: string; rol: string; rolDominio?: string } | null;
    logout: () => Promise<void>;
    esAdmin: boolean;
    tienePermiso: (clave: string) => boolean;
    rolDominio?: string;
  };
  const { esOscuro, alternarTema } = useTheme() as { esOscuro: boolean; alternarTema: () => void };
  const navigate = useNavigate();
  const location = useLocation();
  const [sheetAbierto, setSheetAbierto] = useState(false);
  const [paletteAbierto, setPaletteAbierto] = useState(false);

  const grupos = useMemo(() => filtrarGrupos(esAdmin, tienePermiso, rolDominio), [esAdmin, tienePermiso, rolDominio]);
  const primarios = useMemo(
    () => grupos.flatMap((grupo) => grupo.items).filter((item) => item.primario),
    [grupos],
  );

  const [abiertos, setAbiertos] = useState<string[]>(() => {
    try {
      const guardado = localStorage.getItem(GRUPOS_KEY);
      return guardado ? (JSON.parse(guardado) as string[]) : [];
    } catch {
      return [];
    }
  });

  useEffect(() => {
    localStorage.setItem(GRUPOS_KEY, JSON.stringify(abiertos));
  }, [abiertos]);

  const grupoActivo = grupos.find((grupo) =>
    grupo.items.some((item) => esRutaActiva(location.pathname, item)),
  )?.id;

  useEffect(() => {
    if (grupoActivo) {
      setAbiertos((actuales) => (actuales.includes(grupoActivo) ? actuales : [...actuales, grupoActivo]));
    }
  }, [grupoActivo]);

  useEffect(() => {
    const alPresionar = (evento: KeyboardEvent) => {
      if ((evento.metaKey || evento.ctrlKey) && evento.key.toLowerCase() === 'k') {
        evento.preventDefault();
        setPaletteAbierto((actual) => !actual);
      }
    };
    document.addEventListener('keydown', alPresionar);
    return () => document.removeEventListener('keydown', alPresionar);
  }, []);

  const alternarGrupo = (id: string) =>
    setAbiertos((actuales) => (actuales.includes(id) ? actuales.filter((valor) => valor !== id) : [...actuales, id]));

  const cerrarSesion = async () => {
    await logout();
    navigate('/login', { replace: true });
  };

  const claseItem = ({ isActive }: { isActive: boolean }) =>
    cn(
      'flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm transition-colors',
      isActive
        ? 'bg-primary/15 font-medium text-foreground'
        : 'text-muted-foreground hover:bg-accent/10 hover:text-foreground',
    );

  return (
    <div className="flex h-dvh flex-col bg-background lg:flex-row">
      <a
        href="#contenido"
        className="sr-only focus:not-sr-only focus:absolute focus:left-4 focus:top-4 focus:z-50 focus:rounded-md focus:bg-primary focus:px-4 focus:py-2 focus:text-sm focus:text-primary-foreground"
      >
        Saltar al contenido
      </a>

      {/* Sidebar de escritorio */}
      <aside
        aria-label="Navegación principal"
        className="hidden w-72 shrink-0 flex-col overflow-y-auto border-r border-border bg-card lg:flex"
      >
        <div className="flex items-center gap-2.5 px-3 py-3">
          <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary text-primary-foreground">
            <Wine className="h-5 w-5" />
          </span>
          <div>
            <p className="text-sm font-medium tracking-tighter2 text-foreground">Licorería</p>
            <p className="text-xs text-muted-foreground">{etiquetaRol(usuario?.rolDominio, usuario?.rol)}</p>
          </div>
        </div>

        <button
          type="button"
          onClick={() => setPaletteAbierto(true)}
          className="mx-3 mt-2 flex items-center gap-2 rounded-lg border border-border bg-background px-3 py-2 text-sm text-muted-foreground transition-colors hover:bg-accent/10 hover:text-foreground"
        >
          <Search className="h-4 w-4" />
          <span className="flex-1 text-left">Buscar…</span>
          <kbd className="rounded border border-border px-1.5 py-0.5 text-[10px] text-muted-foreground">⌘K</kbd>
        </button>

        <nav aria-label="Secciones" className="app-scroll mt-2 flex flex-1 flex-col gap-1 overflow-y-auto px-3 pr-1">
          {grupos.map((grupo) => {
            const ItemIcono = grupo.items[0].icon;

            if (grupo.items.length === 1) {
              const item = grupo.items[0];
              return (
                <NavLink
                  key={grupo.id}
                  to={item.to}
                  end={item.end ?? item.to === '/'}
                  className={claseItem}
                >
                  <ItemIcono className="h-[18px] w-[18px]" />
                  {item.label}
                </NavLink>
              );
            }

            const Icono = grupo.icon;
            const activo = grupoActivo === grupo.id;
            const abierto = abiertos.includes(grupo.id);
            return (
              <div key={grupo.id} className="flex flex-col">
                <button
                  type="button"
                  onClick={() => alternarGrupo(grupo.id)}
                  aria-expanded={abierto}
                  className={cn(
                    'flex items-center gap-2.5 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors',
                    activo ? 'text-foreground' : 'text-muted-foreground hover:text-foreground hover:bg-accent/10',
                  )}
                >
                  <Icono className="h-4 w-4" />
                  <span className="flex-1 text-left">{grupo.label}</span>
                  <ChevronRight
                    className={cn('h-3.5 w-3.5 transition-transform motion-reduce:transition-none', abierto && 'rotate-90')}
                  />
                </button>
                {abierto && (
                  <div className="ml-2 mt-0.5 flex flex-col gap-0.5 border-l border-border/60 pl-3">
                    {grupo.items.map((item) => {
                      const SubIcono = item.icon;
                      return (
                        <NavLink
                          key={item.to}
                          to={item.to}
                          end={item.end ?? item.to === '/'}
                          className={claseItem}
                        >
                          <SubIcono className="h-[18px] w-[18px]" />
                          {item.label}
                        </NavLink>
                      );
                    })}
                  </div>
                )}
              </div>
            );
          })}
        </nav>

        <div className="mt-4 flex flex-col gap-1.5 border-t border-border px-3 pb-3 pt-3">
          <button
            onClick={alternarTema}
            className="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm text-muted-foreground transition-colors hover:bg-accent/10 hover:text-foreground"
          >
            {esOscuro ? <Sun className="h-4 w-4" /> : <Moon className="h-4 w-4" />}
            Tema {esOscuro ? 'Claro' : 'Oscuro'}
          </button>
          <div className="flex items-center justify-between rounded-lg bg-muted/60 px-3 py-2.5">
            <div className="min-w-0">
              <p className="truncate text-xs font-medium text-foreground">{usuario?.nombreCompleto}</p>
              <p className="truncate text-[11px] text-muted-foreground">{etiquetaRol(usuario?.rolDominio, usuario?.rol)}</p>
            </div>
            <button
              onClick={cerrarSesion}
              className="text-muted-foreground transition-colors hover:text-destructive-fg"
              aria-label="Cerrar sesión"
            >
              <LogOut className="h-4 w-4" />
            </button>
          </div>
        </div>
      </aside>

      <div className="flex min-h-0 min-w-0 flex-1 flex-col">
        {/* Barra superior: móvil con marca, escritorio con búsqueda y tema. */}
        <header className="z-20 flex h-14 shrink-0 items-center justify-between gap-3 border-b border-border bg-card/95 px-4 backdrop-blur-sm">
          <div className="flex items-center gap-2 lg:hidden">
            <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary text-primary-foreground">
              <Wine className="h-4 w-4" />
            </span>
            <p className="text-sm font-medium text-foreground">Licorería</p>
          </div>
          <div className="flex items-center gap-2 lg:flex-1 lg:justify-end">
            <button
              onClick={alternarTema}
              className="rounded-md border border-border p-2 text-muted-foreground transition-colors hover:bg-accent/10 hover:text-foreground"
              aria-label="Cambiar tema"
            >
              {esOscuro ? <Sun className="h-4 w-4" /> : <Moon className="h-4 w-4" />}
            </button>
            <button
              onClick={cerrarSesion}
              className="rounded-md border border-border p-2 text-muted-foreground transition-colors hover:text-destructive-fg lg:hidden"
              aria-label="Cerrar sesión"
            >
              <LogOut className="h-4 w-4" />
            </button>
          </div>
        </header>

        <main
          id="contenido"
          className="app-scroll relative min-h-0 flex-1 overflow-y-auto p-4 pb-28 lg:p-6"
        >
          {!location.pathname.startsWith('/salon/planos/') && !location.pathname.startsWith('/plano') && <Breadcrumbs />}
          {children}
        </main>
      </div>

      {/* Navegación rápida móvil */}
      <nav
        aria-label="Navegación rápida"
        className="fixed inset-x-4 bottom-4 z-30 flex items-center gap-0.5 rounded-xl border border-border bg-card p-1.5 shadow-md lg:hidden"
      >
        {primarios.map((item) => {
          const Icono = item.icon;
          return (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.end ?? item.to === '/'}
              onClick={() => setSheetAbierto(false)}
              className={({ isActive }) =>
                cn(
                  'flex min-w-0 flex-1 flex-col items-center gap-1 rounded-lg px-1.5 py-1.5 text-[10px] transition-colors',
                  isActive ? 'bg-primary/15 font-medium text-foreground' : 'text-muted-foreground',
                )
              }
            >
              <Icono className="h-5 w-5" />
              <span className="w-full truncate text-center">{item.corto ?? item.label}</span>
            </NavLink>
          );
        })}
        <button
          type="button"
          onClick={() => setSheetAbierto(true)}
          className="flex min-w-0 flex-1 flex-col items-center gap-1 rounded-lg px-1.5 py-1.5 text-[10px] text-muted-foreground transition-colors hover:text-foreground"
        >
          <Menu size={20} />
          <span className="w-full truncate text-center">Más</span>
        </button>
      </nav>

      <MobileNavSheet open={sheetAbierto} onClose={() => setSheetAbierto(false)} grupos={grupos} />
      <CommandPalette open={paletteAbierto} onClose={() => setPaletteAbierto(false)} grupos={grupos} />
    </div>
  );
}