import { FolderTabs, type FolderTab } from './FolderTabs';

const TABS: FolderTab[] = [
  { to: '/productos', label: 'Productos', end: true },
  { to: '/catalogos', label: 'Categorías y marcas' },
  { to: '/catalogos-avanzado', label: 'Unidades, impuestos y listas' },
];

/** Sub-navegación del catálogo unificado (estilo carpeta). */
export function CatalogoTabs() {
  return <FolderTabs tabs={TABS} ariaLabel="Secciones del catálogo" />;
}