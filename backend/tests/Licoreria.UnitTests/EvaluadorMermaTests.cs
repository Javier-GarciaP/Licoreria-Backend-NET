using Licoreria.Domain.Services;

namespace Licoreria.UnitTests;

public class EvaluadorMermaTests
{
    private readonly EvaluadorMerma _evaluador = new();

    [Fact]
    public void Evaluar_SinReposicion_GeneraSoloElMovimientoDeMerma()
    {
        var resultado = _evaluador.Evaluar(MotivoMerma.Danado, 2, reponerSinCobro: false);

        Assert.Single(resultado.Movimientos);
        Assert.Equal("Merma", resultado.Movimientos[0].Tipo);
        Assert.Equal(2, resultado.TotalUnidadesDescontadas);
    }

    [Fact]
    public void Evaluar_ConReposicion_GeneraMermaYCortesia()
    {
        var resultado = _evaluador.Evaluar(MotivoMerma.Danado, 2, reponerSinCobro: true);

        Assert.Equal(2, resultado.Movimientos.Count);
        Assert.Contains(resultado.Movimientos, m => m.Tipo == "Merma");
        Assert.Contains(resultado.Movimientos, m => m.Tipo == "Cortesia");
        Assert.Equal(4, resultado.TotalUnidadesDescontadas);
    }

    [Fact]
    public void Evaluar_ConCantidadNoPositiva_Lanza()
    {
        Assert.Throws<InvalidOperationException>(
            () => _evaluador.Evaluar(MotivoMerma.Partido, 0, reponerSinCobro: false));
    }
}
