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
dotnet run --project backend/src/Licoreria.WebAPI

# Base de datos local (PostgreSQL)
docker compose up -d postgres

# Documentación
cd docs/site && npm install && npm run dev
```

> El proyecto incluye un `Taskfile.yml` con atajos: `task --list`.

---

## Fases del proyecto

| Fase | Alcance | Estado |
| :---: | :--- | :---: |
| 1 | Documentación (este repositorio) | En curso |
| 2 | API final sobre PostgreSQL | Pendiente |
| 3 | Seguridad JWT / RBAC | Pendiente |
| 4 | Frontend React | Pendiente |
| 5 | Despliegue | Pendiente |

> **Nota:** Las fases de desarrollo académico se rigen por lo indicado por el docente.
> Las mejoras de estructura, documentación e infraestructura de este repositorio son
> independientes de dichas fases. Ver [roadmap](docs/dev/10-fases/roadmap.md).

---

## Contribuir

Lee la [guía de contribución](CONTRIBUTING.md) y la
[convención de commits](docs/dev/11-guias/commits.md) antes de abrir un Pull Request.
