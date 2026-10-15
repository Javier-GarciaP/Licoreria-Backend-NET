using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Services;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Licoreria.UnitTests.Support;
using Moq;

namespace Licoreria.UnitTests;

/// <summary>
/// Inicio de sesión, refresco con rotación, cierre de sesión y usuario actual de
/// <see cref="ServicioAutenticacion"/>, aislado con Moq. La auditoría de accesos tiene su propio archivo.
/// </summary>
public sealed class ServicioAutenticacionTests
{
    private const string TokenAcceso = "jwt-firmado";
    private static readonly DateTime ExpiraAcceso = RelojFijo.FechaPorDefecto.AddMinutes(15);

    private readonly Mock<IUsuarioRepository> _usuarios = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokens = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly Mock<IRelojSistema> _reloj = RelojFijo.Crear();
    private readonly Mock<IServicioAuditoria> _auditoria = new();
    private Mock<IContextoUsuario> _contexto = ContextoUsuarioFalso.Anonimo();
    private readonly OpcionesAutenticacion _opciones = new() { RefreshExpiresDays = 7 };

    private readonly Usuario _usuario = new()
    {
        NombreCompleto = "Ana Pérez",
        Email = "ana@test.com",
        PasswordHash = "hash-guardado",
        Rol = RolUsuario.Cajero
    };

    public ServicioAutenticacionTests()
    {
        _tokenService.Setup(t => t.GenerarToken(It.IsAny<Usuario>())).Returns((TokenAcceso, ExpiraAcceso));
        _tokenService.Setup(t => t.GenerarTokenRefresco()).Returns("refresh-nuevo");
        _usuarios.Setup(u => u.ObtenerPorEmailAsync("ana@test.com", It.IsAny<CancellationToken>())).ReturnsAsync(_usuario);
        _usuarios.Setup(u => u.GetByIdAsync(_usuario.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_usuario);
        _hasher.Setup(h => h.Verify("clave-correcta", "hash-guardado")).Returns(true);
    }

    private ServicioAutenticacion CrearServicio()
        => new(_usuarios.Object, _refreshTokens.Object, _hasher.Object, _tokenService.Object, _reloj.Object,
            _contexto.Object, _opciones, _auditoria.Object);

    private RefreshToken ConToken(string valor, DateTime? expira = null, bool revocado = false)
    {
        var token = new RefreshToken(_usuario.Id, valor, expira ?? RelojFijo.FechaPorDefecto.AddDays(1));
        if (revocado)
        {
            token.Revocar(RelojFijo.FechaPorDefecto.AddHours(-1));
        }

        _refreshTokens.Setup(r => r.ObtenerPorTokenAsync(valor, It.IsAny<CancellationToken>())).ReturnsAsync(token);
        return token;
    }

    // ======================= Iniciar sesión =======================

    [Fact]
    public async Task IniciarSesion_ConCredencialesValidas_DevuelveTokensPermisosYGuardaElRefresco()
    {
        RefreshToken? guardado = null;
        _refreshTokens
            .Setup(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Callback<RefreshToken, CancellationToken>((t, _) => guardado = t)
            .Returns(Task.CompletedTask);

        var respuesta = await CrearServicio().IniciarSesionAsync(new LoginDto("ana@test.com", "clave-correcta"));

        Assert.NotNull(respuesta);
        Assert.Equal(TokenAcceso, respuesta!.AccessToken);
        Assert.Equal("refresh-nuevo", respuesta.RefreshToken);
        Assert.Equal(ExpiraAcceso, respuesta.ExpiraEn);
        Assert.Equal(_usuario.Id, respuesta.UsuarioId);
        Assert.Equal("Cajero", respuesta.RolDominio);
        Assert.NotEmpty(respuesta.Permisos);
        Assert.NotNull(guardado);
        Assert.Equal(_usuario.Id, guardado!.UsuarioId);
        Assert.Equal("refresh-nuevo", guardado.Token);
        Assert.Equal(RelojFijo.FechaPorDefecto.AddDays(7), guardado.ExpiraEn);
        _refreshTokens.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IniciarSesion_ConContrasenaIncorrecta_NoDevuelveTokensNiGuardaNada()
    {
        var respuesta = await CrearServicio().IniciarSesionAsync(new LoginDto("ana@test.com", "mala"));

        Assert.Null(respuesta);
        _tokenService.Verify(t => t.GenerarToken(It.IsAny<Usuario>()), Times.Never);
        _refreshTokens.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task IniciarSesion_UsuarioInactivo_NoDevuelveTokensAunqueLaClaveSeaCorrecta()
    {
        _usuario.Desactivar();

        var respuesta = await CrearServicio().IniciarSesionAsync(new LoginDto("ana@test.com", "clave-correcta"));

        Assert.Null(respuesta);
        _tokenService.Verify(t => t.GenerarToken(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task IniciarSesion_FalloPorCorreoInexistenteContrasenaMalaOUsuarioInactivo_RegistraElMismoEventoSinRevelarLaCausa()
    {
        _usuario.Desactivar();
        var servicio = CrearServicio();

        await servicio.IniciarSesionAsync(new LoginDto("nadie@test.com", "x"));
        await servicio.IniciarSesionAsync(new LoginDto("ana@test.com", "clave-correcta"));

        _auditoria.Verify(
            a => a.RegistrarAccesoAsync("intento-fallido", null, It.IsAny<string?>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        _auditoria.Verify(
            a => a.RegistrarAccesoAsync("iniciar-sesion", It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task IniciarSesion_ConCorreoInexistente_NoVerificaNingunaContrasena()
    {
        var respuesta = await CrearServicio().IniciarSesionAsync(new LoginDto("nadie@test.com", "x"));

        Assert.Null(respuesta);
        _hasher.Verify(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    // ======================= Refrescar =======================

    [Fact]
    public async Task Refrescar_ConTokenVigente_RotaElTokenRevocaElViejoYEmiteUnoNuevo()
    {
        var viejo = ConToken("refresh-viejo");
        RefreshToken? nuevo = null;
        _refreshTokens
            .Setup(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Callback<RefreshToken, CancellationToken>((t, _) => nuevo = t)
            .Returns(Task.CompletedTask);

        var respuesta = await CrearServicio().RefrescarAsync("refresh-viejo");

        Assert.NotNull(respuesta);
        Assert.Equal("refresh-nuevo", respuesta!.RefreshToken);
        Assert.True(viejo.Revocado);
        Assert.Equal(RelojFijo.FechaPorDefecto, viejo.RevocadoEn);
        _refreshTokens.Verify(r => r.Update(viejo), Times.Once);
        Assert.NotNull(nuevo);
        Assert.NotEqual(viejo.Token, nuevo!.Token);
        Assert.False(nuevo.Revocado);
    }

    [Fact]
    public async Task Refrescar_ConTokenRevocado_NoDevuelveTokens()
    {
        ConToken("refresh-revocado", revocado: true);

        var respuesta = await CrearServicio().RefrescarAsync("refresh-revocado");

        Assert.Null(respuesta);
        _tokenService.Verify(t => t.GenerarToken(It.IsAny<Usuario>()), Times.Never);
        _refreshTokens.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Refrescar_ConTokenVencido_NoDevuelveTokens()
    {
        ConToken("refresh-vencido", expira: RelojFijo.FechaPorDefecto.AddSeconds(-1));

        Assert.Null(await CrearServicio().RefrescarAsync("refresh-vencido"));
    }

    [Fact]
    public async Task Refrescar_ConTokenQueVenceJustoAhora_NoEsVigente()
    {
        ConToken("refresh-limite", expira: RelojFijo.FechaPorDefecto);

        Assert.Null(await CrearServicio().RefrescarAsync("refresh-limite"));
    }

    [Fact]
    public async Task Refrescar_ConTokenInexistente_NoDevuelveTokens()
        => Assert.Null(await CrearServicio().RefrescarAsync("no-existe"));

    [Fact]
    public async Task Refrescar_ConUsuarioDesactivadoDespuesDeEmitirElToken_NoDevuelveTokensYNoLoRevoca()
    {
        var token = ConToken("refresh-vigente");
        _usuario.Desactivar();

        var respuesta = await CrearServicio().RefrescarAsync("refresh-vigente");

        Assert.Null(respuesta);
        Assert.False(token.Revocado);
    }

    [Fact]
    public async Task Refrescar_ConUsuarioInexistente_NoDevuelveTokens()
    {
        var huerfano = new RefreshToken(Guid.NewGuid(), "refresh-huerfano", RelojFijo.FechaPorDefecto.AddDays(1));
        _refreshTokens.Setup(r => r.ObtenerPorTokenAsync("refresh-huerfano", It.IsAny<CancellationToken>())).ReturnsAsync(huerfano);

        Assert.Null(await CrearServicio().RefrescarAsync("refresh-huerfano"));
    }

    [Fact]
    public async Task Refrescar_ElTokenViejoNoSirveDosVeces()
    {
        ConToken("refresh-viejo");
        var servicio = CrearServicio();

        Assert.NotNull(await servicio.RefrescarAsync("refresh-viejo"));
        Assert.Null(await servicio.RefrescarAsync("refresh-viejo"));
    }

    // ======================= Cerrar sesión =======================

    [Fact]
    public async Task CerrarSesion_ConTokenExistente_LoRevocaYGuarda()
    {
        var token = ConToken("refresh-activo");

        Assert.True(await CrearServicio().CerrarSesionAsync("refresh-activo"));

        Assert.True(token.Revocado);
        _refreshTokens.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CerrarSesion_ConTokenInexistente_DevuelveFalsoSinAuditar()
    {
        Assert.False(await CrearServicio().CerrarSesionAsync("no-existe"));

        _auditoria.Verify(
            a => a.RegistrarAccesoAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CerrarSesion_ConTokenYaRevocado_SigueSiendoCorrecto()
    {
        ConToken("refresh-revocado", revocado: true);

        Assert.True(await CrearServicio().CerrarSesionAsync("refresh-revocado"));
    }

    // ======================= Usuario actual =======================

    [Fact]
    public async Task ObtenerUsuarioActual_SinSesion_DevuelveNulo()
        => Assert.Null(await CrearServicio().ObtenerUsuarioActualAsync());

    [Fact]
    public async Task ObtenerUsuarioActual_ConSesionValida_DevuelveSusDatosYPermisos()
    {
        _contexto = ContextoUsuarioFalso.Crear(usuarioId: _usuario.Id);

        var actual = await CrearServicio().ObtenerUsuarioActualAsync();

        Assert.NotNull(actual);
        Assert.Equal(_usuario.Id, actual!.Id);
        Assert.Equal("ana@test.com", actual.Email);
        Assert.Equal("Cajero", actual.RolDominio);
        Assert.NotEmpty(actual.Permisos);
    }

    [Fact]
    public async Task ObtenerUsuarioActual_ConUsuarioInactivoOInexistente_DevuelveNulo()
    {
        _contexto = ContextoUsuarioFalso.Crear(usuarioId: Guid.NewGuid());
        Assert.Null(await CrearServicio().ObtenerUsuarioActualAsync());

        _contexto = ContextoUsuarioFalso.Crear(usuarioId: _usuario.Id);
        _usuario.Desactivar();
        Assert.Null(await CrearServicio().ObtenerUsuarioActualAsync());
    }
}
