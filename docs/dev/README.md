# Documentación Técnica

Bienvenido a la documentación técnica del proyecto **Licorería / Discoteca**, una
plataforma web (pública e interna) para la operación integral de un local de una
sola sucursal: catálogo, inventario, reservas, comandas, caja, POS, CRM, contenido
web y un módulo de IA.

## Índice

| Sección | Contenido |
| :--- | :--- |
| [00 · Visión](00-vision/vision.md) | Visión, alcance y glosario del dominio. |
| [01 · Requerimientos](01-requerimientos/funcionales.md) | Requerimientos funcionales y no funcionales, historias de usuario. |
| [02 · Arquitectura](02-arquitectura/onion.md) | Onion Architecture, diagramas C4 y patrones. |
| [03 · Base de Datos](03-base-datos/modelo-er.md) | Modelo entidad-relación y estrategia PostgreSQL. |
| [04 · API](04-api/convenciones.md) | Convenciones REST y errores RFC 7807. |
| [05 · Seguridad](05-seguridad/jwt.md) | Autenticación JWT y control de acceso por roles. |
| [06 · Módulos](06-modulos/catalogo.md) | Reglas y flujos de cada módulo de negocio. |
| [07 · Frontend](07-frontend/overview.md) | Aplicaciones React y design system. |
| [08 · IA](08-ia/casos-uso.md) | Casos de uso profesionales de IA. |
| [09 · Operaciones](09-operaciones/entornos.md) | Entornos, despliegue y observabilidad. |
| [10 · Fases](10-fases/roadmap.md) | Roadmap y criterios de aceptación. |
| [11 · Guías](11-guias/commits.md) | Commits, ramas y estilo de trabajo. |
| [ADR](adr/README.md) | Registro de decisiones de arquitectura. |

## Cómo usar esta documentación

- La **fuente canónica** es este directorio (`docs/dev`), visible en GitHub.
- El **sitio de documentación** (Astro Starlight) consume estos archivos para la
  sección *Desarrolladores* y agrega una sección *Cliente*.
- Los diagramas se escriben en **Mermaid** para renderizar en GitHub y en el sitio.

## Estado del proyecto

Consulta el [roadmap de fases](10-fases/roadmap.md) y el
[CHANGELOG](../../CHANGELOG.md) del repositorio.

> Las fases académicas se rigen por lo indicado por el docente. Esta documentación
> describe el objetivo técnico completo e independiente de dichas fases.
