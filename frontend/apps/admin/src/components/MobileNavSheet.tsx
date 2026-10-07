import { NavLink } from 'react-router-dom';
import { cn, useFocusTrap } from '@licoreria/ui';
import type { NavGroup } from '../lib/navigation';

export function MobileNavSheet({
  open,
  onClose,
  grupos,
}: {
  open: boolean;
  onClose: () => void;
  grupos: NavGroup[];
}) {
  const sheetRef = useFocusTrap<HTMLDivElement>(open, onClose);

  if (!open) return null;

  return (
    <div className="fixed inset-0 z-40 flex items-end bg-black/40 lg:hidden" role="presentation" onClick={onClose}>
      <div
        ref={sheetRef}
        role="dialog"
        aria-modal="true"
        aria-label="Navegación"
        tabIndex={-1}
        onClick={(evento) => evento.stopPropagation()}
        className="glass-panel app-scroll max-h-[80dvh] w-full overflow-y-auto rounded-t-3xl px-4 pb-24 pt-3 focus:outline-none"
      >
        <div className="mx-auto mb-4 h-1 w-10 rounded-pill bg-hairline" />
        {grupos.map((grupo) => {
          const Icono = grupo.icon;
          return (
            <section key={grupo.id} className="mb-5">
              <p className="mb-1 flex items-center gap-2 px-2 text-xs font-medium text-muted">
                <Icono size={14} />
                {grupo.label}
              </p>
              <div className="flex flex-col">
                {grupo.items.map((item) => {
                  const ItemIcono = item.icon;
                  return (
                    <NavLink
                      key={item.to}
                      to={item.to}
                      end={item.end ?? item.to === '/'}
                      onClick={onClose}
                      className={({ isActive }) =>
                        cn(
                          'flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm transition',
                          isActive ? 'bg-accent/20 font-medium text-accent-ink' : 'text-muted hover:bg-ink/5 hover:text-ink',
                        )
                      }
                    >
                      <ItemIcono size={18} />
                      {item.label}
                    </NavLink>
                  );
                })}
              </div>
            </section>
          );
        })}
      </div>
    </div>
  );
}
