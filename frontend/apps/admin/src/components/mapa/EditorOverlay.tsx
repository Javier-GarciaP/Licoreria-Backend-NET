import type { PointerEvent as ReactPointerEvent } from 'react';
import {
  ArrowDown,
  ArrowUp,
  Copy,
  RotateCw,
  SlidersHorizontal,
  Trash2,
} from 'lucide-react';
import { cn } from '@licoreria/ui';
import type { Mesa, PlanoElemento, Zona } from '@licoreria/types';
import { ELEMENTOS, UNIT } from './elementos';

const OFFSET = 16;

const PALETA = ['#c9b8f0', '#a8e6cf', '#a9d6f5', '#f7c9a6', '#f3b6c4', '#f4e1a1', '#9aa3b2', '#5b6373'];

export function EditorOverlay({
  elemento,
  zoom,
  pan,
  inspectorAbierto,
  onToggleInspector,
  onRotateStart,
  onResizeStart,
  onDuplicate,
  onDelete,
  onLayer,
  onUpdate,
  zonas,
  mesas,
  rootRef,
}: {
  elemento: PlanoElemento | null;
  zoom: number;
  pan: { x: number; y: number };
  inspectorAbierto: boolean;
  onToggleInspector: () => void;
  onRotateStart: (evento: ReactPointerEvent) => void;
  onResizeStart: (evento: ReactPointerEvent, esquina: string) => void;
  onDuplicate: () => void;
  onDelete: () => void;
  onLayer: (delta: number) => void;
  onUpdate: (cambios: Partial<PlanoElemento>) => void;
  zonas: Zona[];
  mesas: Mesa[];
  rootRef?: React.Ref<HTMLDivElement>;
}) {
  if (!elemento) return null;

  const w = elemento.ancho;
  const h = elemento.alto;
  const cx = elemento.posX + w / 2;
  const cy = elemento.posY + h / 2;
  const rad = (elemento.rotacion * Math.PI) / 180;
  const cos = Math.cos(rad);
  const sin = Math.sin(rad);

  const aPantalla = (gx: number, gy: number) => ({
    x: OFFSET + pan.x + gx * UNIT * zoom,
    y: OFFSET + pan.y + gy * UNIT * zoom,
  });
  const local = (lx: number, ly: number) => {
    const rx = lx * cos - ly * sin;
    const ry = lx * sin + ly * cos;
    return aPantalla(cx + rx, cy + ry);
  };

  const esquinas = {
    nw: local(-w / 2, -h / 2),
    ne: local(w / 2, -h / 2),
    sw: local(-w / 2, h / 2),
    se: local(w / 2, h / 2),
  };
  const arriba = local(0, -h / 2 - 34 / (UNIT * zoom));
  const centro = aPantalla(cx, cy);

  const maxY = Math.max(esquinas.nw.y, esquinas.ne.y, esquinas.sw.y, esquinas.se.y);

  return (
    <div ref={rootRef} className="pointer-events-none absolute inset-0 z-10">
      {/* Caja de selección rotada */}
      <svg className="absolute inset-0 h-full w-full overflow-visible">
        <polygon
          points={Object.values(esquinas).map((p) => `${p.x},${p.y}`).join(' ')}
          fill="none"
          stroke="rgb(var(--color-accent))"
          strokeWidth={1.5}
        />
        <line x1={arriba.x} y1={arriba.y} x2={esquinas.nw.x + (esquinas.ne.x - esquinas.nw.x) / 2} y2={esquinas.nw.y + (esquinas.ne.y - esquinas.nw.y) / 2} stroke="rgb(var(--color-accent))" strokeWidth={1.5} />
      </svg>

      {/* Handles de esquina */}
      {(['nw', 'ne', 'sw', 'se'] as const).map((esquina) => (
        <button
          key={esquina}
          type="button"
          aria-label={`Redimensionar ${esquina}`}
          onPointerDown={(evento) => onResizeStart(evento, esquina)}
          style={{ left: esquinas[esquina].x - 7, top: esquinas[esquina].y - 7 }}
          className={cn(
            'pointer-events-auto absolute h-3.5 w-3.5 touch-none rounded-full border-2 border-accent bg-surface shadow-soft',
            (esquina === 'nw' || esquina === 'se') ? 'cursor-nwse-resize' : 'cursor-nesw-resize',
          )}
        />
      ))}

      {/* Handle de rotar */}
      <button
        type="button"
        aria-label="Girar"
        onPointerDown={onRotateStart}
        style={{ left: arriba.x - 15, top: arriba.y - 15 }}
        className="pointer-events-auto absolute flex h-[30px] w-[30px] touch-none cursor-grab items-center justify-center rounded-full border-2 border-accent bg-surface text-ink shadow-card transition hover:bg-accent/20 active:cursor-grabbing"
      >
        <RotateCw size={14} />
      </button>

      {/* Barra flotante (debajo del elemento para no tapar el handle de giro) */}
      <div
        style={{ left: centro.x, top: maxY + 14 }}
        onPointerDown={(evento) => evento.stopPropagation()}
        className="pointer-events-auto absolute flex -translate-x-1/2 items-center gap-0.5 rounded-pill border border-hairline bg-surface/95 p-1 shadow-card backdrop-blur"
      >
        <BotonOverlay label="Inspector" activo={inspectorAbierto} onClick={onToggleInspector}>
          <SlidersHorizontal size={15} />
        </BotonOverlay>
        <BotonOverlay label="Subir capa" onClick={() => onLayer(1)}>
          <ArrowUp size={15} />
        </BotonOverlay>
        <BotonOverlay label="Bajar capa" onClick={() => onLayer(-1)}>
          <ArrowDown size={15} />
        </BotonOverlay>
        <BotonOverlay label="Duplicar" onClick={onDuplicate}>
          <Copy size={15} />
        </BotonOverlay>
        <BotonOverlay label="Quitar" onClick={onDelete}>
          <Trash2 size={15} />
        </BotonOverlay>
      </div>

      {/* Menú inspector */}
      {inspectorAbierto && (
        <div
          style={{ left: centro.x, top: maxY + 14 + 42 }}
          onPointerDown={(evento) => evento.stopPropagation()}
          className="pointer-events-auto absolute w-64 -translate-x-1/2 rounded-2xl border border-hairline bg-surface/95 p-3 shadow-card backdrop-blur"
        >
          <div className="flex flex-col gap-2.5">
            <label className="flex flex-col gap-1 text-xs text-muted">
              Forma
              <select
                value={elemento.forma ?? ''}
                onChange={(e) => onUpdate({ forma: e.target.value, tipo: e.target.value.startsWith('mesa') ? 'mesa' : e.target.value })}
                className="h-9 rounded-control border border-hairline bg-surface px-2 text-sm text-ink"
              >
                {ELEMENTOS.map((def) => (
                  <option key={def.forma} value={def.forma}>
                    {def.label}
                  </option>
                ))}
              </select>
            </label>

            <div className="flex flex-col gap-1 text-xs text-muted">
              Color
              <div className="flex flex-wrap gap-1.5">
                {PALETA.map((color) => (
                  <button
                    key={color}
                    type="button"
                    aria-label={`Color ${color}`}
                    onClick={() => onUpdate({ color })}
                    style={{ backgroundColor: color }}
                    className={cn(
                      'h-6 w-6 rounded-full border transition',
                      elemento.color === color ? 'border-ink ring-2 ring-accent/40' : 'border-hairline',
                    )}
                  />
                ))}
              </div>
            </div>

            <div className="grid grid-cols-2 gap-2">
              <Campo label="Ancho" value={elemento.ancho} onChange={(v) => onUpdate({ ancho: Math.max(0.5, v) })} />
              <Campo label="Alto" value={elemento.alto} onChange={(v) => onUpdate({ alto: Math.max(0.5, v) })} />
            </div>

            <label className="flex flex-col gap-1 text-xs text-muted">
              Etiqueta
              <input
                value={elemento.etiqueta ?? ''}
                onChange={(e) => onUpdate({ etiqueta: e.target.value })}
                className="h-9 rounded-control border border-hairline bg-surface px-2 text-sm text-ink"
              />
            </label>

            <label className="flex flex-col gap-1 text-xs text-muted">
              Zona
              <select
                value={elemento.zonaId ?? ''}
                onChange={(e) => onUpdate({ zonaId: e.target.value || null })}
                className="h-9 rounded-control border border-hairline bg-surface px-2 text-sm text-ink"
              >
                <option value="">Sin zona</option>
                {zonas.map((zona) => (
                  <option key={zona.id} value={zona.id}>
                    {zona.nombre}
                  </option>
                ))}
              </select>
            </label>

            {(elemento.tipo === 'mesa' || elemento.forma?.startsWith('mesa')) && (
              <label className="flex flex-col gap-1 text-xs text-muted">
                Mesa operativa
                <select
                  value={elemento.mesaId ?? ''}
                  onChange={(e) => onUpdate({ mesaId: e.target.value || null })}
                  className="h-9 rounded-control border border-hairline bg-surface px-2 text-sm text-ink"
                >
                  <option value="">Sin enlazar</option>
                  {mesas.map((mesa) => (
                    <option key={mesa.id} value={mesa.id}>
                      {mesa.numero} · {mesa.zonaNombre}
                    </option>
                  ))}
                </select>
              </label>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

function BotonOverlay({
  label,
  activo,
  onClick,
  children,
}: {
  label: string;
  activo?: boolean;
  onClick: () => void;
  children: React.ReactNode;
}) {
  return (
    <button
      type="button"
      title={label}
      aria-label={label}
      onClick={onClick}
      className={cn(
        'flex h-7 w-7 items-center justify-center rounded-full transition',
        activo ? 'bg-accent/25 text-accent-ink' : 'text-muted hover:bg-ink/5 hover:text-ink',
      )}
    >
      {children}
    </button>
  );
}

function Campo({ label, value, onChange }: { label: string; value: number; onChange: (v: number) => void }) {
  return (
    <label className="flex flex-col gap-1 text-xs text-muted">
      {label}
      <input
        type="number"
        step="0.5"
        value={value}
        onChange={(e) => onChange(Number(e.target.value))}
        className="num h-9 rounded-control border border-hairline bg-surface px-2 text-sm text-ink"
      />
    </label>
  );
}
