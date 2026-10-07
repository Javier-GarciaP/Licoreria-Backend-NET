import { useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'sonner';
import {
  Button,
  Card,
  CardBody,
  CardHeader,
  CardTitle,
  Input,
  Modal,
  PageHeader,
  Pill,
  Select,
  Skeleton,
  StatusBadge,
} from '@licoreria/ui';
import type { Cuenta, MetodoPago } from '@licoreria/types';
import { catalogoApi, cuentasApi, ventasApi } from '@licoreria/api-client';
import { mensajeDeError } from '../lib/api';
import { formatUSD, haceCuanto } from '../lib/format';

interface NuevoItem {
  varianteId: string;
  nombre: string;
  precioUSD: number;
  cantidad: number;
}

interface PagoLinea {
  metodoPagoId: string;
  monto: number;
  moneda: 'USD' | 'BS';
}

const SIGUIENTE_ESTADO: Record<string, string | null> = {
  Recibido: 'Preparado',
  Preparado: 'Entregado',
  Entregado: null,
};

const esquemaAbono = z.object({
  metodoPagoId: z.string().min(1, 'Selecciona el método de pago'),
  monto: z.coerce.number({ invalid_type_error: 'Ingresa un monto' }).positive('Debe ser mayor que 0'),
  moneda: z.enum(['USD', 'BS']),
});

const esquemaDividir = z.object({
  partes: z.coerce.number({ invalid_type_error: 'Ingresa el número de partes' }).int().min(2, 'Mínimo 2 partes'),
});

type FormularioAbono = z.infer<typeof esquemaAbono>;
type FormularioDividir = z.infer<typeof esquemaDividir>;

export function CuentaPage() {
  const { id = '' } = useParams();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const [busqueda, setBusqueda] = useState('');
  const [area, setArea] = useState<'Barra' | 'Cocina'>('Barra');
  const [nuevos, setNuevos] = useState<NuevoItem[]>([]);
  const [pagosCierre, setPagosCierre] = useState<PagoLinea[]>([]);
  const [cierreMetodo, setCierreMetodo] = useState('');
  const [cierreMonto, setCierreMonto] = useState('');
  const [cierreMoneda, setCierreMoneda] = useState<'USD' | 'BS'>('USD');
  const [dividirOpen, setDividirOpen] = useState(false);

  const abonoForm = useForm<FormularioAbono>({
    resolver: zodResolver(esquemaAbono),
    defaultValues: { metodoPagoId: '', monto: 0, moneda: 'USD' },
  });
  const dividirForm = useForm<FormularioDividir>({
    resolver: zodResolver(esquemaDividir),
    defaultValues: { partes: 2 },
  });

  const cuenta = useQuery({ queryKey: ['cuenta', id], queryFn: () => cuentasApi.obtener(id), enabled: Boolean(id) });
  const productos = useQuery({
    queryKey: ['cuenta', 'productos', busqueda],
    queryFn: () => catalogoApi.productos({ busqueda, pageSize: 10, activo: true }),
  });
  const metodos = useQuery({ queryKey: ['metodos-pago'], queryFn: ventasApi.metodosPago });

  const invalidar = () => {
    queryClient.invalidateQueries({ queryKey: ['cuenta', id] });
    queryClient.invalidateQueries({ queryKey: ['cuentas'] });
  };

  const agregarComanda = useMutation({
    mutationFn: () =>
      cuentasApi.agregarComanda(id, {
        area,
        items: nuevos.map((item) => ({ varianteId: item.varianteId, cantidad: item.cantidad })),
      }),
    onSuccess: () => {
      toast.success('Comanda enviada');
      setNuevos([]);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo enviar la comanda', { description: mensajeDeError(error) }),
  });

  const cambiarEstado = useMutation({
    mutationFn: ({ comandaId, detalleId, estado }: { comandaId: string; detalleId: string; estado: string }) =>
      cuentasApi.cambiarEstadoItem(id, comandaId, detalleId, estado),
    onSuccess: invalidar,
    onError: (error) => toast.error('No se pudo actualizar', { description: mensajeDeError(error) }),
  });

  const abonar = useMutation({
    mutationFn: (datos: FormularioAbono) => cuentasApi.abonar(id, datos),
    onSuccess: () => {
      toast.success('Abono registrado');
      abonoForm.reset({ metodoPagoId: '', monto: 0, moneda: 'USD' });
      invalidar();
    },
    onError: (error) => toast.error('No se pudo abonar', { description: mensajeDeError(error) }),
  });

  const dividir = useMutation({
    mutationFn: (datos: FormularioDividir) => cuentasApi.dividir(id, { partes: datos.partes }),
    onSuccess: () => {
      toast.success('Cuenta dividida');
      setDividirOpen(false);
      dividirForm.reset({ partes: 2 });
      invalidar();
    },
    onError: (error) => toast.error('No se pudo dividir la cuenta', { description: mensajeDeError(error) }),
  });

  const cerrar = useMutation({
    mutationFn: () =>
      cuentasApi.cerrar(id, {
        pagos: pagosCierre.map((pago) => ({ metodoPagoId: pago.metodoPagoId, monto: pago.monto, moneda: pago.moneda })),
      }),
    onSuccess: (venta) => {
      toast.success('Cuenta cerrada', { description: `Comprobante ${venta.numeroComprobante ?? venta.id.slice(0, 8)}` });
      queryClient.invalidateQueries({ queryKey: ['cuentas'] });
      queryClient.invalidateQueries({ queryKey: ['mesas'] });
      navigate('/cuentas');
    },
    onError: (error) => toast.error('No se pudo cerrar la cuenta', { description: mensajeDeError(error) }),
  });

  const datos: Cuenta | undefined = cuenta.data;
  const totalCierre = useMemo(
    () => pagosCierre.reduce((acumulado, pago) => acumulado + pago.monto, 0),
    [pagosCierre],
  );

  if (cuenta.isLoading || !datos) {
    return (
      <div className="mx-auto max-w-page">
        <Skeleton className="h-96 w-full" />
      </div>
    );
  }

  const agregarNuevo = (nombre: string, varianteId: string, precioUSD: number) => {
    setNuevos((actuales) => {
      const existente = actuales.find((item) => item.varianteId === varianteId);
      if (existente) {
        return actuales.map((item) => (item.varianteId === varianteId ? { ...item, cantidad: item.cantidad + 1 } : item));
      }
      return [...actuales, { varianteId, nombre, precioUSD, cantidad: 1 }];
    });
  };

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader
        title={`Mesa ${datos.nombreMesa}`}
        subtitle={`Abierta ${haceCuanto(datos.abiertaEn)}`}
        actions={<StatusBadge status={datos.estado} />}
      />

      <div className="grid grid-cols-1 gap-6 xl:grid-cols-[1.4fr_1fr]">
        <Card>
          <CardHeader>
            <CardTitle>Comandas</CardTitle>
            <span className="text-xs text-muted">Recibido → Preparado → Entregado</span>
          </CardHeader>
          <CardBody className="flex flex-col gap-4">
            {datos.comandas.length === 0 && <p className="text-sm text-muted">Sin consumos todavía.</p>}
            {datos.comandas.map((comanda) => (
              <div key={comanda.id} className="rounded-2xl border border-hairline p-3">
                <div className="mb-2 flex items-center justify-between">
                  <Pill tone={comanda.area === 'Barra' ? 'accent' : 'info'}>{comanda.area}</Pill>
                  <span className="text-xs text-muted">{haceCuanto(comanda.fecha)}</span>
                </div>
                <div className="flex flex-col gap-1">
                  {comanda.detalles.map((detalle) => {
                    const siguiente = SIGUIENTE_ESTADO[detalle.estado];
                    return (
                      <div key={detalle.id} className="flex items-center justify-between gap-2 rounded-xl bg-elevated/40 px-3 py-2">
                        <div className="min-w-0">
                          <p className="truncate text-sm text-ink">
                            {detalle.cantidad} × {detalle.nombre}
                            {detalle.esCortesia && <span className="ml-2 text-xs text-warning">cortesía</span>}
                          </p>
                          <p className="text-xs text-muted">{formatUSD(detalle.subtotalUSD)}</p>
                        </div>
                        <div className="flex items-center gap-2">
                          <StatusBadge status={detalle.estado} />
                          {siguiente && (
                            <Button
                              size="sm"
                              variant="ghost"
                              onClick={() =>
                                cambiarEstado.mutate({ comandaId: comanda.id, detalleId: detalle.id, estado: siguiente })
                              }
                            >
                              {siguiente === 'Preparado' ? 'Preparado' : 'Entregado'}
                            </Button>
                          )}
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>
            ))}
          </CardBody>
        </Card>

        <div className="flex flex-col gap-6">
          <Card>
            <CardHeader>
              <CardTitle>Agregar consumo</CardTitle>
            </CardHeader>
            <CardBody className="flex flex-col gap-3">
              <Input placeholder="Buscar producto…" value={busqueda} onChange={(evento) => setBusqueda(evento.target.value)} />
              <div className="flex gap-2">
                {(['Barra', 'Cocina'] as const).map((valor) => (
                  <button
                    key={valor}
                    onClick={() => setArea(valor)}
                    className={`flex-1 rounded-pill border px-3 py-2 text-sm transition ${
                      area === valor ? 'border-accent text-accent-soft' : 'border-hairline text-muted'
                    }`}
                  >
                    {valor}
                  </button>
                ))}
              </div>
              <div className="grid max-h-56 grid-cols-1 gap-2 overflow-y-auto">
                {productos.data?.items.map((producto) =>
                  producto.variantes.map((variante) => (
                    <button
                      key={variante.id}
                      className="flex items-center justify-between rounded-xl border border-hairline px-3 py-2 text-left text-sm transition hover:border-accent/50"
                      onClick={() => agregarNuevo(producto.nombre, variante.id, variante.precioVentaUSD)}
                    >
                      <span className="text-ink">{producto.nombre}</span>
                      <span className="text-xs text-muted">{formatUSD(variante.precioVentaUSD)}</span>
                    </button>
                  )),
                )}
              </div>
              {nuevos.length > 0 && (
                <div className="rounded-2xl bg-elevated/40 p-3">
                  {nuevos.map((item) => (
                    <div key={item.varianteId} className="flex items-center justify-between text-sm">
                      <span className="text-ink">{item.cantidad} × {item.nombre}</span>
                      <span className="text-muted">{formatUSD(item.precioUSD * item.cantidad)}</span>
                    </div>
                  ))}
                </div>
              )}
              <Button disabled={nuevos.length === 0} loading={agregarComanda.isPending} onClick={() => agregarComanda.mutate()}>
                Enviar a {area}
              </Button>
            </CardBody>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Abonar</CardTitle>
              <span className="text-xs text-muted">Saldo {formatUSD(datos.saldo)}</span>
            </CardHeader>
            <CardBody className="flex flex-col gap-3">
              <form
                className="flex flex-col gap-3"
                onSubmit={abonoForm.handleSubmit((datos) => abonar.mutate(datos))}
                noValidate
              >
                <div className="grid grid-cols-2 gap-2">
                  <Select
                    aria-label="Método de pago"
                    error={abonoForm.formState.errors.metodoPagoId?.message}
                    {...abonoForm.register('metodoPagoId')}
                  >
                    <option value="">Método…</option>
                    {metodos.data?.map((metodo: MetodoPago) => (
                      <option key={metodo.id} value={metodo.id}>{metodo.nombre}</option>
                    ))}
                  </Select>
                  <Select aria-label="Moneda" {...abonoForm.register('moneda')}>
                    <option value="USD">USD</option>
                    <option value="BS">Bs</option>
                  </Select>
                </div>
                <div className="flex gap-2">
                  <Input
                    type="number"
                    placeholder="Monto"
                    error={abonoForm.formState.errors.monto?.message}
                    {...abonoForm.register('monto')}
                  />
                  <Button type="submit" variant="ghost" loading={abonar.isPending}>
                    Abonar
                  </Button>
                </div>
              </form>
              {datos.abonos.length > 0 && (
                <div className="flex flex-col gap-1">
                  {datos.abonos.map((abono) => (
                    <div key={abono.id} className="flex justify-between text-xs text-muted">
                      <span>{abono.metodoPago}</span>
                      <span>{abono.moneda === 'USD' ? formatUSD(abono.monto) : `Bs ${abono.monto}`}</span>
                    </div>
                  ))}
                </div>
              )}
            </CardBody>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Dividir cuenta</CardTitle>
              {datos.divisiones.length > 0 && <Pill tone="accent">{datos.divisiones.length} partes</Pill>}
            </CardHeader>
            <CardBody className="flex flex-col gap-3">
              {datos.divisiones.length > 0 ? (
                <div className="flex flex-col gap-1">
                  {datos.divisiones.map((division) => (
                    <div key={division.id} className="flex justify-between text-sm">
                      <span className="text-muted">Parte {division.indice}</span>
                      <span className={division.pagada ? 'text-success' : 'text-ink'}>{formatUSD(division.monto)}</span>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="text-sm text-muted">Divide la cuenta en partes iguales para cobrar por separado.</p>
              )}
              <Button variant="ghost" size="sm" onClick={() => setDividirOpen(true)}>
                Dividir en partes
              </Button>
            </CardBody>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Cerrar cuenta</CardTitle>
              <span className="text-xs text-muted">Total {formatUSD(datos.total)}</span>
            </CardHeader>
            <CardBody className="flex flex-col gap-3">
              <div className="grid grid-cols-2 gap-2">
                <select
                  className="h-10 rounded-pill border border-hairline bg-surface px-3 text-sm text-ink"
                  value={cierreMetodo}
                  onChange={(evento) => setCierreMetodo(evento.target.value)}
                >
                  <option value="">Método…</option>
                  {metodos.data?.map((metodo: MetodoPago) => (
                    <option key={metodo.id} value={metodo.id}>{metodo.nombre}</option>
                  ))}
                </select>
                <select
                  className="h-10 rounded-pill border border-hairline bg-surface px-3 text-sm text-ink"
                  value={cierreMoneda}
                  onChange={(evento) => setCierreMoneda(evento.target.value as 'USD' | 'BS')}
                >
                  <option value="USD">USD</option>
                  <option value="BS">Bs</option>
                </select>
              </div>
              <div className="flex gap-2">
                <Input type="number" placeholder="Monto" value={cierreMonto} onChange={(evento) => setCierreMonto(evento.target.value)} />
                <Button
                  variant="ghost"
                  disabled={!cierreMetodo || Number(cierreMonto) <= 0}
                  onClick={() => {
                    setPagosCierre((actuales) => [...actuales, { metodoPagoId: cierreMetodo, monto: Number(cierreMonto), moneda: cierreMoneda }]);
                    setCierreMonto('');
                  }}
                >
                  Añadir
                </Button>
              </div>
              {pagosCierre.length > 0 && (
                <div className="flex flex-col gap-1">
                  {pagosCierre.map((pago, indice) => (
                    <div key={indice} className="flex justify-between text-xs text-muted">
                      <span>{metodos.data?.find((metodo) => metodo.id === pago.metodoPagoId)?.nombre}</span>
                      <span>{pago.moneda === 'USD' ? formatUSD(pago.monto) : `Bs ${pago.monto}`}</span>
                    </div>
                  ))}
                  <span className="text-right text-xs text-ink">Registrado: {formatUSD(totalCierre)}</span>
                </div>
              )}
              <Button
                disabled={pagosCierre.length === 0}
                loading={cerrar.isPending}
                onClick={() => cerrar.mutate()}
              >
                Cobrar y cerrar
              </Button>
            </CardBody>
          </Card>
        </div>
      </div>

      <Modal
        open={dividirOpen}
        onClose={() => setDividirOpen(false)}
        title="Dividir cuenta"
        footer={
          <>
            <Button variant="ghost" onClick={() => setDividirOpen(false)}>
              Cancelar
            </Button>
            <Button type="submit" form="form-dividir" loading={dividir.isPending}>
              Dividir
            </Button>
          </>
        }
      >
        <form id="form-dividir" onSubmit={dividirForm.handleSubmit((datos) => dividir.mutate(datos))} noValidate>
          <Input
            label="Número de partes"
            type="number"
            placeholder="2"
            error={dividirForm.formState.errors.partes?.message}
            {...dividirForm.register('partes')}
          />
        </form>
      </Modal>
    </div>
  );
}
