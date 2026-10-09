import { useEffect } from 'react';
import { useFieldArray, useForm, useWatch } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { ArrowLeft, Copy, Save, Trash2 } from 'lucide-react';
import { Button, cn, Input, Select } from '@licoreria/ui';
import type { Categoria, Marca, Producto, Receta, UnidadMedida } from '@licoreria/types';
import { formatoRango, metricasDeProducto } from '../../lib/imagenProducto';
import { CodigoBarras } from './CodigoBarras';
import { EditorConsumo } from './EditorConsumo';
import { PanelImagenProducto } from './PanelImagenProducto';

/** Precio opcional: vacío muestra el placeholder; al guardar se convierte en número. */
const numeroPrecio = z.union([z.literal(''), z.coerce.number({ invalid_type_error: 'Precio inválido' }).min(0, 'No puede ser negativo')]);

/** Grado alcohólico opcional: solo aplica a productos de Barra. */
const gradoAlcoholico = z.union([z.literal(''), z.coerce.number({ invalid_type_error: 'Inválido' }).min(0, 'Inválido').max(100, 'Inválido')]);

/** Cantidad de stock opcional: vacío equivale a cero. */
const numeroNoNegativo = z.union([z.literal(''), z.coerce.number({ invalid_type_error: 'Inválido' }).min(0, 'No puede ser negativo')]);

const esquemaReceta = z.object({
  varianteInsumoId: z.string().min(1, 'Elige el insumo'),
  cantidad: numeroNoNegativo,
});

const esquemaVariante = z.object({
  id: z.string().optional(),
  nombre: z.string().min(1, 'Nombre de la variante'),
  sku: z.string().optional(),
  unidadMedidaId: z.string().min(1, 'Selecciona la unidad'),
  precioCompraUSD: numeroPrecio,
  precioVentaUSD: numeroPrecio,
  codigoBarras: z.string().optional(),
  esBase: z.boolean(),
  stockInicial: numeroNoNegativo,
  stockMinimo: numeroNoNegativo,
  stockMaximo: numeroNoNegativo,
  recetas: z.array(esquemaReceta).optional(),
});

const esquemaProducto = z
  .object({
    nombre: z.string().min(2, 'Ingresa el nombre'),
    descripcion: z.string().optional(),
    categoriaId: z.string().min(1, 'Selecciona una categoría'),
    marcaId: z.string(),
    tipo: z.enum(['Simple', 'Preparado']),
    areaDestino: z.enum(['Barra', 'Cocina']),
    gradoAlcoholico,
    imagenUrl: z.string().optional(),
    activo: z.boolean(),
    variantes: z.array(esquemaVariante).min(1, 'Agrega al menos una variante'),
  })
  .superRefine((datos, ctx) => {
    if (datos.areaDestino === 'Barra' && datos.tipo === 'Simple' && !datos.variantes.some((v) => v.esBase)) {
      ctx.addIssue({
        code: 'custom',
        path: ['variantes'],
        message: 'El producto de Barra simple debe tener al menos una presentación base activa para ordenarlo al proveedor.',
      });
    }
    if (datos.tipo === 'Preparado') {
      const sinReceta = datos.variantes.filter(
        (variante) => !(variante.recetas ?? []).some((r) => r.varianteInsumoId !== '' && Number(r.cantidad) > 0),
      );
      if (sinReceta.length > 0) {
        ctx.addIssue({
          code: 'custom',
          path: ['variantes'],
          message:
            'Un preparado debe consumir al menos un insumo por presentación: asigna su receta (insumo y cantidad) para que al venderse descuente del inventario.',
        });
      }
    }
  });

type FormularioProductoForm = z.infer<typeof esquemaProducto>;

export type { FormularioProductoForm as DatosProductoForm };

/** Base del SKU autogenerado: sin tildes, mayúsculas y guiones. */
function slugSku(texto: string): string {
  return texto
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toUpperCase()
    .replace(/[^A-Z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '')
    .slice(0, 12);
}

function generarSku(nombreProducto: string, indice: number): string {
  const base = slugSku(nombreProducto) || 'PRODUCTO';
  return `${base}-${indice + 1}`;
}

/** Rellena los SKU vacíos con uno autogenerado; conserva los existentes. */
function completarSkus(datos: FormularioProductoForm): FormularioProductoForm {
  return {
    ...datos,
    variantes: datos.variantes.map((variante, indice) => ({
      ...variante,
      sku: (variante.sku ?? '').trim() || generarSku(datos.nombre, indice),
    })),
  };
}

const VACIO: FormularioProductoForm = {
  nombre: '',
  descripcion: '',
  categoriaId: '',
  marcaId: '',
  tipo: 'Simple',
  areaDestino: 'Barra',
  gradoAlcoholico: '',
  imagenUrl: '',
  activo: true,
  variantes: [{ nombre: '', sku: '', unidadMedidaId: '', precioCompraUSD: '', precioVentaUSD: '', codigoBarras: '', esBase: true, stockInicial: '', stockMinimo: '', stockMaximo: '', recetas: [] }],
};

function desdeProducto(producto: Producto, recetasPorVariante: Record<string, Receta[]> = {}): FormularioProductoForm {
  return {
    nombre: producto.nombre,
    descripcion: producto.descripcion ?? '',
    categoriaId: producto.categoriaId,
    marcaId: producto.marcaId ?? '',
    tipo: producto.tipo,
    areaDestino: producto.areaDestino,
    gradoAlcoholico: producto.gradoAlcoholico ?? '',
    imagenUrl: producto.imagenUrl ?? '',
    activo: producto.activo,
    variantes: producto.variantes.map((variante) => ({
      id: variante.id,
      nombre: variante.nombre,
      sku: variante.sku,
      unidadMedidaId: variante.unidadMedidaId,
      precioCompraUSD: variante.precioCompraUSD,
      precioVentaUSD: variante.precioVentaUSD,
      codigoBarras: variante.codigosBarras?.[0] ?? '',
      esBase: variante.esBase,
      stockInicial: '',
      stockMinimo: variante.stockMinimo === 0 ? '' : variante.stockMinimo,
      stockMaximo: variante.stockMaximo === 0 ? '' : variante.stockMaximo,
      recetas: (recetasPorVariante[variante.id] ?? []).map((receta) => ({
        varianteInsumoId: receta.varianteInsumoId,
        cantidad: receta.cantidad,
      })),
    })),
  };
}

function Metrica({ etiqueta, valor }: { etiqueta: string; valor: string }) {
  return (
    <div className="rounded-inner border border-border bg-muted/30 px-3 py-2">
      <p className="text-[11px] uppercase tracking-tighter2 text-muted-foreground">{etiqueta}</p>
      <p className="num mt-0.5 text-sm text-foreground">{valor}</p>
    </div>
  );
}

/** Oculta las flechas de los inputs numéricos para que no tapen el contenido. */
const SIN_FLECHAS =
  'appearance-none [&::-webkit-inner-spin-button]:appearance-none [&::-webkit-outer-spin-button]:appearance-none';

/** Al pulsar Enter en un campo, avanza al siguiente campo en lugar de enviar el formulario. */
function avanzarConEnter(evento: React.KeyboardEvent<HTMLFormElement>) {
  if (evento.key !== 'Enter' || evento.shiftKey) return;
  const objetivo = evento.target as HTMLElement;
  if (!['INPUT', 'SELECT', 'TEXTAREA'].includes(objetivo.tagName)) return;
  const campos = Array.from(
    (evento.currentTarget as HTMLFormElement).querySelectorAll('input:not([type="hidden"]), select, textarea'),
  );
  const indice = campos.indexOf(objetivo);
  const siguiente = campos.slice(indice + 1).find((campo) => !(campo as HTMLInputElement).disabled);
  if (siguiente) {
    evento.preventDefault();
    (siguiente as HTMLElement).focus();
  }
}

export function FormularioProducto({
  producto,
  categorias,
  marcas,
  unidades,
  recetasPorVariante,
  guardando,
  onCancelar,
  onGuardar,
}: {
  producto?: Producto | null;
  categorias: Categoria[];
  marcas: Marca[];
  unidades: UnidadMedida[];
  recetasPorVariante?: Record<string, Receta[]>;
  guardando: boolean;
  onCancelar: () => void;
  onGuardar: (datos: FormularioProductoForm) => void;
}) {
  const productoForm = useForm<FormularioProductoForm>({
    resolver: zodResolver(esquemaProducto),
    defaultValues: producto ? desdeProducto(producto, recetasPorVariante ?? {}) : VACIO,
  });
  const { fields, append, remove } = useFieldArray({ control: productoForm.control, name: 'variantes' });
  const imagenUrl = useWatch({ control: productoForm.control, name: 'imagenUrl' });
  const variantes = useWatch({ control: productoForm.control, name: 'variantes' });
  const nombre = useWatch({ control: productoForm.control, name: 'nombre' });
  const areaDestino = useWatch({ control: productoForm.control, name: 'areaDestino' });
  const tipo = useWatch({ control: productoForm.control, name: 'tipo' });
  const metricas = producto ? metricasDeProducto(producto) : null;
  const esNuevo = !producto;

  // useFieldArray agrupa los errores del arreglo en `root`; fuera de él van en `message`.
  const errorVariantes = productoForm.formState.errors.variantes as unknown as
    | { message?: string; root?: { message?: string } }
    | undefined;
  const mensajeVariantes = errorVariantes?.message ?? errorVariantes?.root?.message;

  useEffect(() => {
    if (areaDestino === 'Cocina') {
      // La cocina solo vende: siempre Simple, sin receta ni stock.
      productoForm.setValue('tipo', 'Simple');
      return;
    }
    if (esNuevo) productoForm.setValue('tipo', 'Simple');
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [areaDestino]);

  // Un preparado nunca se ordena al proveedor: todas sus variantes consumen receta.
  useEffect(() => {
    if (tipo !== 'Preparado') return;
    (variantes ?? []).forEach((variante, indice) => {
      if (variante.esBase) productoForm.setValue(`variantes.${indice}.esBase`, false);
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [tipo]);

  const duplicar = (indice: number) => {
    const actual = fields[indice];
    append({
      nombre: actual.nombre,
      sku: actual.sku ? `${actual.sku}-2` : '',
      unidadMedidaId: actual.unidadMedidaId,
      precioCompraUSD: actual.precioCompraUSD,
      precioVentaUSD: actual.precioVentaUSD,
      codigoBarras: '',
      esBase: false,
      stockInicial: '',
      stockMinimo: actual.stockMinimo,
      stockMaximo: actual.stockMaximo,
      recetas: [],
    });
  };

  const marcarBase = (indice: number, base: boolean) => {
    productoForm.setValue(`variantes.${indice}.esBase`, base);
    if (base) {
      (variantes ?? []).forEach((_, i) => {
        if (i !== indice) productoForm.setValue(`variantes.${i}.esBase`, false);
      });
    }
  };

  return (
    <form
      className="flex h-full min-h-0 flex-col gap-4"
      onSubmit={productoForm.handleSubmit((datos) => onGuardar(completarSkus(datos)))}
      onKeyDown={avanzarConEnter}
      noValidate
    >
      <div className="flex flex-wrap items-center gap-2 border-b border-border pb-2">
        <Button variant="ghost" size="sm" type="button" onClick={onCancelar} aria-label="Volver">
          <ArrowLeft size={16} />
        </Button>
        <div>
          <p className="text-base font-medium tracking-tighter2 text-foreground">
            {producto ? `Editar · ${producto.nombre}` : 'Nuevo producto'}
          </p>
          <p className="text-xs text-muted-foreground">
            Define el producto, sus presentaciones, precios y stock inicial.
          </p>
        </div>
        <Button type="submit" className="ml-auto" leftIcon={<Save size={15} />} loading={guardando}>
          {producto ? 'Guardar cambios' : 'Crear producto'}
        </Button>
      </div>

      <div className="grid grid-cols-1 gap-4 lg:h-full lg:items-start lg:grid-cols-[minmax(0,35%)_minmax(0,1fr)]">
        <aside className="min-w-0 lg:self-start">
          <PanelImagenProducto
            imagenUrl={imagenUrl ?? null}
            variantes={(variantes ?? []).map((variante) => ({
              nombre: variante.nombre,
              sku: variante.sku ?? '',
              codigoBarras: variante.codigoBarras,
            }))}
            onImagen={(url) => productoForm.setValue('imagenUrl', url ?? '')}
          />
        </aside>

        <div className="flex min-w-0 flex-col gap-4 lg:overflow-y-auto lg:pr-1">
          {metricas && producto && (
            <section className="grid grid-cols-2 gap-2 sm:grid-cols-3 xl:grid-cols-5">
              <Metrica etiqueta="Variantes" valor={String(metricas.variantes)} />
              <Metrica etiqueta="Precio venta" valor={formatoRango(metricas.precioMinUSD, metricas.precioMaxUSD)} />
              <Metrica etiqueta="Costo" valor={formatoRango(metricas.costoMinUSD, metricas.costoMaxUSD)} />
              <Metrica etiqueta="Margen prom." valor={formatoRango(metricas.margenPromedioUSD, metricas.margenPromedioUSD)} />
              <Metrica
                etiqueta="Estado"
                valor={producto.activo ? 'Activo' : 'Inactivo'}
              />
            </section>
          )}

          <section className="flex flex-col gap-2 p-[5px]">
            <p className="text-xs font-medium uppercase tracking-tighter2 text-muted-foreground">Datos generales</p>
            <div className="grid grid-cols-1 gap-2 sm:grid-cols-2">
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
              {areaDestino === 'Barra' && (
                <Select label="Tipo" {...productoForm.register('tipo')}>
                  <option value="Simple">Simple</option>
                  <option value="Preparado">Preparado</option>
                </Select>
              )}
              <Select label="Área destino" {...productoForm.register('areaDestino')}>
                <option value="Barra">Barra</option>
                <option value="Cocina">Cocina</option>
              </Select>
              {areaDestino === 'Barra' && (
                <Input label="Grado alcohólico" type="number" placeholder="% (opcional)" {...productoForm.register('gradoAlcoholico')} />
              )}
            </div>
            {areaDestino === 'Cocina' && (
              <p className="text-[11px] text-muted-foreground">
                Producto de cocina: Simple, sin receta ni stock; solo se vende.
              </p>
            )}
            <label className={cn('flex items-center gap-2 text-sm text-muted-foreground')}>
              <input type="checkbox" {...productoForm.register('activo')} />
              Activo
            </label>
          </section>

          <section className="flex flex-col gap-2">
            <div className="flex items-center justify-between">
              <p className="text-xs font-medium uppercase tracking-tighter2 text-muted-foreground">Variantes</p>
              <Button
                type="button"
                size="sm"
                variant="ghost"
                onClick={() => append({ nombre: '', sku: '', unidadMedidaId: '', precioCompraUSD: '', precioVentaUSD: '', codigoBarras: '', esBase: false, stockInicial: '', stockMinimo: '', stockMaximo: '', recetas: [] })}
              >
                Agregar variante
              </Button>
            </div>
            {mensajeVariantes && <p className="text-xs text-destructive-fg">{mensajeVariantes}</p>}
            <div className="flex flex-col gap-3">
              {fields.map((field, indice) => (
                <div key={field.id} className="rounded-inner border border-border bg-muted/30 p-2.5">
                  <div className="flex items-center justify-between gap-2">
                    <p className="text-xs font-medium uppercase tracking-tighter2 text-muted-foreground">Variante {indice + 1}</p>
                    <div className="flex items-center gap-1.5">
                      <Button
                        type="button"
                        variant="ghost"
                        size="sm"
                        leftIcon={<Copy size={13} />}
                        title="Duplicar presentación"
                        aria-label={`Duplicar presentación de ${variantes?.[indice]?.nombre || `la variante ${indice + 1}`}`}
                        onClick={() => duplicar(indice)}
                      >
                        Duplicar
                      </Button>
                      <Button type="button" variant="ghost" size="sm" leftIcon={<Trash2 size={13} />} onClick={() => remove(indice)}>
                        Quitar
                      </Button>
                    </div>
                  </div>
                  <div
                    className={cn('mt-2 grid grid-cols-1 gap-2 sm:grid-cols-2', areaDestino === 'Cocina' ? 'xl:grid-cols-5' : 'xl:grid-cols-6')}
                  >
                    <Input label="Nombre" {...productoForm.register(`variantes.${indice}.nombre`)} />
                    <Input
                      label="SKU"
                      placeholder="Opcional"
                      hint={variantes?.[indice]?.sku?.trim() ? undefined : `Se genera: ${generarSku(nombre ?? '', indice)}`}
                      {...productoForm.register(`variantes.${indice}.sku`)}
                    />
                    <Select label="Unidad" {...productoForm.register(`variantes.${indice}.unidadMedidaId`)}>
                      <option value="">Selecciona…</option>
                      {unidades.map((unidad) => (
                        <option key={unidad.id} value={unidad.id}>
                          {unidad.abreviatura}
                        </option>
                      ))}
                    </Select>
                    <Input
                      label="Costo USD"
                      type="number"
                      placeholder="0.00"
                      className={SIN_FLECHAS}
                      {...productoForm.register(`variantes.${indice}.precioCompraUSD`)}
                    />
                    <Input
                      label="Venta USD"
                      type="number"
                      placeholder="0.00"
                      className={SIN_FLECHAS}
                      {...productoForm.register(`variantes.${indice}.precioVentaUSD`)}
                    />
                    {areaDestino === 'Barra' && (
                      <Input
                        label="Código de barras"
                        placeholder="EAN / UPC…"
                        {...productoForm.register(`variantes.${indice}.codigoBarras`)}
                      />
                    )}
                  </div>
                  {areaDestino === 'Barra' && tipo === 'Simple' && (
                    <div className="mt-2 grid grid-cols-1 gap-2 sm:grid-cols-3">
                      {esNuevo && (
                        <Input
                          label="Stock inicial"
                          type="number"
                          placeholder="0"
                          className={SIN_FLECHAS}
                          {...productoForm.register(`variantes.${indice}.stockInicial`)}
                        />
                      )}
                      <Input
                        label="Stock mín."
                        type="number"
                        placeholder="0"
                        className={SIN_FLECHAS}
                        {...productoForm.register(`variantes.${indice}.stockMinimo`)}
                      />
                      <Input
                        label="Stock máx."
                        type="number"
                        placeholder="0"
                        className={SIN_FLECHAS}
                        {...productoForm.register(`variantes.${indice}.stockMaximo`)}
                      />
                    </div>
                  )}
                  {areaDestino === 'Barra' && tipo === 'Simple' && (
                    <label className="mt-2 flex items-center gap-2 text-xs text-muted-foreground">
                      <input
                        type="checkbox"
                        checked={Boolean(variantes?.[indice]?.esBase)}
                        onChange={(evento) => marcarBase(indice, evento.target.checked)}
                      />
                      Presentación base (se ordena a proveedor)
                    </label>
                  )}
                  {areaDestino === 'Barra' && tipo === 'Preparado' && (
                    <p className="mt-2 text-[11px] text-muted-foreground">
                      Preparado: no lleva stock propio ni se ordena al proveedor; consume sus insumos por cada unidad
                      vendida.
                    </p>
                  )}
                  {areaDestino === 'Barra' && (tipo === 'Preparado' || !(variantes?.[indice]?.esBase)) && (
                    <div className="mt-1.5">
                      <EditorConsumo
                        control={productoForm.control}
                        register={productoForm.register}
                        indice={indice}
                        conBasePropia={(variantes ?? []).some((v) => v.esBase)}
                      />
                    </div>
                  )}
                  {areaDestino === 'Barra' && variantes?.[indice]?.codigoBarras && (
                    <div className="mt-2 flex justify-center border-t border-border pt-2">
                      <CodigoBarras valor={variantes[indice].codigoBarras} />
                    </div>
                  )}
                </div>
              ))}
            </div>
          </section>
        </div>
      </div>
    </form>
  );
}
