using Licoreria.Domain.Services;

namespace Licoreria.UnitTests;

public class CalculadoraCuentaTests
{
    private readonly CalculadoraCuenta _calculadora = new();

    [Fact]
    public void Calcular_SumaConsumosYAbonos()
    {
        var resumen = _calculadora.Calcular(new[] { 10m, 20m }, new[] { 5m });

        Assert.Equal(30m, resumen.Total);
        Assert.Equal(5m, resumen.TotalAbonado);
        Assert.Equal(25m, resumen.Saldo);
        Assert.False(resumen.EstaSaldada);
    }

    [Fact]
    public void Calcular_CuandoElSaldoEsCero_EstaSaldada()
    {
        var resumen = _calculadora.Calcular(new[] { 10m }, new[] { 10m });

        Assert.Equal(0m, resumen.Saldo);
        Assert.True(resumen.EstaSaldada);
    }

    [Fact]
    public void ValidarAbono_ConMontoValido_NoLanza()
    {
        var excepcion = Record.Exception(() => _calculadora.ValidarAbono(30m, 5m, 10m));

        Assert.Null(excepcion);
    }

    [Fact]
    public void ValidarAbono_ConMontoNoPositivo_Lanza()
    {
        Assert.Throws<InvalidOperationException>(() => _calculadora.ValidarAbono(30m, 5m, 0m));
    }

    [Fact]
    public void ValidarAbono_QueExcedeElSaldo_Lanza()
    {
        Assert.Throws<InvalidOperationException>(() => _calculadora.ValidarAbono(30m, 5m, 30m));
    }
}
