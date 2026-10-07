import { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Button, Card, CardBody, CardHeader, CardTitle, cn, PageHeader, Pill, Skeleton, StatusBadge } from '@licoreria/ui';
import { cuentasApi } from '@licoreria/api-client';
import { mensajeDeError } from '../lib/api';
import { minutosTranscurridos } from '../lib/format';
import { useAuth } from '../context/AuthContext';
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
  const [params, setParams] = useSearchParams();
  const { rolDominio } = useAuth() as { rolDominio?: string };

  const areaParam = params.get('area');
  const area = areaParam === 'cocina' || areaParam === 'barra' ? areaParam : rolDominio === 'Cocina' ? 'cocina' : 'barra';
  const areaApi = area === 'cocina' ? 'Cocina' : 'Barra';

  useEffect(() => {
    const intervalo = setInterval(() => setTick((valor) => valor + 1), 30_000);
    return () => clearInterval(intervalo);
  }, []);

  useRealtime(area, {
    'comanda:creada': () => queryClient.invalidateQueries({ queryKey: ['kds'] }),
    'comanda:actualizada': () => queryClient.invalidateQueries({ queryKey: ['kds'] }),
    'item:actualizado': () => queryClient.invalidateQueries({ queryKey: ['kds'] }),
  });

  const cuentas = useQuery({
    queryKey: ['kds', 'cuentas', area],
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
        .filter((comanda) => comanda.area === areaApi)
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

  const cambiarArea = (nueva: 'barra' | 'cocina') => {
    setParams(nueva === 'barra' ? {} : { area: 'cocina' }, { replace: true });
  };

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader
        title={`KDS · ${area === 'cocina' ? 'Cocina' : 'Barra'}`}
        subtitle="Comandas pendientes en tiempo real."
        actions={
          <div className="flex items-center gap-2">
            <div className="flex rounded-pill border border-hairline p-0.5">
              {(['barra', 'cocina'] as const).map((valor) => (
                <button
                  key={valor}
                  type="button"
                  onClick={() => cambiarArea(valor)}
                  className={cn(
                    'rounded-pill px-3 py-1 text-xs capitalize transition',
                    area === valor ? 'bg-accent/20 font-medium text-accent-ink' : 'text-muted hover:text-ink',
                  )}
                >
                  {valor}
                </button>
              ))}
            </div>
            <Pill tone="accent">{items.length} pendientes</Pill>
          </div>
        }
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
                    <span className={urgente ? 'text-sm text-danger-ink' : 'text-sm text-muted'}>
                      {minutos < 1 ? 'recién recibido' : <span className="num">{minutos} min</span>}
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
