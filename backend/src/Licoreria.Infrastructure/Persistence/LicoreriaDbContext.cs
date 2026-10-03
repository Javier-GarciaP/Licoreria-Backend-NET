using System.Reflection;
using Licoreria.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Licoreria.Infrastructure.Persistence;

public class LicoreriaDbContext : DbContext
{
    public LicoreriaDbContext(DbContextOptions<LicoreriaDbContext> options) : base(options)
    {
    }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Marca> Marcas => Set<Marca>();
    public DbSet<UnidadMedida> UnidadesMedida => Set<UnidadMedida>();
    public DbSet<Impuesto> Impuestos => Set<Impuesto>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<ProductoVariante> ProductoVariantes => Set<ProductoVariante>();
    public DbSet<CodigoBarras> CodigosBarras => Set<CodigoBarras>();
    public DbSet<ListaPrecio> ListasPrecio => Set<ListaPrecio>();
    public DbSet<PrecioProducto> PreciosProducto => Set<PrecioProducto>();
    public DbSet<Receta> Recetas => Set<Receta>();
    public DbSet<StockProducto> StockProductos => Set<StockProducto>();
    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();
    public DbSet<Merma> Mermas => Set<Merma>();
    public DbSet<TasaCambio> TasasCambio => Set<TasaCambio>();
    public DbSet<MovimientoTesoreria> MovimientosTesoreria => Set<MovimientoTesoreria>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();
    public DbSet<MetodoPago> MetodosPago => Set<MetodoPago>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<ComprobanteFiscal> ComprobantesFiscales => Set<ComprobanteFiscal>();
    public DbSet<Devolucion> Devoluciones => Set<Devolucion>();
    public DbSet<DevolucionDetalle> DevolucionDetalles => Set<DevolucionDetalle>();
    public DbSet<SesionMesa> SesionesMesa => Set<SesionMesa>();
    public DbSet<Cuenta> Cuentas => Set<Cuenta>();
    public DbSet<Abono> Abonos => Set<Abono>();
    public DbSet<Comanda> Comandas => Set<Comanda>();
    public DbSet<ComandaDetalle> ComandaDetalles => Set<ComandaDetalle>();
    public DbSet<Denominacion> Denominaciones => Set<Denominacion>();
    public DbSet<SesionCaja> SesionesCaja => Set<SesionCaja>();
    public DbSet<MovimientoCaja> MovimientosCaja => Set<MovimientoCaja>();
    public DbSet<ArqueoDenominacion> ArqueosDenominacion => Set<ArqueoDenominacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica todas las configuraciones Fluent API (IEntityTypeConfiguration<T>)
        // definidas en este ensamblado, incluidas las precisiones, índices,
        // integridad referencial y la siembra de datos.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
