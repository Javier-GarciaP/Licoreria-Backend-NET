import { configureApi, ApiError } from '@licoreria/api-client';

const ACCESS_KEY = 'licoreria.accessToken';
const REFRESH_KEY = 'licoreria.refreshToken';

const baseUrl = (import.meta.env.VITE_API_URL as string | undefined) ?? 'http://localhost:5190';

/** URL base de la API (para resolver rutas relativas de archivos subidos). */
export const API_BASE_URL = baseUrl;

export const tokens = {
  access: () => localStorage.getItem(ACCESS_KEY),
  refresh: () => localStorage.getItem(REFRESH_KEY),
  set(access: string, refresh: string) {
    localStorage.setItem(ACCESS_KEY, access);
    localStorage.setItem(REFRESH_KEY, refresh);
  },
  clear() {
    localStorage.removeItem(ACCESS_KEY);
    localStorage.removeItem(REFRESH_KEY);
  },
};

let refreshing: Promise<string | null> | null = null;

async function performRefresh(): Promise<string | null> {
  const refreshToken = tokens.refresh();
  if (!refreshToken) return null;

  if (refreshing) return refreshing;

  refreshing = (async () => {
    try {
      const response = await fetch(`${baseUrl}/api/auth/refresh`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken }),
      });

      if (!response.ok) {
        tokens.clear();
        return null;
      }

      const data = (await response.json()) as { accessToken: string; refreshToken: string };
      tokens.set(data.accessToken, data.refreshToken);
      return data.accessToken;
    } catch {
      return null;
    } finally {
      refreshing = null;
    }
  })();

  return refreshing;
}

function expiraEn(token: string): number | null {
  try {
    const carga = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/'))) as { exp?: number };
    return typeof carga.exp === 'number' ? carga.exp * 1000 : null;
  } catch {
    return null;
  }
}

/**
 * Token de acceso listo para usar fuera de `apiFetch` (p. ej. SignalR): si venció o está por vencer
 * lo renueva con el refresh token. Devuelve cadena vacía si no hay sesión.
 */
export async function obtenerAccessTokenVigente(): Promise<string> {
  const actual = tokens.access();
  if (actual) {
    const vence = expiraEn(actual);
    if (vence === null || vence - Date.now() > 30_000) return actual;
  }
  return (await performRefresh()) ?? tokens.access() ?? '';
}

configureApi({
  baseUrl,
  getAccessToken: tokens.access,
  getRefreshToken: tokens.refresh,
  refresh: performRefresh,
  onUnauthorized: () => {
    tokens.clear();
    if (!window.location.pathname.startsWith('/login')) {
      window.location.assign('/login');
    }
  },
});

export { ApiError };

/** Mensaje amigable a partir de un error (RFC 7807 o red). */
/**
 * Si el error es una carrera de concurrencia (409) o un producto agotado o sin abrir (422), devuelve el título
 * con el que avisar al usuario; las interfaces deben refrescar sus datos y conservar lo que se estaba
 * haciendo. Devuelve null para cualquier otro error.
 */
export function describirConcurrencia(error: unknown): { titulo: string } | null {
  if (!(error instanceof ApiError)) return null;
  if (error.esPorcionesSinAbrir) return { titulo: 'Falta destapar una botella' };
  if (error.esExistenciaAgotada) return { titulo: 'Producto agotado' };
  if (error.esConflicto) return { titulo: 'Los datos cambiaron mientras trabajabas' };
  return null;
}

export function mensajeDeError(error: unknown): string {
  if (error instanceof ApiError) {
    if (error.esPorcionesSinAbrir) {
      const { producto, requerido, disponible, envasesCerrados } = error.problem;
      const cerradas = envasesCerrados ?? 0;
      const botellas = cerradas > 0 ? ` Hay ${cerradas} ${cerradas === 1 ? 'botella cerrada' : 'botellas cerradas'}.` : '';
      const nombre = producto ?? 'este producto';
      // Algunas porciones siguen abiertas pero no alcanzan para lo pedido.
      if ((disponible ?? 0) > 0) {
        const quedan = disponible === 1 ? 'Solo queda 1 porción abierta' : `Solo quedan ${disponible} porciones abiertas`;
        return `${quedan} de ${nombre} y se piden ${requerido ?? '?'}: pide a barra que destape otra botella.${botellas}`;
      }
      return `No hay porciones abiertas de ${nombre}: pide a barra que destape una botella.${botellas}`;
    }
    if (error.esExistenciaAgotada) {
      const { producto, requerido, disponible } = error.problem;
      return `Se agotó ${producto ?? 'un producto'}: se piden ${requerido ?? '?'} y quedan ${disponible ?? 0}.`;
    }
    if (error.esConflicto) {
      return 'Otra operación cambió estos datos al mismo tiempo. Se actualizó la información: revísala e inténtalo de nuevo.';
    }
    const campos = Object.values(error.fieldErrors).flat();
    if (campos.length > 0) return campos.join(' ');
    return error.problem.detail || error.problem.title || `Error ${error.status}`;
  }
  if (error instanceof Error) return error.message;
  return 'Ocurrió un error inesperado.';
}