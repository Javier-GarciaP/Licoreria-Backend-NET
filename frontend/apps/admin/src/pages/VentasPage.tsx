import { useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Eye, RotateCcw } from 'lucide-react';
import {
  ActionMenu,
  Buscador,
  Button,
  Card,
  CardBody,
  CardHeader,
  DataTable,
  FiltroDropdown,
  FiltroFechas,
  FiltroRango,
  Input,
  LimpiarFiltros,
  Pagination,
  StatusBadge,
} from '@licoreria/ui';
import type { Venta } from '@licoreria/types';
import { ventasApi } from '@licoreria/api-client';
import { useAuth } from '../context/AuthContext';
import { InlineForm } from '../components/InlineForm';
import { DetalleVentaModal } from '../components/DetalleVentaModal';
import { TicketVenta } from '../components/pos/TicketVenta';
import { mensajeDeError } from '../lib/api';
import { formatDateTime, formatUSD } from '../lib/format';
import { contiene, paginarEnMemoria, PAGE_SIZE_FILTRO_LOCAL } from '../lib/filtros';

const esquemaDevolucion = z.object({
  motivo: z.string().min(3, 'Describe el motivo (mínimo 3 caracteres)'),
});

type FormularioDevolucion = z.infer<typeof esquemaDevolucion>;

export function VentasPage() {
  const [page, setPage] = useState(1);
  const [detalle, setDetalle] = useState<Venta | null>(null);
  const [devolver, setDevolver] = useState<Venta | null>(null);
  const [imprimiendo, setImprimiendo] = useState<Venta | null>(null);
  const [reintegrar, setReintegrar] = useState(true);
  const [cantidades, setCantidades] = useState<Record<string, number>>({});
  const [busqueda, setBusqueda] = useState('');
  const [estado, setEstado] = useState('');
  const [metodoPago, setMetodoPago] = useState('');
  const [totalMin, setTotalMin] = useState('');
  const [totalMax, setTotalMax] = useState('');
  const [desde, setDesde] = useState('');
  const [hasta, setHasta] = useState('');
  const queryClient = useQueryClient();
  const { tienePermiso } = useAuth() as { tienePermiso: (clave: string) => boolean };
  const puedeDevolver = tienePermiso('sales:void');

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormularioDevolucion>({ resolver: zodResolver(esquemaDevolucion), defaultValues: { motivo: '' } });

  const hayFiltroLocal = Boolean(busqueda || estado || metodoPago || totalMin || totalMax);
  const hayFiltros = hayFiltroLocal || Boolean(desde || hasta);

  const limpiarFiltros = () => {
    setBusqueda('');
    setEstado('');
    setMetodoPago('');
    setTotalMin('');
    setTotalMax('');
    setDesde('');
    setHasta('');
    setPage(1);
  };

  const ventas = useQuery({
    queryKey: ['ventas', page, desde, hasta, hayFiltroLocal ? PAGE_SIZE_FILTRO_LOCAL : 15],
    queryFn: () =>
      ventasApi.listar({
        page: hayFiltroLocal ? 1 : page,
        pageSize: hayFiltroLocal ? PAGE_SIZE_FILTRO_LOCAL : 15,
        desde: desde || undefined,
        hasta: hasta || undefined,
      }),
  });

  const opcionesEstado = useMemo(() => {
    const valores = new Set<string>();
    for (const venta of ventas.data?.items ?? []) valores.add(venta.estado);
    return [...valores].sort().map((valor) => ({ valor, etiqueta: valor }));
  }, [ventas.data]);

  const opcionesMetodo = useMemo(() => {
    const valores = new Set<string>();
    for (const venta of ventas.data?.items ?? []) {
      for (const pago of venta.pagos) valores.add(pago.metodoPago);
    }
    return [...valores].sort().map((valor) => ({ valor, etiqueta: valor }));
  }, [ventas.data]);

  const { items: filas, totalPages } = useMemo(() => {
    const totalMinN = Number(totalMin);
    const totalMaxN = Number(totalMax);
    const filtradas = (ventas.data?.items ?? []).filter((venta) => {
      if (!contiene(venta.numeroComprobante, busqueda)) return false;
      if (estado && venta.estado !== estado) return false;
      if (metodoPago && !venta.pagos.some((pago) => pago.metodoPago === metodoPago)) return false;
      if (totalMinN && venta.totalUSD < totalMinN) return false;
      if (totalMaxN && venta.totalUSD > totalMaxN) return false;
      return true;
    });
    return hayFiltroLocal
      ? paginarEnMemoria(filtradas, page, 15)
      : { items: filtradas, totalPages: ventas.data?.totalPages ?? 1 };
  }, [ventas.data, busqueda, estado, metodoPago, totalMin, totalMax, hayFiltroLocal, page]);

  const devolucion = useMutation({
    mutationFn: ({ venta, motivo }: { venta: Venta; motivo: string }) =>
      ventasApi.devolver(venta.id, {
        motivo,
        reintegrarInventario: reintegrar,
        detalles: Object.entries(cantidades)
          .filter(([, cantidad]) => cantidad > 0)
          .map(([varianteId, cantidad]) => ({ varianteId, cantidad })),
      }),
    onSuccess: () => {
      toast.success('Devolución registrada');
      setDevolver(null);
      reset({ motivo: '' });
      setCantidades({});
      queryClient.invalidateQueries({ queryKey: ['ventas'] });
    },
    onError: (error) => toast.error('No se pudo registrar la devolución', { description: mensajeDeError(error) }),
  });

  const totalDevolver = useMemo(
    () => (devolver ? devolver.detalles.filter((d) => (cantidades[d.varianteId] ?? 0) > 0).length : 0),
    [devolver, cantidades],
  );

  const abrirDevolucion = (venta: Venta) => {
    setDevolver(venta);
    reset({ motivo: '' });
    setCantidades({});
  };

  return (
    <div className="mx-auto flex max-w-page flex-col gap-4">
      <Card>
        <CardHeader className="flex flex-col items-stretch gap-3">
          <div className="flex flex-wrap items-center gap-2">
            <Buscador
              placeholder="Buscar comprobante…"
              value={busqueda}
              onCambio={(valor) => {
                setBusqueda(valor);
                setPage(1);
              }}
            />
            <FiltroDropdown label="Estado" opciones={opcionesEstado} valor={estado} onChange={(valor) => { setEstado(valor); setPage(1); }} />
            <FiltroDropdown
              label="Método pago"
              opciones={opcionesMetodo}
              valor={metodoPago}
              onChange={(valor) => {
                setMetodoPago(valor);
                setPage(1);
              }}
            />
            <FiltroFechas
              desde={desde}
              hasta={hasta}
              onDesde={(valor) => { setDesde(valor); setPage(1); }}
              onHasta={(valor) => { setHasta(valor); setPage(1); }}
            />
          </div>
          <div className="border-b border-border" />
          <div className="flex flex-wrap items-center gap-2">
            <FiltroRango
              label="Total USD"
              minimo={totalMin}
              maximo={totalMax}
              onMinimo={(valor) => { setTotalMin(valor); setPage(1); }}
              onMaximo={(valor) => { setTotalMax(valor); setPage(1); }}
            />
            <div className="ml-auto">
              <LimpiarFiltros activo={hayFiltros} onClick={limpiarFiltros} />
            </div>
          </div>
        </CardHeader>
        <CardBody>
          <DataTable<Venta>
            rows={filas}
            loading={ventas.isLoading}
            rowKey={(venta) => venta.id}
            empty="No hay ventas."
            onRowClick={(venta) => setDetalle(venta)}
            expandirKey={devolver?.id ?? null}
            expandedRow={
              devolver
                ? (venta) =>
                    venta.id === devolver.id ? (
                      <InlineForm title={`Devolución · ${venta.numeroComprobante ?? ''}`} onCancel={() => setDevolver(null)}>
                        <form
                          className="flex flex-col gap-4"
                          onSubmit={handleSubmit((datos) => devolucion.mutate({ venta: devolver, motivo: datos.motivo }))}
                          noValidate
                        >
                          <div className="flex flex-col gap-2">
                            {devolver.detalles.map((linea) => (
                              <div key={linea.id} className="flex items-center justify-between gap-3">
                                <span className="text-sm text-foreground">{linea.nombre}</span>
                                <div className="flex items-center gap-2">
                                  <span className="text-xs text-muted-foreground">de {linea.cantidad}</span>
                                  <input
                                    type="number"
                                    min={0}
                                    max={linea.cantidad}
                                    aria-label={`Cantidad a devolver de ${linea.nombre}`}
                                    value={cantidades[linea.varianteId] ?? 0}
                                    onChange={(evento) =>
                                      setCantidades((actuales) => ({
                                        ...actuales,
                                        [linea.varianteId]: Math.min(linea.cantidad, Math.max(0, Number(evento.target.value))),
                                      }))
                                    }
                                    className="num h-9 w-20 rounded-control border border-border bg-card px-3 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-ring/50"
                                  />
                                </div>
                              </div>
                            ))}
                            {totalDevolver === 0 && (
                              <span className="text-xs text-muted-foreground">Indica al menos una cantidad a devolver.</span>
                            )}
                          </div>
                          <Input
                            label="Motivo"
                            placeholder="Producto dañado, error de cobro…"
                            error={errors.motivo?.message}
                            {...register('motivo')}
                          />
                          <label className="flex items-center gap-2 text-sm text-muted-foreground">
                            <input type="checkbox" checked={reintegrar} onChange={(evento) => setReintegrar(evento.target.checked)} />
                            Reintegrar al inventario
                          </label>
                          <div className="flex justify-end gap-2">
                            <Button variant="ghost" onClick={() => setDevolver(null)}>
                              Cancelar
                            </Button>
                            <Button type="submit" disabled={totalDevolver === 0} loading={devolucion.isPending}>
                              Registrar devolución
                            </Button>
                          </div>
                        </form>
                      </InlineForm>
                    ) : null
                  : undefined
            }
            columns={[
              { key: 'fecha', header: 'Fecha', render: (venta) => formatDateTime(venta.fecha) },
              { key: 'comprobante', header: 'Comprobante', render: (venta) => venta.numeroComprobante ?? venta.id.slice(0, 8) },
              { key: 'estado', header: 'Estado', render: (venta) => <StatusBadge status={venta.estado} /> },
              { key: 'total', header: 'Total', align: 'right', render: (venta) => formatUSD(venta.totalUSD) },
              { key: 'bs', header: 'Total Bs', align: 'right', render: (venta) => `Bs ${venta.totalBS.toFixed(2)}` },
              {
                key: 'acciones',
                header: '',
                align: 'right',
                render: (venta) => (
                  <ActionMenu
                    label={`Acciones de venta ${venta.numeroComprobante ?? ''}`}
                    options={[
                      { label: 'Ver detalle', icon: <Eye size={15} />, onClick: () => setDetalle(venta) },
                      ...(puedeDevolver
                        ? [{ label: 'Devolución', icon: <RotateCcw size={15} />, onClick: () => abrirDevolucion(venta) }]
                        : []),
                    ]}
                  />
                ),
              },
            ]}
          />
          <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
        </CardBody>
      </Card>

      <DetalleVentaModal
        venta={detalle}
        onCerrar={() => setDetalle(null)}
        onImprimir={() => {
          setImprimiendo(detalle);
          setDetalle(null);
        }}
      />
      <TicketVenta venta={imprimiendo} onCerrar={() => setImprimiendo(null)} />
    </div>
  );
}