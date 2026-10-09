import { setupServer } from 'msw/node';
import { http, HttpResponse } from 'msw';

const BASE = 'http://localhost:5190';

export const handlers = [
  http.post(`${BASE}/api/auth/login`, async ({ request }) => {
    const body = (await request.json()) as { username?: string; password?: string };
    if (body.username === 'admin@licoreria.com' && body.password === 'demo123') {
      return HttpResponse.json({
        usuarioId: '1',
        nombreCompleto: 'Administrador',
        email: 'admin@licoreria.com',
        rol: 'Admin',
        rolDominio: 'Administrador',
        permisos: ['sales:read', 'sales:write'],
        accessToken: 'test-access-token',
        refreshToken: 'test-refresh-token',
        expiraEn: new Date().toISOString(),
      });
    }
    return HttpResponse.json(
      { title: 'No autorizado', status: 401, detail: 'Credenciales inválidas.' },
      { status: 401 },
    );
  }),
  http.post(`${BASE}/api/auth/refresh`, () =>
    HttpResponse.json({ title: 'No autorizado', status: 401 }, { status: 401 }),
  ),
  http.get(`${BASE}/api/v1/stock`, () =>
    HttpResponse.json({
      items: [
        {
          varianteId: 'variante-1',
          sku: 'SKU-1',
          productoNombre: 'Ron Añejo',
          varianteNombre: '750ml',
          cantidad: 5,
          cantidadReservada: 0,
          stockMinimo: 1,
          stockMaximo: 20,
          bajoMinimo: false,
        },
      ],
      page: 1,
      pageSize: 100,
      totalItems: 1,
      totalPages: 1,
    }),
  ),
  http.get(`${BASE}/api/v1/usuarios`, () =>
    HttpResponse.json({ items: [], page: 1, pageSize: 15, totalItems: 0, totalPages: 0 }),
  ),
  http.get(`${BASE}/api/v1/mermas`, () =>
    HttpResponse.json({ items: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 }),
  ),
];

export const server = setupServer(...handlers);
