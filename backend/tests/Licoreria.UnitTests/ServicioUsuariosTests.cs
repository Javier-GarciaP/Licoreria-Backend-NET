using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Services;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Licoreria.UnitTests.Support;
using Moq;

namespace Licoreria.UnitTests;

/// <summary>Alta, edición, baja lógica, contraseñas y sesiones de <see cref="ServicioUsuarios"/>, aislado con Moq.</summary>
public sealed class ServicioUsuariosTests
{
    private readonly Mock<IUsuarioRepository> _usuarios = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokens = new();
    private readonly Mock<IServicioAuditoria> _auditoria = new();
    private readonly Mock<IRelojSistema> _reloj = RelojFijo.Crear();

    public ServicioUsuariosTests()
    {
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns((string p) => $"hash({p})");
    }

    private ServicioUsuarios CrearServicio()
        => new(_usuarios.Object, _hasher.Object, _refreshTokens.Object, _auditoria.Object, _reloj.Object);

    private Usuario ConUsuario(string email = "ana@test.com")
    {
        var usuario = new Usuario { NombreCompleto = "Ana", Email = email, PasswordHash = "hash-viejo", Rol = RolUsuario.Cajero };
        _usuarios.Setup(u => u.GetByIdAsync(usuario.Id, It.IsAny<CancellationToken>())).ReturnsAsync(usuario);
        return usuario;
    }

    // ======================= Crear =======================

    [Fact]
    public async Task Crear_ConCorreoLibre_GuardaElUsuarioConLaContrasenaHasheada()
    {
        Usuario? agregado = null;
        _usuarios
            .Setup(u => u.AddAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
            .Callback<Usuario, CancellationToken>((u, _) => agregado = u)
            .Returns(Task.CompletedTask);

        var dto = await CrearServicio().CrearUsuarioAsync(new UsuarioCrearDto("Ana", "ana@test.com", "secreto", RolUsuario.Mesero));

        Assert.NotNull(agregado);
        Assert.Equal("hash(secreto)", agregado!.PasswordHash);
        Assert.NotEqual("secreto", agregado.PasswordHash);
        Assert.Equal(RolUsuario.Mesero, agregado.Rol);
        Assert.True(agregado.Activo);
        Assert.Equal("Mesero", dto.Rol);
        Assert.Equal("ana@test.com", dto.Email);
        _usuarios.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Crear_Inactivo_RespetaLaBanderaActivo()
    {
        var dto = await CrearServicio().CrearUsuarioAsync(new UsuarioCrearDto("Ana", "ana@test.com", "x", RolUsuario.Cajero, Activo: false));

        Assert.False(dto.Activo);
    }

    [Fact]
    public async Task Crear_ConCorreoDuplicado_FallaConConflictoYNoGuardaNada()
    {
        _usuarios.Setup(u => u.ExisteEmailAsync("ana@test.com", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<ConflictoException>(() =>
            CrearServicio().CrearUsuarioAsync(new UsuarioCrearDto("Ana", "ana@test.com", "x", RolUsuario.Cajero)));

        Assert.Contains("ana@test.com", ex.Message);
        _usuarios.Verify(u => u.AddAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Never);
        _usuarios.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _hasher.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
    }

    // ======================= Editar =======================

    [Fact]
    public async Task Editar_ActualizaLosDatosYElEstado()
    {
        var usuario = ConUsuario();

        var dto = await CrearServicio().EditarUsuarioAsync(
            new UsuarioEditarDto(usuario.Id, "Ana María", "ana.maria@test.com", RolUsuario.Administrador, Activo: false));

        Assert.Equal("Ana María", usuario.NombreCompleto);
        Assert.Equal("ana.maria@test.com", usuario.Email);
        Assert.Equal(RolUsuario.Administrador, usuario.Rol);
        Assert.False(usuario.Activo);
        Assert.False(dto!.Activo);
        _usuarios.Verify(u => u.Update(usuario), Times.Once);
        _usuarios.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Editar_ReactivandoUnUsuarioInactivo_LoDejaActivo()
    {
        var usuario = ConUsuario();
        usuario.Desactivar();

        await CrearServicio().EditarUsuarioAsync(new UsuarioEditarDto(usuario.Id, "Ana", usuario.Email, usuario.Rol, Activo: true));

        Assert.True(usuario.Activo);
    }

    [Fact]
    public async Task Editar_ConservandoSuPropioCorreo_ExcluyeAlUsuarioDeLaVerificacionDeDuplicados()
    {
        var usuario = ConUsuario();

        await CrearServicio().EditarUsuarioAsync(new UsuarioEditarDto(usuario.Id, "Ana", usuario.Email, usuario.Rol, true));

        _usuarios.Verify(u => u.ExisteEmailAsync(usuario.Email, usuario.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Editar_ConElCorreoDeOtroUsuario_FallaConConflicto()
    {
        var usuario = ConUsuario();
        _usuarios.Setup(u => u.ExisteEmailAsync("otro@test.com", usuario.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictoException>(() =>
            CrearServicio().EditarUsuarioAsync(new UsuarioEditarDto(usuario.Id, "Ana", "otro@test.com", usuario.Rol, true)));

        Assert.Equal("ana@test.com", usuario.Email);
        _usuarios.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Editar_NoEncontradoOEliminado_DevuelveNulo()
    {
        var eliminado = ConUsuario();
        eliminado.EliminarLogico();

        Assert.Null(await CrearServicio().EditarUsuarioAsync(new UsuarioEditarDto(Guid.NewGuid(), "x", "x@test.com", RolUsuario.Cajero, true)));
        Assert.Null(await CrearServicio().EditarUsuarioAsync(new UsuarioEditarDto(eliminado.Id, "x", "x@test.com", RolUsuario.Cajero, true)));
        _usuarios.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Editar_ConNombreVacio_PropagaLaValidacionDelDominioSinGuardar()
    {
        var usuario = ConUsuario();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CrearServicio().EditarUsuarioAsync(new UsuarioEditarDto(usuario.Id, " ", "ana@test.com", usuario.Rol, true)));

        _usuarios.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ======================= Eliminar =======================

    [Fact]
    public async Task Eliminar_Existente_HaceBorradoLogicoYGuarda()
    {
        var usuario = ConUsuario();

        Assert.True(await CrearServicio().EliminarUsuarioAsync(usuario.Id));

        Assert.True(usuario.IsDeleted);
        _usuarios.Verify(u => u.Remove(It.IsAny<Usuario>()), Times.Never);
        _usuarios.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Eliminar_NoEncontradoOYaEliminado_DevuelveFalso()
    {
        var eliminado = ConUsuario();
        eliminado.EliminarLogico();

        Assert.False(await CrearServicio().EliminarUsuarioAsync(Guid.NewGuid()));
        Assert.False(await CrearServicio().EliminarUsuarioAsync(eliminado.Id));
        _usuarios.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ======================= Cambiar contraseña =======================

    [Fact]
    public async Task CambiarPassword_GuardaElNuevoHashRevocaSesionesYAuditaSinLaContrasena()
    {
        var usuario = ConUsuario();
        object? datosAuditados = null;
        _auditoria
            .Setup(a => a.Registrar("cambiar-password", "usuario", usuario.Id, It.IsAny<object?>()))
            .Callback<string, string, Guid?, object?>((_, _, _, datos) => datosAuditados = datos);

        var cambiada = await CrearServicio().CambiarPasswordAsync(usuario.Id, new CambiarPasswordDto("nueva-clave"));

        Assert.True(cambiada);
        Assert.Equal("hash(nueva-clave)", usuario.PasswordHash);
        _refreshTokens.Verify(
            r => r.RevocarTodosDelUsuarioAsync(usuario.Id, RelojFijo.FechaPorDefecto, It.IsAny<CancellationToken>()), Times.Once);
        _usuarios.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull(datosAuditados);
        var serializado = System.Text.Json.JsonSerializer.Serialize(datosAuditados);
        Assert.DoesNotContain("nueva-clave", serializado);
        Assert.DoesNotContain("hash(", serializado);
        Assert.DoesNotContain("hash-viejo", serializado);
    }

    [Fact]
    public async Task CambiarPassword_NoEncontradoOEliminado_DevuelveFalsoSinEfectos()
    {
        var eliminado = ConUsuario();
        eliminado.EliminarLogico();

        Assert.False(await CrearServicio().CambiarPasswordAsync(Guid.NewGuid(), new CambiarPasswordDto("x")));
        Assert.False(await CrearServicio().CambiarPasswordAsync(eliminado.Id, new CambiarPasswordDto("x")));
        _refreshTokens.Verify(r => r.RevocarTodosDelUsuarioAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
        _auditoria.Verify(a => a.Registrar(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<object?>()), Times.Never);
    }

    [Fact]
    public async Task CambiarPassword_ConHashVacio_PropagaLaValidacionDelDominioYNoRevocaSesiones()
    {
        var usuario = ConUsuario();
        _hasher.Setup(h => h.Hash("vacia")).Returns(string.Empty);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CrearServicio().CambiarPasswordAsync(usuario.Id, new CambiarPasswordDto("vacia")));

        Assert.Equal("hash-viejo", usuario.PasswordHash);
        _refreshTokens.Verify(r => r.RevocarTodosDelUsuarioAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ======================= Revocar sesiones =======================

    [Fact]
    public async Task RevocarSesiones_Existente_RevocaAuditaYGuardaLosTokens()
    {
        var usuario = ConUsuario();

        Assert.True(await CrearServicio().RevocarSesionesAsync(usuario.Id));

        _refreshTokens.Verify(
            r => r.RevocarTodosDelUsuarioAsync(usuario.Id, RelojFijo.FechaPorDefecto, It.IsAny<CancellationToken>()), Times.Once);
        _auditoria.Verify(a => a.Registrar("revocar-sesiones", "usuario", usuario.Id, It.IsAny<object?>()), Times.Once);
        _refreshTokens.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevocarSesiones_NoEncontradoOEliminado_DevuelveFalso()
    {
        var eliminado = ConUsuario();
        eliminado.EliminarLogico();

        Assert.False(await CrearServicio().RevocarSesionesAsync(Guid.NewGuid()));
        Assert.False(await CrearServicio().RevocarSesionesAsync(eliminado.Id));
        _refreshTokens.Verify(r => r.RevocarTodosDelUsuarioAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ======================= Consultas =======================

    [Fact]
    public async Task ObtenerUsuario_Existente_DevuelveElDto_Y_EliminadoDevuelveNulo()
    {
        var usuario = ConUsuario();

        var dto = await CrearServicio().ObtenerUsuarioAsync(usuario.Id);
        Assert.Equal(usuario.Email, dto!.Email);

        usuario.EliminarLogico();
        Assert.Null(await CrearServicio().ObtenerUsuarioAsync(usuario.Id));
        Assert.Null(await CrearServicio().ObtenerUsuarioAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ObtenerUsuarios_MapeaLaPaginaConLaBusqueda()
    {
        var usuario = ConUsuario();
        var paginacion = new PaginacionRequest { Page = 3, PageSize = 5 };
        _usuarios
            .Setup(u => u.ObtenerPaginadoAsync(paginacion, "ana", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultadoPaginado<Usuario>.Crear([usuario], 3, 5, 11));

        var pagina = await CrearServicio().ObtenerUsuariosAsync(paginacion, "ana");

        Assert.Equal(usuario.Email, Assert.Single(pagina.Items).Email);
        Assert.Equal(11, pagina.TotalItems);
        Assert.Equal(3, pagina.Page);
    }
}
