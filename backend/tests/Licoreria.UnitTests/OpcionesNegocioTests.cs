using Licoreria.Application.Common;
using Licoreria.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace Licoreria.UnitTests;

public sealed class OpcionesNegocioTests
{
    private static IConfiguration Config(Dictionary<string, string?> valores)
        => new ConfigurationBuilder().AddInMemoryCollection(valores).Build();

    [Fact]
    public void SinConfiguracion_UsanLosValoresPorDefecto()
    {
        var config = Config([]);

        var fidelidad = config.GetSection(OpcionesFidelidad.SectionName).Get<OpcionesFidelidad>() ?? new OpcionesFidelidad();
        var credito = config.GetSection(OpcionesCredito.SectionName).Get<OpcionesCredito>() ?? new OpcionesCredito();

        Assert.Equal(1m, fidelidad.PuntosPorUSD);
        Assert.Equal(30, credito.DiasVencimiento);
    }

    [Fact]
    public void LaConfiguracionSobrescribeLosValores_YCeroDesactivaPuntos()
    {
        var config = Config(new()
        {
            ["Fidelidad:PuntosPorUSD"] = "0",
            ["Credito:DiasVencimiento"] = "15"
        });

        var fidelidad = config.GetSection(OpcionesFidelidad.SectionName).Get<OpcionesFidelidad>()!;
        var credito = config.GetSection(OpcionesCredito.SectionName).Get<OpcionesCredito>()!;

        Assert.Equal(0m, fidelidad.PuntosPorUSD);
        Assert.Equal(15, credito.DiasVencimiento);
    }

    [Fact]
    public void RevertirPuntos_NuncaDejaElSaldoNegativo()
    {
        var cliente = new Cliente { Nombre = "Ana" };
        cliente.AcumularPuntos(8);

        Assert.Equal(5, cliente.RevertirPuntos(5));
        Assert.Equal(3, cliente.RevertirPuntos(10));
        Assert.Equal(0, cliente.Puntos);
        Assert.Equal(0, cliente.RevertirPuntos(4));
    }
}
