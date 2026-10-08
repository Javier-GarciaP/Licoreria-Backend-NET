import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { http, HttpResponse } from 'msw';
import { describe, expect, it, vi } from 'vitest';
import { server } from '../test/server';
import { formatUSD } from '../lib/format';
import { PosPage } from './PosPage';

const BASE = 'http://localhost:5190';

const categoria = {
  id: 'cat-1',
  nombre: 'Whisky',
  descripcion: null,
  categoriaPadreId: null,
  activo: true,
};

const varianteSimple = {
  id: 'var-1',
  nombre: '750ml',
  sku: 'WH-750',
  precioCompraUSD: 10,
  precioVentaUSD: 20,
  unidadMedidaId: 'u-1',
  unidadMedidaNombre: 'Botella',
  activo: true,
  codigosBarras: [],
};

const productoSimple = {
  id: 'prod-1',
  nombre: 'Whisky Etiqueta Negra',
  descripcion: null,
  categoriaId: 'cat-1',
  categoriaNombre: 'Whisky',
  marcaId: null,
  marcaNombre: null,
  impuestoId: null,
  tipo: 'Simple',
  gradoAlcoholico: 40,
  imagenUrl: null,
  activo: true,
  variantes: [varianteSimple],
};

const productoVariantes = {
  ...productoSimple,
  id: 'prod-2',
  nombre: 'Ron Añejo',
  variantes: [
    { ...varianteSimple, id: 'var-2a', nombre: '750ml', precioVentaUSD: 15 },
    { ...varianteSimple, id: 'var-2b', nombre: '1L', sku: 'RO-1L', precioVentaUSD: 28 },
  ],
};

const productoReceta = {
  ...productoSimple,
  id: 'prod-3',
  nombre: 'Mojito',
  tipo: 'Preparado',
  variantes: [{ ...varianteSimple, id: 'var-3', nombre: 'Vaso', precioVentaUSD: 8 }],
};

function usarHandlers(onVenta?: (body: unknown) => void) {
  server.use(
    http.get(`${BASE}/api/v1/categorias`, () => HttpResponse.json([categoria])),
    http.get(`${BASE}/api/v1/productos`, () =>
      HttpResponse.json({
        items: [productoSimple, productoVariantes, productoReceta],
        page: 1,
        pageSize: 100,
        totalItems: 3,
        totalPages: 1,
      }),
    ),
    http.get(`${BASE}/api/v1/metodos-pago`, () =>
      HttpResponse.json([{ id: 'met-1', codigo: 'EFECTIVO', nombre: 'Efectivo' }]),
    ),
    http.get(`${BASE}/api/v1/promociones`, () => HttpResponse.json([])),
    http.get(`${BASE}/api/v1/tasas-cambio/actual`, () =>
      HttpResponse.json({ id: 't-1', fecha: new Date().toISOString(), tipo: 'Paralelo', valor: 40 }),
    ),
    http.get(`${BASE}/api/v1/productos/prod-2/modificadores`, () => HttpResponse.json([])),
    http.get(`${BASE}/api/v1/productos/prod-3/modificadores`, () =>
      HttpResponse.json([
        {
          id: 'pm-1',
          modificadorId: 'mod-1',
          modificadorNombre: 'Doble hielo',
          precioAdicional: 1.5,
          minimo: 0,
          maximo: 1,
          requerido: false,
        },
      ]),
    ),
    http.get(`${BASE}/api/v1/productos/prod-3/recetas`, () => HttpResponse.json([])),
    http.get(`${BASE}/api/v1/local-info`, () =>
      HttpResponse.json({
        id: 'local-1',
        nombre: 'Licorería Test',
        descripcion: '',
        direccion: 'Av. Principal 123',
        telefono: '0212-0000000',
        whatsapp: '',
        email: 'hola@licoreria.test',
        instagram: null,
        facebook: null,
        mapaUrl: null,
        logoUrl: null,
      }),
    ),
    http.post(`${BASE}/api/v1/ventas`, async ({ request }) => {
      onVenta?.(await request.json());
      return HttpResponse.json({
        id: 'venta-1',
        fecha: new Date().toISOString(),
        tasaCambio: 40,
        subtotalUSD: 20,
        descuentoUSD: 0,
        totalUSD: 20,
        totalBS: 800,
        estado: 'Pagada',
        usuarioId: 'u-1',
        numeroComprobante: '0001',
        detalles: [],
        pagos: [],
      });
    }),
  );
}

function renderPos() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <PosPage />
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe('PosPage', () => {
  it('muestra categorías y productos del catálogo', async () => {
    usarHandlers();
    renderPos();

    expect(await screen.findByRole('tab', { name: 'Todas' })).toBeInTheDocument();
    expect(await screen.findByRole('tab', { name: 'Whisky' })).toBeInTheDocument();
    expect(await screen.findByRole('gridcell', { name: /Whisky Etiqueta Negra/ })).toBeInTheDocument();
    expect(screen.getByRole('gridcell', { name: /Mojito/ })).toBeInTheDocument();
  });

  it('agrega un producto de una sola variante con un clic', async () => {
    usarHandlers();
    renderPos();

    const tile = await screen.findByRole('gridcell', { name: /Whisky Etiqueta Negra/ });
    await userEvent.click(tile);

    expect(screen.getByText('1×')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Cobrar/ }).textContent).toContain(formatUSD(20));
  });

  it('abre el selector de variantes y agrega la variante elegida', async () => {
    usarHandlers();
    renderPos();

    await userEvent.click(await screen.findByRole('gridcell', { name: /Ron Añejo/ }));

    const dialog = await screen.findByRole('dialog', { name: 'Ron Añejo' });
    await userEvent.click(within(dialog).getByRole('button', { name: /1L/ }));
    await userEvent.click(within(dialog).getByRole('button', { name: /^Agregar / }));

    await waitFor(() =>
      expect(screen.getByRole('button', { name: /Cobrar/ }).textContent).toContain(formatUSD(28)),
    );
  });

  it('suma los modificadores al precio de la línea', async () => {
    usarHandlers();
    renderPos();

    await userEvent.click(await screen.findByRole('gridcell', { name: /Mojito/ }));
    const dialog = await screen.findByRole('dialog', { name: 'Mojito' });

    await userEvent.click(within(dialog).getByRole('button', { name: /Doble hielo/ }));
    await userEvent.click(within(dialog).getByRole('button', { name: /^Agregar / }));

    expect(await screen.findByText(/\+ Doble hielo/)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Cobrar/ }).textContent).toContain(formatUSD(9.5));
  });

  it('ajusta la cantidad de la línea seleccionada con el atajo +', async () => {
    usarHandlers();
    renderPos();

    await userEvent.click(await screen.findByRole('gridcell', { name: /Whisky Etiqueta Negra/ }));
    expect(screen.getByText('1×')).toBeInTheDocument();

    await userEvent.keyboard('+');

    expect(await screen.findByText('2×')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Cobrar/ }).textContent).toContain(formatUSD(40));
  });

  it('abre el buscador al pulsar una letra en cualquier parte', async () => {
    usarHandlers();
    renderPos();

    await screen.findByRole('gridcell', { name: /Whisky Etiqueta Negra/ });
    await userEvent.keyboard('c');

    const buscador = screen.getByLabelText('Buscar producto') as HTMLInputElement;
    expect(buscador).toHaveValue('c');
    expect(buscador).toHaveFocus();
  });

  it('no cambia de categoría al navegar productos con flechas', async () => {
    usarHandlers();
    renderPos();

    await userEvent.click(await screen.findByRole('gridcell', { name: /Whisky Etiqueta Negra/ }));
    await userEvent.keyboard('{ArrowRight}');

    expect(screen.getByRole('tab', { name: 'Todas' })).toHaveAttribute('aria-selected', 'true');
    expect(screen.getByRole('tab', { name: 'Whisky' })).toHaveAttribute('aria-selected', 'false');
  });

  it('muestra el ticket al cobrar', async () => {
    usarHandlers();
    renderPos();

    await userEvent.click(await screen.findByRole('gridcell', { name: /Whisky Etiqueta Negra/ }));
    await userEvent.click(screen.getByRole('button', { name: /Cobrar total/ }));

    expect(await screen.findByRole('dialog', { name: 'Venta registrada' })).toBeInTheDocument();
    expect(await screen.findByText('Licorería Test')).toBeInTheDocument();
    expect(screen.getByText('¡Gracias por su compra!')).toBeInTheDocument();
  });

  it('cobra el total automáticamente al registrar la venta', async () => {
    const onVenta = vi.fn();
    usarHandlers(onVenta);
    renderPos();

    await userEvent.click(await screen.findByRole('gridcell', { name: /Whisky Etiqueta Negra/ }));

    const cobrar = screen.getByRole('button', { name: /Cobrar total/ });
    await waitFor(() => expect(cobrar).toBeEnabled());
    await userEvent.click(cobrar);

    await waitFor(() => expect(onVenta).toHaveBeenCalledTimes(1));
    const cuerpo = onVenta.mock.calls[0][0] as {
      items: unknown[];
      pagos: { monto: number; metodoPagoId: string }[];
    };
    expect(cuerpo.items).toHaveLength(1);
    expect(cuerpo.pagos).toHaveLength(1);
    expect(cuerpo.pagos[0].metodoPagoId).toBe('met-1');
    expect(cuerpo.pagos[0].monto).toBe(20);
  });
});
