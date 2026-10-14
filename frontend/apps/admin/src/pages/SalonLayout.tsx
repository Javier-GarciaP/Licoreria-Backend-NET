import { Outlet } from 'react-router-dom';
import { FolderPanel, FolderTabs, type FolderTab } from '../components/FolderTabs';
import { useAuth } from '../context/AuthContext';

const TABS: FolderTab[] = [
  { to: '/salon', label: 'Planos', end: true },
  { to: '/salon/mesas', label: 'Mesas' },
];

/** Salón como sistema de carpetas: pestañas sobre un panel que contiene el contenido. */
export function SalonLayout() {
  const { esAdmin, tienePermiso } = useAuth() as { esAdmin: boolean; tienePermiso: (clave: string) => boolean };
  // La pestaña de mesas edita la distribución: solo para quien tiene `club:layout`.
  const tabs = esAdmin || tienePermiso('club:layout') ? TABS : TABS.filter((tab) => tab.to === '/salon');
  return (
    <div className="mx-auto flex max-w-page flex-col">
      <FolderTabs tabs={tabs} ariaLabel="Secciones del salón" />
      <FolderPanel>
        <Outlet />
      </FolderPanel>
    </div>
  );
}