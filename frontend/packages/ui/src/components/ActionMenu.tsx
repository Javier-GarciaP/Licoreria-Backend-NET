import type { ReactNode } from 'react';
import { MoreHorizontal } from 'lucide-react';
import { cn } from '../lib/cn';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuPortal,
  DropdownMenuTrigger,
} from './ui/dropdown-menu';

export interface ActionMenuOption {
  label: string;
  icon?: ReactNode;
  onClick: () => void;
  danger?: boolean;
  disabled?: boolean;
}

/** Menú de acciones "…" alineado al botón (Radix DropdownMenu). */
export function ActionMenu({
  options,
  label,
  className,
}: {
  options: ActionMenuOption[];
  /** Etiqueta accesible del botón. */
  label: string;
  className?: string;
}) {
  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <button
          type="button"
          aria-label={label}
          className={cn(
            'flex h-8 w-8 items-center justify-center rounded-md text-muted-foreground transition',
            'hover:bg-accent/10 hover:text-foreground',
            'focus:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2',
            className,
          )}
        >
          <MoreHorizontal className="h-4 w-4" />
        </button>
      </DropdownMenuTrigger>
      <DropdownMenuPortal>
        <DropdownMenuContent
          align="end"
          sideOffset={6}
          className="z-50 min-w-[11rem] rounded-lg border border-border bg-popover p-1.5 shadow-md"
        >
          {options.map((opcion) => (
            <DropdownMenuItem
              key={opcion.label}
              disabled={opcion.disabled}
              onSelect={() => opcion.onClick()}
              className={cn(
                'relative flex cursor-default select-none items-center gap-2.5 rounded-md px-3 py-2 text-sm outline-none',
                'focus:bg-accent focus:text-accent-foreground',
                'data-[disabled]:pointer-events-none data-[disabled]:opacity-50',
                opcion.danger
                  ? 'text-destructive-fg hover:bg-destructive/15'
                  : 'text-foreground hover:bg-accent/10',
              )}
            >
              {opcion.icon && <span className="shrink-0 text-current opacity-80">{opcion.icon}</span>}
              <span className="flex-1">{opcion.label}</span>
            </DropdownMenuItem>
          ))}
        </DropdownMenuContent>
      </DropdownMenuPortal>
    </DropdownMenu>
  );
}