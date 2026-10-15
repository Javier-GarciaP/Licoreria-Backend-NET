import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { http, HttpResponse } from 'msw';
import { MemoryRouter } from 'react-router-dom';
import { describe, expect, it, vi } from 'vitest';
import '@licoreria/auth'; // configura la URL base del cliente HTTP
import { server } from '../test/server';
import { AuditoriaPage } from './AuditoriaPage';

vi.mock('sonner', () => ({
  toast: Object.assign(vi.fn(), { warning: vi.fn(), error: vi.fn(), success: vi.fn(), info: vi.fn() }),
  Toaster: () => null,
}));

const guardar = vi.hoisted(() => vi.fn());
vi.mock('../lib/descargar', () => ({ guardarArchivo: guardar }));

const BASE = 'http://localhost:5190';

const registro = {
  id: 'log-1', usuarioId: 'u-1', usuario: 'admin@licoreria.com', accion: 'editar', entidad: 'ProductoVariante',
  entidadId: 'e-1', datos: null, ip: '203.0.113.7', fecha: '2026-10-14T12:00:00Z', tipo: 'Cambio',
};

describe('Exportar la bitácora de auditoría', () => {
  it('descarga con los mismos filtros que tiene la pantalla', async () => {
    let consulta = '';
    server.use(
      http.get(`${BASE}/api/v1/auditoria/entidades`, () => HttpResponse.json(['ProductoVariante'])),
      http.get(`${BASE}/api/v1/auditoria`, () =>
        HttpResponse.json({ items: [registro], page: 1, pageSize: 20, totalItems: 1, totalPages: 1 }),
      ),
      http.get(`${BASE}/api/v1/reportes/auditoria/exportar`, ({ request }) => {
        consulta = new URL(request.url).search;
        return new HttpResponse(new Uint8Array([37, 80]), {
          headers: { 'Content-Disposition': 'attachment; filename=reporte-auditoria-2026-10-14.pdf' },
        });
      }),
    );
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter initialEntries={['/auditoria?entidad=ProductoVariante&accion=editar&tipo=Cambio']}>
          <AuditoriaPage />
        </MemoryRouter>
      </QueryClientProvider>,
    );
    await screen.findByText('admin@licoreria.com');

    await userEvent.click(screen.getByRole('button', { name: 'Exportar PDF' }));

    await waitFor(() => expect(guardar).toHaveBeenCalledTimes(1));
    const params = new URLSearchParams(consulta);
    expect(params.get('formato')).toBe('pdf');
    expect(params.get('entidad')).toBe('ProductoVariante');
    expect(params.get('accion')).toBe('editar');
    expect(params.get('tipo')).toBe('Cambio');
  });
});
