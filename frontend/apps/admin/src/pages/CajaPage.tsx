import { useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import {
  Button,
  Card,
  CardBody,
  CardHeader,
  CardTitle,
  DataTable,
  FiltroDropdown,
  FiltroFechas,
  Input,
  LimpiarFiltros,
  Pill,
  Select,
  Skeleton,
  StatusBadge,
} from '@licoreria/ui';
import type { Denominacion, MovimientoCaja, SesionCaja } from '@licoreria/types';
import { cajaApi } from '@licoreria/api-client';
import { mensajeDeError } from '../lib/api';
import { formatDateTime, formatUSD } from '../lib/format';

const esquemaAbrir = z.object({
  fondoInicial: z.coerce.number({ invalid_type_error: 'Ingresa el fondo' }).min(0, 'No puede ser negativo'),
});

const esquemaMovimiento = z.object({
  tipo: z.enum(['Ingreso', 'Egreso']),
  monto: z.coerce.number({ invalid_type_error: 'Ingresa el monto' }).positive('Debe ser mayor que 0'),
  motivo: z.string().min(3, 'Describe el motivo'),
});

type FormularioAbrir = z.infer<typeof esquemaAbrir>;
type FormularioMovimiento = z.infer<typeof esquemaMovimiento>;

export function CajaPage() {
  const queryClient = useQueryClient();
  const [cantidades, setCantidades] = useState<Record<string, number>>({});
  const [estadoHistorial, setEstadoHistorial] = useState('');
  const [desde, setDesde] = useState('');
  const [hasta, setHasta] = useState('');

  const abrirForm = useForm<FormularioAbrir>({
    resolver: zodResolver(esquemaAbrir),
    defaultValues: { fondoInicial: 0 },
  });
  const movimientoForm = useForm<FormularioMovimiento>({
    resolver: zodResolver(esquemaMovimiento),
    defaultValues: { tipo: 'Ingreso', monto: 0, motivo: '' },
  });

  const activa = useQuery({ queryKey: ['caja', 'activa'], queryFn: cajaApi.activa });
  const denominaciones = useQuery({ queryKey: ['caja', 'denominaciones'], queryFn: cajaApi.denominaciones });
  const historial = useQuery({ queryKey: ['caja', 'sesiones'], queryFn: () => cajaApi.sesiones({ pageSize: 10 }) });

  const invalidar = () => {
    queryClient.invalidateQueries({ queryKey: ['caja'] });
  };

  const abrir = useMutation({
    mutationFn: (datos: FormularioAbrir) => cajaApi.abrir(datos.fondoInicial),
    onSuccess: () => {
      toast.success('Caja abierta');
      abrirForm.reset({ fondoInicial: 0 });
      invalidar();
    },
    onError: (error) => toast.error('No se pudo abrir la caja', { description: mensajeDeError(error) }),
  });

  const movimiento = useMutation({
    mutationFn: (datos: FormularioMovimiento) => cajaApi.movimiento(activa.data!.id, { ...datos, moneda: 'USD' }),
    onSuccess: () => {
      toast.success('Movimiento registrado');
      movimientoForm.reset({ tipo: 'Ingreso', monto: 0, motivo: '' });
      invalidar();
    },
    onError: (error) => toast.error('No se pudo registrar', { description: mensajeDeError(error) }),
  });

  const cerrar = useMutation({
    mutationFn: (sesion: SesionCaja) =>
      cajaApi.cerrar(
        sesion.id,
        (denominaciones.data ?? [])
          .filter((denominacion) => denominacion.moneda === 'USD' && (cantidades[denominacion.id] ?? 0) > 0)
          .map((denominacion) => ({ denominacionId: denominacion.id, cantidad: cantidades[denominacion.id] ?? 0 })),
      ),
    onSuccess: () => {
      toast.success('Caja cerrada');
      setCantidades({});
      invalidar();
    },
    onError: (error) => toast.error('No se pudo cerrar la caja', { description: mensajeDeError(error) }),
  });

  const contado = useMemo(
    () =>
      (denominaciones.data ?? []).reduce(
        (acumulado, denominacion) => acumulado + denominacion.valor * (cantidades[denominacion.id] ?? 0),
        0,
      ),
    [denominaciones.data, cantidades],
  );

  const sesionesHistorial = useMemo(
    () =>
      (historial.data?.items ?? []).filter((item) => {
        if (estadoHistorial && item.estado !== estadoHistorial) return false;
        if (desde && item.abiertaEn.slice(0, 10) < desde) return false;
        if (hasta && item.abiertaEn.slice(0, 10) > hasta) return false;
        return true;
      }),
    [historial.data, estadoHistorial, desde, hasta],
  );

  const hayFiltros = Boolean(estadoHistorial || desde || hasta);

  const limpiarFiltros = () => {
    setEstadoHistorial('');
    setDesde('');
    setHasta('');
  };

  const sesion = activa.data ?? null;

  return (
    <div className="mx-auto flex max-w-page flex-col gap-4">
      {activa.isLoading ? (
        <Skeleton className="h-40 w-full" />
      ) : sesion ? (
        <>
          <Card>
            <CardHeader>
              <CardTitle>Sesión activa</CardTitle>
              <span className="text-xs text-muted-foreground">Abierta {formatDateTime(sesion.abiertaEn)}</span>
            </CardHeader>
            <CardBody className="grid grid-cols-2 gap-4 sm:grid-cols-3 xl:grid-cols-6">
              <div>
                <p className="text-xs text-muted-foreground">Fondo inicial</p>
                <p className="text-lg font-medium text-foreground">{formatUSD(sesion.fondoInicial)}</p>
              </div>
              <div>
                <p className="text-xs text-muted-foreground">Esperado</p>
                <p className="text-lg font-medium text-foreground">{formatUSD(sesion.montoEsperado)}</p>
              </div>
              <div>
                <p className="text-xs text-muted-foreground">Ventas del turno</p>
                <p className="num text-lg font-medium text-foreground">{formatUSD(sesion.ventasDelTurnoUSD)}</p>
              </div>
              <div>
                <p className="text-xs text-muted-foreground">Movimientos</p>
                <p className="text-lg font-medium text-foreground">{sesion.movimientos.length}</p>
              </div>
              <div>
                <p className="text-xs text-muted-foreground">Cuentas de mesa</p>
                <p className="num text-lg font-medium text-foreground">{sesion.cuentasDesalojadas}</p>
              </div>
              <div>
                <p className="text-xs text-muted-foreground">Estado</p>
                <Pill tone="success">{sesion.estado}</Pill>
              </div>
            </CardBody>
          </Card>

          <div className="grid grid-cols-1 gap-6 xl:grid-cols-2">
            <Card>
              <CardHeader>
                <CardTitle>Movimientos</CardTitle>
              </CardHeader>
              <CardBody className="flex flex-col gap-4">
                <DataTable<MovimientoCaja>
                  rows={sesion.movimientos}
                  rowKey={(movimientoItem) => movimientoItem.id}
                  empty="Sin movimientos."
                  columns={[
                    { key: 'fecha', header: 'Fecha', render: (m) => formatDateTime(m.fecha) },
                    { key: 'tipo', header: 'Tipo', render: (m) => <StatusBadge status={m.tipo} /> },
                    { key: 'motivo', header: 'Motivo', render: (m) => m.motivo },
                    {
                      key: 'monto',
                      header: 'Monto',
                      align: 'right',
                      render: (m) => formatUSD(m.monto),
                    },
                  ]}
                />
                <form
                  className="grid grid-cols-2 gap-2 sm:grid-cols-4"
                  onSubmit={movimientoForm.handleSubmit((datos) => movimiento.mutate(datos))}
                  noValidate
                >
                  <Select aria-label="Tipo" {...movimientoForm.register('tipo')}>
                    <option value="Ingreso">Ingreso</option>
                    <option value="Egreso">Egreso</option>
                  </Select>
                  <Input type="number" placeholder="Monto" error={movimientoForm.formState.errors.monto?.message} {...movimientoForm.register('monto')} />
                  <Input placeholder="Motivo" error={movimientoForm.formState.errors.motivo?.message} {...movimientoForm.register('motivo')} />
                  <Button type="submit" variant="ghost" loading={movimiento.isPending}>
                    Registrar
                  </Button>
                </form>
              </CardBody>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Arqueo y cierre</CardTitle>
                <span className="text-sm font-medium text-foreground">Contado {formatUSD(contado)}</span>
              </CardHeader>
              <CardBody className="flex flex-col gap-3">
                <div className="grid grid-cols-2 gap-2">
                  {(denominaciones.data ?? []).filter((denominacion) => denominacion.moneda === 'USD').map((denominacion: Denominacion) => (
                    <div key={denominacion.id} className="flex items-center justify-between gap-2 rounded-lg bg-muted/40 px-3 py-2">
                      <span className="text-sm text-foreground">
                        ${denominacion.valor}
                      </span>
                      <input
                        type="number"
                        min={0}
                        aria-label={`Cantidad $${denominacion.valor}`}
                        value={cantidades[denominacion.id] ?? ''}
                        onChange={(evento) => {
                          const valor = evento.target.value;
                          setCantidades((actuales) => {
                            const siguiente = { ...actuales };
                            if (valor === '') delete siguiente[denominacion.id];
                            else siguiente[denominacion.id] = Math.max(0, Number(valor));
                            return siguiente;
                          });
                        }}
                        onKeyDown={(evento) => {
                          if (evento.key !== 'Enter') return;
                          evento.preventDefault();
                          const inputs = Array.from(document.querySelectorAll<HTMLInputElement>('.arqueo-cantidad'));
                          const siguiente = inputs[inputs.indexOf(evento.currentTarget as HTMLInputElement) + 1];
                          siguiente?.focus();
                        }}
                        className="arqueo-cantidad h-9 w-20 rounded-control border border-border bg-card px-3 text-sm text-foreground"
                      />
                    </div>
                  ))}
                  {(denominaciones.data ?? []).filter((denominacion) => denominacion.moneda === 'USD').length === 0 && <p className="text-sm text-muted-foreground">Sin denominaciones.</p>}
                </div>
                <Button variant="danger" loading={cerrar.isPending} onClick={() => cerrar.mutate(sesion)}>
                  Cerrar caja (Z)
                </Button>
              </CardBody>
            </Card>
          </div>
        </>
      ) : (
        <Card>
          <CardHeader>
            <CardTitle>No hay caja abierta</CardTitle>
          </CardHeader>
          <CardBody>
            <form
              className="flex max-w-sm items-end gap-2"
              onSubmit={abrirForm.handleSubmit((datos) => abrir.mutate(datos))}
              noValidate
            >
              <Input
                label="Fondo inicial (USD)"
                type="number"
                error={abrirForm.formState.errors.fondoInicial?.message}
                {...abrirForm.register('fondoInicial')}
              />
              <Button type="submit" loading={abrir.isPending}>
                Abrir caja
              </Button>
            </form>
          </CardBody>
        </Card>
      )}

      <Card>
        <CardHeader className="flex flex-col items-stretch gap-3">
          <div className="flex flex-wrap items-center gap-2">
            <FiltroDropdown
              label="Estado"
              opciones={[
                { valor: 'Abierta', etiqueta: 'Abiertas' },
                { valor: 'Cerrada', etiqueta: 'Cerradas' },
              ]}
              valor={estadoHistorial}
              onChange={setEstadoHistorial}
            />
            <FiltroFechas
              desde={desde}
              hasta={hasta}
              onDesde={setDesde}
              onHasta={setHasta}
            />
            <div className="ml-auto">
              <LimpiarFiltros activo={hayFiltros} onClick={limpiarFiltros} />
            </div>
          </div>
        </CardHeader>
        <CardBody>
          <DataTable<SesionCaja>
            rows={sesionesHistorial}
            loading={historial.isLoading}
            rowKey={(item) => item.id}
            empty="Sin sesiones."
            columns={[
              { key: 'abierta', header: 'Abierta', render: (item) => formatDateTime(item.abiertaEn) },
              { key: 'cerrada', header: 'Cerrada', render: (item) => (item.cerradaEn ? formatDateTime(item.cerradaEn) : '—') },
              { key: 'estado', header: 'Estado', render: (item) => <StatusBadge status={item.estado} /> },
              {
                key: 'ventas',
                header: 'Ventas del turno',
                align: 'right',
                render: (item) => formatUSD(item.ventasDelTurnoUSD),
              },
              { key: 'mesas', header: 'Mesas desalojadas', align: 'right', render: (item) => item.cuentasDesalojadas },
              { key: 'contado', header: 'Contado', align: 'right', render: (item) => formatUSD(item.montoContado) },
              {
                key: 'descuadre',
                header: 'Descuadre',
                align: 'right',
                render: (item) => (
                  <span className={item.descuadre === 0 ? 'text-success-fg' : 'text-destructive-fg'}>{formatUSD(item.descuadre)}</span>
                ),
              },
            ]}
          />
        </CardBody>
      </Card>
    </div>
  );
}
