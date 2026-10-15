using Licoreria.Application.Security;

namespace Licoreria.Application.Reportes;

/// <summary>Reporte exportable con el permiso (o el rol de administrador) que exige su consulta.</summary>
public sealed record ReporteExportable(string Id, string Titulo, string? Permiso, bool SoloAdministrador = false);

/// <summary>
/// Reportes que se pueden descargar. Cada uno hereda el control de acceso de su consulta en pantalla.
/// </summary>
public static class CatalogoReportes
{
    public static readonly IReadOnlyList<ReporteExportable> Todos =
    [
        new("ventas", "Ventas del periodo", Permisos.VentasLeer),
        new("productos", "Productos más vendidos", Permisos.VentasLeer),
        new("inventario", "Inventario valorizado", Permisos.InventarioLeer),
        new("compras", "Compras del periodo", Permisos.ComprasLeer),
        new("propinas", "Propinas del periodo", Permisos.VentasLeer),
        new("mesoneros", "Desempeño de mesoneros", null, SoloAdministrador: true),
        new("caja", "Descuadre de caja por turno", Permisos.CajaCerrar),
        new("ocupacion", "Ocupación de mesas", Permisos.ClubLeer),
        new("mermas-vs-ventas", "Mermas frente a ventas", Permisos.InventarioLeer),
        new("auditoria", "Bitácora de auditoría", null, SoloAdministrador: true)
    ];

    public static ReporteExportable? Buscar(string id)
        => Todos.FirstOrDefault(r => string.Equals(r.Id, id, StringComparison.OrdinalIgnoreCase));
}
