# 0002 · Usar PostgreSQL como base de datos

- **Estado:** Aceptada
- **Fecha:** 2026-09-26
- **Decisores:** Equipo de desarrollo

## Contexto

La implementación final de la API requiere una base de datos robusta. El proyecto
inicial usa SQL Server LocalDB, pero se busca una opción abierta, portable y adecuada
para despliegue en contenedores.

## Decisión

Usar **PostgreSQL** como base de datos principal a partir de la implementación final
de la API.

## Consecuencias

- **Positivas:** software libre, portable, excelente soporte en contenedores, tipos
  avanzados (JSONB, rangos) útiles para contenido y auditoría.
- **Negativas:** las migraciones actuales de SQL Server deberán regenerarse.
- **Neutrales:** requiere el proveedor `Npgsql.EntityFrameworkCore.PostgreSQL`.

## Alternativas consideradas

- SQL Server: licenciamiento y menor portabilidad en Linux.
- MySQL/MariaDB: capacidades avanzadas inferiores para este dominio.
