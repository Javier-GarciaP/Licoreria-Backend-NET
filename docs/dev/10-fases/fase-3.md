# Fase 3 · Seguridad Stateless (JWT), RBAC y Validación Defensiva

Esta página documenta el subsistema de seguridad y validación de la API.

## Objetivo

Implementar autenticación **stateless** con JWT, autorización por roles (**RBAC**) y
protección de entradas con **FluentValidation**.

## Autenticación JWT

- Endpoint: `POST /api/auth/login` con `LoginDto` (`Username`, `Password`).
- Respuesta `AuthResponseDto`: `Token`, `Username`, `Email`, `Role`, `Expira`.
- Contraseñas con **PBKDF2 (SHA-256) + salt** (`IPasswordHasher`).
- Token firmado con **HMAC-SHA256** (`ITokenService`).
- Claims: `NameIdentifier`, `Name`, `Email`, `Role` y expiración.
- Clave secreta en `appsettings.json` (sección `Jwt`) o variables de entorno
  (`Jwt__Key`), nunca hardcodeada en el código.

## Roles y RBAC

El JWT incluye el **rol de seguridad** (`Admin` / `Employee`) y el **rol de dominio**
(`Administrador`, `Cajero`, `Mesero`, `Barra`, `Cocina`, `Host`, `EditorContenido`).

| Acción | Atributo | Roles permitidos |
| :--- | :--- | :--- |
| Consultar / crear | `[Authorize(Roles = "Admin,Employee")]` | Todos los autenticados |
| Eliminar | `[Authorize(Roles = "Admin")]` | Solo `Admin` |

Respuestas HTTP:

- **401 Unauthorized** sin token o token inválido.
- **403 Forbidden** cuando el rol es insuficiente (p. ej. `Employee` intentando eliminar).

## Validación defensiva

- Validadores `AbstractValidator<T>` en `Application/Validators`.
- Reglas: precios > 0, stock ≥ 0, stock máximo > stock mínimo, textos obligatorios con
  longitud máxima.
- Filtro global `ValidationFilter`: si falla, responde **400 Bad Request** con
  `ValidationProblemDetails` y los errores por campo.

## Credenciales de prueba

| Usuario | Contraseña | Rol de dominio | Rol de seguridad |
| :--- | :--- | :--- | :--- |
| `admin@licoreria.com` | `admin123` | Administrador | `Admin` |
| `cajero1@licoreria.com` | `cajero123` | Cajero | `Employee` |

## Evidencia (Postman)

Colección con los 4 escenarios en
[`docs/assets/evidencias/Licoreria_Fase3_Postman_Collection.json`](../../assets/evidencias/Licoreria_Fase3_Postman_Collection.json):

1. Login exitoso con rol `Admin` y con rol `Employee`.
2. Endpoint protegido sin token → **401**.
3. Eliminación con token `Employee` → **403**.
4. Creación con precio negativo → **400** con errores detallados.
