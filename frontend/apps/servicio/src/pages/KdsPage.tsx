import { useEffect, useMemo, useState } from 'react';
import { DndContext, useDraggable, useDroppable, type DragEndEvent } from '@dnd-kit/core';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { ChefHat, Clock3, GripVertical, LogOut, Martini, UserRound } from 'lucide-react';
import { cn } from '@licoreria/ui';
import { cuentasApi } from '@licoreria/api-client';
import { useAuth } from '../context/AuthContext';
import { useRealtime } from '../hooks/useRealtime';
import { Chat } from '../components/Chat';
import { etiquetaRol } from '../lib/roles';
import { formatTime, minutosTranscurridos } from '../lib/format';
import { mensajeDeError } from '../lib/api';

type Area = 'barra' | 'cocina';
type Columna = 'recibido' | 'proceso' | 'terminado';

interface ItemKds {
  id: string;
  cuentaId: string;
  comandaId: string;
  detalleId: string;
  mesa: string;
  nombre: string;
  cantidad: number;
  estado: string;
  fecha: string;
  columna: Columna;
}

const COLUMNAS: { id: Columna; titulo: string; estado: string }[] = [
  { id: 'recibido', titulo: 'Recibido', estado: 'Recibido' },
  { id: 'proceso', titulo: 'En proceso', estado: 'EnProceso' },
  { id: 'terminado', titulo: 'Terminado', estado: 'Preparado' },
];

const ESTADO_A_COLUMNA: Record<string, Columna> = {
  Recibido: 'recibido',
  EnProceso: 'proceso',
  Preparado: 'terminado',
};

function horaActual(): string {
  return new Date().toLocaleTimeString('es-VE', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
}

export function KdsPage() {
  const queryClient = useQueryClient();
  const { usuario, logout, rolDominio } = useAuth();
  const [area, setArea] = useState<Area>(rolDominio === 'Cocina' ? 'cocina' : 'barra');
  const [hora, setHora] = useState(horaActual);

  const areaApi = area === 'cocina' ? 'Cocina' : 'Barra';

  useEffect(() => {
    const intervalo = setInterval(() => setHora(horaActual), 1000);
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
  });

  const items = useMemo<ItemKds[]>(
    () =>
      (cuentas.data?.items ?? []).flatMap((cuenta) =>
        cuenta.comandas
          .filter((comanda) => comanda.area === areaApi)
          .flatMap((comanda) =>
            comanda.detalles
              .filter((detalle) => ESTADO_A_COLUMNA[detalle.estado] != null)
              .map((detalle) => ({
                id: detalle.id,
                cuentaId: cuenta.id,
                comandaId: comanda.id,
                detalleId: detalle.id,
                mesa: cuenta.nombreMesa,
                nombre: detalle.nombre,
                cantidad: detalle.cantidad,
                estado: detalle.estado,
                fecha: comanda.fecha,
                columna: ESTADO_A_COLUMNA[detalle.estado],
              })),
          ),
      ) ?? [],
    [cuentas.data, areaApi],
  );

  const mover = useMutation({
    mutationFn: ({ item, estado }: { item: ItemKds; estado: string }) =>
      cuentasApi.cambiarEstadoItem(item.cuentaId, item.comandaId, item.detalleId, estado),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['kds'] });
    },
    onError: (error) => toast.error('No se pudo mover', { description: mensajeDeError(error) }),
  });

  const alSoltar = (evento: DragEndEvent) => {
    const item = items.find((i) => i.id === evento.active.id);
    if (!item) return;
    const objetivo = COLUMNAS.find((c) => c.id === evento.over?.id);
    if (!objetivo || objetivo.estado === item.estado) return;
    mover.mutate({ item, estado: objetivo.estado });
  };

  return (
    <div className="flex min-h-dvh flex-col bg-canvas p-4 lg:p-6">
      {/* Header */}
      <header className="flex items-center gap-3 border-b border-hairline pb-4">
        <span className="flex h-10 w-10 items-center justify-center rounded-full bg-accent/20 text-accent-ink">
          <UserRound size={18} />
        </span>
        <div className="min-w-0">
          <p className="truncate text-sm font-medium text-ink">{usuario?.nombreCompleto}</p>
          <p className="text-[11px] text-muted">{etiquetaRol(usuario?.rolDominio)} · Tablero</p>
        </div>

        <div className="ml-auto flex items-center gap-2">
          <span className="inline-flex items-center gap-1.5 rounded-pill border border-hairline bg-surface/60 px-3 py-1.5 num text-xs text-ink">
            <Clock3 size={14} className="text-accent-ink" /> {hora}
          </span>

          <div className="flex rounded-pill border border-hairline p-0.5">
            {(['barra', 'cocina'] as const).map((valor) => (
              <button
                key={valor}
                type="button"
                onClick={() => setArea(valor)}
                className={cn(
                  'inline-flex items-center gap-1.5 rounded-pill px-3 py-1.5 text-xs capitalize transition',
                  area === valor ? 'bg-accent/20 font-medium text-accent-ink' : 'text-muted hover:text-ink',
                )}
              >
                {valor === 'barra' ? <Martini size={14} /> : <ChefHat size={14} />}
                {valor}
              </button>
            ))}
          </div>

          <button
            type="button"
            aria-label="Salir"
            onClick={() => void logout()}
            className="flex h-10 w-10 items-center justify-center rounded-full border border-hairline text-muted transition hover:border-danger/40 hover:text-danger-ink"
          >
            <LogOut size={17} />
          </button>
        </div>
      </header>

      {/* Kanban */}
      <div className="flex min-h-0 flex-1 flex-col pt-4">
        {cuentas.isLoading ? (
          <p className="py-16 text-center text-sm text-muted">Cargando…</p>
        ) : (
          <DndContext onDragEnd={alSoltar}>
            <div className="grid min-h-0 flex-1 grid-cols-3 gap-3">
              {COLUMNAS.map((columna) => (
                <ColumnaKanban
                  key={columna.id}
                  columna={columna}
                  items={items.filter((i) => i.columna === columna.id)}
                  moviendo={mover.isPending}
                />
              ))}
            </div>
          </DndContext>
        )}
      </div>

      <Chat />
    </div>
  );
}

function ColumnaKanban({
  columna,
  items,
  moviendo,
}: {
  columna: (typeof COLUMNAS)[number];
  items: ItemKds[];
  moviendo: boolean;
}) {
  const { setNodeRef, isOver } = useDroppable({ id: columna.id });

  return (
    <section
      ref={setNodeRef}
      className={cn(
        'flex min-h-0 flex-col rounded-2xl border border-hairline bg-surface/30 p-3 transition',
        isOver && 'border-accent bg-accent/10',
      )}
    >
      <header className="mb-2 flex items-center justify-between px-1">
        <p className="text-xs font-medium uppercase tracking-tighter2 text-muted">{columna.titulo}</p>
        <span className="num rounded-pill bg-elevated/60 px-2 py-0.5 text-[10px] text-ink">{items.length}</span>
      </header>

      <div className="app-scroll flex min-h-0 flex-1 flex-col gap-2 overflow-y-auto pr-1">
        {items.length === 0 ? (
          <p className="rounded-inner border border-dashed border-hairline py-8 text-center text-xs text-muted">Vacío</p>
        ) : (
          items.map((item) => <TarjetaItem key={item.id} item={item} moviendo={moviendo} />)
        )}
      </div>
    </section>
  );
}

function TarjetaItem({ item, moviendo }: { item: ItemKds; moviendo: boolean }) {
  const { attributes, listeners, setNodeRef, isDragging } = useDraggable({ id: item.id });
  const minutos = minutosTranscurridos(item.fecha);
  const urgente = minutos >= 10;

  return (
    <div
      ref={setNodeRef}
      {...listeners}
      {...attributes}
      className={cn(
        'cursor-grab touch-none rounded-inner border border-hairline bg-surface p-3 shadow-soft transition active:cursor-grabbing',
        isDragging && 'rotate-2 border-accent opacity-80',
        urgente && item.columna !== 'terminado' && 'ring-1 ring-danger/60',
      )}
    >
      <div className="flex items-start justify-between gap-2">
        <div className="min-w-0">
          <p className="text-sm font-medium text-ink">
            Mesa {item.mesa} · {item.cantidad}×
          </p>
          <p className="mt-0.5 text-sm text-ink">{item.nombre}</p>
        </div>
        <GripVertical size={14} className="mt-0.5 shrink-0 text-muted" />
      </div>
      <p className={cn('mt-2 text-[11px]', urgente && item.columna !== 'terminado' ? 'text-danger-ink' : 'text-muted')}>
        {minutos < 1 ? 'recién recibido' : `${minutos} min`} · {formatTime(item.fecha)}
      </p>
      {moviendo && <p className="mt-1 text-[10px] text-muted">actualizando…</p>}
    </div>
  );
}
