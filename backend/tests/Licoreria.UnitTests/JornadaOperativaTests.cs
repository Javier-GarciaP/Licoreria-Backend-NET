using Licoreria.Domain.Services;

namespace Licoreria.UnitTests;

/// <summary>Jornada nocturna por defecto: de 12:00 a 06:00 (hora local, UTC-4).</summary>
public sealed class JornadaOperativaTests
{
    private readonly JornadaOperativa _jornada = new();

    private static DateTime Utc(int dia, int hora, int minuto = 0) => new(2026, 10, dia, hora, minuto, 0, DateTimeKind.Utc);

    [Fact]
    public void Noche_PerteneceALaJornadaQueEmpezoEseDiaAlMediodia()
    {
        // 20:00 local = 00:00 UTC del día siguiente.
        var (desde, hasta) = _jornada.VentanaQueContiene(Utc(9, 0));

        Assert.Equal(Utc(8, 16), desde);   // 12:00 local del día 8
        Assert.Equal(Utc(9, 10), hasta);   // 06:00 local del día 9
    }

    [Fact]
    public void Madrugada_PerteneceALaJornadaQueEmpezoElDiaAnterior()
    {
        // 02:00 local del día 9 = 06:00 UTC.
        var (desde, hasta) = _jornada.VentanaQueContiene(Utc(9, 6));

        Assert.Equal(Utc(8, 16), desde);
        Assert.Equal(Utc(9, 10), hasta);
    }

    [Fact]
    public void Manana_VeLaJornadaDeEstaNocheYNoLaQueTermino()
    {
        // 09:00 local del día 9 = 13:00 UTC: la jornada anterior ya terminó; la de hoy empieza a las 12:00.
        var (desde, hasta) = _jornada.VentanaQueContiene(Utc(9, 13));

        Assert.Equal(Utc(9, 16), desde);
        Assert.Equal(Utc(10, 10), hasta);
    }

    [Fact]
    public void ElInstanteExactoDeFin_YaEsLaSiguienteJornada()
    {
        // 06:00 local = 10:00 UTC: la jornada terminó.
        var (desde, _) = _jornada.VentanaQueContiene(Utc(9, 10));

        Assert.Equal(Utc(9, 16), desde);
    }

    [Fact]
    public void ElInstanteExactoDeInicio_EsLaJornadaQueEmpieza()
    {
        var (desde, hasta) = _jornada.VentanaQueContiene(Utc(9, 16));

        Assert.Equal(Utc(9, 16), desde);
        Assert.Equal(Utc(10, 10), hasta);
    }

    [Fact]
    public void UnaReservaDeLas10DeLaNoche_CaeDentroDeLaJornadaDesdeLaManana()
    {
        // 22:00 local del día 9 = 02:00 UTC del día 10.
        var reserva = Utc(10, 2);
        var (desde, hasta) = _jornada.VentanaQueContiene(Utc(9, 13)); // 09:00 local

        Assert.True(reserva >= desde && reserva <= hasta);
    }

    [Fact]
    public void UnaReservaDeMananaPorLaNoche_NoCaeEnLaJornadaDeHoy()
    {
        // 22:00 local del día 10 = 02:00 UTC del día 11.
        var reserva = Utc(11, 2);
        var (desde, hasta) = _jornada.VentanaQueContiene(Utc(9, 13));

        Assert.False(reserva >= desde && reserva <= hasta);
    }

    [Fact]
    public void JornadaDelMismoDia_SeCalculaSinCruzarMedianoche()
    {
        var diurna = new JornadaOperativa(desfaseUtcHoras: 0, inicioHora: 9, finHora: 18);

        var (desde, hasta) = diurna.VentanaQueContiene(Utc(9, 12));

        Assert.Equal(Utc(9, 9), desde);
        Assert.Equal(Utc(9, 18), hasta);
    }

    [Theory]
    [InlineData(-1, 6)]
    [InlineData(12, 24)]
    public void HorasFueraDeRango_Falla(int inicio, int fin)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new JornadaOperativa(-4, inicio, fin));
    }
}
