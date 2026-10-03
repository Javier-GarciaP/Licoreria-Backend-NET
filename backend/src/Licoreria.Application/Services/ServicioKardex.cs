using Licoreria.Application.Interfaces;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Services;

public sealed class ServicioKardex : IServicioKardex
{
    private readonly IInventarioRepository _inventario;

    public ServicioKardex(IInventarioRepository inventario) => _inventario = inventario;

    public async Task<MovimientoInventario> AplicarAsync(
        Guid varianteId,
        TipoMovimientoInventario tipo,
        decimal cantidad,
        string? referenciaTipo = null,
        Guid? referenciaId = null,
        string? motivo = null,
        CancellationToken cancellationToken = default)
    {
        var stock = await _inventario.ObtenerStockAsync(varianteId, cancellationToken);

        if (stock is null)
        {
            stock = new StockProducto { VarianteId = varianteId };
            await _inventario.AgregarStockAsync(stock, cancellationToken);
        }

        try
        {
            stock.AplicarMovimiento(cantidad);
        }
        catch (InvalidOperationException ex)
        {
            throw new ReglaNegocioException(ex.Message);
        }

        var movimiento = new MovimientoInventario
        {
            VarianteId = varianteId,
            Tipo = tipo,
            Cantidad = cantidad,
            ReferenciaTipo = referenciaTipo,
            ReferenciaId = referenciaId,
            Motivo = motivo
        };

        await _inventario.AgregarMovimientoAsync(movimiento, cancellationToken);
        return movimiento;
    }
}
