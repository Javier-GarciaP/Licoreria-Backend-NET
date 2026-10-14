using System.Net;
using System.Net.Http.Json;
using Licoreria.Domain.Services;

namespace Licoreria.IntegrationTests;

/// <summary>Marcado de mesas reservadas: por jornada en operación y por horario al elegir fecha.</summary>
public partial class ApiEndpointsTests
{
    private async Task<bool> MesaReservadaAsync(Guid mesaId, DateTime? fechaHora = null)
    {
        var url = fechaHora is null
            ? "/api/v1/mesas"
            : $"/api/v1/mesas?fechaHora={Uri.EscapeDataString(fechaHora.Value.ToString("o"))}";
        var mesas = await JsonAsync(await _factory.Client.GetAsync(url));
        for (var i = 0; i < mesas.GetArrayLength(); i++)
        {
            if (mesas[i].GetProperty("id").GetGuid() == mesaId)
            {
                return mesas[i].GetProperty("reservada").GetBoolean();
            }
        }

        throw new InvalidOperationException("La mesa no aparece en el listado.");
    }

    [Fact]
    public async Task Mesa_ConReservaDeEstaJornada_AparecePorReservadaEnLaOperacion()
    {
        var mesaId = await CrearMesaAsync();
        var (desde, hasta) = new JornadaOperativa().VentanaQueContiene(DateTime.UtcNow);

        // Una reserva tardía de la jornada en curso (al menos 30 min en el futuro y antes de que termine).
        var fecha = new[] { desde.AddHours(10), DateTime.UtcNow.AddMinutes(30) }.Max();
        if (fecha >= hasta.AddMinutes(-1))
        {
            return; // en los últimos minutos de la jornada no hay horario futuro dentro de ella
        }

        var crear = await _factory.Client.PostAsJsonAsync("/api/v1/reservas", new
        {
            fechaHora = fecha,
            personas = 2,
            mesas = new[] { mesaId },
            nombreContacto = "Cliente Jornada",
            telefono = "0414-0000000",
            origen = "Web"
        });
        Assert.Equal(HttpStatusCode.Created, crear.StatusCode);

        // Se ve reservada en la operación aunque falten muchas horas para la reserva.
        Assert.True(await MesaReservadaAsync(mesaId));
    }

    [Fact]
    public async Task Mesa_ConReservaLejana_NoAparecePorReservadaEnLaOperacionDeHoy()
    {
        var mesaId = await CrearMesaAsync();
        var fecha = DateTime.UtcNow.AddDays(5);

        var crear = await _factory.Client.PostAsJsonAsync("/api/v1/reservas", new
        {
            fechaHora = fecha,
            personas = 2,
            mesas = new[] { mesaId },
            nombreContacto = "Cliente Lejano",
            telefono = "0414-0000000",
            origen = "Web"
        });
        Assert.Equal(HttpStatusCode.Created, crear.StatusCode);

        Assert.False(await MesaReservadaAsync(mesaId));
        // Al elegir ese día y hora en el formulario sí figura reservada; en otro horario, no.
        Assert.True(await MesaReservadaAsync(mesaId, fecha));
        Assert.True(await MesaReservadaAsync(mesaId, fecha.AddHours(1)));
        Assert.False(await MesaReservadaAsync(mesaId, fecha.AddHours(2)));
        Assert.False(await MesaReservadaAsync(mesaId, fecha.AddDays(1)));
    }
}
