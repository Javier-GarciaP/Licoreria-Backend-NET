import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { Button, Card, CardBody, CardHeader, CardTitle, Modal, PageHeader, Skeleton } from '@licoreria/ui';
import type { Mesa, Plano, PlanoElemento } from '@licoreria/types';
import { clubApi, cuentasApi } from '@licoreria/api-client';
import { MapaView, type EstadoMesaPlano } from '../components/mapa/MapaView';
import { mensajeDeError } from '../lib/api';
import { estadoDeMesa, planoVisible } from '../lib/plano';

const LEYENDA: { estado: EstadoMesaPlano; label: string; color: string }[] = [
  { estado: 'Libre', label: 'Libre', color: '#2fbf71' },
  { estado: 'Ocupada', label: 'Ocupada', color: '#ef4444' },
  { estado: 'Reservada', label: 'Reservada', color: '#f5a524' },
  { estado: 'EnLimpieza', label: 'En limpieza', color: '#3b82f6' },
];

export function PlanoPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [seleccionada, setSeleccionada] = useState<Mesa | null>(null);
  const [mesaAbrir, setMesaAbrir] = useState<Mesa | null>(null);

  const planos = useQuery({ queryKey: ['planos'], queryFn: clubApi.planos });
  const zonas = useQuery({ queryKey: ['zonas'], queryFn: clubApi.zonas });
  const mesas = useQuery({ queryKey: ['mesas'], queryFn: () => clubApi.mesas() });

  const abrir = useMutation({
    mutationFn: (mesa: Mesa) => cuentasApi.abrir({ nombreMesa: mesa.numero, mesaId: mesa.id }),
    onSuccess: (cuenta) => {
      toast.success('Mesa abierta');
      queryClient.invalidateQueries({ queryKey: ['mesas'] });
      navigate(`/cuentas/${cuenta.id}`);
    },
    onError: (error) => toast.error('No se pudo abrir la mesa', { description: mensajeDeError(error) }),
  });

  const desalojar = useMutation({
    mutationFn: (mesaId: string) => clubApi.desalojar(mesaId),
    onSuccess: () => {
      toast.success('Mesa desalojada', { description: 'Se notificó en tiempo real a los demás meseros.' });
      queryClient.invalidateQueries({ queryKey: ['mesas'] });
      queryClient.invalidateQueries({ queryKey: ['cuentas'] });
      setSeleccionada(null);
    },
    onError: (error) => toast.error('No se pudo desalojar', { description: mensajeDeError(error) }),
  });

  const lista = mesas.data ?? [];

  const estadoPorMesa = useMemo(
    () => Object.fromEntries(lista.map((m) => [m.id, estadoDeMesa(m)])) as Record<string, EstadoMesaPlano>,
    [lista],
  );
  const numeroPorMesa = useMemo(() => Object.fromEntries(lista.map((m) => [m.id, m.numero])), [lista]);

  const plano = useMemo<Plano>(() => planoVisible(planos.data ?? [], lista), [planos.data, lista]);

  const manejarElemento = (_evento: unknown, elemento: PlanoElemento) => {
    if (!elemento.mesaId) return;
    const mesa = lista.find((m) => m.id === elemento.mesaId);
    if (!mesa) return;
    setSeleccionada(mesa);
    const estado = estadoDeMesa(mesa);
    if (estado === 'Ocupada' && mesa.cuentaId) {
      navigate(`/cuentas/${mesa.cuentaId}`);
    } else if (estado === 'Libre') {
      setMesaAbrir(mesa);
    }
  };

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader
        title="Mapa del salón"
        subtitle="Mapa interactivo del local. Los cambios de estado se sincronizan en tiempo real."
        actions={
          <div className="flex flex-wrap gap-3">
            {LEYENDA.map((item) => (
              <span key={item.estado} className="inline-flex items-center gap-2 text-xs text-muted">
                <span className="h-3 w-3 rounded-full" style={{ backgroundColor: item.color }} />
                {item.label}
              </span>
            ))}
          </div>
        }
      />

      <Card>
        <CardHeader>
          <CardTitle>{plano.nombre}</CardTitle>
          <span className="text-xs text-muted">{lista.length} mesas</span>
        </CardHeader>
        <CardBody>
          {mesas.isLoading || planos.isLoading ? (
            <Skeleton className="h-96 w-full" />
          ) : (
            <div className="overflow-x-auto">
              <MapaView
                plano={plano}
                zonas={zonas.data ?? []}
                modo="operacion"
                estadoPorMesa={estadoPorMesa}
                numeroPorMesa={numeroPorMesa}
                onElementoPointerDown={manejarElemento}
              />
            </div>
          )}
        </CardBody>
      </Card>

      {seleccionada && (
        <div className="fixed bottom-24 left-1/2 z-40 -translate-x-1/2 lg:bottom-8">
          <Card className="flex items-center gap-4 p-3">
            <span className="px-2 text-sm text-ink">Mesa {seleccionada.numero}</span>
            {estadoDeMesa(seleccionada) === 'Ocupada' && (
              <Button size="sm" variant="danger" loading={desalojar.isPending} onClick={() => desalojar.mutate(seleccionada.id)}>
                Desalojar
              </Button>
            )}
            <Button size="sm" variant="ghost" onClick={() => setSeleccionada(null)}>
              Cerrar
            </Button>
          </Card>
        </div>
      )}

      <Modal
        open={Boolean(mesaAbrir)}
        onClose={() => setMesaAbrir(null)}
        title={`Abrir mesa ${mesaAbrir?.numero ?? ''}`}
        footer={
          <>
            <Button variant="ghost" onClick={() => setMesaAbrir(null)}>
              Cancelar
            </Button>
            <Button loading={abrir.isPending} onClick={() => mesaAbrir && abrir.mutate(mesaAbrir)}>
              Abrir cuenta
            </Button>
          </>
        }
      >
        <p className="text-sm text-muted">
          Se abrirá una cuenta para la mesa {mesaAbrir?.numero} y quedará marcada como
          <span className="text-danger-ink"> Ocupada</span> para todo el personal.
        </p>
      </Modal>
    </div>
  );
}
