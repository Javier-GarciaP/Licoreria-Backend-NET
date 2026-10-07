import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { http, HttpResponse } from 'msw';
import { describe, expect, it, vi } from 'vitest';
import { server } from '../test/server';
import { AuthProvider } from '../context/AuthContext';
import { tokens } from '../lib/api';
import { SalonOperacionPage } from './SalonOperacionPage';

vi.mock('../hooks/useRealtime', () => ({ useRealtime: () => undefined }));

const BASE = 'http://localhost:5190';

const mesaLibre = {
  id: 'mesa-1',
  zonaId: 'z1',
  zonaNombre: 'Salón',
  numero: '1',
  capacidad: 4,
  forma: 'redonda',
  posX: 0,
  posY: 0,
  ancho: 1,
  alto: 1,
  activa: true,
  disponible: true,
  cuentaId: null,
  reservada: false,
};

const mesaOcupada = { ...mesaLibre, id: 'mesa-2', numero: '2', disponible: false, cuentaId: 'cuenta-1' };

function crearCuenta(abonado = 0) {
  return {
    id: 'cuenta-1',
    sesionMesaId: 's1',
    nombreMesa: 'Mesa 2',
    estado: 'Abierta',
    total: 20,
    totalAbonado: abonado,
    saldo: 20 - abonado,
    abiertaEn: new Date().toISOString(),
    comandas: [
      {
        id: 'c1',
        area: 'Barra',
        estado: 'Recibida',
        fecha: new Date().toISOString(),
        detalles: [
          {
            id: 'd1',
            varianteId: 'var-1',
            sku: 'SKU1',
            nombre: 'Cerveza',
            cantidad: 1,
            precioUnitarioUSD: 10,
            areaDestino: 'Barra',
            estado: 'Recibido',
            esCortesia: false,
            subtotalUSD: 10,
          },
          {
            id: 'd2',
            varianteId: 'var-1',
            sku: 'SKU1',
            nombre: 'Cerveza',
            cantidad: 1,
            precioUnitarioUSD: 10,
            areaDestino: 'Barra',
            estado: 'Recibido',
            esCortesia: false,
            subtotalUSD: 10,
          },
        ],
      },
    ],
    abonos: [],
    divisiones: [],
  };
}

function usarHandlers(opciones: { onComanda?: (body: unknown) => void; onAbono?: (body: unknown) => void; abonado?: number } = {}) {
  let cuenta = crearCuenta(opciones.abonado ?? 0);
  server.use(
    http.get(`${BASE}/api/auth/me`, () =>
      HttpResponse.json({
        usuarioId: 'u-1',
        nombreCompleto: 'Mesero Demo',
        email: 'mesero@licoreria.com',
        rol: 'Employee',
        rolDominio: 'Mesero',
        permisos: ['sales:read', 'sales:write', 'club:read', 'account:read', 'account:abono', 'account:close'],
      }),
    ),
    http.get(`${BASE}/api/v1/planos`, () => HttpResponse.json([])),
    http.get(`${BASE}/api/v1/zonas`, () => HttpResponse.json([])),
    http.get(`${BASE}/api/v1/mesas`, () => HttpResponse.json([mesaLibre, mesaOcupada])),
    http.get(`${BASE}/api/v1/cuentas`, () =>
      HttpResponse.json({ items: [cuenta], page: 1, pageSize: 100, totalItems: 1, totalPages: 1 }),
    ),
    http.get(`${BASE}/api/v1/cuentas/:id`, () => HttpResponse.json(cuenta)),
    http.get(`${BASE}/api/v1/metodos-pago`, () => HttpResponse.json([{ id: 'met-1', codigo: 'EFECTIVO', nombre: 'Efectivo' }])),
    http.get(`${BASE}/api/v1/categorias`, () =>
      HttpResponse.json([{ id: 'cat-1', nombre: 'Cervezas', descripcion: null, categoriaPadreId: null, activo: true }]),
    ),
    http.get(`${BASE}/api/v1/productos`, () =>
      HttpResponse.json({
        items: [
          {
            id: 'prod-1',
            nombre: 'Cerveza',
            descripcion: null,
            categoriaId: 'cat-1',
            categoriaNombre: 'Cervezas',
            marcaId: null,
            marcaNombre: null,
            impuestoId: null,
            tipo: 'Simple',
            gradoAlcoholico: null,
            imagenUrl: null,
            activo: true,
            variantes: [
              {
                id: 'var-1',
                nombre: 'Botella',
                sku: 'SKU1',
                precioCompraUSD: 5,
                precioVentaUSD: 10,
                unidadMedidaId: 'u-1',
                unidadMedidaNombre: 'Unidad',
                activo: true,
                codigosBarras: [],
                precios: [],
              },
            ],
          },
        ],
        page: 1,
        pageSize: 100,
        totalItems: 1,
        totalPages: 1,
      }),
    ),
    http.post(`${BASE}/api/v1/cuentas`, () => HttpResponse.json(cuenta)),
    http.post(`${BASE}/api/v1/cuentas/:id/comandas`, async ({ request }) => {
      opciones.onComanda?.(await request.json());
      return HttpResponse.json(cuenta);
    }),
    http.post(`${BASE}/api/v1/cuentas/:id/abonos`, async ({ request }) => {
      opciones.onAbono?.(await request.json());
      cuenta = crearCuenta(20);
      return HttpResponse.json(cuenta);
    }),
  );
}

function renderPagina() {
  tokens.set('test-access-token', 'test-refresh-token');
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <AuthProvider>
          <SalonOperacionPage />
        </AuthProvider>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe('SalonOperacionPage', () => {
  it('abre una mesa libre desde la grilla', async () => {
    usarHandlers();
    renderPagina();

    await userEvent.click(await screen.findByRole('button', { name: 'Vista grilla' }));
    await userEvent.click(await screen.findByRole('button', { name: /Mesa 1/ }));

    expect(await screen.findByRole('button', { name: /Agregar consumo/i })).toBeInTheDocument();
  });

  it('tacha los consumos cubiertos por abonos (FIFO)', async () => {
    usarHandlers({ abonado: 10 });
    renderPagina();

    await userEvent.click(await screen.findByRole('button', { name: 'Vista grilla' }));
    await userEvent.click(await screen.findByRole('button', { name: /Mesa 2/ }));

    expect(await screen.findByText('Pagado')).toBeInTheDocument();
  });

  it('envía una comanda estilo POS', async () => {
    const onComanda = vi.fn();
    usarHandlers({ onComanda });
    renderPagina();

    await userEvent.click(await screen.findByRole('button', { name: 'Vista grilla' }));
    await userEvent.click(await screen.findByRole('button', { name: /Mesa 2/ }));
    await userEvent.click(await screen.findByRole('button', { name: /Agregar consumo/i }));

    expect(await screen.findByRole('dialog', { name: 'Agregar consumo' })).toBeInTheDocument();
    await userEvent.click(await screen.findByRole('gridcell', { name: /Cerveza/ }));
    await userEvent.click(screen.getByRole('button', { name: 'Enviar a Barra' }));

    await waitFor(() => expect(onComanda).toHaveBeenCalledTimes(1));
    const body = onComanda.mock.calls[0][0] as { area: string; items: unknown[] };
    expect(body.area).toBe('Barra');
    expect(body.items).toHaveLength(1);
  });
});
