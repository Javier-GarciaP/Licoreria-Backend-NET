using Npgsql;

namespace Licoreria.IntegrationTests.Support;

/// <summary>Qué hacer con una prueba de integración según la disponibilidad de PostgreSQL.</summary>
public enum DecisionIntegracion
{
    /// <summary>La base de datos responde: la prueba se ejecuta.</summary>
    Ejecutar,

    /// <summary>No responde y no es obligatoria (equipo local): la prueba se omite con un motivo visible.</summary>
    Omitir,

    /// <summary>No responde y es obligatoria (CI): la prueba se ejecuta y falla con un mensaje claro.</summary>
    Fallar
}

/// <summary>
/// Conexión y disponibilidad de la base de datos de las pruebas de integración. Variables de entorno:
/// <c>LICORERIA_TEST_DB</c> (cadena de conexión del servidor, sin base de datos) y
/// <c>LICORERIA_TESTS_REQUIRE_DB</c> (<c>true</c> hace fallar, en lugar de omitir, si no hay base).
/// </summary>
public static class BaseDeDatosDePruebas
{
    public const string VariableConexion = "LICORERIA_TEST_DB";
    public const string VariableObligatoria = "LICORERIA_TESTS_REQUIRE_DB";
    public const string ConexionPorDefecto = "Host=localhost;Port=5432;Username=licoreria;Password=licoreria";
    public const string NombreBase = "licoreria_test";

    /// <summary>Tiempo máximo de la comprobación de disponibilidad (para no colgarse con un servidor mudo).</summary>
    public static readonly TimeSpan TiempoMaximoComprobacion = TimeSpan.FromSeconds(3);

    private static readonly Lazy<(bool Disponible, string? Error)> Estado =
        new(() => Comprobar(ConexionServidor), LazyThreadSafetyMode.ExecutionAndPublication);

    public static string ConexionServidor
        => Environment.GetEnvironmentVariable(VariableConexion) is { Length: > 0 } valor ? valor : ConexionPorDefecto;

    public static bool Obligatoria => EsVerdadero(Environment.GetEnvironmentVariable(VariableObligatoria));

    /// <summary>Se comprueba una sola vez por corrida.</summary>
    public static bool Disponible => Estado.Value.Disponible;

    public static string MotivoNoDisponible
        => $"PostgreSQL no está disponible ({Estado.Value.Error}). Inícielo o defina {VariableConexion}; " +
           $"con {VariableObligatoria}=true esta prueba falla en lugar de omitirse.";

    public static DecisionIntegracion Decidir(bool disponible, bool obligatoria)
        => disponible ? DecisionIntegracion.Ejecutar : obligatoria ? DecisionIntegracion.Fallar : DecisionIntegracion.Omitir;

    public static bool EsVerdadero(string? valor)
        => string.Equals(valor?.Trim(), "true", StringComparison.OrdinalIgnoreCase) || valor?.Trim() == "1";

    /// <summary>Cadena de conexión a la base de mantenimiento <c>postgres</c> con el tiempo máximo de la comprobación.</summary>
    public static string ConexionAdministracion() => ConexionAdministracion(ConexionServidor);

    public static string ConexionAdministracion(string conexionServidor)
        => new NpgsqlConnectionStringBuilder(conexionServidor)
        {
            Database = "postgres",
            Timeout = (int)TiempoMaximoComprobacion.TotalSeconds
        }.ConnectionString;

    /// <summary>Intenta abrir una conexión de mantenimiento; nunca espera más que el tiempo máximo (más un margen).</summary>
    public static (bool Disponible, string? Error) Comprobar(string conexionServidor)
    {
        try
        {
            using var cancelacion = new CancellationTokenSource(TiempoMaximoComprobacion + TimeSpan.FromSeconds(1));
            using var conexion = new NpgsqlConnection(ConexionAdministracion(conexionServidor));
            conexion.OpenAsync(cancelacion.Token).GetAwaiter().GetResult();
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.GetBaseException().Message);
        }
    }
}
