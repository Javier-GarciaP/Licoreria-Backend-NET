# Guía de uso de la API para el frontend

Esta guía resume cómo consumir la API desde las apps React (`public-web` y `admin`).

## URL base y versionado

| Entorno | Base |
| :--- | :--- |
| Desarrollo (HTTP) | `http://localhost:5190` |
| Desarrollo (HTTPS) | `https://localhost:7271` |

- **Autenticación:** `/api/auth/...` (sin versión).
- **Recursos:** `/api/v1/...` (versionados).
- **Swagger UI:** `/swagger` (solo desarrollo).
- **Health check:** `/health`.
- **Contrato:** [`openapi/licoreria.yaml`](../../../openapi/licoreria.yaml).

## Autenticación

```http
POST /api/auth/login
{ "username": "admin@licoreria.com", "password": "admin123" }
```

Respuesta `200`:

```json
{
  "usuarioId": "...",
  "nombreCompleto": "Administrador Principal",
  "email": "admin@licoreria.com",
  "rol": "Admin",
  "permisos": ["catalog:read", "sales:write", "..."],
  "accessToken": "eyJ...",
  "refreshToken": "A1B2...",
  "expiraEn": "2026-10-03T17:00:00Z"
}
```

- Envía el `accessToken` en cada petición: `Authorization: Bearer <accessToken>`.
- Renueva con `POST /api/auth/refresh` (`{ "refreshToken": "..." }`).
- Cierra sesión con `POST /api/auth/logout` (`{ "refreshToken": "..." }`).
- Datos del usuario actual: `GET /api/auth/me`.

Credenciales de prueba: `admin@licoreria.com` / `admin123` y
`cajero1@licoreria.com` / `cajero123`.

## Roles y permisos

El token incluye el claim `permissions` con claves `modulo:accion`
(por ejemplo `sales:write`, `account:close`, `content:publish`). La API **siempre**
valida el permiso en el servidor; el front solo oculta acciones no permitidas.

- `GET /api/v1/roles` y `GET /api/v1/permisos` devuelven el catálogo.

## Errores (RFC 7807)

Todos los errores usan `application/problem+json`:

```json
{
  "type": "https://httpstatuses.com/422",
  "title": "Regla de Negocio Incumplida",
  "status": 422,
  "detail": "El stock no puede quedar en negativo.",
  "instance": "/api/v1/ventas"
}
```

| Código | Significado |
| :--- | :--- |
| `400` | Datos inválidos (incluye `errors` por campo de FluentValidation). |
| `401` | No autenticado o token expirado. |
| `403` | Sin permiso para la acción. |
| `404` | Recurso inexistente. |
| `409` | Conflicto (duplicado, p. ej. correo o SKU repetido). |
| `422` | Regla de negocio incumplida. |
| `500` | Error interno. |

## Paginación

Los listados aceptan `?page=1&pageSize=20` y devuelven:

```json
{ "items": [], "page": 1, "pageSize": 20, "totalItems": 135, "totalPages": 7 }
```

`pageSize` máximo: 100. Muchos listados aceptan filtros extra (por ejemplo
`busqueda`, `categoriaId`, `desde`, `hasta`, `estado`).

## Endpoints públicos (sin token)

- `GET /api/v1/menu-digital` y `GET /api/v1/menu-digital/qr`
- `GET /api/v1/tasas-cambio/actual?tipo=Paralelo`
- `GET /api/v1/eventos`
- `GET /api/v1/paginas`, `GET /api/v1/horarios`, `GET /api/v1/local-info`
- `GET /api/v1/mesas`, `GET /api/v1/planos`, `GET /api/v1/zonas`
- `GET /api/v1/metodos-pago`
- `POST /api/v1/reservas` y `POST /api/v1/reservas/{id}/pagos` (reserva web con seña)

## Mapa de módulos

| Módulo | Prefijo |
| :--- | :--- |
| Auth | `/api/auth` |
| Usuarios / roles / permisos | `/api/v1/usuarios`, `/api/v1/roles`, `/api/v1/permisos` |
| Catálogo | `/api/v1/productos`, `/api/v1/categorias`, `/api/v1/marcas`, `/api/v1/unidades-medida`, `/api/v1/impuestos`, `/api/v1/listas-precio` |
| Inventario | `/api/v1/stock`, `/api/v1/movimientos-inventario`, `/api/v1/mermas`, `/api/v1/ajustes-inventario`, `/api/v1/reportes/mermas` |
| Finanzas | `/api/v1/monedas`, `/api/v1/tasas-cambio`, `/api/v1/movimientos-tesoreria` |
| Ventas | `/api/v1/ventas`, `/api/v1/metodos-pago` |
| Cuentas y comandas | `/api/v1/cuentas` |
| Caja | `/api/v1/sesiones-caja`, `/api/v1/denominaciones` |
| Club | `/api/v1/zonas`, `/api/v1/mesas`, `/api/v1/planos`, `/api/v1/reservas`, `/api/v1/eventos` |
| CRM | `/api/v1/clientes`, `/api/v1/cuentas-por-cobrar` |
| Contenido | `/api/v1/paginas`, `/api/v1/horarios`, `/api/v1/local-info`, `/api/v1/menu-digital`, `/api/v1/archivos` |
| IA | `/api/v1/ai` |

## Tiempo real (SignalR)

- Hub: `/hubs/comandas` (requiere JWT; envíalo como `access_token` en la query al conectar).
- Método `UnirseArea(area)` con `"barra"`, `"cocina"`, `"meseros"` o `"mesas"`.
- Eventos: `comanda:creada`, `comanda:actualizada`, `item:actualizado`,
  `mesa:actualizada` (`{ mesaId, estado, cuentaId }`).
- El desalojo de una mesa (`POST /api/v1/mesas/{id}/desalojar`) emite `mesa:actualizada`
  con `estado: "Libre"` para que los planos de todos los clientes se refresquen al instante.

## Subida de archivos

`POST /api/v1/archivos?carpeta=media` con `multipart/form-data` (campo `archivo`).
Devuelve `{ "url": "/uploads/media/..." }`, que se sirve como archivo estático.

## Menú digital y QR

`GET /api/v1/menu-digital` devuelve las secciones generadas desde el catálogo con
precios en USD y BS. `GET /api/v1/menu-digital/qr` devuelve la URL a codificar; el
front genera la imagen QR con la librería `qrcode` (ya prevista en el stack).

## Notas

- Las fechas viajan en ISO 8601 UTC.
- Los enums se serializan como texto (por ejemplo `"Preparado"`, `"USD"`).
- El contrato OpenAPI es la fuente para generar el cliente tipado del front.
