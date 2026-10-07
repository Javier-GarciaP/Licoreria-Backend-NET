import type { ReactNode } from 'react';
import { cn } from '../lib/cn';

export interface PillProps {
  children: ReactNode;
  className?: string;
  tone?: 'neutral' | 'accent' | 'success' | 'warning' | 'danger' | 'info';
}

const tones: Record<NonNullable<PillProps['tone']>, string> = {
  neutral: 'bg-elevated text-muted',
  accent: 'bg-accent/15 text-accent-soft',
  success: 'bg-success/15 text-success',
  warning: 'bg-warning/15 text-warning',
  danger: 'bg-danger/15 text-danger',
  info: 'bg-info/15 text-info',
};

export function Pill({ children, className, tone = 'neutral' }: PillProps) {
  return (
    <span
      className={cn(
        'inline-flex items-center gap-1.5 rounded-pill px-3 py-1 text-xs font-medium',
        tones[tone],
        className,
      )}
    >
      {children}
    </span>
  );
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
