using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;

namespace Licoreria.Application.Services;

public sealed class ServicioReportes : IServicioReportes
{
    private readonly IReportesRepository _reportes;
    private readonly IRelojSistema _reloj;

    public ServicioReportes(IReportesRepository reportes, IRelojSistema reloj)
    {
        _reportes = reportes;
        _reloj = reloj;
    }

    public Task<DashboardDto> ObtenerDashboardAsync(CancellationToken cancellationToken = default)
    {
        var ahora = _reloj.UtcNow;
        var inicioDia = ahora.Date;
        var inicioMes = new DateTime(ahora.Year, ahora.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        return _reportes.ObtenerDashboardAsync(inicioDia, inicioMes, ahora, cancellationToken);
    }
}
