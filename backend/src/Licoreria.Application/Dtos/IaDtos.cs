using Licoreria.Domain.Enums;

namespace Licoreria.Application.Dtos;

public sealed record AiGeneracionDto(
    Guid Id,
    string Tipo,
    EstadoIa Estado,
    string Entrada,
    string? Salida,
    string? Modelo,
    decimal Costo,
    bool Aprobado,
    DateTime Fecha);

public sealed record SolicitarSeccionIaDto(
    string Prompt,
    string? Plantilla = null);

public sealed record SolicitarImagenIaDto(
    string Prompt,
    string? Estilo = null);
