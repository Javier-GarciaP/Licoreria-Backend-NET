import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { server } from '../test/server';
import { ComprasPage } from './ComprasPage';

const BASE = 'http://localhost:5190';
const URL_LISTA = `${BASE}/api/v1/ordenes-compra`;

const pagina = <T,>(items: T[]) => ({ items, page: 1, pageSize: 15, totalItems: items.length, totalPages: 1 });
const LISTA = pagina([
  { id: 'o-1', numero: 'OC-0001', proveedorId: 'p-1', proveedorNombre: 'Distribuidora Sur', fecha: '2026-10-14T12:00:00Z', estado: 'Aprobada', observaciones: null, totalUSD: 120, detalles: [] },
]);
const VACIA = pagina([]);

function renderPagina() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <ComprasPage />
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe('ComprasPage', () => {
  it('lista los registros', async () => {
    server.use(http.get(URL_LISTA, () => HttpResponse.json(LISTA)));
    renderPagina();

    expect(await screen.findByText('OC-0001')).toBeInTheDocument();
  });

  it('muestra el estado vacío', async () => {
    server.use(http.get(URL_LISTA, () => HttpResponse.json(VACIA)));
    renderPagina();

    expect(await screen.findByText('No hay órdenes.')).toBeInTheDocument();
  });

  it('no rompe cuando la carga falla', async () => {
    server.use(http.get(URL_LISTA, () => HttpResponse.json({ title: 'Error' }, { status: 500 })));
    renderPagina();

    expect(await screen.findByText('No hay órdenes.')).toBeInTheDocument();
  });
});
