import type { ReactNode } from 'react';
import { cn } from '../lib/cn';
import { Empty } from './ui/empty';
import { Spinner as SpinnerPrimitivo } from './ui/spinner';

export function PageHeader({
  title,
  subtitle,
  actions,
  className,
}: {
  title: string;
  subtitle?: string;
  actions?: ReactNode;
  className?: string;
}) {
  return (
    <header className={cn('flex flex-wrap items-end justify-between gap-4', className)}>
      <div>
        <h1 className="text-xl font-medium tracking-tightest text-foreground">{title}</h1>
        {subtitle && <p className="mt-1 text-sm text-muted-foreground">{subtitle}</p>}
      </div>
      {actions && <div className="flex flex-wrap items-center gap-2">{actions}</div>}
    </header>
  );
}

export function EmptyState({ title, description, action }: { title: string; description?: string; action?: ReactNode }) {
  return <Empty title={title} description={description} action={action} />;
}

export function Spinner() {
  return <SpinnerPrimitivo />;
}