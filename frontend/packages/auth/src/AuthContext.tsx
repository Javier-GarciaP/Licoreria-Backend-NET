import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import type { ReactNode } from 'react';
import type { AuthResponse } from '@licoreria/types';
import { authApi } from '@licoreria/api-client';
import { tokens } from './api';
import { inicioDeRol } from './roles';

export interface UsuarioSesion {
  usuarioId: string;
  nombreCompleto: string;
  email: string;
  rol: string;
  rolDominio: string;
  permisos: string[];
}

export interface AuthValue {
  usuario: UsuarioSesion | null;
  autenticado: boolean;
  inicializando: boolean;
  login: (username: string, password: string) => Promise<AuthResponse>;
  logout: () => Promise<void>;
  rolDominio?: string;
  inicio: string;
  esAdmin: boolean;
  tienePermiso: (clave: string) => boolean;
  tieneAlguno: (claves: string[]) => boolean;
}

const AuthContext = createContext<AuthValue | null>(null);

/**
 * Centraliza la sesión: guarda el JWT en localStorage, restaura el usuario con
 * GET /api/auth/me y expone los permisos para los guards de RBAC.
 * Compartido por los apps de administración y servicio.
 */
export function AuthProvider({ children }: { children: ReactNode }) {
  const [usuario, setUsuario] = useState<UsuarioSesion | null>(null);
  const [inicializando, setInicializando] = useState(true);

  useEffect(() => {
    let activo = true;

    (async () => {
      if (!tokens.access()) {
        setInicializando(false);
        return;
      }

      try {
        const me = await authApi.me();
        if (activo) {
          setUsuario({
            usuarioId: me.id,
            nombreCompleto: me.nombreCompleto,
            email: me.email,
            rol: me.rol,
            rolDominio: me.rolDominio,
            permisos: me.permisos,
          });
        }
      } catch {
        tokens.clear();
      } finally {
        if (activo) setInicializando(false);
      }
    })();

    return () => {
      activo = false;
    };
  }, []);

  const login = useCallback(async (username: string, password: string) => {
    const respuesta = await authApi.login({ username, password });
    tokens.set(respuesta.accessToken, respuesta.refreshToken);
    setUsuario({
      usuarioId: respuesta.usuarioId,
      nombreCompleto: respuesta.nombreCompleto,
      email: respuesta.email,
      rol: respuesta.rol,
      rolDominio: respuesta.rolDominio,
      permisos: respuesta.permisos,
    });
    return respuesta;
  }, []);

  const logout = useCallback(async () => {
    const refreshToken = tokens.refresh();
    try {
      if (refreshToken) await authApi.logout(refreshToken);
    } catch {
      /* El cierre local siempre debe completarse. */
    }
    tokens.clear();
    setUsuario(null);
  }, []);

  const tienePermiso = useCallback((clave: string) => Boolean(usuario?.permisos?.includes(clave)), [usuario]);

  const value = useMemo<AuthValue>(
    () => ({
      usuario,
      autenticado: Boolean(usuario),
      inicializando,
      login,
      logout,
      rolDominio: usuario?.rolDominio,
      inicio: inicioDeRol(usuario?.rolDominio),
      esAdmin:
        usuario?.rolDominio === 'Administrador' || usuario?.rol === 'Admin' || usuario?.rol === 'Administrador',
      tienePermiso,
      tieneAlguno: (claves) => claves.some((clave) => Boolean(usuario?.permisos?.includes(clave))),
    }),
    [usuario, inicializando, login, logout, tienePermiso],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthValue {
  const contexto = useContext(AuthContext);
  if (!contexto) throw new Error('useAuth debe usarse dentro de AuthProvider');
  return contexto;
}