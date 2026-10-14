import { Pill } from '@licoreria/ui';
import type { Evento } from '@licoreria/types';

export type Vigencia = 'Proximo' | 'EnCurso' | 'Pasado';

export function vigenciaDe(evento: Evento): Vigencia {
  const ahora = Date.now();
  const inicio = new Date(evento.fechaInicio).getTime();
  const fin = evento.fechaFin ? new Date(evento.fechaFin).getTime() : null;
  if (fin !== null && fin < ahora) return 'Pasado';
  if (inicio <= ahora) return 'EnCurso';
  return 'Proximo';
}

export function EstadoEvento({ evento }: { evento: Evento }) {
  if (!evento.publicado) return <Pill tone="warning">No publicado</Pill>;
  const vigencia = vigenciaDe(evento);
  return (
    <Pill tone={vigencia === 'Pasado' ? 'neutral' : vigencia === 'EnCurso' ? 'success' : 'info'}>
      {vigencia === 'Proximo' ? 'Próximo' : vigencia === 'EnCurso' ? 'En curso' : 'Pasado'}
    </Pill>
  );
}