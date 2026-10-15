using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Services;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Licoreria.Domain.Services;
using Licoreria.UnitTests.Support;
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
    private readonly Mock<IRepository<ProductoVariante>> _variantes = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
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
            .Setup(s => s.ObtenerAbiertaBloqueadaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SesionCaja());
        _finanzas
            .Setup(f => f.ObtenerValorVigenteAsync(TipoTasa.Paralelo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Tasa);

        // La transacción ejecuta la operación; el guardado real lo hace la unidad de trabajo.
        _unitOfWork.EjecutarOperaciones();
        _kardex
            .Setup(k => k.DesglosarInsumosAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid variante, decimal cantidad, CancellationToken _) => new List<KardexInsumo> { new(variante, cantidad) });
    }

    private ServicioCuentas CrearServicio()
        => new(
            _cuentas.Object,
            _variantes.Object,
            _metodos.Object,
            Mock.Of<IRepository<CuentaDivision>>(),
            Mock.Of<IRepository<Mesa>>(),
            _sesionesCaja.Object,
            _ventas.Object,
            _kardex.Object,
            _reloj.Object,
            _notificador.Object,
            _finanzas.Object,
            _maquina,
            _unitOfWork.Object);

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
    public async Task RegistrarAbonoAsync_ConMetodoCredito_LanzaReglaDeNegocio()
    {
        var creditoId = Guid.NewGuid();
        _metodos
            .Setup(m => m.GetByIdAsync(creditoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MetodoPago { Codigo = MetodoPago.CodigoCredito, Nombre = "Crédito" });

        var excepcion = await Assert.ThrowsAsync<ReglaNegocioException>(
            () => CrearServicio().RegistrarAbonoAsync(_cuentaId, new RegistrarAbonoCuentaDto(creditoId, 50m, Moneda.USD)));

        Assert.Contains("cerrar la cuenta", excepcion.Message);
        Assert.Equal(0m, _cuenta.TotalAbonado);
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

    /* ===================== Reserva de stock al pedir ===================== */

    private ProductoVariante PrepararVariante(Guid varianteId, decimal precio = 50m)
    {
        var variante = new ProductoVariante { Nombre = "Lata 355ml", PrecioVentaUSD = precio };
        _variantes
            .Setup(v => v.GetByIdAsync(varianteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(variante);
        return variante;
    }

    [Fact]
    public async Task AgregarComandaAsync_ReservaLosInsumosYMarcaElItem()
    {
        var varianteId = Guid.NewGuid();
        PrepararVariante(varianteId);
        Comanda? creada = null;
        _cuentas
            .Setup(c => c.AgregarComandaAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .Callback<Comanda, CancellationToken>((comanda, _) => creada = comanda)
            .Returns(Task.CompletedTask);

        var resultado = await CrearServicio().AgregarComandaAsync(
            _cuentaId,
            new CrearComandaDto(AreaDestino.Barra, [new ComandaItemCrearDto(varianteId, 2m)]));

        Assert.NotNull(resultado);
        _kardex.Verify(
            k => k.ReservarAsync(
                It.Is<IReadOnlyList<KardexInsumo>>(i => i.Count == 1 && i[0].VarianteId == varianteId && i[0].Cantidad == 2m),
                It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.NotNull(creada);
        Assert.True(Assert.Single(creada!.Detalles).Reservado);
        Assert.Equal(200m, _cuenta.Total); // 100 previos + 2 x 50
    }

    [Fact]
    public async Task AgregarComandaAsync_SinExistencia_NoCreaLaComandaNiAcumulaElTotal()
    {
        var varianteId = Guid.NewGuid();
        PrepararVariante(varianteId);
        _kardex
            .Setup(k => k.ReservarAsync(It.IsAny<IReadOnlyList<KardexInsumo>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ExistenciaAgotadaException("Coca-Cola", 1m, 0m));

        await Assert.ThrowsAsync<ExistenciaAgotadaException>(() => CrearServicio().AgregarComandaAsync(
            _cuentaId,
            new CrearComandaDto(AreaDestino.Barra, [new ComandaItemCrearDto(varianteId, 1m)])));

        Assert.Equal(100m, _cuenta.Total);
        _cuentas.Verify(c => c.AgregarComandaAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()), Times.Never);
        _notificador.Verify(
            n => n.ComandaCreadaAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task AgregarComandaAsync_ProductoSinInventario_NoMarcaReserva()
    {
        var varianteId = Guid.NewGuid();
        PrepararVariante(varianteId);
        _kardex
            .Setup(k => k.DesglosarInsumosAsync(varianteId, It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<KardexInsumo>()); // Cocina: sin stock ni receta
        Comanda? creada = null;
        _cuentas
            .Setup(c => c.AgregarComandaAsync(It.IsAny<Comanda>(), It.IsAny<CancellationToken>()))
            .Callback<Comanda, CancellationToken>((comanda, _) => creada = comanda)
            .Returns(Task.CompletedTask);

        await CrearServicio().AgregarComandaAsync(
            _cuentaId,
            new CrearComandaDto(AreaDestino.Cocina, [new ComandaItemCrearDto(varianteId, 1m)]));

        Assert.False(Assert.Single(creada!.Detalles).Reservado);
    }

    [Fact]
    public async Task AgregarComandaAsync_CuentaCerrada_RetornaNullSinReservar()
    {
        _cuenta.Cerrar();

        var resultado = await CrearServicio().AgregarComandaAsync(
            _cuentaId,
            new CrearComandaDto(AreaDestino.Barra, [new ComandaItemCrearDto(Guid.NewGuid(), 1m)]));

        Assert.Null(resultado);
        _kardex.Verify(k => k.ReservarAsync(It.IsAny<IReadOnlyList<KardexInsumo>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /* ===================== Servir, cancelar y cerrar con reserva ===================== */

    [Fact]
    public async Task CambiarEstadoItemAsync_AlServirUnItemReservado_ConsumeLaReservaYNoDescuentaDosVeces()
    {
        _detalle.Reservado = true;

        await CrearServicio().CambiarEstadoItemAsync(
            _cuentaId, _comanda.Id, _detalle.Id, new ActualizarEstadoItemDto(EstadoItemComanda.Preparado));

        _kardex.Verify(
            k => k.ConsumirReservaAsync(
                _detalle.VarianteId, TipoMovimientoInventario.Venta, 2m, "comanda", _comanda.Id,
                It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _kardex.Verify(
            k => k.AplicarAsync(
                It.IsAny<Guid>(), It.IsAny<TipoMovimientoInventario>(), It.IsAny<decimal>(),
                It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        Assert.False(_detalle.Reservado);
    }

    [Fact]
    public async Task CambiarEstadoItemAsync_DePreparadoAEntregado_NoVuelveAConsumir()
    {
        _detalle.Reservado = true;
        var servicio = CrearServicio();

        await servicio.CambiarEstadoItemAsync(
            _cuentaId, _comanda.Id, _detalle.Id, new ActualizarEstadoItemDto(EstadoItemComanda.Preparado));
        await servicio.CambiarEstadoItemAsync(
            _cuentaId, _comanda.Id, _detalle.Id, new ActualizarEstadoItemDto(EstadoItemComanda.Entregado));

        _kardex.Verify(
            k => k.ConsumirReservaAsync(
                It.IsAny<Guid>(), It.IsAny<TipoMovimientoInventario>(), It.IsAny<decimal>(),
                It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CambiarEstadoItemAsync_AlServirUnItemAnteriorALaReserva_DescuentaDirectoDelInventario()
    {
        _detalle.Reservado = false;

        await CrearServicio().CambiarEstadoItemAsync(
            _cuentaId, _comanda.Id, _detalle.Id, new ActualizarEstadoItemDto(EstadoItemComanda.Preparado));

        _kardex.Verify(
            k => k.AplicarAsync(
                _detalle.VarianteId, TipoMovimientoInventario.Venta, -2m, "comanda", _comanda.Id,
                It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _kardex.Verify(
            k => k.ConsumirReservaAsync(
                It.IsAny<Guid>(), It.IsAny<TipoMovimientoInventario>(), It.IsAny<decimal>(),
                It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CambiarEstadoItemAsync_AlCancelarUnItemReservado_LiberaLaReservaSinTocarLaExistencia()
    {
        _detalle.Reservado = true;

        var resultado = await CrearServicio().CambiarEstadoItemAsync(
            _cuentaId, _comanda.Id, _detalle.Id, new ActualizarEstadoItemDto(EstadoItemComanda.Cancelado));

        Assert.NotNull(resultado);
        _kardex.Verify(
            k => k.LiberarReservasAsync(
                It.Is<IEnumerable<ComandaDetalle>>(d => d.Contains(_detalle)), It.IsAny<CancellationToken>()),
            Times.Once);
        _kardex.Verify(
            k => k.AplicarAsync(
                It.IsAny<Guid>(), It.IsAny<TipoMovimientoInventario>(), It.IsAny<decimal>(),
                It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        Assert.Equal(0m, _cuenta.Total); // 100 - 2 x 50
    }

    [Fact]
    public async Task CambiarEstadoItemAsync_AlCancelarUnItemYaServido_ReintegraAlInventario()
    {
        _detalle.CambiarEstado(EstadoItemComanda.Preparado);

        await CrearServicio().CambiarEstadoItemAsync(
            _cuentaId, _comanda.Id, _detalle.Id, new ActualizarEstadoItemDto(EstadoItemComanda.Cancelado));

        _kardex.Verify(
            k => k.AplicarAsync(
                _detalle.VarianteId, TipoMovimientoInventario.Venta, 2m, "comanda", _comanda.Id,
                It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private VentaDto VentaRegistrada()
        => new(Guid.NewGuid(), DateTime.UtcNow, Tasa, 100m, 0m, 100m, 3650m, EstadoVenta.Completada,
            Guid.NewGuid(), null, "F-00000001", [], []);

    [Fact]
    public async Task CerrarCuentaAsync_LiberaLasReservasAntesDeRegistrarLaVenta()
    {
        _detalle.Reservado = true;
        var orden = new List<string>();
        _kardex
            .Setup(k => k.LiberarReservasAsync(It.IsAny<IEnumerable<ComandaDetalle>>(), It.IsAny<CancellationToken>()))
            .Callback(() => orden.Add("liberar"))
            .Returns(Task.CompletedTask);
        _ventas
            .Setup(v => v.RegistrarVentaAsync(It.IsAny<RegistrarVentaDto>(), It.IsAny<CancellationToken>()))
            .Callback(() => orden.Add("venta"))
            .ReturnsAsync(VentaRegistrada());

        var venta = await CrearServicio().CerrarCuentaAsync(
            _cuentaId, new CerrarCuentaDto([new VentaPagoDto(_metodoId, 100m, Moneda.USD)]));

        Assert.NotNull(venta);
        Assert.Equal(["liberar", "venta"], orden);
        Assert.Equal(EstadoCuenta.Cerrada, _cuenta.Estado);
        Assert.NotNull(_cuenta.SesionMesa.CerradaEn);
    }

    [Fact]
    public async Task CerrarCuentaAsync_ConClienteYVencimiento_LosPasaALaVenta()
    {
        var clienteId = Guid.NewGuid();
        var vence = new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc);
        RegistrarVentaDto? enviado = null;
        _ventas
            .Setup(v => v.RegistrarVentaAsync(It.IsAny<RegistrarVentaDto>(), It.IsAny<CancellationToken>()))
            .Callback<RegistrarVentaDto, CancellationToken>((d, _) => enviado = d)
            .ReturnsAsync(VentaRegistrada());

        var venta = await CrearServicio().CerrarCuentaAsync(
            _cuentaId,
            new CerrarCuentaDto([new VentaPagoDto(_metodoId, 100m, Moneda.USD)], ClienteId: clienteId, VencimientoCredito: vence));

        Assert.NotNull(venta);
        Assert.Equal(clienteId, enviado!.ClienteId);
        Assert.Equal(vence, enviado.VencimientoCredito);
        Assert.Equal(_cuenta.Id, enviado.CuentaId);
    }

    [Fact]
    public async Task CerrarCuentaAsync_SiFallaLaVenta_LaCuentaNoQuedaCerrada()
    {
        _ventas
            .Setup(v => v.RegistrarVentaAsync(It.IsAny<RegistrarVentaDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ExistenciaAgotadaException("Coca-Cola", 1m, 0m));

        await Assert.ThrowsAsync<ExistenciaAgotadaException>(() => CrearServicio().CerrarCuentaAsync(
            _cuentaId, new CerrarCuentaDto([new VentaPagoDto(_metodoId, 100m, Moneda.USD)])));

        Assert.NotEqual(EstadoCuenta.Cerrada, _cuenta.Estado);
        Assert.Null(_cuenta.SesionMesa.CerradaEn);
    }

    [Fact]
    public async Task CerrarCuentaAsync_SiOtraOperacionYaLaCobro_LanzaConflicto()
    {
        // Simula un conflicto de versión: el segundo intento ve la cuenta ya cerrada por otro cajero.
        _ventas
            .Setup(v => v.RegistrarVentaAsync(It.IsAny<RegistrarVentaDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(VentaRegistrada());
        _unitOfWork
            .Setup(u => u.EjecutarEnTransaccionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns(async (Func<CancellationToken, Task> operacion, CancellationToken ct) =>
            {
                await operacion(ct);
                _cuenta.Cerrar();
                await operacion(ct);
            });

        await Assert.ThrowsAsync<ConflictoException>(() => CrearServicio().CerrarCuentaAsync(
            _cuentaId, new CerrarCuentaDto([new VentaPagoDto(_metodoId, 100m, Moneda.USD)])));
    }

    [Fact]
    public async Task CerrarCuentaAsync_CuentaYaCerrada_RetornaNull()
    {
        _cuenta.Cerrar();

        var venta = await CrearServicio().CerrarCuentaAsync(
            _cuentaId, new CerrarCuentaDto([new VentaPagoDto(_metodoId, 100m, Moneda.USD)]));

        Assert.Null(venta);
        _ventas.Verify(v => v.RegistrarVentaAsync(It.IsAny<RegistrarVentaDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
