import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { http, HttpResponse } from 'msw';
import { describe, expect, it, vi } from 'vitest';
import { server } from '../test/server';
import { InventarioPage } from './InventarioPage';

vi.mock('../context/AuthContext', () => ({
  useAuth: () => ({
    esAdmin: true,
    tienePermiso: () => true,
    tieneAlguno: () => true,
    rolDominio: 'Administrador',
  }),
}));

const BASE = 'http://localhost:5190';
const URL_LISTA = `${BASE}/api/v1/stock`;

const pagina = <T,>(items: T[]) => ({ items, page: 1, pageSize: 15, totalItems: items.length, totalPages: 1 });
const LISTA = pagina([
  { varianteId: 'v-1', sku: 'RO-750', productoNombre: 'Ron Añejo', varianteNombre: '750ml', cantidad: 5, cantidadReservada: 0, stockMinimo: 1, stockMaximo: 20, bajoMinimo: false },
]);
const VACIA = pagina([]);

function renderPagina() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <InventarioPage />
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe('InventarioPage', () => {
  it('lista los registros', async () => {
    server.use(http.get(URL_LISTA, () => HttpResponse.json(LISTA)));
    renderPagina();

    expect(await screen.findByText('Ron Añejo')).toBeInTheDocument();
  });

  it('muestra el estado vacío', async () => {
    server.use(http.get(URL_LISTA, () => HttpResponse.json(VACIA)));
    renderPagina();

    expect(await screen.findByText('Sin existencias.')).toBeInTheDocument();
  });

  it('no rompe cuando la carga falla', async () => {
    server.use(http.get(URL_LISTA, () => HttpResponse.json({ title: 'Error' }, { status: 500 })));
    renderPagina();

    expect(await screen.findByText('Sin existencias.')).toBeInTheDocument();
  });
});
