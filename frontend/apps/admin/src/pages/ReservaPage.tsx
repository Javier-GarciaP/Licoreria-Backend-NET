import { useMemo } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'sonner';
import { ArrowLeft } from 'lucide-react';
import { Button, Card, CardBody, CardHeader, CardTitle, Pill, Skeleton, StatusBadge } from '@licoreria/ui';
import type { PedidoAnticipado, Reserva } from '@licoreria/types';
import { clubApi } from '@licoreria/api-client';
import { PanelAgregarPedido } from '../components/reservas/PanelAgregarPedido';
import { mensajeDeError } from '../lib/api';
import { formatDateTime, formatUSD } from '../lib/format';

/** Detalle de una reserva: pedidos anticipados, agregar pedidos y señas. */
export function ReservaPage() {
  const { id = '' } = useParams();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const reserva = useQuery({ queryKey: ['reserva', id], queryFn: () => clubApi.reserva(id), enabled: Boolean(id) });
  const pedidos = useQuery({ queryKey: ['reserva', id, 'pedidos'], queryFn: () => clubApi.pedidos(id), enabled: Boolean(id) });

  const invalidar = () => {
    queryClient.invalidateQueries({ queryKey: ['reserva', id] });
    queryClient.invalidateQueries({ queryKey: ['reserva', id, 'pedidos'] });
    queryClient.invalidateQueries({ queryKey: ['reservas'] });
  };

  const cambiarEstado = useMutation({
    mutationFn: (estado: string) => clubApi.cambiarEstadoReserva(id, estado),
    onSuccess: () => {
      toast.success('Reserva actualizada');
      invalidar();
    },
    onError: (error) => toast.error('No se pudo actualizar', { description: mensajeDeError(error) }),
  });

  const validarPago = useMutation({
    mutationFn: ({ pagoId, aprobar }: { pagoId: string; aprobar: boolean }) => clubApi.validarPagoReserva(id, pagoId, aprobar),
    onSuccess: (_data, variables) => {
      toast.success(variables.aprobar ? 'Seña validada' : 'Seña rechazada');
      invalidar();
    },
    onError: (error) => toast.error('No se pudo validar la seña', { description: mensajeDeError(error) }),
  });

  const quitarPedido = useMutation({
    mutationFn: (pedidoId: string) => clubApi.eliminarPedido(id, pedidoId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['reserva', id, 'pedidos'] }),
    onError: (error) => toast.error('No se pudo quitar', { description: mensajeDeError(error) }),
  });

  const datos: Reserva | undefined = reserva.data;

  const totalPedidos = useMemo(() => (pedidos.data ?? []).reduce((acc, pedido) => acc + pedido.subtotalUSD, 0), [pedidos.data]);

  if (reserva.isLoading) {
    return (
      <div className="mx-auto max-w-page">
        <Skeleton className="h-96 w-full" />
      </div>
    );
  }

  if (!datos) {
    return (
      <div className="mx-auto flex max-w-page flex-col items-center gap-4 text-center">
        <p className="text-sm text-muted-foreground">Esta reserva no existe o fue eliminada.</p>
        <Button variant="ghost" onClick={() => navigate('/reservas')}>
          Volver a reservas
        </Button>
      </div>
    );
  }

  const mesasTexto = datos.mesas.map((mesa) => mesa.numero).join(', ');

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <div className="flex flex-wrap items-center gap-2 border-b border-border pb-2">
        <Button variant="ghost" size="sm" type="button" onClick={() => navigate('/reservas')} aria-label="Volver">
          <ArrowLeft size={16} />
        </Button>
        <div>
          <p className="text-base font-medium tracking-tighter2 text-foreground">Reserva · {datos.nombreContacto}</p>
          <p className="text-xs text-muted-foreground">
            {formatDateTime(datos.fechaHora)} · {datos.personas} {datos.personas === 1 ? 'persona' : 'personas'}
            {mesasTexto ? ` · Mesas ${mesasTexto}` : ''}
          </p>
        </div>
        <div className="ml-auto flex items-center gap-2">
          <StatusBadge status={datos.estado} />
          {datos.estado === 'Pendiente' && (
            <Button size="sm" variant="ghost" loading={cambiarEstado.isPending} onClick={() => cambiarEstado.mutate('Confirmada')}>
              Confirmar
            </Button>
          )}
          {(datos.estado === 'Pendiente' || datos.estado === 'Confirmada') && (
            <Button size="sm" variant="ghost" loading={cambiarEstado.isPending} onClick={() => cambiarEstado.mutate('Cancelada')}>
              Cancelar
            </Button>
          )}
        </div>
      </div>

      <div className="grid grid-cols-1 gap-6 xl:grid-cols-[1.4fr_1fr]">
        <Card>
          <CardHeader>
            <CardTitle>Pedidos anticipados</CardTitle>
            <span className="text-xs text-muted-foreground">
              {pedidos.data?.length ?? 0} {pedidos.data?.length === 1 ? 'pedido' : 'pedidos'} · Total <span className="num">{formatUSD(totalPedidos)}</span>
            </span>
          </CardHeader>
          <CardBody className="flex flex-col gap-2">
            {pedidos.isLoading ? (
              <Skeleton className="h-24 w-full" />
            ) : (pedidos.data?.length ?? 0) === 0 ? (
              <p className="text-sm text-muted-foreground">Sin pedidos todavía. Agrégalos desde el panel lateral.</p>
            ) : (
              pedidos.data?.map((pedido: PedidoAnticipado) => (
                <div key={pedido.id} className="flex items-center justify-between gap-2 rounded-xl bg-muted/40 px-3 py-2">
                  <div className="min-w-0">
                    <p className="truncate text-sm text-foreground">{pedido.cantidad} × {pedido.nombre}</p>
                    <p className="text-xs text-muted-foreground">{pedido.sku}</p>
                  </div>
                  <div className="flex items-center gap-2">
                    <span className="num text-foreground">{formatUSD(pedido.subtotalUSD)}</span>
                    <Button size="sm" variant="ghost" loading={quitarPedido.isPending} onClick={() => quitarPedido.mutate(pedido.id)}>
                      Quitar
                    </Button>
                  </div>
                </div>
              ))
            )}
          </CardBody>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Agregar pedido</CardTitle>
            <span className="text-xs text-muted-foreground">Clic en un producto lo agrega al pedido.</span>
          </CardHeader>
          <CardBody>
            <PanelAgregarPedido reservaId={id} onEnviado={invalidar} />
          </CardBody>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Señas</CardTitle>
          {datos.pagos.length > 0 && <Pill tone="accent">{datos.pagos.length} pago(s)</Pill>}
        </CardHeader>
        <CardBody className="flex flex-col gap-2">
          {datos.pagos.length === 0 && <p className="text-sm text-muted-foreground">Sin señas registradas.</p>}
          {datos.pagos.map((pago) => (
            <div key={pago.id} className="flex items-center justify-between gap-3 rounded-lg bg-muted/40 px-3 py-2">
              <div>
                <p className="text-sm text-foreground">{pago.metodoPago}</p>
                <p className="text-xs text-muted-foreground">{formatUSD(pago.monto)}</p>
              </div>
              <div className="flex items-center gap-2">
                <StatusBadge status={pago.estado} />
                {pago.estado === 'Pendiente' && (
                  <>
                    <Button size="sm" loading={validarPago.isPending} onClick={() => validarPago.mutate({ pagoId: pago.id, aprobar: true })}>
                      Validar
                    </Button>
                    <Button size="sm" variant="ghost" loading={validarPago.isPending} onClick={() => validarPago.mutate({ pagoId: pago.id, aprobar: false })}>
                      Rechazar
                    </Button>
                  </>
                )}
              </div>
            </div>
          ))}
        </CardBody>
      </Card>
    </div>
  );
}