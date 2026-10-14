using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Services;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Moq;

namespace Licoreria.UnitTests;

/// <summary>Cierre de turno: libera las reservas de lo pedido y no servido, aislado con Moq.</summary>
public sealed class ServicioCajaReservaTests
{
    private readonly Mock<ISesionCajaRepository> _sesiones = new();
    private readonly Mock<IRepository<Denominacion>> _denominaciones = new();
    private readonly Mock<ICuentaRepository> _cuentas = new();
    private readonly Mock<IVentaRepository> _ventas = new();
    private readonly Mock<INotificadorComandas> _notificador = new();
    private readonly Mock<IContextoUsuario> _contexto = new();
    private readonly Mock<IRelojSistema> _reloj = new();
    private readonly Mock<IServicioAuditoria> _auditoria = new();
    private readonly Mock<IServicioKardex> _kardex = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly SesionCaja _sesion = new() { FondoInicial = 50m };

    public ServicioCajaReservaTests()
    {
        _reloj.SetupGet(r => r.UtcNow).Returns(new DateTime(2026, 10, 13, 3, 0, 0, DateTimeKind.Utc));
        _sesiones
            .Setup(s => s.ObtenerConDetalleAsync(_sesion.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_sesion);
        _sesiones.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _ventas
            .Setup(v => v.ObtenerTotalVentasDelTurnoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0m);
        _unitOfWork
            .Setup(u => u.EjecutarEnTransaccionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task> operacion, CancellationToken ct) => operacion(ct));
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

    private static Cuenta CuentaAbierta(decimal total, out ComandaDetalle detalle)
    {
        var cuenta = new Cuenta { SesionMesa = new SesionMesa { NombreMesa = "Mesa 1", MesaId = Guid.NewGuid() } };
        cuenta.Acumular(total);
        var comanda = new Comanda { Area = AreaDestino.Barra };
        detalle = new ComandaDetalle { VarianteId = Guid.NewGuid(), Cantidad = 1m, Reservado = true };
        comanda.Detalles.Add(detalle);
        cuenta.Comandas.Add(comanda);
        return cuenta;
    }

    [Fact]
    public async Task CerrarSesionAsync_LiberaLasReservasDeLasCuentasQueQuedaronAbiertas()
    {
        var conSaldo = CuentaAbierta(20m, out var detalle1);
        var sinSaldo = CuentaAbierta(0m, out var detalle2);
        _cuentas
            .Setup(c => c.ObtenerAbiertasConSesionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([conSaldo, sinSaldo]);

        var resultado = await CrearServicio().CerrarSesionAsync(_sesion.Id, new CerrarCajaDto([]));

        Assert.NotNull(resultado);
        _kardex.Verify(
            k => k.LiberarReservasAsync(
                It.Is<IEnumerable<ComandaDetalle>>(d => d.Contains(detalle1) && d.Contains(detalle2)),
                It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.Equal(EstadoCuenta.PorCobrar, conSaldo.Estado);
        Assert.Equal(EstadoCuenta.Cerrada, sinSaldo.Estado);
        Assert.Equal(2, _sesion.CuentasDesalojadas);
    }

    [Fact]
    public async Task CerrarSesionAsync_SinCuentasAbiertas_NoLiberaNada()
    {
        _cuentas
            .Setup(c => c.ObtenerAbiertasConSesionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await CrearServicio().CerrarSesionAsync(_sesion.Id, new CerrarCajaDto([]));

        _kardex.Verify(
            k => k.LiberarReservasAsync(
                It.Is<IEnumerable<ComandaDetalle>>(d => !d.Any()), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CerrarSesionAsync_RegistraElEventoDeCierreSinGuardarPorSuCuenta()
    {
        _cuentas
            .Setup(c => c.ObtenerAbiertasConSesionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await CrearServicio().CerrarSesionAsync(_sesion.Id, new CerrarCajaDto([]));

        _auditoria.Verify(a => a.Registrar("cerrar", "sesion-caja", _sesion.Id, It.IsAny<object?>()), Times.Once);
        _auditoria.Verify(
            a => a.RegistrarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ===== Bloqueo del turno y cierres simultáneos =====

    [Fact]
    public async Task CerrarSesionAsync_BloqueaElTurnoAntesDeLeerloYCalcularElCierre()
    {
        var orden = new List<string>();
        _sesiones
            .Setup(s => s.BloquearParaCierreAsync(_sesion.Id, It.IsAny<CancellationToken>()))
            .Callback(() => orden.Add("bloqueo"))
            .Returns(Task.CompletedTask);
        _sesiones
            .Setup(s => s.ObtenerConDetalleAsync(_sesion.Id, It.IsAny<CancellationToken>()))
            .Callback(() => orden.Add("lectura"))
            .ReturnsAsync(_sesion);
        _cuentas
            .Setup(c => c.ObtenerAbiertasConSesionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await CrearServicio().CerrarSesionAsync(_sesion.Id, new CerrarCajaDto([]));

        Assert.Equal("bloqueo", orden[0]);
        Assert.Equal("lectura", orden[1]);
        Assert.Equal(EstadoSesionCaja.Cerrada, _sesion.Estado);
    }

    [Fact]
    public async Task CerrarSesionAsync_ElTurnoYaEstaCerrado_LanzaConflicto()
    {
        _sesion.Cerrar(50m, 50m, DateTime.UtcNow);
        _cuentas
            .Setup(c => c.ObtenerAbiertasConSesionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await Assert.ThrowsAsync<Licoreria.Domain.Common.ConflictoException>(
            () => CrearServicio().CerrarSesionAsync(_sesion.Id, new CerrarCajaDto([])));

        _notificador.Verify(n => n.TurnoCerradoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CerrarSesionAsync_TurnoInexistente_RetornaNull()
    {
        _sesiones
            .Setup(s => s.ObtenerConDetalleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SesionCaja?)null);

        var resultado = await CrearServicio().CerrarSesionAsync(Guid.NewGuid(), new CerrarCajaDto([]));

        Assert.Null(resultado);
    }

    [Fact]
    public async Task RegistrarMovimientoAsync_ConElTurnoAbierto_RegistraElMovimiento()
    {
        _sesiones
            .Setup(s => s.ObtenerAbiertaBloqueadaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_sesion);

        var resultado = await CrearServicio().RegistrarMovimientoAsync(
            _sesion.Id, new MovimientoCajaCrearDto(TipoMovimientoCaja.Ingreso, 10m, Moneda.USD, "Cambio"));

        Assert.NotNull(resultado);
        _sesiones.Verify(
            s => s.AgregarMovimientoAsync(It.Is<MovimientoCaja>(m => m.Monto == 10m), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegistrarMovimientoAsync_ElTurnoYaSeCerro_RetornaNullSinRegistrar()
    {
        _sesiones
            .Setup(s => s.ObtenerAbiertaBloqueadaAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((SesionCaja?)null);

        var resultado = await CrearServicio().RegistrarMovimientoAsync(
            _sesion.Id, new MovimientoCajaCrearDto(TipoMovimientoCaja.Egreso, 5m, Moneda.USD, "Gasto"));

        Assert.Null(resultado);
        _sesiones.Verify(s => s.AgregarMovimientoAsync(It.IsAny<MovimientoCaja>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegistrarMovimientoAsync_MontoNoPositivo_LanzaReglaDeNegocio()
    {
        await Assert.ThrowsAsync<Licoreria.Domain.Common.ReglaNegocioException>(() => CrearServicio().RegistrarMovimientoAsync(
            _sesion.Id, new MovimientoCajaCrearDto(TipoMovimientoCaja.Ingreso, 0m, Moneda.USD, "Nada")));
    }
}
