# Auditoría

La bitácora responde a «¿quién hizo qué, cuándo, desde dónde y qué había antes?». Todo cambio de
datos queda registrado **automáticamente**, sin depender de que cada funcionalidad se acuerde de
hacerlo, y solo el **Administrador** puede consultarla.

## Qué se registra

Cada entrada (`audit_log`) tiene un **tipo**:

| Tipo | Origen | Ejemplos |
| :--- | :--- | :--- |
| `Cambio` | Automático, en el interceptor de EF Core (`AuditoriaInterceptor`) | Alta, edición, borrado lógico (`eliminar`), restauración (`restaurar`) y borrado físico de cualquier entidad |
| `Evento` | Explícito, desde `IServicioAuditoria` | Abrir/cerrar caja, venta, devolución, recepción de compra, merma, toma física, destapar/descartar envase, cambio de contraseña, revocar sesiones, inicio y cierre de sesión, intento fallido |

Los eventos existen para acciones con significado de negocio o de seguridad que una fila nueva no
describe (por ejemplo el descuadre de un cierre de caja). Una acción que ya queda completamente
descrita por un cambio automático **no** se registra como evento: crear, editar o eliminar un usuario
solo genera la entrada de `Cambio`.

Cada entrada guarda acción, entidad, identificador, usuario, correo, IP y fecha. Un cambio fuera de una
petición de usuario (arranque, siembra) se registra con usuario nulo y correo `sistema`.

## Antes y después

`datos` (jsonb) contiene solo propiedades escalares:

- **Alta:** `{"despues": {...}}` con los valores iniciales.
- **Edición:** `{"antes": {...}, "despues": {...}}` únicamente con los campos que cambiaron.
  Si solo cambia el *bookkeeping* (`LastModifiedAt`, `UpdatedBy`, `CreatedAt`, `xmin`) no se genera entrada.
- **Borrado:** `{"antes": {...}}` con los últimos valores conocidos.

## Datos sensibles y exclusiones

- `PasswordHash` y `Token` se registran con el valor `***`; el campo figura como modificado pero nunca
  se revela el hash. Una prueba por reflexión falla si aparece una propiedad `*Hash`, `*Token`,
  `*Secret` o `*Password` que no esté en la lista de enmascarado
  (`AuditoriaInterceptor.PropiedadesEnmascaradas`).
- No se auditan `AuditLog` (evita recursión), `RefreshToken` (rotación constante) ni `ChatMensaje`
  (contenido de usuario, alto volumen). La lista es `AuditoriaInterceptor.EntidadesExcluidas`.
- El intento de inicio de sesión fallido guarda el correo usado, **nunca la contraseña**, y la
  respuesta al cliente es la misma exista o no el correo.

## Atomicidad

El interceptor agrega la entrada al mismo contexto, así que se guarda en el **mismo `SaveChanges` y la
misma transacción** que el cambio: si la operación se revierte no queda rastro, y un reintento por
conflicto de concurrencia deja una sola entrada por cambio real. Los eventos de negocio usan
`Registrar(...)` (no guarda; viaja con el guardado del llamador). Solo los eventos de acceso usan
`RegistrarAsync`/`RegistrarAccesoAsync`, que guardan enseguida para persistir aunque la petición
termine en `401`.

## IP del cliente

Detrás de nginx la IP de conexión es la del proxy. La API procesa `X-Forwarded-For` y
`X-Forwarded-Proto` **solo** si la conexión directa viene de una red de confianza
(`ForwardedHeaders:RedesConfiables` en `appsettings`, por defecto loopback y redes privadas). Un cliente
externo que llegue directo a la API tiene una IP pública, y su encabezado se ignora.

## Consulta

`GET /api/v1/auditoria` (solo `Admin`, `403` para los demás) admite `entidad`, `usuarioId`, `accion`,
`tipo` (`Cambio` | `Evento`), `desde` y `hasta`, combinables y paginados con lo más reciente primero.
`GET /api/v1/auditoria/{id}` devuelve una entrada y `GET /api/v1/auditoria/entidades` las entidades con
registros. La bitácora es inmutable: no existe ninguna ruta de escritura.

En el panel de administración, **Analítica → Auditoría** muestra el listado con filtros (el estado
queda en la URL) y un detalle con la tabla campo / antes / después.

## Alcance

Fuera de alcance: retención o archivado, reversión de un cambio a partir del log, auditar lecturas y
detectar manipulación directa en la base de datos. Cada venta genera varias filas (venta, detalles,
pagos, comprobante, kardex, stock); los índices por usuario/fecha, entidad, acción y tipo mantienen
la consulta ágil.
