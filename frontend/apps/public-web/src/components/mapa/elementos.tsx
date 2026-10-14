import type { ReactNode } from 'react';

/** Píxeles por unidad de rejilla. */
export const UNIT = 64;

export type GrupoElemento = 'Mobiliario' | 'Estructura' | 'Zonas' | 'Decoración';

export interface ElementoDef {
  forma: string;
  label: string;
  grupo: GrupoElemento;
  /** Tamaño por defecto en unidades de rejilla. */
  w: number;
  h: number;
  color: string;
  dibujar: (w: number, h: number, color: string) => ReactNode;
}

const stroke = 'rgba(15,18,24,0.28)';
const soft = 'rgba(15,18,24,0.14)';

function mesa(w: number, h: number, color: string, redonda: boolean) {
  const cx = w / 2;
  const cy = h / 2;
  const rx = w * 0.32;
  const ry = h * 0.32;
  const sillas = 4;
  const sillasNodos: ReactNode[] = [];
  for (let i = 0; i < sillas; i += 1) {
    const ang = (i / sillas) * Math.PI * 2 + Math.PI / 4;
    const sx = cx + Math.cos(ang) * (w * 0.44);
    const sy = cy + Math.sin(ang) * (h * 0.44);
    sillasNodos.push(<circle key={i} cx={sx} cy={sy} r={Math.min(w, h) * 0.1} fill="rgb(var(--color-surface))" stroke={stroke} strokeWidth={1} />);
  }
  return (
    <>
      {sillasNodos}
      {redonda ? (
        <circle cx={cx} cy={cy} r={Math.min(rx, ry)} fill={color} stroke={stroke} strokeWidth={1.5} />
      ) : (
        <rect x={cx - rx} y={cy - ry} width={rx * 2} height={ry * 2} rx={8} fill={color} stroke={stroke} strokeWidth={1.5} />
      )}
    </>
  );
}

function barra(w: number, h: number, color: string) {
  return (
    <>
      <rect x={4} y={h * 0.28} width={w - 8} height={h * 0.44} rx={6} fill={color} stroke={stroke} strokeWidth={1.5} />
      <line x1={4} y1={h * 0.28} x2={w - 4} y2={h * 0.28} stroke={soft} strokeWidth={3} />
      {[0.14, 0.38, 0.62, 0.86].map((p) => (
        <circle key={p} cx={w * p} cy={h * 0.86} r={Math.min(w, h) * 0.07} fill="rgb(var(--color-surface))" stroke={stroke} strokeWidth={1} />
      ))}
    </>
  );
}

function pista(w: number, h: number, color: string) {
  const inset = 6;
  const iw = w - inset * 2;
  const ih = h - inset * 2;
  const cols = Math.max(4, Math.round(iw / 32));
  const rows = Math.max(3, Math.round(ih / 32));
  const cw = iw / cols;
  const ch = ih / rows;
  const celdas: ReactNode[] = [];
  for (let r = 0; r < rows; r += 1) {
    for (let c = 0; c < cols; c += 1) {
      if ((r + c) % 2 === 0) {
        celdas.push(<rect key={`${r}-${c}`} x={inset + c * cw} y={inset + r * ch} width={cw} height={ch} fill="rgba(255,255,255,0.2)" />);
      }
    }
  }
  return (
    <>
      <rect x={0} y={0} width={w} height={h} rx={10} fill={color} stroke={stroke} strokeWidth={2} />
      {celdas}
      <rect x={0} y={0} width={w} height={h} rx={10} fill="none" stroke={stroke} strokeWidth={2} />
    </>
  );
}

function escenario(w: number, h: number, color: string) {
  return (
    <>
      <rect x={2} y={h * 0.2} width={w - 4} height={h * 0.7} rx={6} fill={color} stroke={stroke} strokeWidth={1.5} />
      <path d={`M 2 ${h * 0.2} L ${w - 2} ${h * 0.2} L ${w - 12} ${h * 0.06} L 12 ${h * 0.06} Z`} fill="rgba(255,255,255,0.25)" stroke={stroke} strokeWidth={1} />
    </>
  );
}

function dj(w: number, h: number, color: string) {
  return (
    <>
      <rect x={2} y={h * 0.18} width={w - 4} height={h * 0.64} rx={6} fill={color} stroke={stroke} strokeWidth={1.5} />
      <circle cx={w * 0.3} cy={h * 0.5} r={Math.min(w, h) * 0.14} fill="rgba(15,18,24,0.35)" />
      <circle cx={w * 0.7} cy={h * 0.5} r={Math.min(w, h) * 0.14} fill="rgba(15,18,24,0.35)" />
    </>
  );
}

function silla(w: number, h: number, color: string) {
  return (
    <>
      <rect x={w * 0.2} y={h * 0.3} width={w * 0.6} height={h * 0.5} rx={5} fill={color} stroke={stroke} strokeWidth={1.2} />
      <rect x={w * 0.2} y={h * 0.16} width={w * 0.6} height={h * 0.12} rx={4} fill={color} stroke={stroke} strokeWidth={1.2} />
    </>
  );
}

function taburete(w: number, h: number, color: string) {
  return <circle cx={w / 2} cy={h / 2} r={Math.min(w, h) * 0.32} fill={color} stroke={stroke} strokeWidth={1.2} />;
}

function muro(w: number, h: number, color: string) {
  return <rect x={0} y={0} width={w} height={h} rx={3} fill={color} stroke={stroke} strokeWidth={1} />;
}

function columna(w: number, h: number, color: string) {
  const r = Math.min(w, h) * 0.34;
  return (
    <>
      <circle cx={w / 2} cy={h / 2} r={r} fill={color} stroke={stroke} strokeWidth={1.5} />
      <circle cx={w / 2} cy={h / 2} r={r * 0.5} fill="rgba(255,255,255,0.25)" />
    </>
  );
}

function planta(w: number, h: number, color: string) {
  return (
    <>
      <circle cx={w / 2} cy={h / 2} r={Math.min(w, h) * 0.34} fill="rgba(15,18,24,0.12)" />
      <path
        d={`M ${w / 2} ${h * 0.78} C ${w * 0.2} ${h * 0.6} ${w * 0.25} ${h * 0.2} ${w / 2} ${h * 0.22} C ${w * 0.75} ${h * 0.2} ${w * 0.8} ${h * 0.6} ${w / 2} ${h * 0.78} Z`}
        fill={color}
        stroke={stroke}
        strokeWidth={1}
      />
    </>
  );
}

function escalera(w: number, h: number, color: string) {
  const pasos = 5;
  return (
    <>
      <rect x={2} y={2} width={w - 4} height={h - 4} rx={3} fill={color} stroke={stroke} strokeWidth={1.5} />
      {Array.from({ length: pasos - 1 }).map((_, i) => (
        <line key={i} x1={2} y1={((i + 1) * h) / pasos} x2={w - 2} y2={((i + 1) * h) / pasos} stroke={soft} strokeWidth={2} />
      ))}
    </>
  );
}

function icono(w: number, h: number, color: string, glyph: 'entrada' | 'bano' | 'guardarropia' | 'caja') {
  return (
    <>
      <rect x={2} y={2} width={w - 4} height={h - 4} rx={6} fill={color} stroke={stroke} strokeWidth={1.5} />
      {glyph === 'entrada' && (
        <path d={`M ${w * 0.28} ${h / 2} L ${w * 0.7} ${h / 2} M ${w * 0.55} ${h * 0.34} L ${w * 0.7} ${h / 2} L ${w * 0.55} ${h * 0.66}`} fill="none" stroke="rgba(15,18,24,0.4)" strokeWidth={2} />
      )}
      {glyph === 'bano' && (
        <>
          <circle cx={w * 0.38} cy={h * 0.35} r={Math.min(w, h) * 0.08} fill="rgba(15,18,24,0.4)" />
          <rect x={w * 0.34} y={h * 0.44} width={w * 0.09} height={h * 0.28} rx={3} fill="rgba(15,18,24,0.4)" />
          <circle cx={w * 0.64} cy={h * 0.35} r={Math.min(w, h) * 0.08} fill="rgba(15,18,24,0.4)" />
          <rect x={w * 0.6} y={h * 0.44} width={w * 0.09} height={h * 0.28} rx={3} fill="rgba(15,18,24,0.4)" />
        </>
      )}
      {glyph === 'guardarropia' && (
        <path d={`M ${w * 0.5} ${h * 0.3} L ${w * 0.5} ${h * 0.45} M ${w * 0.32} ${h * 0.6} L ${w * 0.5} ${h * 0.45} L ${w * 0.68} ${h * 0.6}`} fill="none" stroke="rgba(15,18,24,0.4)" strokeWidth={2} />
      )}
      {glyph === 'caja' && (
        <>
          <rect x={w * 0.3} y={h * 0.28} width={w * 0.4} height={h * 0.3} rx={3} fill="rgba(15,18,24,0.35)" />
          <rect x={w * 0.34} y={h * 0.62} width={w * 0.32} height={h * 0.1} rx={2} fill="rgba(15,18,24,0.25)" />
        </>
      )}
    </>
  );
}

export const ELEMENTOS: ElementoDef[] = [
  { forma: 'mesa_redonda', label: 'Mesa redonda', grupo: 'Mobiliario', w: 2, h: 2, color: '#c9b8f0', dibujar: (w, h, c) => mesa(w, h, c, true) },
  { forma: 'mesa_cuadrada', label: 'Mesa cuadrada', grupo: 'Mobiliario', w: 2, h: 2, color: '#c9b8f0', dibujar: (w, h, c) => mesa(w, h, c, false) },
  { forma: 'mesa_rectangular', label: 'Mesa rectangular', grupo: 'Mobiliario', w: 3, h: 1.5, color: '#c9b8f0', dibujar: (w, h, c) => mesa(w, h, c, false) },
  { forma: 'silla', label: 'Silla', grupo: 'Mobiliario', w: 1, h: 1, color: '#a9d6f5', dibujar: silla },
  { forma: 'taburete', label: 'Taburete', grupo: 'Mobiliario', w: 0.75, h: 0.75, color: '#a9d6f5', dibujar: taburete },
  { forma: 'barra', label: 'Barra', grupo: 'Estructura', w: 5, h: 1, color: '#f7c9a6', dibujar: barra },
  { forma: 'muro', label: 'Muro', grupo: 'Estructura', w: 4, h: 0.5, color: '#5b6373', dibujar: muro },
  { forma: 'columna', label: 'Columna', grupo: 'Estructura', w: 1, h: 1, color: '#9aa3b2', dibujar: columna },
  { forma: 'escalera', label: 'Escalera', grupo: 'Estructura', w: 2, h: 3, color: '#c7ccd6', dibujar: escalera },
  { forma: 'pista', label: 'Pista de baile', grupo: 'Zonas', w: 6, h: 4, color: '#8f7fe0', dibujar: pista },
  { forma: 'escenario', label: 'Escenario', grupo: 'Zonas', w: 5, h: 2, color: '#f4e1a1', dibujar: escenario },
  { forma: 'dj', label: 'Cabina DJ', grupo: 'Zonas', w: 2.5, h: 1.5, color: '#f4e1a1', dibujar: dj },
  { forma: 'entrada', label: 'Entrada', grupo: 'Zonas', w: 1.5, h: 1.5, color: '#a8e6cf', dibujar: (w, h, c) => icono(w, h, c, 'entrada') },
  { forma: 'bano', label: 'Baños', grupo: 'Zonas', w: 2, h: 1.5, color: '#a9d6f5', dibujar: (w, h, c) => icono(w, h, c, 'bano') },
  { forma: 'guardarropia', label: 'Guardarropía', grupo: 'Zonas', w: 1.5, h: 1.5, color: '#c7ccd6', dibujar: (w, h, c) => icono(w, h, c, 'guardarropia') },
  { forma: 'caja', label: 'Caja', grupo: 'Zonas', w: 1.5, h: 1, color: '#f3b6c4', dibujar: (w, h, c) => icono(w, h, c, 'caja') },
  { forma: 'planta', label: 'Planta', grupo: 'Decoración', w: 1, h: 1, color: '#a8e6cf', dibujar: planta },
];

export const ELEMENTOS_POR_FORMA: Record<string, ElementoDef> = Object.fromEntries(
  ELEMENTOS.map((e) => [e.forma, e]),
);

/** Dibuja un elemento por su forma, en un espacio de [0,0,w,h] píxeles. */
export function dibujarElemento(forma: string | null | undefined, w: number, h: number, color: string): ReactNode {
  const def = forma ? ELEMENTOS_POR_FORMA[forma] : undefined;
  if (!def) return <rect x={0} y={0} width={w} height={h} rx={6} fill={color} stroke={stroke} strokeWidth={1.5} />;
  return def.dibujar(w, h, color);
}