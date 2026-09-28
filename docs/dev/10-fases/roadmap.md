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

## Fase 2 · API final

**Alcance**

- Migrar de SQL Server a **PostgreSQL** (ver [estrategia](../03-base-datos/postgresql.md)).
  _Avanzado: proveedor Npgsql, migración inicial y `MigrateAsync` al arrancar._
- Configuración **Fluent API** por entidad y data seeding.
  _Avanzado: `Configurations/` con categorías, marcas, unidades y productos._
- Implementar los módulos: catálogo, inventario, compras, ventas/POS, caja, club,
  CRM, finanzas, contenido e IA.
- Completar el contrato OpenAPI y Swagger.

## Fase 3 · Seguridad

**Alcance**

- Autenticación JWT con refresco.
- RBAC con permisos por rol.
- Interceptor de auditoría (`created_by`/`updated_by`).
- Endurecimiento de la API.

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
