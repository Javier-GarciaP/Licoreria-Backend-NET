using Licoreria.Domain.Enums;

namespace Licoreria.Application.Dtos;

public sealed record ClienteDto(
    Guid Id,
    string Nombre,
    string? Rif,
    string? Ci,
    string? Email,
    string? Telefono,
    string? Direccion,
    int Puntos,
    bool Activo);

public sealed record ClienteCrearDto(
    string Nombre,
    string? Rif = null,
    string? Ci = null,
    string? Email = null,
    string? Telefono = null,
    string? Direccion = null);

public sealed record ClienteEditarDto(
    Guid Id,
    string Nombre,
    string? Rif,
    string? Ci,
    string? Email,
    string? Telefono,
    string? Direccion,
    bool Activo);

public sealed record PuntosMovimientoDto(
    Guid Id,
    TipoMovimientoPuntos Tipo,
    int Puntos,
    string Motivo,
    DateTime Fecha);

public sealed record PuntosOperacionDto(
    int Puntos,
    string Motivo,
    string? ReferenciaTipo = null,
    Guid? ReferenciaId = null);

public sealed record CuentaPorCobrarDto(
    Guid Id,
    Guid ClienteId,
    string ClienteNombre,
    decimal MontoUSD,
    decimal SaldoUSD,
    DateTime Vencimiento,
    EstadoCuentaPorCobrar Estado);

public sealed record CuentaPorCobrarCrearDto(
    Guid ClienteId,
    decimal MontoUSD,
    DateTime Vencimiento);

public sealed record PagoCuentaPorCobrarDto(
    decimal Monto);
