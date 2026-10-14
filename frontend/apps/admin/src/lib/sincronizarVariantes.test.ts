import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { Producto } from '@licoreria/types';

vi.mock('@licoreria/api-client', () => ({
  catalogoApi: {
    recetas: vi.fn(async () => []),
    eliminarReceta: vi.fn(),
    agregarReceta: vi.fn(),
    guardarOrigenes: vi.fn(),
  },
}));

import { catalogoApi } from '@licoreria/api-client';
import { sincronizarVariantes } from './sincronizarVariantes';
import type { DatosProductoForm } from '../components/catalogo/FormularioProducto';

const variante = (id: string, sku: string, extra: Record<string, unknown> = {}) =>
  ({ id, sku, nombre: sku, esBase: false, naturaleza: 'Presentacion', ...extra }) as unknown as Producto['variantes'][number];

const formVariante = (clave: string, sku: string, extra: Record<string, unknown> = {}) => ({
  clave,
  sku,
  nombre: sku,
  unidadMedidaId: 'u',
  precioCompraUSD: '',
  precioVentaUSD: '',
  esBase: false,
  stockInicial: '',
  stockMinimo: '',
  stockMaximo: '',
  recetas: [],
  ...extra,
});

describe('sincronizarVariantes', () => {
  beforeEach(() => vi.clearAllMocks());

  it('guarda los orígenes del vaso resolviendo las claves a los ids guardados', async () => {
    const datos = {
      variantes: [
        formVariante('n-1', 'COLA-1L', { naturaleza: 'Presentacion', esBase: true }),
        formVariante('n-2', 'COLA-2L', { naturaleza: 'Presentacion', esBase: true }),
        formVariante('n-3', 'COLA-V', {
          naturaleza: 'Porcion',
          origenes: [
            { presentacionClave: 'n-1', porcionesPorUnidad: 4 },
            { presentacionClave: 'n-2', porcionesPorUnidad: 8 },
          ],
        }),
      ],
    } as unknown as DatosProductoForm;
    const guardado = {
      variantes: [variante('id-1', 'COLA-1L', { esBase: true }), variante('id-2', 'COLA-2L', { esBase: true }), variante('id-3', 'COLA-V')],
    } as unknown as Producto;

    await sincronizarVariantes(datos, guardado, null);

    expect(catalogoApi.guardarOrigenes).toHaveBeenCalledTimes(1);
    expect(catalogoApi.guardarOrigenes).toHaveBeenCalledWith('id-3', [
      { presentacionVarianteId: 'id-1', porcionesPorUnidad: 4 },
      { presentacionVarianteId: 'id-2', porcionesPorUnidad: 8 },
    ]);
  });

  it('vacía los orígenes de una porción que dejó de serlo y no crea recetas en porciones', async () => {
    const datos = {
      variantes: [
        formVariante('id-1', 'COLA-1L', { naturaleza: 'Presentacion', esBase: true }),
        formVariante('id-3', 'COLA-V', { naturaleza: 'Receta', recetas: [{ varianteInsumoId: 'id-1', cantidad: 0.25 }] }),
      ],
    } as unknown as DatosProductoForm;
    const guardado = { variantes: [variante('id-1', 'COLA-1L', { esBase: true }), variante('id-3', 'COLA-V')] } as unknown as Producto;
    const previo = { variantes: [variante('id-1', 'COLA-1L'), variante('id-3', 'COLA-V', { naturaleza: 'Porcion' })] } as unknown as Producto;

    await sincronizarVariantes(datos, guardado, previo);

    expect(catalogoApi.guardarOrigenes).toHaveBeenCalledWith('id-3', []);
    expect(catalogoApi.agregarReceta).toHaveBeenCalledWith('id-3', { varianteInsumoId: 'id-1', cantidad: 0.25 });
  });
});
