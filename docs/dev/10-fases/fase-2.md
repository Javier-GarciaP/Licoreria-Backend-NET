# Fase 2 · Persistencia de Datos y Seeding (EF Core 10 + PostgreSQL 15)

Configuración de la capa de persistencia con **Entity Framework Core 10** sobre
**PostgreSQL 15**, con Code-First, Fluent API, siembra de datos y repositorios
optimizados para lectura.

## Objetivo

Implementar la capa `Infrastructure` sin contaminar el dominio con atributos de base
de datos, aplicando mapeo explícito, migraciones versionadas, data seeding y el patrón
Repository con `AsNoTracking`.

## Paquetes

| Paquete | Proyecto |
| :--- | :--- |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | `Licoreria.Infrastructure` |
| `Microsoft.EntityFrameworkCore.Tools` | `Licoreria.Infrastructure` |
| `Microsoft.EntityFrameworkCore.Design` | `Licoreria.Infrastructure` |

## Mapeo con Fluent API

Clases `IEntityTypeConfiguration<T>` en
`Licoreria.Infrastructure/Persistence/Configurations`:

| Configuración | Tabla | Puntos clave |
| :--- | :--- | :--- |
| `CategoriaConfiguration` | `categorias` | `HasMaxLength`, `HasData` |
| `MarcaConfiguration` | `marcas` | `HasMaxLength`, `HasData` |
| `UnidadMedidaConfiguration` | `unidades_medida` | `HasMaxLength`, `HasData` |
| `ProductoConfiguration` | `productos` | UUID, `HasPrecision(18,2)`, índice único en `Sku`, `Restrict`, `HasData` |
| `UsuarioConfiguration` | `usuarios` | índice único en `Email`, `Rol` como string, `HasData` |
| `VentaConfiguration` | `ventas` | `HasPrecision(18,4/18,2)`, `Restrict` |
| `DetalleVentaConfiguration` | `detalles_venta` | `Cascade`/`Restrict`, propiedad calculada ignorada |

Reglas aplicadas:

- Claves primarias **UUID** (`Guid`).
- **Tablas en minúsculas**.
- Longitudes máximas con `HasMaxLength`.
- Precisión financiera `numeric(18,2)`.
- Índice único en `Sku` (`HasIndex(p => p.Sku).IsUnique()`).
- Relaciones 1:N con `DeleteBehavior.Restrict`.
- **0 % de Data Annotations** en el dominio.

## DbContext

`LicoreriaDbContext` registra los `DbSet` y aplica todas las configuraciones con
`modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly())`.

## Migraciones

| Migración | Contenido |
| :--- | :--- |
| `InitialInfrastructureCatalog` | Tablas, restricciones e índice único; siembra de catálogo. |
| `SeedUsuariosConPasswordHash` | Hashes PBKDF2 de los usuarios sembrados. |

Se aplican automáticamente al arrancar la API con `Database.MigrateAsync()`.

## Siembra de datos

| Entidad | Registros |
| :--- | :--- |
| Categorías (maestras) | Licores, Gaseosas, Energizantes, Pasabocas |
| Marcas | Cacique, Old Parr, Coca-Cola, Red Bull, Frito-Lay |
| Unidades de medida | Botella, Unidad, Tobo, Plato |
| Productos (dependientes) | 5 productos con SKU, descripción, costo, precio, stock mín/máx |
| Usuarios | Administrador (`Admin`) y Cajero (`Employee`) |

## Repositorios y optimización

- Interfaces en `Licoreria.Application` (`IRepository<T>`, `IProductoRepository`,
  `ICategoriaRepository`, `IUsuarioRepository`).
- Implementaciones en `Licoreria.Infrastructure/Repositories`.
- **`AsNoTracking()`** en todas las consultas de lectura.

## Evidencia

**Tablas creadas en PostgreSQL** (UUID, claves y relaciones):

![Tablas creadas](../../assets/evidencias/fase-2/tablas.png)

**DDL con restricciones** (`uuid`, `numeric(18,2)`, PK/FK y único):

![DDL de productos](../../assets/evidencias/fase-2/genera-sql-productos.png)

**Datos sembrados** (categorías, marcas, unidades y productos):

![Siembra de datos](../../assets/evidencias/fase-2/siembra-datos.png)

> Script DDL completo:
> [`database/scripts/initial-infrastructure-catalog.sql`](../../../database/scripts/initial-infrastructure-catalog.sql).

## Cómo reproducir

```bash
# 1. Levantar PostgreSQL 15
docker compose up -d postgres

# 2. Aplicar migraciones
dotnet ef database update \
  --project backend/src/Licoreria.Infrastructure \
  --startup-project backend/src/Licoreria.WebAPI

# 3. Verificar en DBeaver: localhost:5432 / licoreria / licoreria / licoreria
```

## Rúbrica cumplida

| Criterio | Evidencia |
| :--- | :--- |
| Fluent API y reglas de persistencia | `Configurations/` con UUID, `HasPrecision`, `IsUnique` y `Restrict`. |
| DbContext y migraciones versionadas | `LicoreriaDbContext` + `Migrations/` en Git. |
| Siembra representativa | Categorías, marcas, unidades y productos. |
| Repositorios y `AsNoTracking` | Repositorios desacoplados con lecturas sin tracking. |

## Documentación relacionada

- [Modelo entidad-relación](../03-base-datos/modelo-er.md)
- [Esquemas y tablas](../03-base-datos/esquemas.md)
- [Estrategia PostgreSQL](../03-base-datos/postgresql.md)
- [Evidencias por fase](../../assets/evidencias/README.md)
