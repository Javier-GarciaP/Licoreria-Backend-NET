# Proposal

## Why

El docente pidió un control completo de lo que ocurre en el sistema: quién editó qué y que todo lo que se registre, edite, agregue o elimine quede trazado. Hoy `AuditoriaInterceptor` solo rellena `CreatedBy/UpdatedBy`, y el `AuditLog` se alimenta con 12 llamadas manuales a `ServicioAuditoria.RegistrarAsync` (caja, usuarios, mermas, ventas, compras). Cubren una fracción mínima de las entidades (productos, precios, planos, reservas, promociones y tasas no dejan rastro), dependen de que cada servicio se acuerde de llamarlas, guardan sin los valores anteriores y se escriben en un `SaveChanges` aparte, fuera de la transacción del cambio. Además la vista `/auditoria` está oculta, solo filtra por entidad y no muestra el detalle, y la IP registrada sería la del proxy nginx porque la API no procesa `X-Forwarded-For`.

## What Changes

- **Captura automática de cambios**: el interceptor de EF registra un `AuditLog` por cada alta, edición y borrado (lógico o físico) de cualquier entidad auditable, con usuario, correo, IP, entidad, id, acción y los valores antes/después, dentro del mismo `SaveChanges` y por tanto de la misma transacción. Una edición guarda solo los campos que cambiaron.
- **Protección de datos sensibles**: `PasswordHash` y tokens nunca se guardan en el log (se enmascaran); `AuditLog`, `RefreshToken` y `ChatMensaje` quedan fuera de la auditoría automática.
- **Eventos de negocio y de acceso**: se conservan los eventos con semántica propia (abrir/cerrar caja, devolución, recepción de compra, merma, toma física, cambio de contraseña, revocar sesiones) y se agregan inicio de sesión, cierre de sesión e intento fallido. Se distinguen de los cambios automáticos con un campo `Tipo`; se eliminan las llamadas manuales que solo duplican un alta/edición/baja ya cubierta.
- **IP real del cliente**: la API procesa `X-Forwarded-For` desde el proxy de confianza para registrar la IP de origen.
- **Vista `/auditoria` reactivada**: visible en la navegación para el Administrador, con filtros por usuario, entidad, acción, tipo y rango de fechas, y un detalle que muestra el antes/después por campo. Se agrega el endpoint de detalle y los filtros en el listado.
- **Frontend**: `auditoriaApi` y los tipos aceptan los nuevos filtros y campos.
- **Pruebas**: unitarias del interceptor y del enmascarado, e integración del log dentro de la transacción (un rollback no deja registros).

## Capabilities

### New Capabilities
- `audit-change-capture`: registro automático, atómico y sin datos sensibles de cada alta, edición y borrado con usuario, IP y valores antes/después.
- `audit-business-events`: eventos de negocio y de acceso registrados de forma explícita y distinguibles de los cambios de datos.
- `audit-log-viewer`: consulta de la bitácora por el Administrador, con filtros, paginación y detalle antes/después.

### Modified Capabilities
<!-- No hay specs principales todavía (openspec/specs está vacío). -->

## Impact

- **Backend (Domain)**: `AuditLog` gana `Tipo` y los campos de cambios; marca de exclusión de entidades.
- **Backend (Application)**: `ServicioAuditoria` (filtros, detalle, eventos de acceso), servicios que hoy llaman `RegistrarAsync` (se podan los duplicados), `ServicioAutenticacion`.
- **Backend (Infrastructure)**: `AuditoriaInterceptor` (captura), `AuditLogConfiguration`, `AuditoriaRepository`, nueva migración (columnas e índices).
- **Backend (WebAPI)**: `Program.cs` (`UseForwardedHeaders`), `AuditoriaController` (filtros y detalle).
- **Frontend**: `AuditoriaPage`, `packages/api-client` (`auditoriaApi`), `packages/types` (`AuditLog`), `lib/navigation.tsx` (reactivar grupo e ítem).
- **Rendimiento y almacenamiento**: cada venta genera varias filas de auditoría; se agregan índices y paginación. La retención queda fuera de alcance.
- **Coordinación**: depende de `concurrencia-stock` (reintentos y transacciones) y comparte `lib/navigation.tsx` con `reportes-kpis-export`, que reactivará `/reportes` en el mismo grupo.
