import type { ReactNode } from 'react';
import { cn } from '../lib/cn';

export interface PillProps {
  children: ReactNode;
  className?: string;
  tone?: 'neutral' | 'accent' | 'success' | 'warning' | 'danger' | 'info' | 'butter';
}

const tones: Record<NonNullable<PillProps['tone']>, string> = {
  neutral: 'bg-elevated text-muted',
  accent: 'bg-accent/25 text-accent-ink',
  success: 'bg-success/25 text-success-ink',
  warning: 'bg-warning/25 text-warning-ink',
  danger: 'bg-danger/25 text-danger-ink',
  info: 'bg-info/25 text-info-ink',
  butter: 'bg-butter/25 text-butter-ink',
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
