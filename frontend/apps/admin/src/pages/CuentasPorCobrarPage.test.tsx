import { render, screen } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it } from 'vitest';
import { server } from '../test/server';
import { CuentasPorCobrarPage } from './CuentasPorCobrarPage';

const BASE = 'http://localhost:5190';

const pagina = <T,>(items: T[]) => ({ items, page: 1, pageSize: 100, totalItems: items.length, totalPages: 1 });

const CUENTA = {
  id: 'cxc-1',
  clienteId: 'cli-1',
  clienteNombre: 'Ana Pérez',
  montoUSD: 10,
  saldoUSD: 10,
  vencimiento: '2026-09-01T00:00:00Z',
  estado: 'Vencida',
  ventaId: '12345678-aaaa-bbbb-cccc-1234567890ab',
};

function renderPagina() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <CuentasPorCobrarPage />
    </QueryClientProvider>,
  );
}

describe('CuentasPorCobrarPage', () => {
  // La pantalla también consulta los clientes para el formulario de nueva cuenta.
  beforeEach(() => {
    server.use(http.get(`${BASE}/api/v1/clientes`, () => HttpResponse.json(pagina([]))));
  });

  it('lista las cuentas con la venta enlazada y el estado vencida', async () => {
    server.use(
      http.get(`${BASE}/api/v1/cuentas-por-cobrar`, () =>
        HttpResponse.json(pagina([CUENTA, { ...CUENTA, id: 'cxc-2', ventaId: null, estado: 'Pendiente', clienteNombre: 'Luis Gómez' }])),
      ),
    );
    renderPagina();

    expect(await screen.findByText('Ana Pérez')).toBeInTheDocument();
    expect(screen.getByText('Vencida')).toBeInTheDocument();
    expect(screen.getByText('Venta 12345678')).toBeInTheDocument();
    expect(screen.getByText('Manual')).toBeInTheDocument();
  });

  it('muestra el estado vacío', async () => {
    server.use(http.get(`${BASE}/api/v1/cuentas-por-cobrar`, () => HttpResponse.json(pagina([]))));
    renderPagina();

    expect(await screen.findByText('Sin cuentas por cobrar.')).toBeInTheDocument();
  });

  it('no rompe cuando la carga falla', async () => {
    server.use(http.get(`${BASE}/api/v1/cuentas-por-cobrar`, () => HttpResponse.json({ title: 'Error' }, { status: 500 })));
    renderPagina();

    expect(await screen.findByText('Sin cuentas por cobrar.')).toBeInTheDocument();
  });

  it('pide solo las cuentas pendientes, que incluyen las vencidas', async () => {
    let soloPendientes: string | null = null;
    server.use(
      http.get(`${BASE}/api/v1/cuentas-por-cobrar`, ({ request }) => {
        soloPendientes = new URL(request.url).searchParams.get('soloPendientes');
        return HttpResponse.json(pagina([CUENTA]));
      }),
    );
    renderPagina();

    await screen.findByText('Ana Pérez');
    expect(soloPendientes).toBe('true');
  });
});
