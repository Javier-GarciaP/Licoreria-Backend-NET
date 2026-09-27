# Registro de Decisiones de Arquitectura (ADR)

Un **ADR** (Architecture Decision Record) documenta una decisión de arquitectura
relevante: su contexto, la decisión tomada y sus consecuencias. Se usa el formato
[MADR](https://adr.github.io/madr/).

## Índice

| ID | Decisión | Estado |
| :--- | :--- | :--- |
| [0001](0001-onion-architecture.md) | Usar Onion Architecture en el backend .NET | Aceptada |
| [0002](0002-postgresql.md) | Usar PostgreSQL como base de datos | Aceptada |
| [0003](0003-frontend-react-tailwind.md) | Frontend con React + Tailwind CSS | Aceptada |
| [0004](0004-jwt-rbac.md) | Autenticación JWT con RBAC | Aceptada |
| [0005](0005-ia-en-nube.md) | Ejecutar la IA en la nube | Aceptada |
| [0006](0006-documentacion-astro-starlight.md) | Documentación con Astro Starlight | Aceptada |
| [0007](0007-sucursal-unica.md) | Modelo para una única sucursal | Aceptada |

## Crear un ADR

1. Copia la estructura de un ADR existente.
2. Numera secuencialmente: `000N-titulo-en-kebab-case.md`.
3. Añádelo a la tabla anterior.
4. Usa estado: `Propuesta`, `Aceptada`, `Rechazada`, `Sustituida por NNNN`.
