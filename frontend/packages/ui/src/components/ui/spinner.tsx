import { Loader2 } from 'lucide-react';
import { cn } from '../../lib/cn';

export interface SpinnerProps {
  className?: string;
  size?: 'sm' | 'default' | 'lg';
}

const dims = { sm: 'h-4 w-4', default: 'h-5 w-5', lg: 'h-6 w-6' } as const;

export function Spinner({ className, size = 'default' }: SpinnerProps) {
  return <Loader2 aria-label="Cargando" className={cn('animate-spin text-foreground', dims[size], className)} />;
}