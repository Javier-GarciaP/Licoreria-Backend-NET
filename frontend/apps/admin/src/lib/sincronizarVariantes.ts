import { catalogoApi } from '@licoreria/api-client';
import type { Producto } from '@licoreria/types';
import { INSUMO_BASE } from '../components/catalogo/EditorConsumo';
import type { DatosProductoForm } from '../components/catalogo/FormularioProducto';

/**
 * Sincroniza recetas y orígenes de porción tras guardar el producto. El orden evita choques
 * entre naturalezas: primero se retiran las recetas, luego se reemplazan los orígenes (las
 * porciones los fijan; las que dejaron de serlo los vacían) y al final se crean las recetas.
 */
export async function sincronizarVariantes(datos: DatosProductoForm, guardado: Producto, previo: Producto | null) {
  const baseId = guardado.variantes.find((variante) => variante.esBase)?.id;
  const guardadas = datos.variantes.map(
    (variante, indice) =>
      guardado.variantes.find((v) => v.sku.trim() === (variante.sku ?? '').trim()) ?? guardado.variantes[indice],
  );
  const idPorClave = new Map<string, string>();
  datos.variantes.forEach((variante, indice) => {
    const guardada = guardadas[indice];
    if (guardada) idPorClave.set(variante.clave ?? variante.id ?? '', guardada.id);
  });

  for (const guardada of guardadas) {
    if (!guardada) continue;
    for (const receta of await catalogoApi.recetas(guardada.id)) await catalogoApi.eliminarReceta(guardada.id, receta.id);
  }

  for (let indice = 0; indice < datos.variantes.length; indice += 1) {
    const variante = datos.variantes[indice];
    const guardada = guardadas[indice];
    if (!guardada) continue;
    if (variante.naturaleza === 'Porcion') {
      const origenes = (variante.origenes ?? [])
        .map((origen) => ({
          presentacionVarianteId: idPorClave.get(origen.presentacionClave) ?? '',
          porcionesPorUnidad: Number(origen.porcionesPorUnidad),
        }))
        .filter((origen) => origen.presentacionVarianteId);
      await catalogoApi.guardarOrigenes(guardada.id, origenes);
    } else if (previo?.variantes.find((v) => v.id === guardada.id)?.naturaleza === 'Porcion') {
      await catalogoApi.guardarOrigenes(guardada.id, []);
    }
  }

  for (let indice = 0; indice < datos.variantes.length; indice += 1) {
    const variante = datos.variantes[indice];
    const guardada = guardadas[indice];
    if (!guardada || variante.naturaleza === 'Porcion') continue;
    for (const consumo of variante.recetas ?? []) {
      const insumoId = consumo.varianteInsumoId === INSUMO_BASE ? baseId : consumo.varianteInsumoId;
      const cantidad = Number(consumo.cantidad);
      if (insumoId && cantidad > 0) {
        await catalogoApi.agregarReceta(guardada.id, { varianteInsumoId: insumoId, cantidad });
      }
    }
  }
}
