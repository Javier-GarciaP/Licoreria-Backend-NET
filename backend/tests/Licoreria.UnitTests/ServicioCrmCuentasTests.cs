using Licoreria.Application.Common;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Services;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Moq;

namespace Licoreria.UnitTests;

/// <summary>Estado derivado «vencida» de las cuentas por cobrar y ventas del cliente.</summary>
public sealed class ServicioCrmCuentasTests
{
    private static readonly DateTime Ahora = new(2026, 10, 14, 12, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IClienteRepository> _clientes = new();
    private readonly Mock<IVentaRepository> _ventas = new();
    private readonly Mock<IRelojSistema> _reloj = new();

    public ServicioCrmCuentasTests() => _reloj.SetupGet(r => r.UtcNow).Returns(Ahora);

    private ServicioCrm CrearServicio() => new(_clientes.Object, _ventas.Object, _reloj.Object);

    private async Task<EstadoCuentaPorCobrar> EstadoListadoAsync(CuentaPorCobrar cuenta, bool soloPendientes = false)
    {
        _clientes
            .Setup(c => c.ObtenerCuentasPorCobrarAsync(It.IsAny<PaginacionRequest>(), null, soloPendientes, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultadoPaginado<CuentaPorCobrar>.Crear([cuenta], 1, 10, 1));

        var resultado = await CrearServicio().ObtenerCuentasPorCobrarAsync(new PaginacionRequest(), null, soloPendientes);
        return Assert.Single(resultado.Items).Estado;
    }

    private static CuentaPorCobrar Cuenta(DateTime vencimiento, decimal monto = 10m)
    {
        var cuenta = new CuentaPorCobrar { Vencimiento = vencimiento, Cliente = new Cliente { Nombre = "Ana" } };
        cuenta.Inicializar(monto);
        return cuenta;
    }

    [Fact]
    public async Task CuentaPasadaDeFecha_SeMuestraVencida()
    {
        Assert.Equal(EstadoCuentaPorCobrar.Vencida, await EstadoListadoAsync(Cuenta(Ahora.AddDays(-1))));
    }

    [Fact]
    public async Task CuentaVigente_SigueComoPendiente()
    {
        Assert.Equal(EstadoCuentaPorCobrar.Pendiente, await EstadoListadoAsync(Cuenta(Ahora.AddDays(5))));
    }

    [Fact]
    public async Task CuentaVencidaQueSePagaPorCompleto_EsPagada()
    {
        var cuenta = Cuenta(Ahora.AddDays(-3));
        cuenta.RegistrarPago(10m);

        Assert.Equal(EstadoCuentaPorCobrar.Pagada, await EstadoListadoAsync(cuenta));
    }

    [Fact]
    public async Task FiltroDePendientes_IncluyeLasVencidas()
    {
        var vencida = Cuenta(Ahora.AddDays(-1));

        Assert.Equal(EstadoCuentaPorCobrar.Vencida, await EstadoListadoAsync(vencida, soloPendientes: true));
    }

    [Fact]
    public async Task VentasDeClienteInexistente_DevuelveNull()
    {
        var resultado = await CrearServicio().ObtenerVentasDeClienteAsync(Guid.NewGuid(), new PaginacionRequest());

        Assert.Null(resultado);
    }

    [Fact]
    public async Task VentasDeCliente_MapeaFechaTotalYComprobante()
    {
        var cliente = new Cliente { Nombre = "Ana" };
        var venta = new Venta { TotalUSD = 25m, Comprobante = new ComprobanteFiscal { Numero = "F-00000001" } };
        _clientes.Setup(c => c.ObtenerConDetalleAsync(cliente.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
        _ventas
            .Setup(v => v.ObtenerPorClienteAsync(cliente.Id, It.IsAny<PaginacionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultadoPaginado<Venta>.Crear([venta], 1, 10, 1));

        var resultado = await CrearServicio().ObtenerVentasDeClienteAsync(cliente.Id, new PaginacionRequest());

        var item = Assert.Single(resultado!.Items);
        Assert.Equal(25m, item.TotalUSD);
        Assert.Equal("F-00000001", item.NumeroComprobante);
    }
}
