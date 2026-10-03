using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;
using Licoreria.Domain.Services;

namespace Licoreria.Application.Services;

public sealed class ServicioInventario : IServicioInventario
{
    private readonly IInventarioRepository _inventario;
    private readonly IRepository<ProductoVariante> _variantes;
    private readonly EvaluadorMerma _evaluadorMerma;
    private readonly ConversorMoneda _conversorMoneda;
    private readonly IRelojSistema _reloj;

    public ServicioInventario(
        IInventarioRepository inventario,
        IRepository<ProductoVariante> variantes,
        EvaluadorMerma evaluadorMerma,
        ConversorMoneda conversorMoneda,
        IRelojSistema reloj)
    {
        _inventario = inventario;
        _variantes = variantes;
        _evaluadorMerma = evaluadorMerma;
        _conversorMoneda = conversorMoneda;
        _reloj = reloj;
    }

    public ResultadoMermaDto EvaluarMerma(MermaRequest request)
    {
        if (!Enum.TryParse<MotivoMerma>(request.Motivo, ignoreCase: true, out var motivo))
        {
            throw new ReglaNegocioException($"El motivo de merma '{request.Motivo}' no es válido.");
        }

        var resultado = _evaluadorMerma.Evaluar(motivo, request.Cantidad, request.ReponerSinCobro);

        var movimientos = resultado.Movimientos
            .Select(m => new MovimientoInventarioDto(m.Tipo, m.Cantidad, m.Motivo))
            .ToList();

        return new ResultadoMermaDto(movimientos, resultado.TotalUnidadesDescontadas);
    }

    public decimal ConvertirAusdBolivares(decimal montoUsd, decimal tasaCambio)
        => _conversorMoneda.ConvertirAusdBolivares(montoUsd, tasaCambio);

    public async Task<ResultadoPaginado<StockDto>> ObtenerStockAsync(
        PaginacionRequest paginacion,
        bool soloBajoMinimo = false,
        string? busqueda = null,
        CancellationToken cancellationToken = default)
    {
        var pagina = await _inventario.ObtenerStockPaginadoAsync(paginacion, soloBajoMinimo, busqueda, cancellationToken);
        var items = pagina.Items.Select(MapearStock).ToList();
        return ResultadoPaginado<StockDto>.Crear(items, pagina.Page, pagina.PageSize, pagina.TotalItems);
    }

    public async Task<ResultadoPaginado<MovimientoKardexDto>> ObtenerMovimientosAsync(
        PaginacionRequest paginacion,
        Guid? varianteId = null,
        TipoMovimientoInventario? tipo = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default)
    {
        var pagina = await _inventario.ObtenerMovimientosPaginadoAsync(paginacion, varianteId, tipo, desde, hasta, cancellationToken);
        var items = pagina.Items.Select(MapearMovimiento).ToList();
        return ResultadoPaginado<MovimientoKardexDto>.Crear(items, pagina.Page, pagina.PageSize, pagina.TotalItems);
    }

    public async Task<MermaDto> RegistrarMermaAsync(RegistrarMermaDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Cantidad <= 0)
        {
            throw new ReglaNegocioException("La cantidad de la merma debe ser mayor que cero.");
        }

        var variante = await _variantes.GetByIdAsync(dto.VarianteId, cancellationToken)
            ?? throw new NoEncontradoException($"No existe la variante {dto.VarianteId}.");

        await AplicarStockAsync(dto.VarianteId, -dto.Cantidad, cancellationToken);

        var movimiento = new MovimientoInventario
        {
            VarianteId = dto.VarianteId,
            Tipo = TipoMovimientoInventario.Merma,
            Cantidad = -dto.Cantidad,
            ReferenciaTipo = "merma",
            Motivo = $"Merma: {dto.Motivo}"
        };

        await _inventario.AgregarMovimientoAsync(movimiento, cancellationToken);
        await _inventario.SaveChangesAsync(cancellationToken);

        var merma = new Merma
        {
            MovimientoId = movimiento.Id,
            Motivo = dto.Motivo,
            Repuesto = dto.ReponerSinCobro
        };

        await _inventario.AgregarMermaAsync(merma, cancellationToken);

        if (dto.ReponerSinCobro)
        {
            await AplicarStockAsync(dto.VarianteId, -dto.Cantidad, cancellationToken);

            var cortesia = new MovimientoInventario
            {
                VarianteId = dto.VarianteId,
                Tipo = TipoMovimientoInventario.Cortesia,
                Cantidad = -dto.Cantidad,
                ReferenciaTipo = "cortesia",
                ReferenciaId = movimiento.Id,
                Motivo = "Reposición sin cobro"
            };

            await _inventario.AgregarMovimientoAsync(cortesia, cancellationToken);
        }

        await _inventario.SaveChangesAsync(cancellationToken);

        return new MermaDto(
            merma.Id,
            movimiento.Id,
            dto.VarianteId,
            variante.Sku,
            dto.Cantidad,
            dto.Motivo,
            dto.ReponerSinCobro,
            movimiento.CreatedAt);
    }

    public async Task<ResultadoPaginado<MermaDto>> ObtenerMermasAsync(
        PaginacionRequest paginacion,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken cancellationToken = default)
    {
        var pagina = await _inventario.ObtenerMermasPaginadoAsync(paginacion, desde, hasta, cancellationToken);
        var items = pagina.Items.Select(m => new MermaDto(
            m.Id,
            m.MovimientoId,
            m.Movimiento.VarianteId,
            m.Movimiento.Variante?.Sku ?? string.Empty,
            Math.Abs(m.Movimiento.Cantidad),
            m.Motivo,
            m.Repuesto,
            m.CreatedAt)).ToList();

        return ResultadoPaginado<MermaDto>.Crear(items, pagina.Page, pagina.PageSize, pagina.TotalItems);
    }

    public async Task<MovimientoKardexDto> RegistrarAjusteAsync(
        AjusteInventarioDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto.Cantidad == 0)
        {
            throw new ReglaNegocioException("El ajuste debe ser distinto de cero.");
        }

        var variante = await _variantes.GetByIdAsync(dto.VarianteId, cancellationToken)
            ?? throw new NoEncontradoException($"No existe la variante {dto.VarianteId}.");

        await AplicarStockAsync(dto.VarianteId, dto.Cantidad, cancellationToken);

        var movimiento = new MovimientoInventario
        {
            VarianteId = dto.VarianteId,
            Tipo = TipoMovimientoInventario.Ajuste,
            Cantidad = dto.Cantidad,
            ReferenciaTipo = "ajuste",
            Motivo = dto.Motivo
        };

        await _inventario.AgregarMovimientoAsync(movimiento, cancellationToken);
        await _inventario.SaveChangesAsync(cancellationToken);

        movimiento.Variante = variante;
        return MapearMovimiento(movimiento);
    }

    public async Task<ReporteMermaDto> ObtenerReporteMermasAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        var mermas = await _inventario.ObtenerMermasParaReporteAsync(desde, hasta, cancellationToken);

        var porMotivo = mermas
            .GroupBy(m => m.Motivo)
            .Select(g => new ReporteMermaPorMotivoDto(
                g.Key,
                g.Count(),
                g.Sum(m => Math.Abs(m.Movimiento.Cantidad))))
            .ToList();

        return new ReporteMermaDto(
            desde,
            hasta,
            mermas.Count,
            porMotivo.Sum(m => m.Unidades),
            porMotivo);
    }

    private async Task AplicarStockAsync(Guid varianteId, decimal delta, CancellationToken cancellationToken)
    {
        var stock = await _inventario.ObtenerStockAsync(varianteId, cancellationToken);

        if (stock is null)
        {
            stock = new StockProducto { VarianteId = varianteId };
            await _inventario.AgregarStockAsync(stock, cancellationToken);
        }

        try
        {
            stock.AplicarMovimiento(delta);
        }
        catch (InvalidOperationException ex)
        {
            throw new ReglaNegocioException(ex.Message);
        }
    }

    private static StockDto MapearStock(StockProducto s)
        => new(
            s.VarianteId,
            s.Variante?.Sku ?? string.Empty,
            s.Variante?.Producto?.Nombre ?? string.Empty,
            s.Variante?.Nombre ?? string.Empty,
            s.Cantidad,
            s.CantidadReservada,
            s.StockMinimo,
            s.StockMaximo,
            s.BajoMinimo);

    private static MovimientoKardexDto MapearMovimiento(MovimientoInventario m)
        => new(
            m.Id,
            m.VarianteId,
            m.Variante?.Sku ?? string.Empty,
            m.Tipo,
            m.Cantidad,
            m.CostoUnitario,
            m.ReferenciaTipo,
            m.ReferenciaId,
            m.Motivo,
            m.CreatedAt);
}
