import { useMemo, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { CheckCircle2, Eye, Plus, Send, Truck, XCircle } from 'lucide-react';
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
  Modal,
  ModalSection,
  Pagination,
  Pill,
  type ActionMenuOption,
} from '@licoreria/ui';
import type { EstadoOrdenCompra, OrdenCompra } from '@licoreria/types';
import { comprasApi, proveedoresApi } from '@licoreria/api-client';
import { EditorOrdenCompra } from '../components/compras/EditorOrdenCompra';
import { InlineForm } from '../components/InlineForm';
import { mensajeDeError } from '../lib/api';
import { formatDateTime, formatNumber, formatUSD } from '../lib/format';
import { contiene, paginarEnMemoria, PAGE_SIZE_FILTRO_LOCAL } from '../lib/filtros';

const ESTADOS: EstadoOrdenCompra[] = ['Borrador', 'Aprobada', 'Enviada', 'RecibidaParcial', 'Recibida', 'Cancelada'];

const tono: Record<string, 'success' | 'danger' | 'warning' | 'info' | 'accent' | 'neutral'> = {
  Borrador: 'neutral',
  Aprobada: 'info',
  Enviada: 'accent',
  RecibidaParcial: 'warning',
  Recibida: 'success',
  Cancelada: 'danger',
};

export function ComprasPage() {
  const navigate = useNavigate();
  const { pathname } = useLocation();
  const [page, setPage] = useState(1);
  const [estado, setEstado] = useState('');
  const [proveedorFiltro, setProveedorFiltro] = useState('');
  const [busqueda, setBusqueda] = useState('');
  const [desde, setDesde] = useState('');
  const [hasta, setHasta] = useState('');
  const [totalMin, setTotalMin] = useState('');
  const [totalMax, setTotalMax] = useState('');
  const [detalle, setDetalle] = useState<OrdenCompra | null>(null);
  const [recepcionando, setRecepcionando] = useState<OrdenCompra | null>(null);
  const [recepcionQty, setRecepcionQty] = useState<Record<string, string>>({});
  const [recepcionObs, setRecepcionObs] = useState('');
  const queryClient = useQueryClient();

  const hayFiltroLocal = Boolean(busqueda || proveedorFiltro || desde || hasta || totalMin || totalMax);
  const hayFiltros = hayFiltroLocal || Boolean(estado);

  const limpiarFiltros = () => {
    setBusqueda('');
    setEstado('');
    setProveedorFiltro('');
    setDesde('');
    setHasta('');
    setTotalMin('');
    setTotalMax('');
    setPage(1);
  };

  const ordenes = useQuery({
    queryKey: ['ordenes', page, estado, hayFiltroLocal ? PAGE_SIZE_FILTRO_LOCAL : 15],
    queryFn: () =>
      comprasApi.ordenes({
        page: hayFiltroLocal ? 1 : page,
        pageSize: hayFiltroLocal ? PAGE_SIZE_FILTRO_LOCAL : 15,
        estado: estado || undefined,
      }),
  });
  const proveedores = useQuery({ queryKey: ['proveedores'], queryFn: () => proveedoresApi.listar() });

  const { items: filas, totalPages } = useMemo(() => {
    const totalMinN = Number(totalMin);
    const totalMaxN = Number(totalMax);
    const filtradas = (ordenes.data?.items ?? []).filter((orden) => {
      if (!contiene(`${orden.numero} ${orden.proveedorNombre}`, busqueda)) return false;
      if (proveedorFiltro && orden.proveedorId !== proveedorFiltro) return false;
      if (desde && orden.fecha.slice(0, 10) < desde) return false;
      if (hasta && orden.fecha.slice(0, 10) > hasta) return false;
      if (totalMinN && orden.totalUSD < totalMinN) return false;
      if (totalMaxN && orden.totalUSD > totalMaxN) return false;
      return true;
    });
    return hayFiltroLocal
      ? paginarEnMemoria(filtradas, page, 15)
      : { items: filtradas, totalPages: ordenes.data?.totalPages ?? 1 };
  }, [ordenes.data, busqueda, proveedorFiltro, desde, hasta, totalMin, totalMax, hayFiltroLocal, page]);

  const invalidar = () => {
    queryClient.invalidateQueries({ queryKey: ['ordenes'] });
    queryClient.invalidateQueries({ queryKey: ['recepciones'] });
    queryClient.invalidateQueries({ queryKey: ['cuentas-pagar'] });
    queryClient.invalidateQueries({ queryKey: ['stock'] });
    queryClient.invalidateQueries({ queryKey: ['kardex'] });
    queryClient.invalidateQueries({ queryKey: ['productos'] });
  };

  const accion = useMutation({
    mutationFn: ({ id, op }: { id: string; op: 'aprobar' | 'enviar' | 'cancelar' }) => comprasApi[op](id),
    onSuccess: (_data, variables) => {
      toast.success(`Orden ${variables.op === 'aprobar' ? 'aprobada' : variables.op === 'enviar' ? 'enviada' : 'cancelada'}`);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo actualizar la orden', { description: mensajeDeError(error) }),
  });

  const recepcionar = useMutation({
    mutationFn: (orden: OrdenCompra) =>
      comprasApi.registrarRecepcion({
        ordenCompraId: orden.id,
        observaciones: recepcionObs || null,
        detalles: Object.entries(recepcionQty)
          .filter(([, valor]) => Number(valor) > 0)
          .map(([ordenCompraDetalleId, valor]) => ({ ordenCompraDetalleId, cantidad: Number(valor) })),
      }),
    onSuccess: () => {
      toast.success('Recepción registrada');
      setRecepcionando(null);
      setRecepcionQty({});
      setRecepcionObs('');
      invalidar();
    },
    onError: (error) => toast.error('No se pudo recepcionar', { description: mensajeDeError(error) }),
  });

  const abrirRecepcion = (orden: OrdenCompra) => {
    setRecepcionQty(
      Object.fromEntries(orden.detalles.map((item) => [item.id, String(Math.max(0, item.cantidad - item.cantidadRecibida))])),
    );
    setRecepcionObs('');
    setRecepcionando(orden);
  };

  if (pathname === '/compras/ordenes/nueva') {
    return <EditorOrdenCompra />;
  }

  return (
    <div className="mx-auto flex max-w-page flex-col gap-4">
        <Card>
          <CardHeader className="flex flex-col items-stretch gap-3">
            <div className="flex flex-wrap items-center gap-2">
              <Buscador
                placeholder="Buscar número o proveedor…"
                value={busqueda}
                onCambio={(valor) => {
                  setBusqueda(valor);
                  setPage(1);
                }}
              />
              <FiltroDropdown
                label="Estado"
                opciones={ESTADOS.map((valor) => ({ valor, etiqueta: valor }))}
                valor={estado}
                onChange={(valor) => {
                  setEstado(valor);
                  setPage(1);
                }}
              />
              <FiltroFechas
                desde={desde}
                hasta={hasta}
                onDesde={(valor) => { setDesde(valor); setPage(1); }}
                onHasta={(valor) => { setHasta(valor); setPage(1); }}
              />
              <div className="ml-auto">
                <Button leftIcon={<Plus size={15} />} onClick={() => navigate('/compras/ordenes/nueva')}>
                  Nueva orden
                </Button>
              </div>
            </div>
            <div className="border-b border-border" />
            <div className="flex flex-wrap items-center gap-2">
              <FiltroDropdown
                label="Proveedor"
                opciones={(proveedores.data ?? []).map((p) => ({ valor: p.id, etiqueta: p.nombre }))}
                valor={proveedorFiltro}
                onChange={(valor) => {
                  setProveedorFiltro(valor);
                  setPage(1);
                }}
              />
              <FiltroRango
                label="Total USD"
                minimo={totalMin}
                maximo={totalMax}
                onMinimo={(valor) => {
                  setTotalMin(valor);
                  setPage(1);
                }}
                onMaximo={(valor) => {
                  setTotalMax(valor);
                  setPage(1);
                }}
              />
              <div className="ml-auto">
                <LimpiarFiltros activo={hayFiltros} onClick={limpiarFiltros} />
              </div>
            </div>
          </CardHeader>
          <CardBody>
            <DataTable<OrdenCompra>
              rows={filas}
              loading={ordenes.isLoading}
              rowKey={(orden) => orden.id}
              empty="No hay órdenes."
              expandirKey={recepcionando?.id ?? null}
              expandedRow={
                recepcionando
                  ? (orden) =>
                      orden.id === recepcionando.id ? (
                        <InlineForm title={`Recepcionar · ${orden.numero}`} onCancel={() => setRecepcionando(null)}>
                          <div className="flex flex-col gap-3">
                            <Input
                              label="Observaciones"
                              value={recepcionObs}
                              onChange={(evento) => setRecepcionObs(evento.target.value)}
                            />
                            <div className="flex flex-col gap-2">
                              {recepcionando.detalles.map((item) => {
                                const pendiente = Math.max(0, item.cantidad - item.cantidadRecibida);
                                return (
                                  <div key={item.id} className="flex items-center justify-between gap-3 rounded-inner bg-muted/40 px-3 py-2">
                                    <div>
                                      <p className="text-sm text-foreground">
                                        {item.nombre} <span className="text-xs text-muted-foreground">{item.sku}</span>
                                      </p>
                                      <p className="text-xs text-muted-foreground">Pendiente: {formatNumber(pendiente)}</p>
                                    </div>
                                    <input
                                      type="number"
                                      min={0}
                                      max={pendiente}
                                      aria-label={`Recibir ${item.sku}`}
                                      value={recepcionQty[item.id] ?? ''}
                                      onChange={(evento) =>
                                        setRecepcionQty((actuales) => ({
                                          ...actuales,
                                          [item.id]: String(Math.min(pendiente, Math.max(0, Number(evento.target.value)))),
                                        }))
                                      }
                                      className="num h-9 w-24 rounded-control border border-border bg-card px-3 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-ring/50"
                                    />
                                  </div>
                                );
                              })}
                            </div>
                            <div className="flex justify-end gap-2">
                              <Button variant="ghost" onClick={() => setRecepcionando(null)}>
                                Cancelar
                              </Button>
                              <Button
                                disabled={Object.values(recepcionQty).every((valor) => Number(valor) <= 0)}
                                loading={recepcionar.isPending}
                                onClick={() => recepcionando && recepcionar.mutate(recepcionando)}
                              >
                                Registrar recepción
                              </Button>
                            </div>
                          </div>
                        </InlineForm>
                      ) : null
                  : undefined
              }
              columns={[
                { key: 'numero', header: 'Número', render: (orden) => <span className="text-foreground">{orden.numero}</span> },
                { key: 'proveedor', header: 'Proveedor', render: (orden) => orden.proveedorNombre },
                { key: 'fecha', header: 'Fecha', render: (orden) => formatDateTime(orden.fecha) },
                { key: 'estado', header: 'Estado', render: (orden) => <Pill tone={tono[orden.estado] ?? 'neutral'}>{orden.estado}</Pill> },
                { key: 'total', header: 'Total', align: 'right', render: (orden) => formatUSD(orden.totalUSD) },
                {
                  key: 'acciones',
                  header: '',
                  align: 'right',
                  render: (orden) => {
                    const opciones: ActionMenuOption[] = [
                      { label: 'Ver detalle', icon: <Eye size={15} />, onClick: () => setDetalle(orden) },
                    ];
                    if (orden.estado === 'Borrador') {
                      opciones.push({ label: 'Aprobar', icon: <CheckCircle2 size={15} />, onClick: () => accion.mutate({ id: orden.id, op: 'aprobar' }) });
                      opciones.push({ label: 'Cancelar', icon: <XCircle size={15} />, danger: true, onClick: () => accion.mutate({ id: orden.id, op: 'cancelar' }) });
                    } else if (orden.estado === 'Aprobada') {
                      opciones.push({ label: 'Enviar', icon: <Send size={15} />, onClick: () => accion.mutate({ id: orden.id, op: 'enviar' }) });
                      opciones.push({ label: 'Cancelar', icon: <XCircle size={15} />, danger: true, onClick: () => accion.mutate({ id: orden.id, op: 'cancelar' }) });
                    } else if (orden.estado === 'Enviada' || orden.estado === 'RecibidaParcial') {
                      opciones.push({ label: 'Recepcionar', icon: <Truck size={15} />, onClick: () => abrirRecepcion(orden) });
                      if (orden.estado === 'Enviada') {
                        opciones.push({ label: 'Cancelar', icon: <XCircle size={15} />, danger: true, onClick: () => accion.mutate({ id: orden.id, op: 'cancelar' }) });
                      }
                    }
                    return <ActionMenu label={`Acciones de ${orden.numero}`} options={opciones} />;
                  },
                },
              ]}
            />
            <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
          </CardBody>
        </Card>

      <Modal
        open={Boolean(detalle)}
        onClose={() => setDetalle(null)}
        title={`Orden ${detalle?.numero ?? ''}`}
        size="lg"
        backdrop="none"
      >
        {detalle && (
          <div className="flex flex-col gap-5">
            <div className="flex flex-wrap items-center gap-3">
              <Pill tone={tono[detalle.estado] ?? 'neutral'}>{detalle.estado}</Pill>
              <span className="text-sm text-muted-foreground">{detalle.proveedorNombre}</span>
              <span className="text-sm text-muted-foreground">{formatDateTime(detalle.fecha)}</span>
            </div>
            <ModalSection title="Líneas">
              <div className="overflow-x-auto rounded-inner border border-border">
                <table className="w-full min-w-[520px] text-sm">
                  <thead>
                    <tr className="bg-muted/60 text-left text-xs uppercase tracking-tighter2 text-muted-foreground">
                      <th scope="col" className="px-4 py-2.5">SKU</th>
                      <th scope="col" className="px-4 py-2.5 text-right">Cantidad</th>
                      <th scope="col" className="px-4 py-2.5 text-right">Recibida</th>
                      <th scope="col" className="px-4 py-2.5 text-right">Costo</th>
                      <th scope="col" className="px-4 py-2.5 text-right">Subtotal</th>
                    </tr>
                  </thead>
                  <tbody>
                    {detalle.detalles.map((item) => (
                      <tr key={item.id} className="border-t border-border">
                        <td className="px-4 py-2.5 text-foreground">
                          {item.nombre}
                          <span className="ml-2 text-xs text-muted-foreground">{item.sku}</span>
                        </td>
                        <td className="num px-4 py-2.5 text-right text-muted-foreground">{formatNumber(item.cantidad)}</td>
                        <td className="num px-4 py-2.5 text-right text-muted-foreground">{formatNumber(item.cantidadRecibida)}</td>
                        <td className="num px-4 py-2.5 text-right text-muted-foreground">{formatUSD(item.costoUnitarioUSD)}</td>
                        <td className="num px-4 py-2.5 text-right text-foreground">{formatUSD(item.subtotalUSD)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </ModalSection>
            <div className="flex justify-end border-t border-border pt-3">
              <div className="flex items-baseline gap-3">
                <span className="text-sm text-muted-foreground">Total</span>
                <span className="num text-lg font-medium text-foreground">{formatUSD(detalle.totalUSD)}</span>
              </div>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}