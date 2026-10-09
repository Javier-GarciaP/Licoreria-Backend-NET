import { useEffect, useMemo, useState, type ReactNode } from 'react';
import { NavLink, useLocation, useNavigate } from 'react-router-dom';
import { ChevronRight, LogOut, Menu, Moon, PanelLeft, PanelRight, Search, Sun } from 'lucide-react';
import { cn } from '@licoreria/ui';
import { useAuth } from '../context/AuthContext';
import { useTheme } from '../context/ThemeContext';
import { esRutaActiva, filtrarGrupos } from '../lib/navigation';
import { Breadcrumbs } from './Breadcrumbs';
import { CommandPalette } from './CommandPalette';
import { Marca } from './Logo';
import { MobileNavSheet } from './MobileNavSheet';

const GRUPOS_KEY = 'licoreria.nav.grupos';
const CONTRAIDO_KEY = 'licoreria.nav.contraido';

export function AppShell({ children }: { children: ReactNode }) {
  const { logout, esAdmin, tienePermiso, rolDominio } = useAuth() as {
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

  const grupos = useMemo(
    () => filtrarGrupos(esAdmin, tienePermiso, rolDominio),
    [esAdmin, tienePermiso, rolDominio],
  );
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

  const [contraido, setContraido] = useState<boolean>(() => {
    try {
      return localStorage.getItem(CONTRAIDO_KEY) === '1';
    } catch {
      return false;
    }
  });

  useEffect(() => {
    localStorage.setItem(GRUPOS_KEY, JSON.stringify(abiertos));
  }, [abiertos]);

  useEffect(() => {
    localStorage.setItem(CONTRAIDO_KEY, contraido ? '1' : '0');
  }, [contraido]);

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
    setAbiertos((actuales) =>
      actuales.includes(id) ? actuales.filter((valor) => valor !== id) : [...actuales, id],
    );

  const abrirGrupo = (id: string) =>
    setAbiertos((actuales) => (actuales.includes(id) ? actuales : [...actuales, id]));

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

  const claseIcono = ({ isActive }: { isActive: boolean }) =>
    cn(
      'flex h-9 w-9 items-center justify-center rounded-lg transition-colors',
      isActive
        ? 'bg-primary/15 font-medium text-foreground'
        : 'text-muted-foreground hover:bg-accent/10 hover:text-foreground',
    );

  const esEditorSinMigas =
    location.pathname === '/productos/nuevo' ||
    /^\/productos\/[^/]+\/editar$/.test(location.pathname) ||
    location.pathname === '/compras/ordenes/nueva';
  const esDetalleSinMigas =
    /^\/reservas\/[^/]+$/.test(location.pathname) || /^\/cuentas\/[^/]+$/.test(location.pathname);

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
        onClick={contraido ? () => setContraido(false) : undefined}
        className={cn(
          'hidden shrink-0 flex-col overflow-y-auto border-r border-border bg-card lg:flex',
          'transition-[width] duration-200',
          contraido ? 'w-16 cursor-pointer' : 'w-72',
        )}
      >
        <div className={cn('flex items-center px-3 py-3', contraido && 'justify-center')}>
          <Marca compacto={contraido} />
        </div>

        <nav
          aria-label="Secciones"
          className={cn(
            'app-scroll mt-2 flex flex-1 flex-col overflow-y-auto',
            contraido ? 'items-center gap-[15px] px-1.5' : 'gap-1 px-3 pr-1',
          )}
        >
          {grupos.map((grupo) => {
            const ItemIcono = grupo.items[0].icon;
            const activo = grupoActivo === grupo.id;

            if (grupo.items.length === 1) {
              const item = grupo.items[0];
              return (
                <NavLink
                  key={grupo.id}
                  to={item.to}
                  end={item.end ?? item.to === '/'}
                  title={contraido ? item.label : undefined}
                  aria-label={contraido ? item.label : undefined}
                  className={contraido ? claseIcono : claseItem}
                >
                  <ItemIcono
                    className={cn('text-accent-ink', contraido ? 'h-[22px] w-[22px]' : 'h-[18px] w-[18px]')}
                  />
                  {!contraido && item.label}
                </NavLink>
              );
            }

            const Icono = grupo.icon;
            const abierto = abiertos.includes(grupo.id);

            if (contraido) {
              return (
                <button
                  key={grupo.id}
                  type="button"
                  onClick={() => {
                    setContraido(false);
                    abrirGrupo(grupo.id);
                  }}
                  className={cn(
                    'flex h-9 w-9 items-center justify-center rounded-lg transition-colors',
                    activo
                      ? 'bg-primary/15 text-foreground'
                      : 'text-muted-foreground hover:bg-accent/10 hover:text-foreground',
                  )}
                  title={grupo.label}
                  aria-label={grupo.label}
                >
                  <Icono className="h-[22px] w-[22px] text-accent-ink" />
                </button>
              );
            }

            return (
              <div key={grupo.id} className="flex flex-col">
                <button
                  type="button"
                  onClick={() => alternarGrupo(grupo.id)}
                  aria-expanded={abierto}
                  className={cn(
                    'flex items-center gap-2.5 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors',
                    activo
                      ? 'text-foreground'
                      : 'text-muted-foreground hover:text-foreground hover:bg-accent/10',
                  )}
                >
                  <Icono className="h-4 w-4 text-accent-ink" />
                  <span className="flex-1 text-left">{grupo.label}</span>
                  <ChevronRight
                    className={cn(
                      'h-3.5 w-3.5 transition-transform motion-reduce:transition-none',
                      abierto && 'rotate-90',
                    )}
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
                          <SubIcono className="h-[18px] w-[18px] text-accent-ink" />
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

        <div
          className={cn(
            'mt-4 flex flex-col gap-2 border-t border-border pb-3 pt-3',
            contraido ? 'items-center px-1.5' : 'px-3',
          )}
        >
          <button
            type="button"
            onClick={cerrarSesion}
            className={cn(
              'flex items-center gap-2.5 rounded-lg border text-sm text-muted-foreground transition-colors hover:border-destructive/40 hover:bg-destructive/10 hover:text-destructive-fg',
              contraido
                ? 'h-9 w-9 justify-center border-border bg-muted/40'
                : 'border-border bg-muted/40 px-3 py-2.5',
            )}
            title={contraido ? 'Cerrar sesión' : undefined}
            aria-label="Cerrar sesión"
          >
            <LogOut className="h-4 w-4 shrink-0" />
            {!contraido && <span className="flex-1 text-left">Cerrar sesión</span>}
          </button>
          <button
            type="button"
            onClick={() => setContraido((actual) => !actual)}
            className={cn(
              'flex items-center gap-2.5 rounded-lg border border-border text-sm text-muted-foreground transition-colors hover:bg-accent/10 hover:text-foreground',
              contraido ? 'h-9 w-9 justify-center' : 'px-3 py-2.5',
            )}
            title={contraido ? 'Expandir menú lateral' : undefined}
            aria-label="Contraer menú lateral"
          >
            {contraido ? (
              <PanelRight className="h-4 w-4 shrink-0" />
            ) : (
              <PanelLeft className="h-4 w-4 shrink-0" />
            )}
            {!contraido && <span className="flex-1 text-left">Contraer</span>}
          </button>
        </div>
      </aside>

      <div className="flex min-h-0 min-w-0 flex-1 flex-col">
        {/* Barra superior: marca (móvil), búsqueda a la izquierda y tema a la derecha. */}
        <header className="z-20 flex h-16 shrink-0 items-center gap-3 border-b border-border bg-card/95 px-4 backdrop-blur-sm">
          <Marca chipClassName="h-8 w-8 lg:hidden" nombreClassName="lg:hidden" />

          <button
            type="button"
            onClick={() => setPaletteAbierto(true)}
            className="flex h-9 w-9 items-center justify-center rounded-md border border-border text-muted-foreground transition-colors hover:bg-accent/10 hover:text-foreground lg:hidden"
            aria-label="Buscar"
          >
            <Search className="h-4 w-4 text-accent-ink" />
          </button>

          <button
            type="button"
            onClick={() => setPaletteAbierto(true)}
            className="hidden w-full max-w-md items-center gap-2 rounded-lg border border-border bg-background px-3 py-2 text-sm text-muted-foreground transition-colors hover:bg-accent/10 hover:text-foreground lg:ml-5 lg:flex"
          >
            <Search className="h-4 w-4 shrink-0 text-accent-ink" />
            <span className="flex-1 truncate text-left">Buscar…</span>
            <kbd className="rounded border border-border px-1.5 py-0.5 text-[10px] text-muted-foreground">
              ⌘K
            </kbd>
          </button>

          <div className="flex items-center gap-2 lg:ml-auto">
            <button
              type="button"
              onClick={alternarTema}
              className="rounded-md border border-border p-2 text-muted-foreground transition-colors hover:bg-accent/10 hover:text-foreground"
              aria-label="Cambiar tema"
            >
              {esOscuro ? <Sun className="h-4 w-4" /> : <Moon className="h-4 w-4" />}
            </button>
            <button
              type="button"
              onClick={cerrarSesion}
              className="rounded-md border border-border p-2 text-muted-foreground transition-colors hover:text-destructive-fg lg:hidden"
              aria-label="Cerrar sesión"
            >
              <LogOut className="h-4 w-4" />
            </button>
          </div>
        </header>

        <main id="contenido" className="app-scroll relative min-h-0 flex-1 overflow-y-auto p-4 pb-28 lg:p-6">
          {!esEditorSinMigas &&
            !esDetalleSinMigas &&
            !location.pathname.startsWith('/salon/planos/') &&
            !location.pathname.startsWith('/plano') && <Breadcrumbs />}
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
              <Icono className="h-5 w-5 text-accent-ink" />
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
