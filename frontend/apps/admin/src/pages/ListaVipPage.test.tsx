import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { server } from '../test/server';
import { ListaVipPage } from './ListaVipPage';

const BASE = 'http://localhost:5190';
const URL_LISTA = `${BASE}/api/v1/lista-vip`;

const LISTA = [{ id: 'l-1', clienteId: null, nombre: 'Carla Mora', documento: 'V-999', telefono: null, notas: null, activo: true }];
const VACIA: unknown[] = [];

function renderPagina() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <ListaVipPage />
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe('ListaVipPage', () => {
  it('lista los registros', async () => {
    server.use(http.get(URL_LISTA, () => HttpResponse.json(LISTA)));
    renderPagina();

    expect(await screen.findByText('Carla Mora')).toBeInTheDocument();
  });

  it('muestra el estado vacío', async () => {
    server.use(http.get(URL_LISTA, () => HttpResponse.json(VACIA)));
    renderPagina();

    expect(await screen.findByText('La lista VIP está vacía.')).toBeInTheDocument();
  });

  it('no rompe cuando la carga falla', async () => {
    server.use(http.get(URL_LISTA, () => HttpResponse.json({ title: 'Error' }, { status: 500 })));
    renderPagina();

    expect(await screen.findByText('La lista VIP está vacía.')).toBeInTheDocument();
  });
});
