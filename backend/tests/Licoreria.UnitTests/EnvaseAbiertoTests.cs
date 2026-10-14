using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.UnitTests;

public sealed class EnvaseAbiertoTests
{
    private static readonly DateTime Ahora = new(2026, 10, 14, 20, 0, 0, DateTimeKind.Utc);

    private static EnvaseAbierto Envase(int porciones = 4)
        => EnvaseAbierto.Destapar(Guid.NewGuid(), Guid.NewGuid(), porciones, Guid.NewGuid(), Ahora);

    [Fact]
    public void Destapar_CreaUnEnvaseAbiertoConTodasSusPorciones()
    {
        var envase = Envase(8);

        Assert.Equal(EstadoEnvase.Abierto, envase.Estado);
        Assert.Equal(8, envase.PorcionesIniciales);
        Assert.Equal(8, envase.PorcionesRestantes);
        Assert.Equal(Ahora, envase.AbiertoEn);
    }

    [Fact]
    public void Destapar_SinPorciones_Falla()
    {
        Assert.Throws<InvalidOperationException>(() => Envase(0));
    }

    [Fact]
    public void Consumir_DescuentaYSigueAbierto()
    {
        var envase = Envase();

        envase.Consumir(3, Ahora);

        Assert.Equal(1, envase.PorcionesRestantes);
        Assert.Equal(EstadoEnvase.Abierto, envase.Estado);
        Assert.Null(envase.CerradoEn);
    }

    [Fact]
    public void Consumir_LaUltimaPorcion_LoPasaAAgotado()
    {
        var envase = Envase(2);

        envase.Consumir(2, Ahora.AddMinutes(5));

        Assert.Equal(0, envase.PorcionesRestantes);
        Assert.Equal(EstadoEnvase.Agotado, envase.Estado);
        Assert.Equal(Ahora.AddMinutes(5), envase.CerradoEn);
    }

    [Fact]
    public void Consumir_MasDeLoRestante_FallaSinCambios()
    {
        var envase = Envase(2);

        Assert.Throws<InvalidOperationException>(() => envase.Consumir(3, Ahora));
        Assert.Equal(2, envase.PorcionesRestantes);
    }

    [Fact]
    public void Consumir_UnEnvaseAgotado_Falla()
    {
        var envase = Envase(1);
        envase.Consumir(1, Ahora);

        Assert.Throws<InvalidOperationException>(() => envase.Consumir(1, Ahora));
    }

    [Fact]
    public void Descartar_DevuelveLasPorcionesPerdidasYGuardaElMotivo()
    {
        var envase = Envase(4);
        envase.Consumir(1, Ahora);

        var perdidas = envase.Descartar("  Derrame  ", Ahora);

        Assert.Equal(3, perdidas);
        Assert.Equal(EstadoEnvase.Descartado, envase.Estado);
        Assert.Equal("Derrame", envase.MotivoDescarte);
        Assert.Equal(0, envase.PorcionesRestantes);
    }

    [Fact]
    public void Descartar_SinMotivo_FallaYSigueAbierto()
    {
        var envase = Envase();

        Assert.Throws<InvalidOperationException>(() => envase.Descartar(" ", Ahora));
        Assert.Equal(EstadoEnvase.Abierto, envase.Estado);
    }

    [Fact]
    public void Descartar_UnEnvaseYaCerrado_Falla()
    {
        var envase = Envase(1);
        envase.Consumir(1, Ahora);

        Assert.Throws<InvalidOperationException>(() => envase.Descartar("Tarde", Ahora));
    }
}
