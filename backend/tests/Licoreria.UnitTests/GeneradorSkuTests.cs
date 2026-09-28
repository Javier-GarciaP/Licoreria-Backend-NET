using Licoreria.Domain.Services;

namespace Licoreria.UnitTests;

public class GeneradorSkuTests
{
    private readonly GeneradorSku _generador = new();

    [Fact]
    public void Generar_ProduceElFormatoEsperado()
    {
        var resultado = _generador.Generar("Herramientas", "Taladro", 7);

        Assert.Equal("HER-TAL-0007", resultado.SkuGenerado);
        Assert.True(resultado.EsValidoEmpresarial);
    }

    [Fact]
    public void Generar_SanitizaCaracteresEspecialesYEspacios()
    {
        var resultado = _generador.Generar("herramientas eléctricas!", "taladro 1/2", 5);

        Assert.Equal("HER-TAL-0005", resultado.SkuGenerado);
        Assert.True(resultado.EsValidoEmpresarial);
    }

    [Fact]
    public void Generar_RellenaConRellenoCuandoFaltanCaracteres()
    {
        var resultado = _generador.Generar("AB", "C", 1);

        Assert.Equal("ABX-CPP-0001", resultado.SkuGenerado);
        Assert.True(resultado.EsValidoEmpresarial);
    }

    [Theory]
    [InlineData(0, "0001")]
    [InlineData(-5, "0001")]
    [InlineData(100000, "9999")]
    [InlineData(42, "0042")]
    public void Generar_AplicaLimitesALaSecuencia(int secuencia, string esperado)
    {
        var resultado = _generador.Generar("Licores", "Ron", secuencia);

        Assert.EndsWith(esperado, resultado.SkuGenerado);
    }

    [Fact]
    public void Generar_ConEntradaVacia_UsaRellenosPorDefecto()
    {
        var resultado = _generador.Generar("", "", 1);

        Assert.Equal("XXX-PPP-0001", resultado.SkuGenerado);
        Assert.True(resultado.EsValidoEmpresarial);
    }
}
