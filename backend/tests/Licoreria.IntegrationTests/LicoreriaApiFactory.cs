using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Licoreria.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Licoreria.IntegrationTests;

/// <summary>
/// Fábrica de la API para pruebas de integración. Crea una base de datos de
/// pruebas, aplica las migraciones y expone un cliente autenticado.
/// </summary>
public sealed class LicoreriaApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string DbName = "licoreria_test";
    private const string AdminConn = "Host=localhost;Port=5432;Username=licoreria;Password=licoreria";

    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public HttpClient Client { get; private set; } = null!;
    public string AccessToken { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await using (var conn = new NpgsqlConnection($"{AdminConn};Database=postgres"))
        {
            await conn.OpenAsync();
            await using var drop = new NpgsqlCommand($"DROP DATABASE IF EXISTS {DbName} WITH (FORCE)", conn);
            await drop.ExecuteNonQueryAsync();
            await using var create = new NpgsqlCommand($"CREATE DATABASE {DbName}", conn);
            await create.ExecuteNonQueryAsync();
        }

        Client = CreateClient();

        using (var scope = Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LicoreriaDbContext>();
            await context.Database.MigrateAsync();
        }

        var login = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "admin@licoreria.com",
            password = "admin123"
        });

        login.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        AccessToken = document.RootElement.GetProperty("accessToken").GetString()!;
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"{AdminConn};Database={DbName}"
            });
        });
    }
}

/// <summary>Colección compartida para reutilizar la API entre pruebas.</summary>
[CollectionDefinition("api")]
public sealed class ApiCollection : ICollectionFixture<LicoreriaApiFactory>
{
}
