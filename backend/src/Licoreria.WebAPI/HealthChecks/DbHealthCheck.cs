using Licoreria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Licoreria.WebAPI.HealthChecks;

/// <summary>
/// Verifica que la conexión a PostgreSQL esté disponible.
/// </summary>
public sealed class DbHealthCheck : IHealthCheck
{
    private readonly LicoreriaDbContext _context;

    public DbHealthCheck(LicoreriaDbContext context) => _context = context;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var puedeConectar = await _context.Database.CanConnectAsync(cancellationToken);
            return puedeConectar
                ? HealthCheckResult.Healthy("La base de datos responde.")
                : HealthCheckResult.Unhealthy("La base de datos no responde.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Error al conectar con la base de datos.", ex);
        }
    }
}
