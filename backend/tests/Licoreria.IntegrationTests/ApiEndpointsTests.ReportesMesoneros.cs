using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Licoreria.IntegrationTests;

/// <summary>Desempeño por mesonero: atribución por quien toma el pedido, cancelaciones y tiempo de servicio.</summary>
public partial class ApiEndpointsTests
{
    /// <summary>Id del usuario leído del token ya emitido: evita nuevos inicios de sesión (límite por minuto).</summary>
    private async Task<Guid> UsuarioIdDeAsync(string email)
    {
        string token;
        if (email == "admin@licoreria.com")
        {
            token = _factory.AccessToken;
        }
        else
        {
            using var cliente = await ClienteDeAsync(email);
            token = cliente.DefaultRequestHeaders.Authorization!.Parameter!;
        }

        var carga = token.Split('.')[1].Replace('-', '+').Replace('_', '/');
        carga = carga.PadRight(carga.Length + (4 - carga.Length % 4) % 4, '=');
        using var documento = JsonDocument.Parse(Convert.FromBase64String(carga));
        return Guid.Parse(documento.RootElement.GetProperty("sub").GetString()!);
    }

    private static async Task<(Guid ComandaId, Guid[] Detalles)> PedirComoAsync(
        HttpClient cliente, Guid cuentaId, Guid varianteId, int items)
    {
        var comanda = await cliente.PostAsJsonAsync($"/api/v1/cuentas/{cuentaId}/comandas", new
        {
            area = "Barra",
            items = Enumerable.Range(0, items).Select(_ => new { varianteId, cantidad = 1 }).ToArray()
        });
        Assert.Equal(HttpStatusCode.OK, comanda.StatusCode);
        var cuerpo = await JsonAsync(comanda);
        var ultima = cuerpo.GetProperty("comandas").EnumerateArray().Last();
        return (
            ultima.GetProperty("id").GetGuid(),
            ultima.GetProperty("detalles").EnumerateArray().Select(d => d.GetProperty("id").GetGuid()).ToArray());
    }

    private static async Task CambiarEstadoItemAsync(HttpClient cliente, Guid cuentaId, Guid comandaId, Guid detalleId, string estado)
    {
        var respuesta = await cliente.PutAsJsonAsync(
            $"/api/v1/cuentas/{cuentaId}/comandas/{comandaId}/detalles/{detalleId}/estado", new { estado });
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    private async Task<JsonElement?> MesoneroEnReporteAsync(Guid usuarioId)
    {
        var respuesta = await _factory.Client.GetAsync($"/api/v1/reportes/mesoneros?{PeriodoDeHoy()}");
        Assert.True(respuesta.IsSuccessStatusCode, await respuesta.Content.ReadAsStringAsync());
        foreach (var fila in (await JsonAsync(respuesta)).GetProperty("mesoneros").EnumerateArray())
        {
            if (fila.GetProperty("usuarioId").ValueKind != JsonValueKind.Null && fila.GetProperty("usuarioId").GetGuid() == usuarioId)
            {
                return fila;
            }
        }

        return null;
    }

    [HechoDeIntegracion]
    public async Task ReporteMesoneros_AtribuyeAlQueTomaElPedidoAunqueOtroCobre()
    {
        await AbrirTurnoAsync();
        var mesero = await UsuarioIdDeAsync("mesero1@licoreria.com");
        var admin = await UsuarioIdDeAsync("admin@licoreria.com");
        var antesMesero = await MesoneroEnReporteAsync(mesero);
        var antesAdmin = await MesoneroEnReporteAsync(admin);
        using var clienteMesero = await ClienteDeAsync("mesero1@licoreria.com");
        using var cajero = await ClienteDeAsync("cajero1@licoreria.com");
        var (varianteId, _) = await CrearProductoConStockAsync(10m, 20);

        var abrir = await _factory.Client.PostAsJsonAsync("/api/v1/cuentas", new { nombreMesa = "Mesa Mesoneros" });
        var cuentaId = (await JsonAsync(abrir)).GetProperty("id").GetGuid();

        // El mesonero toma 3 ítems (uno se cancela, uno se sirve) y el administrador toma 1 en la misma cuenta.
        var (comandaMesero, detallesMesero) = await PedirComoAsync(clienteMesero, cuentaId, varianteId, 3);
        await PedirComoAsync(_factory.Client, cuentaId, varianteId, 1);
        await CambiarEstadoItemAsync(clienteMesero, cuentaId, comandaMesero, detallesMesero[0], "Cancelado");
        await CambiarEstadoItemAsync(clienteMesero, cuentaId, comandaMesero, detallesMesero[1], "Preparado");

        // Un cajero cobra la cuenta: no cambia la atribución.
        var cobro = await cajero.PostAsJsonAsync($"/api/v1/cuentas/{cuentaId}/cerrar", new
        {
            pagos = new[] { new { metodoPagoId = await ObtenerPrimerIdAsync("/api/v1/metodos-pago"), monto = 30.0, moneda = "USD" } }
        });
        Assert.Equal(HttpStatusCode.OK, cobro.StatusCode);

        var despuesMesero = await MesoneroEnReporteAsync(mesero);
        var despuesAdmin = await MesoneroEnReporteAsync(admin);

        Assert.NotNull(despuesMesero);
        Assert.NotNull(despuesAdmin);
        decimal Delta(JsonElement? despues, JsonElement? antes, string campo)
            => despues!.Value.GetProperty(campo).GetDecimal() - (antes?.GetProperty(campo).GetDecimal() ?? 0m);

        Assert.Equal(1, Delta(despuesMesero, antesMesero, "comandas"));
        Assert.Equal(20m, Delta(despuesMesero, antesMesero, "ventasGeneradasUSD"));
        Assert.Equal(1, Delta(despuesMesero, antesMesero, "itemsCancelados"));
        Assert.Equal(1, Delta(despuesMesero, antesMesero, "itemsServidos"));
        Assert.Equal(1, Delta(despuesAdmin, antesAdmin, "comandas"));
        Assert.Equal(10m, Delta(despuesAdmin, antesAdmin, "ventasGeneradasUSD"));
        Assert.NotEqual(JsonValueKind.Null, despuesMesero!.Value.GetProperty("tiempoServicioPromedioSegundos").ValueKind);
    }

    [HechoDeIntegracion]
    public async Task ReporteMesoneros_Cajero_Devuelve403YAdministradorDevuelve200()
    {
        using var cajero = await ClienteDeAsync("cajero1@licoreria.com");

        Assert.Equal(HttpStatusCode.Forbidden, (await cajero.GetAsync("/api/v1/reportes/mesoneros")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _factory.Client.GetAsync("/api/v1/reportes/mesoneros")).StatusCode);
    }
}
