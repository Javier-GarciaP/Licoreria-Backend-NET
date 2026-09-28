# Licorería / Discoteca · Plataforma Web

Plataforma web integral para la gestión de un local de **licorería y discoteca**
(una sola sucursal): web pública para clientes y sistema interno para el personal.

> **Backend** .NET 10 con **Onion Architecture** · **Base de datos** PostgreSQL ·
> **Frontend** React + Tailwind CSS · **IA** aplicada al negocio.

---

## Documentación

| Recurso | Descripción |
| :--- | :--- |
| [Documentación técnica](docs/dev/README.md) | Visión, requerimientos, arquitectura, modelo de datos, API, seguridad, módulos, IA y guías. |
| [Sitio de documentación](docs/site) | Sitio Astro Starlight (secciones *Desarrolladores* y *Cliente*). |
| [Contrato OpenAPI](openapi/licoreria.yaml) | Especificación de la API v1 (contract-first). |
| [Guía de contribución](CONTRIBUTING.md) | Flujo de trabajo, ramas y commits. |
| [CHANGELOG](CHANGELOG.md) | Historial de cambios del repositorio. |

---

## Estructura del monorepo

```
.
├── backend/          # Solución .NET (Onion: Domain, Application, Infrastructure, WebAPI)
├── frontend/         # Apps React (public-web, admin) y paquetes compartidos
├── database/         # Scripts PostgreSQL, seeds y diagramas
├── openapi/          # Contrato OpenAPI de la API
├── docs/             # Documentación
│   ├── dev/          #   Documentación técnica canónica (GitHub)
│   └── site/         #   Sitio Astro Starlight (dev + cliente)
├── infra/            # Despliegue (docker, nginx, terraform)
└── .github/          # CI/CD, plantillas y CODEOWNERS
```

La documentación vive **aislada** en `docs/`, de modo que no interfiere con el flujo de
desarrollo de `backend/`, `frontend/` y `database/`.

---

## Arquitectura (resumen)

Onion Architecture con dependencias hacia el dominio:

```
WebAPI → Application → Domain
              ↑
        Infrastructure
```

Detalle en [docs/dev/02-arquitectura](docs/dev/02-arquitectura/onion.md).

---

## Puesta en marcha

```bash
# Backend
dotnet build backend/Licoreria.slnx
dotnet test backend/Licoreria.slnx
dotnet run --project backend/src/Licoreria.WebAPI

# Base de datos local (PostgreSQL, objetivo de la Fase 2)
docker compose up -d postgres

# Documentación
cd docs/site && npm install && npm run dev
```

> El proyecto incluye un `Taskfile.yml` con atajos: `task --list`.
> La persistencia usa **PostgreSQL** (ver `docker-compose.yml`); las migraciones se
> aplican automáticamente al arrancar la API.

---

## Fase 1 · Fundamentos y resiliencia REST

Implementada y verificada:

- **Onion Architecture** en 4 proyectos (`Domain` sin dependencias de frameworks).
- **Inyección de dependencias** con ciclos de vida `Transient`, `Scoped` y `Singleton`
  y validación de scopes (sin dependencias cautivas).
- **Middleware global RFC 7807** (`application/problem+json`) con títulos en español,
  mapeo de `404/400/401/500` y ocultamiento de trazas en producción.
- **CORS** configurable desde `appsettings.json` para el frontend React.
- **Persistencia PostgreSQL** con Fluent API por entidad (`IEntityTypeConfiguration`),
  índices únicos, integridad referencial restrictiva, data seeding y `MigrateAsync`
  al arrancar.
- **Lógica de dominio** con C# 14: salud de stock, generador de SKU, cuenta/abonos,
  merma/cortesía, conversión de moneda, estados de comanda y conflictos de reserva.
- **45 pruebas unitarias** (xUnit) ejecutadas correctamente.

Detalle completo en [`docs/dev/10-fases/fase-1.md`](docs/dev/10-fases/fase-1.md).

### Evidencia RFC 7807 (Postman)

![Respuesta de la API](docs/assets/evidencias/postman1.jpeg)

![Respuesta RFC 7807](docs/assets/evidencias/postman2.jpeg)

Colección de pruebas: [`Licoreria_Fase1_Postman_Collection.json`](docs/assets/evidencias/Licoreria_Fase1_Postman_Collection.json).

---

## Fase 2 · Persistencia PostgreSQL 15

- **PostgreSQL 15** con Npgsql, migraciones versionadas y `MigrateAsync` al arrancar.
- **Fluent API** por entidad (tablas en minúsculas, UUID, `numeric(18,2)`, índice único
  en `Sku`, `DeleteBehavior.Restrict`), sin Data Annotations en el dominio.
- **Data seeding** de categorías, marcas, unidades y productos (licorería).
- Repositorios con **`AsNoTracking`** y script DDL en
  [`database/scripts/initial-infrastructure-catalog.sql`](database/scripts/initial-infrastructure-catalog.sql).

Detalle en [`docs/dev/10-fases/fase-2.md`](docs/dev/10-fases/fase-2.md).

## Fase 3 · Seguridad JWT, RBAC y validación

- Login `POST /api/auth/login` con **JWT HMAC-SHA256** y contraseñas **PBKDF2**.
- **RBAC**: `Admin` (CRUD total) y `Employee` (sin eliminación).
- **401** sin token y **403** con rol insuficiente.
- Validación con **FluentValidation** → **400** con errores por campo.

Detalle y credenciales en [`docs/dev/10-fases/fase-3.md`](docs/dev/10-fases/fase-3.md).
Colección Postman: [`Licoreria_Fase3_Postman_Collection.json`](docs/assets/evidencias/Licoreria_Fase3_Postman_Collection.json).

---

## Fases del proyecto

| Fase | Alcance | Estado |
| :---: | :--- | :---: |
| 1 | Fundamentos, DI, RFC 7807 y dominio | Completada |
| 2 | Persistencia PostgreSQL 15 y seeding | Completada |
| 3 | Seguridad JWT / RBAC y validación | Completada |
| 4 | Frontend React | Pendiente |
| 5 | Despliegue | Pendiente |

> **Nota:** Las fases de desarrollo académico se rigen por lo indicado por el docente.
> Las mejoras de estructura, documentación e infraestructura de este repositorio son
> independientes de dichas fases. Ver [roadmap](docs/dev/10-fases/roadmap.md).

---

## Contribuir

Lee la [guía de contribución](CONTRIBUTING.md) y la
[convención de commits](docs/dev/11-guias/commits.md) antes de abrir un Pull Request.
