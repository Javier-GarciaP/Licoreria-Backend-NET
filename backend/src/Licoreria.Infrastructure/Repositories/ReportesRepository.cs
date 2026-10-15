using Licoreria.Application.Dtos;
using Licoreria.Application.Interfaces;
using Licoreria.Domain.Enums;
using Licoreria.Domain.Services;
using Licoreria.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Licoreria.Infrastructure.Repositories;

public class ReportesRepository : IReportesRepository
{
    private readonly LicoreriaDbContext _context;

    public ReportesRepository(LicoreriaDbContext context) => _context = context;

    public async Task<DashboardDto> ObtenerDashboardAsync(
        DateTime inicioDia,
        DateTime inicioMes,
        DateTime ahora,
        CancellationToken cancellationToken = default)
    {
        var ventasHoy = await _context.Ventas
            .Where(v => !v.IsDeleted && v.Estado == EstadoVenta.Completada && v.Fecha >= inicioDia)
            .GroupBy(v => 1)
            .Select(g => new { Total = g.Sum(v => v.TotalUSD), Cantidad = g.Count() })
            .FirstOrDefaultAsync(cancellationToken);

        var ventasMes = await _context.Ventas
            .Where(v => !v.IsDeleted && v.Estado == EstadoVenta.Completada && v.Fecha >= inicioMes)
            .GroupBy(v => 1)
            .Select(g => new { Total = g.Sum(v => v.TotalUSD), Cantidad = g.Count() })
            .FirstOrDefaultAsync(cancellationToken);

        var mermasMes = await _context.Mermas
            .Where(m => !m.IsDeleted && m.CreatedAt >= inicioMes)
            .GroupBy(m => 1)
            .Select(g => new { Cantidad = g.Count(), Unidades = g.Sum(m => m.Movimiento.Cantidad) })
            .FirstOrDefaultAsync(cancellationToken);

        var cuentasAbiertas = await _context.Cuentas
            .CountAsync(c => !c.IsDeleted && c.Estado != EstadoCuenta.Cerrada, cancellationToken);

        var stockBajo = await _context.StockProductos
            .CountAsync(s => !s.IsDeleted && s.Cantidad <= s.StockMinimo, cancellationToken);

        var limiteReservas = ahora.AddDays(7);
        var reservasProximas = await _context.Reservas
            .CountAsync(r => !r.IsDeleted
                && r.FechaHora >= ahora
                && r.FechaHora <= limiteReservas
                && (r.Estado == EstadoReserva.Pendiente || r.Estado == EstadoReserva.Confirmada),
                cancellationToken);

        var caja = await _context.SesionesCaja
            .Where(s => !s.IsDeleted && s.Estado == EstadoSesionCaja.Abierta)
            .Select(s => (decimal?)s.FondoInicial)
            .FirstOrDefaultAsync(cancellationToken);

        var clientes = await _context.Clientes.CountAsync(c => !c.IsDeleted, cancellationToken);
        var productosActivos = await _context.Productos.CountAsync(p => !p.IsDeleted && p.Activo, cancellationToken);

        var totalMes = ventasMes?.Total ?? 0m;
        var cantidadMes = ventasMes?.Cantidad ?? 0;

        return new DashboardDto(
            ventasHoy?.Total ?? 0m,
            ventasHoy?.Cantidad ?? 0,
            totalMes,
            cantidadMes,
            cantidadMes == 0 ? 0m : Math.Round(totalMes / cantidadMes, 2),
            mermasMes?.Cantidad ?? 0,
            Math.Abs(mermasMes?.Unidades ?? 0m),
            cuentasAbiertas,
            stockBajo,
            reservasProximas,
            caja is not null,
            caja ?? 0m,
            clientes,
            productosActivos,
            ahora);
    }

    public async Task<ReportePropinasDto> ObtenerPropinasAsync(
        DateTime desde,
        DateTime hasta,
        Guid? usuarioId = null,
        CancellationToken cancellationToken = default)
    {
        var filas = await _context.Pagos
            .AsNoTracking()
            .Where(p => !p.IsDeleted
                && p.Propina > 0
                && !p.Venta.IsDeleted
                && p.Venta.Estado == EstadoVenta.Completada
                && p.Venta.Fecha >= desde
                && p.Venta.Fecha <= hasta
                && (usuarioId == null || p.Venta.UsuarioId == usuarioId))
            .Select(p => new
            {
                p.Venta.UsuarioId,
                p.Venta.Usuario.NombreCompleto,
                p.VentaId,
                p.Propina
            })
            .ToListAsync(cancellationToken);

        var porUsuario = filas
            .GroupBy(f => new { f.UsuarioId, f.NombreCompleto })
            .Select(g => new PropinaPorUsuarioDto(
                g.Key.UsuarioId,
                g.Key.NombreCompleto,
                g.Select(x => x.VentaId).Distinct().Count(),
                g.Sum(x => x.Propina)))
            .OrderByDescending(p => p.TotalPropinaUSD)
            .ToList();

        return new ReportePropinasDto(desde, hasta, porUsuario.Sum(p => p.TotalPropinaUSD), porUsuario);
    }

    public async Task<ReporteVentasDto> ObtenerVentasAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        var ventas = await _context.Ventas
            .AsNoTracking()
            .Where(v => !v.IsDeleted
                && v.Estado == EstadoVenta.Completada
                && v.Fecha >= desde
                && v.Fecha <= hasta)
            .Select(v => new
            {
                v.Fecha,
                v.TotalUSD,
                v.TotalBS,
                v.UsuarioId,
                v.Usuario.NombreCompleto
            })
            .ToListAsync(cancellationToken);

        var totalUsd = ventas.Sum(v => v.TotalUSD);
        var totalBs = ventas.Sum(v => v.TotalBS);

        var porDia = ventas
            .GroupBy(v => v.Fecha.Date)
            .Select(g => new VentaPorDiaDto(g.Key, g.Sum(x => x.TotalUSD), g.Count()))
            .OrderBy(v => v.Fecha)
            .ToList();

        var porUsuario = ventas
            .GroupBy(v => new { v.UsuarioId, v.NombreCompleto })
            .Select(g => new VentaPorUsuarioDto(g.Key.UsuarioId, g.Key.NombreCompleto, g.Sum(x => x.TotalUSD), g.Count()))
            .OrderByDescending(v => v.TotalUSD)
            .ToList();

        var pagos = await _context.Pagos
            .AsNoTracking()
            .Where(p => !p.IsDeleted
                && !p.Venta.IsDeleted
                && p.Venta.Estado == EstadoVenta.Completada
                && p.Venta.Fecha >= desde
                && p.Venta.Fecha <= hasta)
            .Select(p => new
            {
                p.MetodoPagoId,
                p.MetodoPago.Nombre,
                p.Moneda,
                p.Monto
            })
            .ToListAsync(cancellationToken);

        var porMetodo = pagos
            .GroupBy(p => new { p.MetodoPagoId, p.Nombre, p.Moneda })
            .Select(g => new VentaPorMetodoPagoDto(
                g.Key.MetodoPagoId,
                g.Key.Nombre,
                g.Key.Moneda,
                g.Sum(x => x.Monto),
                g.Count()))
            .OrderByDescending(p => p.Monto)
            .ToList();

        return new ReporteVentasDto(
            desde,
            hasta,
            totalUsd,
            totalBs,
            ventas.Count,
            ventas.Count == 0 ? 0m : Math.Round(totalUsd / ventas.Count, 2),
            porDia,
            porUsuario,
            porMetodo);
    }

    public async Task<InventarioValorizadoDto> ObtenerInventarioValorizadoAsync(
        CancellationToken cancellationToken = default)
    {
        var filas = await (
            from s in _context.StockProductos.AsNoTracking()
            join v in _context.ProductoVariantes.AsNoTracking() on s.VarianteId equals v.Id
            where !s.IsDeleted && !v.IsDeleted
            select new
            {
                VarianteId = v.Id,
                v.Sku,
                ProductoNombre = v.Producto.Nombre,
                VarianteNombre = v.Nombre,
                s.Cantidad,
                s.StockMinimo,
                v.PrecioCompraUSD,
                v.PrecioVentaUSD
            })
            .ToListAsync(cancellationToken);

        var items = filas
            .Select(f => new InventarioValorizadoItemDto(
                f.VarianteId,
                f.Sku,
                f.ProductoNombre,
                f.VarianteNombre,
                f.Cantidad,
                f.PrecioCompraUSD,
                Math.Round(f.Cantidad * f.PrecioCompraUSD, 2),
                Math.Round(f.Cantidad * f.PrecioVentaUSD, 2),
                f.Cantidad <= f.StockMinimo))
            .OrderByDescending(i => i.CostoTotalUSD)
            .ToList();

        return new InventarioValorizadoDto(
            items.Count,
            items.Sum(i => i.Cantidad),
            items.Sum(i => i.CostoTotalUSD),
            items.Sum(i => i.ValorVentaUSD),
            items.Count(i => i.BajoMinimo),
            items);
    }

    public async Task<ReporteComprasDto> ObtenerComprasAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        var recepciones = await _context.Recepciones
            .AsNoTracking()
            .Where(r => !r.IsDeleted && r.Fecha >= desde && r.Fecha <= hasta)
            .Select(r => r.TotalUSD)
            .ToListAsync(cancellationToken);

        var totalPorPagar = await _context.CuentasPorPagar
            .AsNoTracking()
            .Where(c => !c.IsDeleted && c.Estado != EstadoCuentaPorPagar.Pagada)
            .SumAsync(c => (decimal?)c.SaldoUSD, cancellationToken) ?? 0m;

        var cuentasPendientes = await _context.CuentasPorPagar
            .CountAsync(c => !c.IsDeleted && c.Estado != EstadoCuentaPorPagar.Pagada, cancellationToken);

        var totalPagado = await _context.PagosProveedor
            .AsNoTracking()
            .Where(p => !p.IsDeleted
                && p.Moneda == Moneda.USD
                && p.CreatedAt >= desde
                && p.CreatedAt <= hasta)
            .SumAsync(p => (decimal?)p.Monto, cancellationToken) ?? 0m;

        return new ReporteComprasDto(
            desde,
            hasta,
            recepciones.Count,
            recepciones.Sum(),
            totalPorPagar,
            totalPagado,
            cuentasPendientes);
    }

    public async Task<ReporteHeatmapDto> ObtenerHeatmapAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        var ventas = await _context.Ventas
            .AsNoTracking()
            .Where(v => !v.IsDeleted
                && v.Estado == EstadoVenta.Completada
                && v.Fecha >= desde
                && v.Fecha <= hasta)
            .Select(v => new { v.Fecha, v.TotalUSD })
            .ToListAsync(cancellationToken);

        var franjas = ventas
            .GroupBy(v => new { Dia = (int)v.Fecha.DayOfWeek, v.Fecha.Hour })
            .Select(g => new HeatmapFranjaDto(
                g.Key.Dia,
                g.Key.Hour,
                g.Sum(x => x.TotalUSD),
                g.Count()))
            .OrderBy(f => f.DiaSemana)
            .ThenBy(f => f.Hora)
            .ToList();

        return new ReporteHeatmapDto(desde, hasta, franjas);
    }

    public async Task<IReadOnlyList<StockSaludCrudoDto>> ObtenerStockSaludAsync(
        CancellationToken cancellationToken = default)
    {
        return await (
            from s in _context.StockProductos.AsNoTracking()
            join v in _context.ProductoVariantes.AsNoTracking() on s.VarianteId equals v.Id
            where !s.IsDeleted && !v.IsDeleted
            select new StockSaludCrudoDto(
                v.Id,
                v.Sku,
                v.Producto.Nombre,
                v.Nombre,
                s.Cantidad,
                s.StockMinimo,
                s.StockMaximo))
            .ToListAsync(cancellationToken);
    }

    public async Task<(decimal VentasUSD, IReadOnlyList<ComparativoMermaDto> Mermas)> ObtenerMermasVsVentasAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        var ventasUsd = await _context.Ventas
            .AsNoTracking()
            .Where(v => !v.IsDeleted
                && v.Estado == EstadoVenta.Completada
                && v.Fecha >= desde
                && v.Fecha <= hasta)
            .SumAsync(v => (decimal?)v.TotalUSD, cancellationToken) ?? 0m;

        var mermas = await _context.Mermas
            .AsNoTracking()
            .Where(m => !m.IsDeleted && m.CreatedAt >= desde && m.CreatedAt <= hasta)
            .Select(m => new
            {
                m.Motivo,
                Cantidad = m.Movimiento.Cantidad,
                Costo = m.Movimiento.CostoUnitario ?? m.Movimiento.Variante.PrecioCompraUSD
            })
            .ToListAsync(cancellationToken);

        var porMotivo = mermas
            .GroupBy(m => m.Motivo)
            .Select(g => new ComparativoMermaDto(
                g.Key,
                g.Count(),
                Math.Abs(g.Sum(x => x.Cantidad)),
                Math.Abs(g.Sum(x => x.Cantidad * x.Costo))))
            .OrderByDescending(m => m.ValorUSD)
            .ToList();

        return (ventasUsd, porMotivo);
    }

    public async Task<IReadOnlyList<ProductoVendidoCrudoDto>> ObtenerProductosVendidosAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        var vendidos = await _context.DetallesVenta
            .AsNoTracking()
            .Where(d => !d.IsDeleted
                && !d.Venta.IsDeleted
                && d.Venta.Estado == EstadoVenta.Completada
                && d.Venta.Fecha >= desde
                && d.Venta.Fecha <= hasta)
            .GroupBy(d => d.VarianteId)
            .Select(g => new
            {
                VarianteId = g.Key,
                Unidades = g.Sum(d => d.Cantidad),
                Ingreso = g.Sum(d => d.EsCortesia
                    ? 0m
                    : Math.Max(0m, d.Cantidad * d.PrecioUnitarioUSD - d.DescuentoUSD))
            })
            .ToListAsync(cancellationToken);

        var existencias = await _context.StockProductos
            .AsNoTracking()
            .Where(s => !s.IsDeleted)
            .GroupBy(s => s.VarianteId)
            .Select(g => new { VarianteId = g.Key, Existencia = g.Sum(s => s.Cantidad) })
            .ToDictionaryAsync(x => x.VarianteId, x => x.Existencia, cancellationToken);

        var ids = vendidos.Select(v => v.VarianteId)
            .Concat(existencias.Where(e => e.Value > 0m).Select(e => e.Key))
            .Distinct()
            .ToList();

        var variantes = await _context.ProductoVariantes
            .AsNoTracking()
            .Where(v => ids.Contains(v.Id))
            .Select(v => new { v.Id, v.Sku, ProductoNombre = v.Producto.Nombre, VarianteNombre = v.Nombre, v.IsDeleted })
            .ToDictionaryAsync(v => v.Id, cancellationToken);

        var ventasPorVariante = vendidos.ToDictionary(v => v.VarianteId);

        return ids
            .Where(variantes.ContainsKey)
            .Where(id => ventasPorVariante.ContainsKey(id) || !variantes[id].IsDeleted)
            .Select(id =>
            {
                var variante = variantes[id];
                ventasPorVariante.TryGetValue(id, out var venta);
                existencias.TryGetValue(id, out var existencia);

                return new ProductoVendidoCrudoDto(
                    id,
                    variante.Sku,
                    variante.ProductoNombre,
                    variante.VarianteNombre,
                    venta?.Unidades ?? 0m,
                    venta?.Ingreso ?? 0m,
                    existencia);
            })
            .ToList();
    }

    public async Task<IReadOnlyList<OcupacionZonaCrudaDto>> ObtenerOcupacionAsync(
        DateTime desde,
        DateTime hasta,
        DateTime ahora,
        CancellationToken cancellationToken = default)
    {
        var mesas = await _context.Mesas
            .AsNoTracking()
            .Where(m => !m.IsDeleted && m.Activa && !m.Zona.IsDeleted)
            .Select(m => new { m.Id, m.ZonaId, ZonaNombre = m.Zona.Nombre })
            .ToListAsync(cancellationToken);

        var mesaPorId = mesas.ToDictionary(m => m.Id);

        var ocupadas = await _context.SesionesMesa
            .AsNoTracking()
            .Where(s => !s.IsDeleted
                && s.MesaId != null
                && s.CerradaEn == null
                && (s.Cuenta == null || s.Cuenta.Estado != EstadoCuenta.Cerrada))
            .Select(s => s.MesaId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        var sesiones = await _context.SesionesMesa
            .AsNoTracking()
            .Where(s => !s.IsDeleted && s.AbiertaEn >= desde && s.AbiertaEn <= hasta)
            .Select(s => new { s.MesaId, s.AbiertaEn, s.CerradaEn })
            .ToListAsync(cancellationToken);

        (Guid? ZonaId, string Zona) ZonaDe(Guid? mesaId)
            => mesaId is { } id && mesaPorId.TryGetValue(id, out var mesa)
                ? (mesa.ZonaId, mesa.ZonaNombre)
                : (null, "Sin zona");

        // Las sesiones sin mesa asignada no tienen zona: se agrupan bajo Guid.Empty ("Sin zona").
        var filas = new Dictionary<Guid, (string Zona, int Activas, int Ocupadas, int Sesiones, int Abiertas, decimal Minutos)>();

        void Acumular(Guid? zonaId, string zona, int activas = 0, int ocupadas = 0, int sesion = 0, int abiertas = 0, decimal minutos = 0m)
        {
            var clave = zonaId ?? Guid.Empty;
            filas.TryGetValue(clave, out var actual);
            filas[clave] = (zona, actual.Activas + activas, actual.Ocupadas + ocupadas, actual.Sesiones + sesion, actual.Abiertas + abiertas, actual.Minutos + minutos);
        }

        foreach (var mesa in mesas)
        {
            Acumular(mesa.ZonaId, mesa.ZonaNombre, activas: 1, ocupadas: ocupadas.Contains(mesa.Id) ? 1 : 0);
        }

        foreach (var sesion in sesiones)
        {
            var (zonaId, zona) = ZonaDe(sesion.MesaId);
            var abierta = sesion.CerradaEn is null;
            var minutos = (decimal)((sesion.CerradaEn ?? ahora) - sesion.AbiertaEn).TotalMinutes;
            Acumular(zonaId, zona, sesion: 1, abiertas: abierta ? 1 : 0, minutos: Math.Max(0m, minutos));
        }

        return filas
            .Select(f => new OcupacionZonaCrudaDto(
                f.Key == Guid.Empty ? null : f.Key, f.Value.Zona, f.Value.Activas, f.Value.Ocupadas, f.Value.Sesiones, f.Value.Abiertas, f.Value.Minutos))
            .OrderBy(f => f.Zona)
            .ToList();
    }

    public async Task<IReadOnlyList<ReservasPorEstadoDto>> ObtenerReservasPorEstadoAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        return await _context.Reservas
            .AsNoTracking()
            .Where(r => !r.IsDeleted && r.FechaHora >= desde && r.FechaHora <= hasta)
            .GroupBy(r => r.Estado)
            .OrderBy(g => g.Key)
            .Select(g => new ReservasPorEstadoDto(g.Key, g.Count()))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TurnoCajaDto>> ObtenerTurnosCerradosAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        return await _context.SesionesCaja
            .AsNoTracking()
            .Where(s => !s.IsDeleted
                && s.Estado == EstadoSesionCaja.Cerrada
                && s.CerradaEn != null
                && s.CerradaEn >= desde
                && s.CerradaEn <= hasta)
            .OrderBy(s => s.CerradaEn)
            .Select(s => new TurnoCajaDto(
                s.Id,
                s.UsuarioId,
                s.Usuario.NombreCompleto,
                s.AbiertaEn,
                s.CerradaEn!.Value,
                s.MontoEsperado,
                s.MontoContado,
                s.Descuadre))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MesoneroCrudoDto>> ObtenerActividadMesonerosAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken = default)
    {
        var grupos = await _context.ComandaDetalles
            .AsNoTracking()
            .Where(d => !d.IsDeleted
                && !d.Comanda.IsDeleted
                && d.Comanda.CreatedAt >= desde
                && d.Comanda.CreatedAt <= hasta)
            .GroupBy(d => d.Comanda.CreatedBy)
            .Select(g => new
            {
                Mesonero = g.Key,
                Comandas = g.Select(d => d.ComandaId).Distinct().Count(),
                Cuentas = g.Select(d => d.Comanda.CuentaId).Distinct().Count(),
                Totales = g.Count(),
                Cancelados = g.Count(d => d.Estado == EstadoItemComanda.Cancelado),
                Servidos = g.Count(d => d.Estado == EstadoItemComanda.Preparado || d.Estado == EstadoItemComanda.Entregado),
                Ventas = g.Sum(d => d.Estado != EstadoItemComanda.Cancelado && !d.EsCortesia
                    ? d.Cantidad * d.PrecioUnitarioUSD
                    : 0m),
                ConTiempo = g.Count(d => d.ServidoEn != null),
                Segundos = g.Sum(d => d.ServidoEn != null
                    ? Math.Max(0d, (d.ServidoEn.Value - d.CreatedAt).TotalSeconds)
                    : 0d)
            })
            .ToListAsync(cancellationToken);

        var ids = grupos.Where(g => g.Mesonero != null).Select(g => g.Mesonero!.Value).ToList();
        var nombres = await _context.Usuarios
            .AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.NombreCompleto, cancellationToken);

        return grupos
            .Select(g => new MesoneroCrudoDto(
                g.Mesonero,
                g.Mesonero is { } id && nombres.TryGetValue(id, out var nombre) ? nombre : "Sin asignar",
                g.Comandas,
                g.Cuentas,
                g.Totales,
                g.Cancelados,
                g.Servidos,
                g.Ventas,
                g.ConTiempo,
                g.Segundos))
            .ToList();
    }
}
