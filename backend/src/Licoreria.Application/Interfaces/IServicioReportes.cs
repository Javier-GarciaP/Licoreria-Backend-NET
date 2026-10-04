using Licoreria.Application.Dtos;

namespace Licoreria.Application.Interfaces;

/// <summary>
/// Casos de uso de reportes y tableros.
/// </summary>
public interface IServicioReportes
{
    Task<DashboardDto> ObtenerDashboardAsync(CancellationToken cancellationToken = default);
}
