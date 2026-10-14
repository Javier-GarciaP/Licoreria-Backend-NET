import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { http, HttpResponse } from 'msw';
import { describe, expect, it, vi } from 'vitest';
import { server } from '../../test/server';
import { PanelEnvasesAbiertos } from './PanelEnvasesAbiertos';

vi.mock('sonner', () => ({
  toast: Object.assign(vi.fn(), { warning: vi.fn(), error: vi.fn(), success: vi.fn(), info: vi.fn() }),
  Toaster: () => null,
}));

const BASE = 'http://localhost:5190';

const variante = (extra: Record<string, unknown>) => ({
  sku: 'S',
  precioCompraUSD: 1,
  precioVentaUSD: 2,
  unidadMedidaId: 'u',
  unidadMedidaNombre: 'u',
  activo: true,
  esBase: false,
  codigosBarras: [],
  cantidad: 0,
  cantidadReservada: 0,
  stockMinimo: 0,
  stockMaximo: 0,
  porcionesDisponibles: 0,
  porcionesPotenciales: 0,
  envasesCerradosAbribles: 0,
  ...extra,
});

const cola = {
  id: 'p-cola',
  nombre: 'Coca-Cola',
  areaDestino: 'Barra',
  tipo: 'Simple',
  activo: true,
  variantes: [
    variante({ id: 'l1', nombre: 'Botella 1 L', esBase: true, naturaleza: 'Presentacion', cantidad: 3 }),
    variante({ id: 'l2', nombre: 'Botella 2 L', esBase: true, naturaleza: 'Presentacion', cantidad: 0 }),
    variante({ id: 'vaso', nombre: 'Vaso', naturaleza: 'Porcion', envasesCerradosAbribles: 3, porcionesPotenciales: 12 }),
  ],
};

const envaseAbierto = {
  id: 'env-1',
  porcionVarianteId: 'vaso',
  porcionNombre: 'Coca-Cola · Vaso',
  presentacionVarianteId: 'l1',
  presentacionNombre: 'Botella 1 L',
  porcionesIniciales: 4,
  porcionesRestantes: 3,
  estado: 'Abierto',
  abiertoEn: new Date().toISOString(),
  abiertoPorUsuarioId: 'u1',
  cerradoEn: null,
  motivoDescarte: null,
};

function usarHandlers() {
  server.use(
    http.get(`${BASE}/api/v1/envases-abiertos`, () => HttpResponse.json([envaseAbierto])),
    http.get(`${BASE}/api/v1/productos`, () =>
      HttpResponse.json({ items: [cola], page: 1, pageSize: 100, totalItems: 1, totalPages: 1 }),
    ),
    http.get(`${BASE}/api/v1/variantes/vaso/origenes`, () =>
      HttpResponse.json([
        { id: 'o1', presentacionVarianteId: 'l1', presentacionNombre: 'Botella 1 L', sku: 'S', porcionesPorUnidad: 4 },
        { id: 'o2', presentacionVarianteId: 'l2', presentacionNombre: 'Botella 2 L', sku: 'S', porcionesPorUnidad: 8 },
      ]),
    ),
  );
}

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={queryClient}>
      <PanelEnvasesAbiertos abierto onCerrar={() => undefined} />
    </QueryClientProvider>,
  );
}

describe('PanelEnvasesAbiertos', () => {
  it('lista los envases abiertos con sus porciones restantes', async () => {
    usarHandlers();
    renderPanel();

    const lista = await screen.findByRole('region', { name: 'Envases abiertos' });
    expect(await within(lista).findByText('Coca-Cola · Vaso')).toBeInTheDocument();
    expect(within(lista).getByText('3/4')).toBeInTheDocument();
  });

  it('destapa la botella que elige el barman', async () => {
    usarHandlers();
    let cuerpo: unknown;
    server.use(
      http.post(`${BASE}/api/v1/envases-abiertos`, async ({ request }) => {
        cuerpo = await request.json();
        return HttpResponse.json({ ...envaseAbierto, id: 'env-2' });
      }),
    );
    renderPanel();

    const seccion = await screen.findByRole('region', { name: 'Destapar' });
    await userEvent.click(await within(seccion).findByRole('button', { name: 'Destapar' }));

    const opciones = await screen.findByRole('group', { name: /Elegir botella para Vaso/ });
    // La de 2 L no tiene existencia: no se puede elegir.
    expect(await within(opciones).findByRole('button', { name: /Botella 2 L.*Agotado/ })).toBeDisabled();
    await userEvent.click(within(opciones).getByRole('button', { name: /Botella 1 L.*Quedan 3/ }));

    await waitFor(() => expect(cuerpo).toEqual({ porcionVarianteId: 'vaso', presentacionVarianteId: 'l1' }));
  });

  it('exige un motivo para descartar un envase', async () => {
    usarHandlers();
    let cuerpo: unknown;
    server.use(
      http.post(`${BASE}/api/v1/envases-abiertos/env-1/descartar`, async ({ request }) => {
        cuerpo = await request.json();
        return HttpResponse.json({ ...envaseAbierto, estado: 'Descartado', motivoDescarte: 'Se derramó' });
      }),
    );
    renderPanel();

    await userEvent.click(await screen.findByRole('button', { name: 'Descartar' }));
    const confirmar = screen.getByRole('button', { name: 'Confirmar descarte' });
    expect(confirmar).toBeDisabled();

    await userEvent.type(screen.getByLabelText('Motivo del descarte'), 'Se derramó');
    await userEvent.click(confirmar);

    await waitFor(() => expect(cuerpo).toEqual({ motivo: 'Se derramó' }));
  });
});
