import { ChevronRight } from 'lucide-react';
import { Fragment } from 'react';
import { cn } from '../../lib/cn';

export interface BreadcrumbItem {
  label: string;
  href?: string;
}

export function Breadcrumb({ items, className }: { items: BreadcrumbItem[]; className?: string }) {
  return (
    <nav aria-label="Migas de pan" className={cn('flex items-center gap-1.5 text-sm', className)}>
      {items.map((item, indice) => (
        <Fragment key={item.label}>
          {indice > 0 && <ChevronRight className="h-3.5 w-3.5 text-muted-foreground" />}
          {item.href ? (
            <a
              href={item.href}
              className="rounded-md text-muted-foreground transition-colors hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
            >
              {item.label}
            </a>
          ) : (
            <span className="font-medium text-foreground">{item.label}</span>
          )}
        </Fragment>
      ))}
    </nav>
  );
}