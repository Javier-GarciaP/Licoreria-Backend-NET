import { useLocation } from 'react-router-dom';
import { UiBreadcrumb } from '@licoreria/ui';
import { grupoDeItem, itemDeRuta } from '../lib/navigation';

/** Ruta "Grupo / Página" según el modelo de navegación. */
export function Breadcrumbs() {
  const { pathname } = useLocation();
  const item = itemDeRuta(pathname);
  const grupo = grupoDeItem(item);

  if (!item || !grupo) return null;

  return (
    <UiBreadcrumb
      className="mb-4"
      items={[{ label: grupo.label }, { label: item.label }]}
    />
  );
}