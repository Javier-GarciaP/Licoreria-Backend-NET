import { useFieldArray, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Button, Input, Select } from '@licoreria/ui';
import type { Categoria, Marca, Producto, UnidadMedida } from '@licoreria/types';

const esquemaVariante = z.object({
  id: z.string().optional(),
  nombre: z.string().min(1, 'Nombre de la variante'),
  sku: z.string().min(1, 'SKU requerido'),
  unidadMedidaId: z.string().min(1, 'Selecciona la unidad'),
  precioCompraUSD: z.coerce.number({ invalid_type_error: 'Precio inválido' }).min(0, 'No puede ser negativo'),
  precioVentaUSD: z.coerce.number({ invalid_type_error: 'Precio inválido' }).min(0, 'No puede ser negativo'),
});

const esquemaProducto = z.object({
  nombre: z.string().min(2, 'Ingresa el nombre'),
  descripcion: z.string().optional(),
  categoriaId: z.string().min(1, 'Selecciona una categoría'),
  marcaId: z.string(),
  tipo: z.enum(['Simple', 'Preparado']),
  gradoAlcoholico: z.coerce.number().min(0, 'Inválido').max(100, 'Inválido'),
  imagenUrl: z.string().optional(),
  activo: z.boolean(),
  variantes: z.array(esquemaVariante).min(1, 'Agrega al menos una variante'),
});

type FormularioProductoForm = z.infer<typeof esquemaProducto>;

export type { FormularioProductoForm as DatosProductoForm };

const VACIO: FormularioProductoForm = {
  nombre: '',
  descripcion: '',
  categoriaId: '',
  marcaId: '',
  tipo: 'Simple',
  gradoAlcoholico: 0,
  imagenUrl: '',
  activo: true,
  variantes: [{ nombre: '', sku: '', unidadMedidaId: '', precioCompraUSD: 0, precioVentaUSD: 0 }],
};

function desdeProducto(producto: Producto): FormularioProductoForm {
  return {
    nombre: producto.nombre,
    descripcion: producto.descripcion ?? '',
    categoriaId: producto.categoriaId,
    marcaId: producto.marcaId ?? '',
    tipo: producto.tipo,
    gradoAlcoholico: producto.gradoAlcoholico ?? 0,
    imagenUrl: producto.imagenUrl ?? '',
    activo: producto.activo,
    variantes: producto.variantes.map((variante) => ({
      id: variante.id,
      nombre: variante.nombre,
      sku: variante.sku,
      unidadMedidaId: variante.unidadMedidaId,
      precioCompraUSD: variante.precioCompraUSD,
      precioVentaUSD: variante.precioVentaUSD,
    })),
  };
}

export function FormularioProducto({
  producto,
  categorias,
  marcas,
  unidades,
  guardando,
  onCancelar,
  onGuardar,
}: {
  producto?: Producto | null;
  categorias: Categoria[];
  marcas: Marca[];
  unidades: UnidadMedida[];
  guardando: boolean;
  onCancelar: () => void;
  onGuardar: (datos: FormularioProductoForm) => void;
}) {
  const productoForm = useForm<FormularioProductoForm>({
    resolver: zodResolver(esquemaProducto),
    defaultValues: producto ? desdeProducto(producto) : VACIO,
  });
  const { fields, append, remove } = useFieldArray({ control: productoForm.control, name: 'variantes' });

  return (
    <form className="flex flex-col gap-5" onSubmit={productoForm.handleSubmit(onGuardar)} noValidate>
      <section className="flex flex-col gap-2">
        <p className="text-xs font-medium uppercase tracking-tighter2 text-muted">Datos generales</p>
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <Input label="Nombre" error={productoForm.formState.errors.nombre?.message} {...productoForm.register('nombre')} />
          <Input label="Descripción" {...productoForm.register('descripcion')} />
          <Select label="Categoría" error={productoForm.formState.errors.categoriaId?.message} {...productoForm.register('categoriaId')}>
            <option value="">Selecciona…</option>
            {categorias.map((categoria) => (
              <option key={categoria.id} value={categoria.id}>
                {categoria.nombre}
              </option>
            ))}
          </Select>
          <Select label="Marca" {...productoForm.register('marcaId')}>
            <option value="">Sin marca</option>
            {marcas.map((marca) => (
              <option key={marca.id} value={marca.id}>
                {marca.nombre}
              </option>
            ))}
          </Select>
          <Select label="Tipo" {...productoForm.register('tipo')}>
            <option value="Simple">Simple</option>
            <option value="Preparado">Preparado</option>
          </Select>
          <Input label="Grado alcohólico" type="number" {...productoForm.register('gradoAlcoholico')} />
          <Input label="Imagen (URL)" className="sm:col-span-2" {...productoForm.register('imagenUrl')} />
        </div>
        <label className="flex items-center gap-2 text-sm text-muted">
          <input type="checkbox" {...productoForm.register('activo')} />
          Activo
        </label>
      </section>

      <section className="flex flex-col gap-2">
        <div className="flex items-center justify-between">
          <p className="text-xs font-medium uppercase tracking-tighter2 text-muted">Variantes</p>
          <Button
            type="button"
            size="sm"
            variant="ghost"
            onClick={() => append({ nombre: '', sku: '', unidadMedidaId: '', precioCompraUSD: 0, precioVentaUSD: 0 })}
          >
            Agregar variante
          </Button>
        </div>
        {productoForm.formState.errors.variantes?.message && (
          <p className="text-xs text-danger-ink">{productoForm.formState.errors.variantes.message}</p>
        )}
        <div className="flex flex-col gap-2">
          {fields.map((field, indice) => (
            <div key={field.id} className="grid grid-cols-2 gap-2 rounded-inner border border-hairline bg-elevated/30 p-3 sm:grid-cols-6">
              <Input placeholder="Nombre" {...productoForm.register(`variantes.${indice}.nombre`)} />
              <Input placeholder="SKU" {...productoForm.register(`variantes.${indice}.sku`)} />
              <Select aria-label="Unidad" {...productoForm.register(`variantes.${indice}.unidadMedidaId`)}>
                <option value="">Unidad…</option>
                {unidades.map((unidad) => (
                  <option key={unidad.id} value={unidad.id}>
                    {unidad.abreviatura}
                  </option>
                ))}
              </Select>
              <Input type="number" placeholder="Costo USD" {...productoForm.register(`variantes.${indice}.precioCompraUSD`)} />
              <Input type="number" placeholder="Venta USD" {...productoForm.register(`variantes.${indice}.precioVentaUSD`)} />
              <Button type="button" variant="ghost" size="sm" onClick={() => remove(indice)}>
                Quitar
              </Button>
            </div>
          ))}
        </div>
      </section>

      <div className="flex justify-end gap-2 border-t border-hairline pt-4">
        <Button variant="ghost" onClick={onCancelar}>
          Cancelar
        </Button>
        <Button type="submit" loading={guardando}>
          {producto ? 'Guardar cambios' : 'Crear producto'}
        </Button>
      </div>
    </form>
  );
}