# Fase 1 · Fundamentos Arquitectónicos, Ecosistema .NET 10 y Resiliencia REST

Esta página documenta el alcance técnico de la **Fase 1** y cómo verificarlo.

> Las fases académicas se rigen por lo indicado por el docente. Esta página describe la
> implementación técnica del repositorio para dicha fase.

## Objetivos

1. Estructura base con **Onion Architecture**.
2. **Inyección de dependencias** con ciclos de vida `Transient`, `Scoped` y `Singleton`.
3. **Middleware global de excepciones** conforme a **RFC 7807**.
4. Núcleo de dominio puro con lógica avanzada de C# 14.

## Arquitectura

```
backend/
├── Licoreria.slnx
├── src/
│   ├── Licoreria.Domain          # Núcleo puro (sin dependencias)
│   ├── Licoreria.Application     # Casos de uso, DTOs, validadores
│   ├── Licoreria.Infrastructure  # EF Core, repositorios, DI
│   └── Licoreria.WebAPI          # API REST y middleware
└── tests/
    └── Licoreria.UnitTests       # Pruebas xUnit del dominio
```

**Regla de dependencia:** `WebAPI → Infrastructure → Application → Domain`. El dominio
no referencia ningún framework (verificado: sin `PackageReference`).

## Auditoría (`BaseEntity`)

`Id` (Guid), `CreatedAt` (UTC), `LastModifiedAt`, `IsDeleted`, además de los métodos
`MarcarModificado()` y `EliminarLogico()`.

## Inyección de dependencias

| Ciclo de vida | Registros |
| :--- | :--- |
| **Transient** | Validadores de FluentValidation. |
| **Scoped** | `DbContext`, repositorios y servicios de aplicación. |
| **Singleton** | Servicios de dominio sin estado y `CacheTasasEnMemoria`. |

`Program.cs` habilita `ValidateScopes` y `ValidateOnBuild` para impedir dependencias
cautivas.

## Middleware RFC 7807

Ubicación: `backend/src/Licoreria.WebAPI/Middlewares/ExceptionMiddleware.cs`.
Respuestas con `Content-Type: application/problem+json`.

| Excepción | HTTP | Título |
| :--- | :---: | :--- |
| `KeyNotFoundException` | `404` | Recurso No Encontrado |
| `ArgumentException` | `400` | Solicitud Inválida |
| `InvalidOperationException` | `400` | Solicitud Inválida |
| `UnauthorizedAccessException` | `401` | No Autorizado |
| `Exception` | `500` | Error Interno del Servidor |

En producción, los errores `500` ocultan el detalle técnico. El campo `type` usa
`https://httpstatuses.com/{status}`.

## CORS

Política configurable desde `appsettings.json` (`Cors:AllowedOrigins`), aplicada con
`app.UseCors(...)` antes de la autorización y del mapeo de controladores. Por defecto
permite el frontend React en `http://localhost:5173`.

## Persistencia (PostgreSQL)

- Proveedor **Npgsql** (`UseNpgsql`).
- Configuración **Fluent API** por entidad en `Configurations/` (`IEntityTypeConfiguration<T>`),
  sin Data Annotations en el dominio.
- Precisiones `numeric(18,2)`, índice único en `Sku`, borrado restrictivo e integridad
  referencial.
- **Data seeding** de categorías, marcas, unidades de medida, productos y usuarios.
- **Migraciones aplicadas al arrancar** con `Database.MigrateAsync()` en un scope.

```json
{
  "type": "https://httpstatuses.com/404",
  "title": "Recurso No Encontrado",
  "status": 404,
  "detail": "El producto con ID 9999 no fue encontrado.",
  "instance": "/api/test-error/not-found"
}
```

## Lógica de dominio

| Servicio | Descripción |
| :--- | :--- |
| `EvaluadorSaludStock` | Pattern matching sobre `(stock, min, max)` con `when`; 6 estados. |
| `GeneradorSku` | Sanitización regex, prefijos `[..3]`, secuencia `D4` y validación. |
| `CalculadoraCuenta` | Totales, saldo y validación de abonos. |
| `EvaluadorMerma` | Merma y cortesía por reposición sin cobro. |
| `ConversorMoneda` | Conversión USD/BS con redondeo. |
| `MaquinaEstadosComanda` | Transiciones válidas de una comanda. |
| `DetectorConflictosReserva` | Solapamiento de reservas por mesa. |

## Controlador de simulación

`SimulacionController` en `/api/v1/simulacion`:

| Método | Ruta | Descripción |
| :--- | :--- | :--- |
| `POST` | `/salud-stock` | Evalúa la salud del inventario. |
| `POST` | `/sku` | Genera y sanitiza un SKU. |
| `POST` | `/merma` | Evalúa una merma y su reposición. |
| `POST` | `/cuenta` | Calcula el resumen de una cuenta. |
| `PUT` | `/tasa` | Actualiza la tasa vigente (Singleton). |
| `GET` | `/tasa` | Consulta la tasa vigente. |
| `POST` | `/conversion` | Convierte USD a BS. |

Y `TestErrorController` (`/api/test-error`) para demostrar el middleware.

## Cómo ejecutar

```bash
dotnet build backend/Licoreria.slnx
dotnet test backend/Licoreria.slnx
dotnet run --project backend/src/Licoreria.WebAPI
```

Swagger: `http://localhost:5190/swagger`.

## Evidencia (RFC 7807 en Postman)

![Respuesta de la API](../../assets/evidencias/postman1.jpeg)

![Respuesta RFC 7807](../../assets/evidencias/postman2.jpeg)

Colección de pruebas:
[`Licoreria_Fase1_Postman_Collection.json`](../../assets/evidencias/Licoreria_Fase1_Postman_Collection.json).

## Criterios de evaluación (rúbrica)

| Criterio | Evidencia |
| :--- | :--- |
| Onion, DI y regla de dependencia (24) | 4 proyectos desacoplados, DI por ciclos de vida, dominio sin dependencias. |
| Modelado de entidades base (16) | `BaseEntity` con auditoría UTC y `Producto` encapsulado. |
| Middleware RFC 7807 (28) | Middleware tipificado, `application/problem+json`, 500 sin trazas. |
| Repositorio, documentación y puntualidad (12) | Commits descriptivos, `.gitignore`, README y evidencia Postman. |
