import { useId } from 'react';
import { cn } from '@licoreria/ui';

/** Corcho de champán: cabeza ancha con destello y cuerpo cónico (marca del sistema). */
export function LogoCorcho({ className }: { className?: string }) {
  const id = useId();
  return (
    <svg viewBox="0 0 24 24" fill="none" aria-hidden="true" className={className}>
      <defs>
        <mask id={id} maskUnits="userSpaceOnUse" x="0" y="0" width="24" height="24">
          <rect width="24" height="24" fill="white" />
          <path d="M7.4 5.3Q12 7.7 16.6 5.3" stroke="black" strokeWidth="0.9" strokeLinecap="round" />
        </mask>
      </defs>
      <g fill="currentColor">
        <path
          mask={`url(#${id})`}
          d="M12 2C15.6 2 18.1 2.5 18.1 4.2V9.2C18.1 10.6 16.9 11.2 15.4 11.3H8.6C7.1 11.2 5.9 10.6 5.9 9.2V4.2C5.9 2.5 8.4 2 12 2Z"
        />
        <path d="M8 12.1H16L16.8 20.6C16.8 21.5 15 22 12 22C9 22 7.2 21.5 7.2 20.6L8 12.1Z" />
      </g>
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
