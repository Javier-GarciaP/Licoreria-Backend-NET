import { useState, type ReactNode } from 'react';
import { ChevronDown, ChevronRight } from 'lucide-react';
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
  /** Contenido de una fila expandible (formulario inline, detalle…). */
  expandedRow?: (row: T) => ReactNode;
  /** Expande la primera fila al renderizar (p. ej. tras crear). */
  expandirKey?: string | null;
}

export function DataTable<T>({
  columns,
  rows,
  loading = false,
  empty = 'Sin registros.',
  rowKey,
  onRowClick,
  expandedRow,
  expandirKey,
}: DataTableProps<T>) {
  const [expandidas, setExpandidas] = useState<Set<string>>(new Set());

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

  const conExpandible = Boolean(expandedRow);
  const esExpandible = (clave: string) => conExpandible && (expandidas.has(clave) || clave === expandirKey);

  const alternar = (clave: string) =>
    setExpandidas((actuales) => {
      const siguiente = new Set(actuales);
      if (siguiente.has(clave)) siguiente.delete(clave);
      else siguiente.add(clave);
      return siguiente;
    });

  const alignClass = (align?: Column<T>['align']) =>
    align === 'right' ? 'text-right' : align === 'center' ? 'text-center' : 'text-left';

  return (
    <div className="overflow-x-auto rounded-2xl border border-hairline">
      <table className="w-full min-w-[640px] border-collapse text-sm">
        <thead>
          <tr className="bg-elevated/60">
            {conExpandible && <th scope="col" className="w-10 px-2 py-3" />}
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
          {rows.map((row) => {
            const clave = rowKey(row);
            const expandida = esExpandible(clave);
            return (
              <FragmentoFila
                key={clave}
                row={row}
                clave={clave}
                columns={columns}
                onRowClick={onRowClick}
                conExpandible={conExpandible}
                expandida={expandida}
                alternar={alternar}
                expandedRow={expandedRow}
              />
            );
          })}
        </tbody>
      </table>
    </div>
  );
}

function FragmentoFila<T>({
  row,
  clave,
  columns,
  onRowClick,
  conExpandible,
  expandida,
  alternar,
  expandedRow,
}: {
  row: T;
  clave: string;
  columns: Column<T>[];
  onRowClick?: (row: T) => void;
  conExpandible: boolean;
  expandida: boolean;
  alternar: (clave: string) => void;
  expandedRow?: (row: T) => ReactNode;
}) {
  const alignClass = (align?: Column<T>['align']) =>
    align === 'right' ? 'text-right' : align === 'center' ? 'text-center' : 'text-left';

  return (
    <>
      <tr
        onClick={onRowClick ? () => onRowClick(row) : undefined}
        className={cn(
          'border-t border-hairline transition',
          onRowClick && 'cursor-pointer hover:bg-elevated/40',
          expandida && 'bg-elevated/30',
        )}
      >
        {conExpandible && (
          <td className="px-2 py-3">
            <button
              type="button"
              aria-label={expandida ? 'Contraer' : 'Expandir'}
              aria-expanded={expandida}
              onClick={(evento) => {
                evento.stopPropagation();
                alternar(clave);
              }}
              className="flex h-7 w-7 items-center justify-center rounded-full text-muted transition hover:bg-ink/5 hover:text-ink focus:outline-none focus-visible:ring-2 focus-visible:ring-accent/60"
            >
              {expandida ? <ChevronDown size={15} /> : <ChevronRight size={15} />}
            </button>
          </td>
        )}
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
      {expandida && expandedRow && (
        <tr className="border-t-0">
          <td colSpan={columns.length + 1} className="bg-elevated/20 px-5 py-4">
            {expandedRow(row)}
          </td>
        </tr>
      )}
    </>
  );
}