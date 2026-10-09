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

    public async Task<IReadOnlyList<KardexInsumo>> DesglosarInsumosAsync(
        Guid varianteId,
        decimal cantidad,
        CancellationToken cancellationToken = default)
    {
        var variante = await _inventario.ObtenerVarianteConRecetasAsync(varianteId, cancellationToken);
        if (variante is null)
        {
            return [new KardexInsumo(varianteId, cantidad)];
        }

        // Los productos de Cocina se venden sin control de inventario: no llevan
        // stock ni receta; se sirven y ya. No generan movimientos de kardex.
        if (variante.Producto?.AreaDestino == AreaDestino.Cocina)
        {
            return [];
        }

        if (variante.RecetasVendidas.Count == 0)
        {
            return [new KardexInsumo(varianteId, cantidad)];
        }

        return variante.RecetasVendidas
            .Select(r => new KardexInsumo(r.VarianteInsumoId, cantidad * r.Cantidad))
            .ToList();
    }

    public async Task VerificarStockAsync(
        IReadOnlyList<KardexInsumo> insumos,
        CancellationToken cancellationToken = default)
    {
        if (insumos.Count == 0)
        {
            return;
        }

        var ids = insumos.Select(i => i.VarianteId).Distinct().ToList();
        var porVariante = (await _inventario.ObtenerStockDeVariantesAsync(ids, cancellationToken))
            .ToDictionary(s => s.VarianteId);

        var agrupados = insumos
            .GroupBy(i => i.VarianteId)
            .Select(g => new KardexInsumo(g.Key, g.Sum(i => i.Cantidad)))
            .ToList();

        foreach (var insumo in agrupados)
        {
            var disponible = porVariante.GetValueOrDefault(insumo.VarianteId)?.Cantidad ?? 0m;
            if (disponible >= insumo.Cantidad)
            {
                continue;
            }

            var variante = await _inventario.ObtenerVarianteConRecetasAsync(insumo.VarianteId, cancellationToken);
            var nombre = variante?.Producto?.Nombre;
            if (string.IsNullOrWhiteSpace(nombre)) nombre = variante?.Nombre;
            if (string.IsNullOrWhiteSpace(nombre)) nombre = $"variante {insumo.VarianteId}";

            throw new ReglaNegocioException(
                $"Stock insuficiente para '{nombre}': se requieren {insumo.Cantidad} y hay {disponible}.");
        }
    }
}
