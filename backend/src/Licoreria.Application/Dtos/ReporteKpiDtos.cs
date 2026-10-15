namespace Licoreria.Application.Dtos;

/// <summary>Criterio de orden del ranking de productos.</summary>
public enum OrdenProductos
{
    Unidades = 1,
    Ingresos = 2
}

/// <summary>Situación de la rotación de un producto en el periodo.</summary>
public enum EstadoRotacion
{
    /// <summary>Hubo ventas y existencia: la rotación es unidades / existencia.</summary>
    Calculada = 1,

    /// <summary>Hubo ventas pero la existencia actual es 0: no se divide entre cero.</summary>
    NoCalculable = 2,

    /// <summary>Hay existencia y ninguna venta en el periodo.</summary>
    SinMovimiento = 3
}

/// <summary>Ventas del periodo y existencia actual de una variante, sin derivados.</summary>
public sealed record ProductoVendidoCrudoDto(
    Guid VarianteId,
    string Sku,
    string ProductoNombre,
    string VarianteNombre,
    decimal Unidades,
    decimal IngresoUSD,
    decimal Existencia);

/// <summary>Producto del ranking con su peso en el total y su rotación.</summary>
public sealed record ProductoVendidoDto(
    Guid VarianteId,
    string Sku,
    string ProductoNombre,
    string VarianteNombre,
    decimal Unidades,
    decimal IngresoUSD,
    decimal PorcentajeUnidades,
    decimal PorcentajeIngreso,
    decimal Existencia,
    decimal? Rotacion,
    EstadoRotacion EstadoRotacion);

/// <summary>Top de productos vendidos y productos con existencia sin ventas en el periodo.</summary>
public sealed record ReporteProductosDto(
    DateTime Desde,
    DateTime Hasta,
    OrdenProductos Orden,
    int Limite,
    decimal TotalUnidades,
    decimal TotalIngresoUSD,
    IReadOnlyList<ProductoVendidoDto> Top,
    IReadOnlyList<ProductoVendidoDto> SinMovimiento);

/// <summary>Ocupación y sesiones de una zona antes de calcular promedios y porcentajes.</summary>
public sealed record OcupacionZonaCrudaDto(
    Guid? ZonaId,
    string Zona,
    int MesasActivas,
    int MesasOcupadas,
    int Sesiones,
    int SesionesAbiertas,
    decimal MinutosPermanencia);

/// <summary>Ocupación actual y rotación de mesas de una zona.</summary>
public sealed record OcupacionZonaDto(
    Guid? ZonaId,
    string Zona,
    int MesasActivas,
    int MesasOcupadas,
    decimal PorcentajeOcupacion,
    int Sesiones,
    int SesionesAbiertas,
    decimal? SesionesPorMesa,
    decimal? PermanenciaPromedioMinutos);

/// <summary>Ocupación de mesas en este momento y permanencia y rotación del periodo.</summary>
public sealed record ReporteOcupacionDto(
    DateTime Desde,
    DateTime Hasta,
    DateTime GeneradoEn,
    int MesasActivas,
    int MesasOcupadas,
    decimal PorcentajeOcupacion,
    int Sesiones,
    int SesionesAbiertas,
    decimal? SesionesPorMesa,
    decimal? PermanenciaPromedioMinutos,
    IReadOnlyList<OcupacionZonaDto> PorZona);

/// <summary>Cantidad de reservas del periodo en un estado.</summary>
public sealed record ReservasPorEstadoDto(
    Domain.Enums.EstadoReserva Estado,
    int Cantidad);

/// <summary>Reservas del periodo por estado y tasa de no asistencia.</summary>
public sealed record ReporteReservasAsistenciaDto(
    DateTime Desde,
    DateTime Hasta,
    int Total,
    int Asistieron,
    int NoAsistieron,
    int Canceladas,
    decimal? TasaNoAsistencia,
    IReadOnlyList<ReservasPorEstadoDto> PorEstado);

/// <summary>Turno de caja cerrado con su arqueo.</summary>
public sealed record TurnoCajaDto(
    Guid SesionId,
    Guid UsuarioId,
    string UsuarioNombre,
    DateTime AbiertaEn,
    DateTime CerradaEn,
    decimal MontoEsperado,
    decimal MontoContado,
    decimal Descuadre);

/// <summary>Descuadre de caja por turno cerrado del periodo.</summary>
public sealed record ReporteCajaDto(
    DateTime Desde,
    DateTime Hasta,
    int Turnos,
    decimal DescuadreTotal,
    decimal DescuadrePromedio,
    decimal MayorDescuadre,
    IReadOnlyList<TurnoCajaDto> Items);

/// <summary>Actividad de un mesonero en el periodo antes de calcular razones y promedios.</summary>
public sealed record MesoneroCrudoDto(
    Guid? UsuarioId,
    string Nombre,
    int Comandas,
    int Cuentas,
    int ItemsTotales,
    int ItemsCancelados,
    int ItemsServidos,
    decimal VentasUSD,
    int ItemsConTiempo,
    double SegundosServicio);

/// <summary>Desempeño de un mesonero: ventas, calidad de servicio y rapidez.</summary>
public sealed record MesoneroDto(
    Guid? UsuarioId,
    string Nombre,
    int Comandas,
    int ItemsServidos,
    int ItemsCancelados,
    int Cuentas,
    decimal VentasGeneradasUSD,
    decimal TicketPorCuentaUSD,
    decimal TasaCancelacion,
    decimal? TiempoServicioPromedioSegundos);

/// <summary>Desempeño por mesonero en el periodo, ordenado por ventas generadas.</summary>
public sealed record ReporteMesonerosDto(
    DateTime Desde,
    DateTime Hasta,
    decimal TotalVentasUSD,
    IReadOnlyList<MesoneroDto> Mesoneros);
