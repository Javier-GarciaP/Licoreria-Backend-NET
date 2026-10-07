import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { Modal } from '@licoreria/ui';

describe('Modal', () => {
  it('expone un diálogo accesible con el título', () => {
    render(
      <Modal open onClose={() => undefined} title="Confirmar">
        <p>Contenido</p>
      </Modal>,
    );
    const dialogo = screen.getByRole('dialog');
    expect(dialogo).toHaveAttribute('aria-modal', 'true');
    expect(screen.getByText('Confirmar')).toBeInTheDocument();
  });

  it('se cierra al presionar Escape', async () => {
    const onClose = vi.fn();
    render(
      <Modal open onClose={onClose} title="Confirmar">
        <p>Contenido</p>
      </Modal>,
    );
    await userEvent.keyboard('{Escape}');
    expect(onClose).toHaveBeenCalledTimes(1);
  });

  it('no renderiza nada cuando está cerrado', () => {
    render(
      <Modal open={false} onClose={() => undefined} title="Confirmar">
        <p>Contenido</p>
      </Modal>,
    );
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
  });
});
