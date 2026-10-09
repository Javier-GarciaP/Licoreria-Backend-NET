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
export function mensajeDeError(error: unknown): string {
  if (error instanceof ApiError) {
    const campos = Object.values(error.fieldErrors).flat();
    if (campos.length > 0) return campos.join(' ');
    return error.problem.detail || error.problem.title || `Error ${error.status}`;
  }
  if (error instanceof Error) return error.message;
  return 'Ocurrió un error inesperado.';
}