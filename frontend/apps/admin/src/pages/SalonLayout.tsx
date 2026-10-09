import { Outlet } from 'react-router-dom';
import { FolderPanel, FolderTabs, type FolderTab } from '../components/FolderTabs';

const TABS: FolderTab[] = [
  { to: '/salon', label: 'Planos', end: true },
  { to: '/salon/mesas', label: 'Mesas' },
];

/** Salón como sistema de carpetas: pestañas sobre un panel que contiene el contenido. */
export function SalonLayout() {
  return (
    <div className="mx-auto flex max-w-page flex-col">
      <FolderTabs tabs={TABS} ariaLabel="Secciones del salón" />
      <FolderPanel>
        <Outlet />
      </FolderPanel>
    </div>
  );
}