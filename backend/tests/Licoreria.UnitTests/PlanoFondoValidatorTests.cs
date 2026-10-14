using Licoreria.Application.Dtos;
using Licoreria.Application.Validators;

namespace Licoreria.UnitTests;

public sealed class PlanoFondoValidatorTests
{
    private static PlanoEditarDto Editar(decimal opacidad, string ajuste)
        => new(Guid.NewGuid(), "Plano", true, [], FondoOpacidad: opacidad, FondoAjuste: ajuste);

    [Theory]
    [InlineData(0, "contener")]
    [InlineData(1, "cubrir")]
    [InlineData(0.3, "estirar")]
    public void ValoresValidos_Pasan(double opacidad, string ajuste)
        => Assert.True(new PlanoEditarDtoValidator().Validate(Editar((decimal)opacidad, ajuste)).IsValid);

    [Fact]
    public void OpacidadFueraDeRango_Falla()
        => Assert.False(new PlanoEditarDtoValidator().Validate(Editar(1.5m, "contener")).IsValid);

    [Fact]
    public void AjusteDesconocido_Falla()
        => Assert.False(new PlanoEditarDtoValidator().Validate(Editar(0.5m, "girar")).IsValid);

    [Fact]
    public void Crear_ValidaOpacidadYAjuste()
    {
        var validador = new PlanoCrearDtoValidator();
        Assert.True(validador.Validate(new PlanoCrearDto("P", [])).IsValid);
        Assert.False(validador.Validate(new PlanoCrearDto("P", [], FondoOpacidad: -0.1m)).IsValid);
        Assert.False(validador.Validate(new PlanoCrearDto("P", [], FondoAjuste: "x")).IsValid);
    }
}
