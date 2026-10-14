using Licoreria.Domain.Common;
using Licoreria.Domain.Enums;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Botella o envase que la barra destapó para servir porciones. Guarda cuántas porciones quedan;
/// al llegar a cero pasa a agotado.
/// </summary>
public class EnvaseAbierto : BaseEntity
{
    public Guid PorcionVarianteId { get; set; }
    public ProductoVariante PorcionVariante { get; set; } = null!;

    public Guid PresentacionVarianteId { get; set; }
    public ProductoVariante PresentacionVariante { get; set; } = null!;

    public int PorcionesIniciales { get; set; }
    public int PorcionesRestantes { get; set; }

    public EstadoEnvase Estado { get; set; } = EstadoEnvase.Abierto;
    public DateTime AbiertoEn { get; set; }
    public Guid? AbiertoPorUsuarioId { get; set; }
    public DateTime? CerradoEn { get; set; }
    public string? MotivoDescarte { get; set; }

    public static EnvaseAbierto Destapar(
        Guid porcionVarianteId,
        Guid presentacionVarianteId,
        int porciones,
        Guid? usuarioId,
        DateTime ahora)
    {
        if (porciones < 1)
        {
            throw new InvalidOperationException("Un envase debe dar al menos una porción.");
        }

        return new EnvaseAbierto
        {
            PorcionVarianteId = porcionVarianteId,
            PresentacionVarianteId = presentacionVarianteId,
            PorcionesIniciales = porciones,
            PorcionesRestantes = porciones,
            AbiertoPorUsuarioId = usuarioId,
            AbiertoEn = ahora
        };
    }

    /// <summary>Sirve porciones del envase; pasa a agotado cuando no queda ninguna.</summary>
    public void Consumir(int porciones, DateTime ahora)
    {
        if (Estado != EstadoEnvase.Abierto)
        {
            throw new InvalidOperationException("El envase no está abierto.");
        }

        if (porciones < 1)
        {
            throw new InvalidOperationException("Las porciones a servir deben ser al menos una.");
        }

        if (porciones > PorcionesRestantes)
        {
            throw new InvalidOperationException("El envase no tiene tantas porciones.");
        }

        PorcionesRestantes -= porciones;
        if (PorcionesRestantes == 0)
        {
            Estado = EstadoEnvase.Agotado;
            CerradoEn = ahora;
        }

        MarcarModificado();
    }

    /// <summary>Retira el envase con un motivo y devuelve las porciones que se perdieron.</summary>
    public int Descartar(string motivo, DateTime ahora)
    {
        if (Estado != EstadoEnvase.Abierto)
        {
            throw new InvalidOperationException("Solo se puede descartar un envase abierto.");
        }

        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new InvalidOperationException("El motivo del descarte es obligatorio.");
        }

        var perdidas = PorcionesRestantes;
        PorcionesRestantes = 0;
        Estado = EstadoEnvase.Descartado;
        MotivoDescarte = motivo.Trim();
        CerradoEn = ahora;
        MarcarModificado();
        return perdidas;
    }
}
