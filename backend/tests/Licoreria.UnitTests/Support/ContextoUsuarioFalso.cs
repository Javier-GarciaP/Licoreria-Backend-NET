using Licoreria.Application.Interfaces;
using Moq;

namespace Licoreria.UnitTests.Support;

/// <summary>Usuario autenticado simulado (identificador, correo, IP y rol configurables).</summary>
public static class ContextoUsuarioFalso
{
    public static readonly Guid UsuarioIdPorDefecto = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static Mock<IContextoUsuario> Crear(
        Guid? usuarioId = null,
        string? email = "admin@test.com",
        string? ip = "203.0.113.7",
        string? rol = "Administrador")
    {
        var contexto = new Mock<IContextoUsuario>();
        var id = usuarioId ?? UsuarioIdPorDefecto;
        contexto.SetupGet(c => c.UsuarioId).Returns(id);
        contexto.SetupGet(c => c.Email).Returns(email);
        contexto.SetupGet(c => c.Ip).Returns(ip);
        contexto.SetupGet(c => c.RolDominio).Returns(rol);
        contexto.SetupGet(c => c.EstaAutenticado).Returns(true);
        return contexto;
    }

    /// <summary>Petición sin sesión: no hay usuario ni correo.</summary>
    public static Mock<IContextoUsuario> Anonimo()
    {
        var contexto = new Mock<IContextoUsuario>();
        contexto.SetupGet(c => c.UsuarioId).Returns((Guid?)null);
        contexto.SetupGet(c => c.Email).Returns((string?)null);
        contexto.SetupGet(c => c.Ip).Returns((string?)null);
        contexto.SetupGet(c => c.RolDominio).Returns((string?)null);
        contexto.SetupGet(c => c.EstaAutenticado).Returns(false);
        return contexto;
    }
}
