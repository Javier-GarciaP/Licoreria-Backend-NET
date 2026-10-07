import type { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { Spinner } from '@licoreria/ui';
import { useAuth } from '../context/AuthContext';

function PantallaCarga() {
  return (
    <div className="flex h-dvh items-center justify-center">
      <Spinner />
    </div>
  );
}

/** Exige sesión (y opcionalmente permisos) antes de renderizar las rutas hijas. */
export function ProtectedRoute({
  children,
  permisos,
  admin = false,
}: {
  children: ReactNode;
  permisos?: string[];
  admin?: boolean;
}) {
  const { autenticado, inicializando, esAdmin, tieneAlguno } = useAuth() as {
    autenticado: boolean;
    inicializando: boolean;
    esAdmin: boolean;
    tieneAlguno: (claves: string[]) => boolean;
  };
  const location = useLocation();

  if (inicializando) return <PantallaCarga />;
  if (!autenticado) return <Navigate to="/login" state={{ from: location }} replace />;
  if (admin && !esAdmin) return <Navigate to="/" replace />;
  if (permisos && permisos.length > 0 && !esAdmin && !tieneAlguno(permisos)) {
    return <Navigate to="/" replace />;
  }

  return <>{children}</>;
}

/** Oculta acciones a las que el usuario no tiene permiso. */
export function Can({ permiso, permisos, children }: { permiso?: string; permisos?: string[]; children: ReactNode }) {
  const { esAdmin, tienePermiso, tieneAlguno } = useAuth() as {
    esAdmin: boolean;
    tienePermiso: (clave: string) => boolean;
    tieneAlguno: (claves: string[]) => boolean;
  };

  if (esAdmin) return <>{children}</>;
  if (permiso && !tienePermiso(permiso)) return null;
  if (permisos && permisos.length > 0 && !tieneAlguno(permisos)) return null;
  return <>{children}</>;
}
