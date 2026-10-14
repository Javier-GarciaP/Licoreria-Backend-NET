# Convenciones de la API

## Principios REST

- API **orientada a recursos**, con sustantivos en plural.
- Uso correcto de métodos HTTP: `GET`, `POST`, `PUT`, `PATCH`, `DELETE`.
- Respuestas en **JSON** y `Content-Type: application/json`.
- Errores en `application/problem+json` (ver [RFC 7807](errores-rfc7807.md)).

## Versionado

Todas las rutas se versionan bajo `/api/v1`:

```
GET /api/v1/productos
GET /api/v1/productos/{id}
POST /api/v1/ventas
```

## Nomenclatura de rutas

- Recursos en **plural y kebab-case**: `/api/v1/metodos-pago`.
- Recursos anidados para dependencias: `/api/v1/ventas/{id}/pagos`.
- Acciones especiales como subrecurso: `/api/v1/cuentas/{id}/cerrar`.

## Códigos de estado

| Código | Uso |
| :--- | :--- |
| `200 OK` | Consulta o actualización exitosa. |
| `201 Created` | Recurso creado (incluye `Location`). |
| `204 No Content` | Eliminación exitosa. |
| `400 Bad Request` | Datos inválidos. |
| `401 Unauthorized` | No autenticado. |
| `403 Forbidden` | Sin permisos. |
| `404 Not Found` | Recurso inexistente. |
| `409 Conflict` | Conflicto (duplicado, estado inválido). |
| `422 Unprocessable Entity` | Regla de negocio incumplida. |
| `500 Internal Server Error` | Error no controlado. |

## Paginación, filtrado y orden

- Paginación por `page` y `pageSize` (con `pageSize` máximo).
- Filtros por query string: `?categoriaId=...&activo=true`.
- Orden por `sort` (`?sort=nombre&order=asc`).
- La respuesta paginada incluye metadatos:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalItems": 135,
  "totalPages": 7
}
```

## Contrato de datos

- Se exponen **DTOs**, nunca entidades de dominio.
- Campos en `camelCase` en JSON.
- Fechas en formato **ISO 8601 UTC** (`2026-09-26T14:30:00Z`).
- Montos con su moneda y, cuando aplique, la tasa aplicada.

## Documentación

- Contrato **OpenAPI 3** en [`openapi/licoreria.yaml`](../../../openapi/licoreria.yaml).
- Swagger UI disponible en desarrollo (`/swagger`).
- El contrato es la fuente para generar el cliente del frontend.

## Endpoints por módulo (resumen)

| Módulo | Prefijo |
| :--- | :--- |
| Catálogo | `/api/v1/categorias`, `/api/v1/productos`, `/api/v1/marcas` |
| Inventario | `/api/v1/stock`, `/api/v1/movimientos-inventario`, `/api/v1/mermas` |
| Compras | `/api/v1/proveedores`, `/api/v1/ordenes-compra` |
| Ventas | `/api/v1/ventas`, `/api/v1/comandas`, `/api/v1/cuentas`, `/api/v1/pagos` |
| Caja | `/api/v1/sesiones-caja`, `/api/v1/movimientos-caja` |
| Club | `/api/v1/zonas`, `/api/v1/mesas`, `/api/v1/planos`, `/api/v1/reservas` |
| CRM | `/api/v1/clientes` |
| Finanzas | `/api/v1/tasas-cambio` |
| Contenido | `/api/v1/paginas`, `/api/v1/eventos`, `/api/v1/menu-digital` |
| IA | `/api/v1/ai/generaciones`, `/api/v1/ai/planos` |
| Seguridad | `/api/v1/auth`, `/api/v1/usuarios`, `/api/v1/roles` |
