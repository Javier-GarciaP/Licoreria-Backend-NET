import { cn } from '@licoreria/ui';
import { PERIODOS, type PeriodoId, type PeriodoResuelto } from '../lib/periodo';

interface Props {
  periodo: PeriodoResuelto;
  onElegir: (id: PeriodoId) => void;
  onRango: (desde: string, hasta: string) => void;
  className?: string;
}

const CAMPO = 'h-9 rounded-control border border-border bg-card px-3 text-sm text-foreground';

/** Periodos predefinidos y rango personalizado. Un rango inválido muestra el motivo y no se consulta. */
export function SelectorPeriodo({ periodo, onElegir, onRango, className }: Props) {
  return (
    <div className={cn('flex flex-col gap-2', className)}>
      <div role="group" aria-label="Periodo" className="flex flex-wrap gap-2">
        {PERIODOS.map((item) => (
          <button
            key={item.id}
            type="button"
            aria-pressed={periodo.id === item.id}
            onClick={() => onElegir(item.id)}
            className={cn(
              'rounded-full border px-3.5 py-1.5 text-sm transition',
              periodo.id === item.id
                ? 'border-primary text-foreground'
                : 'border-border text-muted-foreground hover:text-foreground',
            )}
          >
            {item.label}
          </button>
        ))}
      </div>

      {periodo.id === 'personalizado' && (
        <div className="flex flex-wrap items-center gap-2">
          <label className="flex items-center gap-2 text-xs text-muted-foreground">
            Desde
            <input
              type="date"
              aria-label="Fecha inicial"
              value={periodo.desde}
              onChange={(evento) => onRango(evento.target.value, periodo.hasta)}
              className={CAMPO}
            />
          </label>
          <label className="flex items-center gap-2 text-xs text-muted-foreground">
            Hasta
            <input
              type="date"
              aria-label="Fecha final"
              value={periodo.hasta}
              onChange={(evento) => onRango(periodo.desde, evento.target.value)}
              className={CAMPO}
            />
          </label>
        </div>
      )}

      {!periodo.valido && periodo.motivo && (
        <p role="alert" className="text-xs text-danger">
          {periodo.motivo}
        </p>
      )}
    </div>
  );
}
