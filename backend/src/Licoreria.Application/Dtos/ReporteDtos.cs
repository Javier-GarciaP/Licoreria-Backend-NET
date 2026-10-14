using Licoreria.Domain.Enums;
using Licoreria.Domain.Services;

namespace Licoreria.Application.Dtos;

/// <summary>Indicadores del tablero del administrador.</summary>
public sealed record DashboardDto(
    decimal VentasHoyUSD,
    int VentasHoyCantidad,
    decimal VentasMesUSD,
    int VentasMesCantidad,
    decimal TicketPromedioUSD,
    int MermasMesCantidad,
    decimal MermasMesUnidades,
    int CuentasAbiertas,
    int ProductosStockBajo,
    int ReservasProximas,
    bool CajaAbierta,
    decimal CajaFondoInicial,
    int Clientes,
    int ProductosActivos,
    DateTime GeneradoEn);

/// <summary>Propinas registradas por usuario que facturó (RF-CUE-05).</summary>
public sealed record PropinaPorUsuarioDto(
    Guid UsuarioId,
    string UsuarioNombre,
    int Ventas,
    decimal TotalPropinaUSD);

public sealed record ReportePropinasDto(
    DateTime Desde,
    DateTime Hasta,
    decimal TotalPropinaUSD,
    IReadOnlyList<PropinaPorUsuarioDto> PorUsuario);

/// <summary>Reporte de ventas por rango de fechas.</summary>
public sealed record ReporteVentasDto(
    DateTime Desde,
    DateTime Hasta,
    decimal TotalUSD,
    decimal TotalBS,
    int Cantidad,
    decimal TicketPromedioUSD,
    IReadOnlyList<VentaPorDiaDto> PorDia,
    IReadOnlyList<VentaPorUsuarioDto> PorUsuario,
    IReadOnlyList<VentaPorMetodoPagoDto> PorMetodoPago);

public sealed record VentaPorDiaDto(
    DateTime Fecha,
    decimal TotalUSD,
    int Cantidad);

public sealed record VentaPorUsuarioDto(
    Guid UsuarioId,
    string UsuarioNombre,
    decimal TotalUSD,
    int Cantidad);

public sealed record VentaPorMetodoPagoDto(
    Guid MetodoPagoId,
    string MetodoPagoNombre,
    Moneda Moneda,
    decimal Monto,
    int Pagos);

/// <summary>Inventario valorizado a costo y a precio de venta.</summary>
public sealed record InventarioValorizadoDto(
    int Variantes,
    decimal UnidadesTotales,
    decimal CostoTotalUSD,
    decimal ValorVentaTotalUSD,
    int BajoMinimo,
    IReadOnlyList<InventarioValorizadoItemDto> Items);

public sealed record InventarioValorizadoItemDto(
    Guid VarianteId,
    string Sku,
    string ProductoNombre,
    string VarianteNombre,
    decimal Cantidad,
    decimal CostoUnitarioUSD,
    decimal CostoTotalUSD,
    decimal ValorVentaUSD,
    bool BajoMinimo);

/// <summary>Reporte de compras y cuentas por pagar del período.</summary>
public sealed record ReporteComprasDto(
    DateTime Desde,
    DateTime Hasta,
    int Recepciones,
    decimal TotalCompradoUSD,
    decimal TotalPorPagarUSD,
    decimal TotalPagadoUSD,
    int CuentasPendientes);

/// <summary>
/// Consumo agregado por día de la semana y hora del día. Alimenta el mapa de
/// calor de "horas pico". <c>DiaSemana</c> sigue la convención de .NET
/// (0 = domingo … 6 = sábado).
/// </summary>
public sealed record HeatmapFranjaDto(
    int DiaSemana,
    int Hora,
    decimal TotalUSD,
    int Cantidad);

public sealed record ReporteHeatmapDto(
    DateTime Desde,
    DateTime Hasta,
    IReadOnlyList<HeatmapFranjaDto> Franjas);

/// <summary>Fila cruda de stock para clasificar la salud del inventario.</summary>
public sealed record StockSaludCrudoDto(
    Guid VarianteId,
    string Sku,
    string ProductoNombre,
    string VarianteNombre,
    decimal Cantidad,
    decimal StockMinimo,
    decimal StockMaximo);

/// <summary>Diagnóstico de un producto con su estado de salud y sugerencia de compra.</summary>
public sealed record DiagnosticoInventarioItemDto(
    Guid VarianteId,
    string Sku,
    string ProductoNombre,
    string VarianteNombre,
    decimal Cantidad,
    decimal StockMinimo,
    decimal StockMaximo,
    string Estado,
    string MensajeDiagnostico,
    int UnidadesCompraSugeridas);

/// <summary>Salud global del inventario: conteos por estado y detalle de productos.</summary>
public sealed record DiagnosticoInventarioDto(
    int Total,
    int SinStock,
    int RiesgoCritico,
    int Subabastecido,
    int Optimo,
    int Sobreabastecido,
    int Excesivo,
    IReadOnlyList<DiagnosticoInventarioItemDto> Items);

/// <summary>Consumo valorizado de merma por motivo.</summary>
public sealed record ComparativoMermaDto(
    MotivoMerma Motivo,
    int Registros,
    decimal Unidades,
    decimal ValorUSD);

/// <summary>Comparativo de mermas (valorizadas) frente a ventas del período.</summary>
public sealed record ReporteMermasVsVentasDto(
    DateTime Desde,
    DateTime Hasta,
    decimal VentasUSD,
    decimal MermaValorizadaUSD,
    decimal Porcentaje,
    IReadOnlyList<ComparativoMermaDto> PorMotivo);
