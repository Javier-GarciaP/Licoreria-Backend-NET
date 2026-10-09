import { fireEvent, render, screen } from '@testing-library/react';
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

async function abrirModal() {
  const boton = await screen.findByRole('button', { name: 'Nueva merma' });
  fireEvent.click(boton);
}

describe('MermasPage', () => {
  it('abre el formulario de merma desde el botón Nueva merma', async () => {
    renderPagina();
    await abrirModal();
    expect(screen.getByLabelText('Producto / variante')).toBeInTheDocument();
    expect(screen.getByLabelText('Motivo')).toBeInTheDocument();
    expect(screen.getByLabelText('Cantidad')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Registrar merma' })).toBeInTheDocument();
  });

  it('lista las variantes del endpoint de stock (respuesta paginada)', async () => {
    renderPagina();
    await abrirModal();
    expect(await screen.findByRole('option', { name: /Ron Añejo/ })).toBeInTheDocument();
  });
});