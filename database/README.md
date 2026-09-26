# Base de Datos

Artefactos de base de datos **PostgreSQL** complementarios al código.

> Las migraciones de EF Core viven en
> `backend/src/Licoreria.Infrastructure/Persistence/Migrations`. Este directorio
> contiene scripts de infraestructura de datos, no el esquema generado por EF.

## Estructura

| Carpeta | Contenido |
| :--- | :--- |
| `init/` | Scripts que se ejecutan al crear el contenedor (extensiones, roles, esquemas). |
| `seeds/` | Datos iniciales o de referencia (catálogo base, roles, denominaciones). |
| `scripts/` | Utilidades (backup, restore, migración de datos). |
| `diagrams/` | Exportaciones del modelo entidad-relación. |

## Entorno local

```bash
task db:up     # levanta PostgreSQL con docker-compose
task db:down   # detiene los servicios
```

La configuración de conexión se define en `.env` (ver `.env.example`).

## Documentación

- [Modelo entidad-relación](../docs/dev/03-base-datos/modelo-er.md)
- [Esquemas y tablas](../docs/dev/03-base-datos/esquemas.md)
- [Estrategia PostgreSQL](../docs/dev/03-base-datos/postgresql.md)
