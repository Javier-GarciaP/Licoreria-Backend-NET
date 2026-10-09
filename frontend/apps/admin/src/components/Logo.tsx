import { cn } from '@licoreria/ui';

/** Corcho estilizado: cuerpo redondeado con estrías (marca del sistema). */
export function LogoCorcho({ className }: { className?: string }) {
  return (
    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true" className={className}>
      <path
        d="M8 3.5h8A1.5 1.5 0 0 1 17.5 5v14a1.5 1.5 0 0 1-1.5 1.5H8A1.5 1.5 0 0 1 6.5 19V5A1.5 1.5 0 0 1 8 3.5Z"
        fill="currentColor"
      />
      <path
        d="M7.6 8.5h8.8M7.6 12h8.8M7.6 15.5h8.8"
        stroke="rgb(var(--card))"
        strokeWidth="1.3"
        strokeLinecap="round"
      />
    </svg>
  );
}

/** Marca del sistema: corcho + nombre CORCHO. `compacto` muestra solo el icono. */
export function Marca({
  compacto = false,
  chipClassName,
  nombreClassName,
}: {
  compacto?: boolean;
  chipClassName?: string;
  nombreClassName?: string;
}) {
  return (
    <span className="flex items-center gap-2.5">
      <span
        className={cn(
          'flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-primary text-primary-foreground',
          chipClassName,
        )}
      >
        <LogoCorcho className="h-5 w-5" />
      </span>
      {!compacto && (
        <span className={cn('text-sm font-medium tracking-tighter2 text-foreground', nombreClassName)}>
          CORCHO
        </span>
      )}
    </span>
  );
}
