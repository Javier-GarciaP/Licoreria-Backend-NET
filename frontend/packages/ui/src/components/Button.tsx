import type { ButtonHTMLAttributes, ReactNode } from 'react';
import { cn } from '../lib/cn';

type Variant = 'primary' | 'ghost' | 'subtle' | 'danger';
type Size = 'sm' | 'md' | 'lg' | 'icon';

const variants: Record<Variant, string> = {
  primary:
    'bg-accent text-on-pastel shadow-glow hover:brightness-105 active:brightness-95',
  ghost:
    'bg-transparent text-ink border border-hairline hover:bg-elevated',
  subtle:
    'bg-elevated text-ink hover:brightness-110',
  danger:
    'bg-danger text-on-pastel hover:brightness-105',
};

const sizes: Record<Size, string> = {
  sm: 'h-8 px-4 text-xs',
  md: 'h-10 px-5 text-sm',
  lg: 'h-12 px-6 text-base',
  icon: 'h-10 w-10',
};

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant;
  size?: Size;
  loading?: boolean;
  leftIcon?: ReactNode;
}

export function Button({
  variant = 'primary',
  size = 'md',
  loading = false,
  leftIcon,
  className,
  children,
  disabled,
  ...props
}: ButtonProps) {
  return (
    <button
      className={cn(
        'inline-flex items-center justify-center gap-2 rounded-pill font-medium transition',
        'focus:outline-none focus-visible:ring-2 focus-visible:ring-accent/60',
        'disabled:cursor-not-allowed disabled:opacity-50',
        variants[variant],
        sizes[size],
        className,
      )}
      disabled={disabled || loading}
      {...props}
    >
      {loading ? <span className="h-4 w-4 animate-spin rounded-full border-2 border-white/40 border-t-white" /> : leftIcon}
      {children}
    </button>
  );
}
