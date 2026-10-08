using Licoreria.Application.Common;
using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Services;

public sealed class ServicioInventario : IServicioInventario
{
    private readonly IInventarioRepository _inventario;
    private readonly IRepository<ProductoVariante> _variantes;
    private readonly IRepository<Lote> _lotes;
    private readonly IContextoUsuario _contextoUsuario;
    private readonly IRelojSistema _reloj;
    private readonly IServicioAuditoria _auditoria;

    public ServicioInventario(
        IInventarioRepository inventario,
        IRepository<ProductoVariante> variantes,
        IRepository<Lote> lotes,
        IContextoUsuario contextoUsuario,
        IRelojSistema reloj,
        IServicioAuditoria auditoria)
    {
        _inventario = inventario;
        _variantes = variantes;
        _lotes = lotes;
        _contextoUsuario = contextoUsuario;
        _reloj = reloj;
        _auditoria = auditoria;
    }

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

        await _auditoria.RegistrarAsync("registrar", "merma", movimiento.Id, new { dto.VarianteId, dto.Cantidad, Motivo = dto.Motivo.ToString(), dto.ReponerSinCobro }, cancellationToken);

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

    // ================= Lotes =================

    public async Task<IReadOnlyList<LoteDto>> ObtenerLotesAsync(Guid? varianteId = null, CancellationToken cancellationToken = default)
    {
        var lotes = await _lotes.FindAsync(
            l => !l.IsDeleted && (varianteId == null || l.VarianteId == varianteId),
            cancellationToken);

        var variantes = (await _variantes.GetAllAsync(cancellationToken)).ToDictionary(v => v.Id, v => v.Sku);

        return lotes
            .OrderBy(l => l.FechaVencimiento)
            .Select(l => new LoteDto(l.Id, l.VarianteId, variantes.GetValueOrDefault(l.VarianteId, string.Empty), l.Codigo, l.FechaVencimiento, l.Cantidad, l.Activo))
            .ToList();
    }

    public async Task<LoteDto> CrearLoteAsync(LoteCrearDto dto, CancellationToken cancellationToken = default)
    {
        var variante = await _variantes.GetByIdAsync(dto.VarianteId, cancellationToken)
            ?? throw new NoEncontradoException($"No existe la variante {dto.VarianteId}.");

        if (dto.Cantidad < 0)
        {
            throw new ReglaNegocioException("La cantidad del lote no puede ser negativa.");
        }

        var lote = new Lote
        {
            VarianteId = variante.Id,
            Codigo = dto.Codigo,
            Cantidad = dto.Cantidad,
            FechaVencimiento = dto.FechaVencimiento,
            Activo = true
        };

        await _lotes.AddAsync(lote, cancellationToken);
        await _lotes.SaveChangesAsync(cancellationToken);
        return new LoteDto(lote.Id, lote.VarianteId, variante.Sku, lote.Codigo, lote.FechaVencimiento, lote.Cantidad, lote.Activo);
    }

    public async Task<LoteDto?> EditarLoteAsync(LoteEditarDto dto, CancellationToken cancellationToken = default)
    {
        var lote = await _lotes.GetByIdAsync(dto.Id, cancellationToken);
        if (lote is null || lote.IsDeleted)
        {
            return null;
        }

        lote.Codigo = dto.Codigo;
        lote.Cantidad = dto.Cantidad;
        lote.FechaVencimiento = dto.FechaVencimiento;
        lote.Activo = dto.Activo;
        _lotes.Update(lote);
        await _lotes.SaveChangesAsync(cancellationToken);

        var variante = await _variantes.GetByIdAsync(lote.VarianteId, cancellationToken);
        return new LoteDto(lote.Id, lote.VarianteId, variante?.Sku ?? string.Empty, lote.Codigo, lote.FechaVencimiento, lote.Cantidad, lote.Activo);
    }

    public async Task<bool> EliminarLoteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var lote = await _lotes.GetByIdAsync(id, cancellationToken);
        if (lote is null || lote.IsDeleted)
        {
            return false;
        }

        lote.EliminarLogico();
        _lotes.Update(lote);
        await _lotes.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ================= Tomas físicas =================

    public async Task<ResultadoPaginado<TomaFisicaDto>> ObtenerTomasFisicasAsync(PaginacionRequest paginacion, CancellationToken cancellationToken = default)
    {
        var pagina = await _inventario.ObtenerTomasPaginadoAsync(paginacion, cancellationToken);
        var items = pagina.Items.Select(MapearToma).ToList();
        return ResultadoPaginado<TomaFisicaDto>.Crear(items, pagina.Page, pagina.PageSize, pagina.TotalItems);
    }

    public async Task<TomaFisicaDto?> ObtenerTomaFisicaAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var toma = await _inventario.ObtenerTomaConDetalleAsync(id, cancellationToken);
        return toma is null ? null : MapearToma(toma);
    }

    public async Task<TomaFisicaDto> RegistrarTomaFisicaAsync(RegistrarTomaFisicaDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Detalles.Count == 0)
        {
            throw new ReglaNegocioException("La toma física debe tener al menos una línea.");
        }

        var usuarioId = _contextoUsuario.UsuarioId
            ?? throw new ReglaNegocioException("No se pudo identificar al usuario que registra la toma.");

        var toma = new TomaFisica
        {
            UsuarioId = usuarioId,
            Fecha = _reloj.UtcNow,
            Estado = EstadoTomaFisica.Cerrada,
            Observaciones = dto.Observaciones
        };

        foreach (var linea in dto.Detalles)
        {
            var variante = await _variantes.GetByIdAsync(linea.VarianteId, cancellationToken)
                ?? throw new NoEncontradoException($"No existe la variante {linea.VarianteId}.");

            var stock = await _inventario.ObtenerStockAsync(linea.VarianteId, cancellationToken);
            var cantidadSistema = stock?.Cantidad ?? 0m;
            var diferencia = linea.CantidadContada - cantidadSistema;

            toma.Detalles.Add(new TomaFisicaDetalle
            {
                VarianteId = variante.Id,
                CantidadSistema = cantidadSistema,
                CantidadContada = linea.CantidadContada
            });

            if (diferencia != 0)
            {
                await AplicarStockAsync(linea.VarianteId, diferencia, cancellationToken);
                await _inventario.AgregarMovimientoAsync(new MovimientoInventario
                {
                    VarianteId = variante.Id,
                    Tipo = TipoMovimientoInventario.Ajuste,
                    Cantidad = diferencia,
                    ReferenciaTipo = "toma-fisica",
                    ReferenciaId = toma.Id,
                    Motivo = "Ajuste por toma física"
                }, cancellationToken);
            }
        }

        await _inventario.AgregarTomaAsync(toma, cancellationToken);
        await _inventario.SaveChangesAsync(cancellationToken);

        var creada = await _inventario.ObtenerTomaConDetalleAsync(toma.Id, cancellationToken);
        await _auditoria.RegistrarAsync("registrar", "toma-fisica", toma.Id, new { Lineas = toma.Detalles.Count }, cancellationToken);
        return MapearToma(creada!);
    }

    private static TomaFisicaDto MapearToma(TomaFisica t)
        => new(
            t.Id,
            t.Fecha,
            t.Estado,
            t.Observaciones,
            t.Detalles.Select(d => new TomaFisicaDetalleDto(
                d.VarianteId,
                d.Variante?.Sku ?? string.Empty,
                d.CantidadSistema,
                d.CantidadContada,
                d.Diferencia)).ToList());

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
