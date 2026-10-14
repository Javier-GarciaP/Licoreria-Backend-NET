import { configureApi } from '@licoreria/api-client';

const baseUrl = (import.meta.env.VITE_API_URL as string | undefined) ?? 'http://localhost:5190';

configureApi({
  baseUrl,
  getAccessToken: () => null,
  getRefreshToken: () => null,
  refresh: async () => null,
});

/** Origen del API, usado para resolver URLs relativas de imágenes y archivos. */
export const API_BASE_URL = baseUrl;
