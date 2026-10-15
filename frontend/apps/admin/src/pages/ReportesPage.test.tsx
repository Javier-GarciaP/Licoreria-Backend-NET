import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { http, HttpResponse } from 'msw';
import { MemoryRouter } from 'react-router-dom';
import { afterAll, beforeAll, beforeEach, describe, expect, it, vi } from 'vitest';
import { configureApi } from '@licoreria/api-client';
import { server } from '../test/server';
import { ReportesPage } from './ReportesPage';

vi.mock('sonner', () => ({
  toast: Object.assign(vi.fn(), { warning: vi.fn(), error: vi.fn(), success: vi.fn(), info: vi.fn() }),
  Toaster: () => null,
}));

const guardar = vi.hoisted(() => vi.fn());
vi.mock('../lib/descargar', () => ({ guardarArchivo: guardar }));

const sesion = vi.hoisted(() => ({ esAdmin: true, permisos: [] as string[] }));
vi.mock('../context/AuthContext', () => ({
  useAuth: () => ({
    esAdmin: sesion.esAdmin,
    tienePermiso: (clave: string) => sesion.permisos.includes(clave),
  }),
}));

const BASE = 'http://localhost:5190';

beforeAll(() => configureApi({ baseUrl: BASE }));
afterAll(() => configureApi({ baseUrl: '' }));
beforeEach(() => {
  guardar.mockClear();
  sesion.esAdmin = true;
  sesion.permisos = [];
});

const VENTAS = {
  desde: '', hasta: '', totalUSD: 250, totalBS: 9125, cantidad: 10, ticketPromedioUSD: 25,
  porDia: [], porUsuario: [], porMetodoPago: [],
};

function renderizar(url = '/reportes') {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[url]}>
        <ReportesPage />
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe('ReportesPage', () => {
  it('el administrador ve todas las pestañas, incluida la de mesoneros', async () => {
    server.use(http.get(`${BASE}/api/v1/reportes/ventas`, () => HttpResponse.json(VENTAS)));
    renderizar();

    await userEvent.click(screen.getByRole('button', { name: /Ventas/ }));
    const menu = await screen.findByRole('menu');
    for (const nombre of ['Ventas', 'Productos', 'Inventario', 'Compras', 'Propinas', 'Mesoneros', 'Caja', 'Ocupación', 'Mermas']) {
      expect(within(menu).getByRole('menuitem', { name: nombre })).toBeInTheDocument();
    }
  });

  it('quien no es administrador no ve la pestaña de mesoneros ni la abre por la URL', async () => {
    sesion.esAdmin = false;
    sesion.permisos = ['sales:read', 'cash:close'];
    server.use(http.get(`${BASE}/api/v1/reportes/ventas`, () => HttpResponse.json(VENTAS)));
    renderizar('/reportes?reporte=mesoneros');

    await userEvent.click(screen.getByRole('button', { name: /Ventas/ }));
    const menu = await screen.findByRole('menu');
    expect(within(menu).queryByRole('menuitem', { name: 'Mesoneros' })).not.toBeInTheDocument();
    expect(within(menu).queryByRole('menuitem', { name: 'Compras' })).not.toBeInTheDocument();
    expect(within(menu).getByRole('menuitem', { name: 'Caja' })).toBeInTheDocument();
    // Cae en la primera pestaña permitida en lugar de pedir el reporte restringido.
    expect(await screen.findByText('Ventas del período')).toBeInTheDocument();
  });

  it('al cambiar de pestaña conserva el periodo y consulta solo los datos de esa pestaña', async () => {
    const consultas: string[] = [];
    server.use(
      http.get(`${BASE}/api/v1/reportes/ventas`, () => {
        consultas.push('ventas');
        return HttpResponse.json(VENTAS);
      }),
      http.get(`${BASE}/api/v1/reportes/caja`, ({ request }) => {
        const url = new URL(request.url);
        consultas.push(`caja:${url.searchParams.get('desde')}|${url.searchParams.get('hasta')}`);
        return HttpResponse.json({ desde: '', hasta: '', turnos: 1, descuadreTotal: -5, descuadrePromedio: -5, mayorDescuadre: -5, items: [] });
      }),
      http.get(`${BASE}/api/v1/reportes/compras`, () => {
        consultas.push('compras');
        return HttpResponse.json({});
      }),
    );
    renderizar('/reportes?periodo=personalizado&desde=2026-10-01&hasta=2026-10-13');
    await screen.findByText('Ventas del período');

    await userEvent.click(screen.getByRole('button', { name: /Ventas/ }));
    await userEvent.click(await screen.findByRole('menuitem', { name: 'Caja' }));

    expect(await screen.findByText('Descuadre de caja por turno')).toBeInTheDocument();
    expect(consultas).toContain('caja:2026-10-01|2026-10-13');
    expect(consultas).not.toContain('compras');
    expect(screen.getByLabelText('Fecha inicial')).toHaveValue('2026-10-01');
  });

  it('muestra el ranking de productos con su rotación y los productos sin movimiento', async () => {
    const ron = {
      varianteId: 'v1', sku: 'RON', productoNombre: 'Ron Añejo', varianteNombre: '750 ml', unidades: 30, ingresoUSD: 90,
      porcentajeUnidades: 60, porcentajeIngreso: 55, existencia: 10, rotacion: 3, estadoRotacion: 'Calculada',
    };
    const tequila = { ...ron, varianteId: 'v2', productoNombre: 'Tequila', unidades: 0, ingresoUSD: 0, rotacion: 0, estadoRotacion: 'SinMovimiento' };
    server.use(
      http.get(`${BASE}/api/v1/reportes/productos`, () =>
        HttpResponse.json({ desde: '', hasta: '', orden: 'Unidades', limite: 10, totalUnidades: 30, totalIngresoUSD: 90, top: [ron], sinMovimiento: [tequila] }),
      ),
    );
    renderizar('/reportes?reporte=productos');

    expect(await screen.findByText('Ron Añejo')).toBeInTheDocument();
    expect(screen.getByText('Tequila')).toBeInTheDocument();
  });

  it('el botón de Excel pide la descarga con el periodo y guarda el archivo', async () => {
    server.use(http.get(`${BASE}/api/v1/reportes/ventas`, () => HttpResponse.json(VENTAS)));
    let consulta = '';
    server.use(
      http.get(`${BASE}/api/v1/reportes/ventas/exportar`, ({ request }) => {
        consulta = new URL(request.url).search;
        return new HttpResponse(new Uint8Array([80, 75]), {
          headers: { 'Content-Disposition': 'attachment; filename=reporte-ventas-2026-10-01_2026-10-13.xlsx' },
        });
      }),
    );
    renderizar('/reportes?periodo=personalizado&desde=2026-10-01&hasta=2026-10-13');
    await screen.findByText('Ventas del período');

    await userEvent.click(screen.getByRole('button', { name: 'Exportar Excel' }));

    await waitFor(() => expect(guardar).toHaveBeenCalledTimes(1));
    const params = new URLSearchParams(consulta);
    expect(params.get('formato')).toBe('xlsx');
    expect(params.get('desde')).toBe('2026-10-01');
    expect(params.get('hasta')).toBe('2026-10-13');
    expect(guardar.mock.calls[0][1]).toBe('reporte-ventas-2026-10-01_2026-10-13.xlsx');
  });

  it('si la exportación falla avisa del error y no guarda nada', async () => {
    server.use(
      http.get(`${BASE}/api/v1/reportes/ventas`, () => HttpResponse.json(VENTAS)),
      http.get(`${BASE}/api/v1/reportes/ventas/exportar`, () =>
        HttpResponse.json({ title: 'Regla', status: 422, detail: 'Acote el periodo o exporte en Excel.' }, { status: 422 }),
      ),
    );
    const { toast } = await import('sonner');
    renderizar();
    await screen.findByText('Ventas del período');

    await userEvent.click(screen.getByRole('button', { name: 'Exportar PDF' }));

    await waitFor(() => expect(toast.error).toHaveBeenCalled());
    expect(guardar).not.toHaveBeenCalled();
  });

  it('con un rango inválido no consulta ni permite exportar', async () => {
    let consultas = 0;
    server.use(
      http.get(`${BASE}/api/v1/reportes/ventas`, () => {
        consultas += 1;
        return HttpResponse.json(VENTAS);
      }),
    );
    renderizar('/reportes?periodo=personalizado&desde=2026-10-13&hasta=2026-10-01');

    expect(await screen.findByRole('alert')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Exportar PDF' })).toBeDisabled();
    expect(screen.getByRole('button', { name: 'Exportar Excel' })).toBeDisabled();
    await new Promise((resolver) => setTimeout(resolver, 50));
    expect(consultas).toBe(0);
  });

  it('el inventario no depende del periodo: oculta el selector', async () => {
    server.use(
      http.get(`${BASE}/api/v1/reportes/inventario`, () =>
        HttpResponse.json({ variantes: 1, unidadesTotales: 5, costoTotalUSD: 10, valorVentaTotalUSD: 20, bajoMinimo: 0, items: [] }),
      ),
    );
    renderizar('/reportes?reporte=inventario');

    expect(await screen.findByText('Inventario valorizado')).toBeInTheDocument();
    expect(screen.queryByRole('group', { name: 'Periodo' })).not.toBeInTheDocument();
  });
});
