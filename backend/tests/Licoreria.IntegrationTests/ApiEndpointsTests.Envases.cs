using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Licoreria.IntegrationTests.Support;

namespace Licoreria.IntegrationTests;

/// <summary>Destapar, listar y descartar envases contra PostgreSQL real.</summary>
public partial class ApiEndpointsTests
{
    private Task<HttpClient> ClienteComoAsync(string correo) => ClienteDeAsync(correo);

    private Task<HttpResponseMessage> DestaparAsync(Guid vaso, Guid presentacion, HttpClient? cliente = null)
        => (cliente ?? _factory.Client).PostAsJsonAsync(
            "/api/v1/envases-abiertos", new { porcionVarianteId = vaso, presentacionVarianteId = presentacion });

    private async Task<decimal> ExistenciaAsync(Guid productoId, Guid varianteId)
        => (await VarianteDeAsync(productoId, varianteId)).GetProperty("cantidad").GetDecimal();

    [HechoDeIntegracion]
    public async Task Envase_Destapar_LaBotellaDeUnLitro_CreaCuatroVasosYDescuentaUnaBotella()
    {
        var cola = await CrearColaAsync(litros: 3);
        await DefinirOrigenesAsync(cola.Vaso, (cola.Litro, 4), (cola.DosLitros, 8));

        var respuesta = await DestaparAsync(cola.Vaso, cola.Litro);

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var envase = await JsonAsync(respuesta);
        Assert.Equal(4, envase.GetProperty("porcionesRestantes").GetInt32());
        Assert.Equal("Abierto", envase.GetProperty("estado").GetString());
        Assert.Equal(2m, await ExistenciaAsync(cola.ProductoId, cola.Litro));
        Assert.Equal(5m, await ExistenciaAsync(cola.ProductoId, cola.DosLitros));

        var vaso = await VarianteDeAsync(cola.ProductoId, cola.Vaso);
        Assert.Equal(4, vaso.GetProperty("porcionesDisponibles").GetInt32());
    }

    [HechoDeIntegracion]
    public async Task Envase_Destapar_ElegirLaDeDosLitros_CreaOchoVasos()
    {
        var cola = await CrearColaAsync();
        await DefinirOrigenesAsync(cola.Vaso, (cola.Litro, 4), (cola.DosLitros, 8));

        var envase = await JsonAsync(await DestaparAsync(cola.Vaso, cola.DosLitros));

        Assert.Equal(8, envase.GetProperty("porcionesIniciales").GetInt32());
        Assert.Equal(3m, await ExistenciaAsync(cola.ProductoId, cola.Litro));
        Assert.Equal(4m, await ExistenciaAsync(cola.ProductoId, cola.DosLitros));
    }

    [HechoDeIntegracion]
    public async Task Envase_Destapar_UnaPresentacionSinExistencia_Devuelve422()
    {
        var cola = await CrearColaAsync(litros: 0);
        await DefinirOrigenesAsync(cola.Vaso, (cola.Litro, 4));

        var respuesta = await DestaparAsync(cola.Vaso, cola.Litro);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
    }

    [HechoDeIntegracion]
    public async Task Envase_Destapar_UnaPresentacionQueNoEsOrigen_Devuelve422()
    {
        var cola = await CrearColaAsync();
        await DefinirOrigenesAsync(cola.Vaso, (cola.Litro, 4));

        var respuesta = await DestaparAsync(cola.Vaso, cola.DosLitros);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
    }

    [HechoDeIntegracion]
    public async Task Envase_Listar_MuestraLosAbiertosDeLaPorcion()
    {
        var cola = await CrearColaAsync();
        await DefinirOrigenesAsync(cola.Vaso, (cola.Litro, 4), (cola.DosLitros, 8));
        await DestaparAsync(cola.Vaso, cola.Litro);
        await DestaparAsync(cola.Vaso, cola.DosLitros);

        var lista = await JsonAsync(await _factory.Client.GetAsync($"/api/v1/envases-abiertos?porcionVarianteId={cola.Vaso}"));

        Assert.Equal(2, lista.GetArrayLength());
        // El más antiguo primero.
        Assert.Equal(4, lista[0].GetProperty("porcionesIniciales").GetInt32());
        Assert.Contains("Vaso", lista[0].GetProperty("porcionNombre").GetString());
    }

    [HechoDeIntegracion]
    public async Task Envase_Descartar_RetiraElEnvaseYSeRegistraLaMerma()
    {
        var cola = await CrearColaAsync();
        await DefinirOrigenesAsync(cola.Vaso, (cola.Litro, 4));
        var envase = await JsonAsync(await DestaparAsync(cola.Vaso, cola.Litro));
        var id = envase.GetProperty("id").GetGuid();

        var descartar = await _factory.Client.PostAsJsonAsync($"/api/v1/envases-abiertos/{id}/descartar", new { motivo = "Derrame" });

        Assert.Equal(HttpStatusCode.OK, descartar.StatusCode);
        var descartado = await JsonAsync(descartar);
        Assert.Equal("Descartado", descartado.GetProperty("estado").GetString());
        Assert.Equal("Derrame", descartado.GetProperty("motivoDescarte").GetString());

        var abiertos = await JsonAsync(await _factory.Client.GetAsync($"/api/v1/envases-abiertos?porcionVarianteId={cola.Vaso}"));
        Assert.Equal(0, abiertos.GetArrayLength());
        Assert.Equal(0, (await VarianteDeAsync(cola.ProductoId, cola.Vaso)).GetProperty("porcionesDisponibles").GetInt32());
    }

    [HechoDeIntegracion]
    public async Task Envase_Descartar_SinMotivo_Devuelve422YUnEnvaseInexistente_404()
    {
        var cola = await CrearColaAsync();
        await DefinirOrigenesAsync(cola.Vaso, (cola.Litro, 4));
        var id = (await JsonAsync(await DestaparAsync(cola.Vaso, cola.Litro))).GetProperty("id").GetGuid();

        var sinMotivo = await _factory.Client.PostAsJsonAsync($"/api/v1/envases-abiertos/{id}/descartar", new { motivo = "" });
        var inexistente = await _factory.Client.PostAsJsonAsync($"/api/v1/envases-abiertos/{Guid.NewGuid()}/descartar", new { motivo = "x" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, sinMotivo.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, inexistente.StatusCode);
    }

    [HechoDeIntegracion]
    public async Task Envase_UnMeseroNoPuedeDestaparNiDescartar_PeroSiConsultar()
    {
        var cola = await CrearColaAsync();
        await DefinirOrigenesAsync(cola.Vaso, (cola.Litro, 4));
        var id = (await JsonAsync(await DestaparAsync(cola.Vaso, cola.Litro))).GetProperty("id").GetGuid();
        using var mesero = await ClienteComoAsync("mesero1@licoreria.com");

        var destapar = await DestaparAsync(cola.Vaso, cola.Litro, mesero);
        var descartar = await mesero.PostAsJsonAsync($"/api/v1/envases-abiertos/{id}/descartar", new { motivo = "x" });
        var consultar = await mesero.GetAsync("/api/v1/envases-abiertos");

        Assert.Equal(HttpStatusCode.Forbidden, destapar.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, descartar.StatusCode);
        Assert.Equal(HttpStatusCode.OK, consultar.StatusCode);
    }

    [HechoDeIntegracion]
    public async Task Envase_Barra_SiPuedeDestapar()
    {
        var cola = await CrearColaAsync();
        await DefinirOrigenesAsync(cola.Vaso, (cola.Litro, 4));
        using var barra = await ClienteComoAsync("barra1@licoreria.com");

        var respuesta = await DestaparAsync(cola.Vaso, cola.Litro, barra);

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
    }

    [HechoDeIntegracion]
    public async Task Envase_DosBarrasDestapanLaUltimaBotellaAlMismoTiempo_SoloSeAbreUnEnvase()
    {
        for (var ronda = 0; ronda < 10; ronda++)
        {
            var cola = await CrearColaAsync(litros: 1, dosLitros: 0);
            await DefinirOrigenesAsync(cola.Vaso, (cola.Litro, 4));

            var resultados = await EjecucionConcurrente.EjecutarAsync(2, async _ => (await DestaparAsync(cola.Vaso, cola.Litro)).StatusCode);

            Assert.All(resultados, r => Assert.Null(r.Error));
            Assert.Equal(1, resultados.Count(r => r.Valor == HttpStatusCode.Created));
            // El otro se entera de que ya no queda: agotada (422) o conflicto de versión (409), nunca un error interno.
            Assert.All(
                resultados.Where(r => r.Valor != HttpStatusCode.Created),
                r => Assert.True(r.Valor is HttpStatusCode.UnprocessableEntity or HttpStatusCode.Conflict, $"Estado inesperado: {r.Valor}"));

            var abiertos = await JsonAsync(await _factory.Client.GetAsync($"/api/v1/envases-abiertos?porcionVarianteId={cola.Vaso}"));
            Assert.Equal(1, abiertos.GetArrayLength());
            Assert.Equal(0m, await ExistenciaAsync(cola.ProductoId, cola.Litro));
        }
    }
}
