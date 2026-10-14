using Licoreria.Application.Interfaces;
using Licoreria.Infrastructure.Persistence;
using Licoreria.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Licoreria.UnitTests.Support;

/// <summary>
/// Contexto en memoria con el <see cref="AuditoriaInterceptor"/> activo y un usuario simulado.
/// </summary>
public sealed class ArnesAuditoria : IDisposable
{
    public static readonly Guid UsuarioId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly DateTime Ahora = new(2026, 10, 14, 12, 0, 0, DateTimeKind.Utc);

    private readonly string _baseDatos = Guid.NewGuid().ToString();

    public Mock<IContextoUsuario> Contexto { get; } = new();

    public ArnesAuditoria(bool autenticado = true)
    {
        Contexto.SetupGet(c => c.UsuarioId).Returns(autenticado ? UsuarioId : null);
        Contexto.SetupGet(c => c.Email).Returns(autenticado ? "admin@test.com" : null);
        Contexto.SetupGet(c => c.Ip).Returns("203.0.113.7");
        Contexto.SetupGet(c => c.EstaAutenticado).Returns(autenticado);
    }

    /// <summary>Crea un contexto nuevo sobre la misma base en memoria (sin seguimiento compartido).</summary>
    public LicoreriaDbContext CrearContexto()
    {
        var reloj = new Mock<IRelojSistema>();
        reloj.SetupGet(r => r.UtcNow).Returns(Ahora);

        var opciones = new DbContextOptionsBuilder<LicoreriaDbContext>()
            .UseInMemoryDatabase(_baseDatos)
            .AddInterceptors(new AuditoriaInterceptor(Contexto.Object, reloj.Object))
            .Options;

        return new LicoreriaDbContext(opciones);
    }

    public void Dispose()
    {
    }
}
