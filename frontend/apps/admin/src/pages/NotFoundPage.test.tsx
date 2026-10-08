import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { describe, expect, it } from 'vitest';
import { NotFoundPage } from './NotFoundPage';

function renderPagina(ruta = '/ruta-que-no-existe') {
  return render(
    <MemoryRouter initialEntries={[ruta]}>
      <NotFoundPage />
    </MemoryRouter>,
  );
}

describe('NotFoundPage', () => {
  it('muestra el estado 404 y la ruta fallida', () => {
    renderPagina('/inventario/kardexx');
    expect(screen.getByText('404 · No encontrado')).toBeInTheDocument();
    expect(screen.getByRole('heading', { name: 'ESA RUTA NO ESTÁ EN LA CARTA' })).toBeInTheDocument();
    expect(screen.getByText('/inventario/kardexx')).toBeInTheDocument();
  });

  it('ofrece volver al inicio y abrir una venta', () => {
    renderPagina();
    expect(screen.getByRole('link', { name: 'Volver al inicio' })).toHaveAttribute('href', '/');
    expect(screen.getByRole('link', { name: 'Nueva venta' })).toHaveAttribute('href', '/pos');
  });
});