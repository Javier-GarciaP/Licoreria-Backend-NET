using Licoreria.Domain.Common;
using Licoreria.Domain.Enums;

namespace Licoreria.Domain.Entities;

/// <summary>
/// Trabajo del módulo de IA con entrada, salida, modelo, costo y aprobación.
/// </summary>
public class AiGeneracion : BaseEntity
{
    public string Tipo { get; set; } = string.Empty;
    public EstadoIa Estado { get; set; } = EstadoIa.Pendiente;
    public string Entrada { get; set; } = string.Empty;
    public string? Salida { get; set; }
    public string? Modelo { get; set; }
    public decimal Costo { get; set; }
    public bool Aprobado { get; set; }

    public void Completar(string salida, string modelo, decimal costo)
    {
        Salida = salida;
        Modelo = modelo;
        Costo = costo;
        Estado = EstadoIa.Completado;
        MarcarModificado();
    }

    public void Aprobar()
    {
        Aprobado = true;
        MarcarModificado();
    }
}

/// <summary>Resultado de digitalizar un plano desde una imagen.</summary>
public class PlanoGenerado : BaseEntity
{
    public Guid AiGeneracionId { get; set; }
    public AiGeneracion AiGeneracion { get; set; } = null!;

    public string Resultado { get; set; } = string.Empty;
}
