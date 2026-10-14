# Design

## Context

Estado actual verificado en el código:

- `AuditoriaInterceptor` (`Infrastructure/Persistence/Interceptors`) solo completa `CreatedBy/UpdatedBy` y fechas en `SavingChanges`. Depende de `IContextoUsuario` y `IRelojSistema`.
- `AuditLog` (`Entities/AuditLog.cs`, tabla `audit_log`) tiene `UsuarioId`, `Usuario`, `Accion`, `Entidad`, `EntidadId`, `Datos` (jsonb) e `Ip`, e índices por `CreatedAt` y `Entidad`. Hereda de `BaseEntity`, por lo que también recibe `xmin`.
- `ServicioAuditoria.RegistrarAsync` crea el log y llama a `SaveChanges` por su cuenta. Hay 12 llamadas: `ServicioCaja` (abrir/cerrar), `ServicioUsuarios` (crear, editar, eliminar, cambiar contraseña, revocar sesiones), `ServicioInventario` (merma, toma física), `ServicioVentas` (venta, devolución) y `ServicioCompras` (recepción). Casi todas se ejecutan después del `SaveChanges` del cambio, es decir, fuera de su transacción.
- `ContextoUsuarioHttp.Ip` usa `Connection.RemoteIpAddress`. El proxy `infra/nginx/nginx.conf` envía `X-Forwarded-For`, pero `Program.cs` no registra `UseForwardedHeaders`, así que detrás de nginx la IP es la del proxy.
- `Usuario.PasswordHash` y `RefreshToken.Token` son datos sensibles dentro de entidades auditables.
- La vista `AuditoriaPage` solo tiene un filtro de texto por entidad, no muestra `Datos`, y `auditoriaApi.listar` no envía fechas aunque el backend las soporta. El grupo `analitica` y el ítem `/auditoria` tienen `oculto: true` en `lib/navigation.tsx`.
- El controlador `AuditoriaController` ya exige `Roles = "Admin"`.

Motivación y alcance: ver proposal.md. Comportamiento exigido: ver specs/.

## Goals / Non-Goals

**Goals:**
- Que ningún cambio de datos escape de la bitácora por omisión de quien programa un servicio nuevo.
- Que el registro y el cambio sean atómicos.
- Que el Administrador pueda responder "¿quién cambió esto y qué había antes?" desde la interfaz.

**Non-Goals:**
- Política de retención, archivado o exportación de la bitácora.
- Recuperación o reversión automática de un cambio a partir del log.
- Auditar lecturas (consultas) de datos.
- Firma o cadena de hashes para detectar manipulación directa en la base de datos.

## Decisions

**D1. Captura dentro de `SavingChanges`.**
El interceptor recorre `ChangeTracker.Entries<BaseEntity>()` (copiando la lista antes de agregar) y, por cada entrada `Added`, `Modified` o `Deleted` de un tipo no excluido, agrega un `AuditLog` al mismo contexto. Así se guarda en el mismo `SaveChanges` y la misma transacción. El `Id` de toda entidad se asigna en el constructor (`Guid.NewGuid()`), por lo que el identificador del alta ya es conocido antes de guardar.
*Alternativas:* una tabla de disparadores en PostgreSQL (captura también cambios fuera de la API, pero no conoce al usuario de la aplicación ni la IP sin parámetros de sesión); `ServicioAuditoria` por cada servicio (el mecanismo actual, descartado por omisiones y por no ser atómico).

**D2. Clasificación de la acción.**
`Added` → `crear`. `Modified` donde `IsDeleted` pasa de falso a verdadero → `eliminar`; de verdadero a falso → `restaurar`; en otro caso → `editar`. `Deleted` (borrado físico) → `eliminar`. Si en una edición solo cambian los campos de bookkeeping (`LastModifiedAt`, `UpdatedBy`, `xmin`, `CreatedAt`), no se genera entrada.

**D3. Formato del detalle en `Datos` (jsonb).**
Solo propiedades escalares de la entidad (no navegaciones): `crear` → `{"despues": {...}}`; `editar` → `{"antes": {...}, "despues": {...}}` solo con los campos cambiados; `eliminar` → `{"antes": {...}}`. Enums como texto, fechas ISO 8601, decimales sin pérdida. El mismo formato sirve para la vista y evita nuevas tablas.
*Alternativa:* una fila por campo cambiado (consulta más fácil por campo, pero multiplica el volumen). Se descarta.

**D4. Enmascarado de datos sensibles.**
Una lista fija de nombres de propiedad (`PasswordHash`, `Token`) se registra con el valor `"***"` y se marca como cambiada si difiere. Está en un único lugar del interceptor, con una prueba que falla si un campo de la lista aparece con su valor real.

**D5. Exclusiones.**
No se audita `AuditLog` (evita recursión), `RefreshToken` (rotación constante) ni `ChatMensaje` (contenido de usuario, alto volumen). La lista es una constante de la infraestructura, fácil de ampliar.
Las migraciones y `HasData` no pasan por el interceptor; cualquier cambio sin usuario autenticado se registra con usuario nulo y correo `sistema`.

**D6. Separar cambio de evento con `Tipo`.**
`AuditLog` gana `Tipo` (`Cambio` | `Evento`). Las entradas automáticas son `Cambio`; las que escribe `ServicioAuditoria` son `Evento`. La migración rellena `Evento` en las filas existentes (todas manuales) y define `Cambio` como valor por defecto.
Los eventos de negocio se conservan; `crear/editar/eliminar usuario` se eliminan porque el cambio automático los cubre completamente (spec: sin duplicados). Venta, merma, toma física y recepción se conservan como evento porque describen un hecho de negocio con totales que el alta de una fila no refleja.

**D7. Dos formas de registrar eventos.**
`ServicioAuditoria` expone `Registrar(...)` (agrega al contexto sin guardar, para que viaje con el `SaveChanges` del llamador) y `RegistrarAsync(...)` (guarda enseguida, para eventos sin transacción de negocio como el inicio de sesión y el intento fallido, que deben persistir aunque la petición termine en 401). Las llamadas existentes pasan a `Registrar` y se mueven antes del `SaveChanges` del cambio.

**D8. IP real.**
Se registra `UseForwardedHeaders` con `ForwardedHeaders.XForwardedFor | XForwardedProto` antes de la autenticación, confiando solo en las redes privadas (loopback, 10/8, 172.16/12, 192.168/16), configurables en `appsettings`. El puerto de la API también se publica directamente, pero un cliente externo viene de una IP pública que no es de confianza, y su encabezado se ignora.

**D9. API de consulta.**
`GET /api/v1/auditoria` añade filtros `accion` y `tipo` a los existentes (`entidad`, `usuarioId`, `desde`, `hasta`); `GET /api/v1/auditoria/{id}` devuelve la entrada completa; `GET /api/v1/auditoria/entidades` devuelve las entidades con registros para poblar el filtro. Se agregan índices `(usuario_id, created_at)`, `(entidad, entidad_id)`, `(accion)` y `(tipo)`. `AuditLogDto` agrega `Tipo`. No hay rutas de escritura.

**D10. Vista.**
`AuditoriaPage` usa los componentes de filtro de `@licoreria/ui` (`FiltroDropdown`, `FiltroFechas`, `LimpiarFiltros`) con el estado en la URL, una tabla con la fila clicable y un modal de detalle con una tabla campo / antes / después. Se reactiva el grupo `analitica` y el ítem `/auditoria` quitando su `oculto`; `/reportes` sigue oculto hasta su propio cambio.

## Risks / Trade-offs

- [Volumen: una venta genera varias filas (venta, detalles, pagos, comprobante, kardex, stock)] → Índices adecuados, paginación y límite de tamaño de página; la retención queda como trabajo futuro documentado.
- [Costo extra por cada `SaveChanges`] → La captura solo recorre entradas ya rastreadas; se mide con una prueba que guarda un lote grande y se compara con el tiempo base.
- [Un campo sensible nuevo no está en la lista de enmascarado] → Prueba por reflexión que recorre las entidades buscando propiedades llamadas `*Hash`, `*Token`, `*Secret` o `*Password` y exige que estén en la lista.
- [Reordenar `Registrar` antes del `SaveChanges` en 7 servicios puede olvidarse y perder el evento] → Pruebas Moq por servicio que verifican que el evento se registró y que se guardó una sola vez.
- [`X-Forwarded-For` falsificado] → Solo se confía en redes privadas; ver D8.
- [Las entradas de auditoría crecen con la concurrencia: un reintento reejecuta el interceptor] → El reintento descarta el seguimiento previo (`ChangeTracker.Clear()` de `concurrencia-stock`), por lo que no se duplican; hay una prueba que lo verifica.

## Migration Plan

1. Aplicar la migración (columna `tipo`, índices, backfill de filas existentes como `Evento`).
2. Desplegar backend y frontend juntos: el frontend nuevo pide filtros que el backend antiguo ignoraría.
3. Rollback: revertir la migración elimina columna e índices; el código previo sigue leyendo `audit_log` sin `tipo`.
