import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { authApi } from '@licoreria/api-client';
import { tokens } from '../lib/api';
import { inicioDeRol } from '../lib/roles';

const AuthContext = createContext(null);

/**
 * Centraliza la sesión: guarda el JWT en localStorage, restaura el usuario con
 * GET /api/auth/me y expone los permisos para los guards de RBAC.
 */
export function AuthProvider({ children }) {
  const [usuario, setUsuario] = useState(null);
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
        if (activo) setUsuario(me);
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

  const login = useCallback(async (username, password) => {
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

  const value = useMemo(
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
      tienePermiso: (clave) => Boolean(usuario?.permisos?.includes(clave)),
      tieneAlguno: (claves) => claves.some((clave) => Boolean(usuario?.permisos?.includes(clave))),
    }),
    [usuario, inicializando, login, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const contexto = useContext(AuthContext);
  if (!contexto) throw new Error('useAuth debe usarse dentro de AuthProvider');
  return contexto;
}
