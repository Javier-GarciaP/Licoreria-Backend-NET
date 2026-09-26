# Manejo de Errores · RFC 7807

Todos los errores se devuelven con el formato **`application/problem+json`**
(*Problem Details for HTTP APIs*, RFC 7807), mediante el `ExceptionMiddleware` global.

## Estructura

```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "El recurso solicitado no fue encontrado.",
  "instance": "/api/v1/productos/00000000-0000-0000-0000-000000000000"
}
```

| Campo | Descripción |
| :--- | :--- |
| `type` | URI que identifica el tipo de problema (RFC 7231). |
| `title` | Resumen legible y estable del tipo. |
| `status` | Código de estado HTTP. |
| `detail` | Explicación específica de la ocurrencia. |
| `instance` | URI de la petición que originó el error. |

## Mapeo de excepciones

| Excepción | HTTP | Title |
| :--- | :---: | :--- |
| `KeyNotFoundException` | `404` | Not Found |
| `InvalidOperationException` | `400` | Bad Request |
| `ValidationException` (previsto) | `422` | Unprocessable Entity |
| `Exception` no controlada | `500` | Internal Server Error |

## Validación (previsto)

Cuando se incorpore FluentValidation, los errores de validación añadirán el arreglo
`errors` con los campos inválidos:

```json
{
  "type": ".../validation",
  "title": "Unprocessable Entity",
  "status": 422,
  "detail": "La petición contiene datos inválidos.",
  "errors": {
    "cantidad": ["La cantidad debe ser mayor que cero."]
  }
}
```

## Seguridad en producción

- En entorno `Production`, los errores `500` **ocultan** el mensaje técnico y el
  *stack trace*; se devuelve un detalle genérico.
- Todas las excepciones se registran con `ILogger` antes de responder.
- La respuesta se serializa en `camelCase` para consistencia con el contrato REST.

## Endpoints de prueba

El `TestErrorController` permite verificar el comportamiento:

| Ruta | Excepción | Respuesta |
| :--- | :--- | :---: |
| `GET /api/test-error/not-found` | `KeyNotFoundException` | `404` |
| `GET /api/test-error/bad-request` | `InvalidOperationException` | `400` |
| `GET /api/test-error/server-error` | `Exception` | `500` |
