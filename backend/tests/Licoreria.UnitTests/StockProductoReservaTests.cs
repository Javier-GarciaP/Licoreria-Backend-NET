using Licoreria.Domain.Entities;

namespace Licoreria.UnitTests;

public sealed class StockProductoReservaTests
{
    private static StockProducto ConExistencia(decimal cantidad)
    {
        var stock = new StockProducto();
        stock.AplicarMovimiento(cantidad);
        return stock;
    }

    [Fact]
    public void Reservar_ConExistencia_DisminuyeElDisponibleSinTocarLaCantidad()
    {
        var stock = ConExistencia(5m);

        stock.Reservar(2m);

        Assert.Equal(5m, stock.Cantidad);
        Assert.Equal(2m, stock.CantidadReservada);
        Assert.Equal(3m, stock.Disponible);
    }

    [Fact]
    public void Reservar_MasDeLoDisponible_Falla()
    {
        var stock = ConExistencia(1m);
        stock.Reservar(1m);

        var ex = Assert.Throws<InvalidOperationException>(() => stock.Reservar(1m));

        Assert.Contains("reservar", ex.Message);
        Assert.Equal(1m, stock.CantidadReservada);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Reservar_CantidadNoPositiva_Falla(int cantidad)
    {
        var stock = ConExistencia(5m);

        Assert.Throws<InvalidOperationException>(() => stock.Reservar(cantidad));
    }

    [Fact]
    public void LiberarReserva_DevuelveLoReservadoADisponible()
    {
        var stock = ConExistencia(5m);
        stock.Reservar(3m);

        stock.LiberarReserva(2m);

        Assert.Equal(1m, stock.CantidadReservada);
        Assert.Equal(4m, stock.Disponible);
    }

    [Fact]
    public void LiberarReserva_MasDeLoReservado_Falla()
    {
        var stock = ConExistencia(5m);
        stock.Reservar(1m);

        Assert.Throws<InvalidOperationException>(() => stock.LiberarReserva(2m));
        Assert.Equal(1m, stock.CantidadReservada);
    }

    [Fact]
    public void ConsumirReserva_DescuentaExistenciaYReserva()
    {
        var stock = ConExistencia(5m);
        stock.Reservar(2m);

        stock.ConsumirReserva(2m);

        Assert.Equal(3m, stock.Cantidad);
        Assert.Equal(0m, stock.CantidadReservada);
        Assert.Equal(3m, stock.Disponible);
    }

    [Fact]
    public void ConsumirReserva_MasDeLoReservado_Falla()
    {
        var stock = ConExistencia(5m);
        stock.Reservar(1m);

        Assert.Throws<InvalidOperationException>(() => stock.ConsumirReserva(2m));
        Assert.Equal(5m, stock.Cantidad);
    }

    [Fact]
    public void AplicarMovimiento_ParaDejarMenosDeLoReservado_Falla()
    {
        var stock = ConExistencia(3m);
        stock.Reservar(2m);

        var ex = Assert.Throws<InvalidOperationException>(() => stock.AplicarMovimiento(-2m));

        Assert.Contains("reservado", ex.Message);
        Assert.Equal(3m, stock.Cantidad);
    }

    [Fact]
    public void AplicarMovimiento_SobreLoDisponible_Funciona()
    {
        var stock = ConExistencia(3m);
        stock.Reservar(2m);

        stock.AplicarMovimiento(-1m);

        Assert.Equal(2m, stock.Cantidad);
        Assert.Equal(0m, stock.Disponible);
    }

    [Fact]
    public void AplicarMovimiento_ParaDejarNegativo_Falla()
    {
        var stock = ConExistencia(1m);

        Assert.Throws<InvalidOperationException>(() => stock.AplicarMovimiento(-2m));
    }
}
