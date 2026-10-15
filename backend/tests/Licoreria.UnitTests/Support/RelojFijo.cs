using Licoreria.Application.Interfaces;
using Moq;

namespace Licoreria.UnitTests.Support;

/// <summary>Reloj del sistema con una fecha fija, para que las pruebas no dependan de la hora real.</summary>
public static class RelojFijo
{
    public static readonly DateTime FechaPorDefecto = new(2026, 10, 14, 12, 0, 0, DateTimeKind.Utc);

    public static Mock<IRelojSistema> Crear(DateTime? ahora = null)
    {
        var reloj = new Mock<IRelojSistema>();
        reloj.SetupGet(r => r.UtcNow).Returns(ahora ?? FechaPorDefecto);
        return reloj;
    }
}
