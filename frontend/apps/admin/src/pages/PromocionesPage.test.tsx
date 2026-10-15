import { render, screen } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { http, HttpResponse } from 'msw';
import { describe, expect, it, vi } from 'vitest';
import { server } from '../test/server';
import { PromocionesPage } from './PromocionesPage';

const sesion = { esAdmin: false, permisos: [] as string[] };
vi.mock('../context/AuthContext', () => ({
  useAuth: () => ({
    esAdmin: sesion.esAdmin,
    tienePermiso: (clave: string) => sesion.permisos.includes(clave),
  }),
}));

const BASE = 'http://localhost:5190';
const PROMO = {
  id: 'p1',
  nombre: 'Happy Hour',
  tipo: 'Porcentaje',
  valor: 15,
  activo: true,
  fechaInicio: null,
  fechaFin: null,
};

function renderPagina() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <PromocionesPage />
    </QueryClientProvider>,
  );
}

describe('PromocionesPage', () => {
  it('lista las promociones y permite editar a quien tiene promotions:manage', async () => {
    sesion.esAdmin = false;
    sesion.permisos = ['promotions:manage'];
    server.use(http.get(`${BASE}/api/v1/promociones`, () => HttpResponse.json([PROMO])));
    renderPagina();

    expect(await screen.findByText('Happy Hour')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Nueva promoción' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Acciones de Happy Hour' })).toBeInTheDocument();
  });

  it('oculta las acciones de edición a quien no tiene promotions:manage', async () => {
    sesion.esAdmin = false;
    sesion.permisos = ['sales:read'];
    server.use(http.get(`${BASE}/api/v1/promociones`, () => HttpResponse.json([PROMO])));
    renderPagina();

    expect(await screen.findByText('Happy Hour')).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Nueva promoción' })).toBeNull();
    expect(screen.queryByRole('button', { name: 'Acciones de Happy Hour' })).toBeNull();
  });

  it('muestra el estado vacío', async () => {
    sesion.esAdmin = true;
    server.use(http.get(`${BASE}/api/v1/promociones`, () => HttpResponse.json([])));
    renderPagina();

    expect(await screen.findByText('No hay promociones.')).toBeInTheDocument();
  });

  it('no rompe cuando la carga falla', async () => {
    sesion.esAdmin = true;
    server.use(http.get(`${BASE}/api/v1/promociones`, () => HttpResponse.json({ title: 'Error' }, { status: 500 })));
    renderPagina();

    expect(await screen.findByText('No hay promociones.')).toBeInTheDocument();
  });
});
