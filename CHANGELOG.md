# Changelog

Todos los cambios relevantes de este proyecto se documentan en este archivo.

El formato esta basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/)
y el proyecto se adhiere a [Versionado Semantico](https://semver.org/lang/es/).

> **Nota:** Las fases de desarrollo academico se rigen por lo indicado por el docente.
> Este changelog registra la evolucion tecnica del repositorio (estructura, documentacion,
> infraestructura y refactorizaciones), independientemente de dichas fases.

## [No publicado]

### Agregado

- Estructura base de monorepo (`backend`, `frontend`, `database`, `docs`, `infra`, `openapi`).
- Archivos de configuracion raiz (`.editorconfig`, `Taskfile.yml`, `docker-compose.yml`, `.env.example`).
- Guia de contribucion, convencion de commits y plantilla `.gitmessage`.
- Documentacion tecnica completa en `docs/dev` (vision, requerimientos, arquitectura,
  base de datos, API, seguridad, modulos, frontend, IA, operaciones, fases y ADRs).
- Contrato OpenAPI 3 de la API v1 en `openapi/licoreria.yaml`.
- Sitio de documentacion Astro Starlight con secciones *Desarrolladores* y *Cliente*.
- Workflows de GitHub Actions para el sitio de documentacion y el backend.
- Plantillas de Pull Request, issues y `CODEOWNERS`.
- Encapsulamiento de `Producto` con `Sku`, `StockMaximo` y metodos de dominio.
- Enums `RolUsuario` y `EstadoSaludStock`.
- Servicios de dominio: salud de stock, generador de SKU, cuenta/abonos,
  merma/cortesia, conversion de moneda, estados de comanda y conflictos de reserva.
- DTOs, interfaces de servicios y validadores con FluentValidation.
- Controlador de simulacion (`/api/v1/simulacion`) para la logica de dominio.
- Proyecto de pruebas `Licoreria.UnitTests` (xUnit) con 45 pruebas.
- Fase 2: tablas en minusculas, `Descripcion` en `Producto`, PostgreSQL 15 y migracion
  `InitialInfrastructureCatalog`.
- Fase 3: autenticacion JWT (`POST /api/auth/login`), `IPasswordHasher` (PBKDF2),
  `ITokenService` (HMAC-SHA256), controladores de catalogo con RBAC y `ValidationFilter`.
- Coleccion Postman de la Fase 3 con los 4 escenarios.
- Paginas de documentacion de Fase 2 y Fase 3.
- Pagina de la Fase 1 en la documentacion.
- Entidades `Marca` y `UnidadMedida` con su siembra y relacion con `Producto`.
- Configuraciones Fluent API por entidad (`IEntityTypeConfiguration<T>`) en `Configurations/`.
- Politica de CORS configurable desde `appsettings.json`.
- Migracion inicial de PostgreSQL (`InicialPostgreSql`).

### Cambiado

- La solucion .NET se reubico en `backend/src` conservando la arquitectura Onion.
- El `README.md` raiz se reescribio como portada del monorepo.
- La inyeccion de dependencias se organiza por ciclos de vida (Transient, Scoped, Singleton).
- El middleware RFC 7807 usa titulos en espanol y `https://httpstatuses.com/{status}`,
  y mapea `ArgumentException` (400) y `UnauthorizedAccessException` (401).
- `Program.cs` valida scopes para evitar dependencias cautivas.
- La persistencia migra de SQL Server a **PostgreSQL** (Npgsql) y aplica `MigrateAsync`
  al arrancar.

### Eliminado

- El scaffolding de plantilla `WeatherForecast`.

[No publicado]: https://github.com/Javier-GarciaP/Licoreria-Backend-NET/commits/master
