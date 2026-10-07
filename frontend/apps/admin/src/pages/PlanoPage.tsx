import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { Button, Card, CardBody, CardHeader, CardTitle, Modal, PageHeader } from '@licoreria/ui';
import type { Mesa } from '@licoreria/types';
import { clubApi, cuentasApi } from '@licoreria/api-client';
import { mensajeDeError } from '../lib/api';

const ESCALA = 96;
const LEYENDA = [
  { estado: 'Libre', color: 'rgb(var(--color-success))' },
  { estado: 'Ocupada', color: 'rgb(var(--color-danger))' },
  { estado: 'Reservada', color: 'rgb(var(--color-warning))' },
  { estado: 'En limpieza', color: 'rgb(var(--color-info))' },
];

function estadoDeMesa(mesa: Mesa): string {
  if (!mesa.activa) return 'En limpieza';
  if (mesa.cuentaId) return 'Ocupada';
  if (mesa.reservada) return 'Reservada';
  return 'Libre';
}

function colorDeMesa(estado: string): string {
  if (estado === 'Ocupada') return 'rgb(var(--color-danger))';
  if (estado === 'Reservada') return 'rgb(var(--color-warning))';
  if (estado === 'En limpieza') return 'rgb(var(--color-info))';
  return 'rgb(var(--color-success))';
}

export function PlanoPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [seleccionada, setSeleccionada] = useState<Mesa | null>(null);
  const [mesaAbrir, setMesaAbrir] = useState<Mesa | null>(null);

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
  const anchoMax = Math.max(4, ...lista.map((mesa) => mesa.posX + mesa.ancho)) * ESCALA;
  const altoMax = Math.max(3, ...lista.map((mesa) => mesa.posY + mesa.alto)) * ESCALA;

  const manejarClick = (mesa: Mesa) => {
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
        title="Plano de mesas"
        subtitle="Mapa interactivo del local. Los cambios se sincronizan en tiempo real."
        actions={
          <div className="flex flex-wrap gap-2">
            {LEYENDA.map((item) => (
              <span key={item.estado} className="inline-flex items-center gap-2 text-xs text-muted">
                <span className="h-3 w-3 rounded-full" style={{ backgroundColor: item.color }} />
                {item.estado}
              </span>
            ))}
          </div>
        }
      />

      <Card>
        <CardHeader>
          <CardTitle>Salón</CardTitle>
          <span className="text-xs text-muted">{lista.length} mesas</span>
        </CardHeader>
        <CardBody>
          <div className="overflow-x-auto">
            <svg width={anchoMax} height={altoMax} className="min-w-full">
              <rect x={0} y={0} width={anchoMax} height={altoMax} rx={24} fill="rgb(var(--color-elevated))" opacity={0.4} />
              {lista.map((mesa) => {
                const estado = estadoDeMesa(mesa);
                return (
                  <g
                    key={mesa.id}
                    transform={`translate(${mesa.posX * ESCALA}, ${mesa.posY * ESCALA})`}
                    onClick={() => manejarClick(mesa)}
                    className="cursor-pointer"
                  >
                    <rect
                      width={mesa.ancho * ESCALA - 12}
                      height={mesa.alto * ESCALA - 12}
                      rx={20}
                      fill={colorDeMesa(estado)}
                      opacity={0.18}
                      stroke={colorDeMesa(estado)}
                      strokeWidth={2}
                    />
                    <text x="50%" y="44%" textAnchor="middle" fill="rgb(var(--color-ink))" fontSize={16} fontWeight={600}>
                      {mesa.numero}
                    </text>
                    <text x="50%" y="64%" textAnchor="middle" fill="rgb(var(--color-muted))" fontSize={10}>
                      {mesa.capacidad} pers · {estado}
                    </text>
                  </g>
                );
              })}
            </svg>
          </div>
          {lista.length === 0 && <p className="py-10 text-center text-sm text-muted">No hay mesas configuradas.</p>}
        </CardBody>
      </Card>

      {seleccionada && (
        <div className="fixed bottom-24 left-1/2 z-40 -translate-x-1/2 lg:bottom-8">
          <Card className="flex items-center gap-4 p-3">
            <span className="px-2 text-sm text-ink">Mesa {seleccionada.numero}</span>
            {estadoDeMesa(seleccionada) === 'Ocupada' && (
              <Button
                size="sm"
                variant="danger"
                loading={desalojar.isPending}
                onClick={() => desalojar.mutate(seleccionada.id)}
              >
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
          Se abrirá una cuenta para la mesa {mesaAbrir?.numero} ({mesaAbrir?.zonaNombre}) y quedará marcada como
          <span className="text-danger"> Ocupada</span> para todo el personal.
        </p>
      </Modal>
    </div>
  );
}
