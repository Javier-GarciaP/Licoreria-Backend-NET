import { useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Map as MapIcon, Plus, Star, Trash2 } from 'lucide-react';
import { BubbleModal, Button, Input, Skeleton } from '@licoreria/ui';
import type { Plano } from '@licoreria/types';
import { clubApi } from '@licoreria/api-client';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { mensajeDeError } from '../lib/api';
import { planoAPayload } from '../lib/plano';

export function PlanosPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [creando, setCreando] = useState(false);
  const [porEliminar, setPorEliminar] = useState<Plano | null>(null);
  const [nombreNuevo, setNombreNuevo] = useState('');
  const botonNuevoRef = useRef<HTMLSpanElement>(null);

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

  const activar = useMutation({
    mutationFn: (plano: Plano) => clubApi.actualizarPlano(plano.id, planoAPayload({ ...plano, activo: true })),
    onSuccess: () => {
      toast.success('Plano establecido como activo');
      invalidar();
    },
    onError: (error) => toast.error('No se pudo activar', { description: mensajeDeError(error) }),
  });

  return (
    <div className="flex flex-col gap-4">
      <div className="flex items-center justify-between gap-3">
        <p className="text-sm text-muted-foreground">
          {planos.data?.length ?? 0} {planos.data?.length === 1 ? 'mapa' : 'mapas'} del local
        </p>
        <span ref={botonNuevoRef}>
          <Button onClick={() => setCreando(true)}>
            <Plus size={16} /> Nuevo plano
          </Button>
        </span>
      </div>

      {planos.isLoading ? (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {Array.from({ length: 3 }).map((_, i) => (
            <Skeleton key={i} className="h-32 w-full" />
          ))}
        </div>
      ) : (planos.data?.length ?? 0) === 0 ? (
        <div className="flex flex-col items-center gap-3 rounded-xl border border-dashed border-border px-6 py-14 text-center">
          <span className="flex h-12 w-12 items-center justify-center rounded-full bg-primary/20 text-foreground">
            <MapIcon size={22} />
          </span>
          <p className="text-sm text-foreground">Aún no hay mapas</p>
          <p className="max-w-sm text-xs text-muted-foreground">Crea el primer plano del local para distribuir mesas, barra, pista y zonas.</p>
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
              className="group relative flex flex-col items-start gap-3 rounded-xl border border-border bg-card/60 p-4 text-left transition hover:border-primary hover:bg-primary/10"
            >
              <div className="flex w-full items-start justify-between gap-2">
                <span className="flex h-9 w-9 items-center justify-center rounded-full bg-primary/20 text-foreground">
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
                  className="rounded-full p-1.5 text-muted-foreground opacity-0 transition group-hover:opacity-100 hover:text-destructive-fg"
                  aria-label="Eliminar mapa"
                >
                  <Trash2 size={15} />
                </span>
              </div>
              <div>
                <p className="text-sm font-medium text-foreground">{plano.nombre}</p>
                <p className="text-xs text-muted-foreground">
                  v{plano.version} · {plano.elementos.length} elementos
                </p>
              </div>
              {plano.activo ? (
                <span className="inline-flex items-center gap-1 text-[11px] text-success-fg">
                  <Star size={12} /> Activo
                </span>
              ) : (
                <span
                  role="button"
                  tabIndex={0}
                  onClick={(evento) => {
                    evento.stopPropagation();
                    activar.mutate(plano);
                  }}
                  onKeyDown={(evento) => {
                    if (evento.key === 'Enter') {
                      evento.stopPropagation();
                      activar.mutate(plano);
                    }
                  }}
                  className="inline-flex items-center gap-1 rounded-full px-2 text-[11px] text-foreground opacity-0 transition group-hover:opacity-100 hover:bg-primary/15"
                  aria-label="Establecer como activo"
                >
                  <Star size={12} /> Establecer como activo
                </span>
              )}
            </button>
          ))}
        </div>
      )}

      <BubbleModal
        open={creando}
        onClose={() => setCreando(false)}
        title="Nuevo plano"
        anchor={botonNuevoRef.current}
      >
        <div className="flex flex-col gap-2">
          <Input
            label="Nombre del plano"
            value={nombreNuevo}
            autoFocus
            onChange={(evento) => setNombreNuevo(evento.target.value)}
            onKeyDown={(evento) => {
              if (evento.key === 'Enter' && nombreNuevo.trim().length >= 2) {
                evento.preventDefault();
                crear.mutate(nombreNuevo.trim());
              }
            }}
          />
        </div>
        <div className="mt-4 flex justify-end gap-2">
          <Button variant="ghost" size="sm" onClick={() => setCreando(false)}>
            Cancelar
          </Button>
          <Button
            size="sm"
            disabled={nombreNuevo.trim().length < 2}
            loading={crear.isPending}
            onClick={() => crear.mutate(nombreNuevo.trim())}
          >
            Crear y abrir
          </Button>
        </div>
      </BubbleModal>
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
