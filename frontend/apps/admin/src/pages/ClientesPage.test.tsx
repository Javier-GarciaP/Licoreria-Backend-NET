import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { server } from '../test/server';
import { ClientesPage } from './ClientesPage';

const BASE = 'http://localhost:5190';

const ANA = {
  id: 'cli-1',
  nombre: 'Ana Pérez',
  rif: null,
  ci: 'V-123',
  email: null,
  telefono: '0414-1111111',
  direccion: null,
  puntos: 42,
  activo: true,
};

const pagina = <T,>(items: T[]) => ({ items, page: 1, pageSize: 15, totalItems: items.length, totalPages: 1 });

function renderPagina() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <ClientesPage />
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe('ClientesPage', () => {
  it('lista los clientes con sus puntos', async () => {
    server.use(http.get(`${BASE}/api/v1/clientes`, () => HttpResponse.json(pagina([ANA]))));
    renderPagina();

    expect(await screen.findByText('Ana Pérez')).toBeInTheDocument();
    expect(screen.getByText('42')).toBeInTheDocument();
  });

  it('muestra el estado vacío', async () => {
    server.use(http.get(`${BASE}/api/v1/clientes`, () => HttpResponse.json(pagina([]))));
    renderPagina();

    expect(await screen.findByText('No hay clientes.')).toBeInTheDocument();
  });

  it('no rompe cuando la carga falla', async () => {
    server.use(http.get(`${BASE}/api/v1/clientes`, () => HttpResponse.json({ title: 'Error' }, { status: 500 })));
    renderPagina();

    expect(await screen.findByText('No hay clientes.')).toBeInTheDocument();
  });

  it('el detalle muestra las ventas, las cuentas por cobrar y los movimientos de puntos', async () => {
    server.use(
      http.get(`${BASE}/api/v1/clientes`, () => HttpResponse.json(pagina([ANA]))),
      http.get(`${BASE}/api/v1/clientes/cli-1/ventas`, () =>
        HttpResponse.json(
          pagina([{ id: 'v-1', fecha: '2026-10-14T15:00:00Z', totalUSD: 25.8, numeroComprobante: 'F-00000007', estado: 'Completada' }]),
        ),
      ),
      http.get(`${BASE}/api/v1/clientes/cli-1/puntos`, () =>
        HttpResponse.json([
          { id: 'p-1', tipo: 'Acumulacion', puntos: 25, motivo: 'Puntos por venta', fecha: '2026-10-14T15:00:00Z' },
          { id: 'p-2', tipo: 'Reversion', puntos: 5, motivo: 'Reversión por devolución', fecha: '2026-10-14T16:00:00Z' },
        ]),
      ),
      http.get(`${BASE}/api/v1/cuentas-por-cobrar`, ({ request }) => {
        expect(new URL(request.url).searchParams.get('clienteId')).toBe('cli-1');
        return HttpResponse.json(
          pagina([
            {
              id: 'cxc-1',
              clienteId: 'cli-1',
              clienteNombre: 'Ana Pérez',
              montoUSD: 10,
              saldoUSD: 10,
              vencimiento: '2026-09-01T00:00:00Z',
              estado: 'Vencida',
              ventaId: 'v-1',
            },
          ]),
        );
      }),
    );
    renderPagina();

    await userEvent.click(await screen.findByRole('button', { name: 'Acciones de Ana Pérez' }));
    await userEvent.click(await screen.findByRole('menuitem', { name: 'Ver detalle' }));

    const dialogo = await screen.findByRole('dialog', { name: /Cliente · Ana Pérez/ });
    expect(await within(dialogo).findByText(/F-00000007/)).toBeInTheDocument();
    expect(await within(dialogo).findByText('Vencida')).toBeInTheDocument();
    expect(await within(dialogo).findByText(/Reversión por devolución/)).toBeInTheDocument();
    expect(within(dialogo).getByText('+25')).toBeInTheDocument();
    expect(within(dialogo).getByText('−5')).toBeInTheDocument();
  });

  it('el detalle de un cliente sin actividad muestra los estados vacíos', async () => {
    server.use(
      http.get(`${BASE}/api/v1/clientes`, () => HttpResponse.json(pagina([ANA]))),
      http.get(`${BASE}/api/v1/clientes/cli-1/ventas`, () => HttpResponse.json(pagina([]))),
      http.get(`${BASE}/api/v1/clientes/cli-1/puntos`, () => HttpResponse.json([])),
      http.get(`${BASE}/api/v1/cuentas-por-cobrar`, () => HttpResponse.json(pagina([]))),
    );
    renderPagina();

    await userEvent.click(await screen.findByRole('button', { name: 'Acciones de Ana Pérez' }));
    await userEvent.click(await screen.findByRole('menuitem', { name: 'Ver detalle' }));

    const dialogo = await screen.findByRole('dialog', { name: /Cliente · Ana Pérez/ });
    expect(await within(dialogo).findByText('Sin ventas asociadas.')).toBeInTheDocument();
    expect(await within(dialogo).findByText('Sin cuentas por cobrar.')).toBeInTheDocument();
    expect(await within(dialogo).findByText('Sin movimientos de puntos.')).toBeInTheDocument();
  });
});
