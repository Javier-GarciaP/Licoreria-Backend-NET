import { render, screen } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { describe, expect, it } from 'vitest';
import type { Venta } from '@licoreria/types';
import { TicketVenta } from './TicketVenta';

const base: Venta = {
  id: 'venta-1234567',
  fecha: '2026-10-14T15:00:00Z',
  tasaCambio: 40,
  subtotalUSD: 40,
  descuentoUSD: 0,
  totalUSD: 40,
  totalBS: 1600,
  estado: 'Completada',
  usuarioId: 'u-1',
  sesionCajaId: null,
  numeroComprobante: 'F-00000001',
  detalles: [],
  pagos: [],
};

function renderTicket(venta: Venta) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <TicketVenta venta={venta} onCerrar={() => undefined} />
    </QueryClientProvider>,
  );
}

describe('TicketVenta', () => {
  it('muestra cliente, puntos ganados y monto a crédito cuando existen', () => {
    renderTicket({ ...base, clienteId: 'c-1', clienteNombre: 'Ana Pérez', puntosGanados: 30, creditoUSD: 10 });

    expect(screen.getByText('Ana Pérez')).toBeInTheDocument();
    expect(screen.getByText('Puntos ganados')).toBeInTheDocument();
    expect(screen.getByText('+30')).toBeInTheDocument();
    expect(screen.getByText('A crédito (fiado)')).toBeInTheDocument();
  });

  it('no muestra cliente, puntos ni crédito en una venta sin ellos', () => {
    renderTicket(base);

    expect(screen.queryByText('Cliente')).toBeNull();
    expect(screen.queryByText('Puntos ganados')).toBeNull();
    expect(screen.queryByText('A crédito (fiado)')).toBeNull();
  });
});
