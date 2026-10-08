import { Link, useLocation } from 'react-router-dom';
import { motion, useReducedMotion } from 'framer-motion';
import { ArrowLeft, Plus } from 'lucide-react';
import { Button } from '@licoreria/ui';

/** Botella vacía: la ruta pedida no está en la carta. Un solo momento orquestado. */
function BotellaVacia() {
  const reduce = useReducedMotion();

  return (
    <div className="relative mx-auto aspect-[4/5] w-full max-w-[300px]">
      <div
        aria-hidden
        className="absolute left-1/2 top-1/2 h-56 w-56 -translate-x-1/2 -translate-y-1/2 rounded-full bg-danger/30 blur-[70px]"
      />

      <motion.div
        aria-hidden
        className="absolute inset-0"
        initial={reduce ? { opacity: 0 } : { opacity: 0, rotate: -16, y: 20 }}
        animate={reduce ? { opacity: 1 } : { opacity: 1, rotate: -8, y: 0 }}
        transition={{ type: 'spring', stiffness: 120, damping: 16, mass: 0.8 }}
      >
        <svg viewBox="0 0 220 360" className="h-full w-full" fill="none">
          <path
            d="M 36 328 L 184 328 Q 188 328 188 322 L 188 208 Q 188 184 160 174 L 132 154 Q 130 150 130 138 L 130 108 L 90 108 L 90 138 Q 90 150 88 154 L 60 174 Q 32 184 32 208 L 32 322 Q 32 328 36 328 Z"
            fill="rgb(var(--color-danger) / 0.45)"
            stroke="rgb(var(--color-danger-ink) / 0.55)"
            strokeWidth={1.5}
          />
          <rect
            x={84}
            y={96}
            width={52}
            height={18}
            rx={5}
            fill="rgb(var(--color-danger) / 0.6)"
            stroke="rgb(var(--color-danger-ink) / 0.55)"
            strokeWidth={1.5}
          />
          <path
            d="M 48 316 L 48 224 Q 48 206 66 194"
            stroke="rgba(255,255,255,0.5)"
            strokeWidth={3}
            strokeLinecap="round"
          />
          <path
            d="M 178 250 Q 178 238 170 230"
            stroke="rgba(255,255,255,0.35)"
            strokeWidth={2.5}
            strokeLinecap="round"
          />
          <g>
            <rect x={72} y={236} width={76} height={66} rx={8} fill="rgb(var(--color-danger))" />
            <text
              x={110}
              y={254}
              textAnchor="middle"
              fontFamily="'IBM Plex Mono', monospace"
              fontSize={8.5}
              letterSpacing={2}
              fill="rgb(var(--color-on-pastel) / 0.75)"
            >
              SIN CONTENIDO
            </text>
            <line x1={88} y1={262} x2={132} y2={262} stroke="rgb(var(--color-on-pastel) / 0.35)" strokeWidth={1} />
            <text
              x={110}
              y={294}
              textAnchor="middle"
              fontFamily="'IBM Plex Mono', monospace"
              fontSize={34}
              fontWeight={500}
              fill="rgb(var(--color-on-pastel))"
            >
              404
            </text>
          </g>
        </svg>
      </motion.div>

      {!reduce && (
        <>
          <motion.span
            aria-hidden
            className="absolute left-[54%] h-2 w-2 rounded-full bg-danger-ink/70"
            initial={{ top: '32%', opacity: 0 }}
            animate={{
              top: ['32%', '90%'],
              opacity: [0, 1, 1, 0],
              transition: { duration: 1.4, delay: 0.8, times: [0, 0.12, 0.85, 1], ease: 'easeIn' },
            }}
          />
          <motion.span
            aria-hidden
            className="absolute left-[54%] top-[90%] h-4 w-4 rounded-full border border-destructive-ink/60"
            style={{ marginLeft: -8, marginTop: -8 }}
            initial={{ scale: 0.2, opacity: 0.8 }}
            animate={{ scale: 3.2, opacity: 0, transition: { duration: 1.2, delay: 2.0, ease: 'easeOut' } }}
          />
          <motion.span
            aria-hidden
            className="absolute left-[54%] top-[90%] h-4 w-4 rounded-full border border-destructive-ink/40"
            style={{ marginLeft: -8, marginTop: -8 }}
            initial={{ scale: 0.2, opacity: 0.6 }}
            animate={{ scale: 2.4, opacity: 0, transition: { duration: 1.2, delay: 2.2, ease: 'easeOut' } }}
          />
        </>
      )}

      <div aria-hidden className="absolute left-1/2 top-[94%] h-px w-3/4 -translate-x-1/2 bg-danger-ink/25" />
    </div>
  );
}

export function NotFoundPage() {
  const { pathname } = useLocation();

  return (
    <div className="flex min-h-full items-center justify-center py-6">
      <div className="border border-border bg-card w-full max-w-5xl overflow-hidden rounded-2xl">
        <div className="grid items-center gap-10 p-6 sm:p-10 lg:grid-cols-[0.9fr_1.1fr] lg:gap-12 lg:p-12">
          <BotellaVacia />

          <div className="flex flex-col items-start gap-5">
            <span className="inline-flex items-center gap-2 rounded-full bg-danger/25 px-3 py-1 text-[11px] font-medium uppercase tracking-[0.18em] text-destructive-fg">
              404 · No encontrado
            </span>
            <h1 className="text-3xl font-medium tracking-tighter2 text-foreground sm:text-4xl">
              ESA RUTA NO ESTÁ EN LA CARTA
            </h1>
            <p className="max-w-md text-sm leading-relaxed text-muted-foreground">
              La URL no existe o cambió de sección. Revisá la dirección o buscá lo que necesitás con ⌘K.
            </p>
            <div className="flex items-center gap-3 rounded-control border border-border bg-card px-3 py-2">
              <span className="num text-[11px] uppercase tracking-[0.14em] text-muted-foreground">Ruta</span>
              <span className="num truncate text-sm text-foreground">{pathname}</span>
            </div>
            <div className="mt-1 flex flex-wrap gap-2">
              <Link to="/">
                <Button leftIcon={<ArrowLeft size={16} />}>Volver al inicio</Button>
              </Link>
              <Link to="/pos">
                <Button variant="ghost" leftIcon={<Plus size={16} />}>
                  Nueva venta
                </Button>
              </Link>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}