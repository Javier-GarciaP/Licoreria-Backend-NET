using Xunit.Abstractions;
using Xunit.Sdk;

namespace Licoreria.IntegrationTests.Support;

/// <summary>
/// <see cref="FactAttribute"/> de las pruebas que necesitan PostgreSQL. Declara la categoría
/// <c>Integration</c> (para filtrar con <c>--filter "Category=Integration"</c>) y se omite con un motivo
/// visible si no hay base de datos, salvo que <c>LICORERIA_TESTS_REQUIRE_DB=true</c>.
/// </summary>
[TraitDiscoverer("Licoreria.IntegrationTests.Support.CategoriaIntegracionDescubridor", "Licoreria.IntegrationTests")]
public sealed class HechoDeIntegracionAttribute : FactAttribute, ITraitAttribute
{
    public HechoDeIntegracionAttribute()
    {
        if (BaseDeDatosDePruebas.Decidir(BaseDeDatosDePruebas.Disponible, BaseDeDatosDePruebas.Obligatoria) == DecisionIntegracion.Omitir)
        {
            Skip = BaseDeDatosDePruebas.MotivoNoDisponible;
        }
    }
}

/// <summary>Igual que <see cref="HechoDeIntegracionAttribute"/> para pruebas con datos (<c>[InlineData]</c>).</summary>
[TraitDiscoverer("Licoreria.IntegrationTests.Support.CategoriaIntegracionDescubridor", "Licoreria.IntegrationTests")]
public sealed class TeoriaDeIntegracionAttribute : TheoryAttribute, ITraitAttribute
{
    public TeoriaDeIntegracionAttribute()
    {
        if (BaseDeDatosDePruebas.Decidir(BaseDeDatosDePruebas.Disponible, BaseDeDatosDePruebas.Obligatoria) == DecisionIntegracion.Omitir)
        {
            Skip = BaseDeDatosDePruebas.MotivoNoDisponible;
        }
    }
}

/// <summary>Añade el rasgo <c>Category=Integration</c> a las pruebas marcadas con los atributos de integración.</summary>
public sealed class CategoriaIntegracionDescubridor : ITraitDiscoverer
{
    public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
    {
        yield return new KeyValuePair<string, string>("Category", "Integration");
    }
}
