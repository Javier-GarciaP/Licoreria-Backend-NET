using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Licoreria.IntegrationTests;

/// <summary>Presentaciones y porciones: catálogo, orígenes, envases abiertos y control de porciones.</summary>
public partial class ApiEndpointsTests
{
    private sealed record Cola(Guid ProductoId, Guid Litro, Guid DosLitros, Guid Vaso, string SkuVaso);

    /// <summary>Coca-Cola con botellas de 1 L (3 u.) y 2 L (5 u.) y un vaso sin definir todavía.</summary>
    private async Task<Cola> CrearColaAsync(int litros = 3, int dosLitros = 5)
    {
        var categoriaId = await ObtenerPrimerIdAsync("/api/v1/categorias");
        var unidadId = await ObtenerPrimerIdAsync("/api/v1/unidades-medida");
        var sufijo = Guid.NewGuid().ToString("N")[..8];

        object Variante(string nombre, string sku, bool esBase, decimal precio, int? stock) => new
        {
            id = (Guid?)null,
            nombre,
            sku = $"{sku}-{sufijo}",
            unidadMedidaId = unidadId,
            precioCompraUSD = 0.5,
            precioVentaUSD = precio,
            esBase,
            stockInicial = stock,
            codigosBarras = new[] { $"CB-{sku}-{sufijo}" }
        };

        var respuesta = await _factory.Client.PostAsJsonAsync("/api/v1/productos", new
        {
            nombre = $"Coca-Cola {sufijo}",
            categoriaId,
            tipo = "Simple",
            areaDestino = "Barra",
            variantes = new[]
            {
                Variante("Botella 1 L", "COCA1", true, 2.0m, litros),
                Variante("Botella 2 L", "COCA2", true, 3.5m, dosLitros),
                Variante("Vaso", "VASO", false, 1.0m, null)
            }
        });
        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var producto = await JsonAsync(respuesta);
        var variantes = producto.GetProperty("variantes");
        Guid Id(string nombre)
        {
            for (var i = 0; i < variantes.GetArrayLength(); i++)
            {
                if (variantes[i].GetProperty("nombre").GetString() == nombre) return variantes[i].GetProperty("id").GetGuid();
            }

            throw new InvalidOperationException($"No hay variante {nombre}");
        }

        return new Cola(producto.GetProperty("id").GetGuid(), Id("Botella 1 L"), Id("Botella 2 L"), Id("Vaso"), $"VASO-{sufijo}");
    }

    private Task<HttpResponseMessage> DefinirOrigenesAsync(Guid vaso, params (Guid Presentacion, int Porciones)[] origenes)
        => _factory.Client.PutAsJsonAsync(
            $"/api/v1/variantes/{vaso}/origenes",
            origenes.Select(o => new { presentacionVarianteId = o.Presentacion, porcionesPorUnidad = o.Porciones }).ToArray());

    private async Task<JsonElement> VarianteDeAsync(Guid productoId, Guid varianteId)
    {
        var producto = await JsonAsync(await _factory.Client.GetAsync($"/api/v1/productos/{productoId}"));
        var variantes = producto.GetProperty("variantes");
        for (var i = 0; i < variantes.GetArrayLength(); i++)
        {
            if (variantes[i].GetProperty("id").GetGuid() == varianteId) return variantes[i];
        }

        throw new InvalidOperationException("Variante no encontrada");
    }

    [Fact]
    public async Task Origenes_VasoConDosBotellas_SeGuardanYElProductoInformaLaDisponibilidad()
    {
        var cola = await CrearColaAsync();

        var guardar = await DefinirOrigenesAsync(cola.Vaso, (cola.Litro, 4), (cola.DosLitros, 8));
        Assert.Equal(HttpStatusCode.OK, guardar.StatusCode);
        var origenes = await JsonAsync(guardar);
        Assert.Equal(2, origenes.GetArrayLength());

        var listado = await JsonAsync(await _factory.Client.GetAsync($"/api/v1/variantes/{cola.Vaso}/origenes"));
        Assert.Equal(2, listado.GetArrayLength());

        var vaso = await VarianteDeAsync(cola.ProductoId, cola.Vaso);
        Assert.Equal("Porcion", vaso.GetProperty("naturaleza").GetString());
        Assert.Equal(0, vaso.GetProperty("porcionesDisponibles").GetInt32());
        Assert.Equal(8, vaso.GetProperty("envasesCerradosAbribles").GetInt32());          // 3 + 5 botellas
        Assert.Equal(3 * 4 + 5 * 8, vaso.GetProperty("porcionesPotenciales").GetInt32());

        var litro = await VarianteDeAsync(cola.ProductoId, cola.Litro);
        Assert.Equal("Presentacion", litro.GetProperty("naturaleza").GetString());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public async Task Origenes_PorcionesInvalidas_Devuelve422(int porciones)
    {
        var cola = await CrearColaAsync();

        var respuesta = await DefinirOrigenesAsync(cola.Vaso, (cola.Litro, porciones));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
    }

    [Fact]
    public async Task Origenes_UnaPresentacionNoPuedeSerPorcion_Devuelve422()
    {
        var cola = await CrearColaAsync();

        var respuesta = await DefinirOrigenesAsync(cola.Litro, (cola.DosLitros, 2));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
    }

    [Fact]
    public async Task Origenes_DeOtroProducto_Devuelve422()
    {
        var cola = await CrearColaAsync();
        var otra = await CrearColaAsync();

        var respuesta = await DefinirOrigenesAsync(cola.Vaso, (otra.Litro, 4));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
    }

    [Fact]
    public async Task Origenes_SinPermisoDeCatalogo_Devuelve401SinToken()
    {
        var cola = await CrearColaAsync();
        using var anonimo = _factory.CreateClient();

        var respuesta = await anonimo.PutAsJsonAsync($"/api/v1/variantes/{cola.Vaso}/origenes", Array.Empty<object>());

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Receta_EnUnaPorcion_Devuelve422()
    {
        var cola = await CrearColaAsync();
        await DefinirOrigenesAsync(cola.Vaso, (cola.Litro, 4));

        var respuesta = await _factory.Client.PostAsJsonAsync(
            $"/api/v1/variantes/{cola.Vaso}/recetas", new { varianteInsumoId = cola.DosLitros, cantidad = 0.25 });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
    }

    [Fact]
    public async Task Producto_ConOrigenDePorcion_NoSePuedeEliminar_Devuelve409()
    {
        var cola = await CrearColaAsync();
        await DefinirOrigenesAsync(cola.Vaso, (cola.Litro, 4));

        var respuesta = await _factory.Client.DeleteAsync($"/api/v1/productos/{cola.ProductoId}");

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    /// <summary>Edita el producto cambiando solo la marca de presentación de una variante (o quitando una).</summary>
    private async Task<HttpResponseMessage> EditarVariantesAsync(Cola cola, Func<JsonElement, bool> esBase, Guid? quitar = null)
    {
        var producto = await JsonAsync(await _factory.Client.GetAsync($"/api/v1/productos/{cola.ProductoId}"));
        var variantes = new List<object>();
        foreach (var v in producto.GetProperty("variantes").EnumerateArray())
        {
            if (quitar is Guid id && v.GetProperty("id").GetGuid() == id) continue;
            variantes.Add(new
            {
                id = v.GetProperty("id").GetGuid(),
                nombre = v.GetProperty("nombre").GetString(),
                sku = v.GetProperty("sku").GetString(),
                unidadMedidaId = v.GetProperty("unidadMedidaId").GetGuid(),
                precioCompraUSD = v.GetProperty("precioCompraUSD").GetDecimal(),
                precioVentaUSD = v.GetProperty("precioVentaUSD").GetDecimal(),
                codigosBarras = v.GetProperty("codigosBarras").EnumerateArray().Select(c => c.GetString()).ToArray(),
                esBase = esBase(v)
            });
        }

        return await _factory.Client.PutAsJsonAsync($"/api/v1/productos/{cola.ProductoId}", new
        {
            id = cola.ProductoId,
            nombre = producto.GetProperty("nombre").GetString(),
            descripcion = (string?)null,
            categoriaId = producto.GetProperty("categoriaId").GetGuid(),
            marcaId = (Guid?)null,
            impuestoId = (Guid?)null,
            tipo = "Simple",
            areaDestino = "Barra",
            gradoAlcoholico = (decimal?)null,
            imagenUrl = (string?)null,
            activo = true,
            variantes
        });
    }

    [Fact]
    public async Task Presentacion_ConUnaPorcionQueSaleDeEllaNoPuedeDejarDeSerlo_Devuelve409()
    {
        var cola = await CrearColaAsync();
        await DefinirOrigenesAsync(cola.Vaso, (cola.Litro, 4));

        var respuesta = await EditarVariantesAsync(cola, v => v.GetProperty("esBase").GetBoolean() && v.GetProperty("id").GetGuid() != cola.Litro);

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    [Fact]
    public async Task Presentacion_ConUnaPorcionQueSaleDeEllaNoSePuedeQuitarDelProducto_Devuelve409()
    {
        var cola = await CrearColaAsync();
        await DefinirOrigenesAsync(cola.Vaso, (cola.Litro, 4));

        var respuesta = await EditarVariantesAsync(cola, v => v.GetProperty("esBase").GetBoolean(), quitar: cola.Litro);

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    [Fact]
    public async Task Presentacion_SinVinculos_SiSePuedeEditar()
    {
        var cola = await CrearColaAsync();

        var respuesta = await EditarVariantesAsync(cola, v => v.GetProperty("esBase").GetBoolean());

        Assert.True(respuesta.StatusCode == HttpStatusCode.OK, await respuesta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Porcion_NoPuedeConvertirseEnPresentacionSinQuitarAntesSusOrigenes_Devuelve422()
    {
        var cola = await CrearColaAsync();
        await DefinirOrigenesAsync(cola.Vaso, (cola.Litro, 4));

        var respuesta = await EditarVariantesAsync(cola, v => v.GetProperty("esBase").GetBoolean() || v.GetProperty("id").GetGuid() == cola.Vaso);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);

        await DefinirOrigenesAsync(cola.Vaso);   // se quitan los orígenes
        var ahora = await EditarVariantesAsync(cola, v => v.GetProperty("esBase").GetBoolean() || v.GetProperty("id").GetGuid() == cola.Vaso);
        Assert.Equal(HttpStatusCode.OK, ahora.StatusCode);
    }
}
