using Licoreria.Domain.Enums;
using Licoreria.Domain.Services;

namespace Licoreria.Application.Dtos;

public sealed record StockDto(
    Guid VarianteId,
    string Sku,
    string ProductoNombre,
    string VarianteNombre,
    decimal Cantidad,
    decimal CantidadReservada,
    decimal StockMinimo,
    decimal StockMaximo,
    bool BajoMinimo);

public sealed record MovimientoKardexDto(
    Guid Id,
    Guid VarianteId,
    string Sku,
    TipoMovimientoInventario Tipo,
    decimal Cantidad,
    decimal? CostoUnitario,
    string? ReferenciaTipo,
    Guid? ReferenciaId,
    string? Motivo,
    DateTime Fecha);

public sealed record RegistrarMermaDto(
    Guid VarianteId,
    decimal Cantidad,
    MotivoMerma Motivo,
    bool ReponerSinCobro = false);

public sealed record MermaDto(
    Guid Id,
    Guid MovimientoId,
    Guid VarianteId,
    string Sku,
    decimal Cantidad,
    MotivoMerma Motivo,
    bool Repuesto,
    DateTime Fecha);

public sealed record AjusteInventarioDto(
    Guid VarianteId,
    decimal Cantidad,
    string Motivo);

public sealed record ReporteMermaDto(
    DateTime Desde,
    DateTime Hasta,
    int TotalRegistros,
    decimal TotalUnidades,
    IReadOnlyList<ReporteMermaPorMotivoDto> PorMotivo);

public sealed record ReporteMermaPorMotivoDto(
    MotivoMerma Motivo,
    int Registros,
    decimal Unidades);
