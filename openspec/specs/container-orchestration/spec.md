# container-orchestration Specification

## Purpose

Levanta la pila completa con un solo comando y garantiza el orden de arranque por salud.

## Requirements

### Requirement: Pila db, api y spa
`docker compose up --build -d` SHALL levantar `db` (PostgreSQL 15, 5432, volumen `postgres_data`), `api` (5000) y `spa` (8080), con healthcheck en `db` y `api`.

#### Scenario: Arranque ordenado
- **WHEN** se ejecuta `docker compose up --build -d`
- **THEN** `api` espera a que `db` esté `healthy` y `spa` a que `api` lo esté, y `docker ps` muestra `db` y `api` en `(healthy)`

### Requirement: Imágenes multi-stage
Las imágenes SHALL compilarse en una etapa con SDK y ejecutarse en una imagen runtime (Alpine / nginx) sin herramientas de compilación.

#### Scenario: Imagen de la SPA
- **WHEN** se construye la `spa`
- **THEN** la imagen final contiene solo nginx y los activos compilados
