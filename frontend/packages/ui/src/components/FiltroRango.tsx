import { Button } from './Button';
import { Input } from './Input';
import { FiltroPopover } from './FiltroPopover';

export interface FiltroRangoProps {
  /** Etiqueta del botón (p. ej. "Total USD", "Precio"). */
  label: string;
  minimo?: string;
  maximo?: string;
  onMinimo?: (valor: string) => void;
  onMaximo?: (valor: string) => void;
  step?: string;
}

/** Botón de filtro que abre un popover con dos inputs (Mín/Máx). Patrón estándar de listados. */
export function FiltroRango({
  label,
  minimo = '',
  maximo = '',
  onMinimo,
  onMaximo,
  step = '0.01',
}: FiltroRangoProps) {
  const valor = minimo || maximo ? `${minimo || '0'} – ${maximo || '∞'}` : '';

  const limpiar = () => {
    onMinimo?.('');
    onMaximo?.('');
  };

  return (
    <FiltroPopover label={label} valor={valor}>
      <div className="flex flex-col gap-3">
        <Input
          label="Mínimo"
          type="number"
          step={step}
          value={minimo}
          onChange={(evento) => onMinimo?.(evento.target.value)}
        />
        <Input
          label="Máximo"
          type="number"
          step={step}
          value={maximo}
          onChange={(evento) => onMaximo?.(evento.target.value)}
        />
        {(minimo || maximo) && (
          <div className="mt-1 flex justify-end">
            <Button variant="ghost" size="sm" onClick={limpiar}>
              Limpiar
            </Button>
          </div>
        )}
      </div>
    </FiltroPopover>
  );
}