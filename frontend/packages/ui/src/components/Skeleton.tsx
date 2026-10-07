import { cn } from '../lib/cn';

export function Skeleton({ className }: { className?: string }) {
  return <div className={cn('animate-pulse rounded-2xl bg-elevated', className)} />;
}

export function SkeletonCard() {
  return (
    <div className="rounded-card bg-surface p-5 shadow-card">
      <Skeleton className="h-3 w-24" />
      <Skeleton className="mt-4 h-8 w-32" />
      <Skeleton className="mt-3 h-3 w-40" />
    </div>
  );
}

export function SkeletonTable({ rows = 6 }: { rows?: number }) {
  return (
    <div className="flex flex-col gap-2">
      {Array.from({ length: rows }).map((_, index) => (
        <Skeleton key={index} className="h-11 w-full" />
      ))}
    </div>
  );
}
