using Licoreria.Domain.Services;

namespace Licoreria.UnitTests;

public class DetectorConflictosReservaTests
{
    private readonly DetectorConflictosReserva _detector = new();
    private readonly Guid _mesa = Guid.NewGuid();

    [Fact]
    public void HayConflicto_CuandoLosIntervalosSeSolapan_DevuelveTrue()
    {
        var existente = new IntervaloReserva(_mesa, new DateTime(2026, 10, 1, 20, 0, 0), new DateTime(2026, 10, 1, 22, 0, 0));
        var candidata = new IntervaloReserva(_mesa, new DateTime(2026, 10, 1, 21, 0, 0), new DateTime(2026, 10, 1, 23, 0, 0));

        Assert.True(_detector.HayConflicto(existente, candidata));
    }

    [Fact]
    public void HayConflicto_CuandoSonContiguos_DevuelveFalse()
    {
        var existente = new IntervaloReserva(_mesa, new DateTime(2026, 10, 1, 20, 0, 0), new DateTime(2026, 10, 1, 22, 0, 0));
        var candidata = new IntervaloReserva(_mesa, new DateTime(2026, 10, 1, 22, 0, 0), new DateTime(2026, 10, 1, 23, 0, 0));

        Assert.False(_detector.HayConflicto(existente, candidata));
    }

    [Fact]
    public void HayConflicto_EnMesasDistintas_DevuelveFalse()
    {
        var existente = new IntervaloReserva(_mesa, new DateTime(2026, 10, 1, 20, 0, 0), new DateTime(2026, 10, 1, 22, 0, 0));
        var candidata = new IntervaloReserva(Guid.NewGuid(), new DateTime(2026, 10, 1, 21, 0, 0), new DateTime(2026, 10, 1, 23, 0, 0));

        Assert.False(_detector.HayConflicto(existente, candidata));
    }

    [Fact]
    public void HayConflicto_ConIntervaloInvalido_Lanza()
    {
        var invalido = new IntervaloReserva(_mesa, new DateTime(2026, 10, 1, 22, 0, 0), new DateTime(2026, 10, 1, 20, 0, 0));
        var candidata = new IntervaloReserva(_mesa, new DateTime(2026, 10, 1, 21, 0, 0), new DateTime(2026, 10, 1, 23, 0, 0));

        Assert.Throws<InvalidOperationException>(() => _detector.HayConflicto(invalido, candidata));
    }
}
