import { useState } from 'react';
import { FileSpreadsheet, FileText } from 'lucide-react';
import { Button } from '@licoreria/ui';
import { reportesApi } from '@licoreria/api-client';
import { avisarError } from '@licoreria/auth';
import type { FormatoExportacion, OrdenProductos, ReporteExportableId, TipoAuditoria } from '@licoreria/types';
import { guardarArchivo } from '../lib/descargar';

export interface FiltrosExportar {
  desde?: string;
  hasta?: string;
  orden?: OrdenProductos;
  limite?: number;
  /** Filtros de la bitácora de auditoría. */
  usuarioId?: string;
  entidad?: string;
  accion?: string;
  tipo?: TipoAuditoria;
}

interface Props {
  reporte: ReporteExportableId;
  filtros?: FiltrosExportar;
  /** Sin datos que exportar, o un periodo inválido. */
  deshabilitado?: boolean;
}

const FORMATOS: { formato: FormatoExportacion; etiqueta: string; icono: typeof FileText }[] = [
  { formato: 'pdf', etiqueta: 'PDF', icono: FileText },
  { formato: 'xlsx', etiqueta: 'Excel', icono: FileSpreadsheet },
];

/** Descarga el reporte visible en PDF o Excel con los mismos filtros de la pantalla. */
export function BotonesExportar({ reporte, filtros, deshabilitado = false }: Props) {
  const [descargando, setDescargando] = useState<FormatoExportacion | null>(null);

  const exportar = async (formato: FormatoExportacion) => {
    setDescargando(formato);
    try {
      const archivo = await reportesApi.exportar(reporte, formato, filtros);
      guardarArchivo(archivo.blob, archivo.nombre ?? `reporte-${reporte}.${formato}`);
    } catch (error) {
      avisarError('No se pudo exportar el reporte', error);
    } finally {
      setDescargando(null);
    }
  };

  return (
    <div className="flex items-center gap-2" role="group" aria-label="Exportar reporte">
      {FORMATOS.map(({ formato, etiqueta, icono: Icono }) => (
        <Button
          key={formato}
          type="button"
          variant="subtle"
          size="sm"
          disabled={deshabilitado || descargando !== null}
          onClick={() => void exportar(formato)}
          aria-label={`Exportar ${etiqueta}`}
          leftIcon={<Icono size={14} aria-hidden />}
        >
          {descargando === formato ? 'Generando…' : etiqueta}
        </Button>
      ))}
    </div>
  );
}
