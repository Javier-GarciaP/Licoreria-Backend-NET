using Licoreria.Domain.Enums;
using Licoreria.Domain.Services;

namespace Licoreria.UnitTests;

public class EvaluadorSaludStockTests
{
    private readonly EvaluadorSaludStock _evaluador = new();

    [Fact]
    public void SinStock_CuandoNoHayExistencias_SugiereReponerHastaElMaximo()
    {
        var informe = _evaluador.Evaluar("LIC-RON-0001", stockActual: 0, stockMinimo: 5, stockMaximo: 60);

        Assert.Equal(EstadoSaludStock.SinStock, informe.Estado);
        Assert.Equal(60, informe.UnidadesCompraSugeridas);
    }

    [Fact]
    public void RiesgoCritico_CuandoEstaEnElMinimo_SugiereLaDiferencia()
    {
        var informe = _evaluador.Evaluar("LIC-RON-0001", stockActual: 5, stockMinimo: 5, stockMaximo: 60);

        Assert.Equal(EstadoSaludStock.RiesgoCritico, informe.Estado);
        Assert.Equal(55, informe.UnidadesCompraSugeridas);
    }

    [Fact]
    public void Subabastecido_CuandoEstaBajoElPuntoMedio()
    {
        var informe = _evaluador.Evaluar("LIC-RON-0001", stockActual: 30, stockMinimo: 5, stockMaximo: 60);

        Assert.Equal(EstadoSaludStock.Subabastecido, informe.Estado);
    }

    [Fact]
    public void Optimo_CuandoEstaEntreElPuntoMedioYElMaximo()
    {
        var informe = _evaluador.Evaluar("LIC-RON-0001", stockActual: 40, stockMinimo: 5, stockMaximo: 60);

        Assert.Equal(EstadoSaludStock.Optimo, informe.Estado);
        Assert.Equal(0, informe.UnidadesCompraSugeridas);
    }

    [Fact]
    public void Sobreabastecido_CuandoEstaExactamenteEnElMaximo()
    {
        var informe = _evaluador.Evaluar("LIC-RON-0001", stockActual: 60, stockMinimo: 5, stockMaximo: 60);

        Assert.Equal(EstadoSaludStock.Sobreabastecido, informe.Estado);
    }

    [Fact]
    public void Excesivo_CuandoSuperaElMaximo()
    {
        var informe = _evaluador.Evaluar("LIC-RON-0001", stockActual: 100, stockMinimo: 5, stockMaximo: 60);

        Assert.Equal(EstadoSaludStock.Excesivo, informe.Estado);
    }

    [Fact]
    public void Evaluar_ConUmbralesInvalidos_LanzaExcepcion()
    {
        Assert.Throws<InvalidOperationException>(
            () => _evaluador.Evaluar("LIC-RON-0001", stockActual: 10, stockMinimo: 60, stockMaximo: 5));
    }
}
