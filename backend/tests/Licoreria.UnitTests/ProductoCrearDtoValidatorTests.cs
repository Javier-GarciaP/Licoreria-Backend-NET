using Licoreria.Application.Dtos;
using Licoreria.Application.Validators;

namespace Licoreria.UnitTests;

public class ProductoCrearDtoValidatorTests
{
    private readonly ProductoCrearDtoValidator _validator = new();

    private static ProductoCrearDto DtoValido() => new(
        Nombre: "Ron Cacique",
        Descripcion: "Ron añejo.",
        Sku: "LIC-RON-0001",
        CodigoBarras: "759100100101",
        PrecioCompraUSD: 8.50m,
        PrecioVentaUSD: 12.00m,
        Stock: 10,
        StockMinimo: 5,
        StockMaximo: 60,
        CategoriaId: Guid.NewGuid(),
        MarcaId: Guid.NewGuid(),
        UnidadMedidaId: Guid.NewGuid(),
        ImagenUrl: null);

    [Fact]
    public void Validar_DtoValido_EsValido()
    {
        Assert.True(_validator.Validate(DtoValido()).IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validar_PrecioNoPositivo_EsInvalido(decimal precio)
    {
        var dto = DtoValido() with { PrecioVentaUSD = precio };

        Assert.False(_validator.Validate(dto).IsValid);
    }

    [Fact]
    public void Validar_StockNegativo_EsInvalido()
    {
        var dto = DtoValido() with { Stock = -1 };

        Assert.False(_validator.Validate(dto).IsValid);
    }

    [Fact]
    public void Validar_StockMaximoNoMayorAlMinimo_EsInvalido()
    {
        var dto = DtoValido() with { StockMinimo = 10, StockMaximo = 10 };

        Assert.False(_validator.Validate(dto).IsValid);
    }

    [Fact]
    public void Validar_SinNombreNiSku_EsInvalido()
    {
        var dto = DtoValido() with { Nombre = "", Sku = "" };

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(ProductoCrearDto.Nombre));
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(ProductoCrearDto.Sku));
    }
}
