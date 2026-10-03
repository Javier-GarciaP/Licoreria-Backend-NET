using Licoreria.Domain.Enums;

namespace Licoreria.Application.Dtos;

public sealed record AbrirMesaDto(
    string NombreMesa,
    Guid? MesaId = null);

public sealed record ComandaItemCrearDto(
    Guid VarianteId,
    decimal Cantidad,
    bool EsCortesia = false);

public sealed record CrearComandaDto(
    AreaDestino Area,
    IReadOnlyList<ComandaItemCrearDto> Items);

public sealed record RegistrarAbonoCuentaDto(
    Guid MetodoPagoId,
    decimal Monto,
    Moneda Moneda = Moneda.USD);

public sealed record CerrarCuentaDto(
    IReadOnlyList<VentaPagoDto> Pagos,
    decimal DescuentoUSD = 0);

public sealed record AbonoDto(
    Guid Id,
    Guid MetodoPagoId,
    string MetodoPago,
    decimal Monto,
    Moneda Moneda,
    DateTime Fecha);

public sealed record ComandaDetalleDto(
    Guid Id,
    Guid VarianteId,
    string Sku,
    string Nombre,
    decimal Cantidad,
    decimal PrecioUnitarioUSD,
    AreaDestino AreaDestino,
    EstadoItemComanda Estado,
    bool EsCortesia,
    decimal SubtotalUSD);

public sealed record ComandaDto(
    Guid Id,
    AreaDestino Area,
    EstadoComanda Estado,
    DateTime Fecha,
    IReadOnlyList<ComandaDetalleDto> Detalles);

public sealed record CuentaDto(
    Guid Id,
    Guid SesionMesaId,
    string NombreMesa,
    EstadoCuenta Estado,
    decimal Total,
    decimal TotalAbonado,
    decimal Saldo,
    DateTime AbiertaEn,
    IReadOnlyList<ComandaDto> Comandas,
    IReadOnlyList<AbonoDto> Abonos);

public sealed record ActualizarEstadoItemDto(
    EstadoItemComanda Estado);
