import { memo, type PointerEvent as ReactPointerEvent, type ReactNode } from 'react';
import type { Plano, PlanoElemento, Zona } from '@licoreria/types';
import { cn } from '@licoreria/ui';
import { dibujarElemento, ELEMENTOS_POR_FORMA, UNIT } from './elementos';

export type EstadoMesaPlano = 'Libre' | 'Ocupada' | 'Reservada' | 'EnLimpieza';

const COLOR_ESTADO: Record<EstadoMesaPlano, string> = {
  Libre: '#2fbf71',
  Ocupada: '#ef4444',
  Reservada: '#f5a524',
  EnLimpieza: '#3b82f6',
};

const PISO: Record<string, string> = {
  madera: '#6b4a2f',
  cemento: '#3a4150',
  neon: '#241b3a',
  claro: '#d9d2c6',
};

const ElementoMapa = memo(function ElementoMapa({
  el,
  estado,
  numero,
  modo,
  onPointerDown,
}: {
  el: PlanoElemento;
  estado?: EstadoMesaPlano;
  numero?: string;
  modo: 'editor' | 'operacion';
  onPointerDown?: (evento: ReactPointerEvent, elemento: PlanoElemento) => void;
}) {
  const x = el.posX * UNIT;
  const y = el.posY * UNIT;
  const w = el.ancho * UNIT;
  const h = el.alto * UNIT;
  const esMesa = (el.tipo ?? '').startsWith('mesa');
  const color = (estado ? COLOR_ESTADO[estado] : el.color) || ELEMENTOS_POR_FORMA[el.forma ?? '']?.color || '#c9b8f0';
  const etiqueta = numero ?? el.etiqueta;

  return (
    <g
      data-el={el.id}
      transform={`translate(${x} ${y})`}
      style={{ cursor: modo === 'editor' ? 'grab' : esMesa ? 'pointer' : 'default' }}
      onPointerDown={onPointerDown ? (evento) => onPointerDown(evento, el) : undefined}
    >
      <g transform={`rotate(${el.rotacion} ${w / 2} ${h / 2})`}>{dibujarElemento(el.forma, w, h, color)}</g>
      {etiqueta && (
        <text x={w / 2} y={h / 2 + 4} textAnchor="middle" fill="#0b0d10" fontSize={13} fontWeight={600} pointerEvents="none">
          {etiqueta}
        </text>
      )}
    </g>
  );
});

function FondoMapa({ plano, rejilla }: { plano: Plano; rejilla: boolean }) {
  const W = plano.anchoFondo * UNIT;
  const H = plano.altoFondo * UNIT;
  const paso = Math.max(0.25, plano.rejilla) * UNIT;
  const gridId = `grid-${plano.id}`;
  return (
    <>
      <defs>
        <pattern id={gridId} width={paso} height={paso} patternUnits="userSpaceOnUse">
          <path d={`M ${paso} 0 L 0 0 0 ${paso}`} fill="none" stroke="rgba(255,255,255,0.08)" strokeWidth={1} />
        </pattern>
      </defs>
      <rect x={0} y={0} width={W} height={H} rx={16} fill={PISO[plano.piso] ?? PISO.madera} />
      {rejilla && <rect x={0} y={0} width={W} height={H} rx={16} fill={`url(#${gridId})`} />}
    </>
  );
}

function CapaZonas({ zonas }: { zonas: Zona[] }) {
  return (
    <>
      {zonas
        .filter((zona) => zona.ancho > 0 && zona.alto > 0)
        .map((zona) => (
          <g key={zona.id}>
            <rect
              x={zona.posX * UNIT}
              y={zona.posY * UNIT}
              width={zona.ancho * UNIT}
              height={zona.alto * UNIT}
              rx={16}
              fill={zona.color ?? '#c9b8f0'}
              opacity={0.16}
              stroke={zona.color ?? '#c9b8f0'}
              strokeWidth={1.5}
              strokeDasharray="8 6"
            />
            <text x={zona.posX * UNIT + 12} y={zona.posY * UNIT + 22} fill="rgba(255,255,255,0.75)" fontSize={13} fontWeight={600}>
              {zona.nombre}
            </text>
          </g>
        ))}
    </>
  );
}

export function MapaViewBase({
  plano,
  zonas,
  estadoPorMesa,
  numeroPorMesa,
  modo = 'operacion',
  rejilla = true,
  onElementoPointerDown,
  onFondoPointerDown,
  children,
}: {
  plano: Plano;
  zonas: Zona[];
  estadoPorMesa?: Record<string, EstadoMesaPlano>;
  numeroPorMesa?: Record<string, string>;
  modo?: 'editor' | 'operacion';
  rejilla?: boolean;
  onElementoPointerDown?: (evento: ReactPointerEvent, elemento: PlanoElemento) => void;
  onFondoPointerDown?: (evento: ReactPointerEvent) => void;
  children?: ReactNode;
}) {
  const W = plano.anchoFondo * UNIT;
  const H = plano.altoFondo * UNIT;
  const elementos = [...plano.elementos].sort((a, b) => a.z - b.z);

  return (
    <svg
      width={W}
      height={H}
      viewBox={`0 0 ${W} ${H}`}
      className={cn('block rounded-lg', modo === 'editor' ? 'touch-none select-none' : 'max-w-full')}
      onPointerDown={onFondoPointerDown}
    >
      <FondoMapa plano={plano} rejilla={rejilla} />
      <CapaZonas zonas={zonas} />
      {elementos.map((el) => {
        const esMesa = (el.tipo ?? '').startsWith('mesa');
        const estado = esMesa && el.mesaId ? estadoPorMesa?.[el.mesaId] : undefined;
        const numero = esMesa && el.mesaId ? numeroPorMesa?.[el.mesaId] : undefined;
        return <ElementoMapa key={el.id} el={el} estado={estado} numero={numero} modo={modo} onPointerDown={onElementoPointerDown} />;
      })}
      {children}
    </svg>
  );
}

export const MapaView = memo(MapaViewBase);
