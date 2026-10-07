import { createRef } from 'react';
import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { Select } from '@licoreria/ui';

describe('Select', () => {
  it('renderiza la etiqueta asociada al campo', () => {
    render(
      <Select label="Motivo">
        <option value="a">A</option>
      </Select>,
    );
    expect(screen.getByLabelText('Motivo')).toBeInTheDocument();
  });

  it('reenvía el ref al elemento select (compatibilidad con react-hook-form)', () => {
    const ref = createRef<HTMLSelectElement>();
    render(
      <Select label="Motivo" ref={ref}>
        <option value="a">A</option>
      </Select>,
    );
    expect(ref.current?.tagName).toBe('SELECT');
  });

  it('muestra el mensaje de error', () => {
    render(
      <Select label="Motivo" error="Selecciona una opción">
        <option value="a">A</option>
      </Select>,
    );
    expect(screen.getByText('Selecciona una opción')).toBeInTheDocument();
  });
});
