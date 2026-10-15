using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Licoreria.IntegrationTests;

/// <summary>
/// Mermas y ajustes, recepciones, pagos y altas de usuario simultáneos contra PostgreSQL real. Cada escenario se
/// repite con datos nuevos y verifica invariantes (existencia, saldos, cantidad de éxitos), no quién gana la carrera:
/// quien pierde recibe un conflicto (409) o un rechazo de regla de negocio (422), nunca un error interno.
/// </summary>
public partial class ApiEndpointsTests
{
    private static readonly HttpStatusCode[] PerdedorValido = [HttpStatusCode.Conflict, HttpStatusCode.UnprocessableEntity];

    private static bool Exito(HttpStatusCode estado) => (int)estado is >= 200 and < 300;

    private static decimal Numero(JsonElement elemento, string nombre)
        => elemento.EnumerateObject().First(p => string.Equals(p.Name, nombre, StringComparison.OrdinalIgnoreCase)).Value.GetDecimal();

    private static void VerificarSinErroresInternos(IEnumerable<ResultadoConcurrente<HttpStatusCode>> resultados)
    {
        Assert.All(resultados, r => Assert.Null(r.Error));
        Assert.All(resultados, r => Assert.True((int)r.Valor < 500, $"La operación {r.Indice} terminó con un error interno ({r.Valor})."));
    }

    private static void VerificarPerdedores(IEnumerable<ResultadoConcurrente<HttpStatusCode>> resultados)
        => Assert.All(
            resultados.Where(r => !Exito(r.Valor)),
            r => Assert.True(PerdedorValido.Contains(r.Valor), $"Estado inesperado para quien pierde la carrera: {r.Valor}"));

    // ===================== Mermas y ajustes sobre la misma variante =====================

    [Trait("Category", "Concurrency")]
    [HechoDeIntegracion]
    public async Task Mermas_DosSobreLaUltimaUnidad_UnaSeConfirmaYLaExistenciaQuedaEnCero()
    {
        await EjecucionConcurrente.RepetirAsync(10, async () =>
        {
            var (varianteId, sku) = await CrearProductoConStockAsync(5.0m, 1);

            var resultados = await EjecucionConcurrente.EjecutarAsync(2, async _ =>
                (await _factory.Client.PostAsJsonAsync("/api/v1/mermas", new { varianteId, cantidad = 1, motivo = "Danado" })).StatusCode);

            VerificarSinErroresInternos(resultados);
            Assert.Equal(1, resultados.Count(r => Exito(r.Valor)));
            VerificarPerdedores(resultados);
            Assert.Equal(0m, (await ObtenerStockAsync(sku)).Cantidad);
        });
    }

    [Trait("Category", "Concurrency")]
    [HechoDeIntegracion]
    public async Task Ajustes_AlMismoTiempoSobreLaMismaVariante_LaExistenciaEsLaInicialMasLosAjustesConfirmados()
    {
        await EjecucionConcurrente.RepetirAsync(10, async () =>
        {
            const int inicial = 5;
            var (varianteId, sku) = await CrearProductoConStockAsync(5.0m, inicial);
            // Entradas de 3 y salidas de 2 mezcladas: algunas salidas pueden quedar sin existencia suficiente.
            int Delta(int indice) => indice % 2 == 0 ? 3 : -2;

            var resultados = await EjecucionConcurrente.EjecutarAsync(6, async indice =>
                (await _factory.Client.PostAsJsonAsync("/api/v1/ajustes-inventario", new { varianteId, cantidad = Delta(indice), motivo = "Carrera" })).StatusCode);

            VerificarSinErroresInternos(resultados);
            VerificarPerdedores(resultados);
            Assert.True(resultados.Any(r => Exito(r.Valor)), "Al menos un ajuste debe confirmarse.");

            var confirmados = resultados.Where(r => Exito(r.Valor)).Sum(r => Delta(r.Indice));
            var (cantidad, _) = await ObtenerStockAsync(sku);
            Assert.Equal(inicial + confirmados, cantidad);
            Assert.True(cantidad >= 0m, $"La existencia no puede ser negativa ({cantidad}).");
        });
    }

    // ===================== Compras: recepción y pagos =====================

    private async Task<Guid> CrearProveedorAsync()
    {
        var respuesta = await _factory.Client.PostAsJsonAsync("/api/v1/proveedores", new { nombre = $"Proveedor {Guid.NewGuid():N}"[..20], diasCredito = 15 });
        respuesta.EnsureSuccessStatusCode();
        return (await JsonAsync(respuesta)).GetProperty("id").GetGuid();
    }

    /// <summary>Crea una orden de compra enviada para una variante nueva (existencia inicial de una unidad).</summary>
    private async Task<(Guid OrdenId, Guid DetalleId, Guid ProveedorId, string Sku)> CrearOrdenEnviadaAsync(int cantidad, decimal costo)
    {
        var (varianteId, sku) = await CrearProductoConStockAsync(5.0m, 1);
        var proveedorId = await CrearProveedorAsync();

        var creada = await _factory.Client.PostAsJsonAsync("/api/v1/ordenes-compra", new
        {
            proveedorId,
            detalles = new[] { new { varianteId, cantidad, costoUnitarioUSD = costo } }
        });
        creada.EnsureSuccessStatusCode();
        var orden = await JsonAsync(creada);
        var ordenId = orden.GetProperty("id").GetGuid();
        var detalleId = orden.GetProperty("detalles")[0].GetProperty("id").GetGuid();

        (await _factory.Client.PostAsync($"/api/v1/ordenes-compra/{ordenId}/aprobar", null)).EnsureSuccessStatusCode();
        (await _factory.Client.PostAsync($"/api/v1/ordenes-compra/{ordenId}/enviar", null)).EnsureSuccessStatusCode();
        return (ordenId, detalleId, proveedorId, sku);
    }

    [Trait("Category", "Concurrency")]
    [HechoDeIntegracion]
    public async Task Recepcion_DeLaMismaLineaAlMismoTiempo_SoloUnaSeConfirmaYLaExistenciaAumentaUnaVez()
    {
        await EjecucionConcurrente.RepetirAsync(10, async () =>
        {
            var (ordenId, detalleId, _, sku) = await CrearOrdenEnviadaAsync(cantidad: 10, costo: 2m);

            var resultados = await EjecucionConcurrente.EjecutarAsync(2, async _ =>
                (await _factory.Client.PostAsJsonAsync("/api/v1/recepciones", new
                {
                    ordenCompraId = ordenId,
                    detalles = new[] { new { ordenCompraDetalleId = detalleId, cantidad = 10 } }
                })).StatusCode);

            VerificarSinErroresInternos(resultados);
            Assert.Equal(1, resultados.Count(r => Exito(r.Valor)));
            VerificarPerdedores(resultados);

            // Una unidad inicial más las 10 recibidas una sola vez.
            Assert.Equal(11m, (await ObtenerStockAsync(sku)).Cantidad);
            var orden = await JsonAsync(await _factory.Client.GetAsync($"/api/v1/ordenes-compra/{ordenId}"));
            Assert.Equal(10m, Numero(orden.GetProperty("detalles")[0], "cantidadRecibida"));
        });
    }

    [Trait("Category", "Concurrency")]
    [HechoDeIntegracion]
    public async Task PagosDeCuentaPorPagar_AlMismoTiempo_NuncaSuperanElSaldo()
    {
        await EjecucionConcurrente.RepetirAsync(10, async () =>
        {
            var (ordenId, detalleId, proveedorId, _) = await CrearOrdenEnviadaAsync(cantidad: 10, costo: 10m);
            var recepcion = await _factory.Client.PostAsJsonAsync("/api/v1/recepciones", new
            {
                ordenCompraId = ordenId,
                detalles = new[] { new { ordenCompraDetalleId = detalleId, cantidad = 10 } }
            });
            recepcion.EnsureSuccessStatusCode();
            var cuentas = await JsonAsync(await _factory.Client.GetAsync($"/api/v1/cuentas-por-pagar?proveedorId={proveedorId}"));
            var cuenta = cuentas.GetProperty("items")[0];
            var cuentaId = cuenta.GetProperty("id").GetGuid();
            Assert.Equal(100m, Numero(cuenta, "saldoUSD"));

            var resultados = await EjecucionConcurrente.EjecutarAsync(2, async _ =>
                (await _factory.Client.PostAsJsonAsync($"/api/v1/cuentas-por-pagar/{cuentaId}/pagos", new { monto = 70, moneda = "USD" })).StatusCode);

            VerificarSinErroresInternos(resultados);
            Assert.Equal(1, resultados.Count(r => Exito(r.Valor)));
            VerificarPerdedores(resultados);

            var final = (await JsonAsync(await _factory.Client.GetAsync($"/api/v1/cuentas-por-pagar?proveedorId={proveedorId}")))
                .GetProperty("items")[0];
            Assert.Equal(30m, Numero(final, "saldoUSD"));
        });
    }

    // ===================== Cuentas por cobrar =====================

    [Trait("Category", "Concurrency")]
    [HechoDeIntegracion]
    public async Task PagosDeCuentaPorCobrar_AlMismoTiempo_NuncaSuperanElSaldo()
    {
        await EjecucionConcurrente.RepetirAsync(10, async () =>
        {
            var cliente = await _factory.Client.PostAsJsonAsync("/api/v1/clientes", new { nombre = $"Cliente {Guid.NewGuid():N}"[..20] });
            cliente.EnsureSuccessStatusCode();
            var clienteId = (await JsonAsync(cliente)).GetProperty("id").GetGuid();
            var credito = await _factory.Client.PostAsJsonAsync("/api/v1/cuentas-por-cobrar", new
            {
                clienteId,
                montoUSD = 100,
                vencimiento = DateTime.UtcNow.AddDays(15)
            });
            credito.EnsureSuccessStatusCode();
            var cuentaId = (await JsonAsync(credito)).GetProperty("id").GetGuid();

            var resultados = await EjecucionConcurrente.EjecutarAsync(2, async _ =>
                (await _factory.Client.PostAsJsonAsync($"/api/v1/cuentas-por-cobrar/{cuentaId}/pagos", new { monto = 70 })).StatusCode);

            VerificarSinErroresInternos(resultados);
            Assert.Equal(1, resultados.Count(r => Exito(r.Valor)));
            VerificarPerdedores(resultados);

            var final = (await JsonAsync(await _factory.Client.GetAsync($"/api/v1/cuentas-por-cobrar?clienteId={clienteId}")))
                .GetProperty("items")[0];
            Assert.Equal(30m, Numero(final, "saldoUSD"));
        });
    }

    // ===================== Usuarios =====================

    [Trait("Category", "Concurrency")]
    [HechoDeIntegracion]
    public async Task Usuarios_CreadosConElMismoCorreoAlMismoTiempo_UnoSeCreaYElOtroRecibeConflicto()
    {
        await EjecucionConcurrente.RepetirAsync(10, async () =>
        {
            var correo = $"carrera-{Guid.NewGuid():N}@test.com";

            var resultados = await EjecucionConcurrente.EjecutarAsync(2, async i =>
                (await _factory.Client.PostAsJsonAsync("/api/v1/usuarios", new
                {
                    nombreCompleto = $"Usuario {i}",
                    email = correo,
                    password = "Clave-123456",
                    rol = "Cajero",
                    activo = true
                })).StatusCode);

            VerificarSinErroresInternos(resultados);
            Assert.Equal(1, resultados.Count(r => r.Valor == HttpStatusCode.Created));
            Assert.Equal(1, resultados.Count(r => r.Valor == HttpStatusCode.Conflict));

            var usuarios = await JsonAsync(await _factory.Client.GetAsync($"/api/v1/usuarios?busqueda={Uri.EscapeDataString(correo)}"));
            Assert.Equal(1, usuarios.GetProperty("totalItems").GetInt32());
        });
    }
}
