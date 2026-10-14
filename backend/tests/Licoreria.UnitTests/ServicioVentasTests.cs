using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Services;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Moq;

namespace Licoreria.UnitTests;

/// <summary>
/// Pruebas deterministas de <see cref="ServicioVentas"/> para pagos mixtos USD/Bs,
/// uso de la tasa de cambio vigente y exigencia de turno de caja. Todas las
/// dependencias se aíslan con Moq: no hay conexión real a la base de datos.
/// </summary>
public sealed class ServicioVentasTests
{
    private const decimal Tasa = 36.50m;

    private readonly Mock<IVentaRepository> _ventas = new();
    private readonly Mock<IInventarioRepository> _inventario = new();
    private readonly Mock<IServicioKardex> _kardex = new();
    private readonly Mock<IRepository<MetodoPago>> _metodos = new();
    private readonly Mock<IServicioFinanzas> _finanzas = new();
    private readonly Mock<ISesionCajaRepository> _sesionesCaja = new();
    private readonly Mock<IContextoUsuario> _contexto = new();
    private readonly Mock<IRelojSistema> _reloj = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IServicioAuditoria> _auditoria = new();
    private readonly Mock<IServicioClienteVenta> _clienteVenta = new();

    private readonly Guid _varianteId = Guid.NewGuid();
    private readonly Guid _metodoId = Guid.NewGuid();
    private Venta _creada = null!;

    public ServicioVentasTests()
    {
        _inventario
            .Setup(i => i.ObtenerVarianteConRecetasAsync(_varianteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductoVariante { Nombre = "Botella 750ml", PrecioVentaUSD = 100m });
        _kardex
            .Setup(k => k.DesglosarInsumosAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<KardexInsumo>());
        _metodos
            .Setup(m => m.GetByIdAsync(_metodoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MetodoPago { Codigo = "EFE", Nombre = "Efectivo" });
        _finanzas
            .Setup(f => f.ObtenerValorVigenteAsync(TipoTasa.Paralelo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Tasa);
        _sesionesCaja
            .Setup(s => s.ObtenerAbiertaBloqueadaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SesionCaja());
        _contexto.SetupGet(c => c.UsuarioId).Returns(Guid.NewGuid());
        _reloj.SetupGet(r => r.UtcNow).Returns(new DateTime(2026, 10, 9, 12, 0, 0, DateTimeKind.Utc));

        _ventas.Setup(v => v.ObtenerSiguienteCorrelativoComprobanteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1L);
        _ventas
            .Setup(v => v.AgregarAsync(It.IsAny<Venta>(), It.IsAny<CancellationToken>()))
            .Callback<Venta, CancellationToken>((venta, _) => _creada = venta)
            .Returns(Task.CompletedTask);
        _ventas
            .Setup(v => v.ObtenerConDetalleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _creada);
        _ventas.Setup(v => v.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // La transacción debe ejecutar la operación para que la venta se construya.
        _unitOfWork
            .Setup(u => u.EjecutarEnTransaccionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task> operacion, CancellationToken ct) => operacion(ct));
    }

    private ServicioVentas CrearServicio()
        => new(
            _ventas.Object,
            _inventario.Object,
            _kardex.Object,
            _metodos.Object,
            Mock.Of<IRepository<Promocion>>(),
            _finanzas.Object,
            _sesionesCaja.Object,
            _contexto.Object,
            _reloj.Object,
            _unitOfWork.Object,
            _auditoria.Object,
            _clienteVenta.Object);

    private RegistrarVentaDto VentaUnica(decimal pago, Moneda moneda = Moneda.USD)
        => new(
            Items: [new VentaItemDto(_varianteId, Cantidad: 1)],
            Pagos: [new VentaPagoDto(_metodoId, pago, moneda)]);

    [Fact]
    public async Task RegistrarVentaAsync_RegistraElEventoDentroDeLaTransaccion()
    {
        var venta = await CrearServicio().RegistrarVentaAsync(VentaUnica(100m));

        _auditoria.Verify(a => a.Registrar("registrar", "venta", venta.Id, It.IsAny<object?>()), Times.Once);
        _auditoria.Verify(
            a => a.RegistrarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RegistrarVentaAsync_PagoEnUsd_RegistraTasaYTotalBs()
    {
        var venta = await CrearServicio().RegistrarVentaAsync(VentaUnica(100m));

        Assert.Equal(100m, venta.TotalUSD);
        Assert.Equal(Tasa, venta.TasaCambio);
        Assert.Equal(Math.Round(100m * Tasa, 2), venta.TotalBS);

        var pago = Assert.Single(venta.Pagos);
        Assert.Equal(Moneda.USD, pago.Moneda);
        Assert.Equal(100m, pago.Monto);
    }

    [Fact]
    public async Task RegistrarVentaAsync_NumeraElComprobanteConElCorrelativoDeLaSecuencia()
    {
        _ventas
            .Setup(v => v.ObtenerSiguienteCorrelativoComprobanteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(42L);

        var venta = await CrearServicio().RegistrarVentaAsync(VentaUnica(100m));

        Assert.Equal("F-00000042", venta.NumeroComprobante);
        Assert.Equal("00-00000042", _creada.Comprobante!.NumeroControl);
        _ventas.Verify(
            v => v.ObtenerSiguienteCorrelativoComprobanteAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarVentaAsync_PagoMixtoUsdYBs_ConvierteBsConLaTasaVigente()
    {
        // 100 USD de total: 50 USD en efectivo + 1.825 Bs (1.825 / 36,50 = 50 USD).
        var dto = new RegistrarVentaDto(
            Items: [new VentaItemDto(_varianteId, Cantidad: 1)],
            Pagos:
            [
                new VentaPagoDto(_metodoId, 50m, Moneda.USD),
                new VentaPagoDto(_metodoId, 1825m, Moneda.BS),
            ]);

        var venta = await CrearServicio().RegistrarVentaAsync(dto);

        Assert.Equal(100m, venta.TotalUSD);
        Assert.Equal(2, venta.Pagos.Count);
        Assert.Contains(venta.Pagos, p => p.Moneda == Moneda.USD && p.Monto == 50m);
        Assert.Contains(venta.Pagos, p => p.Moneda == Moneda.BS && p.Monto == 1825m);
    }

    [Fact]
    public async Task RegistrarVentaAsync_PagosInsuficientes_LanzaReglaDeNegocio()
    {
        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(
            () => CrearServicio().RegistrarVentaAsync(VentaUnica(50m)));

        Assert.Contains("no cubre", excepcion.Message);
        Assert.Null(_creada);
    }

    [Fact]
    public async Task RegistrarVentaAsync_SinTurnoAbierto_LanzaReglaDeNegocio()
    {
        _sesionesCaja
            .Setup(s => s.ObtenerAbiertaBloqueadaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((SesionCaja?)null);

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(
            () => CrearServicio().RegistrarVentaAsync(VentaUnica(100m)));

        Assert.Contains("turno abierto", excepcion.Message);
    }

    [Fact]
    public async Task RegistrarVentaAsync_SinTasaVigente_LanzaNoEncontrado()
    {
        _finanzas
            .Setup(f => f.ObtenerValorVigenteAsync(TipoTasa.Paralelo, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NoEncontradoException("No hay una tasa de cambio vigente para el tipo Paralelo."));

        await Assert.ThrowsAsync<NoEncontradoException>(
            () => CrearServicio().RegistrarVentaAsync(VentaUnica(100m)));
    }

    // ===================== Cliente, puntos y crédito =====================

    private readonly Guid _clienteId = Guid.NewGuid();
    private readonly Guid _metodoCreditoId = Guid.NewGuid();

    private void ConMetodoCredito()
        => _metodos
            .Setup(m => m.GetByIdAsync(_metodoCreditoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MetodoPago { Codigo = MetodoPago.CodigoCredito, Nombre = "Crédito" });

    [Fact]
    public async Task RegistrarVentaAsync_ConCliente_ValidaAsociaYDevuelvePuntos()
    {
        _clienteVenta
            .Setup(c => c.AcumularPorVentaAsync(It.IsAny<Venta>(), 0m, It.IsAny<CancellationToken>()))
            .ReturnsAsync(100);

        var dto = VentaUnica(100m) with { ClienteId = _clienteId };
        var venta = await CrearServicio().RegistrarVentaAsync(dto);

        _clienteVenta.Verify(c => c.ValidarClienteAsync(_clienteId, It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(_clienteId, _creada.ClienteId);
        Assert.Equal(_clienteId, venta.ClienteId);
        Assert.Equal(100, venta.PuntosGanados);
        Assert.Equal(0m, venta.CreditoUSD);
    }

    [Fact]
    public async Task RegistrarVentaAsync_ClienteInactivo_NoRegistraLaVenta()
    {
        _clienteVenta
            .Setup(c => c.ValidarClienteAsync(_clienteId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ReglaNegocioException("El cliente está inactivo."));

        await Assert.ThrowsAsync<ReglaNegocioException>(
            () => CrearServicio().RegistrarVentaAsync(VentaUnica(100m) with { ClienteId = _clienteId }));

        Assert.Null(_creada);
        _ventas.Verify(v => v.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegistrarVentaAsync_SinCliente_NoGeneraPuntosNiCredito()
    {
        var venta = await CrearServicio().RegistrarVentaAsync(VentaUnica(100m));

        Assert.Null(venta.ClienteId);
        _clienteVenta.Verify(c => c.ValidarClienteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegistrarVentaAsync_CreditoSinCliente_LanzaReglaDeNegocio()
    {
        ConMetodoCredito();
        var dto = new RegistrarVentaDto(
            Items: [new VentaItemDto(_varianteId, Cantidad: 1)],
            Pagos: [new VentaPagoDto(_metodoCreditoId, 100m)]);

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(() => CrearServicio().RegistrarVentaAsync(dto));

        Assert.Contains("cliente", excepcion.Message);
        Assert.Null(_creada);
    }

    [Fact]
    public async Task RegistrarVentaAsync_VentaMixta_RegistraCreditoYCalculaPuntosSinEl()
    {
        ConMetodoCredito();
        IReadOnlyList<Pago>? pagosCredito = null;
        _clienteVenta
            .Setup(c => c.RegistrarCreditoAsync(It.IsAny<Venta>(), It.IsAny<IReadOnlyList<Pago>>(), null, It.IsAny<CancellationToken>()))
            .Callback<Venta, IReadOnlyList<Pago>, DateTime?, CancellationToken>((_, pagos, _, _) => pagosCredito = pagos)
            .ReturnsAsync(40m);
        _clienteVenta
            .Setup(c => c.AcumularPorVentaAsync(It.IsAny<Venta>(), 40m, It.IsAny<CancellationToken>()))
            .ReturnsAsync(60);

        var dto = new RegistrarVentaDto(
            Items: [new VentaItemDto(_varianteId, Cantidad: 1)],
            Pagos: [new VentaPagoDto(_metodoId, 60m), new VentaPagoDto(_metodoCreditoId, 40m)],
            ClienteId: _clienteId);
        var venta = await CrearServicio().RegistrarVentaAsync(dto);

        var credito = Assert.Single(pagosCredito!);
        Assert.Equal(40m, credito.Monto);
        Assert.Equal(40m, venta.CreditoUSD);
        Assert.Equal(60, venta.PuntosGanados);
    }

    [Fact]
    public async Task RegistrarVentaAsync_FallaDeStock_NoDejaPuntosNiCuentaPorCobrar()
    {
        ConMetodoCredito();
        _kardex
            .Setup(k => k.DesglosarInsumosAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<KardexInsumo> { new(_varianteId, 1m) });
        _kardex
            .Setup(k => k.VerificarStockAsync(It.IsAny<IReadOnlyList<KardexInsumo>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ExistenciaAgotadaException("Ron", 1m, 0m));

        var dto = new RegistrarVentaDto(
            Items: [new VentaItemDto(_varianteId, Cantidad: 1)],
            Pagos: [new VentaPagoDto(_metodoCreditoId, 100m)],
            ClienteId: _clienteId);

        await Assert.ThrowsAsync<ExistenciaAgotadaException>(() => CrearServicio().RegistrarVentaAsync(dto));

        _clienteVenta.Verify(
            c => c.RegistrarCreditoAsync(It.IsAny<Venta>(), It.IsAny<IReadOnlyList<Pago>>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _clienteVenta.Verify(
            c => c.AcumularPorVentaAsync(It.IsAny<Venta>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
        _ventas.Verify(v => v.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ===================== Devolución =====================

    private async Task<Venta> VentaExistenteAsync()
    {
        await CrearServicio().RegistrarVentaAsync(VentaUnica(100m) with { ClienteId = _clienteId });
        return _creada;
    }

    [Fact]
    public async Task RegistrarDevolucionAsync_RevierteLosPuntosDelMontoDevuelto()
    {
        var venta = await VentaExistenteAsync();
        _clienteVenta
            .Setup(c => c.RevertirPorDevolucionAsync(venta, 100m, It.IsAny<CancellationToken>()))
            .ReturnsAsync(100);

        var dto = new RegistrarDevolucionDto("Defecto", false, [new DevolucionDetalleCrearDto(venta.Detalles.First().VarianteId, 1m)]);
        var resultado = await CrearServicio().RegistrarDevolucionAsync(venta.Id, dto);

        Assert.NotNull(resultado);
        _clienteVenta.Verify(c => c.RevertirPorDevolucionAsync(venta, 100m, It.IsAny<CancellationToken>()), Times.Once);
        Assert.Null(resultado!.Aviso);
    }

    [Fact]
    public async Task RegistrarDevolucionAsync_VentaFiada_AvisaQueLaCuentaSeAjustaAMano()
    {
        var venta = await VentaExistenteAsync();
        _clienteVenta
            .Setup(c => c.TieneCuentaPendienteAsync(venta.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var dto = new RegistrarDevolucionDto("Defecto", false, [new DevolucionDetalleCrearDto(venta.Detalles.First().VarianteId, 1m)]);
        var resultado = await CrearServicio().RegistrarDevolucionAsync(venta.Id, dto);

        Assert.Contains("manualmente", resultado!.Aviso);
    }
}
