using System.Diagnostics;

namespace Licoreria.IntegrationTests;

/// <summary>Pruebas de la lógica que decide si las pruebas de integración se ejecutan, se omiten o fallan. No usan la base de datos.</summary>
public class BaseDeDatosDePruebasTests
{
    [Fact]
    public void Decidir_ConBaseDisponible_Ejecuta()
    {
        Assert.Equal(DecisionIntegracion.Ejecutar, BaseDeDatosDePruebas.Decidir(disponible: true, obligatoria: false));
        Assert.Equal(DecisionIntegracion.Ejecutar, BaseDeDatosDePruebas.Decidir(disponible: true, obligatoria: true));
    }

    [Fact]
    public void Decidir_SinBaseYNoObligatoria_Omite()
        => Assert.Equal(DecisionIntegracion.Omitir, BaseDeDatosDePruebas.Decidir(disponible: false, obligatoria: false));

    [Fact]
    public void Decidir_SinBaseYObligatoria_Falla()
        => Assert.Equal(DecisionIntegracion.Fallar, BaseDeDatosDePruebas.Decidir(disponible: false, obligatoria: true));

    [Theory]
    [InlineData("true", true)]
    [InlineData("TRUE", true)]
    [InlineData(" 1 ", true)]
    [InlineData("false", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void EsVerdadero_InterpretaLaVariableDeEntorno(string? valor, bool esperado)
        => Assert.Equal(esperado, BaseDeDatosDePruebas.EsVerdadero(valor));

    [Fact]
    public void ConexionAdministracion_ApuntaALaBaseDeMantenimientoConTiempoMaximo()
    {
        var conexion = BaseDeDatosDePruebas.ConexionAdministracion("Host=otro;Port=5433;Username=u;Password=p;Database=licoreria_test");

        Assert.Contains("Host=otro", conexion);
        Assert.Contains("Port=5433", conexion);
        Assert.Contains("Database=postgres", conexion);
        Assert.Contains("Timeout=3", conexion);
    }

    [Fact]
    public void Comprobar_ConServidorQueNoResponde_TerminaEnMenosDeCincoSegundosConElMotivo()
    {
        var reloj = Stopwatch.StartNew();

        // 192.0.2.1 pertenece a un rango reservado para documentación: no enruta, así que la conexión no se completa.
        var (disponible, error) = BaseDeDatosDePruebas.Comprobar("Host=192.0.2.1;Port=5432;Username=u;Password=p");

        Assert.False(disponible);
        Assert.False(string.IsNullOrWhiteSpace(error));
        Assert.True(reloj.Elapsed < TimeSpan.FromSeconds(5), $"La comprobación tardó {reloj.Elapsed.TotalSeconds:0.0} s.");
    }

    [Fact]
    public void MotivoNoDisponible_IndicaLasVariablesQueLoControlan()
    {
        // Solo se evalúa el texto si la base falta; con base disponible no hay motivo que mostrar.
        if (BaseDeDatosDePruebas.Disponible)
        {
            return;
        }

        Assert.Contains(BaseDeDatosDePruebas.VariableConexion, BaseDeDatosDePruebas.MotivoNoDisponible);
        Assert.Contains(BaseDeDatosDePruebas.VariableObligatoria, BaseDeDatosDePruebas.MotivoNoDisponible);
    }

    [HechoDeIntegracion]
    public void HechoDeIntegracion_ConBaseDisponible_NoSeOmite()
    {
        var atributo = new HechoDeIntegracionAttribute();

        Assert.Null(atributo.Skip);
    }

    [Fact]
    public void CategoriaIntegracionDescubridor_DeclaraLaCategoriaIntegration()
    {
        var rasgos = new CategoriaIntegracionDescubridor().GetTraits(null!).ToList();

        Assert.Equal(new KeyValuePair<string, string>("Category", "Integration"), Assert.Single(rasgos));
    }
}
