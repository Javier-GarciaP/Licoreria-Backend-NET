using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Services;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Moq;

namespace Licoreria.UnitTests;

/// <summary>Destapar, listar y descartar envases en <see cref="ServicioEnvases"/>, aislado con Moq.</summary>
public sealed class ServicioEnvasesTests
{
    private static readonly DateTime Ahora = new(2026, 10, 14, 21, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IPorcionRepository> _porciones = new();
    private readonly Mock<IInventarioRepository> _inventario = new();
    private readonly Mock<IServicioKardex> _kardex = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IContextoUsuario> _contexto = new();
    private readonly Mock<IRelojSistema> _reloj = new();
    private readonly Mock<IServicioAuditoria> _auditoria = new();
    private readonly Mock<INotificadorComandas> _notificador = new();

    private readonly ProductoVariante _litro = new() { Nombre = "Botella 1 L", EsBase = true };
    private readonly ProductoVariante _dosLitros = new() { Nombre = "Botella 2 L", EsBase = true };
    private readonly ProductoVariante _vaso = new() { Nombre = "Vaso" };
    private readonly Guid _usuarioId = Guid.NewGuid();

    public ServicioEnvasesTests()
    {
        _reloj.SetupGet(r => r.UtcNow).Returns(Ahora);
        _contexto.SetupGet(c => c.UsuarioId).Returns(_usuarioId);
        _unitOfWork
            .Setup(u => u.EjecutarEnTransaccionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task> operacion, CancellationToken ct) => operacion(ct));
        _inventario
            .Setup(i => i.ObtenerVarianteConRecetasAsync(_vaso.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_vaso);
        _porciones
            .Setup(p => p.ObtenerOrigenesAsync(_vaso.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new PorcionOrigen { PorcionVarianteId = _vaso.Id, PresentacionVarianteId = _litro.Id, PresentacionVariante = _litro, PorcionesPorUnidad = 4 },
                new PorcionOrigen { PorcionVarianteId = _vaso.Id, PresentacionVarianteId = _dosLitros.Id, PresentacionVariante = _dosLitros, PorcionesPorUnidad = 8 }
            ]);
        _porciones
            .Setup(p => p.ObtenerEnvaseAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EnvaseAbierto?)null);
        ConStock(_litro, 5m);
        ConStock(_dosLitros, 5m);
    }

    private ServicioEnvases CrearServicio()
        => new(_porciones.Object, _inventario.Object, _kardex.Object, _unitOfWork.Object, _contexto.Object, _reloj.Object, _auditoria.Object, _notificador.Object);

    private void ConStock(ProductoVariante variante, decimal cantidad, decimal reservada = 0m)
    {
        var stock = new StockProducto { VarianteId = variante.Id };
        if (cantidad > 0) stock.AplicarMovimiento(cantidad);
        if (reservada > 0) stock.Reservar(reservada);
        _inventario.Setup(i => i.ObtenerStockAsync(variante.Id, It.IsAny<CancellationToken>())).ReturnsAsync(stock);
    }

    /* ===================== Destapar ===================== */

    [Fact]
    public async Task Destapar_LaBotellaDeUnLitro_CreaUnEnvaseDeCuatroVasosYRegistraLaApertura()
    {
        EnvaseAbierto? guardado = null;
        _porciones
            .Setup(p => p.AgregarEnvaseAsync(It.IsAny<EnvaseAbierto>(), It.IsAny<CancellationToken>()))
            .Callback<EnvaseAbierto, CancellationToken>((e, _) => guardado = e)
            .Returns(Task.CompletedTask);

        var resultado = await CrearServicio().DestaparAsync(new DestaparEnvaseDto(_vaso.Id, _litro.Id));
        _notificador.Verify(n => n.EnvaseActualizadoAsync(It.IsAny<Guid>(), _vaso.Id, "Abierto", It.IsAny<CancellationToken>()), Times.Once);

        Assert.NotNull(guardado);
        Assert.Equal(4, guardado!.PorcionesIniciales);
        Assert.Equal(4, resultado.PorcionesRestantes);
        Assert.Equal(EstadoEnvase.Abierto, resultado.Estado);
        Assert.Equal(_usuarioId, guardado.AbiertoPorUsuarioId);
        Assert.Equal(Ahora, guardado.AbiertoEn);
        _kardex.Verify(
            k => k.AplicarAsync(_litro.Id, TipoMovimientoInventario.Apertura, -1m, "envase", guardado.Id, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _auditoria.Verify(a => a.Registrar("destapar", "envase", guardado.Id, It.IsAny<object?>()), Times.Once);
    }

    [Fact]
    public async Task Destapar_LaBotellaDeDosLitros_CreaUnEnvaseDeOchoVasos()
    {
        var resultado = await CrearServicio().DestaparAsync(new DestaparEnvaseDto(_vaso.Id, _dosLitros.Id));

        Assert.Equal(8, resultado.PorcionesIniciales);
        _kardex.Verify(
            k => k.AplicarAsync(_dosLitros.Id, TipoMovimientoInventario.Apertura, -1m, "envase", It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _kardex.Verify(
            k => k.AplicarAsync(_litro.Id, It.IsAny<TipoMovimientoInventario>(), It.IsAny<decimal>(), It.IsAny<string?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Destapar_UnaPresentacionSinExistencia_LanzaExistenciaAgotadaYNoCreaNada()
    {
        ConStock(_litro, 0m);

        await Assert.ThrowsAsync<ExistenciaAgotadaException>(
            () => CrearServicio().DestaparAsync(new DestaparEnvaseDto(_vaso.Id, _litro.Id)));

        _porciones.Verify(p => p.AgregarEnvaseAsync(It.IsAny<EnvaseAbierto>(), It.IsAny<CancellationToken>()), Times.Never);
        _kardex.Verify(
            k => k.AplicarAsync(It.IsAny<Guid>(), It.IsAny<TipoMovimientoInventario>(), It.IsAny<decimal>(), It.IsAny<string?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Destapar_LaUltimaBotellaYaReservadaPorOtraOperacion_NoHayDisponible()
    {
        ConStock(_litro, 1m, reservada: 1m);

        await Assert.ThrowsAsync<ExistenciaAgotadaException>(
            () => CrearServicio().DestaparAsync(new DestaparEnvaseDto(_vaso.Id, _litro.Id)));
    }

    [Fact]
    public async Task Destapar_UnaPresentacionQueNoEsOrigenDeLaPorcion_LanzaReglaDeNegocio()
    {
        var otra = new ProductoVariante { Nombre = "Lata" };

        var ex = await Assert.ThrowsAsync<ReglaNegocioException>(
            () => CrearServicio().DestaparAsync(new DestaparEnvaseDto(_vaso.Id, otra.Id)));

        Assert.Contains("origen", ex.Message);
    }

    [Fact]
    public async Task Destapar_UnaVarianteQueNoEsPorcion_LanzaReglaDeNegocio()
    {
        _porciones.Setup(p => p.ObtenerOrigenesAsync(_vaso.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new List<PorcionOrigen>());

        await Assert.ThrowsAsync<ReglaNegocioException>(
            () => CrearServicio().DestaparAsync(new DestaparEnvaseDto(_vaso.Id, _litro.Id)));
    }

    [Fact]
    public async Task Destapar_UnaPorcionInexistente_LanzaNoEncontrado()
    {
        await Assert.ThrowsAsync<NoEncontradoException>(
            () => CrearServicio().DestaparAsync(new DestaparEnvaseDto(Guid.NewGuid(), _litro.Id)));
    }

    /* ===================== Descartar ===================== */

    private EnvaseAbierto EnvaseConRestantes(int restantes, ProductoVariante? presentacion = null)
    {
        var envase = EnvaseAbierto.Destapar(_vaso.Id, (presentacion ?? _litro).Id, 8, _usuarioId, Ahora.AddHours(-1));
        if (restantes < 8) envase.Consumir(8 - restantes, Ahora.AddMinutes(-30));
        _porciones.Setup(p => p.ObtenerEnvaseAsync(envase.Id, It.IsAny<CancellationToken>())).ReturnsAsync(envase);
        _porciones.Setup(p => p.ObtenerAbiertosAsync(_vaso.Id, It.IsAny<CancellationToken>())).ReturnsAsync([envase]);
        return envase;
    }

    [Fact]
    public async Task Descartar_RegistraUnaMermaPorLasPorcionesRestantesYRetiraElEnvase()
    {
        var envase = EnvaseConRestantes(3);
        MovimientoInventario? merma = null;
        _inventario
            .Setup(i => i.AgregarMovimientoAsync(It.IsAny<MovimientoInventario>(), It.IsAny<CancellationToken>()))
            .Callback<MovimientoInventario, CancellationToken>((m, _) => merma = m)
            .Returns(Task.CompletedTask);

        var resultado = await CrearServicio().DescartarAsync(envase.Id, new DescartarEnvaseDto("Derrame"));

        Assert.Equal(EstadoEnvase.Descartado, resultado!.Estado);
        Assert.Equal("Derrame", resultado.MotivoDescarte);
        Assert.NotNull(merma);
        Assert.Equal(TipoMovimientoInventario.Merma, merma!.Tipo);
        Assert.Equal(-3m, merma.Cantidad);
        Assert.Equal(_vaso.Id, merma.VarianteId);
        Assert.Equal(envase.Id, merma.ReferenciaId);
        _notificador.Verify(n => n.EnvaseActualizadoAsync(envase.Id, _vaso.Id, "Descartado", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Descartar_SiHayMasPorcionesReservadasQueLasQueQuedarianAbiertas_LanzaConflicto()
    {
        var envase = EnvaseConRestantes(3);
        var stock = new StockPorcion { VarianteId = _vaso.Id };
        stock.Reservar(2);
        _porciones.Setup(p => p.ObtenerStockPorcionAsync(_vaso.Id, It.IsAny<CancellationToken>())).ReturnsAsync(stock);

        var ex = await Assert.ThrowsAsync<ConflictoException>(
            () => CrearServicio().DescartarAsync(envase.Id, new DescartarEnvaseDto("Derrame")));

        Assert.Contains("reservada", ex.Message);
        Assert.Equal(EstadoEnvase.Abierto, envase.Estado);
    }

    [Fact]
    public async Task Descartar_ConOtroEnvaseQueCubreLasReservas_SePermite()
    {
        var envase = EnvaseConRestantes(3);
        var otro = EnvaseAbierto.Destapar(_vaso.Id, _dosLitros.Id, 8, _usuarioId, Ahora);
        _porciones.Setup(p => p.ObtenerAbiertosAsync(_vaso.Id, It.IsAny<CancellationToken>())).ReturnsAsync([envase, otro]);
        var stock = new StockPorcion { VarianteId = _vaso.Id };
        stock.Reservar(5);
        _porciones.Setup(p => p.ObtenerStockPorcionAsync(_vaso.Id, It.IsAny<CancellationToken>())).ReturnsAsync(stock);

        var resultado = await CrearServicio().DescartarAsync(envase.Id, new DescartarEnvaseDto("Se cayó"));

        Assert.Equal(EstadoEnvase.Descartado, resultado!.Estado);
    }

    [Fact]
    public async Task Descartar_SinMotivo_LanzaReglaDeNegocio()
    {
        var envase = EnvaseConRestantes(3);

        await Assert.ThrowsAsync<ReglaNegocioException>(
            () => CrearServicio().DescartarAsync(envase.Id, new DescartarEnvaseDto("  ")));

        Assert.Equal(EstadoEnvase.Abierto, envase.Estado);
    }

    [Fact]
    public async Task Descartar_UnEnvaseInexistente_RetornaNull()
    {
        Assert.Null(await CrearServicio().DescartarAsync(Guid.NewGuid(), new DescartarEnvaseDto("x")));
    }

    [Fact]
    public async Task Descartar_UnEnvaseYaAgotado_LanzaReglaDeNegocio()
    {
        var envase = EnvaseConRestantes(1);
        envase.Consumir(1, Ahora);

        await Assert.ThrowsAsync<ReglaNegocioException>(
            () => CrearServicio().DescartarAsync(envase.Id, new DescartarEnvaseDto("x")));
    }

    /* ===================== Listar ===================== */

    [Fact]
    public async Task Listar_DevuelveLosAbiertosConSusPorcionesRestantes()
    {
        var envase = EnvaseConRestantes(5);
        _porciones
            .Setup(p => p.ListarAsync(_vaso.Id, EstadoEnvase.Abierto, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync([envase]);

        var lista = await CrearServicio().ListarAsync(_vaso.Id, EstadoEnvase.Abierto);

        var item = Assert.Single(lista);
        Assert.Equal(5, item.PorcionesRestantes);
        Assert.Equal(8, item.PorcionesIniciales);
    }

    [Fact]
    public async Task Listar_AcotaElLimite()
    {
        _porciones
            .Setup(p => p.ListarAsync(It.IsAny<Guid?>(), It.IsAny<EstadoEnvase?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EnvaseAbierto>());

        await CrearServicio().ListarAsync(limite: 100000);

        _porciones.Verify(p => p.ListarAsync(null, null, 500, It.IsAny<CancellationToken>()), Times.Once);
    }
}
