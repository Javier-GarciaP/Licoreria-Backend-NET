import { UserRound } from 'lucide-react';
import { cn } from '../../lib/cn';

export interface AvatarProps {
  className?: string;
  /** Iniciales o texto corto (se muestra si no hay imagen). */
  fallback?: string;
  src?: string;
  alt?: string;
}

export function Avatar({ className, fallback, src, alt = '' }: AvatarProps) {
  return (
    <span className={cn('relative flex h-10 w-10 shrink-0 overflow-hidden rounded-full', className)}>
      {src ? (
        <img src={src} alt={alt} className="aspect-square h-full w-full object-cover" />
      ) : (
        <span className="flex h-full w-full items-center justify-center rounded-full bg-muted text-muted-foreground">
          {fallback ? (
            <span className="text-xs font-medium leading-none">{fallback.slice(0, 2).toUpperCase()}</span>
          ) : (
            <UserRound className="h-5 w-5" />
          )}
        </span>
      )}
    </span>
  );
}