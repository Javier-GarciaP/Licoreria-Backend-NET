import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { server } from '../test/server';
import { EntradasPage } from './EntradasPage';

const BASE = 'http://localhost:5190';
const URL_LISTA = `${BASE}/api/v1/entradas`;

const pagina = <T,>(items: T[]) => ({ items, page: 1, pageSize: 20, totalItems: items.length, totalPages: 1 });

const LISTA = pagina([
  { id: 'e-1', codigo: 'ENT-0001', eventoId: null, eventoTitulo: 'Noche de Salsa', reservaId: null, clienteId: null, precio: 10, moneda: 'USD', estado: 'Emitida', emitidaEn: '2026-10-14T20:00:00Z', usadaEn: null },
]);
const VACIA = pagina([]);

function renderPagina() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <EntradasPage />
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe('EntradasPage', () => {
  it('lista los registros', async () => {
    server.use(http.get(URL_LISTA, () => HttpResponse.json(LISTA)));
    renderPagina();

    expect(await screen.findByText('ENT-0001')).toBeInTheDocument();
  });

  it('muestra el estado vacío', async () => {
    server.use(http.get(URL_LISTA, () => HttpResponse.json(VACIA)));
    renderPagina();

    expect(await screen.findByText('Sin entradas emitidas.')).toBeInTheDocument();
  });

  it('no rompe cuando la carga falla', async () => {
    server.use(http.get(URL_LISTA, () => HttpResponse.json({ title: 'Error' }, { status: 500 })));
    renderPagina();

    expect(await screen.findByText('Sin entradas emitidas.')).toBeInTheDocument();
  });
});
