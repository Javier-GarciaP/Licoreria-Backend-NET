import { useQuery } from '@tanstack/react-query';
import { catalogoApi } from '@licoreria/api-client';
import type { NaturalezaVariante } from '@licoreria/types';
import { formatNumber } from '../../lib/format';

/** Orígenes de una porción: cuántas porciones da cada presentación al destaparse. */
function OrigenesVariante({ varianteId }: { varianteId: string }) {
  const origenes = useQuery({ queryKey: ['origenes', varianteId], queryFn: () => catalogoApi.origenes(varianteId) });
  const items = origenes.data ?? [];
  if (items.length === 0) return <span className="text-xs text-muted-foreground">—</span>;
  return (
    <span className="text-xs text-muted-foreground">
      Porción · {items.map((origen) => `${origen.porcionesPorUnidad} por ${origen.presentacionNombre}`).join(' · ')}
    </span>
  );
}

/** Consumo de una variante derivada (qué y cuánto consume al venderse). */
export function ConsumoVariante({ varianteId, naturaleza }: { varianteId: string; naturaleza?: NaturalezaVariante }) {
  const recetas = useQuery({
    queryKey: ['recetas', varianteId],
    queryFn: () => catalogoApi.recetas(varianteId),
    enabled: naturaleza !== 'Porcion',
  });
  if (naturaleza === 'Porcion') return <OrigenesVariante varianteId={varianteId} />;
  const items = recetas.data ?? [];
  if (items.length === 0) return <span className="text-xs text-muted-foreground">—</span>;
  return (
    <span className="text-xs text-muted-foreground">
      {naturaleza === 'Receta' && 'Receta heredada · '}
      {items.map((receta) => `${formatNumber(receta.cantidad)} ${receta.varianteInsumoNombre}`).join(' + ')}
    </span>
  );
}
