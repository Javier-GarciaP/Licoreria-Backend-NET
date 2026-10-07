import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Map as MapIcon, Plus, Trash2 } from 'lucide-react';
import { Button, Input, Modal, Skeleton } from '@licoreria/ui';
import type { Plano } from '@licoreria/types';
import { clubApi } from '@licoreria/api-client';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { mensajeDeError } from '../lib/api';

export function PlanosPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [creando, setCreando] = useState(false);
  const [porEliminar, setPorEliminar] = useState<Plano | null>(null);

  const planos = useQuery({ queryKey: ['planos'], queryFn: clubApi.planos });

  const invalidar = () => queryClient.invalidateQueries({ queryKey: ['planos'] });

  const crear = useMutation({
    mutationFn: (nombre: string) => clubApi.crearPlano({ nombre, elementos: [] }),
    onSuccess: (plano) => {
      toast.success('Mapa creado');
      setCreando(false);
      invalidar();
      navigate(`/salon/planos/${plano.id}`);
    },
    onError: (error) => toast.error('No se pudo crear', { description: mensajeDeError(error) }),
  });

  const eliminar = useMutation({
    mutationFn: (id: string) => clubApi.eliminarPlano(id),
    onSuccess: () => {
      toast.success('Mapa eliminado');
      setPorEliminar(null);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo eliminar', { description: mensajeDeError(error) }),
  });

  return (
    <div className="flex flex-col gap-4">
      <div className="flex items-center justify-between gap-3">
        <p className="text-sm text-muted">
          {planos.data?.length ?? 0} {planos.data?.length === 1 ? 'mapa' : 'mapas'} del local
        </p>
        <Button onClick={() => setCreando(true)}>
          <Plus size={16} /> Nuevo plano
        </Button>
      </div>

      {planos.isLoading ? (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {Array.from({ length: 3 }).map((_, i) => (
            <Skeleton key={i} className="h-32 w-full" />
          ))}
        </div>
      ) : (planos.data?.length ?? 0) === 0 ? (
        <div className="flex flex-col items-center gap-3 rounded-card border border-dashed border-hairline px-6 py-14 text-center">
          <span className="flex h-12 w-12 items-center justify-center rounded-full bg-accent/20 text-accent-ink">
            <MapIcon size={22} />
          </span>
          <p className="text-sm text-ink">Aún no hay mapas</p>
          <p className="max-w-sm text-xs text-muted">Crea el primer plano del local para distribuir mesas, barra, pista y zonas.</p>
          <Button onClick={() => setCreando(true)}>
            <Plus size={16} /> Nuevo plano
          </Button>
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {(planos.data ?? []).map((plano) => (
            <button
              key={plano.id}
              type="button"
              onClick={() => navigate(`/salon/planos/${plano.id}`)}
              className="group relative flex flex-col items-start gap-3 rounded-card border border-hairline bg-surface/60 p-4 text-left transition hover:border-accent hover:bg-accent/10"
            >
              <div className="flex w-full items-start justify-between gap-2">
                <span className="flex h-9 w-9 items-center justify-center rounded-full bg-accent/20 text-accent-ink">
                  <MapIcon size={16} />
                </span>
                <span
                  role="button"
                  tabIndex={0}
                  onClick={(evento) => {
                    evento.stopPropagation();
                    setPorEliminar(plano);
                  }}
                  onKeyDown={(evento) => {
                    if (evento.key === 'Enter') {
                      evento.stopPropagation();
                      setPorEliminar(plano);
                    }
                  }}
                  className="rounded-full p-1.5 text-muted opacity-0 transition group-hover:opacity-100 hover:text-danger-ink"
                  aria-label="Eliminar mapa"
                >
                  <Trash2 size={15} />
                </span>
              </div>
              <div>
                <p className="text-sm font-medium text-ink">{plano.nombre}</p>
                <p className="text-xs text-muted">
                  v{plano.version} · {plano.elementos.length} elementos
                </p>
              </div>
              {plano.activo && <span className="text-[11px] text-success-ink">Activo</span>}
            </button>
          ))}
        </div>
      )}

      <ModalCrearPlano open={creando} onClose={() => setCreando(false)} onCrear={(v) => crear.mutate(v)} cargando={crear.isPending} />
      <ConfirmDialog
        open={Boolean(porEliminar)}
        title="Eliminar mapa"
        description={`¿Seguro que deseas eliminar "${porEliminar?.nombre ?? ''}"?`}
        confirmLabel="Eliminar"
        loading={eliminar.isPending}
        onClose={() => setPorEliminar(null)}
        onConfirm={() => porEliminar && eliminar.mutate(porEliminar.id)}
      />
    </div>
  );
}

function ModalCrearPlano({
  open,
  onClose,
  onCrear,
  cargando,
}: {
  open: boolean;
  onClose: () => void;
  onCrear: (nombre: string) => void;
  cargando: boolean;
}) {
  const [valor, setValor] = useState('');
  return (
    <Modal
      open={open}
      onClose={onClose}
      title="Nuevo plano"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>
            Cancelar
          </Button>
          <Button disabled={valor.trim().length < 2} loading={cargando} onClick={() => onCrear(valor.trim())}>
            Crear y abrir
          </Button>
        </>
      }
    >
      <Input label="Nombre del plano" value={valor} onChange={(evento) => setValor(evento.target.value)} />
    </Modal>
  );
}
