import { useFieldArray } from 'react-hook-form';
import { Plus, Trash2 } from 'lucide-react';
import { Button, Input, Select } from '@licoreria/ui';

interface ErrorOrigen {
  presentacionClave?: { message?: string };
  porcionesPorUnidad?: { message?: string };
}

/**
 * Editor de orígenes de una porción (vaso, trago): qué presentaciones del mismo producto
 * la alimentan y cuántas porciones da cada una. Se guarda con el producto.
 */
export function EditorOrigenes({
  control,
  register,
  indice,
  presentaciones,
  errores,
}: {
  control: any;
  register: (name: any) => Record<string, unknown>;
  indice: number;
  presentaciones: { clave: string; nombre: string }[];
  errores?: ErrorOrigen[];
}) {
  const { fields, append, remove } = useFieldArray({ control, name: `variantes.${indice}.origenes` });

  return (
    <div className="rounded-inner border border-dashed border-border p-2" aria-label="Orígenes de la porción">
      <div className="flex items-center justify-between gap-2">
        <p className="text-xs font-medium text-muted-foreground">Se sirve de estas presentaciones</p>
        <Button
          type="button"
          size="sm"
          variant="ghost"
          leftIcon={<Plus size={13} />}
          disabled={presentaciones.length === 0}
          onClick={() => append({ presentacionClave: '', porcionesPorUnidad: '' })}
        >
          Agregar origen
        </Button>
      </div>
      {presentaciones.length === 0 && (
        <p className="mt-1 text-[11px] text-muted-foreground">
          Primero define al menos una variante como presentación (por ejemplo, la botella).
        </p>
      )}
      <div className="mt-1.5 flex flex-col gap-1.5">
        {fields.map((field, i) => (
          <div key={field.id} className="grid grid-cols-[minmax(0,1fr)_7rem_auto] items-end gap-2">
            <Select
              label="Presentación"
              error={errores?.[i]?.presentacionClave?.message}
              {...register(`variantes.${indice}.origenes.${i}.presentacionClave`)}
            >
              <option value="">Selecciona…</option>
              {presentaciones.map((presentacion) => (
                <option key={presentacion.clave} value={presentacion.clave}>
                  {presentacion.nombre}
                </option>
              ))}
            </Select>
            <Input
              label="Porciones"
              type="number"
              min={1}
              step={1}
              placeholder="Ej. 4"
              error={errores?.[i]?.porcionesPorUnidad?.message}
              {...register(`variantes.${indice}.origenes.${i}.porcionesPorUnidad`)}
            />
            <Button
              type="button"
              variant="ghost"
              size="sm"
              aria-label="Quitar origen"
              leftIcon={<Trash2 size={13} />}
              onClick={() => remove(i)}
            />
          </div>
        ))}
      </div>
      {fields.length > 0 && (
        <p className="mt-1.5 text-[11px] text-muted-foreground">
          Cada presentación destapada da esa cantidad de porciones; el barman elige cuál destapar.
        </p>
      )}
    </div>
  );
}
