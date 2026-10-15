import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { http, HttpResponse } from 'msw';
import { describe, expect, it, vi } from 'vitest';
import { server } from '../test/server';
import { CuentaPage } from './CuentaPage';

vi.mock('sonner', () => ({
  toast: Object.assign(vi.fn(), { warning: vi.fn(), error: vi.fn(), success: vi.fn(), info: vi.fn() }),
  Toaster: () => null,
}));

const BASE = 'http://localhost:5190';

const CUENTA = {
  id: 'cta-1',
  sesionMesaId: 'ses-1',
  nombreMesa: 'Mesa 4',
  estado: 'Abierta',
  total: 30,
  totalAbonado: 0,
  saldo: 30,
  abiertaEn: new Date().toISOString(),
  comandas: [],
  abonos: [],
  divisiones: [],
  abiertaPorId: null,
  cliente: null,
};

const METODOS = [
  { id: 'met-usd', codigo: 'EfectivoUSD', nombre: 'Efectivo en dólares' },
  { id: 'met-cr', codigo: 'Credito', nombre: 'Crédito' },
];

const ANA = { id: 'cli-1', nombre: 'Ana Pérez', rif: null, ci: null, email: null, telefono: null, direccion: null, puntos: 7, activo: true };

function usarHandlers(onCierre?: (body: unknown) => void) {
  server.use(
    http.get(`${BASE}/api/v1/cuentas/cta-1`, () => HttpResponse.json(CUENTA)),
    http.get(`${BASE}/api/v1/metodos-pago`, () => HttpResponse.json(METODOS)),
    http.get(`${BASE}/api/v1/tasas-cambio/actual`, () =>
      HttpResponse.json({ id: 't-1', fecha: new Date().toISOString(), tipo: 'Paralelo', valor: 40 }),
    ),
    http.get(`${BASE}/api/v1/clientes`, () =>
      HttpResponse.json({ items: [ANA], page: 1, pageSize: 6, totalItems: 1, totalPages: 1 }),
    ),
    http.get(`${BASE}/api/v1/local-info`, () => HttpResponse.json({ nombre: 'Licorería Test' })),
    http.post(`${BASE}/api/v1/cuentas/cta-1/cerrar`, async ({ request }) => {
      onCierre?.(await request.json());
      return HttpResponse.json({
        id: 'venta-1',
        fecha: new Date().toISOString(),
        tasaCambio: 40,
        subtotalUSD: 30,
        descuentoUSD: 0,
        totalUSD: 30,
        totalBS: 1200,
        estado: 'Completada',
        usuarioId: 'u-1',
        sesionCajaId: null,
        numeroComprobante: 'F-00000001',
        detalles: [],
        pagos: [],
        clienteId: 'cli-1',
        clienteNombre: 'Ana Pérez',
        puntosGanados: 0,
        creditoUSD: 30,
      });
    }),
  );
}

function renderCuenta() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={['/cuentas/cta-1']}>
        <Routes>
          <Route path="/cuentas/:id" element={<CuentaPage />} />
          <Route path="/cuentas" element={<p>Listado</p>} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

async function abrirCobro() {
  await userEvent.click(await screen.findByRole('button', { name: 'Cobrar' }));
  return screen.findByRole('dialog', { name: 'Cobrar cuenta' });
}

describe('CuentaPage — cierre de cuenta', () => {
  it('cierra la cuenta a crédito con un cliente y envía el vencimiento', async () => {
    const onCierre = vi.fn();
    usarHandlers(onCierre);
    renderCuenta();
    const dialogo = await abrirCobro();

    await userEvent.selectOptions(within(dialogo).getByRole('combobox', { name: 'Método de pago' }), 'met-cr');
    await userEvent.click(within(dialogo).getByRole('button', { name: 'Añadir' }));

    // Con un pago a crédito y sin cliente el cierre queda bloqueado con el motivo visible.
    expect(within(dialogo).getByRole('button', { name: /Cobrar y cerrar/ })).toBeDisabled();
    expect(within(dialogo).getByRole('alert').textContent).toContain('Elige un cliente');

    await userEvent.type(within(dialogo).getByLabelText('Buscar cliente'), 'Ana');
    await userEvent.click(await within(dialogo).findByRole('button', { name: /Ana Pérez/ }));

    const cerrar = within(dialogo).getByRole('button', { name: /Cobrar y cerrar/ });
    await waitFor(() => expect(cerrar).toBeEnabled());
    await userEvent.click(cerrar);

    await waitFor(() => expect(onCierre).toHaveBeenCalledTimes(1));
    const cuerpo = onCierre.mock.calls[0][0] as {
      pagos: { metodoPagoId: string; monto: number }[];
      clienteId: string;
      vencimientoCredito: string | null;
    };
    expect(cuerpo.pagos).toEqual([{ metodoPagoId: 'met-cr', monto: 30, moneda: 'USD' }]);
    expect(cuerpo.clienteId).toBe('cli-1');
    expect(new Date(cuerpo.vencimientoCredito!).getTime()).toBeGreaterThan(Date.now());
  });

  it('un cierre en efectivo no exige cliente ni envía vencimiento', async () => {
    const onCierre = vi.fn();
    usarHandlers(onCierre);
    renderCuenta();
    const dialogo = await abrirCobro();

    await userEvent.selectOptions(within(dialogo).getByRole('combobox', { name: 'Método de pago' }), 'met-usd');
    await userEvent.click(within(dialogo).getByRole('button', { name: 'Añadir' }));
    await userEvent.click(within(dialogo).getByRole('button', { name: /Cobrar y cerrar/ }));

    await waitFor(() => expect(onCierre).toHaveBeenCalledTimes(1));
    const cuerpo = onCierre.mock.calls[0][0] as { clienteId: string | null; vencimientoCredito: string | null };
    expect(cuerpo.clienteId).toBeNull();
    expect(cuerpo.vencimientoCredito).toBeNull();
  });

  it('no ofrece Crédito como método de abono', async () => {
    usarHandlers();
    renderCuenta();

    await userEvent.click(await screen.findByRole('button', { name: /Abonar|Abono/ }));
    const dialogo = await screen.findByRole('dialog', { name: /bono/i });

    expect(within(dialogo).getByRole('button', { name: 'Efectivo en dólares' })).toBeInTheDocument();
    expect(within(dialogo).queryByRole('button', { name: 'Crédito' })).toBeNull();
  });
});
