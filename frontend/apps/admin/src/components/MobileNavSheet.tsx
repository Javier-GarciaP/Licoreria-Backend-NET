import { NavLink } from 'react-router-dom';
import { cn, Sheet, SheetContent, SheetOverlay, SheetPortal, SheetTitle } from '@licoreria/ui';
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
  return (
    <Sheet
      open={open}
      onOpenChange={(abierto) => {
        if (!abierto) onClose();
      }}
    >
      <SheetPortal>
        <SheetOverlay className="fixed inset-0 z-40 bg-black/50 lg:hidden" />
        <SheetContent
          aria-label="Navegación"
          className="fixed inset-x-0 bottom-0 z-40 max-h-[80dvh] overflow-y-auto rounded-t-2xl border-t border-border bg-card p-4 pb-6 shadow-lg app-scroll lg:hidden"
        >
          <SheetTitle className="sr-only">Navegación</SheetTitle>
          <div className="mx-auto mb-4 h-1 w-10 rounded-pill bg-border" />
          {grupos.map((grupo) => {
            const Icono = grupo.icon;
            return (
              <section key={grupo.id} className="mb-5">
                <p className="mb-1 flex items-center gap-2 px-2 text-xs font-medium text-muted-foreground">
                  <Icono className="h-3.5 w-3.5 text-accent-ink" />
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
                            'flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm transition-colors',
                            isActive
                              ? 'bg-primary/15 font-medium text-foreground'
                              : 'text-muted-foreground hover:bg-accent/10 hover:text-foreground',
                          )
                        }
                      >
                        <ItemIcono className="h-[18px] w-[18px] text-accent-ink" />
                        {item.label}
                      </NavLink>
                    );
                  })}
                </div>
              </section>
            );
          })}
        </SheetContent>
      </SheetPortal>
    </Sheet>
  );
}
