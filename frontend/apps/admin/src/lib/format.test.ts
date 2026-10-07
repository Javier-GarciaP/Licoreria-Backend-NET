import { describe, expect, it } from 'vitest';
import { formatNumber, formatUSD, haceCuanto, minutosTranscurridos } from './format';

describe('format', () => {
  it('formatea montos en USD', () => {
    expect(formatUSD(12.5)).toContain('12');
    expect(formatUSD(0)).toContain('0');
  });

  it('formatea números con separador de miles', () => {
    expect(formatNumber(1234)).toBe('1.234');
  });

  it('calcula minutos transcurridos desde una fecha ISO', () => {
    const haceCincoMinutos = new Date(Date.now() - 5 * 60_000).toISOString();
    expect(minutosTranscurridos(haceCincoMinutos)).toBe(5);
    expect(haceCuanto(haceCincoMinutos)).toBe('hace 5 min');
  });

  it('devuelve "ahora" para fechas muy recientes', () => {
    expect(haceCuanto(new Date().toISOString())).toBe('ahora');
  });
});
