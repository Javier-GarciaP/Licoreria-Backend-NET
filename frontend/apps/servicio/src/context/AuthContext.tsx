import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import type { AuthResponse } from '@licoreria/types';
import { authApi } from '@licoreria/api-client';
import { tokens } from '../lib/api';
import { inicioDeRol } from '../lib/roles';

interface UsuarioSesion {
  usuarioId: string;
  nombreCompleto: string;
  email: string;
  rol: string;
  rolDominio: string;
  permisos: string[];
}

interface AuthValue {
  usuario: UsuarioSesion | null;
  autenticado: boolean;
  inicializando: boolean;
  login: (username: string, password: string) => Promise<AuthResponse>;
  logout: () => Promise<void>;
  rolDominio?: string;
  inicio: string;
  esAdmin: boolean;
  tienePermiso: (clave: string) => boolean;
}

const AuthContext = createContext<AuthValue | null>(null);

/** Sesión del app de servicio: JWT, usuario y permisos desde la misma API. */
export function AuthProvider({ children }: { children: React.ReactNode }) {
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

  const value = useMemo<AuthValue>(
    () => ({
      usuario,
      autenticado: Boolean(usuario),
      inicializando,
      login,
      logout,
      rolDominio: usuario?.rolDominio,
      inicio: inicioDeRol(usuario?.rolDominio),
      esAdmin: usuario?.rol === 'Admin' || usuario?.rolDominio === 'Administrador',
      tienePermiso: (clave) => Boolean(usuario?.permisos?.includes(clave)),
    }),
    [usuario, inicializando, login, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthValue {
  const contexto = useContext(AuthContext);
  if (!contexto) throw new Error('useAuth debe usarse dentro de AuthProvider');
  return contexto;
}
