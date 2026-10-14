import { describe, expect, it } from 'vitest';
import type { Plano } from '@licoreria/types';
import { planoAPayload } from './plano';

const plano: Plano = {
  id: '11111111-1111-1111-1111-111111111111',
  nombre: 'Salón',
  version: 3,
  activo: true,
  anchoFondo: 24,
  altoFondo: 16,
  rejilla: 0.5,
  piso: 'madera',
  fondoImagenUrl: '/uploads/planos/abc.png',
  fondoOpacidad: 0.3,
  fondoAjuste: 'cubrir',
  esPlantilla: false,
  elementos: [],
};

describe('planoAPayload', () => {
  it('incluye opacidad y ajuste del fondo pero nunca la URL ni el indicador de plantilla', () => {
    const payload = planoAPayload(plano);

    expect(payload.fondoOpacidad).toBe(0.3);
    expect(payload.fondoAjuste).toBe('cubrir');
    expect(payload).not.toHaveProperty('fondoImagenUrl');
    expect(payload).not.toHaveProperty('esPlantilla');
  });

  it('usa los valores por defecto cuando el plano no trae fondo', () => {
    const payload = planoAPayload({ ...plano, fondoOpacidad: undefined, fondoAjuste: undefined });

    expect(payload.fondoOpacidad).toBe(0.5);
    expect(payload.fondoAjuste).toBe('contener');
  });
});
