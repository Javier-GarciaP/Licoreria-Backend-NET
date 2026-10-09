import { RotateCcw } from 'lucide-react';
import { Button } from './Button';

export interface LimpiarFiltrosProps {
  /** ¿Hay filtros activos? Deshabilita el botón cuando no los hay. */
  activo: boolean;
  onClick: () => void;
}

/** Botón para limpiar todos los filtros activos de un listado. Patrón estándar de listados. */
export function LimpiarFiltros({ activo, onClick }: LimpiarFiltrosProps) {
  return (
    <Button variant="ghost" size="sm" leftIcon={<RotateCcw size={15} />} disabled={!activo} onClick={onClick}>
      Limpiar
    </Button>
  );
}