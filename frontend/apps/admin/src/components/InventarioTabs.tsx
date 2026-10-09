import { FolderTabs, type FolderTab } from './FolderTabs';

const TABS: FolderTab[] = [{ to: '/inventario/kardex', label: 'Kardex' }];

export function InventarioTabs() {
  return <FolderTabs tabs={TABS} ariaLabel="Secciones de inventario" />;
}