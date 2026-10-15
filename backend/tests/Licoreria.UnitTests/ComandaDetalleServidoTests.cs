using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.UnitTests;

/// <summary>Momento en que un ítem de comanda se sirve por primera vez.</summary>
public sealed class ComandaDetalleServidoTests
{
    private static readonly DateTime T1 = new(2026, 10, 14, 21, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime T2 = T1.AddMinutes(5);

    [Fact]
    public void CambiarEstado_PrimerServicio_AsignaServidoEn()
    {
        var detalle = new ComandaDetalle();

        detalle.CambiarEstado(EstadoItemComanda.Preparado, T1);

        Assert.Equal(T1, detalle.ServidoEn);
    }

    [Fact]
    public void CambiarEstado_PreparadoAEntregado_NoSobrescribeServidoEn()
    {
        var detalle = new ComandaDetalle();
        detalle.CambiarEstado(EstadoItemComanda.Preparado, T1);

        detalle.CambiarEstado(EstadoItemComanda.Entregado, T2);

        Assert.Equal(T1, detalle.ServidoEn);
        Assert.Equal(EstadoItemComanda.Entregado, detalle.Estado);
    }

    [Fact]
    public void CambiarEstado_EntregadoDirecto_AsignaServidoEn()
    {
        var detalle = new ComandaDetalle();

        detalle.CambiarEstado(EstadoItemComanda.Entregado, T1);

        Assert.Equal(T1, detalle.ServidoEn);
    }

    [Theory]
    [InlineData(EstadoItemComanda.EnProceso)]
    [InlineData(EstadoItemComanda.Cancelado)]
    public void CambiarEstado_SinServir_NoAsignaServidoEn(EstadoItemComanda estado)
    {
        var detalle = new ComandaDetalle();

        detalle.CambiarEstado(estado, T1);

        Assert.Null(detalle.ServidoEn);
    }

    [Fact]
    public void CambiarEstado_ServidoLuegoCancelado_ConservaLaMarca()
    {
        var detalle = new ComandaDetalle();
        detalle.CambiarEstado(EstadoItemComanda.Preparado, T1);

        detalle.CambiarEstado(EstadoItemComanda.Cancelado, T2);

        Assert.Equal(T1, detalle.ServidoEn);
    }

    [Fact]
    public void CambiarEstado_SinReloj_UsaLaHoraActual()
    {
        var detalle = new ComandaDetalle();
        var antes = DateTime.UtcNow;

        detalle.CambiarEstado(EstadoItemComanda.Preparado);

        Assert.NotNull(detalle.ServidoEn);
        Assert.InRange(detalle.ServidoEn!.Value, antes, DateTime.UtcNow);
    }
}
