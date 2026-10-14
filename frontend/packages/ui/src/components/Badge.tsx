import type { ReactNode } from 'react';
import { cn } from '../lib/cn';
import { Badge as BadgePrimitivo, type BadgeVariant } from './ui/badge';

export interface PillProps {
  children: ReactNode;
  className?: string;
  tone?: 'neutral' | 'accent' | 'success' | 'warning' | 'danger' | 'info' | 'butter';
}

const toneVariant: Record<NonNullable<PillProps['tone']>, BadgeVariant> = {
  neutral: 'secondary',
  accent: 'default',
  success: 'success',
  warning: 'warning',
  danger: 'destructive',
  info: 'info',
  butter: 'butter',
};

export function Pill({ children, className, tone = 'neutral' }: PillProps) {
  return <BadgePrimitivo variant={toneVariant[tone]} className={cn('px-3 py-1', className)}>{children}</BadgePrimitivo>;
}

const stateTone: Record<string, NonNullable<PillProps['tone']>> = {
  Libre: 'success',
  Ocupada: 'danger',
  Reservada: 'warning',
  'En limpieza': 'info',
  Recibido: 'info',
  Preparado: 'success',
  Entregado: 'neutral',
  Cancelado: 'danger',
  Pendiente: 'warning',
  Confirmada: 'success',
  Asistio: 'success',
  NoAsistio: 'danger',
  Cancelada: 'danger',
  Abierta: 'info',
  PorCobrar: 'warning',
  Cerrada: 'neutral',
  Optimo: 'success',
  Subabastecido: 'warning',
  RiesgoCritico: 'danger',
  SinStock: 'danger',
  Sobreabastecido: 'warning',
  Excesivo: 'danger',
};

export function StatusBadge({ status, className }: { status: string; className?: string }) {
  return (
    <Pill tone={stateTone[status] ?? 'neutral'} className={className}>
      {status}
    </Pill>
  );
}