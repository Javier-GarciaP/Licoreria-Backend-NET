using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Services;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Licoreria.Domain.Services;
using Moq;

namespace Licoreria.UnitTests;

/// <summary>
/// Los eventos de negocio se agregan con <c>Registrar</c> ANTES del guardado del cambio (misma
/// transacción) y el cambio se guarda una sola vez; los cambios de usuario no duplican el log automático.
/// </summary>
public class ServicioEventosAuditoriaTests
{
    private readonly Mock<IServicioAuditoria> _auditoria = new();
    private readonly List<string> _orden = new();

    public ServicioEventosAuditoriaTests()
    {
        _auditoria
            .Setup(a => a.Registrar(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<object?>()))
            .Callback<string, string, Guid?, object?>((accion, _, _, _) => _orden.Add("evento:" + accion));
    }

    // ===== Usuarios =====

    private readonly Mock<IUsuarioRepository> _usuarios = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IRefreshTokenRepository> _tokens = new();

    private ServicioUsuarios CrearServicioUsuarios()
    {
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hash");
        _usuarios.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => _orden.Add("guardar")).ReturnsAsync(1);
        _tokens.Setup(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => _orden.Add("guardar")).ReturnsAsync(1);
        var reloj = new Mock<IRelojSistema>();
        reloj.SetupGet(r => r.UtcNow).Returns(DateTime.UtcNow);
        return new ServicioUsuarios(_usuarios.Object, _hasher.Object, _tokens.Object, _auditoria.Object, reloj.Object);
    }

    private Usuario UsuarioExistente()
    {
        var usuario = new Usuario { NombreCompleto = "Ana", Email = "ana@test.com", PasswordHash = "x" };
        _usuarios.Setup(u => u.GetByIdAsync(usuario.Id, It.IsAny<CancellationToken>())).ReturnsAsync(usuario);
        return usuario;
    }

    [Fact]
    public async Task CambiarPassword_registra_el_evento_antes_de_guardar_una_sola_vez()
    {
        var usuario = UsuarioExistente();

        await CrearServicioUsuarios().CambiarPasswordAsync(usuario.Id, new CambiarPasswordDto("nueva-clave"));

        Assert.Equal(new[] { "evento:cambiar-password", "guardar" }, _orden);
    }

    [Fact]
    public async Task RevocarSesiones_registra_el_evento_antes_de_guardar_una_sola_vez()
    {
        var usuario = UsuarioExistente();

        await CrearServicioUsuarios().RevocarSesionesAsync(usuario.Id);

        Assert.Equal(new[] { "evento:revocar-sesiones", "guardar" }, _orden);
    }

    [Fact]
    public async Task Crear_editar_y_eliminar_usuario_no_registran_eventos_duplicados()
    {
        var servicio = CrearServicioUsuarios();
        var usuario = UsuarioExistente();
        _usuarios.Setup(u => u.ExisteEmailAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await servicio.CrearUsuarioAsync(new UsuarioCrearDto("Luis", "luis@test.com", "clave-segura", RolUsuario.Cajero));
        await servicio.EditarUsuarioAsync(new UsuarioEditarDto(usuario.Id, "Ana M.", "ana@test.com", RolUsuario.Cajero, true));
        await servicio.EliminarUsuarioAsync(usuario.Id);

        _auditoria.Verify(a => a.Registrar(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<object?>()), Times.Never);
        _auditoria.Verify(a => a.RegistrarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ===== Inventario: toma física =====

    [Fact]
    public async Task TomaFisica_registra_el_evento_antes_de_guardar_una_sola_vez()
    {
        var varianteId = Guid.NewGuid();
        var inventario = new Mock<IInventarioRepository>();
        var variantes = new Mock<IRepository<ProductoVariante>>();
        var contexto = new Mock<IContextoUsuario>();
        var reloj = new Mock<IRelojSistema>();
        contexto.SetupGet(c => c.UsuarioId).Returns(Guid.NewGuid());
        reloj.SetupGet(r => r.UtcNow).Returns(DateTime.UtcNow);
        variantes.Setup(v => v.GetByIdAsync(varianteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductoVariante { Nombre = "Ron" });
        inventario.Setup(i => i.ObtenerStockAsync(varianteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StockProducto { VarianteId = varianteId });
        inventario.Setup(i => i.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => _orden.Add("guardar")).ReturnsAsync(1);
        TomaFisica? toma = null;
        inventario.Setup(i => i.AgregarTomaAsync(It.IsAny<TomaFisica>(), It.IsAny<CancellationToken>()))
            .Callback<TomaFisica, CancellationToken>((t, _) => toma = t).Returns(Task.CompletedTask);
        inventario.Setup(i => i.ObtenerTomaConDetalleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => toma);

        var servicio = new ServicioInventario(
            inventario.Object, variantes.Object, Mock.Of<IRepository<Lote>>(),
            contexto.Object, reloj.Object, _auditoria.Object, new EvaluadorMerma());

        await servicio.RegistrarTomaFisicaAsync(new RegistrarTomaFisicaDto([new TomaFisicaDetalleCrearDto(varianteId, 0m)]));

        Assert.Equal(new[] { "evento:registrar", "guardar" }, _orden);
    }
}
