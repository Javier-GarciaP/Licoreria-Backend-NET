import { describe, expect, it } from 'vitest';
import type { MetodoPago } from '@licoreria/types';
import { esMetodoCredito, fechaEnDias, metodoPredeterminado, vencimientoIso } from './credito';

const metodos: MetodoPago[] = [
  { id: 'm1', codigo: 'PagoMovil', nombre: 'Pago móvil' },
  { id: 'm2', codigo: 'EfectivoBS', nombre: 'Efectivo en bolívares' },
  { id: 'm3', codigo: 'EfectivoUSD', nombre: 'Efectivo en dólares' },
  { id: 'm4', codigo: 'Credito', nombre: 'Crédito' },
];

describe('credito', () => {
  it('identifica el método Crédito por su código', () => {
    expect(esMetodoCredito(metodos[3])).toBe(true);
    expect(esMetodoCredito(metodos[0])).toBe(false);
    expect(esMetodoCredito(null)).toBe(false);
  });

  it('el método por defecto es el efectivo de la moneda elegida', () => {
    expect(metodoPredeterminado(metodos, 'USD')?.id).toBe('m3');
    expect(metodoPredeterminado(metodos, 'BS')?.id).toBe('m2');
  });

  it('sin efectivo usa el primer método y sin métodos devuelve null', () => {
    expect(metodoPredeterminado([metodos[0]], 'USD')?.id).toBe('m1');
    expect(metodoPredeterminado([], 'USD')).toBeNull();
  });

  it('calcula el vencimiento por defecto a 30 días', () => {
    expect(fechaEnDias(30, new Date(2026, 9, 14))).toBe('2026-11-13');
    expect(fechaEnDias(30, new Date(2026, 11, 15))).toBe('2027-01-14');
  });

  it('envía el vencimiento al final del día como instante ISO', () => {
    expect(new Date(vencimientoIso('2026-11-13')).getDate()).toBe(13);
  });
});
