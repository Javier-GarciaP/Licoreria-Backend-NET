import type { ReactNode } from 'react';
import { cva } from 'class-variance-authority';
import { cn } from '../../lib/cn';

export type BadgeVariant = 'default' | 'secondary' | 'outline' | 'success' | 'warning' | 'info' | 'destructive' | 'butter';

export const badgeVariants = cva(
  'inline-flex items-center gap-1 rounded-full border px-2.5 py-0.5 text-xs font-medium transition-colors',
  {
    variants: {
      variant: {
        default: 'border-transparent bg-primary text-primary-foreground',
        secondary: 'border-transparent bg-secondary text-secondary-foreground',
        outline: 'text-foreground',
        success: 'border-transparent bg-success text-success-fg',
        warning: 'border-transparent bg-warning text-warning-fg',
        info: 'border-transparent bg-info text-info-fg',
        destructive: 'border-transparent bg-destructive text-destructive-fg',
        butter: 'border-transparent bg-butter text-butter-fg',
      },
    },
    defaultVariants: { variant: 'default' },
  },
);

export interface BadgeProps {
  className?: string;
  variant?: BadgeVariant;
  children?: ReactNode;
}

export function Badge({ className, variant = 'default', children }: BadgeProps) {
  return <span className={cn(badgeVariants({ variant }), className)}>{children}</span>;
}