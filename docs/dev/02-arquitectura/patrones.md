# Patrones de Diseño

## Patrones de arquitectura

| Patrón | Aplicación |
| :--- | :--- |
| **Onion Architecture** | Separación en capas concéntricas con dependencias hacia el dominio. |
| **Repository** | Abstracción del acceso a datos mediante `IRepository<T>` y repositorios específicos. |
| **Unit of Work** | El `DbContext` actúa como unidad de trabajo por petición (`Scoped`). |
| **Dependency Injection** | Composición de dependencias en `Program.cs` e `Infrastructure.DependencyInjection`. |
| **CQRS ligero** | Separación de comandos y consultas mediante MediatR (previsto). |
| **Middleware** | Captura global de excepciones con RFC 7807. |

## Patrones de dominio

| Patrón | Aplicación |
| :--- | :--- |
| **Entidad base** | `BaseEntity` con `Id`, auditoría UTC y borrado lógico. |
| **Objeto de valor** | Montos con moneda y tasa; rangos de horario. |
| **Especificación** | Filtros de consulta reutilizables (previsto). |
| **Eventos de dominio** | Notificaciones internas (por ejemplo, stock bajo) (previsto). |
| **Máquina de estados** | Estados de comanda: Recibido → Preparado → Entregado/Cancelado. |

## Patrones de integración

| Patrón | Aplicación |
| :--- | :--- |
| **Adapter** | Cliente del proveedor de IA y de almacenamiento. |
| **Outbox / cola** | Trabajos de IA asíncronos (planos, imágenes, pronósticos). |
| **Circuit Breaker** | Resiliencia ante fallos de servicios externos (previsto). |

## Convenciones de código

- Entidades de dominio sin dependencias de EF Core ni ASP.NET.
- Configuración de EF Core mediante clases `IEntityTypeConfiguration<T>` (no en un único
  `OnModelCreating`).
- DTOs separados de las entidades; nunca exponer entidades directamente.
- Nombres de clases y propiedades en español, consistentes con el dominio.
