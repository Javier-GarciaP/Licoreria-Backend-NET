namespace Licoreria.Application.Security;

/// <summary>
/// Catálogo de permisos atómicos de la API, nombrados <c>modulo:accion</c>.
/// Los mismos valores viajan como claims en el JWT y se usan en las políticas de autorización.
/// </summary>
public static class Permisos
{
    public const string CatalogoLeer = "catalog:read";
    public const string CatalogoEscribir = "catalog:write";

    public const string InventarioLeer = "inventory:read";
    public const string InventarioEscribir = "inventory:write";
    public const string InventarioMerma = "inventory:merma";

    public const string ComprasLeer = "purchasing:read";
    public const string ComprasEscribir = "purchasing:write";

    public const string VentasLeer = "sales:read";
    public const string VentasEscribir = "sales:write";
    public const string VentasAnular = "sales:void";

    public const string CuentasLeer = "account:read";
    public const string CuentasAbonar = "account:abono";
    public const string CuentasCerrar = "account:close";

    public const string CajaAbrir = "cash:open";
    public const string CajaCerrar = "cash:close";
    public const string CajaMovimiento = "cash:movement";

    public const string ClubLeer = "club:read";
    public const string ClubGestionar = "club:manage";
    public const string ReservasGestionar = "reservation:manage";

    public const string CrmLeer = "crm:read";
    public const string CrmEscribir = "crm:write";

    public const string FinanzasLeer = "finance:read";
    public const string FinanzasTasa = "finance:rate";

    public const string ContenidoLeer = "content:read";
    public const string ContenidoPublicar = "content:publish";

    public const string IaGenerar = "ai:generate";
    public const string IaAprobar = "ai:approve";

    public const string SeguridadUsuarios = "security:users";
    public const string SeguridadRoles = "security:roles";

    /// <summary>
    /// Catálogo completo de permisos con su módulo y descripción.
    /// </summary>
    public static IReadOnlyList<PermisoInfo> Todos { get; } =
    [
        new(CatalogoLeer, "catalog", "Consultar catálogo de productos."),
        new(CatalogoEscribir, "catalog", "Crear y modificar productos, variantes y precios."),
        new(InventarioLeer, "inventory", "Consultar existencias y kardex."),
        new(InventarioEscribir, "inventory", "Registrar ajustes y tomas físicas."),
        new(InventarioMerma, "inventory", "Registrar mermas y cortesías."),
        new(ComprasLeer, "purchasing", "Consultar proveedores y órdenes de compra."),
        new(ComprasEscribir, "purchasing", "Crear órdenes y recepciones."),
        new(VentasLeer, "sales", "Consultar ventas y comprobantes."),
        new(VentasEscribir, "sales", "Registrar ventas y pagos."),
        new(VentasAnular, "sales", "Anular ventas y emitir devoluciones."),
        new(CuentasLeer, "account", "Consultar cuentas y abonos."),
        new(CuentasAbonar, "account", "Registrar abonos sobre una cuenta."),
        new(CuentasCerrar, "account", "Cerrar cuentas y generar la venta."),
        new(CajaAbrir, "cash", "Abrir sesiones de caja."),
        new(CajaCerrar, "cash", "Cerrar sesiones de caja con arqueo."),
        new(CajaMovimiento, "cash", "Registrar ingresos y egresos de caja."),
        new(ClubLeer, "club", "Consultar zonas, mesas y planos."),
        new(ClubGestionar, "club", "Administrar zonas, mesas y planos."),
        new(ReservasGestionar, "club", "Gestionar reservas y validar señas."),
        new(CrmLeer, "crm", "Consultar clientes y fidelidad."),
        new(CrmEscribir, "crm", "Crear y modificar clientes."),
        new(FinanzasLeer, "finance", "Consultar monedas, tasas y tesorería."),
        new(FinanzasTasa, "finance", "Registrar la tasa de cambio del día."),
        new(ContenidoLeer, "content", "Consultar contenido de la web pública."),
        new(ContenidoPublicar, "content", "Publicar páginas, eventos y menú digital."),
        new(IaGenerar, "ai", "Solicitar trabajos de IA."),
        new(IaAprobar, "ai", "Aprobar resultados de IA."),
        new(SeguridadUsuarios, "security", "Administrar usuarios."),
        new(SeguridadRoles, "security", "Administrar roles y permisos.")
    ];
}

/// <summary>Información descriptiva de un permiso.</summary>
public sealed record PermisoInfo(string Clave, string Modulo, string Descripcion);
