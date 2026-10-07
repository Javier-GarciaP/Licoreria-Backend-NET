import type { ReactNode } from 'react';
import { cn } from '../lib/cn';
import { SkeletonTable } from './Skeleton';

export interface Column<T> {
  key: string;
  header: string;
  render: (row: T) => ReactNode;
  align?: 'left' | 'center' | 'right';
  className?: string;
}

export interface DataTableProps<T> {
  columns: Column<T>[];
  rows: T[];
  loading?: boolean;
  empty?: ReactNode;
  rowKey: (row: T) => string;
  onRowClick?: (row: T) => void;
}

export function DataTable<T>({
  columns,
  rows,
  loading = false,
  empty = 'Sin registros.',
  rowKey,
  onRowClick,
}: DataTableProps<T>) {
  if (loading) {
    return <SkeletonTable />;
  }

  if (rows.length === 0) {
    return (
      <div className="rounded-2xl border border-hairline bg-surface px-6 py-10 text-center text-sm text-muted">
        {empty}
      </div>
    );
  }

  const alignClass = (align?: Column<T>['align']) =>
    align === 'right' ? 'text-right' : align === 'center' ? 'text-center' : 'text-left';

  return (
    <div className="overflow-x-auto rounded-2xl border border-hairline">
      <table className="w-full min-w-[640px] border-collapse text-sm">
        <thead>
          <tr className="bg-elevated/60">
            {columns.map((column) => (
              <th
                key={column.key}
                scope="col"
                className={cn(
                  'whitespace-nowrap px-4 py-3 text-xs font-medium uppercase tracking-tighter2 text-muted',
                  alignClass(column.align),
                )}
              >
                {column.header}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr
              key={rowKey(row)}
              onClick={onRowClick ? () => onRowClick(row) : undefined}
              className={cn(
                'border-t border-hairline transition',
                onRowClick && 'cursor-pointer hover:bg-elevated/40',
              )}
            >
              {columns.map((column) => (
                <td
                  key={column.key}
                  className={cn(
                    'px-4 py-3 text-ink',
                    alignClass(column.align),
                    column.align === 'right' && 'num',
                    column.className,
                  )}
                >
                  {column.render(row)}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
