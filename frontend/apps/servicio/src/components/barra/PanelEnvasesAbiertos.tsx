import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Button, Modal, cn } from '@licoreria/ui';
import { catalogoApi, envasesApi } from '@licoreria/api-client';
import type { EnvaseAbierto, Producto, ProductoVariante } from '@licoreria/types';
import { avisarError } from '../../lib/api';
import { minutosTranscurridos } from '../../lib/format';

/**
 * Panel de barra para controlar las botellas destapadas: lista los envases abiertos con sus porciones
 * restantes, permite destapar una presentación a elección del barman y descartar un envase con motivo.
 */
export function PanelEnvasesAbiertos({ abierto, onCerrar }: { abierto: boolean; onCerrar: () => void }) {
  const queryClient = useQueryClient();
  const [porcionElegida, setPorcionElegida] = useState<{ producto: Producto; variante: ProductoVariante } | null>(null);
  const [descartando, setDescartando] = useState<EnvaseAbierto | null>(null);
  const [motivo, setMotivo] = useState('');

  const envases = useQuery({ queryKey: ['envases', 'abiertos'], queryFn: () => envasesApi.listar(), enabled: abierto });
  const catalogo = useQuery({
    queryKey: ['envases', 'porciones'],
    queryFn: () => catalogoApi.productos({ activo: true, pageSize: 100 }),
    enabled: abierto,
  });

  const porciones = useMemo(
    () =>
      (catalogo.data?.items ?? [])
        .filter((producto) => producto.areaDestino === 'Barra')
        .flatMap((producto) =>
          producto.variantes
            .filter((variante) => variante.activo && variante.naturaleza === 'Porcion')
            .map((variante) => ({ producto, variante })),
        ),
    [catalogo.data],
  );

  const refrescar = () => {
    queryClient.invalidateQueries({ queryKey: ['envases'] });
    queryClient.invalidateQueries({ queryKey: ['carta-productos'] });
  };

  const destapar = useMutation({
    mutationFn: (body: { porcionVarianteId: string; presentacionVarianteId: string }) => envasesApi.destapar(body),
    onSuccess: (envase) => {
      toast.success(`Destapada ${envase.presentacionNombre}`, {
        description: `${envase.porcionesIniciales} porciones de ${envase.porcionNombre}.`,
      });
      setPorcionElegida(null);
      refrescar();
    },
    onError: (error) => avisarError('No se pudo destapar', error, refrescar),
  });

  const descartar = useMutation({
    mutationFn: ({ id, motivo: texto }: { id: string; motivo: string }) => envasesApi.descartar(id, texto),
    onSuccess: () => {
      toast.success('Envase descartado');
      setDescartando(null);
      setMotivo('');
      refrescar();
    },
    onError: (error) => avisarError('No se pudo descartar', error, refrescar),
  });

  return (
    <Modal open={abierto} onClose={onCerrar} title="Envases abiertos" className="max-w-2xl">
      <div className="flex flex-col gap-5">
        <section className="flex flex-col gap-2" aria-label="Envases abiertos">
          <p className="text-xs font-medium uppercase tracking-tighter2 text-muted-foreground">Abiertos ahora</p>
          {envases.isLoading ? (
            <p className="text-sm text-muted-foreground">Cargando…</p>
          ) : (envases.data ?? []).length === 0 ? (
            <p className="rounded-inner border border-dashed border-border py-4 text-center text-sm text-muted-foreground">
              No hay botellas destapadas.
            </p>
          ) : (
            <ul className="flex flex-col gap-2">
              {envases.data?.map((envase) => (
                <li key={envase.id} className="rounded-inner border border-border bg-card p-3">
                  <div className="flex items-start justify-between gap-3">
                    <div className="min-w-0">
                      <p className="text-sm font-medium text-foreground">{envase.porcionNombre}</p>
                      <p className="text-[11px] text-muted-foreground">
                        {envase.presentacionNombre} · abierta hace {minutosTranscurridos(envase.abiertoEn)} min
                      </p>
                    </div>
                    <div className="flex items-center gap-2">
                      <span className="num rounded-full bg-muted/60 px-2 py-0.5 text-xs text-foreground">
                        {envase.porcionesRestantes}/{envase.porcionesIniciales}
                      </span>
                      <Button size="sm" variant="ghost" onClick={() => setDescartando(envase)}>
                        Descartar
                      </Button>
                    </div>
                  </div>
                  {descartando?.id === envase.id && (
                    <form
                      className="mt-2 flex items-end gap-2"
                      onSubmit={(evento) => {
                        evento.preventDefault();
                        if (motivo.trim()) descartar.mutate({ id: envase.id, motivo: motivo.trim() });
                      }}
                    >
                      <label className="flex flex-1 flex-col gap-1 text-xs text-muted-foreground">
                        Motivo del descarte
                        <input
                          autoFocus
                          value={motivo}
                          onChange={(evento) => setMotivo(evento.target.value)}
                          placeholder="Ej. se derramó, se venció…"
                          className="h-9 rounded-control border border-border bg-card px-3 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-ring/50"
                        />
                      </label>
                      <Button type="submit" size="sm" disabled={!motivo.trim()} loading={descartar.isPending}>
                        Confirmar descarte
                      </Button>
                      <Button type="button" size="sm" variant="ghost" onClick={() => setDescartando(null)}>
                        Cancelar
                      </Button>
                    </form>
                  )}
                </li>
              ))}
            </ul>
          )}
        </section>

        <section className="flex flex-col gap-2" aria-label="Destapar">
          <p className="text-xs font-medium uppercase tracking-tighter2 text-muted-foreground">Destapar una botella</p>
          {porciones.length === 0 ? (
            <p className="text-sm text-muted-foreground">No hay porciones definidas en el catálogo.</p>
          ) : (
            <ul className="flex flex-col gap-2">
              {porciones.map(({ producto, variante }) => (
                <li key={variante.id} className="rounded-inner border border-border bg-card p-3">
                  <div className="flex items-center justify-between gap-3">
                    <div className="min-w-0">
                      <p className="text-sm font-medium text-foreground">
                        {producto.nombre} · {variante.nombre}
                      </p>
                      <p className={cn('text-[11px]', variante.porcionesDisponibles > 0 ? 'text-muted-foreground' : 'text-destructive-fg')}>
                        {variante.porcionesDisponibles} porciones abiertas · {variante.envasesCerradosAbribles} cerradas para destapar
                      </p>
                    </div>
                    <Button
                      size="sm"
                      disabled={variante.envasesCerradosAbribles <= 0}
                      onClick={() => setPorcionElegida({ producto, variante })}
                    >
                      Destapar
                    </Button>
                  </div>
                  {porcionElegida?.variante.id === variante.id && (
                    <OpcionesDestape
                      producto={producto}
                      porcion={variante}
                      pendiente={destapar.isPending}
                      onElegir={(presentacionVarianteId) =>
                        destapar.mutate({ porcionVarianteId: variante.id, presentacionVarianteId })
                      }
                      onCancelar={() => setPorcionElegida(null)}
                    />
                  )}
                </li>
              ))}
            </ul>
          )}
        </section>
      </div>
    </Modal>
  );
}

/** Presentaciones de origen de una porción, con su existencia, para que el barman elija cuál destapar. */
function OpcionesDestape({
  producto,
  porcion,
  pendiente,
  onElegir,
  onCancelar,
}: {
  producto: Producto;
  porcion: ProductoVariante;
  pendiente: boolean;
  onElegir: (presentacionVarianteId: string) => void;
  onCancelar: () => void;
}) {
  const origenes = useQuery({ queryKey: ['envases', 'origenes', porcion.id], queryFn: () => catalogoApi.origenes(porcion.id) });
  const existencia = (id: string) => {
    const variante = producto.variantes.find((v) => v.id === id);
    return variante ? Math.max(0, variante.cantidad - variante.cantidadReservada) : 0;
  };

  return (
    <div className="mt-2 flex flex-col gap-1.5 border-t border-border pt-2" role="group" aria-label={`Elegir botella para ${porcion.nombre}`}>
      <p className="text-xs text-muted-foreground">¿Cuál destapas?</p>
      {(origenes.data ?? []).map((origen) => {
        const disponible = existencia(origen.presentacionVarianteId);
        return (
          <button
            key={origen.id}
            type="button"
            disabled={disponible < 1 || pendiente}
            onClick={() => onElegir(origen.presentacionVarianteId)}
            className={cn(
              'flex items-center justify-between gap-3 rounded-inner border border-border px-3 py-2 text-left text-sm transition',
              disponible < 1 ? 'cursor-not-allowed opacity-50' : 'hover:border-primary hover:bg-primary/10',
            )}
          >
            <span className="text-foreground">{origen.presentacionNombre}</span>
            <span className="num text-[11px] text-muted-foreground">
              da {origen.porcionesPorUnidad} · {disponible < 1 ? 'Agotado' : `Quedan ${disponible}`}
            </span>
          </button>
        );
      })}
      <Button type="button" size="sm" variant="ghost" className="self-start" onClick={onCancelar}>
        Cancelar
      </Button>
    </div>
  );
}
