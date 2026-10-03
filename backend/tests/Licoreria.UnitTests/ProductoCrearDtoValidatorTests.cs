using Licoreria.Application.Dtos;
using Licoreria.Application.Validators;
using Licoreria.Domain.Enums;

namespace Licoreria.UnitTests;

public class ProductoCrearDtoValidatorTests
{
    private readonly ProductoCrearDtoValidator _validator = new();

    private static VarianteCrearDto VarianteValida() => new(
        Id: null,
        Nombre: "Botella 0.75L",
        Sku: "LIC-RON-0001",
        UnidadMedidaId: Guid.NewGuid(),
        PrecioCompraUSD: 8.50m,
        PrecioVentaUSD: 12.00m,
        CodigosBarras: ["759100100101"]);

    private static ProductoCrearDto DtoValido() => new(
        Nombre: "Ron Cacique",
        Descripcion: "Ron añejo.",
        CategoriaId: Guid.NewGuid(),
        MarcaId: Guid.NewGuid(),
        ImpuestoId: null,
        Tipo: TipoProducto.Simple,
        GradoAlcoholico: 40m,
        ImagenUrl: null,
        Variantes: [VarianteValida()]);

    [Fact]
    public void Validar_DtoValido_EsValido()
    {
        Assert.True(_validator.Validate(DtoValido()).IsValid);
    }

    [Fact]
    public void Validar_SinVariantes_EsInvalido()
    {
        var dto = DtoValido() with { Variantes = [] };

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(ProductoCrearDto.Variantes));
    }

    [Fact]
    public void Validar_PrecioVentaMenorQueCompra_EsInvalido()
    {
        var dto = DtoValido() with
        {
            Variantes = [VarianteValida() with { PrecioVentaUSD = 1m }]
        };

        Assert.False(_validator.Validate(dto).IsValid);
    }

    [Fact]
    public void Validar_PrecioNegativo_EsInvalido()
    {
        var dto = DtoValido() with
        {
            Variantes = [VarianteValida() with { PrecioCompraUSD = -1m }]
        };

        Assert.False(_validator.Validate(dto).IsValid);
    }

    [Fact]
    public void Validar_SinNombreNiSku_EsInvalido()
    {
        var dto = DtoValido() with
        {
            Nombre = "",
            Variantes = [VarianteValida() with { Sku = "" }]
        };

        var resultado = _validator.Validate(dto);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(ProductoCrearDto.Nombre));
        Assert.Contains(resultado.Errors, e => e.PropertyName == "Variantes[0].Sku");
    }
}
