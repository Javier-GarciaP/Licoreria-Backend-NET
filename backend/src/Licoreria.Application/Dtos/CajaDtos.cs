using Licoreria.Domain.Enums;

namespace Licoreria.Application.Dtos;

public sealed record DenominacionDto(
    Guid Id,
    Moneda Moneda,
    TipoDenominacion Tipo,
    decimal Valor);

public sealed record AbrirCajaDto(
    decimal FondoInicial);

public sealed record MovimientoCajaCrearDto(
    TipoMovimientoCaja Tipo,
    decimal Monto,
    Moneda Moneda,
    string Motivo);

public sealed record MovimientoCajaDto(
    Guid Id,
    TipoMovimientoCaja Tipo,
    decimal Monto,
    Moneda Moneda,
    string Motivo,
    DateTime Fecha);

public sealed record ArqueoLineaCrearDto(
    Guid DenominacionId,
    int Cantidad);

public sealed record CerrarCajaDto(
    IReadOnlyList<ArqueoLineaCrearDto> Arqueo);

public sealed record ArqueoLineaDto(
    Guid DenominacionId,
    Moneda Moneda,
    decimal Valor,
    int Cantidad,
    decimal Subtotal);

public sealed record SesionCajaDto(
    Guid Id,
    EstadoSesionCaja Estado,
    decimal FondoInicial,
    decimal MontoEsperado,
    decimal MontoContado,
    decimal Descuadre,
    DateTime AbiertaEn,
    DateTime? CerradaEn,
    IReadOnlyList<MovimientoCajaDto> Movimientos,
    IReadOnlyList<ArqueoLineaDto> Arqueo);
