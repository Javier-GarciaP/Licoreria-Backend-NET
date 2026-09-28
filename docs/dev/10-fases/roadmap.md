# Roadmap por Fases

> **Nota:** Las fases de desarrollo académico se rigen por lo indicado por el docente.
> Este roadmap describe el alcance técnico del proyecto y su trazabilidad con dichas
> fases; no las sustituye ni altera su orden.

## Fases del proyecto

| Fase | Nombre | Alcance | Entregable |
| :---: | :--- | :--- | :--- |
| **1** | Documentación | Visión, requerimientos, arquitectura, modelo de datos, API, seguridad, módulos, frontend, IA y operaciones. | `docs/` + sitio Astro publicado. |
| **2** | API final | Implementación completa de la API sobre **PostgreSQL**. | Solución .NET finalizada. |
| **3** | Seguridad | **JWT**, RBAC, auditoría y políticas de acceso. | API asegurada. |
| **4** | Frontend | Apps React (pública e interna) con Tailwind. | Interfaces funcionales. |
| **5** | Despliegue | CI/CD, entornos y puesta en producción. | Sistema en línea. |

## Fase 1 · Documentación (actual)

**Criterios de aceptación**

- [x] Estructura de monorepo y documentación aislada.
- [x] `docs/` con visión, requerimientos, arquitectura, BD, API, seguridad, módulos,
      frontend, IA, operaciones y guías.
- [x] Registro de decisiones (ADR).
- [x] Sitio Astro Starlight con secciones *Desarrolladores* y *Cliente*.
- [x] Workflow de GitHub Pages publicando el sitio.
- [x] Contrato OpenAPI inicial.

> La **Fase 1** del docente (fundamentos, DI, CORS, RFC 7807 y lógica de dominio) está
> documentada en [Fase 1](fase-1.md).

## Fase 2 · Persistencia de Datos y Seeding

Documentada en [Fase 2](fase-2.md).

- [x] Migrar de SQL Server a **PostgreSQL 15** (proveedor Npgsql).
- [x] Configuración **Fluent API** por entidad (`Configurations/`) sin Data Annotations.
- [x] Tablas en minúsculas, UUID, `numeric(18,2)`, índice único y `DeleteBehavior.Restrict`.
- [x] Data seeding de categorías, marcas, unidades y productos (sector licorería).
- [x] Repositorios desacoplados con `AsNoTracking`.
- [x] Migraciones versionadas y script SQL de evidencia.
- [ ] Implementar los módulos restantes: inventario, compras, caja, club, CRM, finanzas, IA.

## Fase 3 · Seguridad Stateless (JWT), RBAC y Validación

Documentada en [Fase 3](fase-3.md).

- [x] Autenticación JWT (HMAC-SHA256) con `POST /api/auth/login`.
- [x] Hash de contraseñas con PBKDF2 + salt.
- [x] RBAC con roles de seguridad `Admin`/`Employee` y roles de dominio.
- [x] 401 sin token y 403 con rol insuficiente.
- [x] Validación defensiva con FluentValidation y 400 estructurado.
- [x] Colección Postman con los 4 escenarios.
- [ ] Refresco de token e interceptor de auditoría (`created_by`/`updated_by`).

## Fase 4 · Frontend

**Alcance**

- App pública: catálogo, menú, tasas, reservas y contacto.
- App interna: dashboard, operación, POS, plano y administración.
- Design system compartido.

## Fase 5 · Despliegue

**Alcance**

- Contenedores y CI/CD.
- Entornos de pruebas y producción.
- Respaldos y observabilidad.

## Trazabilidad con las fases académicas

Cada fase técnica se desarrolla en una rama (`fase-N/...`) y se documenta con commits
atómicos (ver [ramas](../11-guias/ramas.md)). El docente define el orden y los entregables
oficiales; este documento mantiene la correspondencia técnica.
