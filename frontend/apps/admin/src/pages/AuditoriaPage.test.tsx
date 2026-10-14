import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter } from 'react-router-dom';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import '@licoreria/auth'; // configura la URL base del cliente HTTP
import { server } from '../test/server';
import { AuditoriaPage } from './AuditoriaPage';

const BASE = 'http://localhost:5190';

const edicion = {
  id: 'log-1',
  usuarioId: 'u-1',
  usuario: 'admin@licoreria.com',
  accion: 'editar',
  entidad: 'ProductoVariante',
  entidadId: 'e-1',
  datos: '{"antes":{"PrecioVentaUSD":10},"despues":{"PrecioVentaUSD":12}}',
  ip: '203.0.113.7',
  fecha: '2026-10-14T12:00:00Z',
  tipo: 'Cambio',
};

function pagina(items: unknown[]) {
  return { items, page: 1, pageSize: 20, totalItems: items.length, totalPages: 1 };
}

function renderPagina(ruta = '/auditoria') {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[ruta]}>
        <AuditoriaPage />
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe('AuditoriaPage', () => {
  it('envía al backend los filtros que vienen en la URL', async () => {
    const consultas: URLSearchParams[] = [];
    server.use(
      http.get(`${BASE}/api/v1/auditoria/entidades`, () => HttpResponse.json(['ProductoVariante'])),
      http.get(`${BASE}/api/v1/auditoria`, ({ request }) => {
        consultas.push(new URL(request.url).searchParams);
        return HttpResponse.json(pagina([edicion]));
      }),
    );

    renderPagina('/auditoria?entidad=ProductoVariante&accion=editar&tipo=Cambio&usuario=u-1&desde=2026-10-01');

    await screen.findByText('admin@licoreria.com');
    const consulta = consultas.at(-1)!;
    expect(consulta.get('entidad')).toBe('ProductoVariante');
    expect(consulta.get('accion')).toBe('editar');
    expect(consulta.get('tipo')).toBe('Cambio');
    expect(consulta.get('usuarioId')).toBe('u-1');
    expect(consulta.get('desde')).toBeTruthy();
    expect(consulta.get('hasta')).toBeNull();
  });

  it('muestra el estado vacío con la opción de limpiar filtros', async () => {
    server.use(
      http.get(`${BASE}/api/v1/auditoria/entidades`, () => HttpResponse.json([])),
      http.get(`${BASE}/api/v1/auditoria`, () => HttpResponse.json(pagina([]))),
    );

    renderPagina('/auditoria?tipo=Evento');

    expect(await screen.findByText('Ningún registro cumple los filtros.')).toBeInTheDocument();
    expect(screen.getAllByRole('button', { name: /limpiar/i }).length).toBeGreaterThan(0);
  });

  it('abre el detalle de una edición con el valor anterior y el nuevo', async () => {
    server.use(
      http.get(`${BASE}/api/v1/auditoria/entidades`, () => HttpResponse.json(['ProductoVariante'])),
      http.get(`${BASE}/api/v1/auditoria/log-1`, () => HttpResponse.json(edicion)),
      http.get(`${BASE}/api/v1/auditoria`, () => HttpResponse.json(pagina([edicion]))),
    );

    renderPagina();

    fireEvent.click(await screen.findByText('admin@licoreria.com'));

    const tabla = await screen.findByRole('table', { name: 'Valores antes y después' });
    await waitFor(() => expect(tabla).toHaveTextContent('PrecioVentaUSD'));
    expect(tabla).toHaveTextContent('10');
    expect(tabla).toHaveTextContent('12');
  });
});
