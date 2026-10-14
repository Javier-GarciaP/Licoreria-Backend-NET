using Licoreria.Domain.Common;

namespace Licoreria.UnitTests;

public sealed class ExistenciaAgotadaExceptionTests
{
    [Fact]
    public void Constructor_ExponeProductoRequeridoYDisponible()
    {
        var ex = new ExistenciaAgotadaException("Coca-Cola", 2m, 1m);

        Assert.Equal("Coca-Cola", ex.Producto);
        Assert.Equal(2m, ex.Requerido);
        Assert.Equal(1m, ex.Disponible);
        Assert.Contains("Coca-Cola", ex.Message);
    }

    [Fact]
    public void PorcionesSinAbrir_EsUnaExistenciaAgotadaConLosEnvasesCerrados()
    {
        var ex = new PorcionesSinAbrirException("Vaso Coca-Cola", 2m, 0m, 3);

        Assert.IsAssignableFrom<ExistenciaAgotadaException>(ex);
        Assert.Equal(3, ex.EnvasesCerrados);
        Assert.Equal("Vaso Coca-Cola", ex.Producto);
        Assert.Contains("destape", ex.Message);
    }

    [Fact]
    public void Hereda_DeReglaNegocio_ParaMapearA422()
    {
        Assert.IsAssignableFrom<ReglaNegocioException>(new ExistenciaAgotadaException("X", 1m, 0m));
    }
}
