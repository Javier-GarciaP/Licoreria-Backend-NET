using System.Net;
using System.Net.Http.Json;
using Licoreria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Licoreria.IntegrationTests.Support;

namespace Licoreria.IntegrationTests;

/// <summary>Operaciones simultáneas sobre cuentas, mesas y comandas contra PostgreSQL real.</summary>
public partial class ApiEndpointsTests
{
    private async Task<Guid> CrearMesaAsync()
    {
        var zonaId = await CrearZonaAsync();
        var respuesta = await _factory.Client.PostAsJsonAsync("/api/v1/mesas", new
        {
            zonaId,
            numero = $"C-{Guid.NewGuid().ToString("N")[..4]}",
            capacidad = 4
        });
        respuesta.EnsureSuccessStatusCode();
        return (await JsonAsync(respuesta)).GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task Mesa_AbiertaPorVariosMesonerosAlMismoTiempo_SoloUnaCuentaSeAbre()
    {
        await AbrirTurnoAsync();
        var mesaId = await CrearMesaAsync();

        var resultados = await EjecucionConcurrente.EjecutarAsync(4, async i =>
            (await _factory.Client.PostAsJsonAsync("/api/v1/cuentas", new { nombreMesa = $"Mesa disputada {i}", mesaId })).StatusCode);

        Assert.All(resultados, r => Assert.Null(r.Error));
        Assert.Equal(1, resultados.Count(r => r.Valor == HttpStatusCode.Created));
        // Quien pierde la carrera recibe un conflicto claro, nunca un error interno.
        Assert.All(resultados.Where(r => r.Valor != HttpStatusCode.Created), r => Assert.Equal(HttpStatusCode.Conflict, r.Valor));
    }

    [Fact]
    public async Task Cuenta_CobradaPorVariosCajerosAlMismoTiempo_RegistraUnaSolaVenta()
    {
        await AbrirTurnoAsync();
        var (varianteId, sku) = await CrearProductoConStockAsync(5.0m, 10);
        var (cuentaId, _, _) = await PedirAsync("Mesa cobro simultaneo", varianteId, 2);
        var metodoPagoId = await ObtenerPrimerIdAsync("/api/v1/metodos-pago");

        var resultados = await EjecucionConcurrente.EjecutarAsync(3, async _ =>
            (await _factory.Client.PostAsJsonAsync($"/api/v1/cuentas/{cuentaId}/cerrar", new
            {
                pagos = new[] { new { metodoPagoId, monto = 10.0, moneda = "USD" } }
            })).StatusCode);

        Assert.All(resultados, r => Assert.Null(r.Error));
        Assert.Equal(1, resultados.Count(r => r.Valor == HttpStatusCode.OK));
        // El resto se entera de que la cuenta ya se cobró (404 si llegó después, 409 si coincidió).
        Assert.All(
            resultados.Where(r => r.Valor != HttpStatusCode.OK),
            r => Assert.True(r.Valor is HttpStatusCode.Conflict or HttpStatusCode.NotFound, $"Estado inesperado: {r.Valor}"));

        // Una sola venta: el inventario se descontó una vez y no queda nada reservado.
        var (cantidad, reservada) = await ObtenerStockAsync(sku);
        Assert.Equal(8m, cantidad);
        Assert.Equal(0m, reservada);
    }

    [Fact]
    public async Task Abonos_AlMismoTiempo_NoPierdenNingunAbono()
    {
        await AbrirTurnoAsync();
        var (varianteId, _) = await CrearProductoConStockAsync(5.0m, 10);
        var (cuentaId, _, _) = await PedirAsync("Mesa abonos simultaneos", varianteId, 2);
        var metodoPagoId = await ObtenerPrimerIdAsync("/api/v1/metodos-pago");

        var resultados = await EjecucionConcurrente.EjecutarAsync(4, async _ =>
            (await _factory.Client.PostAsJsonAsync($"/api/v1/cuentas/{cuentaId}/abonos", new
            {
                metodoPagoId,
                monto = 2.0,
                moneda = "USD"
            })).StatusCode);

        Assert.All(resultados, r => Assert.Null(r.Error));
        Assert.All(
            resultados,
            r => Assert.True(r.Valor is HttpStatusCode.OK or HttpStatusCode.Conflict, $"Estado inesperado: {r.Valor}"));

        var confirmados = resultados.Count(r => r.Valor == HttpStatusCode.OK);
        var cuenta = await JsonAsync(await _factory.Client.GetAsync($"/api/v1/cuentas/{cuentaId}"));
        // El total abonado es exactamente la suma de los abonos confirmados: ninguno se pierde.
        Assert.Equal(confirmados * 2m, cuenta.GetProperty("totalAbonado").GetDecimal());
        Assert.Equal(confirmados, cuenta.GetProperty("abonos").GetArrayLength());
    }

    [Fact]
    public async Task Item_ServidoPorBarraYMesoneroAlMismoTiempo_DescuentaUnaSolaVez()
    {
        await AbrirTurnoAsync();
        var (varianteId, sku) = await CrearProductoConStockAsync(5.0m, 10);
        var (cuentaId, comandaId, detalleId) = await PedirAsync("Mesa estado simultaneo", varianteId, 2);

        var resultados = await EjecucionConcurrente.EjecutarAsync(4, async _ =>
            (await _factory.Client.PutAsJsonAsync(
                $"/api/v1/cuentas/{cuentaId}/comandas/{comandaId}/detalles/{detalleId}/estado",
                new { estado = "Preparado" })).StatusCode);

        Assert.All(resultados, r => Assert.Null(r.Error));
        Assert.All(
            resultados,
            r => Assert.True(r.Valor is HttpStatusCode.OK or HttpStatusCode.Conflict, $"Estado inesperado: {r.Valor}"));
        Assert.True(resultados.Any(r => r.Valor == HttpStatusCode.OK));

        var (cantidad, reservada) = await ObtenerStockAsync(sku);
        Assert.Equal(8m, cantidad);   // 2 unidades, descontadas una sola vez
        Assert.Equal(0m, reservada);
    }

    private static object ReservaDe(DateTime fecha, params Guid[] mesas) => new
    {
        fechaHora = fecha,
        personas = 2,
        mesas,
        nombreContacto = "Cliente Carrera",
        telefono = "0414-0000000",
        origen = "Web"
    };

    [Fact]
    public async Task Reservas_DeLaMismaMesaYHoraAlMismoTiempo_SoloUnaSeCrea()
    {
        var mesaId = await CrearMesaAsync();
        var fecha = DateTime.UtcNow.AddDays(10).Date.AddHours(21);

        var resultados = await EjecucionConcurrente.EjecutarAsync(5, async _ =>
            (await _factory.Client.PostAsJsonAsync("/api/v1/reservas", ReservaDe(fecha, mesaId))).StatusCode);

        Assert.All(resultados, r => Assert.Null(r.Error));
        Assert.Equal(1, resultados.Count(r => r.Valor == HttpStatusCode.Created));
        Assert.All(resultados.Where(r => r.Valor != HttpStatusCode.Created), r => Assert.Equal(HttpStatusCode.Conflict, r.Valor));
    }

    [Fact]
    public async Task Reservas_MultimesaEnOrdenCruzado_NoSeBloqueanEntreSi()
    {
        var mesaA = await CrearMesaAsync();
        var mesaB = await CrearMesaAsync();
        var fecha = DateTime.UtcNow.AddDays(11).Date.AddHours(21);

        // Una pide (A, B) y la otra (B, A): sin un orden estable de bloqueo se esperarían mutuamente.
        var resultados = await EjecucionConcurrente.EjecutarAsync(6, async i =>
            (await _factory.Client.PostAsJsonAsync(
                "/api/v1/reservas",
                i % 2 == 0 ? ReservaDe(fecha, mesaA, mesaB) : ReservaDe(fecha, mesaB, mesaA))).StatusCode,
            TimeSpan.FromSeconds(30));

        Assert.All(resultados, r => Assert.Null(r.Error));
        Assert.Equal(1, resultados.Count(r => r.Valor == HttpStatusCode.Created));
        Assert.All(resultados.Where(r => r.Valor != HttpStatusCode.Created), r => Assert.Equal(HttpStatusCode.Conflict, r.Valor));
    }

    [Fact]
    public async Task Reservas_DeMesasDistintasAlMismoTiempo_SeCreanTodas()
    {
        var mesas = new List<Guid>();
        for (var i = 0; i < 4; i++)
        {
            mesas.Add(await CrearMesaAsync());
        }

        var fecha = DateTime.UtcNow.AddDays(12).Date.AddHours(21);
        var resultados = await EjecucionConcurrente.EjecutarAsync(mesas.Count, async i =>
            (await _factory.Client.PostAsJsonAsync("/api/v1/reservas", ReservaDe(fecha, mesas[i]))).StatusCode);

        Assert.All(resultados, r => Assert.Equal(HttpStatusCode.Created, r.Valor));
    }

    /* ===================== Turno de caja ===================== */

    private async Task CerrarTurnoSiHayAsync()
    {
        var estado = await JsonAsync(await _factory.Client.GetAsync("/api/v1/sesiones-caja/turno"));
        if (!estado.GetProperty("abierto").GetBoolean())
        {
            return;
        }

        var sesion = await JsonAsync(await _factory.Client.GetAsync("/api/v1/sesiones-caja/activa"));
        var cerrar = await _factory.Client.PostAsJsonAsync(
            $"/api/v1/sesiones-caja/{sesion.GetProperty("id").GetGuid()}/cerrar", new { arqueo = await ArqueoCeroAsync() });
        cerrar.EnsureSuccessStatusCode();
    }

    private async Task<Guid> TurnoActivoAsync()
        => (await JsonAsync(await _factory.Client.GetAsync("/api/v1/sesiones-caja/activa"))).GetProperty("id").GetGuid();

    [Fact]
    public async Task Turno_AbiertoPorVariosCajerosAlMismoTiempo_SoloUnoQuedaAbierto()
    {
        await CerrarTurnoSiHayAsync();

        var resultados = await EjecucionConcurrente.EjecutarAsync(4, async _ =>
            (await _factory.Client.PostAsJsonAsync("/api/v1/sesiones-caja", new { fondoInicial = 10 })).StatusCode);

        Assert.All(resultados, r => Assert.Null(r.Error));
        Assert.Equal(1, resultados.Count(r => r.Valor == HttpStatusCode.Created));
        Assert.All(resultados.Where(r => r.Valor != HttpStatusCode.Created), r => Assert.Equal(HttpStatusCode.Conflict, r.Valor));
    }

    [Fact]
    public async Task Turno_CerradoPorVariosUsuariosAlMismoTiempo_SeCierraUnaSolaVez()
    {
        await CerrarTurnoSiHayAsync();
        await AbrirTurnoAsync();
        var sesionId = await TurnoActivoAsync();
        var arqueo = await ArqueoCeroAsync();

        var resultados = await EjecucionConcurrente.EjecutarAsync(3, async _ =>
            (await _factory.Client.PostAsJsonAsync($"/api/v1/sesiones-caja/{sesionId}/cerrar", new { arqueo })).StatusCode);

        Assert.All(resultados, r => Assert.Null(r.Error));
        Assert.Equal(1, resultados.Count(r => r.Valor == HttpStatusCode.OK));
        Assert.All(resultados.Where(r => r.Valor != HttpStatusCode.OK), r => Assert.Equal(HttpStatusCode.Conflict, r.Valor));
    }

    [Fact]
    public async Task Ventas_DuranteElCierreDelTurno_NuncaQuedanFueraDelArqueo()
    {
        await CerrarTurnoSiHayAsync();
        await AbrirTurnoAsync();
        var sesionId = await TurnoActivoAsync();
        var arqueo = await ArqueoCeroAsync();
        var metodoPagoId = await ObtenerPrimerIdAsync("/api/v1/metodos-pago");

        var variantes = new List<Guid>();
        for (var i = 0; i < 6; i++)
        {
            variantes.Add((await CrearProductoConStockAsync(5.0m, 10)).VarianteId);
        }

        // Seis ventas y un cierre salen a la vez: cada venta entra al arqueo o se rechaza por turno cerrado.
        var resultados = await EjecucionConcurrente.EjecutarAsync(variantes.Count + 1, async i =>
        {
            if (i == variantes.Count)
            {
                return (await _factory.Client.PostAsJsonAsync($"/api/v1/sesiones-caja/{sesionId}/cerrar", new { arqueo })).StatusCode;
            }

            return (await _factory.Client.PostAsJsonAsync("/api/v1/ventas", new
            {
                items = new[] { new { varianteId = variantes[i], cantidad = 1 } },
                pagos = new[] { new { metodoPagoId, monto = 5.0, moneda = "USD" } }
            })).StatusCode;
        });

        Assert.All(resultados, r => Assert.Null(r.Error));
        Assert.Equal(HttpStatusCode.OK, resultados[^1].Valor);
        Assert.All(
            resultados.Take(variantes.Count),
            r => Assert.True(r.Valor is HttpStatusCode.Created or HttpStatusCode.UnprocessableEntity, $"Estado inesperado: {r.Valor}"));

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LicoreriaDbContext>();
        var sesion = await context.SesionesCaja.AsNoTracking().SingleAsync(s => s.Id == sesionId);
        var vendidoEnElTurno = await context.Ventas.AsNoTracking()
            .Where(v => v.SesionCajaId == sesionId)
            .SumAsync(v => (decimal?)v.TotalUSD) ?? 0m;

        // Lo registrado por el cierre coincide con las ventas realmente asociadas al turno.
        Assert.Equal(vendidoEnElTurno, sesion.VentasDelTurnoUSD);
        Assert.Equal(resultados.Take(variantes.Count).Count(r => r.Valor == HttpStatusCode.Created) * 5m, vendidoEnElTurno);
    }

    /* ===================== La última Coca-Cola ===================== */

    [Fact]
    public async Task UltimaUnidad_PedidaPorDosMesonerosAlMismoTiempo_SoloUnoLaObtiene()
    {
        await AbrirTurnoAsync();

        // Se repite varias veces con producto y mesas nuevos: la invariante debe cumplirse siempre,
        // gane quien gane la carrera.
        for (var ronda = 0; ronda < 10; ronda++)
        {
            var (varianteId, sku) = await CrearProductoConStockAsync(2.0m, 1);
            var cuentas = new List<Guid>();
            foreach (var mesa in new[] { "A", "B" })
            {
                var abrir = await _factory.Client.PostAsJsonAsync("/api/v1/cuentas", new { nombreMesa = $"Ultima {ronda}-{mesa}" });
                abrir.EnsureSuccessStatusCode();
                cuentas.Add((await JsonAsync(abrir)).GetProperty("id").GetGuid());
            }

            var resultados = await EjecucionConcurrente.EjecutarAsync(2, async i =>
            {
                var respuesta = await _factory.Client.PostAsJsonAsync($"/api/v1/cuentas/{cuentas[i]}/comandas", new
                {
                    area = "Barra",
                    items = new[] { new { varianteId, cantidad = 1 } }
                });
                return (respuesta.StatusCode, Cuerpo: await respuesta.Content.ReadAsStringAsync());
            });

            Assert.All(resultados, r => Assert.Null(r.Error));
            Assert.Equal(1, resultados.Count(r => r.Valor.StatusCode == HttpStatusCode.OK));

            // El perdedor se entera al pedir (422 con el código de agotado), nunca con un error interno.
            var perdedor = Assert.Single(resultados, r => r.Valor.StatusCode != HttpStatusCode.OK).Valor;
            Assert.Equal(HttpStatusCode.UnprocessableEntity, perdedor.StatusCode);
            Assert.Contains("existencia-agotada", perdedor.Cuerpo);

            // La unidad quedó comprometida una sola vez: ni sobreventa ni existencia perdida.
            var (cantidad, reservada) = await ObtenerStockAsync(sku);
            Assert.Equal(1m, cantidad);
            Assert.Equal(1m, reservada);
        }
    }
}
