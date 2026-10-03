using Licoreria.Application.Interfaces;

namespace Licoreria.Infrastructure.Services;

/// <summary>
/// Reloj del sistema basado en la hora UTC real.
/// </summary>
public sealed class RelojSistema : IRelojSistema
{
    public DateTime UtcNow => DateTime.UtcNow;
}
