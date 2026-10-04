using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Licoreria.IntegrationTests;

[Collection("api")]
public class ApiEndpointsTests
{
    private readonly LicoreriaApiFactory _factory;

    public ApiEndpointsTests(LicoreriaApiFactory factory) => _factory = factory;

    private static async Task<JsonElement> JsonAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json).RootElement.Clone();
    }

    [Fact]
    public async Task Login_ConCredencialesValidas_DevuelveToken()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "admin@licoreria.com",
            password = "admin123"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await JsonAsync(response);
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("accessToken").GetString()));
    }

    [Fact]
    public async Task EndpointProtegido_SinToken_Devuelve401()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/productos");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Producto_ConVariante_SeCreaYObtiene()
    {
        var categoriaId = await ObtenerPrimerIdAsync("/api/v1/categorias");
        var unidadId = await ObtenerPrimerIdAsync("/api/v1/unidades-medida");
        var sku = $"IT-{Guid.NewGuid().ToString("N")[..10]}";

        var response = await _factory.Client.PostAsJsonAsync("/api/v1/productos", new
        {
            nombre = "Producto Integración",
            categoriaId,
            tipo = "Simple",
            variantes = new[]
            {
                new
                {
                    id = (Guid?)null,
                    nombre = "Presentación",
                    sku,
                    unidadMedidaId = unidadId,
                    precioCompraUSD = 2.0,
                    precioVentaUSD = 5.0,
                    codigosBarras = new[] { $"CB-{Guid.NewGuid().ToString("N")[..10]}" }
                }
            }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var producto = await JsonAsync(response);
        var productoId = producto.GetProperty("id").GetGuid();

        var obtenido = await _factory.Client.GetAsync($"/api/v1/productos/{productoId}");
        Assert.Equal(HttpStatusCode.OK, obtenido.StatusCode);
        var body = await JsonAsync(obtenido);
        Assert.Equal(sku, body.GetProperty("variantes")[0].GetProperty("sku").GetString());
    }

    [Fact]
    public async Task Venta_ConPagoSuficiente_GeneraComprobanteYDescuentaStock()
    {
        var (varianteId, sku) = await CrearProductoConStockAsync(5.0m, 10);

        var metodoPagoId = await ObtenerPrimerIdAsync("/api/v1/metodos-pago");

        var response = await _factory.Client.PostAsJsonAsync("/api/v1/ventas", new
        {
            items = new[] { new { varianteId, cantidad = 1 } },
            pagos = new[] { new { metodoPagoId, monto = 5.0, moneda = "USD" } }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var venta = await JsonAsync(response);
        Assert.Equal(5.0m, venta.GetProperty("totalUSD").GetDecimal());
        Assert.False(string.IsNullOrWhiteSpace(venta.GetProperty("numeroComprobante").GetString()));

        var stock = await _factory.Client.GetAsync($"/api/v1/stock?busqueda={sku}");
        var stockBody = await JsonAsync(stock);
        Assert.Equal(9m, stockBody.GetProperty("items")[0].GetProperty("cantidad").GetDecimal());
    }

    [Fact]
    public async Task Venta_ConPagoInsuficiente_Devuelve422()
    {
        var (varianteId, _) = await CrearProductoConStockAsync(5.0m, 5);
        var metodoPagoId = await ObtenerPrimerIdAsync("/api/v1/metodos-pago");

        var response = await _factory.Client.PostAsJsonAsync("/api/v1/ventas", new
        {
            items = new[] { new { varianteId, cantidad = 1 } },
            pagos = new[] { new { metodoPagoId, monto = 1.0, moneda = "USD" } }
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Dashboard_ComoAdmin_DevuelveIndicadores()
    {
        var response = await _factory.Client.GetAsync("/api/v1/reportes/dashboard");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await JsonAsync(response);
        Assert.True(body.TryGetProperty("ventasMesUSD", out _));
    }

    private async Task<Guid> ObtenerPrimerIdAsync(string url)
    {
        var response = await _factory.Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var body = await JsonAsync(response);
        return body[0].GetProperty("id").GetGuid();
    }

    private async Task<(Guid VarianteId, string Sku)> CrearProductoConStockAsync(decimal precioVenta, int stock)
    {
        var categoriaId = await ObtenerPrimerIdAsync("/api/v1/categorias");
        var unidadId = await ObtenerPrimerIdAsync("/api/v1/unidades-medida");
        var sku = $"IT-{Guid.NewGuid().ToString("N")[..10]}";

        var productoResponse = await _factory.Client.PostAsJsonAsync("/api/v1/productos", new
        {
            nombre = "Producto Venta",
            categoriaId,
            tipo = "Simple",
            variantes = new[]
            {
                new
                {
                    id = (Guid?)null,
                    nombre = "Presentación",
                    sku,
                    unidadMedidaId = unidadId,
                    precioCompraUSD = 1.0,
                    precioVentaUSD = precioVenta,
                    codigosBarras = new[] { $"CB-{Guid.NewGuid().ToString("N")[..10]}" }
                }
            }
        });

        productoResponse.EnsureSuccessStatusCode();
        var producto = await JsonAsync(productoResponse);
        var varianteId = producto.GetProperty("variantes")[0].GetProperty("id").GetGuid();

        var ajuste = await _factory.Client.PostAsJsonAsync("/api/v1/ajustes-inventario", new
        {
            varianteId,
            cantidad = stock,
            motivo = "Carga de prueba"
        });
        ajuste.EnsureSuccessStatusCode();

        return (varianteId, sku);
    }
}
