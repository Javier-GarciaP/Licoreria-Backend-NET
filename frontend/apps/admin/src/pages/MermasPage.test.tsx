import { render, screen } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { describe, expect, it } from 'vitest';
import { MermasPage } from './MermasPage';

function renderPagina() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <MermasPage />
    </QueryClientProvider>,
  );
}

describe('MermasPage', () => {
  it('renderiza el formulario de merma', async () => {
    renderPagina();
    expect(await screen.findByLabelText('Producto / variante')).toBeInTheDocument();
    expect(screen.getByLabelText('Motivo')).toBeInTheDocument();
    expect(screen.getByLabelText('Cantidad')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Registrar merma' })).toBeInTheDocument();
  });

  it('lista las variantes del endpoint de stock (respuesta paginada)', async () => {
    renderPagina();
    expect(await screen.findByRole('option', { name: /Ron Añejo/ })).toBeInTheDocument();
  });
});
