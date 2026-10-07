import { Button } from './Button';

export interface PaginationProps {
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}

export function Pagination({ page, totalPages, onPageChange }: PaginationProps) {
  if (totalPages <= 1) return null;

  const next = Math.min(page + 1, totalPages);
  const prev = Math.max(page - 1, 1);

  return (
    <div className="flex items-center justify-between gap-4 pt-4">
      <span className="text-xs text-muted">
        Página {page} de {totalPages}
      </span>
      <div className="flex gap-2">
        <Button variant="ghost" size="sm" disabled={page <= 1} onClick={() => onPageChange(prev)}>
          Anterior
        </Button>
        <Button variant="ghost" size="sm" disabled={page >= totalPages} onClick={() => onPageChange(next)}>
          Siguiente
        </Button>
      </div>
    </div>
  );
}
