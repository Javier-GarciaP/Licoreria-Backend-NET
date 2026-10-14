using Licoreria.Application.Interfaces;
using Licoreria.Domain.Common;
using Licoreria.Domain.Entities;
using Licoreria.Domain.Enums;

namespace Licoreria.Application.Services;

public sealed class ServicioKardex : IServicioKardex
{
    private readonly IInventarioRepository _inventario;
    private readonly IPorcionRepository _porciones;
    private readonly IRelojSistema _reloj;

    // Una variante es porción mientras tenga orígenes; se consulta una vez por solicitud.
    private readonly Dictionary<Guid, bool> _esPorcion = new();

    public ServicioKardex(IInventarioRepository inventario, IPorcionRepository porciones, IRelojSistema reloj)
    {
        _inventario = inventario;
        _porciones = porciones;
        _reloj = reloj;
    }

    private async Task<bool> EsPorcionAsync(Guid varianteId, CancellationToken cancellationToken)
    {
        if (!_esPorcion.TryGetValue(varianteId, out var esPorcion))
        {
            esPorcion = await _porciones.TieneOrigenesAsync(varianteId, cancellationToken);
            _esPorcion[varianteId] = esPorcion;
        }

        return esPorcion;
    }

    /// <summary>Separa los insumos agrupados en porciones (envases abiertos) y presentaciones (existencia propia).</summary>
    private async Task<(List<KardexInsumo> Porciones, List<KardexInsumo> Presentaciones)> SepararAsync(
        IReadOnlyList<KardexInsumo> insumos,
        CancellationToken cancellationToken)
    {
        var porciones = new List<KardexInsumo>();
        var presentaciones = new List<KardexInsumo>();
        foreach (var insumo in Agrupar(insumos))
        {
            (await EsPorcionAsync(insumo.VarianteId, cancellationToken) ? porciones : presentaciones).Add(insumo);
        }

        return (porciones, presentaciones);
    }

    public async Task<MovimientoInventario> AplicarAsync(
        Guid varianteId,
        TipoMovimientoInventario tipo,
        decimal cantidad,
        string? referenciaTipo = null,
        Guid? referenciaId = null,
        string? motivo = null,
        CancellationToken cancellationToken = default)
    {
        if (await EsPorcionAsync(varianteId, cancellationToken))
        {
            return await AplicarAPorcionAsync(varianteId, tipo, cantidad, referenciaTipo, referenciaId, motivo, cancellationToken);
        }

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

        var (porciones, presentaciones) = await SepararAsync(insumos, cancellationToken);

        if (presentaciones.Count > 0)
        {
            var ids = presentaciones.Select(i => i.VarianteId).ToList();
            var porVariante = (await _inventario.ObtenerStockDeVariantesAsync(ids, cancellationToken))
                .ToDictionary(s => s.VarianteId);

            foreach (var insumo in presentaciones)
            {
                var disponible = porVariante.GetValueOrDefault(insumo.VarianteId)?.Disponible ?? 0m;
                if (disponible >= insumo.Cantidad)
                {
                    continue;
                }

                throw new ExistenciaAgotadaException(
                    await NombreDeVarianteAsync(insumo.VarianteId, cancellationToken),
                    insumo.Cantidad,
                    disponible);
            }
        }

        foreach (var insumo in porciones)
        {
            var pedidas = AEntero(insumo.Cantidad);
            var disponibles = await PorcionesDisponiblesAsync(insumo.VarianteId, cancellationToken);
            if (disponibles < pedidas)
            {
                throw await ErrorDePorcionesAsync(insumo.VarianteId, pedidas, disponibles, cancellationToken);
            }
        }
    }

    public async Task ReservarAsync(
        IReadOnlyList<KardexInsumo> insumos,
        CancellationToken cancellationToken = default)
    {
        var (porciones, presentaciones) = await SepararAsync(insumos, cancellationToken);

        // Orden estable por variante: dos transacciones que reservan los mismos insumos
        // toman las filas en el mismo orden y no se bloquean entre sí.
        foreach (var insumo in presentaciones.OrderBy(i => i.VarianteId))
        {
            var stock = await _inventario.ObtenerStockAsync(insumo.VarianteId, cancellationToken);
            var disponible = stock?.Disponible ?? 0m;

            if (stock is null || disponible < insumo.Cantidad)
            {
                throw new ExistenciaAgotadaException(
                    await NombreDeVarianteAsync(insumo.VarianteId, cancellationToken),
                    insumo.Cantidad,
                    disponible);
            }

            stock.Reservar(insumo.Cantidad);
        }

        foreach (var insumo in porciones.OrderBy(i => i.VarianteId))
        {
            var pedidas = AEntero(insumo.Cantidad);
            var stock = await ObtenerOCrearStockPorcionAsync(insumo.VarianteId, cancellationToken);
            var abiertas = (await _porciones.ObtenerAbiertosAsync(insumo.VarianteId, cancellationToken)).Sum(e => e.PorcionesRestantes);
            var disponibles = Math.Max(0, abiertas - stock.PorcionesReservadas);

            if (disponibles < pedidas)
            {
                throw await ErrorDePorcionesAsync(insumo.VarianteId, pedidas, disponibles, cancellationToken);
            }

            stock.Reservar(pedidas);
        }
    }

    public async Task LiberarReservaAsync(
        IReadOnlyList<KardexInsumo> insumos,
        CancellationToken cancellationToken = default)
    {
        var (porciones, presentaciones) = await SepararAsync(insumos, cancellationToken);

        foreach (var insumo in presentaciones.OrderBy(i => i.VarianteId))
        {
            var stock = await _inventario.ObtenerStockAsync(insumo.VarianteId, cancellationToken);
            if (stock is null)
            {
                continue;
            }

            var liberar = Math.Min(insumo.Cantidad, stock.CantidadReservada);
            if (liberar > 0)
            {
                stock.LiberarReserva(liberar);
            }
        }

        foreach (var insumo in porciones.OrderBy(i => i.VarianteId))
        {
            var stock = await _porciones.ObtenerStockPorcionAsync(insumo.VarianteId, cancellationToken);
            if (stock is null)
            {
                continue;
            }

            var liberar = Math.Min((int)Math.Floor(insumo.Cantidad), stock.PorcionesReservadas);
            if (liberar > 0)
            {
                stock.Liberar(liberar);
            }
        }
    }

    public async Task<MovimientoInventario> ConsumirReservaAsync(
        Guid varianteId,
        TipoMovimientoInventario tipo,
        decimal cantidad,
        string? referenciaTipo = null,
        Guid? referenciaId = null,
        string? motivo = null,
        CancellationToken cancellationToken = default)
    {
        if (await EsPorcionAsync(varianteId, cancellationToken))
        {
            return await ConsumirPorcionesAsync(
                varianteId, tipo, AEntero(cantidad), consumeReserva: true, referenciaTipo, referenciaId, motivo, cancellationToken);
        }

        var stock = await _inventario.ObtenerStockAsync(varianteId, cancellationToken)
            ?? throw new ExistenciaAgotadaException(
                await NombreDeVarianteAsync(varianteId, cancellationToken), cantidad, 0m);

        try
        {
            var reservado = Math.Min(cantidad, stock.CantidadReservada);
            if (reservado > 0)
            {
                stock.ConsumirReserva(reservado);
            }

            var resto = cantidad - reservado;
            if (resto > 0)
            {
                stock.AplicarMovimiento(-resto);
            }
        }
        catch (InvalidOperationException ex)
        {
            throw new ReglaNegocioException(ex.Message);
        }

        var movimiento = new MovimientoInventario
        {
            VarianteId = varianteId,
            Tipo = tipo,
            Cantidad = -cantidad,
            ReferenciaTipo = referenciaTipo,
            ReferenciaId = referenciaId,
            Motivo = motivo
        };

        await _inventario.AgregarMovimientoAsync(movimiento, cancellationToken);
        return movimiento;
    }

    // ===================== Porciones (envases abiertos) =====================

    /// <summary>Las porciones se piden y se sirven en unidades enteras.</summary>
    private static int AEntero(decimal cantidad)
    {
        if (cantidad != Math.Truncate(cantidad) || cantidad < 1m)
        {
            throw new ReglaNegocioException("Las porciones deben pedirse en cantidades enteras mayores que cero.");
        }

        return (int)cantidad;
    }

    private async Task<StockPorcion> ObtenerOCrearStockPorcionAsync(Guid varianteId, CancellationToken cancellationToken)
    {
        var stock = await _porciones.ObtenerStockPorcionAsync(varianteId, cancellationToken);
        if (stock is null)
        {
            stock = new StockPorcion { VarianteId = varianteId };
            await _porciones.AgregarStockPorcionAsync(stock, cancellationToken);
        }

        return stock;
    }

    /// <summary>Porciones abiertas menos las reservadas por comandas pendientes.</summary>
    private async Task<int> PorcionesDisponiblesAsync(Guid varianteId, CancellationToken cancellationToken)
    {
        var abiertas = (await _porciones.ObtenerAbiertosAsync(varianteId, cancellationToken)).Sum(e => e.PorcionesRestantes);
        var reservadas = (await _porciones.ObtenerStockPorcionAsync(varianteId, cancellationToken))?.PorcionesReservadas ?? 0;
        return Math.Max(0, abiertas - reservadas);
    }

    /// <summary>
    /// «Sin abrir» si hay presentaciones cerradas que se pueden destapar para esta porción;
    /// «agotada» si no hay nada que servir.
    /// </summary>
    private async Task<ExistenciaAgotadaException> ErrorDePorcionesAsync(
        Guid porcionId,
        int pedidas,
        int disponibles,
        CancellationToken cancellationToken)
    {
        var cerradas = 0;
        foreach (var origen in await _porciones.ObtenerOrigenesAsync(porcionId, cancellationToken))
        {
            var existencia = await _inventario.ObtenerStockAsync(origen.PresentacionVarianteId, cancellationToken);
            cerradas += existencia is null ? 0 : (int)Math.Max(0m, Math.Floor(existencia.Disponible));
        }

        var nombre = await NombreDePorcionAsync(porcionId, cancellationToken);
        return cerradas > 0
            ? new PorcionesSinAbrirException(nombre, pedidas, disponibles, cerradas)
            : new ExistenciaAgotadaException(nombre, pedidas, disponibles);
    }

    private async Task<string> NombreDePorcionAsync(Guid porcionId, CancellationToken cancellationToken)
    {
        var variante = await _inventario.ObtenerVarianteConRecetasAsync(porcionId, cancellationToken);
        var producto = variante?.Producto?.Nombre;
        var nombre = variante?.Nombre;
        if (string.IsNullOrWhiteSpace(nombre)) return $"porción {porcionId}";
        return string.IsNullOrWhiteSpace(producto) ? nombre : $"{producto} · {nombre}";
    }

    /// <summary>Movimientos directos sobre una porción: salida (venta, cortesía) o reintegro.</summary>
    private async Task<MovimientoInventario> AplicarAPorcionAsync(
        Guid varianteId,
        TipoMovimientoInventario tipo,
        decimal cantidad,
        string? referenciaTipo,
        Guid? referenciaId,
        string? motivo,
        CancellationToken cancellationToken)
    {
        if (tipo == TipoMovimientoInventario.Apertura)
        {
            throw new ReglaNegocioException("Una porción no se destapa: se destapa una de sus presentaciones de origen.");
        }

        // Lo ya servido no vuelve a un envase: un reintegro no modifica ninguna porción.
        if (cantidad >= 0)
        {
            return new MovimientoInventario
            {
                VarianteId = varianteId,
                Tipo = tipo,
                Cantidad = 0m,
                ReferenciaTipo = referenciaTipo,
                ReferenciaId = referenciaId,
                Motivo = motivo
            };
        }

        return await ConsumirPorcionesAsync(
            varianteId, tipo, AEntero(-cantidad), consumeReserva: false, referenciaTipo, referenciaId, motivo, cancellationToken);
    }

    /// <summary>
    /// Sirve porciones del envase abierto más antiguo, pasando al siguiente cuando se agota. Si
    /// <paramref name="consumeReserva"/>, primero libera lo reservado por la propia comanda.
    /// </summary>
    private async Task<MovimientoInventario> ConsumirPorcionesAsync(
        Guid varianteId,
        TipoMovimientoInventario tipo,
        int porciones,
        bool consumeReserva,
        string? referenciaTipo,
        Guid? referenciaId,
        string? motivo,
        CancellationToken cancellationToken)
    {
        var abiertos = await _porciones.ObtenerAbiertosAsync(varianteId, cancellationToken);
        var stock = await _porciones.ObtenerStockPorcionAsync(varianteId, cancellationToken);

        if (consumeReserva && stock is not null)
        {
            var propias = Math.Min(porciones, stock.PorcionesReservadas);
            if (propias > 0)
            {
                stock.Liberar(propias);
            }
        }

        // Lo reservado por otras comandas no se puede tocar.
        var abiertas = abiertos.Sum(e => e.PorcionesRestantes);
        var ajenas = stock?.PorcionesReservadas ?? 0;
        if (abiertas - ajenas < porciones)
        {
            throw await ErrorDePorcionesAsync(varianteId, porciones, Math.Max(0, abiertas - ajenas), cancellationToken);
        }

        var ahora = _reloj.UtcNow;
        var pendiente = porciones;
        foreach (var envase in abiertos)
        {
            if (pendiente == 0)
            {
                break;
            }

            var tomar = Math.Min(envase.PorcionesRestantes, pendiente);
            envase.Consumir(tomar, ahora);
            pendiente -= tomar;
        }

        var movimiento = new MovimientoInventario
        {
            VarianteId = varianteId,
            Tipo = tipo,
            Cantidad = -porciones,
            ReferenciaTipo = referenciaTipo,
            ReferenciaId = referenciaId,
            Motivo = motivo
        };

        await _inventario.AgregarMovimientoAsync(movimiento, cancellationToken);
        return movimiento;
    }

    public async Task LiberarReservasAsync(
        IEnumerable<ComandaDetalle> detalles,
        CancellationToken cancellationToken = default)
    {
        foreach (var detalle in detalles.Where(d => d.Reservado).ToList())
        {
            var insumos = await DesglosarInsumosAsync(detalle.VarianteId, detalle.Cantidad, cancellationToken);
            await LiberarReservaAsync(insumos, cancellationToken);
            detalle.Reservado = false;
        }
    }

    private static List<KardexInsumo> Agrupar(IReadOnlyList<KardexInsumo> insumos)
        => insumos
            .GroupBy(i => i.VarianteId)
            .Select(g => new KardexInsumo(g.Key, g.Sum(i => i.Cantidad)))
            .ToList();

    private async Task<string> NombreDeVarianteAsync(Guid varianteId, CancellationToken cancellationToken)
    {
        var variante = await _inventario.ObtenerVarianteConRecetasAsync(varianteId, cancellationToken);
        var nombre = variante?.Producto?.Nombre;
        if (string.IsNullOrWhiteSpace(nombre)) nombre = variante?.Nombre;
        if (string.IsNullOrWhiteSpace(nombre)) nombre = $"variante {varianteId}";
        return nombre;
    }
}
