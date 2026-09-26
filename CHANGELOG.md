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

### Cambiado

- La solucion .NET se reubico en `backend/src` conservando la arquitectura Onion.
- El `README.md` raiz se reescribio como portada del monorepo.

[No publicado]: https://github.com/Javier-GarciaP/Licoreria-Backend-NET/commits/master
