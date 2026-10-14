using Licoreria.Application.Interfaces;
using Licoreria.Application.Services;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Moq;

namespace Licoreria.UnitTests;

public class ServicioAuditoriaTests
{
    private readonly Mock<IAuditoriaRepository> _repositorio = new();
    private readonly Mock<IContextoUsuario> _contexto = new();
    private readonly Mock<IRelojSistema> _reloj = new();

    private ServicioAuditoria Crear()
    {
        _contexto.SetupGet(c => c.UsuarioId).Returns(Guid.NewGuid());
        _contexto.SetupGet(c => c.Email).Returns("admin@test.com");
        _contexto.SetupGet(c => c.Ip).Returns("10.0.0.9");
        _reloj.SetupGet(r => r.UtcNow).Returns(DateTime.UtcNow);
        return new ServicioAuditoria(_repositorio.Object, _contexto.Object, _reloj.Object);
    }

    [Fact]
    public void Registrar_agrega_un_evento_sin_guardar()
    {
        AuditLog? agregado = null;
        _repositorio.Setup(r => r.Agregar(It.IsAny<AuditLog>())).Callback<AuditLog>(l => agregado = l);

        Crear().Registrar("cerrar", "sesion-caja", Guid.NewGuid(), new { esperado = 10 });

        Assert.NotNull(agregado);
        Assert.Equal(TipoAuditoria.Evento, agregado!.Tipo);
        Assert.Equal("admin@test.com", agregado.Usuario);
        _repositorio.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegistrarAsync_agrega_el_evento_y_guarda()
    {
        AuditLog? agregado = null;
        _repositorio.Setup(r => r.Agregar(It.IsAny<AuditLog>())).Callback<AuditLog>(l => agregado = l);

        await Crear().RegistrarAsync("login", "sesion");

        Assert.Equal(TipoAuditoria.Evento, agregado!.Tipo);
        _repositorio.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarAccesoAsync_usa_el_usuario_indicado_y_guarda()
    {
        AuditLog? agregado = null;
        _repositorio.Setup(r => r.Agregar(It.IsAny<AuditLog>())).Callback<AuditLog>(l => agregado = l);
        var usuarioId = Guid.NewGuid();

        await Crear().RegistrarAccesoAsync("iniciar-sesion", usuarioId, "ana@test.com");

        Assert.Equal(usuarioId, agregado!.UsuarioId);
        Assert.Equal("ana@test.com", agregado.Usuario);
        Assert.Equal("sesion", agregado.Entidad);
        Assert.Equal(TipoAuditoria.Evento, agregado.Tipo);
        _repositorio.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObtenerAsync_pasa_todos_los_filtros_al_repositorio_y_mapea_el_tipo()
    {
        var usuarioId = Guid.NewGuid();
        var desde = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);
        var hasta = new DateTime(2026, 10, 14, 0, 0, 0, DateTimeKind.Utc);
        var log = new AuditLog { Accion = "editar", Entidad = "Producto", Tipo = TipoAuditoria.Cambio };
        var paginacion = new Licoreria.Application.Common.PaginacionRequest();
        _repositorio
            .Setup(r => r.ObtenerPaginadoAsync(paginacion, "Producto", usuarioId, desde, hasta, "editar", TipoAuditoria.Cambio, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Licoreria.Application.Common.ResultadoPaginado<AuditLog>.Crear([log], 1, 20, 1));

        var resultado = await Crear().ObtenerAsync(paginacion, "Producto", usuarioId, desde, hasta, "editar", TipoAuditoria.Cambio);

        Assert.Equal("Cambio", Assert.Single(resultado.Items).Tipo);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_devuelve_nulo_si_no_existe()
    {
        _repositorio.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((AuditLog?)null);

        Assert.Null(await Crear().ObtenerPorIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ObtenerAsync_interpreta_las_fechas_sin_zona_como_utc()
    {
        DateTime? recibido = null;
        _repositorio
            .Setup(r => r.ObtenerPaginadoAsync(It.IsAny<Licoreria.Application.Common.PaginacionRequest>(), null, null, It.IsAny<DateTime?>(), null, null, null, It.IsAny<CancellationToken>()))
            .Callback<Licoreria.Application.Common.PaginacionRequest, string?, Guid?, DateTime?, DateTime?, string?, TipoAuditoria?, CancellationToken>((_, _, _, d, _, _, _, _) => recibido = d)
            .ReturnsAsync(Licoreria.Application.Common.ResultadoPaginado<AuditLog>.Crear([], 1, 20, 0));

        await Crear().ObtenerAsync(new Licoreria.Application.Common.PaginacionRequest(), desde: new DateTime(2026, 10, 14));

        Assert.Equal(DateTimeKind.Utc, recibido!.Value.Kind);
    }
}
