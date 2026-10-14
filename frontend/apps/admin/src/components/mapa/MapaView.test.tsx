import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import type { Plano } from '@licoreria/types';
import { MapaView } from './MapaView';
import { UNIT } from './elementos';

const plano: Plano = {
  id: 'p-1',
  nombre: 'Salón',
  version: 1,
  activo: true,
  anchoFondo: 10,
  altoFondo: 6,
  rejilla: 0.5,
  piso: 'madera',
  elementos: [],
  fondoImagenUrl: '/uploads/planos/croquis.png',
  fondoOpacidad: 0.3,
  fondoAjuste: 'cubrir',
};

describe('MapaView · croquis', () => {
  it('dibuja el fondo con su opacidad y ajuste sin capturar eventos', () => {
    render(<MapaView plano={plano} zonas={[]} />);

    const imagen = screen.getByTestId('fondo-croquis');
    expect(imagen.getAttribute('href')).toMatch(/\/uploads\/planos\/croquis\.png$/);
    expect(imagen).toHaveAttribute('opacity', '0.3');
    expect(imagen).toHaveAttribute('preserveAspectRatio', 'xMidYMid slice');
    expect(imagen).toHaveAttribute('pointer-events', 'none');
    expect(imagen).toHaveAttribute('width', String(10 * UNIT));
    expect(imagen).toHaveAttribute('height', String(6 * UNIT));
  });

  it.each([
    ['contener', 'xMidYMid meet'],
    ['estirar', 'none'],
  ] as const)('traduce el ajuste %s', (ajuste, esperado) => {
    render(<MapaView plano={{ ...plano, fondoAjuste: ajuste }} zonas={[]} />);

    expect(screen.getByTestId('fondo-croquis')).toHaveAttribute('preserveAspectRatio', esperado);
  });

  it('queda debajo de los elementos', () => {
    const { container } = render(
      <MapaView
        plano={{
          ...plano,
          elementos: [{ id: 'e1', zonaId: null, mesaId: null, tipo: 'mesa', forma: 'mesa_redonda', color: null, etiqueta: '1', z: 0, posX: 1, posY: 1, ancho: 2, alto: 2, rotacion: 0 }],
        }}
        zonas={[]}
      />,
    );

    const nodos = Array.from(container.querySelector('svg')!.children);
    const iImagen = nodos.findIndex((n) => n.tagName.toLowerCase() === 'image');
    const iElemento = nodos.findIndex((n) => n.getAttribute('data-el') === 'e1');
    expect(iImagen).toBeGreaterThanOrEqual(0);
    expect(iImagen).toBeLessThan(iElemento);
  });

  it('no dibuja nada sin croquis o con el interruptor local apagado', () => {
    const { rerender } = render(<MapaView plano={{ ...plano, fondoImagenUrl: null }} zonas={[]} />);
    expect(screen.queryByTestId('fondo-croquis')).toBeNull();

    rerender(<MapaView plano={plano} zonas={[]} mostrarCroquis={false} />);
    expect(screen.queryByTestId('fondo-croquis')).toBeNull();
  });

  it('el MapaView de public-web no dibuja el croquis', async () => {
    const fuente = await import('../../../../public-web/src/components/mapa/MapaView.tsx?raw');
    expect(fuente.default).not.toMatch(/fondoImagenUrl|<image/);
  });
});
