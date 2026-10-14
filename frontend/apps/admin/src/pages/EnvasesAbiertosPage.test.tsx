import { render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { http, HttpResponse } from 'msw';
import { describe, expect, it, vi } from 'vitest';
import '../lib/api';
import { server } from '../test/server';
import { EnvasesAbiertosPage } from './EnvasesAbiertosPage';

vi.mock('../hooks/useRealtime', () => ({ useRealtime: () => undefined }));

const BASE = 'http://localhost:5190';

const envase = (extra: Record<string, unknown>) => ({
  id: 'e',
  porcionVarianteId: 'vaso',
  porcionNombre: 'Coca-Cola · Vaso',
  presentacionVarianteId: 'l1',
  presentacionNombre: 'Botella 1 L',
  porcionesIniciales: 4,
  porcionesRestantes: 4,
  estado: 'Abierto',
  abiertoEn: '2026-10-14T20:00:00Z',
  abiertoPorUsuarioId: 'u1',
  cerradoEn: null,
  motivoDescarte: null,
  ...extra,
});

describe('EnvasesAbiertosPage', () => {
  it('muestra lo abierto y el historial de descartes con su motivo', async () => {
    let consulta = '';
    server.use(
      http.get(`${BASE}/api/v1/envases-abiertos`, ({ request }) => {
        consulta = new URL(request.url).search;
        return HttpResponse.json([
          envase({ id: 'e1', porcionesRestantes: 3 }),
          envase({ id: 'e2', estado: 'Descartado', porcionesRestantes: 0, motivoDescarte: 'Se derramó', cerradoEn: '2026-10-14T21:00:00Z' }),
        ]);
      }),
    );
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter initialEntries={['/inventario/envases']}>
          <EnvasesAbiertosPage />
        </MemoryRouter>
      </QueryClientProvider>,
    );

    expect(await screen.findByText('Se derramó')).toBeInTheDocument();
    expect(screen.getByText('3 / 4')).toBeInTheDocument();
    expect(screen.getAllByText('Coca-Cola · Vaso')).toHaveLength(2);
    expect(screen.getByText('Descartado')).toBeInTheDocument();
    await waitFor(() => expect(consulta).toContain('todos=true'));
  });
});
