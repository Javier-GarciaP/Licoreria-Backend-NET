using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Services;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Licoreria.Domain.Services;
using Moq;

namespace Licoreria.UnitTests;

/// <summary>
/// Pruebas deterministas de <see cref="ServicioCuentas"/> para abonos mixtos
/// USD/Bs (conversión con la tasa vigente) y para la validación de la máquina
/// de estados al cambiar el estado de una línea de comanda.
/// </summary>
public sealed class ServicioCuentasTests
{
    private const decimal Tasa = 36.50m;

    private readonly Mock<ICuentaRepository> _cuentas = new();
    private readonly Mock<IRepository<MetodoPago>> _metodos = new();
    private readonly Mock<ISesionCajaRepository> _sesionesCaja = new();
    private readonly Mock<IServicioVentas> _ventas = new();
    private readonly Mock<IServicioKardex> _kardex = new();
    private readonly Mock<IRelojSistema> _reloj = new();
    private readonly Mock<INotificadorComandas> _notificador = new();
    private readonly Mock<IServicioFinanzas> _finanzas = new();
    private readonly MaquinaEstadosComanda _maquina = new();

    private readonly Guid _cuentaId = Guid.NewGuid();
    private readonly Guid _metodoId = Guid.NewGuid();
    private readonly Cuenta _cuenta;
    private readonly Comanda _comanda;
    private readonly ComandaDetalle _detalle;
    private Abono _abono = null!;

    public ServicioCuentasTests()
    {
        _cuenta = new Cuenta { SesionMesa = new SesionMesa { NombreMesa = "Mesa 1" } };
        _cuenta.Acumular(100m);

        _comanda = new Comanda { CuentaId = _cuentaId, Area = AreaDestino.Barra };
        _detalle = new ComandaDetalle
        {
            ComandaId = _comanda.Id,
            VarianteId = Guid.NewGuid(),
            Cantidad = 2,
            PrecioUnitarioUSD = 50m,
            AreaDestino = AreaDestino.Barra,
            Estado = EstadoItemComanda.Recibido,
        };
        _comanda.Detalles.Add(_detalle);
        _cuenta.Comandas.Add(_comanda);

        _cuentas
            .Setup(c => c.ObtenerConDetalleAsync(_cuentaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _cuenta);
        _cuentas
            .Setup(c => c.ObtenerDetalleAsync(_comanda.Id, _detalle.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _detalle);
        _cuentas
            .Setup(c => c.AgregarAbonoAsync(It.IsAny<Abono>(), It.IsAny<CancellationToken>()))
            .Callback<Abono, CancellationToken>((abono, _) => _abono = abono)
            .Returns(Task.CompletedTask);
        _cuentas.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _metodos
            .Setup(m => m.GetByIdAsync(_metodoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MetodoPago { Codigo = "EFE", Nombre = "Efectivo" });
        _sesionesCaja
            .Setup(s => s.ObtenerAbiertaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SesionCaja());
        _finanzas
            .Setup(f => f.ObtenerValorVigenteAsync(TipoTasa.Paralelo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Tasa);
    }

    private ServicioCuentas CrearServicio()
        => new(
            _cuentas.Object,
            Mock.Of<IRepository<ProductoVariante>>(),
            _metodos.Object,
            Mock.Of<IRepository<CuentaDivision>>(),
            Mock.Of<IRepository<Mesa>>(),
            _sesionesCaja.Object,
            _ventas.Object,
            _kardex.Object,
            _reloj.Object,
            _notificador.Object,
            _finanzas.Object,
            _maquina);

    /* ===================== Abonos mixtos ===================== */

    [Fact]
    public async Task RegistrarAbonoAsync_EnUsd_AbonaSinConversion()
    {
        var resultado = await CrearServicio()
            .RegistrarAbonoAsync(_cuentaId, new RegistrarAbonoCuentaDto(_metodoId, 50m, Moneda.USD));

        Assert.NotNull(resultado);
        Assert.Equal(50m, resultado.TotalAbonado);
        Assert.Equal(50m, resultado.Saldo);
        Assert.Equal(50m, _abono.Monto);
        Assert.Equal(Moneda.USD, _abono.Moneda);
    }

    [Fact]
    public async Task RegistrarAbonoAsync_EnBs_ConvierteConLaTasaVigente()
    {
        // 3.650 Bs a 36,50 Bs/USD = 100 USD: cubre exactamente el saldo.
        var resultado = await CrearServicio()
            .RegistrarAbonoAsync(_cuentaId, new RegistrarAbonoCuentaDto(_metodoId, 3650m, Moneda.BS));

        Assert.NotNull(resultado);
        Assert.Equal(100m, resultado.TotalAbonado);
        Assert.Equal(0m, resultado.Saldo);

        // El historial conserva el monto y la moneda originales.
        Assert.Equal(3650m, _abono.Monto);
        Assert.Equal(Moneda.BS, _abono.Moneda);
    }

    [Fact]
    public async Task RegistrarAbonoAsync_SinTasaVigente_LanzaReglaDeNegocio()
    {
        _finanzas
            .Setup(f => f.ObtenerValorVigenteAsync(TipoTasa.Paralelo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0m);

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(
            () => CrearServicio().RegistrarAbonoAsync(_cuentaId, new RegistrarAbonoCuentaDto(_metodoId, 3650m, Moneda.BS)));

        Assert.Contains("tasa de cambio", excepcion.Message);
    }

    [Fact]
    public async Task RegistrarAbonoAsync_CuentaCerrada_RetornaNull()
    {
        _cuenta.Cerrar();

        var resultado = await CrearServicio()
            .RegistrarAbonoAsync(_cuentaId, new RegistrarAbonoCuentaDto(_metodoId, 50m, Moneda.USD));

        Assert.Null(resultado);
    }

    /* ===================== Máquina de estados ===================== */

    [Fact]
    public async Task CambiarEstadoItemAsync_TransicionValida_AplicaDerivaComandaYNotifica()
    {
        var resultado = await CrearServicio().CambiarEstadoItemAsync(
            _cuentaId,
            _comanda.Id,
            _detalle.Id,
            new ActualizarEstadoItemDto(EstadoItemComanda.EnProceso));

        Assert.NotNull(resultado);
        Assert.Equal(EstadoItemComanda.EnProceso, _detalle.Estado);
        Assert.Equal(EstadoComanda.EnPreparacion, Assert.Single(resultado.Comandas).Estado);

        _notificador.Verify(
            n => n.ItemActualizadoAsync(
                _cuentaId,
                _comanda.Id,
                _detalle.Id,
                "EnProceso",
                "Barra",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CambiarEstadoItemAsync_DelCicloComplejo_EntregadoEsAlcanzable()
    {
        _detalle.CambiarEstado(EstadoItemComanda.Preparado);

        var resultado = await CrearServicio().CambiarEstadoItemAsync(
            _cuentaId,
            _comanda.Id,
            _detalle.Id,
            new ActualizarEstadoItemDto(EstadoItemComanda.Entregado));

        Assert.NotNull(resultado);
        Assert.Equal(EstadoItemComanda.Entregado, _detalle.Estado);
        Assert.Equal(EstadoComanda.Entregada, Assert.Single(resultado.Comandas).Estado);
    }

    [Fact]
    public async Task CambiarEstadoItemAsync_TransicionInvalida_LanzaSinNotificar()
    {
        _detalle.CambiarEstado(EstadoItemComanda.Preparado);

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(
            () => CrearServicio().CambiarEstadoItemAsync(
                _cuentaId,
                _comanda.Id,
                _detalle.Id,
                new ActualizarEstadoItemDto(EstadoItemComanda.Recibido)));

        Assert.Contains("no permitida", excepcion.Message);

        // Estado sin cambios y sin efectos secundarios.
        Assert.Equal(EstadoItemComanda.Preparado, _detalle.Estado);
        _notificador.Verify(
            n => n.ItemActualizadoAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _cuentas.Verify(
            c => c.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        _kardex.Verify(
            k => k.AplicarAsync(
                It.IsAny<Guid>(),
                It.IsAny<TipoMovimientoInventario>(),
                It.IsAny<decimal>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CambiarEstadoItemAsync_MismoEstado_EsNoOp()
    {
        var resultado = await CrearServicio().CambiarEstadoItemAsync(
            _cuentaId,
            _comanda.Id,
            _detalle.Id,
            new ActualizarEstadoItemDto(EstadoItemComanda.Recibido));

        Assert.NotNull(resultado);
        Assert.Equal(EstadoItemComanda.Recibido, _detalle.Estado);
        _cuentas.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
