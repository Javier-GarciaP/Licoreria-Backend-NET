using Licoreria.Application.Dtos;
using Licoreria.Application.Validators;
using Licoreria.Domain.Entities;

namespace Licoreria.UnitTests;

public class ModificadorTests
{
    [Fact]
    public void Actualizar_DatosValidos_RegistraLaModificacion()
    {
        var modificador = new Modificador { Nombre = "Doble hielo", PrecioAdicional = 0m };

        modificador.Actualizar("Doble hielo", 1.5m, true);

        Assert.Equal(1.5m, modificador.PrecioAdicional);
        Assert.NotNull(modificador.LastModifiedAt);
    }

    [Fact]
    public void Actualizar_PrecioNegativo_Lanza()
    {
        var modificador = new Modificador { Nombre = "Extra limón" };

        Assert.Throws<InvalidOperationException>(() => modificador.Actualizar("Extra limón", -1m, true));
    }

    [Fact]
    public void Actualizar_SinNombre_Lanza()
    {
        var modificador = new Modificador();

        Assert.Throws<InvalidOperationException>(() => modificador.Actualizar("", 1m, true));
    }
}

public class ModificadorValidatorsTests
{
    private readonly ModificadorCrearDtoValidator _crear = new();
    private readonly ModificadorEditarDtoValidator _editar = new();
    private readonly AsignarModificadorDtoValidator _asignar = new();

    [Fact]
    public void Crear_DtoValido_EsValido()
        => Assert.True(_crear.Validate(new ModificadorCrearDto("Doble hielo", 0m)).IsValid);

    [Fact]
    public void Crear_SinNombre_EsInvalido()
        => Assert.False(_crear.Validate(new ModificadorCrearDto("", 0m)).IsValid);

    [Fact]
    public void Crear_PrecioNegativo_EsInvalido()
        => Assert.False(_crear.Validate(new ModificadorCrearDto("Extra", -1m)).IsValid);

    [Fact]
    public void Editar_IdVacio_EsInvalido()
        => Assert.False(_editar.Validate(new ModificadorEditarDto(Guid.Empty, "Extra", 1m, true)).IsValid);

    [Fact]
    public void Asignar_MaximoMenorQueMinimo_EsInvalido()
        => Assert.False(_asignar.Validate(new AsignarModificadorDto(Guid.NewGuid(), Minimo: 2, Maximo: 1)).IsValid);

    [Fact]
    public void Asignar_DtoValido_EsValido()
        => Assert.True(_asignar.Validate(new AsignarModificadorDto(Guid.NewGuid())).IsValid);
}
