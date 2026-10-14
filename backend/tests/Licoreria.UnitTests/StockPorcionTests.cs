using Licoreria.Domain.Entities;

namespace Licoreria.UnitTests;

public sealed class StockPorcionTests
{
    [Fact]
    public void Reservar_SumaLasPorcionesReservadas()
    {
        var stock = new StockPorcion();

        stock.Reservar(2);
        stock.Reservar(3);

        Assert.Equal(5, stock.PorcionesReservadas);
    }

    [Fact]
    public void Liberar_RestaLasPorcionesReservadas()
    {
        var stock = new StockPorcion();
        stock.Reservar(4);

        stock.Liberar(3);

        Assert.Equal(1, stock.PorcionesReservadas);
    }

    [Fact]
    public void Liberar_MasDeLoReservado_Falla()
    {
        var stock = new StockPorcion();
        stock.Reservar(1);

        Assert.Throws<InvalidOperationException>(() => stock.Liberar(2));
        Assert.Equal(1, stock.PorcionesReservadas);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Reservar_CantidadNoPositiva_Falla(int porciones)
    {
        Assert.Throws<InvalidOperationException>(() => new StockPorcion().Reservar(porciones));
    }
}
