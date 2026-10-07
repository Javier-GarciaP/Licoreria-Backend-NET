import { useEffect, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Button, Card, CardBody, CardHeader, CardTitle, PageHeader, Pill, Skeleton, StatusBadge } from '@licoreria/ui';
import { cuentasApi } from '@licoreria/api-client';
import { mensajeDeError } from '../lib/api';
import { minutosTranscurridos } from '../lib/format';
import { useRealtime } from '../hooks/useRealtime';

interface ItemKds {
  cuentaId: string;
  mesa: string;
  comandaId: string;
  detalleId: string;
  nombre: string;
  cantidad: number;
  estado: string;
  fecha: string;
}

export function KdsPage() {
  const queryClient = useQueryClient();
  const [tick, setTick] = useState(0);

  useEffect(() => {
    const intervalo = setInterval(() => setTick((valor) => valor + 1), 30_000);
    return () => clearInterval(intervalo);
  }, []);

  useRealtime('barra', {
    'comanda:creada': () => queryClient.invalidateQueries({ queryKey: ['kds'] }),
    'comanda:actualizada': () => queryClient.invalidateQueries({ queryKey: ['kds'] }),
    'item:actualizado': () => queryClient.invalidateQueries({ queryKey: ['kds'] }),
  });

  const cuentas = useQuery({
    queryKey: ['kds', 'cuentas'],
    queryFn: () => cuentasApi.listar({ estado: 'Abierta', pageSize: 100 }),
    refetchInterval: 60_000,
  });

  const marcar = useMutation({
    mutationFn: (item: ItemKds) => cuentasApi.cambiarEstadoItem(item.cuentaId, item.comandaId, item.detalleId, 'Preparado'),
    onSuccess: () => {
      toast.success('Marcado como preparado');
      queryClient.invalidateQueries({ queryKey: ['kds'] });
    },
    onError: (error) => toast.error('No se pudo marcar', { description: mensajeDeError(error) }),
  });

  const items: ItemKds[] =
    cuentas.data?.items.flatMap((cuenta) =>
      cuenta.comandas
        .filter((comanda) => comanda.area === 'Barra')
        .flatMap((comanda) =>
          comanda.detalles
            .filter((detalle) => detalle.estado === 'Recibido' || detalle.estado === 'Preparado')
            .map((detalle) => ({
              cuentaId: cuenta.id,
              mesa: cuenta.nombreMesa,
              comandaId: comanda.id,
              detalleId: detalle.id,
              nombre: detalle.nombre,
              cantidad: detalle.cantidad,
              estado: detalle.estado,
              fecha: comanda.fecha,
            })),
        ),
    ) ?? [];

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader
        title="KDS · Barra"
        subtitle="Comandas pendientes en tiempo real."
        actions={<Pill tone="accent">{items.length} pendientes</Pill>}
      />

      {cuentas.isLoading ? (
        <Skeleton className="h-80 w-full" />
      ) : items.length === 0 ? (
        <Card>
          <CardBody>
            <p className="py-16 text-center text-sm text-muted">Sin comandas pendientes. Todo al día.</p>
          </CardBody>
        </Card>
      ) : (
        <div data-tick={tick} className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
          {items.map((item) => {
            const minutos = minutosTranscurridos(item.fecha);
            const urgente = minutos >= 10;
            return (
              <Card key={item.detalleId} className={urgente ? 'ring-1 ring-danger/60' : undefined}>
                <CardHeader>
                  <CardTitle>Mesa {item.mesa}</CardTitle>
                  <StatusBadge status={item.estado} />
                </CardHeader>
                <CardBody className="flex flex-col gap-4">
                  <p className="text-lg font-medium text-ink">
                    {item.cantidad} × {item.nombre}
                  </p>
                  <div className="flex items-center justify-between">
                    <span className={urgente ? 'text-sm text-danger' : 'text-sm text-muted'}>
                      {minutos < 1 ? 'recién recibido' : `${minutos} min`}
                    </span>
                    <Button size="sm" loading={marcar.isPending} onClick={() => marcar.mutate(item)}>
                      Marcar preparado
                    </Button>
                  </div>
                </CardBody>
              </Card>
            );
          })}
        </div>
      )}
    </div>
  );
}
