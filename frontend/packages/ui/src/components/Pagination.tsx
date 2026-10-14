import { Pagination as PaginationPrimitivo } from './ui/pagination';

export interface PaginationProps {
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  className?: string;
}

export function Pagination({ page, totalPages, onPageChange, className }: PaginationProps) {
  return <PaginationPrimitivo page={page} totalPages={totalPages} onPageChange={onPageChange} className={className} />;
}

export type { PaginationProps as PaginationPropsUI } from './ui/pagination';