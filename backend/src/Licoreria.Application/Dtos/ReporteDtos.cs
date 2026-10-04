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
