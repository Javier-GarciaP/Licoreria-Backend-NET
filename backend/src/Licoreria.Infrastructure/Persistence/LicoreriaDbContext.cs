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
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica todas las configuraciones Fluent API (IEntityTypeConfiguration<T>)
        // definidas en este ensamblado, incluidas las precisiones, índices,
        // integridad referencial y la siembra de datos.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
