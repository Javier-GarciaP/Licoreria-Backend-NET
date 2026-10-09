import { motion } from 'framer-motion';
import { CalendarDays, X } from 'lucide-react';
import { Pill } from '@licoreria/ui';
import type { Evento } from '@licoreria/types';
import { urlDeImagen } from '../../lib/imagenProducto';
import { formatDateTime } from '../../lib/format';
import { EstadoEvento } from './estado';

function Metrica({ etiqueta, valor }: { etiqueta: string; valor: string }) {
  return (
    <div className="rounded-inner border border-border bg-muted/30 px-3 py-2">
      <p className="text-[11px] uppercase tracking-tighter2 text-muted-foreground">{etiqueta}</p>
      <p className="num mt-0.5 text-sm text-foreground">{valor}</p>
    </div>
  );
}

/** Panel expandido de evento al hacer clic en una fila (sin recuadro, directo en la fila). */
export function PanelDetalleEvento({
  evento,
  imagenId,
  onCerrar,
}: {
  evento: Evento;
  imagenId: string;
  onCerrar: () => void;
}) {
  const imagen = urlDeImagen(evento.imagenUrl);

  return (
    <div className="flex flex-col gap-5">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <div className="flex flex-wrap items-center gap-2">
          <EstadoEvento evento={evento} />
          {evento.activo ? <Pill tone="success">Activo</Pill> : <Pill tone="danger">Inactivo</Pill>}
        </div>
        <button
          type="button"
          onClick={onCerrar}
          aria-label="Cerrar detalle"
          className="flex h-7 w-7 items-center justify-center rounded-md text-muted-foreground transition hover:bg-accent/10 hover:text-foreground focus:outline-none focus-visible:ring-2 focus-visible:ring-ring/60"
        >
          <X size={14} />
        </button>
      </div>

      <div className="grid grid-cols-1 gap-6 md:grid-cols-[220px_1fr]">
        {imagen ? (
          <motion.img
            layoutId={imagenId}
            src={imagen}
            alt={evento.titulo}
            className="h-[194px] w-full rounded-xl border border-border object-cover md:h-[229px]"
          />
        ) : (
          <motion.div
            layoutId={imagenId}
            className="flex h-[194px] w-full flex-col items-center justify-center gap-2 rounded-xl border border-dashed border-border bg-muted/30 text-sm text-muted-foreground md:h-[229px]"
          >
            <CalendarDays size={20} />
            Sin imagen
          </motion.div>
        )}

        <div className="flex min-w-0 flex-col gap-4">
          {evento.descripcion && <p className="text-sm text-muted-foreground">{evento.descripcion}</p>}

          <div className="grid grid-cols-2 gap-2 sm:grid-cols-3">
            <Metrica etiqueta="Inicio" valor={formatDateTime(evento.fechaInicio)} />
            <Metrica etiqueta="Fin" valor={evento.fechaFin ? formatDateTime(evento.fechaFin) : '—'} />
            <Metrica etiqueta="Publicado" valor={evento.publicado ? 'Sí' : 'No'} />
          </div>
        </div>
      </div>
    </div>
  );
}