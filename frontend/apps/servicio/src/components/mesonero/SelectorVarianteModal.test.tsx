import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import type { Producto, ProductoVariante } from '@licoreria/types';
import { SelectorVarianteModal } from './SelectorVarianteModal';

const variante = (extra: Partial<ProductoVariante>): ProductoVariante =>
  ({
    id: 'v',
    nombre: 'V',
    sku: 'S',
    activo: true,
    esBase: false,
    naturaleza: 'SinDefinir',
    precioVentaUSD: 1,
    cantidad: 0,
    cantidadReservada: 0,
    porcionesDisponibles: 0,
    porcionesPotenciales: 0,
    envasesCerradosAbribles: 0,
    ...extra,
  }) as ProductoVariante;

const cola = (vaso: Partial<ProductoVariante>) =>
  ({
    id: 'p',
    nombre: 'Coca-Cola',
    areaDestino: 'Barra',
    variantes: [
      variante({ id: 'l1', nombre: 'Botella 1 L', esBase: true, naturaleza: 'Presentacion', cantidad: 5 }),
      variante({ id: 'l2', nombre: 'Botella 2 L', esBase: true, naturaleza: 'Presentacion', cantidad: 2, cantidadReservada: 2 }),
      variante({ id: 'vaso', nombre: 'Vaso', naturaleza: 'Porcion', ...vaso }),
    ],
  }) as unknown as Producto;

function abrir(producto: Producto) {
  const onElegir = vi.fn();
  render(<SelectorVarianteModal producto={producto} onCerrar={() => undefined} onElegir={onElegir} />);
  return { onElegir };
}

describe('SelectorVarianteModal', () => {
  it('muestra la existencia de cada presentación y deshabilita la agotada', () => {
    abrir(cola({ porcionesDisponibles: 3 }));
    expect(screen.getByRole('button', { name: /Botella 1 L.*Quedan 5/ })).toBeEnabled();
    expect(screen.getByRole('button', { name: /Botella 2 L.*Agotado/ })).toBeDisabled();
  });

  it('permite pedir un vaso cuando hay envase abierto', async () => {
    const { onElegir } = abrir(cola({ porcionesDisponibles: 3 }));
    const boton = screen.getByRole('button', { name: /Vaso.*3 porciones abiertas/ });
    await userEvent.click(boton);
    expect(onElegir).toHaveBeenCalledWith(expect.anything(), expect.objectContaining({ id: 'vaso' }));
  });

  it('no permite pedir un vaso sin abrir y pide avisar a barra', async () => {
    const { onElegir } = abrir(cola({ envasesCerradosAbribles: 2 }));
    const boton = screen.getByRole('button', { name: /Vaso.*Sin abrir · 2 botellas/ });
    expect(boton).toBeDisabled();
    expect(screen.getByText('Pide a barra que destape una botella')).toBeInTheDocument();
    await userEvent.click(boton);
    expect(onElegir).not.toHaveBeenCalled();
  });

  it('marca el vaso agotado cuando no hay envases ni botellas', () => {
    abrir(cola({}));
    expect(screen.getByRole('button', { name: /Vaso.*Agotado/ })).toBeDisabled();
  });
});
