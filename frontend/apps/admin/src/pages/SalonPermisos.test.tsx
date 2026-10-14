import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { describe, expect, it, vi } from 'vitest';
import { ProtectedRoute } from '../components/Rbac';
import { filtrarGrupos, itemDeRuta, puedeAcceder } from '../lib/navigation';
import { SalonLayout } from './SalonLayout';

const sesion = { esAdmin: false, permisos: [] as string[], autenticado: true };
vi.mock('../context/AuthContext', () => ({
  useAuth: () => ({
    autenticado: sesion.autenticado,
    inicializando: false,
    esAdmin: sesion.esAdmin,
    tienePermiso: (clave: string) => sesion.permisos.includes(clave),
    tieneAlguno: (claves: string[]) => claves.some((c) => sesion.permisos.includes(c)),
  }),
}));

const HOST = ['club:read', 'club:manage', 'reservation:manage', 'content:read'];
const ADMIN = [...HOST, 'club:layout'];

function renderRuta(ruta: string) {
  return render(
    <MemoryRouter initialEntries={[ruta]}>
      <Routes>
        <Route path="/" element={<p>Inicio</p>} />
        <Route
          path="/salon/planos/:id"
          element={
            <ProtectedRoute permisos={['club:layout']}>
              <p>Editor</p>
            </ProtectedRoute>
          }
        />
        <Route path="/salon" element={<SalonLayout />} />
      </Routes>
    </MemoryRouter>,
  );
}

describe('Salón según permisos', () => {
  it('el Host que abre el editor por URL directa es redirigido', () => {
    sesion.esAdmin = false;
    sesion.permisos = HOST;
    renderRuta('/salon/planos/abc');

    expect(screen.queryByText('Editor')).toBeNull();
    expect(screen.getByText('Inicio')).toBeInTheDocument();
  });

  it('quien tiene club:layout entra al editor', () => {
    sesion.esAdmin = false;
    sesion.permisos = ADMIN;
    renderRuta('/salon/planos/abc');

    expect(screen.getByText('Editor')).toBeInTheDocument();
  });

  it('el Administrador entra al editor', () => {
    sesion.esAdmin = true;
    sesion.permisos = [];
    renderRuta('/salon/planos/abc');

    expect(screen.getByText('Editor')).toBeInTheDocument();
  });

  it('la pestaña Mesas solo aparece con el permiso de distribución', () => {
    sesion.esAdmin = false;
    sesion.permisos = HOST;
    const { unmount } = renderRuta('/salon');
    expect(screen.getByRole('link', { name: 'Planos' })).toBeInTheDocument();
    expect(screen.queryByRole('link', { name: 'Mesas' })).toBeNull();
    unmount();

    sesion.permisos = ADMIN;
    renderRuta('/salon');
    expect(screen.getByRole('link', { name: 'Mesas' })).toBeInTheDocument();
  });
});

describe('Navegación del Salón', () => {
  it('el Host sigue viendo Salón (solo lectura) con club:read y Entradas sigue protegido por club:manage', () => {
    const tiene = (clave: string) => HOST.includes(clave);
    const rutas = filtrarGrupos(false, tiene, 'Host', 'discoteca').flatMap((g) => g.items.map((i) => i.to));

    expect(rutas).toContain('/salon');
    expect(puedeAcceder(itemDeRuta('/salon/planos/abc'), false, tiene, 'Host', 'discoteca')).toBe(true);
    expect(itemDeRuta('/entradas')?.permiso).toBe('club:manage');
  });
});
