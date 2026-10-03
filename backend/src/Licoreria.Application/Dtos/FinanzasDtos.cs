using Licoreria.Domain.Enums;

namespace Licoreria.Application.Dtos;

public sealed record MonedaDto(
    string Codigo,
    string Nombre);

public sealed record TasaCambioDto(
    Guid Id,
    DateTime Fecha,
    TipoTasa Tipo,
    decimal Valor);

public sealed record RegistrarTasaDto(
    DateTime? Fecha,
    TipoTasa Tipo,
    decimal Valor);

public sealed record MovimientoTesoreriaDto(
    Guid Id,
    TipoMovimientoTesoreria Tipo,
    decimal Monto,
    Moneda Moneda,
    string Motivo,
    string? ReferenciaTipo,
    Guid? ReferenciaId,
    DateTime Fecha);

public sealed record RegistrarMovimientoTesoreriaDto(
    TipoMovimientoTesoreria Tipo,
    decimal Monto,
    Moneda Moneda,
    string Motivo,
    string? ReferenciaTipo = null,
    Guid? ReferenciaId = null);
