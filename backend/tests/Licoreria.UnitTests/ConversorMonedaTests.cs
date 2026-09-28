using Licoreria.Domain.Services;

namespace Licoreria.UnitTests;

public class ConversorMonedaTests
{
    private readonly ConversorMoneda _conversor = new();

    [Fact]
    public void ConvertirAusdBolivares_MultiplicaPorLaTasa()
    {
        var resultado = _conversor.ConvertirAusdBolivares(10m, 40m);

        Assert.Equal(400m, resultado);
    }

    [Fact]
    public void ConvertirAusdBolivares_RedondeaADosDecimales()
    {
        var resultado = _conversor.ConvertirAusdBolivares(3m, 1.005m);

        Assert.Equal(3.02m, resultado);
    }

    [Fact]
    public void ConvertirDeBsAUsd_DivideEntreLaTasa()
    {
        var resultado = _conversor.ConvertirDeBsAUsd(400m, 40m);

        Assert.Equal(10m, resultado);
    }

    [Fact]
    public void Convertir_ConMontoNegativo_Lanza()
    {
        Assert.Throws<InvalidOperationException>(() => _conversor.ConvertirAusdBolivares(-1m, 40m));
    }

    [Fact]
    public void Convertir_ConTasaNoPositiva_Lanza()
    {
        Assert.Throws<InvalidOperationException>(() => _conversor.ConvertirAusdBolivares(10m, 0m));
    }
}
