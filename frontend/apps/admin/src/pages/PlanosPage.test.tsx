import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import '@licoreria/auth'; // configura la URL base del cliente HTTP
import { server } from '../test/server';
import { PlanosPage } from './PlanosPage';

vi.mock('sonner', () => ({
  toast: Object.assign(vi.fn(), { warning: vi.fn(), error: vi.fn(), success: vi.fn(), info: vi.fn() }),
  Toaster: () => null,
}));

const sesion = { esAdmin: false, permisos: [] as string[] };
vi.mock('../context/AuthContext', () => ({
  useAuth: () => ({
    esAdmin: sesion.esAdmin,
    tienePermiso: (clave: string) => sesion.permisos.includes(clave),
    tieneAlguno: (claves: string[]) => claves.some((c) => sesion.permisos.includes(c)),
  }),
}));

const BASE = 'http://localhost:5190';

const plano = (id: string, nombre: string, extra: Record<string, unknown> = {}) => ({
  id,
  nombre,
  version: 1,
  activo: false,
  anchoFondo: 24,
  altoFondo: 16,
  rejilla: 0.5,
  piso: 'madera',
  elementos: [],
  fondoImagenUrl: null,
  fondoOpacidad: 0.5,
  fondoAjuste: 'contener',
  esPlantilla: false,
  ...extra,
});

const salon = plano('p-1', 'Salón principal', { activo: true });
const plantilla = plano('t-1', 'Terraza base', { esPlantilla: true });

function renderPagina() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={['/salon']}>
        <Routes>
          <Route path="/salon" element={<PlanosPage />} />
          <Route path="/salon/planos/:id" element={<p>Editor</p>} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

beforeEach(() => {
  sesion.esAdmin = true;
  sesion.permisos = [];
  server.use(
    http.get(`${BASE}/api/v1/planos`, ({ request }) =>
      HttpResponse.json(new URL(request.url).searchParams.get('plantillas') === 'true' ? [salon, plantilla] : [salon]),
    ),
    http.get(`${BASE}/api/v1/zonas`, () => HttpResponse.json([])),
    http.get(`${BASE}/api/v1/mesas`, () => HttpResponse.json([])),
  );
});

describe('PlanosPage · Administrador', () => {
  it('lista los planos y las plantillas por separado', async () => {
    renderPagina();

    expect(await screen.findByText('Salón principal')).toBeInTheDocument();
    const plantillas = await screen.findByRole('region', { name: 'Plantillas' });
    expect(within(plantillas).getByText('Terraza base')).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Nuevo desde plantilla' })).toBeInTheDocument();
  });

  it('duplica un plano: pide el nombre y llama a duplicar sin plantilla', async () => {
    let cuerpo: unknown = null;
    server.use(
      http.post(`${BASE}/api/v1/planos/p-1/duplicar`, async ({ request }) => {
        cuerpo = await request.json();
        return HttpResponse.json(plano('p-2', 'Evento', { activo: false }), { status: 201 });
      }),
    );
    renderPagina();

    await userEvent.click(await screen.findByRole('button', { name: 'Duplicar' }));
    const campo = await screen.findByLabelText('Nombre');
    expect(campo).toHaveValue('Salón principal (copia)');
    await userEvent.clear(campo);
    await userEvent.type(campo, 'Evento');
    await userEvent.click(within(await screen.findByRole('dialog')).getByRole('button', { name: 'Duplicar' }));

    await waitFor(() => expect(cuerpo).toEqual({ nombre: 'Evento', comoPlantilla: false }));
  });

  it('guarda un plano como plantilla', async () => {
    let cuerpo: unknown = null;
    server.use(
      http.post(`${BASE}/api/v1/planos/p-1/duplicar`, async ({ request }) => {
        cuerpo = await request.json();
        return HttpResponse.json(plano('t-2', 'Base', { esPlantilla: true }), { status: 201 });
      }),
    );
    renderPagina();

    await userEvent.click(await screen.findByRole('button', { name: 'Guardar como plantilla' }));
    await userEvent.click(await screen.findByRole('button', { name: 'Guardar plantilla' }));

    await waitFor(() => expect(cuerpo).toEqual({ nombre: 'Salón principal (plantilla)', comoPlantilla: true }));
  });

  it('crea un plano nuevo desde una plantilla (inactivo, sin marcarlo plantilla)', async () => {
    let cuerpo: unknown = null;
    server.use(
      http.post(`${BASE}/api/v1/planos/t-1/duplicar`, async ({ request }) => {
        cuerpo = await request.json();
        return HttpResponse.json(plano('p-3', 'Viernes', { activo: false }), { status: 201 });
      }),
    );
    renderPagina();

    await userEvent.click(await screen.findByRole('button', { name: 'Nuevo desde plantilla' }));
    await userEvent.type(await screen.findByLabelText('Nombre'), 'Viernes');
    await userEvent.click(screen.getByRole('button', { name: 'Crear plano' }));

    await waitFor(() => expect(cuerpo).toEqual({ nombre: 'Viernes', comoPlantilla: false }));
  });
});

describe('PlanosPage · sin permiso de distribución', () => {
  it('el Host ve el plano activo en solo lectura, sin controles de edición', async () => {
    sesion.esAdmin = false;
    sesion.permisos = ['club:read', 'club:manage'];
    renderPagina();

    expect(await screen.findByText(/solo lectura/i)).toBeInTheDocument();
    expect(screen.getByText('Salón principal')).toBeInTheDocument();
    for (const nombre of [/nuevo plano/i, /duplicar/i, /plantilla/i, /eliminar/i]) {
      expect(screen.queryByRole('button', { name: nombre })).toBeNull();
    }
  });
});
