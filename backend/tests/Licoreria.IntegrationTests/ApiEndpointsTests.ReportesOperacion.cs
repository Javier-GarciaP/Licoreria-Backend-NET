using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Licoreria.IntegrationTests;

/// <summary>KPIs de operación del local: ocupación, asistencia de reservas y descuadre de caja.</summary>
public partial class ApiEndpointsTests
{
    private async Task<Guid> CrearZonaNombradaAsync(string nombre)
    {
        var respuesta = await _factory.Client.PostAsJsonAsync("/api/v1/zonas", new { nombre, tipo = "Mesas" });
        respuesta.EnsureSuccessStatusCode();
        return (await JsonAsync(respuesta)).GetProperty("id").GetGuid();
    }

    private async Task<Guid> CrearMesaEnZonaAsync(Guid zonaId)
    {
        var respuesta = await _factory.Client.PostAsJsonAsync("/api/v1/mesas", new
        {
            zonaId,
            numero = $"K-{Guid.NewGuid().ToString("N")[..4]}",
            capacidad = 4
        });
        respuesta.EnsureSuccessStatusCode();
        return (await JsonAsync(respuesta)).GetProperty("id").GetGuid();
    }

    [HechoDeIntegracion]
    public async Task ReporteOcupacion_ZonaConUnaMesaOcupada_CuentaOcupacionYSesionAbierta()
    {
        await AbrirTurnoAsync();
        var zonaId = await CrearZonaNombradaAsync($"Zona KPI {Guid.NewGuid():N}"[..20]);
        var ocupada = await CrearMesaEnZonaAsync(zonaId);
        await CrearMesaEnZonaAsync(zonaId);
        var abrir = await _factory.Client.PostAsJsonAsync("/api/v1/cuentas", new { nombreMesa = "Mesa KPI", mesaId = ocupada });
        Assert.Equal(HttpStatusCode.Created, abrir.StatusCode);

        // Una cuenta abierta sin mesa del plano (solo por nombre) no pertenece a ninguna zona.
        var sinMesa = await _factory.Client.PostAsJsonAsync("/api/v1/cuentas", new { nombreMesa = "Barra sin mesa" });
        Assert.Equal(HttpStatusCode.Created, sinMesa.StatusCode);

        var respuesta = await _factory.Client.GetAsync($"/api/v1/reportes/ocupacion?{PeriodoDeHoy()}");

        Assert.True(respuesta.IsSuccessStatusCode, await respuesta.Content.ReadAsStringAsync());
        var reporte = await JsonAsync(respuesta);
        Assert.Contains(
            reporte.GetProperty("porZona").EnumerateArray(),
            z => z.GetProperty("zona").GetString() == "Sin zona" && z.GetProperty("zonaId").ValueKind == JsonValueKind.Null);
        var zona = reporte.GetProperty("porZona").EnumerateArray()
            .Single(z => z.GetProperty("zonaId").ValueKind != JsonValueKind.Null && z.GetProperty("zonaId").GetGuid() == zonaId);
        Assert.Equal(2, zona.GetProperty("mesasActivas").GetInt32());
        Assert.Equal(1, zona.GetProperty("mesasOcupadas").GetInt32());
        Assert.Equal(50m, zona.GetProperty("porcentajeOcupacion").GetDecimal());
        Assert.Equal(1, zona.GetProperty("sesiones").GetInt32());
        Assert.Equal(1, zona.GetProperty("sesionesAbiertas").GetInt32());
        Assert.Equal(0.5m, zona.GetProperty("sesionesPorMesa").GetDecimal());
        Assert.True(zona.GetProperty("permanenciaPromedioMinutos").GetDecimal() >= 0m);
        Assert.True(reporte.GetProperty("mesasActivas").GetInt32() >= 2);
    }

    [HechoDeIntegracion]
    public async Task ReporteReservasAsistencia_CuentaPorEstadoYExcluyeCanceladasDeLaTasa()
    {
        var dia = DateTime.UtcNow.AddDays(40 + Random.Shared.Next(1, 200)).Date.AddHours(20);
        var asistio = await CrearReservaEnAsync(dia);
        var noAsistio = await CrearReservaEnAsync(dia.AddHours(3));
        var cancelada = await CrearReservaEnAsync(dia.AddHours(6));
        await CambiarEstadoReservaAsync(asistio, "Asistio");
        await CambiarEstadoReservaAsync(noAsistio, "NoAsistio");
        await CambiarEstadoReservaAsync(cancelada, "Cancelada");

        var respuesta = await _factory.Client.GetAsync(
            $"/api/v1/reportes/reservas-asistencia?desde={Fecha(dia.AddHours(-1))}&hasta={Fecha(dia.AddHours(8))}");

        Assert.True(respuesta.IsSuccessStatusCode, await respuesta.Content.ReadAsStringAsync());
        var reporte = await JsonAsync(respuesta);
        Assert.Equal(3, reporte.GetProperty("total").GetInt32());
        Assert.Equal(1, reporte.GetProperty("asistieron").GetInt32());
        Assert.Equal(1, reporte.GetProperty("noAsistieron").GetInt32());
        Assert.Equal(1, reporte.GetProperty("canceladas").GetInt32());
        Assert.Equal(50m, reporte.GetProperty("tasaNoAsistencia").GetDecimal());
    }

    private async Task<Guid> CrearReservaEnAsync(DateTime fecha)
    {
        var mesaId = await CrearMesaAsync();
        var crear = await _factory.Client.PostAsJsonAsync("/api/v1/reservas", new
        {
            fechaHora = fecha,
            personas = 2,
            mesas = new[] { mesaId },
            nombreContacto = "Cliente KPI",
            telefono = "0414-0000000",
            origen = "Web"
        });
        Assert.Equal(HttpStatusCode.Created, crear.StatusCode);
        return (await JsonAsync(crear)).GetProperty("id").GetGuid();
    }

    private async Task CambiarEstadoReservaAsync(Guid reservaId, string estado)
    {
        var respuesta = await _factory.Client.PutAsJsonAsync($"/api/v1/reservas/{reservaId}/estado", new { estado });
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    [HechoDeIntegracion]
    public async Task ReporteCaja_TurnoCerradoConFaltante_AparecePeroElAbiertoNo()
    {
        await CerrarTurnoSiHayAsync();
        var abrir = await _factory.Client.PostAsJsonAsync("/api/v1/sesiones-caja", new { fondoInicial = 12.5 });
        Assert.Equal(HttpStatusCode.Created, abrir.StatusCode);
        var cerradoId = (await JsonAsync(abrir)).GetProperty("id").GetGuid();
        var cerrar = await _factory.Client.PostAsJsonAsync(
            $"/api/v1/sesiones-caja/{cerradoId}/cerrar", new { arqueo = await ArqueoCeroAsync() });
        Assert.Equal(HttpStatusCode.OK, cerrar.StatusCode);

        var abierto = await _factory.Client.PostAsJsonAsync("/api/v1/sesiones-caja", new { fondoInicial = 3 });
        Assert.Equal(HttpStatusCode.Created, abierto.StatusCode);
        var abiertoId = (await JsonAsync(abierto)).GetProperty("id").GetGuid();

        var respuesta = await _factory.Client.GetAsync($"/api/v1/reportes/caja?{PeriodoDeHoy()}");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var reporte = await JsonAsync(respuesta);
        var items = reporte.GetProperty("items").EnumerateArray().ToList();
        var turno = items.Single(i => i.GetProperty("sesionId").GetGuid() == cerradoId);
        Assert.Equal(12.5m, turno.GetProperty("montoEsperado").GetDecimal());
        Assert.Equal(-12.5m, turno.GetProperty("descuadre").GetDecimal());
        Assert.DoesNotContain(items, i => i.GetProperty("sesionId").GetGuid() == abiertoId);
        Assert.Equal(items.Count, reporte.GetProperty("turnos").GetInt32());
        Assert.Equal(items.Sum(i => i.GetProperty("descuadre").GetDecimal()), reporte.GetProperty("descuadreTotal").GetDecimal());
    }

    [HechoDeIntegracion]
    public async Task ReportesDeOperacion_RespetanLosPermisosDeCadaRol()
    {
        using var mesero = await ClienteDeAsync("mesero1@licoreria.com");
        using var barra = await ClienteDeAsync("barra1@licoreria.com");

        Assert.Equal(HttpStatusCode.OK, (await mesero.GetAsync("/api/v1/reportes/ocupacion")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await barra.GetAsync("/api/v1/reportes/ocupacion")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await barra.GetAsync("/api/v1/reportes/reservas-asistencia")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await mesero.GetAsync("/api/v1/reportes/caja")).StatusCode);
    }
}
