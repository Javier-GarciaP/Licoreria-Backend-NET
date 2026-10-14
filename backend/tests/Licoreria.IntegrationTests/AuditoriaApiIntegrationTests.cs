using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Licoreria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Licoreria.IntegrationTests;

/// <summary>Consulta de la bitácora (filtros, detalle, permisos, inmutabilidad) y eventos de acceso.</summary>
[Collection("api")]
public class AuditoriaApiIntegrationTests
{
    private readonly LicoreriaApiFactory _factory;

    public AuditoriaApiIntegrationTests(LicoreriaApiFactory factory) => _factory = factory;

    private static async Task<JsonElement> JsonAsync(HttpResponseMessage response)
        => JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.Clone();

    private async Task<Guid> CrearCategoriaAsync(string nombre)
    {
        var respuesta = await _factory.Client.PostAsJsonAsync("/api/v1/categorias", new { nombre, descripcion = "aud", activo = true });
        respuesta.EnsureSuccessStatusCode();
        return (await JsonAsync(respuesta)).GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task IntentoFallido_DejaEntradaDeAccesoAunqueLaRespuestaSea401()
    {
        var correo = $"nadie-{Guid.NewGuid():N}@licoreria.com";
        using var cliente = _factory.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync("/api/auth/login", new { username = correo, password = "incorrecta" });

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LicoreriaDbContext>();
        var log = await db.AuditLogs.SingleAsync(l => l.Usuario == correo);
        Assert.Equal("intento-fallido", log.Accion);
        Assert.Equal("Evento", log.Tipo.ToString());
        Assert.DoesNotContain("incorrecta", log.Datos ?? string.Empty);
        Assert.DoesNotContain("incorrecta", await respuesta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Listado_FiltraPorEntidadAccionTipoYCombinados()
    {
        var nombre = $"Cat-{Guid.NewGuid():N}";
        var id = await CrearCategoriaAsync(nombre);

        var porEntidad = await JsonAsync(await _factory.Client.GetAsync("/api/v1/auditoria?entidad=Categoria&accion=crear&tipo=Cambio&pageSize=100"));
        var items = porEntidad.GetProperty("items").EnumerateArray().ToList();
        Assert.Contains(items, i => i.GetProperty("entidadId").GetGuid() == id);
        Assert.All(items, i =>
        {
            Assert.Equal("Categoria", i.GetProperty("entidad").GetString());
            Assert.Equal("crear", i.GetProperty("accion").GetString());
            Assert.Equal("Cambio", i.GetProperty("tipo").GetString());
        });

        var eventos = await JsonAsync(await _factory.Client.GetAsync("/api/v1/auditoria?tipo=Evento&pageSize=100"));
        Assert.All(eventos.GetProperty("items").EnumerateArray(), i => Assert.Equal("Evento", i.GetProperty("tipo").GetString()));
    }

    [Fact]
    public async Task Detalle_DevuelveLaEntradaConSusValoresYEntidadesListaLasConRegistros()
    {
        var id = await CrearCategoriaAsync($"Cat-{Guid.NewGuid():N}");
        var listado = await JsonAsync(await _factory.Client.GetAsync("/api/v1/auditoria?entidad=Categoria&accion=crear&pageSize=100"));
        var entradaId = listado.GetProperty("items").EnumerateArray()
            .First(i => i.GetProperty("entidadId").GetGuid() == id).GetProperty("id").GetGuid();

        var detalle = await _factory.Client.GetAsync($"/api/v1/auditoria/{entradaId}");
        Assert.Equal(HttpStatusCode.OK, detalle.StatusCode);
        var cuerpo = await JsonAsync(detalle);
        Assert.Contains("despues", cuerpo.GetProperty("datos").GetString());

        Assert.Equal(HttpStatusCode.NotFound, (await _factory.Client.GetAsync($"/api/v1/auditoria/{Guid.NewGuid()}")).StatusCode);

        var entidades = await JsonAsync(await _factory.Client.GetAsync("/api/v1/auditoria/entidades"));
        Assert.Contains(entidades.EnumerateArray(), e => e.GetString() == "Categoria");
    }

    [Fact]
    public async Task Cajero_RecibeForbiddenEnTodasLasRutasDeLaBitacora()
    {
        using var cliente = _factory.CreateClient();
        var login = await cliente.PostAsJsonAsync("/api/auth/login", new { username = "cajero1@licoreria.com", password = "demo123" });
        login.EnsureSuccessStatusCode();
        var token = (await JsonAsync(login)).GetProperty("accessToken").GetString();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        Assert.Equal(HttpStatusCode.Forbidden, (await cliente.GetAsync("/api/v1/auditoria")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await cliente.GetAsync("/api/v1/auditoria/entidades")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await cliente.GetAsync($"/api/v1/auditoria/{Guid.NewGuid()}")).StatusCode);
    }

    [Theory]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    [InlineData("PATCH")]
    [InlineData("POST")]
    public async Task Bitacora_NoExponeRutasDeEscritura(string metodo)
    {
        var id = Guid.NewGuid();
        foreach (var ruta in new[] { "/api/v1/auditoria", $"/api/v1/auditoria/{id}" })
        {
            var respuesta = await _factory.Client.SendAsync(new HttpRequestMessage(new HttpMethod(metodo), ruta)
            {
                Content = JsonContent.Create(new { })
            });

            Assert.True(
                respuesta.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.MethodNotAllowed,
                $"{metodo} {ruta} respondió {(int)respuesta.StatusCode}");
        }
    }

    [Fact]
    public async Task Listado_AceptaFechasSinZonaYConRango()
    {
        await CrearCategoriaAsync($"Cat-{Guid.NewGuid():N}");
        var hoy = DateTime.UtcNow.Date;

        var enRango = await _factory.Client.GetAsync($"/api/v1/auditoria?desde={hoy:yyyy-MM-dd}&hasta={hoy.AddDays(1):yyyy-MM-ddTHH:mm:ssZ}&usuarioId=00000000-0000-0000-0000-000000000000");
        Assert.Equal(HttpStatusCode.OK, enRango.StatusCode);
        Assert.Equal(0, (await JsonAsync(enRango)).GetProperty("totalItems").GetInt32());

        var futuro = await JsonAsync(await _factory.Client.GetAsync($"/api/v1/auditoria?desde={hoy.AddDays(2):yyyy-MM-dd}"));
        Assert.Equal(0, futuro.GetProperty("totalItems").GetInt32());

        var desdeHoy = await JsonAsync(await _factory.Client.GetAsync($"/api/v1/auditoria?desde={hoy:yyyy-MM-dd}"));
        Assert.True(desdeHoy.GetProperty("totalItems").GetInt32() > 0);
    }
}
