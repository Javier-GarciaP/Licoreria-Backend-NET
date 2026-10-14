interface BotanicalWatermarkProps {
  /** Variante de composición por sección. */
  variant?: 'hero' | 'left' | 'right' | 'band';
}

/**
 * Capa de marca de agua botánica (helechos / hojas / flor).
 * Casi invisible en reposo; se percibe como textura de muro tandoor.
 */
export function BotanicalWatermark({ variant = 'hero' }: BotanicalWatermarkProps) {
  if (variant === 'band') {
    return (
      <svg
        className="botanical-layer"
        viewBox="0 0 1440 400"
        preserveAspectRatio="xMidYMid slice"
        aria-hidden="true"
      >
        <path
          d="M-40 300 C 200 220 340 300 480 240 S 760 120 960 200 1240 320 1480 180"
          fill="none"
          stroke="currentColor"
          strokeWidth="2"
        />
        {Array.from({ length: 14 }).map((_, i) => (
          <ellipse
            key={i}
            cx={80 + i * 100}
            cy={260 - Math.sin(i) * 60}
            rx="26"
            ry="12"
            fill="currentColor"
            transform={`rotate(${-18 + i * 7} ${80 + i * 100} ${260 - Math.sin(i) * 60})`}
          />
        ))}
      </svg>
    );
  }

  const flip = variant === 'right';
  return (
    <svg
      className="botanical-layer"
      viewBox="0 0 1440 900"
      preserveAspectRatio="xMidYMid slice"
      aria-hidden="true"
      style={flip ? { transform: 'scaleX(-1)' } : undefined}
    >
      {[
        { x: 120, y: 120, s: 1.5, r: -24 },
        { x: 260, y: 420, s: 1.1, r: 18 },
        { x: 1180, y: 180, s: 1.35, r: 32 },
        { x: 1320, y: 620, s: 1.7, r: -14 },
        { x: 60, y: 760, s: 1.2, r: 10 },
      ].map((leaf, i) => (
        <g key={i} transform={`translate(${leaf.x} ${leaf.y}) rotate(${leaf.r}) scale(${leaf.s})`}>
          <path
            d="M0 0 C 60 -40 120 -40 180 0 C 120 40 60 40 0 0 Z"
            fill="none"
            stroke="currentColor"
            strokeWidth="2.5"
          />
          <path d="M0 0 H 180" stroke="currentColor" strokeWidth="1.5" />
          {Array.from({ length: 8 }).map((_, n) => (
            <path
              key={n}
              d={`M${12 + n * 21} 0 L ${28 + n * 21} ${n % 2 === 0 ? -16 : 16}`}
              stroke="currentColor"
              strokeWidth="1"
            />
          ))}
        </g>
      ))}
    </svg>
  );
}
