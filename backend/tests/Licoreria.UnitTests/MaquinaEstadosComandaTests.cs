using Licoreria.Domain.Enums;
using Licoreria.Domain.Services;

namespace Licoreria.UnitTests;

public class MaquinaEstadosComandaTests
{
    private readonly MaquinaEstadosComanda _maquina = new();

    [Theory]
    [InlineData(EstadoItemComanda.Recibido, EstadoItemComanda.EnProceso)]
    [InlineData(EstadoItemComanda.Recibido, EstadoItemComanda.Preparado)]
    [InlineData(EstadoItemComanda.Recibido, EstadoItemComanda.Cancelado)]
    [InlineData(EstadoItemComanda.EnProceso, EstadoItemComanda.Preparado)]
    [InlineData(EstadoItemComanda.EnProceso, EstadoItemComanda.Cancelado)]
    [InlineData(EstadoItemComanda.Preparado, EstadoItemComanda.Entregado)]
    [InlineData(EstadoItemComanda.Preparado, EstadoItemComanda.Cancelado)]
    public void PuedeTransicionar_PermiteTransicionesValidas(EstadoItemComanda actual, EstadoItemComanda nuevo)
    {
        Assert.True(_maquina.PuedeTransicionar(actual, nuevo));
    }

    [Theory]
    [InlineData(EstadoItemComanda.Recibido, EstadoItemComanda.Entregado)]
    [InlineData(EstadoItemComanda.EnProceso, EstadoItemComanda.Recibido)]
    [InlineData(EstadoItemComanda.Preparado, EstadoItemComanda.Recibido)]
    [InlineData(EstadoItemComanda.Preparado, EstadoItemComanda.EnProceso)]
    [InlineData(EstadoItemComanda.Entregado, EstadoItemComanda.Preparado)]
    [InlineData(EstadoItemComanda.Entregado, EstadoItemComanda.Cancelado)]
    [InlineData(EstadoItemComanda.Cancelado, EstadoItemComanda.Preparado)]
    [InlineData(EstadoItemComanda.Entregado, EstadoItemComanda.Entregado)]
    public void PuedeTransicionar_RechazaTransicionesInvalidas(EstadoItemComanda actual, EstadoItemComanda nuevo)
    {
        Assert.False(_maquina.PuedeTransicionar(actual, nuevo));
    }

    [Fact]
    public void Transicionar_ConTransicionInvalida_Lanza()
    {
        Assert.Throws<InvalidOperationException>(
            () => _maquina.Transicionar(EstadoItemComanda.Entregado, EstadoItemComanda.Preparado));
    }

    [Fact]
    public void EstadosTerminales_NoPermitenSalidas()
    {
        foreach (var terminal in new[] { EstadoItemComanda.Entregado, EstadoItemComanda.Cancelado })
        {
            foreach (var destino in Enum.GetValues<EstadoItemComanda>())
            {
                Assert.False(_maquina.PuedeTransicionar(terminal, destino));
            }
        }
    }
}
