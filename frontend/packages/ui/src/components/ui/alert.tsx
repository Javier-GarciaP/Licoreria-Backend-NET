import type { ReactNode } from 'react';
import { cva } from 'class-variance-authority';
import { cn } from '../../lib/cn';

export type AlertVariant = 'default' | 'info' | 'success' | 'warning' | 'destructive';

export const alertVariants = cva(
  'relative w-full rounded-lg border p-4 text-sm',
  {
    variants: {
      variant: {
        default: 'border-border bg-card text-foreground',
        info: 'border-transparent bg-info/20 text-info-fg',
        success: 'border-transparent bg-success/20 text-success-fg',
        warning: 'border-transparent bg-warning/20 text-warning-fg',
        destructive: 'border-transparent bg-destructive/20 text-destructive-fg',
      },
    },
    defaultVariants: { variant: 'default' },
  },
);

export interface AlertProps {
  className?: string;
  variant?: AlertVariant;
  children?: ReactNode;
}

export function Alert({ className, variant = 'default', children }: AlertProps) {
  return <div role="alert" className={cn(alertVariants({ variant }), className)}>{children}</div>;
}