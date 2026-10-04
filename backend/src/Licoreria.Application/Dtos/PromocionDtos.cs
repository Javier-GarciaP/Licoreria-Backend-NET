using Licoreria.Domain.Enums;

namespace Licoreria.Application.Dtos;

public sealed record PromocionDto(
    Guid Id,
    string Nombre,
    TipoPromocion Tipo,
    decimal Valor,
    bool Activo,
    DateTime? FechaInicio,
    DateTime? FechaFin);

public sealed record PromocionCrearDto(
    string Nombre,
    TipoPromocion Tipo,
    decimal Valor,
    bool Activo = true,
    DateTime? FechaInicio = null,
    DateTime? FechaFin = null);

public sealed record PromocionEditarDto(
    Guid Id,
    string Nombre,
    TipoPromocion Tipo,
    decimal Valor,
    bool Activo,
    DateTime? FechaInicio,
    DateTime? FechaFin);
