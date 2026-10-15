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

/// <summary>
/// Apertura, movimientos, arqueo y cierre de turno de <see cref="ServicioCaja"/>, aislado con Moq.
/// La liberación de reservas al cerrar tiene su propio archivo (<c>ServicioCajaReservaTests</c>).
/// </summary>
public sealed class ServicioCajaTests
{
    private readonly Mock<ISesionCajaRepository> _sesiones = new();
    private readonly Mock<IRepository<Denominacion>> _denominaciones = new();
    private readonly Mock<ICuentaRepository> _cuentas = new();
    private readonly Mock<IVentaRepository> _ventas = new();
    private readonly Mock<INotificadorComandas> _notificador = new();
    private readonly Mock<IContextoUsuario> _contexto = ContextoUsuarioFalso.Crear();
    private readonly Mock<IRelojSistema> _reloj = RelojFijo.Crear();
    private readonly Mock<IServicioAuditoria> _auditoria = new();
    private readonly Mock<IServicioKardex> _kardex = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    public ServicioCajaTests()
    {
        _unitOfWork.EjecutarOperaciones();
        _ventas
            .Setup(v => v.ObtenerTotalVentasDelTurnoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0m);
        _cuentas
            .Setup(c => c.ObtenerAbiertasConSesionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
    }

    private ServicioCaja CrearServicio()
        => new(
            _sesiones.Object,
            _denominaciones.Object,
            _cuentas.Object,
            _ventas.Object,
            _notificador.Object,
            _contexto.Object,
            _reloj.Object,
            _auditoria.Object,
            _kardex.Object,
            _unitOfWork.Object);

    private SesionCaja ConSesionPorId(decimal fondo = 100m, params MovimientoCaja[] movimientos)
    {
        var sesion = new SesionCaja { FondoInicial = fondo };
        foreach (var movimiento in movimientos)
        {
            sesion.Movimientos.Add(movimiento);
        }

        _sesiones.Setup(s => s.ObtenerConDetalleAsync(sesion.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sesion);
        return sesion;
    }

    private static MovimientoCaja Movimiento(TipoMovimientoCaja tipo, decimal monto)
        => new() { Tipo = tipo, Monto = monto, Moneda = Moneda.USD, Motivo = "x" };

    private Denominacion ConDenominacion(decimal valor, Moneda moneda = Moneda.USD)
    {
        var denominacion = new Denominacion { Valor = valor, Moneda = moneda, Tipo = TipoDenominacion.Billete };
        _denominaciones.Setup(d => d.GetByIdAsync(denominacion.Id, It.IsAny<CancellationToken>())).ReturnsAsync(denominacion);
        return denominacion;
    }

    // ======================= Apertura =======================

    [Fact]
    public async Task AbrirSesion_SinTurnoAbierto_CreaLaSesionAudita_YNotifica()
    {
        SesionCaja? creada = null;
        _sesiones
            .Setup(s => s.AgregarAsync(It.IsAny<SesionCaja>(), It.IsAny<CancellationToken>()))
            .Callback<SesionCaja, CancellationToken>((s, _) => creada = s)
            .Returns(Task.CompletedTask);

        var dto = await CrearServicio().AbrirSesionAsync(new AbrirCajaDto(75m));

        Assert.NotNull(creada);
        Assert.Equal(ContextoUsuarioFalso.UsuarioIdPorDefecto, creada!.UsuarioId);
        Assert.Equal(75m, creada.FondoInicial);
        Assert.Equal(RelojFijo.FechaPorDefecto, creada.AbiertaEn);
        Assert.Equal(creada.Id, dto.Id);
        Assert.Equal(EstadoSesionCaja.Abierta, dto.Estado);
        _auditoria.Verify(a => a.Registrar("abrir", "sesion-caja", creada.Id, It.IsAny<object?>()), Times.Once);
        _sesiones.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _notificador.Verify(n => n.TurnoAbiertoAsync(creada.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AbrirSesion_ConFondoCero_EsValido()
    {
        var dto = await CrearServicio().AbrirSesionAsync(new AbrirCajaDto(0m));

        Assert.Equal(0m, dto.FondoInicial);
    }

    [Fact]
    public async Task AbrirSesion_ConTurnoAbierto_FallaConConflictoYNoGuarda()
    {
        _sesiones.Setup(s => s.ObtenerAbiertaAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new SesionCaja());

        await Assert.ThrowsAsync<ConflictoException>(() => CrearServicio().AbrirSesionAsync(new AbrirCajaDto(10m)));

        _sesiones.Verify(s => s.AgregarAsync(It.IsAny<SesionCaja>(), It.IsAny<CancellationToken>()), Times.Never);
        _sesiones.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _notificador.Verify(n => n.TurnoAbiertoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AbrirSesion_ConFondoNegativo_SeRechaza()
    {
        await Assert.ThrowsAsync<ReglaNegocioException>(() => CrearServicio().AbrirSesionAsync(new AbrirCajaDto(-0.01m)));

        _sesiones.Verify(s => s.AgregarAsync(It.IsAny<SesionCaja>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AbrirSesion_SinUsuarioIdentificado_SeRechaza()
    {
        var anonimo = ContextoUsuarioFalso.Anonimo();
        var servicio = new ServicioCaja(
            _sesiones.Object, _denominaciones.Object, _cuentas.Object, _ventas.Object, _notificador.Object,
            anonimo.Object, _reloj.Object, _auditoria.Object, _kardex.Object, _unitOfWork.Object);

        await Assert.ThrowsAsync<ReglaNegocioException>(() => servicio.AbrirSesionAsync(new AbrirCajaDto(10m)));

        _sesiones.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ======================= Estado del turno y consultas =======================

    [Fact]
    public async Task ObtenerEstadoTurno_ConTurnoAbierto_IndicaLaHoraDeApertura()
    {
        var sesion = new SesionCaja { AbiertaEn = RelojFijo.FechaPorDefecto };
        _sesiones.Setup(s => s.ObtenerAbiertaAsync(It.IsAny<CancellationToken>())).ReturnsAsync(sesion);

        var estado = await CrearServicio().ObtenerEstadoTurnoAsync();

        Assert.True(estado.Abierto);
        Assert.Equal(RelojFijo.FechaPorDefecto, estado.AbiertaEn);
    }

    [Fact]
    public async Task ObtenerEstadoTurno_SinTurno_IndicaCerrado()
    {
        var estado = await CrearServicio().ObtenerEstadoTurnoAsync();

        Assert.False(estado.Abierto);
        Assert.Null(estado.AbiertaEn);
    }

    [Fact]
    public async Task ObtenerSesionActiva_SinTurno_DevuelveNulo()
        => Assert.Null(await CrearServicio().ObtenerSesionActivaAsync());

    [Fact]
    public async Task ObtenerSesionActiva_ConTurno_IncluyeLasVentasDelTurno()
    {
        var sesion = new SesionCaja { FondoInicial = 20m };
        _sesiones.Setup(s => s.ObtenerAbiertaAsync(It.IsAny<CancellationToken>())).ReturnsAsync(sesion);
        _ventas.Setup(v => v.ObtenerTotalVentasDelTurnoAsync(sesion.Id, It.IsAny<CancellationToken>())).ReturnsAsync(135m);

        var dto = await CrearServicio().ObtenerSesionActivaAsync();

        Assert.Equal(135m, dto!.VentasDelTurnoUSD);
        Assert.Equal(20m, dto.FondoInicial);
    }

    [Fact]
    public async Task ObtenerSesiones_MapeaCadaSesionConservandoLaPaginacion()
    {
        var a = new SesionCaja();
        var b = new SesionCaja();
        var paginacion = new PaginacionRequest { Page = 1, PageSize = 2 };
        _sesiones
            .Setup(s => s.ObtenerPaginadoAsync(paginacion, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultadoPaginado<SesionCaja>.Crear([a, b], 1, 2, 5));

        var pagina = await CrearServicio().ObtenerSesionesAsync(paginacion);

        Assert.Equal([a.Id, b.Id], pagina.Items.Select(i => i.Id));
        Assert.Equal(5, pagina.TotalItems);
    }

    [Fact]
    public async Task ObtenerDenominaciones_ExcluyeInactivasYEliminadasYOrdena()
    {
        var usd100 = new Denominacion { Moneda = Moneda.USD, Valor = 100m };
        var usd5 = new Denominacion { Moneda = Moneda.USD, Valor = 5m };
        var inactiva = new Denominacion { Moneda = Moneda.USD, Valor = 50m, Activo = false };
        var eliminada = new Denominacion { Moneda = Moneda.USD, Valor = 20m };
        eliminada.EliminarLogico();
        var bs = new Denominacion { Moneda = Moneda.BS, Valor = 10m };
        _denominaciones.Setup(d => d.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([usd5, bs, inactiva, usd100, eliminada]);

        var lista = await CrearServicio().ObtenerDenominacionesAsync();

        Assert.Equal(3, lista.Count);
        Assert.DoesNotContain(lista, d => d.Valor is 50m or 20m);
        var usd = lista.Where(d => d.Moneda == Moneda.USD).Select(d => d.Valor).ToList();
        Assert.Equal([100m, 5m], usd);
    }

    // ======================= Movimientos =======================

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task RegistrarMovimiento_ConMontoNoPositivo_SeRechazaSinAbrirTransaccion(int monto)
    {
        await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            CrearServicio().RegistrarMovimientoAsync(Guid.NewGuid(), new MovimientoCajaCrearDto(TipoMovimientoCaja.Ingreso, monto, Moneda.USD, "x")));

        _unitOfWork.Verify(
            u => u.EjecutarEnTransaccionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(TipoMovimientoCaja.Ingreso)]
    [InlineData(TipoMovimientoCaja.Egreso)]
    public async Task RegistrarMovimiento_EnElTurnoAbierto_LoAgregaYDevuelveLaSesion(TipoMovimientoCaja tipo)
    {
        var sesion = ConSesionPorId();
        _sesiones.Setup(s => s.ObtenerAbiertaBloqueadaAsync(It.IsAny<CancellationToken>())).ReturnsAsync(sesion);
        MovimientoCaja? agregado = null;
        _sesiones
            .Setup(s => s.AgregarMovimientoAsync(It.IsAny<MovimientoCaja>(), It.IsAny<CancellationToken>()))
            .Callback<MovimientoCaja, CancellationToken>((m, _) => agregado = m)
            .Returns(Task.CompletedTask);

        var dto = await CrearServicio().RegistrarMovimientoAsync(
            sesion.Id, new MovimientoCajaCrearDto(tipo, 25m, Moneda.USD, "Propina"));

        Assert.NotNull(dto);
        Assert.NotNull(agregado);
        Assert.Equal(sesion.Id, agregado!.SesionCajaId);
        Assert.Equal(tipo, agregado.Tipo);
        Assert.Equal(25m, agregado.Monto);
        Assert.Equal("Propina", agregado.Motivo);
    }

    [Fact]
    public async Task RegistrarMovimiento_ConElTurnoYaCerrado_DevuelveNuloYNoAgrega()
    {
        var sesion = ConSesionPorId();
        _sesiones.Setup(s => s.ObtenerAbiertaBloqueadaAsync(It.IsAny<CancellationToken>())).ReturnsAsync((SesionCaja?)null);

        var dto = await CrearServicio().RegistrarMovimientoAsync(
            sesion.Id, new MovimientoCajaCrearDto(TipoMovimientoCaja.Ingreso, 5m, Moneda.USD, "x"));

        Assert.Null(dto);
        _sesiones.Verify(s => s.AgregarMovimientoAsync(It.IsAny<MovimientoCaja>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegistrarMovimiento_EnUnaSesionDistintaALaAbierta_DevuelveNulo()
    {
        var otra = ConSesionPorId();
        _sesiones.Setup(s => s.ObtenerAbiertaBloqueadaAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new SesionCaja());

        var dto = await CrearServicio().RegistrarMovimientoAsync(
            otra.Id, new MovimientoCajaCrearDto(TipoMovimientoCaja.Egreso, 5m, Moneda.USD, "x"));

        Assert.Null(dto);
        _sesiones.Verify(s => s.AgregarMovimientoAsync(It.IsAny<MovimientoCaja>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ======================= Cierre =======================

    [Fact]
    public async Task CerrarSesion_CalculaElEsperadoConFondoIngresosYEgresos()
    {
        var sesion = ConSesionPorId(100m, Movimiento(TipoMovimientoCaja.Ingreso, 50m), Movimiento(TipoMovimientoCaja.Egreso, 30m));
        var billete = ConDenominacion(20m);

        var dto = await CrearServicio().CerrarSesionAsync(sesion.Id, new CerrarCajaDto([new ArqueoLineaCrearDto(billete.Id, 6)]));

        Assert.Equal(120m, dto!.MontoEsperado);
        Assert.Equal(120m, dto.MontoContado);
        Assert.Equal(0m, dto.Descuadre);
        Assert.Equal(EstadoSesionCaja.Cerrada, dto.Estado);
    }

    [Fact]
    public async Task CerrarSesion_ConContadoMenorQueElEsperado_DejaDescuadreNegativo()
    {
        var sesion = ConSesionPorId(100m);
        var billete = ConDenominacion(20m);

        var dto = await CrearServicio().CerrarSesionAsync(sesion.Id, new CerrarCajaDto([new ArqueoLineaCrearDto(billete.Id, 4)]));

        Assert.Equal(-20m, dto!.Descuadre);
        Assert.Equal(EstadoSesionCaja.Cerrada, sesion.Estado);
        Assert.Equal(RelojFijo.FechaPorDefecto, sesion.CerradaEn);
    }

    [Fact]
    public async Task CerrarSesion_ConContadoMayorQueElEsperado_DejaDescuadrePositivo()
    {
        var sesion = ConSesionPorId(100m);
        var billete = ConDenominacion(20m);

        var dto = await CrearServicio().CerrarSesionAsync(sesion.Id, new CerrarCajaDto([new ArqueoLineaCrearDto(billete.Id, 7)]));

        Assert.Equal(40m, dto!.Descuadre);
    }

    [Fact]
    public async Task CerrarSesion_RegistraUnArqueoPorDenominacionConSuSubtotal()
    {
        var sesion = ConSesionPorId(0m);
        var grande = ConDenominacion(50m);
        var chico = ConDenominacion(1m);
        var arqueos = new List<ArqueoDenominacion>();
        _sesiones
            .Setup(s => s.AgregarArqueoAsync(It.IsAny<ArqueoDenominacion>(), It.IsAny<CancellationToken>()))
            .Callback<ArqueoDenominacion, CancellationToken>((a, _) => arqueos.Add(a))
            .Returns(Task.CompletedTask);

        await CrearServicio().CerrarSesionAsync(
            sesion.Id, new CerrarCajaDto([new ArqueoLineaCrearDto(grande.Id, 2), new ArqueoLineaCrearDto(chico.Id, 0)]));

        Assert.Equal(2, arqueos.Count);
        Assert.Equal(100m, arqueos.Single(a => a.DenominacionId == grande.Id).Subtotal);
        Assert.Equal(0m, arqueos.Single(a => a.DenominacionId == chico.Id).Subtotal);
    }

    [Fact]
    public async Task CerrarSesion_ConDenominacionInexistente_LanzaNoEncontradoYNoMarcaElTurnoComoCerrado()
    {
        var sesion = ConSesionPorId();

        await Assert.ThrowsAsync<NoEncontradoException>(() =>
            CrearServicio().CerrarSesionAsync(sesion.Id, new CerrarCajaDto([new ArqueoLineaCrearDto(Guid.NewGuid(), 1)])));

        Assert.Equal(EstadoSesionCaja.Abierta, sesion.Estado);
        _notificador.Verify(n => n.TurnoCerradoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CerrarSesion_ConCantidadNegativa_SeRechaza()
    {
        var sesion = ConSesionPorId();
        var billete = ConDenominacion(10m);

        await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            CrearServicio().CerrarSesionAsync(sesion.Id, new CerrarCajaDto([new ArqueoLineaCrearDto(billete.Id, -1)])));

        Assert.Equal(EstadoSesionCaja.Abierta, sesion.Estado);
    }

    [Fact]
    public async Task CerrarSesion_YaCerrada_FallaConConflicto()
    {
        var sesion = ConSesionPorId();
        sesion.Cerrar(0m, 0m, RelojFijo.FechaPorDefecto);

        await Assert.ThrowsAsync<ConflictoException>(() => CrearServicio().CerrarSesionAsync(sesion.Id, new CerrarCajaDto([])));

        _notificador.Verify(n => n.TurnoCerradoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CerrarSesion_Inexistente_DevuelveNulo()
    {
        var dto = await CrearServicio().CerrarSesionAsync(Guid.NewGuid(), new CerrarCajaDto([]));

        Assert.Null(dto);
        _notificador.Verify(n => n.TurnoCerradoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CerrarSesion_Eliminada_DevuelveNulo()
    {
        var sesion = ConSesionPorId();
        sesion.EliminarLogico();

        Assert.Null(await CrearServicio().CerrarSesionAsync(sesion.Id, new CerrarCajaDto([])));
    }

    [Fact]
    public async Task CerrarSesion_ConCuentasConYSinSaldo_UnaQuedaPorCobrarYLaOtraCerrada()
    {
        var sesion = ConSesionPorId();
        var mesaConSaldo = Guid.NewGuid();
        var mesaSinSaldo = Guid.NewGuid();
        var conSaldo = CuentaAbierta(mesaConSaldo, total: 30m);
        var sinSaldo = CuentaAbierta(mesaSinSaldo, total: 0m);
        _cuentas.Setup(c => c.ObtenerAbiertasConSesionAsync(It.IsAny<CancellationToken>())).ReturnsAsync([conSaldo, sinSaldo]);

        var dto = await CrearServicio().CerrarSesionAsync(sesion.Id, new CerrarCajaDto([]));

        Assert.Equal(EstadoCuenta.PorCobrar, conSaldo.Estado);
        Assert.Equal(EstadoCuenta.Cerrada, sinSaldo.Estado);
        Assert.Equal(RelojFijo.FechaPorDefecto, conSaldo.SesionMesa.CerradaEn);
        Assert.Equal(2, dto!.CuentasDesalojadas);
        _notificador.Verify(n => n.MesaActualizadaAsync(mesaConSaldo, "Libre", null, It.IsAny<CancellationToken>()), Times.Once);
        _notificador.Verify(n => n.MesaActualizadaAsync(mesaSinSaldo, "Libre", null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CerrarSesion_CuentaSinMesa_SeDesalojaSinNotificarMesa()
    {
        var sesion = ConSesionPorId();
        var sinMesa = CuentaAbierta(mesaId: null, total: 0m);
        _cuentas.Setup(c => c.ObtenerAbiertasConSesionAsync(It.IsAny<CancellationToken>())).ReturnsAsync([sinMesa]);

        var dto = await CrearServicio().CerrarSesionAsync(sesion.Id, new CerrarCajaDto([]));

        Assert.Equal(0, dto!.CuentasDesalojadas);
        _notificador.Verify(
            n => n.MesaActualizadaAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CerrarSesion_RegistraVentasDelTurnoAuditoriaYNotificaElCierre()
    {
        var sesion = ConSesionPorId();
        _ventas.Setup(v => v.ObtenerTotalVentasDelTurnoAsync(sesion.Id, It.IsAny<CancellationToken>())).ReturnsAsync(480m);

        await CrearServicio().CerrarSesionAsync(sesion.Id, new CerrarCajaDto([]));

        Assert.Equal(480m, sesion.VentasDelTurnoUSD);
        _auditoria.Verify(a => a.Registrar("cerrar", "sesion-caja", sesion.Id, It.IsAny<object?>()), Times.Once);
        _notificador.Verify(n => n.TurnoCerradoAsync(sesion.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Cuenta CuentaAbierta(Guid? mesaId, decimal total)
    {
        var cuenta = new Cuenta { SesionMesa = new SesionMesa { NombreMesa = "Mesa", MesaId = mesaId } };
        cuenta.Acumular(total);
        return cuenta;
    }
}
