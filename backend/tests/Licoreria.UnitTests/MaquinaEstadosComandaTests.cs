using Licoreria.Domain.Services;

namespace Licoreria.UnitTests;

public class MaquinaEstadosComandaTests
{
    private readonly MaquinaEstadosComanda _maquina = new();

    [Theory]
    [InlineData(EstadoComanda.Recibido, EstadoComanda.Preparado)]
    [InlineData(EstadoComanda.Recibido, EstadoComanda.Cancelado)]
    [InlineData(EstadoComanda.Preparado, EstadoComanda.Entregado)]
    [InlineData(EstadoComanda.Preparado, EstadoComanda.Cancelado)]
    public void PuedeTransicionar_PermiteTransicionesValidas(EstadoComanda actual, EstadoComanda nuevo)
    {
        Assert.True(_maquina.PuedeTransicionar(actual, nuevo));
    }

    [Theory]
    [InlineData(EstadoComanda.Recibido, EstadoComanda.Entregado)]
    [InlineData(EstadoComanda.Entregado, EstadoComanda.Cancelado)]
    [InlineData(EstadoComanda.Cancelado, EstadoComanda.Preparado)]
    public void PuedeTransicionar_RechazaTransicionesInvalidas(EstadoComanda actual, EstadoComanda nuevo)
    {
        Assert.False(_maquina.PuedeTransicionar(actual, nuevo));
    }

    [Fact]
    public void Transicionar_ConTransicionInvalida_Lanza()
    {
        Assert.Throws<InvalidOperationException>(
            () => _maquina.Transicionar(EstadoComanda.Entregado, EstadoComanda.Preparado));
    }
}
