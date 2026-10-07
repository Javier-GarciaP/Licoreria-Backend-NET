import { useMemo, useState } from 'react';
import { useFieldArray, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Button, Card, CardBody, CardHeader, CardTitle, Input, PageHeader, Pill, Select } from '@licoreria/ui';
import type { MetodoPago, ProductoVariante, Promocion, TasaCambio } from '@licoreria/types';
import { catalogoApi, finanzasApi, promocionesApi, ventasApi } from '@licoreria/api-client';
import { mensajeDeError } from '../lib/api';
import { formatBS, formatUSD } from '../lib/format';

interface Linea {
  varianteId: string;
  nombre: string;
  sku: string;
  precioUSD: number;
  cantidad: number;
}

const esquemaPago = z.object({
  metodoPagoId: z.string().min(1, 'Selecciona el método de pago'),
  monto: z.coerce.number({ invalid_type_error: 'Ingresa un monto' }).positive('El monto debe ser mayor que 0'),
  moneda: z.enum(['USD', 'BS']),
});

const esquemaVenta = z.object({
  descuentoUSD: z.coerce.number().min(0, 'No puede ser negativo'),
  propinaUSD: z.coerce.number().min(0, 'No puede ser negativo'),
  promocionId: z.string(),
  pagos: z.array(esquemaPago),
});

type FormularioPago = z.infer<typeof esquemaPago>;
type FormularioVenta = z.infer<typeof esquemaVenta>;

export function PosPage() {
  const [busqueda, setBusqueda] = useState('');
  const [lineas, setLineas] = useState<Linea[]>([]);

  const ventaForm = useForm<FormularioVenta>({
    resolver: zodResolver(esquemaVenta),
    defaultValues: { descuentoUSD: 0, propinaUSD: 0, promocionId: '', pagos: [] },
  });
  const pagoForm = useForm<FormularioPago>({
    resolver: zodResolver(esquemaPago),
    defaultValues: { metodoPagoId: '', monto: 0, moneda: 'USD' },
  });
  const { fields, append, remove } = useFieldArray({ control: ventaForm.control, name: 'pagos' });

  const productos = useQuery({
    queryKey: ['pos', 'productos', busqueda],
    queryFn: () => catalogoApi.productos({ busqueda, pageSize: 12, activo: true }),
  });
  const metodos = useQuery({ queryKey: ['pos', 'metodos-pago'], queryFn: ventasApi.metodosPago });
  const promociones = useQuery({ queryKey: ['pos', 'promociones'], queryFn: promocionesApi.listar });
  const tasa = useQuery({ queryKey: ['pos', 'tasa'], queryFn: () => finanzasApi.tasaActual('Paralelo') });

  const valorTasa = (tasa.data as TasaCambio | undefined)?.valor ?? 0;
  const pagosRaw = ventaForm.watch('pagos');
  const pagos = useMemo(() => pagosRaw ?? [], [pagosRaw]);
  const descuento = Number(ventaForm.watch('descuentoUSD')) || 0;

  const totalUSD = useMemo(
    () => lineas.reduce((acumulado, linea) => acumulado + linea.precioUSD * linea.cantidad, 0),
    [lineas],
  );
  const totalCobrar = Math.max(0, totalUSD - descuento);
  const pagadoUSD = useMemo(
    () =>
      pagos.reduce(
        (acumulado, pago) =>
          acumulado + (pago.moneda === 'USD' ? Number(pago.monto) : valorTasa > 0 ? Number(pago.monto) / valorTasa : 0),
        0,
      ),
    [pagos, valorTasa],
  );

  const registrar = useMutation({
    mutationFn: (datos: FormularioVenta) =>
      ventasApi.registrar({
        items: lineas.map((linea) => ({ varianteId: linea.varianteId, cantidad: linea.cantidad })),
        descuentoUSD: datos.descuentoUSD,
        promocionId: datos.promocionId || null,
        pagos: datos.pagos.map((pago, indice) => ({
          metodoPagoId: pago.metodoPagoId,
          monto: pago.monto,
          moneda: pago.moneda,
          propina: indice === 0 ? datos.propinaUSD : 0,
        })),
      }),
    onSuccess: (venta) => {
      toast.success('Venta registrada', { description: `Comprobante ${venta.numeroComprobante ?? venta.id.slice(0, 8)}` });
      setLineas([]);
      ventaForm.reset({ descuentoUSD: 0, propinaUSD: 0, promocionId: '', pagos: [] });
    },
    onError: (error) => toast.error('No se pudo registrar la venta', { description: mensajeDeError(error) }),
  });

  const enviar = ventaForm.handleSubmit((datos) => {
    if (lineas.length === 0) {
      toast.error('Agrega al menos un producto');
      return;
    }
    if (datos.pagos.length === 0) {
      toast.error('Agrega al menos un pago');
      return;
    }
    registrar.mutate(datos);
  });

  const agregarPago = pagoForm.handleSubmit((datos) => {
    append(datos);
    pagoForm.reset({ metodoPagoId: '', monto: 0, moneda: 'USD' });
  });

  const agregarVariante = (nombre: string, variante: ProductoVariante) => {
    setLineas((actuales) => {
      const existente = actuales.find((linea) => linea.varianteId === variante.id);
      if (existente) {
        return actuales.map((linea) =>
          linea.varianteId === variante.id ? { ...linea, cantidad: linea.cantidad + 1 } : linea,
        );
      }
      return [
        ...actuales,
        { varianteId: variante.id, nombre, sku: variante.sku, precioUSD: variante.precioVentaUSD, cantidad: 1 },
      ];
    });
  };

  const cambiarCantidad = (varianteId: string, delta: number) =>
    setLineas((actuales) =>
      actuales
        .map((linea) => (linea.varianteId === varianteId ? { ...linea, cantidad: linea.cantidad + delta } : linea))
        .filter((linea) => linea.cantidad > 0),
    );

  const puedeCobrar = lineas.length > 0 && pagos.length > 0 && pagadoUSD >= totalCobrar - 0.01;

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader title="POS · Modo Licorería" subtitle="Venta rápida de mostrador con pago mixto (USD / Bs)." />

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-[1.4fr_1fr]">
        <Card>
          <CardHeader>
            <CardTitle>Productos</CardTitle>
            <Pill tone="accent">Tasa {valorTasa > 0 ? formatBS(valorTasa) : '—'}</Pill>
          </CardHeader>
          <CardBody className="flex flex-col gap-4">
            <Input placeholder="Buscar por nombre o SKU…" value={busqueda} onChange={(evento) => setBusqueda(evento.target.value)} />
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
              {productos.data?.items.map((producto) =>
                producto.variantes.map((variante) => (
                  <button
                    key={variante.id}
                    onClick={() => agregarVariante(producto.nombre, variante)}
                    className="flex flex-col items-start gap-1 rounded-2xl border border-hairline bg-elevated/40 p-4 text-left transition hover:border-accent/50"
                  >
                    <span className="text-sm font-medium text-ink">{producto.nombre}</span>
                    <span className="text-xs text-muted">{variante.sku}</span>
                    <span className="mt-1 text-sm text-accent-soft">{formatUSD(variante.precioVentaUSD)}</span>
                  </button>
                )),
              )}
              {productos.data?.items.length === 0 && <p className="text-sm text-muted">Sin resultados.</p>}
            </div>
          </CardBody>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Cuenta</CardTitle>
            <span className="text-sm font-semibold text-ink">{formatUSD(totalCobrar)}</span>
          </CardHeader>
          <CardBody>
            <form className="flex flex-col gap-4" onSubmit={enviar} noValidate>
              <div className="flex flex-col gap-2">
                {lineas.length === 0 && <p className="text-sm text-muted">Agrega productos para cobrar.</p>}
                {lineas.map((linea) => (
                  <div key={linea.varianteId} className="flex items-center justify-between gap-2 rounded-2xl bg-elevated/40 px-3 py-2">
                    <div className="min-w-0">
                      <p className="truncate text-sm text-ink">{linea.nombre}</p>
                      <p className="text-xs text-muted">{formatUSD(linea.precioUSD)}</p>
                    </div>
                    <div className="flex items-center gap-2">
                      <button
                        type="button"
                        aria-label={`Quitar ${linea.nombre}`}
                        className="h-7 w-7 rounded-full border border-hairline text-ink"
                        onClick={() => cambiarCantidad(linea.varianteId, -1)}
                      >
                        −
                      </button>
                      <span className="w-6 text-center text-sm text-ink">{linea.cantidad}</span>
                      <button
                        type="button"
                        aria-label={`Agregar ${linea.nombre}`}
                        className="h-7 w-7 rounded-full border border-hairline text-ink"
                        onClick={() => cambiarCantidad(linea.varianteId, 1)}
                      >
                        +
                      </button>
                    </div>
                  </div>
                ))}
              </div>

              <div className="rounded-2xl border border-hairline p-3">
                <p className="mb-2 text-xs font-semibold uppercase tracking-tighter2 text-muted">Pagos mixtos</p>
                <div className="mb-2 grid grid-cols-3 gap-2">
                  <Input
                    type="number"
                    placeholder="Descuento USD"
                    error={ventaForm.formState.errors.descuentoUSD?.message}
                    {...ventaForm.register('descuentoUSD')}
                  />
                  <Input
                    type="number"
                    placeholder="Propina USD"
                    error={ventaForm.formState.errors.propinaUSD?.message}
                    {...ventaForm.register('propinaUSD')}
                  />
                  <Select aria-label="Promoción" {...ventaForm.register('promocionId')}>
                    <option value="">Sin promoción</option>
                    {promociones.data?.map((promocion: Promocion) => (
                      <option key={promocion.id} value={promocion.id}>
                        {promocion.nombre}
                      </option>
                    ))}
                  </Select>
                </div>

                <div className="flex flex-col gap-1">
                  {fields.map((field, indice) => {
                    const metodo = metodos.data?.find((m) => m.id === field.metodoPagoId);
                    return (
                      <div key={field.id} className="flex items-center justify-between text-sm">
                        <span className="text-muted">
                          {metodo?.nombre ?? 'Pago'} ·{' '}
                          {field.moneda === 'USD' ? formatUSD(Number(field.monto)) : formatBS(Number(field.monto))}
                        </span>
                        <button
                          type="button"
                          className="text-danger"
                          aria-label="Quitar pago"
                          onClick={() => remove(indice)}
                        >
                          &#x2715;
                        </button>
                      </div>
                    );
                  })}
                  {pagos.length === 0 && <p className="text-xs text-muted">Sin pagos agregados.</p>}
                </div>
              </div>

              <div className="flex items-center justify-between text-xs text-muted">
                <span>Cubierto</span>
                <span>
                  {formatUSD(pagadoUSD)} / {formatUSD(totalCobrar)}
                </span>
              </div>

              <Button type="submit" disabled={!puedeCobrar || registrar.isPending} loading={registrar.isPending}>
                Cobrar {formatUSD(totalCobrar)}
              </Button>
            </form>

            <div className="mt-4 border-t border-hairline pt-4">
              <form className="flex items-end gap-2" onSubmit={agregarPago} noValidate>
                <Select
                  aria-label="Método de pago"
                  className="flex-1"
                  error={pagoForm.formState.errors.metodoPagoId?.message}
                  {...pagoForm.register('metodoPagoId')}
                >
                  <option value="">Método…</option>
                  {metodos.data?.map((metodo: MetodoPago) => (
                    <option key={metodo.id} value={metodo.id}>
                      {metodo.nombre}
                    </option>
                  ))}
                </Select>
                <Select aria-label="Moneda" {...pagoForm.register('moneda')}>
                  <option value="USD">USD</option>
                  <option value="BS">Bs</option>
                </Select>
                <Input
                  type="number"
                  placeholder="Monto"
                  className="w-28"
                  error={pagoForm.formState.errors.monto?.message}
                  {...pagoForm.register('monto')}
                />
                <Button type="submit" variant="ghost">
                  Agregar
                </Button>
              </form>
            </div>
          </CardBody>
        </Card>
      </div>
    </div>
  );
}
