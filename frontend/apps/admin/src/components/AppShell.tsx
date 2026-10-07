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

  return (
    <div className="relative flex h-dvh flex-col overflow-hidden bg-canvas p-4 lg:p-6">
      <a
        href="#contenido"
        className="sr-only focus:not-sr-only focus:absolute focus:left-4 focus:top-4 focus:z-50 focus:rounded-pill focus:bg-accent focus:px-4 focus:py-2 focus:text-sm focus:text-on-pastel"
      >
        Saltar al contenido
      </a>

      {/* Ambiente: luz baja de la paleta actual que da profundidad al vidrio. */}
      <div aria-hidden className="pointer-events-none absolute inset-0 z-0 overflow-hidden">
        <div className="absolute -left-32 -top-40 h-[40rem] w-[40rem] rounded-full bg-accent/20 blur-[130px]" />
      </div>

      <div className="relative z-10 mx-auto flex min-h-0 w-full max-w-[1800px] flex-1 gap-4 lg:gap-6">
        <aside
          aria-label="Navegación principal"
          className="glass-panel hidden w-72 shrink-0 flex-col overflow-hidden rounded-3xl px-4 py-5 lg:flex lg:px-5"
        >
          <div className="flex items-center gap-2.5 px-2">
            <span className="flex h-10 w-10 items-center justify-center rounded-full bg-accent text-on-pastel shadow-glow">
              <Wine size={19} />
            </span>
            <div>
              <p className="text-sm font-medium tracking-tighter2 text-ink">Licorería</p>
              <p className="text-[11px] text-muted">{etiquetaRol(usuario?.rolDominio, usuario?.rol)}</p>
            </div>
          </div>

          <button
            type="button"
            onClick={() => setPaletteAbierto(true)}
            className="mt-5 flex items-center gap-2 rounded-2xl border border-hairline px-3 py-2.5 text-sm text-muted transition hover:bg-ink/5 hover:text-ink"
          >
            <Search size={16} />
            <span className="flex-1 text-left">Buscar…</span>
            <kbd className="rounded border border-hairline px-1.5 py-0.5 text-[10px]">⌘K</kbd>
          </button>

          <nav aria-label="Secciones" className="app-scroll mt-4 flex flex-1 flex-col gap-1 overflow-y-auto pr-1">
            {grupos.map((grupo) => {
              const ItemIcono = grupo.items[0].icon;

              if (grupo.items.length === 1) {
                const item = grupo.items[0];
                return (
                  <NavLink
                    key={grupo.id}
                    to={item.to}
                    end={item.end ?? item.to === '/'}
                    className={({ isActive }) =>
                      cn(
                        'flex items-center gap-3 rounded-2xl px-3 py-2.5 text-sm transition',
                        isActive ? 'bg-accent/20 font-medium text-accent-ink' : 'text-muted hover:bg-ink/5 hover:text-ink',
                      )
                    }
                  >
                    <ItemIcono size={18} />
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
                      'flex items-center gap-2.5 rounded-2xl px-3 py-2.5 text-sm font-medium transition',
                      activo ? 'text-ink' : 'text-muted hover:text-ink',
                    )}
                  >
                    <Icono size={16} />
                    <span className="flex-1 text-left">{grupo.label}</span>
                    <ChevronRight
                      size={14}
                      className={cn('transition-transform motion-reduce:transition-none', abierto && 'rotate-90')}
                    />
                  </button>
                  {abierto && (
                    <div className="ml-3 mt-1 flex flex-col gap-0.5 border-l border-hairline/60 pl-3">
                      {grupo.items.map((item) => {
                        const SubIcono = item.icon;
                        return (
                          <NavLink
                            key={item.to}
                            to={item.to}
                            end={item.end ?? item.to === '/'}
                            className={({ isActive }) =>
                              cn(
                                'flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm transition',
                                isActive
                                  ? 'bg-accent/20 font-medium text-accent-ink'
                                  : 'text-muted hover:bg-ink/5 hover:text-ink',
                              )
                            }
                          >
                            <SubIcono size={18} />
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

          <div className="mt-4 flex flex-col gap-1.5 border-t border-hairline pt-4">
            <button
              onClick={alternarTema}
              className="flex items-center gap-3 rounded-2xl px-3 py-2.5 text-sm text-muted transition hover:bg-ink/5 hover:text-ink"
            >
              {esOscuro ? <Sun size={18} /> : <Moon size={18} />}
              Tema {esOscuro ? 'Claro' : 'Oscuro'}
            </button>
            <div className="flex items-center justify-between rounded-2xl bg-ink/5 px-3 py-2.5">
              <div className="min-w-0">
                <p className="truncate text-xs font-medium text-ink">{usuario?.nombreCompleto}</p>
                <p className="truncate text-[11px] text-muted">{etiquetaRol(usuario?.rolDominio, usuario?.rol)}</p>
              </div>
              <button onClick={cerrarSesion} className="text-muted transition hover:text-danger-ink" aria-label="Cerrar sesión">
                <LogOut size={16} />
              </button>
            </div>
          </div>
        </aside>

        <div className="flex min-h-0 min-w-0 flex-1 flex-col gap-4 lg:gap-6">
          <header className="glass-bar z-20 flex items-center justify-between gap-3 rounded-3xl px-4 py-3 lg:hidden">
            <div className="flex items-center gap-2">
              <span className="flex h-8 w-8 items-center justify-center rounded-full bg-accent text-on-pastel">
                <Wine size={16} />
              </span>
              <p className="text-sm font-medium text-ink">Licorería</p>
            </div>
            <div className="flex items-center gap-2">
              <button
                onClick={alternarTema}
                className="rounded-pill border border-hairline p-2 text-muted transition hover:text-ink"
                aria-label="Cambiar tema"
              >
                {esOscuro ? <Sun size={16} /> : <Moon size={16} />}
              </button>
              <button
                onClick={cerrarSesion}
                className="rounded-pill border border-hairline p-2 text-muted transition hover:text-danger-ink"
                aria-label="Cerrar sesión"
              >
                <LogOut size={16} />
              </button>
            </div>
          </header>

          <main
            id="contenido"
            className="glass-panel app-scroll relative min-h-0 flex-1 overflow-y-auto rounded-3xl p-4 pb-28 lg:p-6"
          >
            {!location.pathname.startsWith('/salon/planos/') && <Breadcrumbs />}
            {children}
          </main>
        </div>
      </div>

      <nav
        aria-label="Navegación rápida"
        className="glass-bar fixed inset-x-4 bottom-4 z-30 flex items-center gap-0.5 rounded-3xl px-2 py-2 lg:hidden"
      >
        {primarios.map((item) => {
          const Icono = item.icon;
          return (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.end ?? item.to === '/'}
              className={({ isActive }) =>
                cn(
                  'flex min-w-0 flex-1 flex-col items-center gap-1 rounded-2xl px-1.5 py-1.5 text-[10px] transition',
                  isActive ? 'bg-accent/20 font-medium text-accent-ink' : 'text-muted',
                )
              }
            >
              <Icono size={20} />
              <span className="w-full truncate text-center">{item.corto ?? item.label}</span>
            </NavLink>
          );
        })}
        <button
          type="button"
          onClick={() => setSheetAbierto(true)}
          className="flex min-w-0 flex-1 flex-col items-center gap-1 rounded-2xl px-1.5 py-1.5 text-[10px] text-muted transition hover:text-ink"
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
