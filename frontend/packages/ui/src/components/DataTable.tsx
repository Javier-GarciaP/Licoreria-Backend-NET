import { useState, type ReactNode } from 'react';
import { ChevronDown, ChevronRight } from 'lucide-react';
import { cn } from '../lib/cn';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from './ui/table';
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
      <div className="rounded-xl border border-border bg-card px-6 py-10 text-center text-sm text-muted-foreground">
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
    <div className="overflow-x-auto rounded-xl border border-border">
      <Table className="min-w-[640px]">
        <TableHeader>
          <TableRow>
            {conExpandible && <TableHead className="w-10 px-2" />}
            {columns.map((column) => (
              <TableHead key={column.key} className={cn('whitespace-nowrap', alignClass(column.align))}>
                {column.header}
              </TableHead>
            ))}
          </TableRow>
        </TableHeader>
        <TableBody>
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
        </TableBody>
      </Table>
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
      <TableRow
        onClick={onRowClick ? () => onRowClick(row) : undefined}
        className={cn(
          onRowClick && 'cursor-pointer',
          expandida && 'bg-muted/30',
        )}
      >
        {conExpandible && (
          <TableCell className="px-2">
            <button
              type="button"
              aria-label={expandida ? 'Contraer' : 'Expandir'}
              aria-expanded={expandida}
              onClick={(evento) => {
                evento.stopPropagation();
                alternar(clave);
              }}
              className="flex h-7 w-7 items-center justify-center rounded-md text-muted-foreground transition hover:bg-accent/10 hover:text-foreground focus:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
            >
              {expandida ? <ChevronDown className="h-4 w-4" /> : <ChevronRight className="h-4 w-4" />}
            </button>
          </TableCell>
        )}
        {columns.map((column) => (
          <TableCell
            key={column.key}
            className={cn(
              alignClass(column.align),
              column.align === 'right' && 'num',
              column.className,
            )}
          >
            {column.render(row)}
          </TableCell>
        ))}
      </TableRow>
      {expandida && expandedRow && (
        <TableRow className="border-0 bg-muted/10">
          <TableCell colSpan={columns.length + 1} className="px-5 py-4">
            {expandedRow(row)}
          </TableCell>
        </TableRow>
      )}
    </>
  );
}