using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Services;
using Licoreria.Domain.Entities;
using Moq;

namespace Licoreria.UnitTests;

/// <summary>Los eventos de acceso se registran sin contraseña y sin revelar si el correo existe.</summary>
public class ServicioAutenticacionAuditoriaTests
{
    private readonly Mock<IUsuarioRepository> _usuarios = new();
    private readonly Mock<IRefreshTokenRepository> _tokens = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly Mock<IRelojSistema> _reloj = new();
    private readonly Mock<IServicioAuditoria> _auditoria = new();
    private readonly Usuario _usuario = new() { NombreCompleto = "Ana", Email = "ana@test.com", PasswordHash = "hash" };

    public ServicioAutenticacionAuditoriaTests()
    {
        _reloj.SetupGet(r => r.UtcNow).Returns(DateTime.UtcNow);
        _tokenService.Setup(t => t.GenerarToken(It.IsAny<Usuario>())).Returns(("jwt", DateTime.UtcNow.AddHours(1)));
        _tokenService.Setup(t => t.GenerarTokenRefresco()).Returns("refresh");
        _usuarios.Setup(u => u.ObtenerPorEmailAsync("ana@test.com", It.IsAny<CancellationToken>())).ReturnsAsync(_usuario);
        _usuarios.Setup(u => u.GetByIdAsync(_usuario.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_usuario);
    }

    private ServicioAutenticacion Crear()
        => new(_usuarios.Object, _tokens.Object, _hasher.Object, _tokenService.Object, _reloj.Object,
            Mock.Of<IContextoUsuario>(), new OpcionesAutenticacion(), _auditoria.Object);

    [Fact]
    public async Task InicioDeSesionCorrecto_registra_el_evento_con_el_usuario()
    {
        _hasher.Setup(h => h.Verify("clave", "hash")).Returns(true);

        var respuesta = await Crear().IniciarSesionAsync(new LoginDto("ana@test.com", "clave"));

        Assert.NotNull(respuesta);
        _auditoria.Verify(a => a.RegistrarAccesoAsync("iniciar-sesion", _usuario.Id, "ana@test.com", It.IsAny<object?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IntentoFallido_registra_el_correo_y_nunca_la_contrasena()
    {
        _hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
        object? datos = null;
        _auditoria
            .Setup(a => a.RegistrarAccesoAsync("intento-fallido", null, "ana@test.com", It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .Callback<string, Guid?, string?, object?, CancellationToken>((_, _, _, d, _) => datos = d)
            .Returns(Task.CompletedTask);

        var respuesta = await Crear().IniciarSesionAsync(new LoginDto("ana@test.com", "SECRETA"));

        Assert.Null(respuesta);
        _auditoria.Verify(a => a.RegistrarAccesoAsync("intento-fallido", null, "ana@test.com", It.IsAny<object?>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.DoesNotContain("SECRETA", System.Text.Json.JsonSerializer.Serialize(datos));
    }

    [Fact]
    public async Task CorreoInexistente_registra_el_mismo_intento_fallido_y_la_misma_respuesta()
    {
        var respuesta = await Crear().IniciarSesionAsync(new LoginDto("nadie@test.com", "clave"));

        Assert.Null(respuesta);
        _auditoria.Verify(a => a.RegistrarAccesoAsync("intento-fallido", null, "nadie@test.com", It.IsAny<object?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CerrarSesion_registra_el_evento_del_usuario_del_token()
    {
        var token = new RefreshToken(_usuario.Id, "refresh", DateTime.UtcNow.AddDays(1));
        _tokens.Setup(t => t.ObtenerPorTokenAsync("refresh", It.IsAny<CancellationToken>())).ReturnsAsync(token);

        Assert.True(await Crear().CerrarSesionAsync("refresh"));

        _auditoria.Verify(a => a.RegistrarAccesoAsync("cerrar-sesion", _usuario.Id, "ana@test.com", It.IsAny<object?>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
