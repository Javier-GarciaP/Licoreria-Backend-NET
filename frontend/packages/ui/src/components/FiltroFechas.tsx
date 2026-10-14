import { Button } from './Button';
import { Input } from './Input';
import { FiltroPopover } from './FiltroPopover';

export interface FiltroFechasProps {
  desde?: string;
  hasta?: string;
  onDesde?: (valor: string) => void;
  onHasta?: (valor: string) => void;
}

const formatear = (fecha: string) => {
  if (!fecha) return '';
  const [anio, mes, dia] = fecha.split('-');
  return `${dia}/${mes}/${anio}`;
};

/** Botón de filtro "Fecha" que abre un popover con Desde/Hasta. Patrón estándar de listados. */
export function FiltroFechas({ desde = '', hasta = '', onDesde, onHasta }: FiltroFechasProps) {
  const valor = desde || hasta ? `${formatear(desde) || '…'} – ${formatear(hasta) || '…'}` : '';

  const limpiar = () => {
    onDesde?.('');
    onHasta?.('');
  };

  return (
    <FiltroPopover label="Fecha" valor={valor}>
      <div className="flex flex-col gap-3">
        <Input
          label="Desde"
          type="date"
          value={desde}
          onChange={(evento) => onDesde?.(evento.target.value)}
        />
        <Input
          label="Hasta"
          type="date"
          value={hasta}
          onChange={(evento) => onHasta?.(evento.target.value)}
        />
        {(desde || hasta) && (
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