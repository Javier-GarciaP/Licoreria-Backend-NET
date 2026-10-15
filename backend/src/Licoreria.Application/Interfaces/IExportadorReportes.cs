using Licoreria.Application.Reportes;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Puerto que convierte un documento de reporte en un archivo. Cada formato tiene su implementación.
/// </summary>
public interface IExportadorReportes
{
    FormatoExportacion Formato { get; }

    string TipoContenido { get; }

    string Extension { get; }

    byte[] Exportar(DocumentoReporte documento);
}
