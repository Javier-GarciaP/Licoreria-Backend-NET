import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { http, HttpResponse } from 'msw';
import { MemoryRouter } from 'react-router-dom';
import { beforeAll, afterAll, describe, expect, it, vi } from 'vitest';
import { configureApi } from '@licoreria/api-client';
import { server } from '../test/server';
import { fechaLocal } from '../lib/periodo';
import { DashboardPage } from './DashboardPage';

// Las cifras se animan desde 0; con movimiento reducido aparecen de inmediato.
vi.mock('framer-motion', async (importar) => ({
  ...(await importar<typeof import('framer-motion')>()),
  useReducedMotion: () => true,
}));

const BASE = 'http://localhost:5190';

beforeAll(() => configureApi({ baseUrl: BASE }));
afterAll(() => configureApi({ baseUrl: '' }));

const DASHBOARD = {
  ventasHoyUSD: 0, ventasHoyCantidad: 0, ventasMesUSD: 0, ventasMesCantidad: 0, ticketPromedioUSD: 0,
  mermasMesCantidad: 0, mermasMesUnidades: 0, cuentasAbiertas: 3, productosStockBajo: 2, reservasProximas: 4,
  cajaAbierta: true, cajaFondoInicial: 20, clientes: 10, productosActivos: 30, generadoEn: '2026-10-14T12:00:00Z',
};

const VENTAS = {
  desde: '2026-09-15', hasta: '2026-10-14', totalUSD: 250, totalBS: 9125, cantidad: 10, ticketPromedioUSD: 25,
  porDia: [{ fecha: '2026-10-13T00:00:00Z', totalUSD: 250, cantidad: 10 }], porUsuario: [], porMetodoPago: [],
};

const PRODUCTO = {
  varianteId: 'v1', sku: 'RON', productoNombre: 'Ron Añejo', varianteNombre: '750 ml', unidades: 30, ingresoUSD: 90,
  porcentajeUnidades: 60, porcentajeIngreso: 55, existencia: 10, rotacion: 3, estadoRotacion: 'Calculada',
};

function registrar(overrides: Record<string, unknown> = {}) {
  const respuestas: Record<string, unknown> = {
    dashboard: DASHBOARD,
    'inventario-salud': { total: 5, sinStock: 0, riesgoCritico: 1, subabastecido: 1, optimo: 3, sobreabastecido: 0, excesivo: 0, items: [] },
    ventas: VENTAS,
    productos: { desde: '', hasta: '', orden: 'Unidades', limite: 5, totalUnidades: 50, totalIngresoUSD: 160, top: [PRODUCTO], sinMovimiento: [] },
    ocupacion: { desde: '', hasta: '', generadoEn: '', mesasActivas: 20, mesasOcupadas: 5, porcentajeOcupacion: 25, sesiones: 10, sesionesAbiertas: 5, sesionesPorMesa: 0.5, permanenciaPromedioMinutos: 90, porZona: [] },
    'reservas-asistencia': { desde: '', hasta: '', total: 10, asistieron: 8, noAsistieron: 2, canceladas: 0, tasaNoAsistencia: 20, porEstado: [] },
    caja: { desde: '', hasta: '', turnos: 2, descuadreTotal: -5, descuadrePromedio: -2.5, mayorDescuadre: -5, items: [] },
    'mermas-vs-ventas': { desde: '', hasta: '', ventasUSD: 250, mermaValorizadaUSD: 15, porcentaje: 6, porMotivo: [{ motivo: 'Danado', registros: 2, unidades: 3, valorUSD: 15, porcentajeVentas: 6 }] },
    heatmap: { desde: '', hasta: '', franjas: [] },
    ...overrides,
  };
  for (const [ruta, cuerpo] of Object.entries(respuestas)) {
    server.use(http.get(`${BASE}/api/v1/reportes/${ruta}`, () => HttpResponse.json(cuerpo as object)));
  }
}

function renderizar(url = '/?periodo=30d') {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[url]}>
        <DashboardPage />
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

const tarjeta = (titulo: string) => screen.getByText(titulo).closest('div[class*="rounded-xl"]') as HTMLElement;

describe('DashboardPage', () => {
  it('muestra esqueletos mientras carga y luego las tarjetas con su valor', async () => {
    registrar();
    const { container } = renderizar();

    expect(container.querySelector('[class*="animate-pulse"], [data-skeleton]')).not.toBeNull();

    expect((await screen.findAllByText('Ron Añejo')).length).toBeGreaterThan(0);
    await waitFor(() => expect(within(tarjeta('Ticket promedio')).getByText(/25,00/)).toBeInTheDocument());
    expect(within(tarjeta('Más vendido')).getByText('Ron Añejo')).toBeInTheDocument();
    expect(within(tarjeta('Ocupación de mesas')).getByText(/5 de 20 mesas/)).toBeInTheDocument();
    expect(within(tarjeta('No asistencia a reservas')).getByText(/2 de 10 reservas/)).toBeInTheDocument();
    expect(within(tarjeta('Descuadre de caja')).getByText(/2 turnos cerrados/)).toBeInTheDocument();
    expect(within(tarjeta('Mermas del periodo')).getByText(/6,0% de las ventas/)).toBeInTheDocument();
  });

  it('un indicador sin datos muestra un estado vacío legible, no un cero engañoso', async () => {
    registrar({
      ventas: { ...VENTAS, totalUSD: 0, cantidad: 0, ticketPromedioUSD: 0, porDia: [] },
      productos: { desde: '', hasta: '', orden: 'Unidades', limite: 5, totalUnidades: 0, totalIngresoUSD: 0, top: [], sinMovimiento: [] },
      caja: { desde: '', hasta: '', turnos: 0, descuadreTotal: 0, descuadrePromedio: 0, mayorDescuadre: 0, items: [] },
      'reservas-asistencia': { desde: '', hasta: '', total: 0, asistieron: 0, noAsistieron: 0, canceladas: 0, tasaNoAsistencia: null, porEstado: [] },
      'mermas-vs-ventas': { desde: '', hasta: '', ventasUSD: 0, mermaValorizadaUSD: 0, porcentaje: 0, porMotivo: [] },
    });
    renderizar();

    expect(await screen.findByText('No hubo ventas en el periodo.')).toBeInTheDocument();
    expect(screen.getByText('No hubo productos vendidos en el periodo.')).toBeInTheDocument();
    expect(screen.getByText('Sin turnos cerrados en el periodo.')).toBeInTheDocument();
    expect(screen.getByText('Sin reservas que debieran concretarse.')).toBeInTheDocument();
    expect(screen.getAllByText('Sin datos').length).toBeGreaterThanOrEqual(5);
    expect(within(tarjeta('Descuadre de caja')).queryByText(/\$|US/)).toBeNull();
  });

  it('un indicador prohibido para el rol no rompe el resto del tablero', async () => {
    registrar();
    server.use(
      http.get(`${BASE}/api/v1/reportes/ocupacion`, () =>
        HttpResponse.json({ title: 'Prohibido', status: 403 }, { status: 403 }),
      ),
    );
    renderizar();

    expect(await within(tarjeta('Ocupación de mesas')).findByText('Sin acceso')).toBeInTheDocument();
    await waitFor(() => expect(within(tarjeta('Ticket promedio')).getByText(/25,00/)).toBeInTheDocument());
  });

  it('al cambiar el periodo vuelve a consultar con las nuevas fechas, sin recargar', async () => {
    registrar();
    const consultas: string[] = [];
    server.use(
      http.get(`${BASE}/api/v1/reportes/ventas`, ({ request }) => {
        const url = new URL(request.url);
        consultas.push(`${url.searchParams.get('desde')}|${url.searchParams.get('hasta')}`);
        return HttpResponse.json(VENTAS);
      }),
    );
    renderizar();
    await waitFor(() => expect(consultas.length).toBe(1));

    await userEvent.click(screen.getByRole('button', { name: 'Hoy' }));

    const hoy = fechaLocal(new Date());
    await waitFor(() => expect(consultas).toContain(`${hoy}|${hoy}`));
    expect(screen.getByRole('button', { name: 'Hoy' })).toHaveAttribute('aria-pressed', 'true');
  });

  it('un rango personalizado inválido no consulta y muestra el motivo', async () => {
    registrar();
    let consultas = 0;
    server.use(
      http.get(`${BASE}/api/v1/reportes/ventas`, () => {
        consultas += 1;
        return HttpResponse.json(VENTAS);
      }),
    );
    renderizar('/?periodo=personalizado&desde=2026-10-13&hasta=2026-10-01');

    expect(await screen.findByRole('alert')).toHaveTextContent(/final no puede ser anterior/);
    await new Promise((resolver) => setTimeout(resolver, 50));
    expect(consultas).toBe(0);
  });
});
