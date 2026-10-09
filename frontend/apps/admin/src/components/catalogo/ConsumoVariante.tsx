import { useQuery } from '@tanstack/react-query';
import { catalogoApi } from '@licoreria/api-client';
import { formatNumber } from '../../lib/format';

/** Consumo de una variante derivada (qué y cuánto consume al venderse). */
export function ConsumoVariante({ varianteId }: { varianteId: string }) {
  const recetas = useQuery({ queryKey: ['recetas', varianteId], queryFn: () => catalogoApi.recetas(varianteId) });
  const items = recetas.data ?? [];
  if (items.length === 0) return <span className="text-xs text-muted-foreground">—</span>;
  return (
    <span className="text-xs text-muted-foreground">
      {items.map((receta) => `${formatNumber(receta.cantidad)} ${receta.varianteInsumoNombre}`).join(' + ')}
    </span>
  );
}