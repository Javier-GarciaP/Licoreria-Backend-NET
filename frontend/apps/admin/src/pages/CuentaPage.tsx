import { useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'sonner';
import { HandCoins, ArrowLeft, Plus, Receipt, Scissors, UtensilsCrossed, Wallet, X } from 'lucide-react';
import {
  Button,
  Card,
  CardBody,
  CardHeader,
  CardTitle,
  cn,
  Input,
  Modal,
  Pill,
  Select,
  Skeleton,
  StatusBadge,
} from '@licoreria/ui';
import type { Cuenta, MetodoPago, Venta } from '@licoreria/types';
import { cuentasApi, ventasApi } from '@licoreria/api-client';
import { AgregarComandaModal } from '../components/cuentas/AgregarComandaModal';
import { TicketVenta } from '../components/pos/TicketVenta';
import { mensajeDeError } from '../lib/api';
import { formatUSD, haceCuanto } from '../lib/format';

interface PagoLinea {
  metodoPagoId: string;
  monto: number;
}

function KpiTile({
  label,
  value,
  icon,
  chip,
}: {
  label: string;
  value: React.ReactNode;
  icon: React.ReactNode;
  chip: string;
}) {
  return (
    <div className="flex items-center gap-3 rounded-xl border border-border bg-card px-4 py-3">
      <span className={cn('flex h-9 w-9 shrink-0 items-center justify-center rounded-full', chip)}>{icon}</span>
      <div className="min-w-0">
        <p className="num truncate text-lg font-medium leading-tight text-foreground">{value}</p>
        <p className="truncate text-[11px] text-muted-foreground">{label}</p>
      </div>
    </div>
  );
}

const esquemaAbono = z.object({
  metodoPagoId: z.string().min(1, 'Selecciona el método de pago'),
  monto: z.coerce.number({ invalid_type_error: 'Ingresa un monto' }).positive('Debe ser mayor que 0'),
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

  const [consumoOpen, setConsumoOpen] = useState(false);
  const [abonoOpen, setAbonoOpen] = useState(false);
  const [dividirOpen, setDividirOpen] = useState(false);
  const [cobroOpen, setCobroOpen] = useState(false);
  const [ventaTicket, setVentaTicket] = useState<Venta | null>(null);

  const [pagosCierre, setPagosCierre] = useState<PagoLinea[]>([]);
  const [cierreMetodo, setCierreMetodo] = useState('');
  const [cierreMonto, setCierreMonto] = useState('');

  const abonoForm = useForm<FormularioAbono>({
    resolver: zodResolver(esquemaAbono),
    defaultValues: { metodoPagoId: '', monto: 0 },
  });
  const dividirForm = useForm<FormularioDividir>({
    resolver: zodResolver(esquemaDividir),
    defaultValues: { partes: 2 },
  });

  const cuenta = useQuery({ queryKey: ['cuenta', id], queryFn: () => cuentasApi.obtener(id), enabled: Boolean(id) });
  const metodos = useQuery({ queryKey: ['metodos-pago'], queryFn: ventasApi.metodosPago });

  const invalidar = () => {
    queryClient.invalidateQueries({ queryKey: ['cuenta', id] });
    queryClient.invalidateQueries({ queryKey: ['cuentas'] });
  };

  const abonar = useMutation({
    mutationFn: (datos: FormularioAbono) => cuentasApi.abonar(id, { ...datos, moneda: 'USD' }),
    onSuccess: () => {
      toast.success('Abono registrado');
      abonoForm.reset({ metodoPagoId: '', monto: 0 });
      invalidar();
    },
    onError: (error) => toast.error('No se pudo abonar', { description: mensajeDeError(error) }),
  });

  const dividir = useMutation({
    mutationFn: (datos: FormularioDividir) => cuentasApi.dividir(id, { partes: datos.partes }),
    onSuccess: () => {
      toast.success('Cuenta dividida');
      dividirForm.reset({ partes: 2 });
      invalidar();
    },
    onError: (error) => toast.error('No se pudo dividir la cuenta', { description: mensajeDeError(error) }),
  });

  const cerrar = useMutation({
    mutationFn: () =>
      cuentasApi.cerrar(id, {
        pagos: pagosCierre.map((pago) => ({ metodoPagoId: pago.metodoPagoId, monto: pago.monto, moneda: 'USD' })),
      }),
    onSuccess: (venta) => {
      toast.success('Cuenta cerrada', { description: `Comprobante ${venta.numeroComprobante ?? venta.id.slice(0, 8)}` });
      setPagosCierre([]);
      setCobroOpen(false);
      queryClient.invalidateQueries({ queryKey: ['cuentas'] });
      queryClient.invalidateQueries({ queryKey: ['mesas'] });
      setVentaTicket(venta);
    },
    onError: (error) => toast.error('No se pudo cerrar la cuenta', { description: mensajeDeError(error) }),
  });

  const datos: Cuenta | undefined = cuenta.data;

  const totalCierre = useMemo(() => pagosCierre.reduce((acumulado, pago) => acumulado + pago.monto, 0), [pagosCierre]);
  const restante = Math.max(0, (datos?.saldo ?? 0) - totalCierre);
  const cubierto = restante <= 0.01;

  const abrirCobro = () => {
    setPagosCierre([]);
    setCierreMetodo(metodos.data?.[0]?.id ?? '');
    setCierreMonto(datos ? String(datos.saldo) : '');
    setCobroOpen(true);
  };

  const agregarPago = () => {
    const monto = Number(cierreMonto);
    if (!cierreMetodo || monto <= 0) return;
    setPagosCierre((actuales) => [...actuales, { metodoPagoId: cierreMetodo, monto }]);
    setCierreMonto(String(Math.max(0, restante - monto)));
  };

  if (cuenta.isLoading || !datos) {
    return (
      <div className="mx-auto max-w-page">
        <Skeleton className="h-96 w-full" />
      </div>
    );
  }

  const consumos = datos.comandas.reduce(
    (acumulado, comanda) => acumulado + comanda.detalles.reduce((subtotal, detalle) => subtotal + detalle.cantidad, 0),
    0,
  );

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <div className="flex flex-wrap items-center gap-2 border-b border-border pb-2">
        <Button variant="ghost" size="sm" type="button" onClick={() => navigate('/cuentas')} aria-label="Volver">
          <ArrowLeft size={16} />
        </Button>
        <div>
          <p className="text-base font-medium tracking-tighter2 text-foreground">Mesa {datos.nombreMesa}</p>
          <p className="text-xs text-muted-foreground">
            Abierta {haceCuanto(datos.abiertaEn)}{datos.cliente ? ` · ${datos.cliente}` : ''}
          </p>
        </div>
        <div className="ml-auto flex items-center gap-2">
          <StatusBadge status={datos.estado} />
          <Button disabled={datos.estado !== 'Abierta'} onClick={abrirCobro} leftIcon={<Wallet size={16} />}>
            Cobrar
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-2 gap-2 sm:grid-cols-4">
        <KpiTile label="Saldo" value={formatUSD(datos.saldo)} icon={<Wallet size={16} />} chip="bg-butter/25 text-butter-fg" />
        <KpiTile label="Total" value={formatUSD(datos.total)} icon={<Receipt size={16} />} chip="bg-primary/25 text-foreground" />
        <KpiTile label="Abonado" value={formatUSD(datos.totalAbonado)} icon={<HandCoins size={16} />} chip="bg-success/25 text-success-fg" />
        <KpiTile label="Consumos" value={consumos} icon={<UtensilsCrossed size={16} />} chip="bg-info/25 text-info-fg" />
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Consumos</CardTitle>
          <span className="text-xs text-muted-foreground">
            {datos.comandas.length} comanda{datos.comandas.length === 1 ? '' : 's'}
          </span>
        </CardHeader>
        <CardBody className="flex flex-col gap-4">
          {datos.comandas.length === 0 && <p className="text-sm text-muted-foreground">Sin consumos todavía.</p>}
          {datos.comandas.map((comanda) => (
            <div key={comanda.id} className="rounded-lg border border-border p-3">
              <div className="mb-2 flex items-center justify-between">
                <Pill tone={comanda.area === 'Barra' ? 'accent' : 'info'}>{comanda.area}</Pill>
                <span className="text-xs text-muted-foreground">{haceCuanto(comanda.fecha)}</span>
              </div>
              <div className="flex flex-col gap-1">
                {comanda.detalles.map((detalle) => (
                  <div key={detalle.id} className="flex items-center justify-between gap-2 rounded-xl bg-muted/40 px-3 py-2">
                    <div className="min-w-0">
                      <p className="truncate text-sm text-foreground">
                        {detalle.cantidad} × {detalle.nombre}
                        {detalle.esCortesia && <span className="ml-2 text-xs text-warning-fg">cortesía</span>}
                      </p>
                      <p className="text-xs text-muted-foreground">{formatUSD(detalle.subtotalUSD)}</p>
                    </div>
                    <StatusBadge status={detalle.estado} />
                  </div>
                ))}
              </div>
            </div>
          ))}
        </CardBody>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Acciones</CardTitle>
        </CardHeader>
        <CardBody className="grid grid-cols-1 gap-2 sm:grid-cols-3">
          <Button variant="ghost" onClick={() => setConsumoOpen(true)} leftIcon={<Plus size={16} />}>
            Agregar a la comanda
          </Button>
          <Button variant="ghost" onClick={() => setAbonoOpen(true)} leftIcon={<HandCoins size={16} />}>
            Abonar
          </Button>
          <Button variant="ghost" onClick={() => setDividirOpen(true)} leftIcon={<Scissors size={16} />}>
            Dividir
          </Button>
        </CardBody>
      </Card>

      <AgregarComandaModal
        cuentaId={id}
        abierto={consumoOpen}
        onCerrar={() => setConsumoOpen(false)}
        onEnviado={invalidar}
      />

      <Modal
        open={abonoOpen}
        onClose={() => setAbonoOpen(false)}
        title="Pago parcial (abono)"
        footer={
          <>
            <Button variant="ghost" onClick={() => setAbonoOpen(false)}>
              Cerrar
            </Button>
            <Button type="submit" form="form-abono" loading={abonar.isPending}>
              Registrar abono
            </Button>
          </>
        }
      >
        <div className="flex flex-col gap-4">
          <div className="rounded-xl border border-border bg-muted/40 px-4 py-3">
            <div className="flex items-baseline justify-between">
              <span className="text-sm text-muted-foreground">Saldo pendiente</span>
              <span className="num text-xl font-medium text-foreground">{formatUSD(datos.saldo)}</span>
            </div>
            {Number(abonoForm.watch('monto')) > 0 && (
              <div className="mt-1 flex items-baseline justify-between">
                <span className="text-sm text-muted-foreground">Quedará pendiente</span>
                <span className="num text-sm font-medium text-foreground">
                  {formatUSD(Math.max(0, datos.saldo - Number(abonoForm.watch('monto'))))}
                </span>
              </div>
            )}
          </div>

          <form
            id="form-abono"
            className="flex flex-col gap-3"
            onSubmit={abonoForm.handleSubmit((valores) => abonar.mutate(valores))}
            noValidate
          >
            <div>
              <p className="mb-1.5 text-sm font-medium text-foreground">Método de pago</p>
              <div className="flex flex-wrap gap-2">
                {metodos.data?.map((metodo: MetodoPago) => {
                  const activo = abonoForm.watch('metodoPagoId') === metodo.id;
                  return (
                    <button
                      key={metodo.id}
                      type="button"
                      onClick={() => abonoForm.setValue('metodoPagoId', metodo.id, { shouldValidate: true })}
                      className={cn(
                        'flex h-9 items-center gap-2 rounded-full border px-3.5 text-sm transition',
                        activo ? 'border-primary bg-primary/15 text-foreground' : 'border-border text-muted-foreground hover:text-foreground',
                      )}
                    >
                      {metodo.nombre}
                    </button>
                  );
                })}
              </div>
              {abonoForm.formState.errors.metodoPagoId?.message && (
                <p className="mt-1 text-sm text-destructive-fg">{abonoForm.formState.errors.metodoPagoId.message}</p>
              )}
            </div>

            <div className="flex flex-col gap-2">
              <Input
                type="number"
                placeholder="Monto (USD)"
                error={abonoForm.formState.errors.monto?.message}
                {...abonoForm.register('monto')}
              />
              <div className="flex gap-2">
                <Button type="button" variant="ghost" size="sm" onClick={() => abonoForm.setValue('monto', datos.saldo / 2)}>
                  Mitad
                </Button>
                <Button type="button" variant="ghost" size="sm" onClick={() => abonoForm.setValue('monto', datos.saldo)}>
                  Saldo completo
                </Button>
              </div>
            </div>
          </form>

          <div className="flex flex-col gap-1">
            <p className="text-xs font-medium text-muted-foreground">Historial de abonos</p>
            {datos.abonos.length === 0 && <p className="text-sm text-muted-foreground">Sin abonos todavía.</p>}
            {datos.abonos.map((abono) => (
              <div key={abono.id} className="flex justify-between text-xs text-muted-foreground">
                <span>{abono.metodoPago}</span>
                <span className="num text-foreground">{formatUSD(abono.monto)}</span>
              </div>
            ))}
          </div>
        </div>
      </Modal>

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
        <div className="flex flex-col gap-4">
          <div className="rounded-xl border border-border bg-muted/40 px-4 py-3">
            <div className="flex items-baseline justify-between">
              <span className="text-sm text-muted-foreground">Saldo a dividir</span>
              <span className="num text-xl font-medium text-foreground">{formatUSD(datos.saldo)}</span>
            </div>
          </div>

          <form id="form-dividir" onSubmit={dividirForm.handleSubmit((valores) => dividir.mutate(valores))} noValidate className="flex flex-col gap-3">
            <div>
              <p className="mb-1.5 text-sm font-medium text-foreground">Número de partes</p>
              <div className="flex flex-wrap gap-2">
                {[2, 3, 4, 5, 6].map((numero) => {
                  const activo = Number(dividirForm.watch('partes')) === numero;
                  return (
                    <button
                      key={numero}
                      type="button"
                      onClick={() => dividirForm.setValue('partes', numero)}
                      className={cn(
                        'flex h-9 min-w-10 items-center justify-center rounded-full border px-3 text-sm transition',
                        activo ? 'border-primary bg-primary/15 text-foreground' : 'border-border text-muted-foreground hover:text-foreground',
                      )}
                    >
                      {numero}
                    </button>
                  );
                })}
                <input
                  type="number"
                  min={2}
                  placeholder="N"
                  aria-label="Número de partes"
                  className="h-9 w-20 rounded-control border border-border bg-card px-3 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-ring/50"
                  {...dividirForm.register('partes')}
                />
              </div>
              {dividirForm.formState.errors.partes?.message && (
                <p className="mt-1 text-sm text-destructive-fg">{dividirForm.formState.errors.partes.message}</p>
              )}
            </div>
            {datos.saldo > 0 && Number(dividirForm.watch('partes')) >= 2 && (
              <p className="text-sm text-muted-foreground">
                Se divide en <span className="num font-medium text-foreground">{Number(dividirForm.watch('partes'))}</span> partes de{' '}
                <span className="num font-medium text-foreground">{formatUSD(datos.saldo / Number(dividirForm.watch('partes')))}</span> aprox.
              </p>
            )}
            <p className="text-xs text-muted-foreground">Divide para cobrar cada parte por separado. La cuenta sigue abierta.</p>
          </form>

          {datos.divisiones.length > 0 && (
            <div className="flex flex-col gap-1">
              <p className="text-xs font-medium text-muted-foreground">Partes actuales</p>
              {datos.divisiones.map((division) => (
                <div key={division.id} className="flex justify-between text-sm">
                  <span className="text-muted-foreground">Parte {division.indice}</span>
                  <span className={division.pagada ? 'text-success-fg' : 'text-foreground'}>{formatUSD(division.monto)}</span>
                </div>
              ))}
            </div>
          )}
        </div>
      </Modal>

      <Modal
        open={cobroOpen}
        onClose={() => setCobroOpen(false)}
        title="Cobrar cuenta"
        size="md"
        footer={
          <>
            <Button variant="ghost" onClick={() => setCobroOpen(false)}>
              Cancelar
            </Button>
            <Button
              disabled={pagosCierre.length === 0 || !cubierto}
              loading={cerrar.isPending}
              onClick={() => cerrar.mutate()}
              leftIcon={<Wallet size={16} />}
            >
              Cobrar y cerrar
            </Button>
          </>
        }
      >
        <div className="flex flex-col gap-4">
          <div className="flex items-baseline justify-between rounded-xl border border-border bg-muted/40 px-4 py-3">
            <span className="text-sm text-muted-foreground">Saldo a cobrar</span>
            <span className="num text-xl font-medium text-foreground">{formatUSD(datos.saldo)}</span>
          </div>

          <div className="flex gap-2">
            <Select aria-label="Método de pago" value={cierreMetodo} onChange={(evento) => setCierreMetodo(evento.target.value)}>
              <option value="">Método…</option>
              {metodos.data?.map((metodo: MetodoPago) => (
                <option key={metodo.id} value={metodo.id}>
                  {metodo.nombre}
                </option>
              ))}
            </Select>
            <Input
              aria-label="Monto"
              type="number"
              placeholder="Monto"
              value={cierreMonto}
              onChange={(evento) => setCierreMonto(evento.target.value)}
            />
            <Button
              variant="ghost"
              disabled={!cierreMetodo || Number(cierreMonto) <= 0}
              onClick={agregarPago}
            >
              Añadir
            </Button>
          </div>

          {pagosCierre.length > 0 && (
            <div className="flex flex-col gap-1">
              {pagosCierre.map((pago, indice) => (
                <div key={indice} className="flex items-center justify-between gap-2 text-xs text-muted-foreground">
                  <span>{metodos.data?.find((metodo) => metodo.id === pago.metodoPagoId)?.nombre ?? '—'}</span>
                  <span className="num text-foreground">{formatUSD(pago.monto)}</span>
                  <button
                    type="button"
                    aria-label="Quitar pago"
                    onClick={() => setPagosCierre((actuales) => actuales.filter((_, i) => i !== indice))}
                    className="flex h-6 w-6 items-center justify-center rounded-full text-muted-foreground transition hover:text-foreground"
                  >
                    <X size={13} />
                  </button>
                </div>
              ))}
              <span className="text-right text-xs text-foreground">
                Registrado: {formatUSD(totalCierre)} · {cubierto ? <span className="text-success-fg">Saldo cubierto</span> : <span>Falta {formatUSD(restante)}</span>}
              </span>
            </div>
          )}
        </div>
      </Modal>

      <TicketVenta venta={ventaTicket} onCerrar={() => navigate('/cuentas')} />
    </div>
  );
}