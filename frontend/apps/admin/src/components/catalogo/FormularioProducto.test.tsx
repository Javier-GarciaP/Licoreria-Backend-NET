import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { http, HttpResponse } from 'msw';
import { describe, expect, it, vi } from 'vitest';
import { server } from '../../test/server';
import { FormularioProducto, type DatosProductoForm } from './FormularioProducto';

const BASE = 'http://localhost:5190';

const categorias = [
  { id: 'cat-1', nombre: 'Licores', descripcion: null, categoriaPadreId: null, activo: true },
];
const marcas = [{ id: 'mar-1', nombre: 'Cacique', descripcion: null, activo: true }];
const unidades = [{ id: 'uni-1', nombre: 'Botella', abreviatura: 'BOT' }];

function handlerPaginaVacia() {
  return HttpResponse.json({ items: [], page: 1, pageSize: 100, totalItems: 0, totalPages: 1 });
}

function renderForm() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  const onGuardar = vi.fn<(datos: DatosProductoForm) => void>();
  render(
    <QueryClientProvider client={queryClient}>
      <FormularioProducto
        categorias={categorias}
        marcas={marcas}
        unidades={unidades}
        guardando={false}
        onCancelar={() => undefined}
        onGuardar={onGuardar}
      />
    </QueryClientProvider>,
  );
  return { onGuardar };
}

async function completarDatosBasicos() {
  const [nombreProducto, nombreVariante] = screen.getAllByLabelText('Nombre');
  await userEvent.type(nombreProducto, 'Trago de Prueba');
  await userEvent.selectOptions(screen.getByLabelText('Categoría'), 'cat-1');
  await userEvent.type(nombreVariante, 'Trago');
  await userEvent.selectOptions(screen.getByLabelText('Unidad'), 'uni-1');
}

describe('FormularioProducto', () => {
  it('muestra el editor de consumo al elegir Preparado sin desmarcar la base', async () => {
    server.use(
      http.get(`${BASE}/api/v1/stock`, handlerPaginaVacia),
      http.get(`${BASE}/api/v1/productos`, handlerPaginaVacia),
    );
    renderForm();

    expect(screen.queryByText('Consumo por unidad vendida')).not.toBeInTheDocument();

    await userEvent.selectOptions(screen.getByLabelText('Tipo'), 'Preparado');

    expect(await screen.findByText('Consumo por unidad vendida')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Agregar insumo' })).toBeInTheDocument();
    // Un preparado nunca se ordena al proveedor: no ofrece presentación base.
    expect(screen.queryByRole('checkbox', { name: /Presentación base/ })).not.toBeInTheDocument();
    expect(screen.getByText(/no lleva stock propio ni se ordena al proveedor/)).toBeInTheDocument();
  });

  it('impide guardar un Preparado que no tenga receta', async () => {
    server.use(
      http.get(`${BASE}/api/v1/stock`, handlerPaginaVacia),
      http.get(`${BASE}/api/v1/productos`, handlerPaginaVacia),
    );
    const { onGuardar } = renderForm();

    await completarDatosBasicos();
    await userEvent.selectOptions(screen.getByLabelText('Tipo'), 'Preparado');
    await userEvent.click(screen.getByRole('button', { name: 'Crear producto' }));

    await waitFor(() =>
      expect(screen.getByText(/Un preparado debe consumir al menos un insumo/)).toBeInTheDocument(),
    );
    expect(onGuardar).not.toHaveBeenCalled();
  });

  it('permite guardar un Preparado cuando tiene insumo y cantidad', async () => {
    server.use(
      http.get(`${BASE}/api/v1/stock`, handlerPaginaVacia),
      http.get(
        `${BASE}/api/v1/productos`,
        () =>
          HttpResponse.json({
            items: [
              {
                id: 'prod-licor',
                nombre: 'Ron Cacique',
                descripcion: null,
                categoriaId: 'cat-1',
                categoriaNombre: 'Licores',
                marcaId: null,
                marcaNombre: null,
                impuestoId: null,
                tipo: 'Simple',
                areaDestino: 'Barra',
                gradoAlcoholico: null,
                imagenUrl: null,
                activo: true,
                variantes: [
                  {
                    id: 'var-licor',
                    nombre: 'Botella 0.75L',
                    sku: 'RON-75',
                    precioCompraUSD: 10,
                    precioVentaUSD: 20,
                    unidadMedidaId: 'uni-1',
                    unidadMedidaNombre: 'BOT',
                    activo: true,
                    esBase: true,
                    codigosBarras: [],
                    cantidad: 0,
                    cantidadReservada: 0,
                    stockMinimo: 0,
                    stockMaximo: 0,
                  },
                ],
              },
            ],
            page: 1,
            pageSize: 100,
            totalItems: 1,
            totalPages: 1,
          }),
      ),
    );
    const { onGuardar } = renderForm();

    await completarDatosBasicos();
    await userEvent.selectOptions(screen.getByLabelText('Tipo'), 'Preparado');
    await userEvent.click(screen.getByRole('button', { name: 'Agregar insumo' }));

    const insumo = await screen.findByLabelText('Insumo');
    await userEvent.selectOptions(insumo, 'var-licor');
    await userEvent.type(screen.getByLabelText('Cantidad por unidad'), '0.05');
    await userEvent.click(screen.getByRole('button', { name: 'Crear producto' }));

    await waitFor(() => expect(onGuardar).toHaveBeenCalledTimes(1));
    const datos = onGuardar.mock.calls[0][0];
    expect(datos.tipo).toBe('Preparado');
    expect(datos.variantes[0].esBase).toBe(false);
    expect(datos.variantes[0].recetas).toEqual([{ varianteInsumoId: 'var-licor', cantidad: 0.05 }]);
  });
});
