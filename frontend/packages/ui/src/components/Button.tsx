import type { ButtonHTMLAttributes, ReactNode } from 'react';
import { cn } from '../lib/cn';
import { Button as ButtonPrimitivo } from './ui/button';
import { Spinner } from './ui/spinner';

type Variant = 'primary' | 'ghost' | 'subtle' | 'danger';
type Size = 'sm' | 'md' | 'lg' | 'icon';

const mapVariant: Record<Variant, 'default' | 'outline' | 'secondary' | 'destructive'> = {
  primary: 'default',
  ghost: 'outline',
  subtle: 'secondary',
  danger: 'destructive',
};

const mapSize: Record<Size, 'sm' | 'default' | 'lg' | 'icon'> = {
  sm: 'sm',
  md: 'default',
  lg: 'lg',
  icon: 'icon',
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
    <ButtonPrimitivo
      variant={mapVariant[variant]}
      size={mapSize[size]}
      disabled={disabled || loading}
      className={className}
      {...props}
    >
      {loading ? <Spinner size="sm" className="h-4 w-4" /> : leftIcon}
      {children}
    </ButtonPrimitivo>
  );
}