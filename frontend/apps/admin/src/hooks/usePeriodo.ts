import { useCallback, useMemo } from 'react';
import { useSearchParams } from 'react-router-dom';
import { fechaLocal, leerPeriodo, type PeriodoId, type PeriodoResuelto } from '../lib/periodo';

export interface UsePeriodo extends PeriodoResuelto {
  elegir: (id: PeriodoId) => void;
  /** Fija el rango personalizado; se guarda aunque sea inválido para que la interfaz explique el motivo. */
  fijarRango: (desde: string, hasta: string) => void;
}

/**
 * Periodo común del Dashboard y de Reportes. Vive en la URL (`?periodo=7d` o
 * `?periodo=personalizado&desde=…&hasta=…`) para compartirse entre páginas y sobrevivir a recargas.
 */
export function usePeriodo(): UsePeriodo {
  const [params, setParams] = useSearchParams();
  const paramsTexto = params.toString();
  const periodo = useMemo(() => leerPeriodo(new URLSearchParams(paramsTexto)), [paramsTexto]);

  const elegir = useCallback(
    (id: PeriodoId) => {
      setParams(
        (previo) => {
          const siguiente = new URLSearchParams(previo);
          siguiente.set('periodo', id);
          if (id === 'personalizado') {
            // Al pasar a personalizado se parte del rango que se estaba viendo.
            if (!siguiente.get('desde')) siguiente.set('desde', periodo.desde || fechaLocal(new Date()));
            if (!siguiente.get('hasta')) siguiente.set('hasta', periodo.hasta || fechaLocal(new Date()));
          } else {
            siguiente.delete('desde');
            siguiente.delete('hasta');
          }
          return siguiente;
        },
        { replace: true },
      );
    },
    [setParams, periodo.desde, periodo.hasta],
  );

  const fijarRango = useCallback(
    (desde: string, hasta: string) => {
      setParams(
        (previo) => {
          const siguiente = new URLSearchParams(previo);
          siguiente.set('periodo', 'personalizado');
          siguiente.set('desde', desde);
          siguiente.set('hasta', hasta);
          return siguiente;
        },
        { replace: true },
      );
    },
    [setParams],
  );

  return { ...periodo, elegir, fijarRango };
}
