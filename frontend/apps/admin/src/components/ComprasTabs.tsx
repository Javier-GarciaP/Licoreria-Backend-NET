import { FolderTabs, type FolderTab } from './FolderTabs';

const TABS: FolderTab[] = [
  { to: '/compras', label: 'Órdenes', end: true },
  { to: '/compras/proveedores', label: 'Proveedores' },
  { to: '/compras/recepciones', label: 'Recepciones' },
  { to: '/compras/cuentas', label: 'Cuentas por pagar' },
];

export function ComprasTabs() {
  return <FolderTabs tabs={TABS} ariaLabel="Secciones de compras" />;
}