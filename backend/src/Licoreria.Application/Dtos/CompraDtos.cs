using Licoreria.Domain.Enums;

namespace Licoreria.Application.Dtos;

public sealed record ProveedorDto(
    Guid Id,
    string Nombre,
    string? Rif,
    string? Contacto,
    string? Telefono,
    string? Email,
    string? Direccion,
    int DiasCredito,
    bool Activo);

public sealed record ProveedorCrearDto(
    string Nombre,
    string? Rif = null,
    string? Contacto = null,
    string? Telefono = null,
    string? Email = null,
    string? Direccion = null,
    int DiasCredito = 0);

public sealed record ProveedorEditarDto(
    Guid Id,
    string Nombre,
    string? Rif,
    string? Contacto,
    string? Telefono,
    string? Email,
    string? Direccion,
    int DiasCredito,
    bool Activo);

public sealed record OrdenCompraDetalleDto(
    Guid Id,
    Guid VarianteId,
    string Sku,
    string Nombre,
    decimal Cantidad,
    decimal CostoUnitarioUSD,
    decimal CantidadRecibida,
    decimal SubtotalUSD);

public sealed record OrdenCompraDto(
    Guid Id,
    string Numero,
    Guid ProveedorId,
    string ProveedorNombre,
    DateTime Fecha,
    EstadoOrdenCompra Estado,
    string? Observaciones,
    decimal TotalUSD,
    IReadOnlyList<OrdenCompraDetalleDto> Detalles);

public sealed record OrdenCompraDetalleCrearDto(
    Guid VarianteId,
    decimal Cantidad,
    decimal CostoUnitarioUSD);

public sealed record OrdenCompraCrearDto(
    Guid ProveedorId,
    IReadOnlyList<OrdenCompraDetalleCrearDto> Detalles,
    string? Observaciones = null);

public sealed record RecepcionDetalleDto(
    Guid Id,
    Guid OrdenCompraDetalleId,
    Guid VarianteId,
    string Sku,
    decimal Cantidad,
    decimal CostoUnitarioUSD);

public sealed record RecepcionDto(
    Guid Id,
    Guid OrdenCompraId,
    string NumeroOrden,
    DateTime Fecha,
    decimal TotalUSD,
    string? Observaciones,
    IReadOnlyList<RecepcionDetalleDto> Detalles);

public sealed record RecepcionDetalleCrearDto(
    Guid OrdenCompraDetalleId,
    decimal Cantidad,
    decimal? CostoUnitarioUSD = null);

public sealed record RegistrarRecepcionDto(
    Guid OrdenCompraId,
    IReadOnlyList<RecepcionDetalleCrearDto> Detalles,
    string? Observaciones = null);

public sealed record CuentaPorPagarDto(
    Guid Id,
    Guid ProveedorId,
    string ProveedorNombre,
    Guid? OrdenCompraId,
    decimal MontoUSD,
    decimal SaldoUSD,
    DateTime Vencimiento,
    EstadoCuentaPorPagar Estado);

public sealed record PagoProveedorDto(
    Guid Id,
    Guid CuentaPorPagarId,
    decimal Monto,
    Moneda Moneda,
    string? Referencia,
    DateTime Fecha);

public sealed record RegistrarPagoProveedorDto(
    decimal Monto,
    Moneda Moneda = Moneda.USD,
    Guid? MetodoPagoId = null,
    string? Referencia = null);
