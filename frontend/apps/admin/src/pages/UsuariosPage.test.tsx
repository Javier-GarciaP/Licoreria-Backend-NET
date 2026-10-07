import { render, screen } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { describe, expect, it } from 'vitest';
import { UsuariosPage } from './UsuariosPage';

function renderPagina() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <UsuariosPage />
    </QueryClientProvider>,
  );
}

describe('UsuariosPage', () => {
  it('renderiza el encabezado y el botón de nuevo usuario', async () => {
    renderPagina();
    expect(await screen.findByRole('button', { name: 'Nuevo usuario' })).toBeInTheDocument();
    expect(screen.getByText('Usuarios')).toBeInTheDocument();
  });
});
