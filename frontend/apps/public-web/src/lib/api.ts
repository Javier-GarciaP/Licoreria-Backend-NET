import { configureApi } from '@licoreria/api-client';

configureApi({
  baseUrl: (import.meta.env.VITE_API_URL as string | undefined) ?? 'http://localhost:5190',
  getAccessToken: () => null,
  getRefreshToken: () => null,
  refresh: async () => null,
});
