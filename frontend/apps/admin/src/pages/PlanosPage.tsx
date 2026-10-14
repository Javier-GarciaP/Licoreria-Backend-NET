import { useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Copy, LayoutTemplate, Map as MapIcon, Plus, Star, Trash2 } from 'lucide-react';
import { BubbleModal, Button, Input, Modal, Skeleton } from '@licoreria/ui';
import type { Plano } from '@licoreria/types';
import { clubApi } from '@licoreria/api-client';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { MapaView, type EstadoMesaPlano } from '../components/mapa/MapaView';
import { useAuth } from '../context/AuthContext';
import { mensajeDeError } from '../lib/api';
import { estadoDeMesa, planoAPayload, planoVisible } from '../lib/plano';

type AccionPlano = { tipo: 'duplicar' | 'plantilla' | 'desdePlantilla'; plano: Plano };

const TEXTOS_ACCION: Record<AccionPlano['tipo'], { titulo: string; boton: string; exito: string }> = {
  duplicar: { titulo: 'Duplicar plano', boton: 'Duplicar', exito: 'Plano duplicado (queda inactivo)' },
  plantilla: { titulo: 'Guardar como plantilla', boton: 'Guardar plantilla', exito: 'Plantilla guardada' },
  desdePlantilla: { titulo: 'Nuevo desde plantilla', boton: 'Crear plano', exito: 'Plano creado desde la plantilla (queda inactivo)' },
};

function nombreSugerido({ tipo, plano }: AccionPlano): string {
  if (tipo === 'duplicar') return `${plano.nombre} (copia)`;
  if (tipo === 'plantilla') return `${plano.nombre} (plantilla)`;
  return '';
}

/** Consulta de solo lectura para quien no edita la distribución: muestra el plano activo. */
function PlanoActivoSoloLectura() {
  const planos = useQuery({ queryKey: ['planos'], queryFn: () => clubApi.planos() });
  const zonas = useQuery({ queryKey: ['zonas'], queryFn: clubApi.zonas });
  const mesas = useQuery({ queryKey: ['mesas'], queryFn: () => clubApi.mesas() });

  if (planos.isLoading) return <Skeleton className="h-64 w-full" />;
  const listaMesas = mesas.data ?? [];
  const plano = planoVisible(planos.data ?? [], listaMesas);
  const estadoPorMesa = Object.fromEntries(listaMesas.map((m) => [m.id, estadoDeMesa(m)])) as Record<string, EstadoMesaPlano>;

  return (
    <div className="flex flex-col gap-3">
      <p className="text-sm text-muted-foreground">
        Plano activo: <span className="font-medium text-foreground">{plano.nombre}</span> · solo lectura
      </p>
      <div className="app-scroll overflow-auto rounded-lg border border-border bg-muted/30 p-4">
        <MapaView plano={plano} zonas={zonas.data ?? []} modo="operacion" estadoPorMesa={estadoPorMesa} />
      </div>
    </div>
  );
}

export function PlanosPage() {
  const { esAdmin, tienePermiso } = useAuth() as { esAdmin: boolean; tienePermiso: (clave: string) => boolean };
  if (!esAdmin && !tienePermiso('club:layout')) return <PlanoActivoSoloLectura />;
  return <GestionPlanos />;
}

function GestionPlanos() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [creando, setCreando] = useState(false);
  const [porEliminar, setPorEliminar] = useState<Plano | null>(null);
  const [nombreNuevo, setNombreNuevo] = useState('');
  const [accion, setAccion] = useState<AccionPlano | null>(null);
  const [nombreAccion, setNombreAccion] = useState('');
  const botonNuevoRef = useRef<HTMLSpanElement>(null);

  // Con el permiso de distribución el listado incluye las plantillas.
  const planos = useQuery({ queryKey: ['planos', 'todos'], queryFn: () => clubApi.planos(true) });
  const normales = (planos.data ?? []).filter((p) => !p.esPlantilla);
  const plantillas = (planos.data ?? []).filter((p) => p.esPlantilla);

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

  const duplicar = useMutation({
    mutationFn: ({ tipo, plano }: AccionPlano) =>
      clubApi.duplicarPlano(plano.id, {
        nombre: nombreAccion.trim() || undefined,
        comoPlantilla: tipo === 'plantilla',
      }),
    onSuccess: (_copia, { tipo }) => {
      toast.success(TEXTOS_ACCION[tipo].exito);
      setAccion(null);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo completar', { description: mensajeDeError(error) }),
  });

  const abrirAccion = (nueva: AccionPlano) => {
    setNombreAccion(nombreSugerido(nueva));
    setAccion(nueva);
  };

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
          {normales.length} {normales.length === 1 ? 'mapa' : 'mapas'} del local · {plantillas.length} {plantillas.length === 1 ? 'plantilla' : 'plantillas'}
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
      ) : normales.length === 0 ? (
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
          {normales.map((plano) => (
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
              <div className="flex gap-1">
                <AccionTarjeta etiqueta="Duplicar" onClick={() => abrirAccion({ tipo: 'duplicar', plano })}>
                  <Copy size={12} /> Duplicar
                </AccionTarjeta>
                <AccionTarjeta etiqueta="Guardar como plantilla" onClick={() => abrirAccion({ tipo: 'plantilla', plano })}>
                  <LayoutTemplate size={12} /> Guardar como plantilla
                </AccionTarjeta>
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

      {plantillas.length > 0 && (
        <section aria-label="Plantillas" className="flex flex-col gap-3">
          <h2 className="text-sm font-medium text-foreground">Plantillas</h2>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-3">
            {plantillas.map((plantilla) => (
              <div key={plantilla.id} className="flex flex-col gap-3 rounded-xl border border-dashed border-border bg-card/40 p-4">
                <div>
                  <p className="text-sm font-medium text-foreground">{plantilla.nombre}</p>
                  <p className="text-xs text-muted-foreground">{plantilla.elementos.length} elementos · sin mesas enlazadas</p>
                </div>
                <div className="flex flex-wrap gap-2">
                  <Button size="sm" onClick={() => abrirAccion({ tipo: 'desdePlantilla', plano: plantilla })}>
                    Nuevo desde plantilla
                  </Button>
                  <Button size="sm" variant="ghost" aria-label={`Eliminar plantilla ${plantilla.nombre}`} onClick={() => setPorEliminar(plantilla)}>
                    <Trash2 size={14} />
                  </Button>
                </div>
              </div>
            ))}
          </div>
        </section>
      )}

      <Modal
        open={Boolean(accion)}
        onClose={() => setAccion(null)}
        title={accion ? TEXTOS_ACCION[accion.tipo].titulo : ''}
        size="sm"
        footer={
          <>
            <Button variant="ghost" size="sm" onClick={() => setAccion(null)}>
              Cancelar
            </Button>
            <Button size="sm" loading={duplicar.isPending} onClick={() => accion && duplicar.mutate(accion)}>
              {accion ? TEXTOS_ACCION[accion.tipo].boton : ''}
            </Button>
          </>
        }
      >
        <Input label="Nombre" value={nombreAccion} autoFocus onChange={(evento) => setNombreAccion(evento.target.value)} />
        <p className="mt-2 text-xs text-muted-foreground">
          La copia siempre queda inactiva: no cambia el plano activo.
          {accion?.tipo === 'plantilla' && ' Las mesas enlazadas se desvinculan en la plantilla.'}
        </p>
      </Modal>

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

function AccionTarjeta({ etiqueta, onClick, children }: { etiqueta: string; onClick: () => void; children: React.ReactNode }) {
  return (
    <span
      role="button"
      tabIndex={0}
      aria-label={etiqueta}
      onClick={(evento) => {
        evento.stopPropagation();
        onClick();
      }}
      onKeyDown={(evento) => {
        if (evento.key === 'Enter') {
          evento.stopPropagation();
          onClick();
        }
      }}
      className="inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-[11px] text-muted-foreground transition hover:bg-primary/15 hover:text-foreground"
    >
      {children}
    </span>
  );
}
