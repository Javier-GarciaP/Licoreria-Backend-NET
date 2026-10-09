using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.UnitTests;

public class StockProductoTests
{
    [Fact]
    public void AplicarMovimiento_EntradaAumentaElStock()
    {
        var stock = new StockProducto { VarianteId = Guid.NewGuid() };

        stock.AplicarMovimiento(10);

        Assert.Equal(10, stock.Cantidad);
    }

    [Fact]
    public void AplicarMovimiento_SalidaMayorAlStock_Lanza()
    {
        var stock = new StockProducto { VarianteId = Guid.NewGuid() };

        Assert.Throws<InvalidOperationException>(() => stock.AplicarMovimiento(-1));
    }

    [Fact]
    public void BajoMinimo_SeCalculaConElUmbral()
    {
        var stock = new StockProducto { VarianteId = Guid.NewGuid(), StockMinimo = 5 };

        stock.AplicarMovimiento(3);

        Assert.True(stock.BajoMinimo);
    }
}

public class CuentaTests
{
    [Fact]
    public void Acumular_MantieneLaCuentaAbierta()
    {
        var cuenta = new Cuenta();

        cuenta.Acumular(25.5m);

        Assert.Equal(25.5m, cuenta.Total);
        Assert.Equal(25.5m, cuenta.Saldo);
        Assert.Equal(EstadoCuenta.Abierta, cuenta.Estado);
    }

    [Fact]
    public void Reabrir_DevuelveLaCuentaAEstadosAbierta()
    {
        var cuenta = new Cuenta { Estado = EstadoCuenta.PorCobrar };

        cuenta.Reabrir();

        Assert.Equal(EstadoCuenta.Abierta, cuenta.Estado);
    }

    [Fact]
    public void Abonar_ReduceElSaldo()
    {
        var cuenta = new Cuenta();
        cuenta.Acumular(100m);

        cuenta.Abonar(40m);

        Assert.Equal(40m, cuenta.TotalAbonado);
        Assert.Equal(60m, cuenta.Saldo);
    }

    [Fact]
    public void Abonar_MontoNoPositivo_Lanza()
    {
        var cuenta = new Cuenta();

        Assert.Throws<InvalidOperationException>(() => cuenta.Abonar(0m));
    }
}

public class ClienteTests
{
    [Fact]
    public void AcumularYCanjearPuntos_ActualizaElSaldo()
    {
        var cliente = new Cliente();

        cliente.AcumularPuntos(100);
        cliente.CanjearPuntos(30);

        Assert.Equal(70, cliente.Puntos);
    }

    [Fact]
    public void CanjearPuntos_SinSaldoSuficiente_Lanza()
    {
        var cliente = new Cliente();
        cliente.AcumularPuntos(10);

        Assert.Throws<InvalidOperationException>(() => cliente.CanjearPuntos(50));
    }
}

public class CuentaPorCobrarTests
{
    [Fact]
    public void RegistrarPago_TotalQuedaPagada()
    {
        var cuenta = new CuentaPorCobrar();
        cuenta.Inicializar(50m);

        cuenta.RegistrarPago(50m);

        Assert.Equal(0m, cuenta.SaldoUSD);
    }

    [Fact]
    public void RegistrarPago_MayorAlSaldo_Lanza()
    {
        var cuenta = new CuentaPorCobrar();
        cuenta.Inicializar(50m);

        Assert.Throws<InvalidOperationException>(() => cuenta.RegistrarPago(60m));
    }
}
