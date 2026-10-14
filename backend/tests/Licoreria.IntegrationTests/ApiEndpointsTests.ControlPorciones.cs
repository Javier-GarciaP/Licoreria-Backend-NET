using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Licoreria.IntegrationTests.Support;

namespace Licoreria.IntegrationTests;

/// <summary>Reserva, consumo y liberación de porciones (vasos) de punta a punta, contra PostgreSQL.</summary>
public partial class ApiEndpointsTests
{
    /// <summary>Coca-Cola con el vaso definido (1 L = 4 vasos) y un envase de 1 L ya destapado.</summary>
    private async Task<(Cola Cola, Guid EnvaseId)> ColaConEnvaseAbiertoAsync(int litros = 3)
    {
        var cola = await CrearColaAsync(litros: litros, dosLitros: 0);
        await DefinirOrigenesAsync(cola.Vaso, (cola.Litro, 4));
        var envase = await JsonAsync(await DestaparAsync(cola.Vaso, cola.Litro));
        return (cola, envase.GetProperty("id").GetGuid());
    }

    private async Task<int> VasosDisponiblesAsync(Cola cola)
        => (await VarianteDeAsync(cola.ProductoId, cola.Vaso)).GetProperty("porcionesDisponibles").GetInt32();

    private async Task<int> RestantesDelEnvaseAsync(Cola cola, Guid envaseId)
    {
        var lista = await JsonAsync(await _factory.Client.GetAsync($"/api/v1/envases-abiertos?porcionVarianteId={cola.Vaso}&todos=true"));
        foreach (var e in lista.EnumerateArray())
        {
            if (e.GetProperty("id").GetGuid() == envaseId) return e.GetProperty("porcionesRestantes").GetInt32();
        }

        throw new InvalidOperationException("Envase no encontrado");
    }

    private async Task<HttpResponseMessage> PedirSinAfirmarAsync(Guid cuentaId, Guid varianteId, int cantidad)
        => await _factory.Client.PostAsJsonAsync($"/api/v1/cuentas/{cuentaId}/comandas", new
        {
            area = "Barra",
            items = new[] { new { varianteId, cantidad } }
        });

    private async Task<Guid> AbrirCuentaAsync(string nombre)
    {
        var abrir = await _factory.Client.PostAsJsonAsync("/api/v1/cuentas", new { nombreMesa = nombre });
        abrir.EnsureSuccessStatusCode();
        return (await JsonAsync(abrir)).GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task Porcion_SinEnvaseAbiertoPeroConBotellasCerradas_PideDestaparYNoEsAgotado()
    {
        await AbrirTurnoAsync();
        var cola = await CrearColaAsync(litros: 3, dosLitros: 0);
        await DefinirOrigenesAsync(cola.Vaso, (cola.Litro, 4));
        var cuenta = await AbrirCuentaAsync("Mesa sin abrir");

        var respuesta = await PedirSinAfirmarAsync(cuenta, cola.Vaso, 1);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        var cuerpo = await JsonAsync(respuesta);
        Assert.Equal("porciones-sin-abrir", cuerpo.GetProperty("codigo").GetString());
        Assert.Equal(3, cuerpo.GetProperty("envasesCerrados").GetInt32());
    }

    [Fact]
    public async Task Porcion_SinNadaQueServir_EsExistenciaAgotada()
    {
        await AbrirTurnoAsync();
        var cola = await CrearColaAsync(litros: 0, dosLitros: 0);
        await DefinirOrigenesAsync(cola.Vaso, (cola.Litro, 4));
        var cuenta = await AbrirCuentaAsync("Mesa sin nada");

        var respuesta = await PedirSinAfirmarAsync(cuenta, cola.Vaso, 1);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        Assert.Equal("existencia-agotada", (await JsonAsync(respuesta)).GetProperty("codigo").GetString());
    }

    [Fact]
    public async Task Porcion_AlPedir_ReservaYAlServirConsumeDelEnvaseUnaSolaVez()
    {
        await AbrirTurnoAsync();
        var (cola, envaseId) = await ColaConEnvaseAbiertoAsync();
        var (cuentaId, comandaId, detalleId) = await PedirAsync("Mesa vasos", cola.Vaso, 3);

        Assert.Equal(1, await VasosDisponiblesAsync(cola));            // 4 abiertos - 3 reservados
        Assert.Equal(4, await RestantesDelEnvaseAsync(cola, envaseId)); // todavía no se sirvió

        var servir = await _factory.Client.PutAsJsonAsync(
            $"/api/v1/cuentas/{cuentaId}/comandas/{comandaId}/detalles/{detalleId}/estado", new { estado = "Preparado" });
        Assert.Equal(HttpStatusCode.OK, servir.StatusCode);
        var entregar = await _factory.Client.PutAsJsonAsync(
            $"/api/v1/cuentas/{cuentaId}/comandas/{comandaId}/detalles/{detalleId}/estado", new { estado = "Entregado" });
        Assert.Equal(HttpStatusCode.OK, entregar.StatusCode);

        Assert.Equal(1, await RestantesDelEnvaseAsync(cola, envaseId));
        Assert.Equal(1, await VasosDisponiblesAsync(cola));
    }

    [Fact]
    public async Task Porcion_AlCancelarElPedido_LasPorcionesVuelvenADisponibles()
    {
        await AbrirTurnoAsync();
        var (cola, envaseId) = await ColaConEnvaseAbiertoAsync();
        var (cuentaId, comandaId, detalleId) = await PedirAsync("Mesa cancelar vasos", cola.Vaso, 2);
        Assert.Equal(2, await VasosDisponiblesAsync(cola));

        var cancelar = await _factory.Client.PutAsJsonAsync(
            $"/api/v1/cuentas/{cuentaId}/comandas/{comandaId}/detalles/{detalleId}/estado", new { estado = "Cancelado" });

        Assert.Equal(HttpStatusCode.OK, cancelar.StatusCode);
        Assert.Equal(4, await VasosDisponiblesAsync(cola));
        Assert.Equal(4, await RestantesDelEnvaseAsync(cola, envaseId));
    }

    [Fact]
    public async Task Porcion_AlCerrarLaCuentaSinServir_SeDescuentaUnaVezYSeLiberaLaReserva()
    {
        await AbrirTurnoAsync();
        var (cola, envaseId) = await ColaConEnvaseAbiertoAsync();
        var (cuentaId, _, _) = await PedirAsync("Mesa cerrar vasos", cola.Vaso, 2);

        var cerrar = await _factory.Client.PostAsJsonAsync($"/api/v1/cuentas/{cuentaId}/cerrar", new
        {
            pagos = new[] { new { metodoPagoId = await ObtenerPrimerIdAsync("/api/v1/metodos-pago"), monto = 2.0, moneda = "USD" } }
        });

        Assert.Equal(HttpStatusCode.OK, cerrar.StatusCode);
        Assert.Equal(2, await RestantesDelEnvaseAsync(cola, envaseId));
        Assert.Equal(2, await VasosDisponiblesAsync(cola));
    }

    [Fact]
    public async Task Porcion_AlCerrarElTurno_SeLiberanLasReservasPendientes()
    {
        await AbrirTurnoAsync();
        var (cola, _) = await ColaConEnvaseAbiertoAsync();
        await PedirAsync("Mesa turno vasos", cola.Vaso, 3);
        Assert.Equal(1, await VasosDisponiblesAsync(cola));

        var sesion = await JsonAsync(await _factory.Client.GetAsync("/api/v1/sesiones-caja/activa"));
        var cerrar = await _factory.Client.PostAsJsonAsync(
            $"/api/v1/sesiones-caja/{sesion.GetProperty("id").GetGuid()}/cerrar", new { arqueo = await ArqueoCeroAsync() });
        Assert.Equal(HttpStatusCode.OK, cerrar.StatusCode);

        Assert.Equal(4, await VasosDisponiblesAsync(cola));
    }

    [Fact]
    public async Task Porcion_VentaDirectaEnElPos_ConsumeDeInmediatoYNoToca_LoReservadoPorMesas()
    {
        await AbrirTurnoAsync();
        var (cola, envaseId) = await ColaConEnvaseAbiertoAsync();
        var metodo = await ObtenerPrimerIdAsync("/api/v1/metodos-pago");

        var venta = await _factory.Client.PostAsJsonAsync("/api/v1/ventas", new
        {
            items = new[] { new { varianteId = cola.Vaso, cantidad = 1 } },
            pagos = new[] { new { metodoPagoId = metodo, monto = 1.0, moneda = "USD" } }
        });
        Assert.Equal(HttpStatusCode.Created, venta.StatusCode);
        Assert.Equal(3, await RestantesDelEnvaseAsync(cola, envaseId));

        // Una mesa reserva los 3 que quedan: el mostrador ya no puede venderlos.
        await PedirAsync("Mesa reserva todo", cola.Vaso, 3);
        var otraVenta = await _factory.Client.PostAsJsonAsync("/api/v1/ventas", new
        {
            items = new[] { new { varianteId = cola.Vaso, cantidad = 1 } },
            pagos = new[] { new { metodoPagoId = metodo, monto = 1.0, moneda = "USD" } }
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, otraVenta.StatusCode);
        Assert.Equal(3, await RestantesDelEnvaseAsync(cola, envaseId));
    }

    [Fact]
    public async Task Porcion_UnPreparadoConUnaPorcionComoInsumo_ReservaYConsumeLosVasos()
    {
        await AbrirTurnoAsync();
        var (cola, envaseId) = await ColaConEnvaseAbiertoAsync();
        var categoriaId = await ObtenerPrimerIdAsync("/api/v1/categorias");
        var unidadId = await ObtenerPrimerIdAsync("/api/v1/unidades-medida");
        var sufijo = Guid.NewGuid().ToString("N")[..8];

        var crear = await _factory.Client.PostAsJsonAsync("/api/v1/productos", new
        {
            nombre = $"Cola especial {sufijo}",
            categoriaId,
            tipo = "Preparado",
            areaDestino = "Barra",
            variantes = new[]
            {
                new
                {
                    id = (Guid?)null, nombre = "Copa", sku = $"ESP-{sufijo}", unidadMedidaId = unidadId,
                    precioCompraUSD = 0.5, precioVentaUSD = 2.5, esBase = false, stockInicial = (int?)null,
                    codigosBarras = Array.Empty<string>()
                }
            }
        });
        Assert.Equal(HttpStatusCode.Created, crear.StatusCode);
        var copa = (await JsonAsync(crear)).GetProperty("variantes")[0].GetProperty("id").GetGuid();
        var receta = await _factory.Client.PostAsJsonAsync($"/api/v1/variantes/{copa}/recetas", new { varianteInsumoId = cola.Vaso, cantidad = 2 });
        Assert.Equal(HttpStatusCode.Created, receta.StatusCode);

        var (cuentaId, comandaId, detalleId) = await PedirAsync("Mesa copas", copa, 1);   // 1 copa = 2 vasos

        Assert.Equal(2, await VasosDisponiblesAsync(cola));
        var servir = await _factory.Client.PutAsJsonAsync(
            $"/api/v1/cuentas/{cuentaId}/comandas/{comandaId}/detalles/{detalleId}/estado", new { estado = "Preparado" });
        Assert.Equal(HttpStatusCode.OK, servir.StatusCode);
        Assert.Equal(2, await RestantesDelEnvaseAsync(cola, envaseId));

        // Pedir más copas de las que alcanzan se rechaza con el mensaje de la porción.
        var cuenta2 = await AbrirCuentaAsync("Mesa copas 2");
        var demasiadas = await PedirSinAfirmarAsync(cuenta2, copa, 2);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, demasiadas.StatusCode);
    }

    [Fact]
    public async Task Porcion_DosMeserosPidenElUltimoVasoAlMismoTiempo_SoloUnoLoObtiene()
    {
        await AbrirTurnoAsync();
        var metodo = await ObtenerPrimerIdAsync("/api/v1/metodos-pago");

        for (var ronda = 0; ronda < 10; ronda++)
        {
            var (cola, envaseId) = await ColaConEnvaseAbiertoAsync(litros: 1);

            // Se venden 3 vasos en el mostrador para dejar uno solo en el envase.
            var venta = await _factory.Client.PostAsJsonAsync("/api/v1/ventas", new
            {
                items = new[] { new { varianteId = cola.Vaso, cantidad = 3 } },
                pagos = new[] { new { metodoPagoId = metodo, monto = 3.0, moneda = "USD" } }
            });
            Assert.Equal(HttpStatusCode.Created, venta.StatusCode);
            Assert.Equal(1, await RestantesDelEnvaseAsync(cola, envaseId));

            var cuentas = new[] { await AbrirCuentaAsync($"Vaso {ronda}-A"), await AbrirCuentaAsync($"Vaso {ronda}-B") };
            var resultados = await EjecucionConcurrente.EjecutarAsync(2, async i =>
                (await PedirSinAfirmarAsync(cuentas[i], cola.Vaso, 1)).StatusCode);

            Assert.All(resultados, r => Assert.Null(r.Error));
            Assert.Equal(1, resultados.Count(r => r.Valor == HttpStatusCode.OK));
            Assert.All(
                resultados.Where(r => r.Valor != HttpStatusCode.OK),
                r => Assert.Equal(HttpStatusCode.UnprocessableEntity, r.Valor));
            Assert.Equal(0, await VasosDisponiblesAsync(cola));
        }
    }
}
