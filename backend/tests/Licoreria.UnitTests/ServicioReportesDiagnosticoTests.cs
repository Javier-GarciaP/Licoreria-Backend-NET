using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Application.Services;
using Licoreria.Domain.Services;
using Licoreria.UnitTests.Support;
using Moq;

namespace Licoreria.UnitTests;

/// <summary>Diagnóstico de salud del inventario y límites del dashboard de <see cref="ServicioReportes"/>.</summary>
public sealed class ServicioReportesDiagnosticoTests
{
    private readonly Mock<IReportesRepository> _repo = new();

    private ServicioReportes CrearServicio()
        => new(_repo.Object, RelojFijo.Crear().Object, new EvaluadorSaludStock());

    private static StockSaludCrudoDto Stock(string sku, decimal cantidad, decimal minimo, decimal maximo)
        => new(Guid.NewGuid(), sku, "Producto " + sku, "750 ml", cantidad, minimo, maximo);

    [Fact]
    public async Task Diagnostico_SinStockOptimoYExceso_ContabilizaCadaEstadoYSugiereUnidades()
    {
        _repo.Setup(r => r.ObtenerStockSaludAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[]
        {
            Stock("SIN", cantidad: 0m, minimo: 5m, maximo: 20m),
            Stock("OPT", cantidad: 15m, minimo: 5m, maximo: 20m),
            Stock("EXC", cantidad: 25m, minimo: 5m, maximo: 20m),
        });

        var diag = await CrearServicio().ObtenerDiagnosticoInventarioAsync();

        Assert.Equal(3, diag.Total);
        Assert.Equal(1, diag.SinStock);
        Assert.Equal(1, diag.Optimo);
        Assert.Equal(1, diag.Excesivo);
        Assert.Equal(0, diag.RiesgoCritico + diag.Subabastecido + diag.Sobreabastecido);
        Assert.Equal(20, diag.Items.Single(i => i.Sku == "SIN").UnidadesCompraSugeridas);
        Assert.Equal(0, diag.Items.Single(i => i.Sku == "OPT").UnidadesCompraSugeridas);
        Assert.Equal(0, diag.Items.Single(i => i.Sku == "EXC").UnidadesCompraSugeridas);
    }

    [Fact]
    public async Task Diagnostico_ProductoEnRiesgoCritico_SugiereLlegarAlMaximo()
    {
        _repo.Setup(r => r.ObtenerStockSaludAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { Stock("RIE", 3m, 5m, 20m) });

        var diag = await CrearServicio().ObtenerDiagnosticoInventarioAsync();

        var item = Assert.Single(diag.Items);
        Assert.Equal("RiesgoCritico", item.Estado);
        Assert.Equal(17, item.UnidadesCompraSugeridas);
        Assert.False(string.IsNullOrWhiteSpace(item.MensajeDiagnostico));
    }

    [Fact]
    public async Task Diagnostico_SinProductos_DevuelveTodoEnCero()
    {
        _repo.Setup(r => r.ObtenerStockSaludAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<StockSaludCrudoDto>());

        var diag = await CrearServicio().ObtenerDiagnosticoInventarioAsync();

        Assert.Equal(0, diag.Total);
        Assert.Empty(diag.Items);
    }

    [Fact]
    public async Task Diagnostico_ConUmbralesInvalidos_PropagaElErrorDelDominio()
    {
        _repo.Setup(r => r.ObtenerStockSaludAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { Stock("MAL", 5m, minimo: 10m, maximo: 2m) });

        await Assert.ThrowsAsync<InvalidOperationException>(() => CrearServicio().ObtenerDiagnosticoInventarioAsync());
    }
}
