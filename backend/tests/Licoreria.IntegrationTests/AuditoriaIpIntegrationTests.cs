using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Licoreria.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Licoreria.IntegrationTests;

/// <summary>La bitácora registra la IP del cliente real y no la del proxy, sin confiar en cabezales ajenos.</summary>
[Collection("api")]
public class AuditoriaIpIntegrationTests
{
    private const string CabeceraConexion = "X-Test-Conexion";

    private readonly LicoreriaApiFactory _factory;

    public AuditoriaIpIntegrationTests(LicoreriaApiFactory factory) => _factory = factory;

    /// <summary>Simula la IP de la conexión directa (TestServer no tiene una) antes del middleware de cabeceras.</summary>
    private sealed class FiltroConexion : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.Use((contexto, siguiente) =>
            {
                if (contexto.Request.Headers.TryGetValue(CabeceraConexion, out var ip))
                {
                    contexto.Connection.RemoteIpAddress = IPAddress.Parse(ip.ToString());
                }

                return siguiente();
            });
            next(app);
        };
    }

    private async Task<string?> CrearCategoriaDesdeAsync(string ipConexion, string? forwardedFor)
    {
        using var fabrica = _factory.WithWebHostBuilder(b =>
            b.ConfigureServices(s => s.AddSingleton<IStartupFilter, FiltroConexion>()));
        using var cliente = fabrica.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _factory.AccessToken);
        cliente.DefaultRequestHeaders.Add(CabeceraConexion, ipConexion);
        if (forwardedFor is not null)
        {
            cliente.DefaultRequestHeaders.Add("X-Forwarded-For", forwardedFor);
        }

        var nombre = $"Cat-{Guid.NewGuid():N}";
        var respuesta = await cliente.PostAsJsonAsync("/api/v1/categorias", new { nombre, descripcion = "ip", activo = true });
        respuesta.EnsureSuccessStatusCode();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LicoreriaDbContext>();
        var id = (await db.Categorias.SingleAsync(c => c.Nombre == nombre)).Id;
        return (await db.AuditLogs.SingleAsync(l => l.EntidadId == id && l.Accion == "crear")).Ip;
    }

    [Fact]
    public async Task ProxyPrivado_ConXForwardedFor_RegistraLaIpDelCliente()
    {
        var ip = await CrearCategoriaDesdeAsync("172.18.0.5", "198.51.100.23");

        Assert.Equal("198.51.100.23", ip);
    }

    [Fact]
    public async Task ConexionPublica_IgnoraElXForwardedForFalsificado()
    {
        var ip = await CrearCategoriaDesdeAsync("203.0.113.9", "1.2.3.4");

        Assert.Equal("203.0.113.9", ip);
    }
}
