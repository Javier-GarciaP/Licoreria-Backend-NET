import { createRef } from 'react';
import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { Input } from '@licoreria/ui';

describe('Input', () => {
  it('renderiza la etiqueta asociada al campo', () => {
    render(<Input label="Correo" placeholder="test" />);
    expect(screen.getByLabelText('Correo')).toBeInTheDocument();
  });

  it('reenvía el ref al elemento input (compatibilidad con react-hook-form)', () => {
    const ref = createRef<HTMLInputElement>();
    render(<Input label="Correo" ref={ref} />);
    expect(ref.current).toBeInstanceOf(HTMLInputElement);
    expect(ref.current?.tagName).toBe('INPUT');
  });

  it('muestra el mensaje de error', () => {
    render(<Input label="Correo" error="Correo inválido" />);
    expect(screen.getByText('Correo inválido')).toBeInTheDocument();
  });
});
