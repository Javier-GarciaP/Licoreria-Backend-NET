import { useLocation } from 'react-router-dom';
import { grupoDeItem, itemDeRuta } from '../lib/navigation';

/** Ruta "Grupo / Página" según el modelo de navegación. */
export function Breadcrumbs() {
  const { pathname } = useLocation();
  const item = itemDeRuta(pathname);
  const grupo = grupoDeItem(item);

  if (!item || !grupo) return null;

  return (
    <nav aria-label="Ruta actual" className="mb-4 flex items-center gap-1.5 text-xs text-muted">
      <span>{grupo.label}</span>
      <span aria-hidden className="text-stone">
        /
      </span>
      <span className="text-ink">{item.label}</span>
    </nav>
  );
}
