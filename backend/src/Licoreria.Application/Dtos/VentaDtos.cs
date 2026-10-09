using Licoreria.Domain.Enums;

namespace Licoreria.Application.Dtos;

public sealed record MetodoPagoDto(
    Guid Id,
    string Codigo,
    string Nombre);

public sealed record VentaItemDto(
    Guid VarianteId,
    decimal Cantidad,
    decimal? PrecioUnitarioUSD = null,
    decimal DescuentoUSD = 0,
    bool EsCortesia = false,
    bool YaDescontado = false);

public sealed record VentaPagoDto(
    Guid MetodoPagoId,
    decimal Monto,
    Moneda Moneda = Moneda.USD,
    decimal Propina = 0);

public sealed record RegistrarVentaDto(
    IReadOnlyList<VentaItemDto> Items,
    IReadOnlyList<VentaPagoDto> Pagos,
    decimal DescuentoUSD = 0,
    Guid? CuentaId = null,
    Guid? PromocionId = null);

public sealed record RegistrarPagoVentaDto(
    Guid MetodoPagoId,
    decimal Monto,
    Moneda Moneda = Moneda.USD,
    decimal Propina = 0);

public sealed record VentaDetalleDto(
    Guid Id,
    Guid VarianteId,
    string Sku,
    string Nombre,
    decimal Cantidad,
    decimal PrecioUnitarioUSD,
    decimal DescuentoUSD,
    bool EsCortesia,
    decimal SubtotalUSD);

public sealed record VentaPagoRegistradoDto(
    Guid Id,
    Guid MetodoPagoId,
    string MetodoPago,
    decimal Monto,
    Moneda Moneda,
    decimal Propina);

public sealed record VentaDto(
    Guid Id,
    DateTime Fecha,
    decimal TasaCambio,
    decimal SubtotalUSD,
    decimal DescuentoUSD,
    decimal TotalUSD,
    decimal TotalBS,
    EstadoVenta Estado,
    Guid UsuarioId,
    Guid? SesionCajaId,
    string? NumeroComprobante,
    IReadOnlyList<VentaDetalleDto> Detalles,
    IReadOnlyList<VentaPagoRegistradoDto> Pagos);

public sealed record DevolucionDetalleCrearDto(
    Guid VarianteId,
    decimal Cantidad);

public sealed record RegistrarDevolucionDto(
    string Motivo,
    bool ReintegrarInventario,
    IReadOnlyList<DevolucionDetalleCrearDto> Detalles);

public sealed record DevolucionDetalleDto(
    Guid VarianteId,
    string Sku,
    decimal Cantidad,
    decimal PrecioUnitarioUSD);

public sealed record DevolucionDto(
    Guid Id,
    Guid VentaId,
    string Motivo,
    decimal MontoUSD,
    bool ReintegrarInventario,
    DateTime Fecha,
    IReadOnlyList<DevolucionDetalleDto> Detalles);
