# Estrategia PostgreSQL

La implementación final de la API usará **PostgreSQL**. El proyecto parte de SQL Server
LocalDB (fases académicas iniciales); esta sección describe el objetivo y la migración.

## Convenciones

| Aspecto | Convención |
| :--- | :--- |
| Esquemas | Un esquema por bounded context (`catalog`, `inventory`, `sales`, ...). |
| Tablas | `snake_case`, en singular (`producto_variante`, `movimiento_inventario`). |
| Columnas | `snake_case` (`created_at`, `precio_unitario`). |
| Claves primarias | `uuid` con `gen_random_uuid()` (extensión `pgcrypto`). |
| Claves foráneas | `<tabla>_id` con `ON DELETE RESTRICT` por defecto. |
| Montos | `numeric(18,2)`; tasas `numeric(18,4)`. |
| Cantidades | `numeric(14,3)` para permitir fracciones. |
| Fechas | `timestamptz` almacenado en UTC. |
| Booleanos | `boolean` con `is_` como prefijo cuando aplique. |
| Texto | `varchar(n)` con longitud explícita; `text` solo cuando sea necesario. |
| JSON | `jsonb` para contenido flexible (bloques, resultados de IA). |

## Borrado lógico y auditoría

- Todas las entidades de negocio tienen `is_deleted`; las consultas aplican un
  **global query filter** (`WHERE is_deleted = false`).
- `created_at`, `last_modified_at`, `created_by`, `updated_by` se gestionan con un
  **interceptor de `SaveChanges`**.
- La concurrencia optimista usa `row_version` (columna `xmin` de PostgreSQL o un
  `bigint` gestionado por la aplicación).

## Migración desde SQL Server

| Paso | Acción |
| :--- | :--- |
| 1 | Sustituir `Microsoft.EntityFrameworkCore.SqlServer` por `Npgsql.EntityFrameworkCore.PostgreSQL`. |
| 2 | Actualizar la cadena de conexión a PostgreSQL. |
| 3 | Regenerar las migraciones (el historial de SQL Server no aplica a PostgreSQL). |
| 4 | Ajustar tipos específicos (`nvarchar` → `varchar`, `datetime2` → `timestamptz`). |
| 5 | Definir esquemas con `modelBuilder.HasDefaultSchema` o `ToTable(..., "esquema")`. |
| 6 | Ejecutar `dotnet ef database update` contra la base de datos nueva. |

> Las migraciones actuales de SQL Server se conservan como referencia histórica de las
> fases académicas, pero el esquema objetivo de producción es PostgreSQL.

## Configuración EF Core (objetivo)

```csharp
services.AddDbContext<LicoreriaDbContext>(options =>
    options.UseNpgsql(
        configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.MigrationsAssembly(
            typeof(LicoreriaDbContext).Assembly.FullName)));
```

## Entorno local

El `docker-compose.yml` de la raíz levanta PostgreSQL 16 con:

- Base de datos: `licoreria`
- Usuario/contraseña: definidos en `.env` (ver `.env.example`)
- Scripts de inicialización en `database/init`
- Datos semilla en `database/seeds`

## Respaldos

- `pg_dump` programado para respaldos lógicos.
- Los respaldos se almacenan fuera del repositorio (`database/backups` está ignorado).
