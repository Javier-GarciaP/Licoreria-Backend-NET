import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { describe, expect, it } from 'vitest';
import { AuthProvider } from '../context/AuthContext';
import { tokens } from '../lib/api';
import { LoginPage } from './LoginPage';

function renderLogin() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <AuthProvider>
          <LoginPage />
        </AuthProvider>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe('LoginPage', () => {
  it('inicia sesión y persiste el token', async () => {
    const user = userEvent.setup();
    renderLogin();

    await user.type(screen.getByLabelText('Correo'), 'admin@licoreria.com');
    await user.type(screen.getByLabelText('Contraseña'), 'admin123');
    await user.click(screen.getByRole('button', { name: 'Entrar' }));

    await waitFor(() => expect(tokens.access()).toBe('test-access-token'));
  });

  it('no persiste token con credenciales inválidas', async () => {
    const user = userEvent.setup();
    renderLogin();

    await user.type(screen.getByLabelText('Correo'), 'otro@licoreria.com');
    await user.type(screen.getByLabelText('Contraseña'), 'mala');
    await user.click(screen.getByRole('button', { name: 'Entrar' }));

    await waitFor(() => expect(tokens.access()).toBeNull());
  });
});
