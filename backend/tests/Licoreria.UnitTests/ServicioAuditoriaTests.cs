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

    [Fact]
    public void Registrar_guarda_usuario_correo_ip_y_serializa_los_datos()
    {
        var usuarioId = Guid.NewGuid();
        var ahora = new DateTime(2026, 10, 14, 15, 30, 0, DateTimeKind.Utc);
        _contexto.SetupGet(c => c.UsuarioId).Returns(usuarioId);
        _contexto.SetupGet(c => c.Email).Returns("admin@test.com");
        _contexto.SetupGet(c => c.Ip).Returns("203.0.113.7");
        _reloj.SetupGet(r => r.UtcNow).Returns(ahora);
        var servicio = new ServicioAuditoria(_repositorio.Object, _contexto.Object, _reloj.Object);
        AuditLog? agregado = null;
        _repositorio.Setup(r => r.Agregar(It.IsAny<AuditLog>())).Callback<AuditLog>(l => agregado = l);
        var entidadId = Guid.NewGuid();

        servicio.Registrar("cerrar", "sesion-caja", entidadId, new { esperado = 10, motivo = "turno" });

        Assert.Equal(usuarioId, agregado!.UsuarioId);
        Assert.Equal("admin@test.com", agregado.Usuario);
        Assert.Equal("203.0.113.7", agregado.Ip);
        Assert.Equal("cerrar", agregado.Accion);
        Assert.Equal("sesion-caja", agregado.Entidad);
        Assert.Equal(entidadId, agregado.EntidadId);
        Assert.Equal("""{"esperado":10,"motivo":"turno"}""", agregado.Datos);
        Assert.Equal(ahora, agregado.CreatedAt);
        Assert.Equal(usuarioId, agregado.CreatedBy);
    }

    [Fact]
    public void Registrar_sin_datos_deja_la_serializacion_vacia()
    {
        AuditLog? agregado = null;
        _repositorio.Setup(r => r.Agregar(It.IsAny<AuditLog>())).Callback<AuditLog>(l => agregado = l);

        Crear().Registrar("ver", "reporte");

        Assert.Null(agregado!.Datos);
        Assert.Null(agregado.EntidadId);
    }

    [Fact]
    public void Registrar_sin_usuario_autenticado_deja_el_actor_vacio_pero_conserva_la_ip()
    {
        _contexto.SetupGet(c => c.UsuarioId).Returns((Guid?)null);
        _contexto.SetupGet(c => c.Email).Returns((string?)null);
        _contexto.SetupGet(c => c.Ip).Returns("198.51.100.1");
        _reloj.SetupGet(r => r.UtcNow).Returns(DateTime.UtcNow);
        var servicio = new ServicioAuditoria(_repositorio.Object, _contexto.Object, _reloj.Object);
        AuditLog? agregado = null;
        _repositorio.Setup(r => r.Agregar(It.IsAny<AuditLog>())).Callback<AuditLog>(l => agregado = l);

        servicio.Registrar("intento", "sesion");

        Assert.Null(agregado!.UsuarioId);
        Assert.Null(agregado.Usuario);
        Assert.Equal("198.51.100.1", agregado.Ip);
    }

    [Fact]
    public async Task ObtenerAsync_sin_filtros_pasa_nulos_al_repositorio_y_devuelve_la_pagina_mapeada()
    {
        var paginacion = new Licoreria.Application.Common.PaginacionRequest { Page = 2, PageSize = 10 };
        var log = new AuditLog { Accion = "crear", Entidad = "Producto", Usuario = "a@test.com", Ip = "10.0.0.1", Tipo = TipoAuditoria.Evento };
        _repositorio
            .Setup(r => r.ObtenerPaginadoAsync(paginacion, null, null, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Licoreria.Application.Common.ResultadoPaginado<AuditLog>.Crear([log], 2, 10, 11));

        var resultado = await Crear().ObtenerAsync(paginacion);

        var item = Assert.Single(resultado.Items);
        Assert.Equal("crear", item.Accion);
        Assert.Equal("a@test.com", item.Usuario);
        Assert.Equal("Evento", item.Tipo);
        Assert.Equal(11, resultado.TotalItems);
        Assert.Equal(2, resultado.Page);
    }

    [Fact]
    public async Task ObtenerAsync_con_entidad_y_rango_pasa_ambos_filtros_al_repositorio()
    {
        var desde = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);
        var hasta = new DateTime(2026, 10, 31, 0, 0, 0, DateTimeKind.Utc);
        var paginacion = new Licoreria.Application.Common.PaginacionRequest();
        _repositorio
            .Setup(r => r.ObtenerPaginadoAsync(paginacion, "Venta", null, desde, hasta, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Licoreria.Application.Common.ResultadoPaginado<AuditLog>.Crear([], 1, 20, 0));

        await Crear().ObtenerAsync(paginacion, entidad: "Venta", desde: desde, hasta: hasta);

        _repositorio.Verify(
            r => r.ObtenerPaginadoAsync(paginacion, "Venta", null, desde, hasta, null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ObtenerAsync_convierte_las_fechas_locales_a_utc()
    {
        DateTime? recibido = null;
        _repositorio
            .Setup(r => r.ObtenerPaginadoAsync(It.IsAny<Licoreria.Application.Common.PaginacionRequest>(), null, null, null, It.IsAny<DateTime?>(), null, null, It.IsAny<CancellationToken>()))
            .Callback<Licoreria.Application.Common.PaginacionRequest, string?, Guid?, DateTime?, DateTime?, string?, TipoAuditoria?, CancellationToken>((_, _, _, _, h, _, _, _) => recibido = h)
            .ReturnsAsync(Licoreria.Application.Common.ResultadoPaginado<AuditLog>.Crear([], 1, 20, 0));
        var local = new DateTime(2026, 10, 14, 8, 0, 0, DateTimeKind.Local);

        await Crear().ObtenerAsync(new Licoreria.Application.Common.PaginacionRequest(), hasta: local);

        Assert.Equal(DateTimeKind.Utc, recibido!.Value.Kind);
        Assert.Equal(local.ToUniversalTime(), recibido.Value);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_existente_devuelve_el_dto()
    {
        var log = new AuditLog { Accion = "editar", Entidad = "Producto", Tipo = TipoAuditoria.Cambio };
        _repositorio.Setup(r => r.ObtenerPorIdAsync(log.Id, It.IsAny<CancellationToken>())).ReturnsAsync(log);

        var dto = await Crear().ObtenerPorIdAsync(log.Id);

        Assert.Equal(log.Id, dto!.Id);
        Assert.Equal("Cambio", dto.Tipo);
    }

    [Fact]
    public async Task ObtenerEntidadesAsync_devuelve_lo_que_entrega_el_repositorio()
    {
        _repositorio.Setup(r => r.ObtenerEntidadesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(["Producto", "Venta"]);

        var entidades = await Crear().ObtenerEntidadesAsync();

        Assert.Equal(["Producto", "Venta"], entidades);
    }
}
