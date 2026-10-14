using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Licoreria.IntegrationTests;

/// <summary>Permiso de distribución, croquis de fondo, duplicado y plantillas de plano.</summary>
public partial class ApiEndpointsTests
{
    private static readonly byte[] PngMinimo = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0, 0, 0, 0, 0];

    /// <summary>Cliente autenticado con otro usuario sembrado (contraseña demo123).</summary>
    private async Task<HttpClient> ClienteDeAsync(string email)
    {
        var cliente = _factory.CreateClient();
        var login = await cliente.PostAsJsonAsync("/api/auth/login", new { username = email, password = "demo123" });
        login.EnsureSuccessStatusCode();
        var token = (await JsonAsync(login)).GetProperty("accessToken").GetString()!;
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return cliente;
    }

    private static MultipartFormDataContent Archivo(byte[] contenido, string nombre, string tipo)
    {
        var parte = new ByteArrayContent(contenido);
        parte.Headers.ContentType = new MediaTypeHeaderValue(tipo);
        return new MultipartFormDataContent { { parte, "archivo", nombre } };
    }

    private async Task<Guid> PlanoNuevoAsync(string nombre)
        => (await CrearPlanoAsync(nombre)).GetProperty("id").GetGuid();

    // ================= Permisos por rol =================

    [Fact]
    public async Task Host_NoPuedeEditarPlanosNiCrearMesas_PeroSiGestionarListaVip()
    {
        var planoId = await PlanoNuevoAsync("Plano Permisos");
        var zonaId = await CrearZonaAsync();
        using var host = await ClienteDeAsync("host1@licoreria.com");

        var editar = await host.PutAsJsonAsync($"/api/v1/planos/{planoId}", new { id = planoId, nombre = "X", activo = false, elementos = Array.Empty<object>() });
        var crearMesa = await host.PostAsJsonAsync("/api/v1/mesas", new { zonaId, numero = "H-1", capacidad = 2 });
        var crearZona = await host.PostAsJsonAsync("/api/v1/zonas", new { nombre = "Zona Host", tipo = "Mesas" });
        var duplicar = await host.PostAsJsonAsync($"/api/v1/planos/{planoId}/duplicar", new { });
        var fondo = await host.PostAsync($"/api/v1/planos/{planoId}/fondo", Archivo(PngMinimo, "a.png", "image/png"));

        Assert.Equal(HttpStatusCode.Forbidden, editar.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, crearMesa.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, crearZona.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, duplicar.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, fondo.StatusCode);

        var vip = await host.PostAsJsonAsync("/api/v1/lista-vip", new { nombre = "Invitado Host", personas = 2, fecha = DateTime.UtcNow.AddDays(1) });
        Assert.NotEqual(HttpStatusCode.Forbidden, vip.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await host.GetAsync("/api/v1/lista-vip")).StatusCode);
    }

    [Fact]
    public async Task Mesero_PuedeConsultarElPlanoYDesalojar_PeroNoEditar()
    {
        var planoId = await PlanoNuevoAsync("Plano Mesero");
        var mesaId = await CrearMesaAsync();
        using var mesero = await ClienteDeAsync("mesero1@licoreria.com");

        Assert.Equal(HttpStatusCode.OK, (await mesero.GetAsync($"/api/v1/planos/{planoId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await mesero.DeleteAsync($"/api/v1/planos/{planoId}")).StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, (await mesero.PostAsync($"/api/v1/mesas/{mesaId}/desalojar", null)).StatusCode);
    }

    // ================= Fondo =================

    [Fact]
    public async Task Fondo_SubirReemplazarYQuitar_GestionaElArchivo()
    {
        var planoId = await PlanoNuevoAsync("Plano Croquis");

        var subida = await _factory.Client.PostAsync($"/api/v1/planos/{planoId}/fondo", Archivo(PngMinimo, "croquis.png", "image/png"));
        Assert.Equal(HttpStatusCode.OK, subida.StatusCode);
        var url1 = (await JsonAsync(subida)).GetProperty("fondoImagenUrl").GetString()!;
        Assert.StartsWith("/uploads/planos/", url1);

        var reemplazo = await _factory.Client.PostAsync($"/api/v1/planos/{planoId}/fondo", Archivo(PngMinimo, "otro.png", "image/png"));
        var url2 = (await JsonAsync(reemplazo)).GetProperty("fondoImagenUrl").GetString()!;
        Assert.NotEqual(url1, url2);

        var quitar = await _factory.Client.DeleteAsync($"/api/v1/planos/{planoId}/fondo");
        Assert.Equal(HttpStatusCode.OK, quitar.StatusCode);
        Assert.Equal(JsonValueKind.Null, (await JsonAsync(quitar)).GetProperty("fondoImagenUrl").ValueKind);
    }

    [Fact]
    public async Task Fondo_ArchivoInvalido_Devuelve400()
    {
        var planoId = await PlanoNuevoAsync("Plano Croquis Invalido");

        var disfrazado = await _factory.Client.PostAsync($"/api/v1/planos/{planoId}/fondo", Archivo("<html></html>"u8.ToArray(), "a.png", "image/png"));
        var grande = await _factory.Client.PostAsync($"/api/v1/planos/{planoId}/fondo", Archivo(new byte[6 * 1024 * 1024], "a.png", "image/png"));

        Assert.Equal(HttpStatusCode.BadRequest, disfrazado.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, grande.StatusCode);
    }

    [Fact]
    public async Task Fondo_PlanoInexistente_Devuelve404()
    {
        var respuesta = await _factory.Client.PostAsync($"/api/v1/planos/{Guid.NewGuid()}/fondo", Archivo(PngMinimo, "a.png", "image/png"));

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Plano_Edicion_IgnoraUnaUrlDeFondoExterna()
    {
        var planoId = await PlanoNuevoAsync("Plano Url Externa");

        var editar = await _factory.Client.PutAsJsonAsync($"/api/v1/planos/{planoId}", new
        {
            id = planoId,
            nombre = "Plano Url Externa",
            activo = false,
            elementos = Array.Empty<object>(),
            fondoImagenUrl = "https://malicioso.example/x.png",
            fondoOpacidad = 0.3
        });

        Assert.Equal(HttpStatusCode.OK, editar.StatusCode);
        var body = await JsonAsync(editar);
        Assert.Equal(JsonValueKind.Null, body.GetProperty("fondoImagenUrl").ValueKind);
        Assert.Equal(0.3, body.GetProperty("fondoOpacidad").GetDouble());
    }

    [Fact]
    public async Task Plano_OpacidadFueraDeRango_Devuelve400()
    {
        var planoId = await PlanoNuevoAsync("Plano Opacidad");

        var editar = await _factory.Client.PutAsJsonAsync($"/api/v1/planos/{planoId}", new
        {
            id = planoId,
            nombre = "Plano Opacidad",
            activo = false,
            elementos = Array.Empty<object>(),
            fondoOpacidad = 1.5
        });

        Assert.Equal(HttpStatusCode.BadRequest, editar.StatusCode);
    }

    // ================= Duplicar y plantillas =================

    [Fact]
    public async Task Duplicar_CreaCopiaInactivaYDejaElOriginalIntacto()
    {
        var planoId = await PlanoNuevoAsync("Plano Original");
        await _factory.Client.PostAsync($"/api/v1/planos/{planoId}/fondo", Archivo(PngMinimo, "a.png", "image/png"));

        var respuesta = await _factory.Client.PostAsJsonAsync($"/api/v1/planos/{planoId}/duplicar", new { nombre = "Plano Copia" });

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var copia = await JsonAsync(respuesta);
        Assert.False(copia.GetProperty("activo").GetBoolean());
        Assert.Equal(1, copia.GetProperty("elementos").GetArrayLength());
        Assert.NotEqual(Guid.Empty, copia.GetProperty("id").GetGuid());

        var original = await JsonAsync(await _factory.Client.GetAsync($"/api/v1/planos/{planoId}"));
        Assert.True(original.GetProperty("activo").GetBoolean());
        Assert.NotEqual(
            original.GetProperty("fondoImagenUrl").GetString(),
            copia.GetProperty("fondoImagenUrl").GetString());
    }

    [Fact]
    public async Task Plantilla_NoSeVeSinPermisoNiSePuedeActivar()
    {
        var planoId = await PlanoNuevoAsync("Plano Base");
        var respuesta = await _factory.Client.PostAsJsonAsync($"/api/v1/planos/{planoId}/duplicar", new { nombre = "Plantilla Secreta", comoPlantilla = true });
        var plantillaId = (await JsonAsync(respuesta)).GetProperty("id").GetGuid();

        using var anonimo = _factory.CreateClient();
        var listaAnonima = await JsonAsync(await anonimo.GetAsync("/api/v1/planos?plantillas=true"));
        Assert.DoesNotContain(listaAnonima.EnumerateArray(), p => p.GetProperty("id").GetGuid() == plantillaId);
        Assert.Equal(HttpStatusCode.NotFound, (await anonimo.GetAsync($"/api/v1/planos/{plantillaId}")).StatusCode);

        var listaAdmin = await JsonAsync(await _factory.Client.GetAsync("/api/v1/planos?plantillas=true"));
        Assert.Contains(listaAdmin.EnumerateArray(), p => p.GetProperty("id").GetGuid() == plantillaId);
        var sinFiltro = await JsonAsync(await _factory.Client.GetAsync("/api/v1/planos"));
        Assert.DoesNotContain(sinFiltro.EnumerateArray(), p => p.GetProperty("id").GetGuid() == plantillaId);
        Assert.Equal(HttpStatusCode.OK, (await _factory.Client.GetAsync($"/api/v1/planos/{plantillaId}")).StatusCode);

        var activar = await _factory.Client.PutAsJsonAsync($"/api/v1/planos/{plantillaId}", new { id = plantillaId, nombre = "Plantilla Secreta", activo = true, elementos = Array.Empty<object>() });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, activar.StatusCode);
    }
}
