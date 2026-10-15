using Licoreria.Application.Common;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Services;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Moq;

namespace Licoreria.UnitTests;

/// <summary>Cliente, puntos de fidelidad y crédito de una venta: reglas aisladas con Moq.</summary>
public sealed class ServicioClienteVentaTests
{
    private const decimal Tasa = 36.50m;
    private static readonly DateTime FechaVenta = new(2026, 10, 9, 12, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IClienteRepository> _clientes = new();
    private readonly Cliente _cliente = new() { Nombre = "Ana Pérez" };
    private PuntosMovimiento? _movimiento;
    private CuentaPorCobrar? _cuenta;

    public ServicioClienteVentaTests()
    {
        _clientes
            .Setup(c => c.ObtenerConDetalleAsync(_cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _cliente);
        _clientes
            .Setup(c => c.AgregarMovimientoAsync(It.IsAny<PuntosMovimiento>(), It.IsAny<CancellationToken>()))
            .Callback<PuntosMovimiento, CancellationToken>((m, _) => _movimiento = m)
            .Returns(Task.CompletedTask);
        _clientes
            .Setup(c => c.AgregarCuentaPorCobrarAsync(It.IsAny<CuentaPorCobrar>(), It.IsAny<CancellationToken>()))
            .Callback<CuentaPorCobrar, CancellationToken>((c, _) => _cuenta = c)
            .Returns(Task.CompletedTask);
    }

    private ServicioClienteVenta CrearServicio(decimal puntosPorUsd = 1m, int dias = 30)
        => new(_clientes.Object, new OpcionesFidelidad { PuntosPorUSD = puntosPorUsd }, new OpcionesCredito { DiasVencimiento = dias });

    private Venta VentaDe(decimal totalUsd, bool conCliente = true)
        => new()
        {
            Fecha = FechaVenta,
            TasaCambio = Tasa,
            TotalUSD = totalUsd,
            ClienteId = conCliente ? _cliente.Id : null
        };

    private static Pago PagoCredito(decimal monto, Moneda moneda = Moneda.USD)
        => new() { Monto = monto, Moneda = moneda };

    // ---------- ValidarCliente ----------

    [Fact]
    public async Task ValidarCliente_Activo_LoDevuelve()
    {
        var cliente = await CrearServicio().ValidarClienteAsync(_cliente.Id);

        Assert.Same(_cliente, cliente);
    }

    [Fact]
    public async Task ValidarCliente_Inexistente_LanzaNoEncontrado()
    {
        await Assert.ThrowsAsync<NoEncontradoException>(() => CrearServicio().ValidarClienteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ValidarCliente_Eliminado_LanzaNoEncontrado()
    {
        _cliente.EliminarLogico();

        await Assert.ThrowsAsync<NoEncontradoException>(() => CrearServicio().ValidarClienteAsync(_cliente.Id));
    }

    [Fact]
    public async Task ValidarCliente_Inactivo_LanzaReglaDeNegocio()
    {
        _cliente.Activo = false;

        await Assert.ThrowsAsync<ReglaNegocioException>(() => CrearServicio().ValidarClienteAsync(_cliente.Id));
    }

    // ---------- Acumulación ----------

    [Fact]
    public async Task Acumular_Venta25Con80_Otorga25PuntosLigadosALaVenta()
    {
        var venta = VentaDe(25.80m);

        var puntos = await CrearServicio().AcumularPorVentaAsync(venta, 0m);

        Assert.Equal(25, puntos);
        Assert.Equal(25, _cliente.Puntos);
        Assert.Equal(TipoMovimientoPuntos.Acumulacion, _movimiento!.Tipo);
        Assert.Equal(25, _movimiento.Puntos);
        Assert.Equal(venta.Id, _movimiento.ReferenciaId);
        Assert.Equal("venta", _movimiento.ReferenciaTipo);
    }

    [Fact]
    public async Task Acumular_VentaMenorAUnPunto_NoRegistraMovimiento()
    {
        var puntos = await CrearServicio().AcumularPorVentaAsync(VentaDe(0.80m), 0m);

        Assert.Equal(0, puntos);
        Assert.Null(_movimiento);
        Assert.Equal(0, _cliente.Puntos);
    }

    [Fact]
    public async Task Acumular_VentaMixta_ExcluyeLaPorcionACredito()
    {
        var puntos = await CrearServicio().AcumularPorVentaAsync(VentaDe(40m), creditoUSD: 10m);

        Assert.Equal(30, puntos);
        Assert.Equal(30, _cliente.Puntos);
    }

    [Fact]
    public async Task Acumular_TasaCero_DesactivaLosPuntos()
    {
        var puntos = await CrearServicio(puntosPorUsd: 0m).AcumularPorVentaAsync(VentaDe(40m), 0m);

        Assert.Equal(0, puntos);
        Assert.Null(_movimiento);
    }

    [Fact]
    public async Task Acumular_TasaDistinta_Escala()
    {
        var puntos = await CrearServicio(puntosPorUsd: 2m).AcumularPorVentaAsync(VentaDe(10.40m), 0m);

        Assert.Equal(20, puntos);
    }

    [Fact]
    public async Task Acumular_SinCliente_NoHaceNada()
    {
        var puntos = await CrearServicio().AcumularPorVentaAsync(VentaDe(40m, conCliente: false), 0m);

        Assert.Equal(0, puntos);
        _clientes.Verify(c => c.ObtenerConDetalleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------- Crédito ----------

    [Fact]
    public async Task RegistrarCredito_SinCliente_LanzaReglaDeNegocio()
    {
        await Assert.ThrowsAsync<ReglaNegocioException>(
            () => CrearServicio().RegistrarCreditoAsync(VentaDe(40m, conCliente: false), [PagoCredito(10m)], null));

        Assert.Null(_cuenta);
    }

    [Fact]
    public async Task RegistrarCredito_VentaParcial_CreaCuentaEnlazadaConVencimientoPorDefecto()
    {
        var venta = VentaDe(40m);

        var monto = await CrearServicio().RegistrarCreditoAsync(venta, [PagoCredito(10m)], null);

        Assert.Equal(10m, monto);
        Assert.Equal(venta.Id, _cuenta!.VentaId);
        Assert.Equal(_cliente.Id, _cuenta.ClienteId);
        Assert.Equal(10m, _cuenta.MontoUSD);
        Assert.Equal(10m, _cuenta.SaldoUSD);
        Assert.Equal(FechaVenta.AddDays(30), _cuenta.Vencimiento);
        Assert.Equal(EstadoCuentaPorCobrar.Pendiente, _cuenta.Estado);
    }

    [Fact]
    public async Task RegistrarCredito_EnBs_SeConvierteAUsdConLaTasaDeLaVenta()
    {
        var monto = await CrearServicio().RegistrarCreditoAsync(VentaDe(40m), [PagoCredito(365m, Moneda.BS)], null);

        Assert.Equal(10m, monto);
        Assert.Equal(10m, _cuenta!.MontoUSD);
    }

    [Fact]
    public async Task RegistrarCredito_VencimientoIndicado_SeRespeta()
    {
        var vence = FechaVenta.AddDays(7);

        await CrearServicio().RegistrarCreditoAsync(VentaDe(40m), [PagoCredito(10m)], vence);

        Assert.Equal(vence, _cuenta!.Vencimiento);
    }

    [Fact]
    public async Task RegistrarCredito_VencimientoAnteriorALaVenta_LanzaReglaDeNegocio()
    {
        await Assert.ThrowsAsync<ReglaNegocioException>(
            () => CrearServicio().RegistrarCreditoAsync(VentaDe(40m), [PagoCredito(10m)], FechaVenta.AddDays(-1)));

        Assert.Null(_cuenta);
    }

    [Fact]
    public async Task RegistrarCredito_PlazoConfigurable()
    {
        await CrearServicio(dias: 15).RegistrarCreditoAsync(VentaDe(40m), [PagoCredito(10m)], null);

        Assert.Equal(FechaVenta.AddDays(15), _cuenta!.Vencimiento);
    }

    [Fact]
    public async Task RegistrarCredito_SinPagosACredito_NoCreaCuenta()
    {
        var monto = await CrearServicio().RegistrarCreditoAsync(VentaDe(40m), [], null);

        Assert.Equal(0m, monto);
        Assert.Null(_cuenta);
    }

    // ---------- Reversión ----------

    [Fact]
    public async Task Revertir_DevolucionParcial_RestaLosPuntosProporcionales()
    {
        _cliente.AcumularPuntos(20);

        var revertidos = await CrearServicio().RevertirPorDevolucionAsync(VentaDe(20m), 5m);

        Assert.Equal(5, revertidos);
        Assert.Equal(15, _cliente.Puntos);
        Assert.Equal(TipoMovimientoPuntos.Reversion, _movimiento!.Tipo);
        Assert.Equal(5, _movimiento.Puntos);
    }

    [Fact]
    public async Task Revertir_PuntosYaCanjeados_RevierteSoloLosDisponibles()
    {
        _cliente.AcumularPuntos(3);

        var revertidos = await CrearServicio().RevertirPorDevolucionAsync(VentaDe(20m), 5m);

        Assert.Equal(3, revertidos);
        Assert.Equal(0, _cliente.Puntos);
        Assert.Equal(3, _movimiento!.Puntos);
    }

    [Fact]
    public async Task Revertir_SinPuntosDisponibles_NoRegistraMovimiento()
    {
        var revertidos = await CrearServicio().RevertirPorDevolucionAsync(VentaDe(20m), 5m);

        Assert.Equal(0, revertidos);
        Assert.Null(_movimiento);
    }

    [Fact]
    public async Task Revertir_VentaSinCliente_NoHaceNada()
    {
        var revertidos = await CrearServicio().RevertirPorDevolucionAsync(VentaDe(20m, conCliente: false), 5m);

        Assert.Equal(0, revertidos);
    }

    // ---------- Cuenta pendiente ----------

    [Fact]
    public async Task TieneCuentaPendiente_SoloConSaldo()
    {
        var ventaId = Guid.NewGuid();
        var conSaldo = new CuentaPorCobrar { VentaId = ventaId };
        conSaldo.Inicializar(10m);
        _clientes.Setup(c => c.ObtenerCuentaPorCobrarDeVentaAsync(ventaId, It.IsAny<CancellationToken>())).ReturnsAsync(conSaldo);
        Assert.True(await CrearServicio().TieneCuentaPendienteAsync(ventaId));

        conSaldo.RegistrarPago(10m);
        Assert.False(await CrearServicio().TieneCuentaPendienteAsync(ventaId));
        Assert.False(await CrearServicio().TieneCuentaPendienteAsync(Guid.NewGuid()));
    }
}
