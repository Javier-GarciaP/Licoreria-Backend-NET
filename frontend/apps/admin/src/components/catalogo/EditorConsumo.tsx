import { useMemo } from 'react';
import { useFieldArray, useWatch } from 'react-hook-form';
import { useQuery } from '@tanstack/react-query';
import { Plus, Trash2 } from 'lucide-react';
import { Button, Input, Select } from '@licoreria/ui';
import { catalogoApi, inventarioApi } from '@licoreria/api-client';

/** Valor centinela: consumo que apunta a la base del propio producto. */
export const INSUMO_BASE = '__BASE__';

interface Insumo {
  varianteId: string;
  productoNombre: string;
  varianteNombre: string;
  sku: string;
}

/**
 * Editor de consumo (receta) de una variante derivada, integrado en el formulario:
 * se guarda junto al producto al pulsar Guardar.
 */
export function EditorConsumo({
  control,
  register,
  indice,
  conBasePropia,
}: {
  control: any;
  register: (name: any) => Record<string, unknown>;
  indice: number;
  conBasePropia: boolean;
}) {
  const { fields, append, remove } = useFieldArray({ control, name: `variantes.${indice}.recetas` });
  const varianteActualId = useWatch({ control, name: `variantes.${indice}.id` }) as string | undefined;

  const stock = useQuery({
    queryKey: ['stock', 'insumos'],
    queryFn: () => inventarioApi.stock({ page: 1, pageSize: 1000 }),
  });
  const productos = useQuery({
    queryKey: ['catalogo', 'insumos'],
    queryFn: () => catalogoApi.productos({ activo: true, pageSize: 100 }),
  });

  const insumos = useMemo(() => {
    const porId = new Map<string, Insumo>();

    // Las presentaciones base de Barra son los insumos reales:
    // se listan aunque todavía no tengan fila de stock.
    for (const producto of productos.data?.items ?? []) {
      if (producto.areaDestino !== 'Barra' || producto.tipo !== 'Simple') continue;
      for (const variante of producto.variantes) {
        if (!variante.activo || !variante.esBase) continue;
        porId.set(variante.id, {
          varianteId: variante.id,
          productoNombre: producto.nombre,
          varianteNombre: variante.nombre,
          sku: variante.sku,
        });
      }
    }

    // Completa con cualquier variante que ya lleve control de stock.
    for (const item of stock.data?.items ?? []) {
      if (!porId.has(item.varianteId)) {
        porId.set(item.varianteId, {
          varianteId: item.varianteId,
          productoNombre: item.productoNombre,
          varianteNombre: item.varianteNombre,
          sku: item.sku,
        });
      }
    }

    // Una variante nunca puede ser insumo de sí misma.
    if (varianteActualId) porId.delete(varianteActualId);

    return [...porId.values()].sort((a, b) =>
      `${a.productoNombre}${a.varianteNombre}`.localeCompare(`${b.productoNombre}${b.varianteNombre}`),
    );
  }, [productos.data, stock.data, varianteActualId]);

  return (
    <div className="rounded-inner border border-border bg-muted/30 p-2.5">
      <p className="text-[11px] uppercase tracking-tighter2 text-muted-foreground">Consumo por unidad vendida</p>
      {fields.length === 0 ? (
        <p className="mt-1 text-xs text-muted-foreground">Sin consumo asignado (se vende tal cual).</p>
      ) : (
        <div className="mt-1 flex flex-col gap-1.5">
          {fields.map((field, i) => (
            <div key={field.id} className="grid grid-cols-1 gap-1.5 sm:grid-cols-[1fr_6rem_auto]">
              <Select aria-label="Insumo" {...register(`variantes.${indice}.recetas.${i}.varianteInsumoId`)}>
                <option value="">Insumo…</option>
                {conBasePropia && <option value={INSUMO_BASE}>La base de este producto</option>}
                {insumos.map((item) => (
                  <option key={item.varianteId} value={item.varianteId}>
                    {item.productoNombre} · {item.varianteNombre} · {item.sku}
                  </option>
                ))}
              </Select>
              <Input
                type="number"
                min={0}
                step="0.001"
                placeholder="Cant."
                aria-label="Cantidad por unidad"
                {...register(`variantes.${indice}.recetas.${i}.cantidad`)}
              />
              <Button type="button" variant="ghost" size="sm" leftIcon={<Trash2 size={12} />} onClick={() => remove(i)}>
                Quitar
              </Button>
            </div>
          ))}
        </div>
      )}
      <Button
        type="button"
        size="sm"
        variant="ghost"
        className="mt-1"
        leftIcon={<Plus size={13} />}
        onClick={() => append({ varianteInsumoId: '', cantidad: '' })}
      >
        Agregar insumo
      </Button>
    </div>
  );
}
