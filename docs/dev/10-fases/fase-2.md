# Fase 2 · Persistencia de Datos y Seeding (EF Core 10 + PostgreSQL 15)

Esta página documenta la capa de persistencia implementada.

## Objetivo

Configurar la capa `Infrastructure` con **Entity Framework Core 10** sobre
**PostgreSQL 15**, con Code-First sin Data Annotations, siembra de datos maestros y
repositorios optimizados para lectura.

## Paquetes

| Paquete | Proyecto |
| :--- | :--- |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | Infrastructure |
| `Microsoft.EntityFrameworkCore.Tools` | Infrastructure |
| `Microsoft.EntityFrameworkCore.Design` | Infrastructure |

## Mapeo con Fluent API

- Clases `IEntityTypeConfiguration<T>` en `Infrastructure/Persistence/Configurations`.
- Claves primarias **UUID** (`Guid`).
- **Tablas en minúsculas**: `productos`, `categorias`, `marcas`, `unidades_medida`,
  `usuarios`, `ventas`, `detalles_venta`.
- Precisión financiera `numeric(18,2)` (`HasPrecision(18, 2)`).
- Índice único en `Sku` (`HasIndex(p => p.Sku).IsUnique()`).
- Relaciones 1:N con `DeleteBehavior.Restrict`.
- Longitudes máximas con `HasMaxLength`.
- **0 % de Data Annotations** en el dominio.

## DbContext

`LicoreriaDbContext` mapea los `DbSet` y aplica las configuraciones con
`modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly())`.

## Migraciones

- Migración inicial: `InitialInfrastructureCatalog` (tablas, restricciones y siembra).
- Migración incremental: `SeedUsuariosConPasswordHash` (hashes PBKDF2).
- Aplicación automática con `Database.MigrateAsync()` al arrancar la API.
- Script DDL exportado en
  [`database/scripts/initial-infrastructure-catalog.sql`](../../../database/scripts/initial-infrastructure-catalog.sql).

## Siembra de datos (sector licorería)

| Entidad | Datos |
| :--- | :--- |
| Categorías | Licores, Gaseosas, Energizantes, Pasabocas |
| Marcas | Cacique, Old Parr, Coca-Cola, Red Bull, Frito-Lay |
| Unidades de medida | Botella, Unidad, Tobo, Plato |
| Productos | 5 productos con SKU, nombre, descripción, costo, precio, stock, mín/máx y FKs |

## Repositorios y optimización

- Interfaces en `Application` (`IRepository<T>`, `IProductoRepository`, ...).
- Implementaciones en `Infrastructure/Repositories`.
- **`AsNoTracking()`** en las consultas de lectura (`GetAllAsync`, `FindAsync`,
  `ObtenerPorEmailAsync`).

## Cómo ejecutar

```bash
docker compose up -d postgres
dotnet ef database update --project backend/src/Licoreria.Infrastructure --startup-project backend/src/Licoreria.WebAPI
```

## Evidencia

- Script SQL: `database/scripts/initial-infrastructure-catalog.sql`.
- Capturas de pgAdmin/DBeaver de tablas, restricciones y datos sembrados.
