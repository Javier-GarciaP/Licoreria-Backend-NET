import type { ReactNode } from 'react';
import { cn } from '../../lib/cn';

export interface EmptyProps {
  icon?: ReactNode;
  title: string;
  description?: string;
  action?: ReactNode;
  className?: string;
}

export function Empty({ icon, title, description, action, className }: EmptyProps) {
  return (
    <div className={cn('flex flex-col items-center justify-center gap-4 rounded-lg border border-dashed py-16 text-center', className)}>
      {icon && (
        <div className="flex h-14 w-14 items-center justify-center rounded-full bg-muted text-muted-foreground">
          {icon}
        </div>
      )}
      <div className="flex max-w-md flex-col gap-1.5">
        <p className="text-lg font-medium text-foreground">{title}</p>
        {description && <p className="text-sm text-muted-foreground">{description}</p>}
      </div>
      {action}
    </div>
  );
}