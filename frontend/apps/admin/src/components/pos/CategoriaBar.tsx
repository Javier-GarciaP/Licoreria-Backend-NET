import type { Categoria } from '@licoreria/types';
import { cn } from '@licoreria/ui';

export function CategoriaBar({
  categorias,
  activa,
  onCambiar,
}: {
  categorias: Categoria[];
  activa: string | null;
  onCambiar: (id: string | null) => void;
}) {
  const items = [{ id: null as string | null, nombre: 'Todas' }, ...categorias.map((c) => ({ id: c.id, nombre: c.nombre }))];

  return (
    <div
      role="tablist"
      aria-label="Categorías"
      className="app-scroll -mx-1 flex gap-2 overflow-x-auto px-1 pb-1"
    >
      {items.map((item) => {
        const seleccionada = activa === item.id;
        return (
          <button
            key={item.id ?? 'todas'}
            type="button"
            role="tab"
            aria-selected={seleccionada}
            onClick={() => onCambiar(item.id)}
            className={cn(
              'flex h-10 shrink-0 items-center whitespace-nowrap rounded-full px-4 text-sm font-medium transition',
              seleccionada
                ? 'bg-primary text-primary-foreground'
                : 'border border-border bg-muted/50 text-muted-foreground hover:text-foreground',
            )}
          >
            {item.nombre}
          </button>
        );
      })}
    </div>
  );
}
