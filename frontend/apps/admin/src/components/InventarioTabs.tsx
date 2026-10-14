import { FolderTabs, type FolderTab } from './FolderTabs';

const TABS: FolderTab[] = [
  { to: '/inventario', label: 'Existencias', end: true },
  { to: '/inventario/kardex', label: 'Kardex' },
  { to: '/inventario/envases', label: 'Envases abiertos' },
];

export function InventarioTabs() {
  return <FolderTabs tabs={TABS} ariaLabel="Secciones de inventario" />;
}