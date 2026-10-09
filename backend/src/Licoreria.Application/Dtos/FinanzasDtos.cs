using Licoreria.Domain.Enums;

namespace Licoreria.Application.Dtos;

public sealed record TasaCambioDto(
    Guid Id,
    DateTime Fecha,
    TipoTasa Tipo,
    decimal Valor);

public sealed record RegistrarTasaDto(
    DateTime? Fecha,
    TipoTasa Tipo,
    decimal Valor);
