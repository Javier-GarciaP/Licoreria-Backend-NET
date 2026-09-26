# 0001 · Usar Onion Architecture en el backend .NET

- **Estado:** Aceptada
- **Fecha:** 2026-09-26
- **Decisores:** Equipo de desarrollo

## Contexto

El backend debe crecer desde un POS simple hacia un sistema empresarial con múltiples
módulos (inventario, ventas, caja, reservas, CRM, IA). Se necesita un diseño que aísle
las reglas de negocio de la infraestructura y facilite las pruebas y el mantenimiento.

## Decisión

Adoptar **Onion Architecture** con cuatro capas (`Domain`, `Application`,
`Infrastructure`, `WebAPI`) y dependencias dirigidas siempre hacia el dominio.

## Consecuencias

- **Positivas:** bajo acoplamiento, dominio testeable, infraestructura reemplazable.
- **Negativas:** mayor cantidad de proyectos y mapeos entre capas.
- **Neutrales:** requiere disciplina para no filtrar dependencias hacia afuera.

## Alternativas consideradas

- Arquitectura en capas tradicional (N-Layer): acopla el dominio a la infraestructura.
- Vertical Slice: válida, pero el equipo ya trabaja con Onion y el docente lo exige.
