namespace Licoreria.Domain.Services;

/// <summary>
/// Jornada de operación del local (por ejemplo, de las 12:00 a las 06:00 del día siguiente), expresada
/// en hora local mediante un desfase fijo respecto de UTC. Sirve para decidir qué reservas corresponden
/// a «hoy»: una reserva de las 10 p. m. marca la mesa como reservada durante toda su jornada, y la
/// madrugada pertenece a la jornada que empezó el día anterior.
/// </summary>
public sealed class JornadaOperativa
{
    private readonly TimeSpan _desfaseUtc;
    private readonly TimeSpan _inicio;
    private readonly TimeSpan _duracion;

    /// <param name="desfaseUtcHoras">Horas de la hora local respecto de UTC (Venezuela: -4, sin horario de verano).</param>
    /// <param name="inicioHora">Hora local (0-23) en que empieza la jornada.</param>
    /// <param name="finHora">Hora local (0-23) en que termina; si es menor o igual al inicio, termina al día siguiente.</param>
    public JornadaOperativa(double desfaseUtcHoras = -4, int inicioHora = 12, int finHora = 6)
    {
        if (inicioHora is < 0 or > 23 || finHora is < 0 or > 23)
        {
            throw new ArgumentOutOfRangeException(nameof(inicioHora), "Las horas de la jornada deben estar entre 0 y 23.");
        }

        _desfaseUtc = TimeSpan.FromHours(desfaseUtcHoras);
        _inicio = TimeSpan.FromHours(inicioHora);

        var horas = (finHora - inicioHora + 24) % 24;
        _duracion = TimeSpan.FromHours(horas == 0 ? 24 : horas);
    }

    /// <summary>
    /// Ventana UTC de la jornada que contiene el instante dado o, si todavía no empieza
    /// (por ejemplo, de mañana), la que empieza más adelante ese mismo día.
    /// </summary>
    public (DateTime DesdeUtc, DateTime HastaUtc) VentanaQueContiene(DateTime ahoraUtc)
    {
        var local = DateTime.SpecifyKind(ahoraUtc, DateTimeKind.Unspecified) + _desfaseUtc;

        var inicioHoy = local.Date + _inicio;
        var inicioAyer = inicioHoy.AddDays(-1);

        // La jornada de ayer sigue vigente mientras no haya pasado su duración (madrugada).
        var inicioLocal = local < inicioAyer + _duracion ? inicioAyer : inicioHoy;
        var finLocal = inicioLocal + _duracion;

        return (
            DateTime.SpecifyKind(inicioLocal - _desfaseUtc, DateTimeKind.Utc),
            DateTime.SpecifyKind(finLocal - _desfaseUtc, DateTimeKind.Utc));
    }
}
