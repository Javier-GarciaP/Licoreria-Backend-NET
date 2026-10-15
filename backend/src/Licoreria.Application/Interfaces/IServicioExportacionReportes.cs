using Licoreria.Application.Dtos;
using Licoreria.Application.Reportes;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Interfaces;

/// <summary>Filtros de una exportación; cada reporte usa los que le corresponden.</summary>
public sealed record FiltrosExportacion(
    DateTime? Desde = null,
    DateTime? Hasta = null,
    Guid? UsuarioId = null,
    OrdenProductos Orden = OrdenProductos.Unidades,
    int Limite = 10,
    string? Entidad = null,
    string? Accion = null,
    TipoAuditoria? Tipo = null);

/// <summary>Genera la descarga de un reporte en PDF o Excel con los mismos filtros de la consulta.</summary>
public interface IServicioExportacionReportes
{
    Task<ArchivoExportado> ExportarAsync(
        string reporte,
        string formato,
        FiltrosExportacion filtros,
        CancellationToken cancellationToken = default);
}
