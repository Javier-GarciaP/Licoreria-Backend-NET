import { ChevronLeft, ChevronRight } from 'lucide-react';
import { cn } from '../../lib/cn';

export interface PaginationProps {
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  className?: string;
}

function PaginationButton({ disabled, onClick, label, icon }: {
  disabled: boolean;
  onClick: () => void;
  label: string;
  icon: 'prev' | 'next';
}) {
  return (
    <button
      type="button"
      disabled={disabled}
      onClick={onClick}
      aria-label={label}
      className={cn(
        'inline-flex h-8 w-8 items-center justify-center rounded-md text-sm font-medium',
        'text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground',
        'disabled:pointer-events-none disabled:opacity-40',
        'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2',
      )}
    >
      {icon === 'prev' ? <ChevronLeft className="h-4 w-4" /> : <ChevronRight className="h-4 w-4" />}
    </button>
  );
}

export function Pagination({ page, totalPages, onPageChange, className }: PaginationProps) {
  if (totalPages <= 1) return null;

  const next = Math.min(page + 1, totalPages);
  const prev = Math.max(page - 1, 1);

  return (
    <div className={cn('flex flex-wrap items-center justify-between gap-4 pt-4', className)}>
      <p className="text-sm text-muted-foreground">
        Página <span className="num">{page}</span> de <span className="num">{totalPages}</span>
      </p>
      <div className="flex items-center gap-1">
        <PaginationButton disabled={page <= 1} onClick={() => onPageChange(prev)} label="Página anterior" icon="prev" />
        <PaginationButton
          disabled={page >= totalPages}
          onClick={() => onPageChange(next)}
          label="Página siguiente"
          icon="next"
        />
      </div>
    </div>
  );
}