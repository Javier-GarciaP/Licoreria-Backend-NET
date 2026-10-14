import { useQuery } from '@tanstack/react-query';
import { cajaApi } from '@licoreria/api-client';

/**
 * Estado del turno (caja) para el mesonero/barra/cocina. Usa el endpoint ligero
 * `GET /sesiones-caja/turno` (solo `sales:read`), sin permisos de caja.
 */
export function useTurno() {
  const turno = useQuery({
    queryKey: ['turno'],
    queryFn: cajaApi.turno,
    staleTime: 30_000,
  });

  return {
    turnoAbierto: turno.data?.abierto ?? false,
    abiertaEn: turno.data?.abiertaEn ?? null,
    cargando: turno.isLoading,
    turno,
  };
}