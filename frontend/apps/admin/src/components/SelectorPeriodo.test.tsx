import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, useLocation } from 'react-router-dom';
import { describe, expect, it } from 'vitest';
import { usePeriodo } from '../hooks/usePeriodo';
import { SelectorPeriodo } from './SelectorPeriodo';

function Probe() {
  const periodo = usePeriodo();
  const ubicacion = useLocation();
  return (
    <>
      <SelectorPeriodo periodo={periodo} onElegir={periodo.elegir} onRango={periodo.fijarRango} />
      <output data-testid="url">{ubicacion.search}</output>
      <output data-testid="rango">{`${periodo.desde}|${periodo.hasta}|${periodo.valido}`}</output>
    </>
  );
}

const renderizar = (url = '/') =>
  render(
    <MemoryRouter initialEntries={[url]}>
      <Probe />
    </MemoryRouter>,
  );

describe('SelectorPeriodo', () => {
  it('ofrece los cinco periodos y marca los últimos 30 días por defecto', () => {
    renderizar();

    for (const nombre of ['Hoy', 'Últimos 7 días', 'Últimos 30 días', 'Mes actual', 'Personalizado']) {
      expect(screen.getByRole('button', { name: nombre })).toBeInTheDocument();
    }
    expect(screen.getByRole('button', { name: 'Últimos 30 días' })).toHaveAttribute('aria-pressed', 'true');
  });

  it.each([
    ['Hoy', 'hoy'],
    ['Últimos 7 días', '7d'],
    ['Mes actual', 'mes'],
  ])('al elegir %s guarda el periodo en la URL', async (nombre, id) => {
    renderizar();

    await userEvent.click(screen.getByRole('button', { name: nombre }));

    expect(screen.getByTestId('url')).toHaveTextContent(`periodo=${id}`);
    expect(screen.getByRole('button', { name: nombre })).toHaveAttribute('aria-pressed', 'true');
  });

  it('restaura el periodo desde la URL', () => {
    renderizar('/?periodo=7d');

    expect(screen.getByRole('button', { name: 'Últimos 7 días' })).toHaveAttribute('aria-pressed', 'true');
  });

  it('personalizado muestra las fechas y acepta un rango válido', async () => {
    renderizar('/?periodo=personalizado&desde=2026-10-01&hasta=2026-10-13');

    expect(screen.getByLabelText('Fecha inicial')).toHaveValue('2026-10-01');
    expect(screen.getByLabelText('Fecha final')).toHaveValue('2026-10-13');
    expect(screen.queryByRole('alert')).not.toBeInTheDocument();
    expect(screen.getByTestId('rango')).toHaveTextContent('2026-10-01|2026-10-13|true');
  });

  it('un rango invertido muestra el motivo y queda marcado como no consultable', () => {
    renderizar('/?periodo=personalizado&desde=2026-10-13&hasta=2026-10-01');

    expect(screen.getByRole('alert')).toHaveTextContent(/final no puede ser anterior/);
    expect(screen.getByTestId('rango')).toHaveTextContent('2026-10-13|2026-10-01|false');
  });

  it('al cambiar de un periodo personalizado a otro se limpian las fechas de la URL', async () => {
    renderizar('/?periodo=personalizado&desde=2026-10-01&hasta=2026-10-05');

    await userEvent.click(screen.getByRole('button', { name: 'Hoy' }));

    expect(screen.getByTestId('url')).toHaveTextContent('?periodo=hoy');
    expect(screen.getByTestId('url')).not.toHaveTextContent('desde=');
  });
});
