using Licoreria.Application.Dtos;

namespace Licoreria.Application.Reportes;

/// <summary>
/// Convierte los DTO de cada reporte en tablas neutrales. No toca la base de datos ni conoce el formato.
/// </summary>
public static class MapeadoresReporte
{
    private static ColumnaReporte Texto(string t) => new(t, TipoColumnaReporte.Texto);
    private static ColumnaReporte Entero(string t) => new(t, TipoColumnaReporte.Entero);
    private static ColumnaReporte Decimal(string t) => new(t, TipoColumnaReporte.Decimal);
    private static ColumnaReporte Moneda(string t) => new(t, TipoColumnaReporte.Moneda);
    private static ColumnaReporte Porcentaje(string t) => new(t, TipoColumnaReporte.Porcentaje);
    private static ColumnaReporte Fecha(string t) => new(t, TipoColumnaReporte.Fecha);
    private static ColumnaReporte FechaHora(string t) => new(t, TipoColumnaReporte.FechaHora);

    private static IReadOnlyList<object?> Fila(params object?[] valores) => valores;

    public static IReadOnlyList<TablaReporte> Ventas(ReporteVentasDto r) =>
    [
        new("Resumen",
            [Moneda("Total USD"), Decimal("Total Bs"), Moneda("Ticket promedio USD"), Entero("Ventas")],
            [Fila(r.TotalUSD, r.TotalBS, r.TicketPromedioUSD, r.Cantidad)]),
        new("Por día",
            [Fecha("Fecha"), Entero("Ventas"), Moneda("Total USD")],
            r.PorDia.Select(d => Fila(d.Fecha, d.Cantidad, d.TotalUSD)).ToList(),
            Fila("Total", r.PorDia.Sum(d => d.Cantidad), r.PorDia.Sum(d => d.TotalUSD))),
        new("Por usuario",
            [Texto("Usuario"), Entero("Ventas"), Moneda("Total USD")],
            r.PorUsuario.Select(u => Fila(u.UsuarioNombre, u.Cantidad, u.TotalUSD)).ToList(),
            Fila("Total", r.PorUsuario.Sum(u => u.Cantidad), r.PorUsuario.Sum(u => u.TotalUSD))),
        new("Por método de pago",
            [Texto("Método"), Texto("Moneda"), Decimal("Monto"), Entero("Pagos")],
            r.PorMetodoPago.Select(m => Fila(m.MetodoPagoNombre, m.Moneda.ToString(), m.Monto, m.Pagos)).ToList())
    ];

    public static IReadOnlyList<TablaReporte> Productos(ReporteProductosDto r)
    {
        var columnas = new[]
        {
            Texto("Producto"), Texto("Presentación"), Texto("SKU"), Decimal("Unidades"), Moneda("Ingreso USD"),
            Porcentaje("% unidades"), Porcentaje("% ingreso"), Decimal("Existencia"), Decimal("Rotación"), Texto("Estado")
        };

        static IReadOnlyList<object?> Mapear(ProductoVendidoDto p) => Fila(
            p.ProductoNombre, p.VarianteNombre, p.Sku, p.Unidades, p.IngresoUSD, p.PorcentajeUnidades,
            p.PorcentajeIngreso, p.Existencia, p.Rotacion, DescribirRotacion(p.EstadoRotacion));

        return
        [
            new("Productos más vendidos", columnas, r.Top.Select(Mapear).ToList(),
                Fila("Total", null, null, r.TotalUnidades, r.TotalIngresoUSD, null, null, null, null, null)),
            new("Sin movimiento", columnas, r.SinMovimiento.Select(Mapear).ToList())
        ];
    }

    private static string DescribirRotacion(EstadoRotacion estado) => estado switch
    {
        EstadoRotacion.Calculada => "Calculada",
        EstadoRotacion.NoCalculable => "No calculable",
        _ => "Sin movimiento"
    };

    public static IReadOnlyList<TablaReporte> Inventario(InventarioValorizadoDto r) =>
    [
        new("Inventario valorizado",
            [Texto("Producto"), Texto("Presentación"), Texto("SKU"), Decimal("Cantidad"), Moneda("Costo unitario USD"),
             Moneda("Costo total USD"), Moneda("Valor de venta USD"), Texto("Bajo mínimo")],
            r.Items.Select(i => Fila(i.ProductoNombre, i.VarianteNombre, i.Sku, i.Cantidad, i.CostoUnitarioUSD,
                i.CostoTotalUSD, i.ValorVentaUSD, i.BajoMinimo ? "Sí" : "No")).ToList(),
            Fila("Total", null, null, r.UnidadesTotales, null, r.CostoTotalUSD, r.ValorVentaTotalUSD, null))
    ];

    public static IReadOnlyList<TablaReporte> Compras(ReporteComprasDto r) =>
    [
        new("Compras del periodo",
            [Entero("Recepciones"), Moneda("Comprado USD"), Moneda("Por pagar USD"), Moneda("Pagado USD"), Entero("Cuentas pendientes")],
            [Fila(r.Recepciones, r.TotalCompradoUSD, r.TotalPorPagarUSD, r.TotalPagadoUSD, r.CuentasPendientes)])
    ];

    public static IReadOnlyList<TablaReporte> Propinas(ReportePropinasDto r) =>
    [
        new("Propinas por usuario",
            [Texto("Usuario"), Entero("Ventas"), Moneda("Propina USD")],
            r.PorUsuario.Select(u => Fila(u.UsuarioNombre, u.Ventas, u.TotalPropinaUSD)).ToList(),
            Fila("Total", r.PorUsuario.Sum(u => u.Ventas), r.TotalPropinaUSD))
    ];

    public static IReadOnlyList<TablaReporte> Mesoneros(ReporteMesonerosDto r) =>
    [
        new("Desempeño por mesonero",
            [Texto("Mesonero"), Entero("Comandas"), Entero("Ítems servidos"), Entero("Ítems cancelados"), Entero("Cuentas"),
             Moneda("Ventas generadas USD"), Moneda("Ticket por cuenta USD"), Porcentaje("% cancelación"), Entero("Servicio promedio (s)")],
            r.Mesoneros.Select(m => Fila(m.Nombre, m.Comandas, m.ItemsServidos, m.ItemsCancelados, m.Cuentas,
                m.VentasGeneradasUSD, m.TicketPorCuentaUSD, m.TasaCancelacion, m.TiempoServicioPromedioSegundos)).ToList(),
            Fila("Total", r.Mesoneros.Sum(m => m.Comandas), r.Mesoneros.Sum(m => m.ItemsServidos),
                r.Mesoneros.Sum(m => m.ItemsCancelados), null, r.TotalVentasUSD, null, null, null))
    ];

    public static IReadOnlyList<TablaReporte> Caja(ReporteCajaDto r) =>
    [
        new("Resumen",
            [Entero("Turnos cerrados"), Moneda("Descuadre total USD"), Moneda("Descuadre promedio USD"), Moneda("Mayor descuadre USD")],
            [Fila(r.Turnos, r.DescuadreTotal, r.DescuadrePromedio, r.MayorDescuadre)]),
        new("Turnos cerrados",
            [Texto("Cajero"), FechaHora("Apertura"), FechaHora("Cierre"), Moneda("Esperado USD"), Moneda("Contado USD"), Moneda("Descuadre USD")],
            r.Items.Select(t => Fila(t.UsuarioNombre, t.AbiertaEn, t.CerradaEn, t.MontoEsperado, t.MontoContado, t.Descuadre)).ToList(),
            Fila("Total", null, null, r.Items.Sum(t => t.MontoEsperado), r.Items.Sum(t => t.MontoContado), r.DescuadreTotal))
    ];

    public static IReadOnlyList<TablaReporte> Ocupacion(ReporteOcupacionDto r) =>
    [
        new("Resumen",
            [Entero("Mesas activas"), Entero("Mesas ocupadas"), Porcentaje("% ocupación"), Entero("Sesiones del periodo"),
             Entero("Sesiones abiertas"), Decimal("Sesiones por mesa"), Decimal("Permanencia promedio (min)")],
            [Fila(r.MesasActivas, r.MesasOcupadas, r.PorcentajeOcupacion, r.Sesiones, r.SesionesAbiertas,
                r.SesionesPorMesa, r.PermanenciaPromedioMinutos)]),
        new("Por zona",
            [Texto("Zona"), Entero("Mesas activas"), Entero("Mesas ocupadas"), Porcentaje("% ocupación"), Entero("Sesiones"),
             Entero("Sesiones abiertas"), Decimal("Sesiones por mesa"), Decimal("Permanencia promedio (min)")],
            r.PorZona.Select(z => Fila(z.Zona, z.MesasActivas, z.MesasOcupadas, z.PorcentajeOcupacion, z.Sesiones,
                z.SesionesAbiertas, z.SesionesPorMesa, z.PermanenciaPromedioMinutos)).ToList())
    ];

    public static IReadOnlyList<TablaReporte> MermasVsVentas(ReporteMermasVsVentasDto r) =>
    [
        new("Resumen",
            [Moneda("Ventas USD"), Moneda("Merma valorizada USD"), Porcentaje("% sobre ventas")],
            [Fila(r.VentasUSD, r.MermaValorizadaUSD, r.Porcentaje)]),
        new("Mermas por motivo",
            [Texto("Motivo"), Entero("Registros"), Decimal("Unidades"), Moneda("Costo estimado USD"), Porcentaje("% sobre ventas")],
            r.PorMotivo.Select(m => Fila(m.Motivo.ToString(), m.Registros, m.Unidades, m.ValorUSD, m.PorcentajeVentas)).ToList(),
            Fila("Total", r.PorMotivo.Sum(m => m.Registros), r.PorMotivo.Sum(m => m.Unidades), r.MermaValorizadaUSD, r.Porcentaje))
    ];

    public static IReadOnlyList<TablaReporte> Auditoria(IReadOnlyList<AuditLogDto> registros) =>
    [
        new("Bitácora de auditoría",
            [FechaHora("Fecha"), Texto("Usuario"), Texto("Tipo"), Texto("Acción"), Texto("Entidad"), Texto("Detalle"), Texto("IP")],
            registros.Select(a => Fila(a.Fecha, a.Usuario ?? "Sistema", a.Tipo, a.Accion, a.Entidad, a.Datos, a.Ip)).ToList())
    ];
}
