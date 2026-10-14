# Tasks

## 1. Modelo de datos de la bitácora

- [x] 1.1 Añadir `Tipo` (`Cambio` | `Evento`) a `AuditLog` y a `AuditLogDto`, y actualizar `AuditLogConfiguration` con los índices `(usuario_id, created_at)`, `(entidad, entidad_id)`, `(accion)` y `(tipo)`; verificar que el proyecto compila y que `AuditLogDto` expone `Tipo`
- [x] 1.2 Crear la migración con la columna `tipo` (valor por defecto `Cambio`), backfill de las filas existentes como `Evento` y los índices nuevos; verificar aplicándola sobre una BD con filas de auditoría previas y comprobando que quedan como `Evento`

## 2. Captura automática

- [x] 2.1 Añadir el paquete `Microsoft.EntityFrameworkCore.InMemory` a `Licoreria.UnitTests` y crear un arnés de pruebas con el interceptor y un `IContextoUsuario` simulado con Moq; verificar con una prueba trivial que se puede guardar una entidad con el interceptor activo
- [x] 2.2 Ampliar `AuditoriaInterceptor` para generar un `AuditLog` por cada alta, edición y borrado (clasificando `eliminar` y `restaurar` según `IsDeleted`), con usuario, correo, IP y `Tipo = Cambio`; verificar con pruebas de alta, edición, borrado lógico y borrado físico
- [x] 2.3 Generar el detalle antes/después en `Datos` (solo campos cambiados en ediciones, ignorando `LastModifiedAt`, `UpdatedBy`, `CreatedAt` y `xmin`) y omitir la entrada si no hay cambios reales; verificar con pruebas de edición de un precio (10 → 12) y de guardado sin cambios
- [x] 2.4 Enmascarar `PasswordHash` y `Token`, y excluir `AuditLog`, `RefreshToken` y `ChatMensaje`; verificar con pruebas de cambio de contraseña (sin hash en `Datos`), de no recursión y una prueba por reflexión que falla si una propiedad `*Hash`, `*Token`, `*Secret` o `*Password` no está en la lista de enmascarado
- [x] 2.5 Verificar atomicidad con una prueba de integración: una transacción revertida no deja entradas, y un conflicto de concurrencia con reintento deja una sola entrada por cambio; ejecutar `dotnet test` contra PostgreSQL

## 3. IP real del cliente

- [x] 3.1 Registrar `UseForwardedHeaders` en `Program.cs` con redes confiables configurables en `appsettings`; verificar con una prueba de integración que una petición con `X-Forwarded-For` desde una red privada registra la IP del cliente y que desde una IP pública el encabezado se ignora
- [x] 3.2 Probar manualmente a través de nginx del stack Docker y comprobar que la entrada de auditoría muestra la IP del cliente y no la del proxy

## 4. Eventos de negocio y de acceso

- [x] 4.1 Añadir `Registrar` (sin guardar) y mantener `RegistrarAsync` (guarda) en `IServicioAuditoria` con `Tipo = Evento`; verificar con pruebas Moq de que `Registrar` no llama a `SaveChanges` y `RegistrarAsync` sí
- [x] 4.2 Mover las llamadas de caja, devolución, recepción, merma, toma física, cambio de contraseña y revocación de sesiones a `Registrar` antes del `SaveChanges` del cambio, y eliminar las de crear, editar y eliminar usuario; verificar con pruebas Moq por servicio de que el evento se registra y el guardado ocurre una sola vez
- [x] 4.3 Registrar eventos de inicio de sesión, cierre de sesión e intento fallido en `ServicioAutenticacion` con `RegistrarAsync`, sin contraseña ni revelar si el correo existe; verificar con pruebas Moq de los tres casos y con una prueba de integración de que el intento fallido deja entrada aunque la respuesta sea 401

## 5. API de consulta

- [x] 5.1 Añadir los filtros `accion` y `tipo`, el endpoint `GET /api/v1/auditoria/{id}` y `GET /api/v1/auditoria/entidades` en `AuditoriaController`, `ServicioAuditoria` y `AuditoriaRepository`; verificar con pruebas Moq del servicio y con pruebas de integración de filtros combinados, detalle y 403 para un cajero
- [x] 5.2 Actualizar `openapi/licoreria.yaml` con los nuevos parámetros y rutas; verificar con `task backend:openapi:check`
- [x] 5.3 Verificar que no existe ninguna ruta de escritura sobre la bitácora (prueba de integración que espera 404 o 405 en `PUT` y `DELETE`)

## 6. Frontend

- [x] 6.1 Actualizar `packages/types` (`AuditLog.tipo`, detalle) y `auditoriaApi` (`desde`, `hasta`, `accion`, `tipo`, `obtener`, `entidades`); verificar con `npm run typecheck`
- [x] 6.2 Reescribir `AuditoriaPage` con filtros por usuario, entidad, acción, tipo y rango de fechas (estado en la URL), tabla paginada con fila clicable y modal de detalle antes/después; verificar con pruebas Vitest del filtrado, el estado vacío y el detalle de una edición
- [x] 6.3 Quitar `oculto` del grupo `analitica` y del ítem `/auditoria` en `lib/navigation.tsx`, dejando `/reportes` oculto; verificar con la prueba de navegación existente ampliada (el administrador ve Auditoría, el cajero no)
- [x] 6.4 Verificar visualmente con el stack levantado: crear, editar y eliminar un producto y comprobar que los tres movimientos aparecen con su antes/después

## 7. Documentación y cierre

- [x] 7.1 Documentar la bitácora (qué se registra, exclusiones, enmascarado, IP) en `docs/` y registrar el cambio en `CHANGELOG.md`; verificar que el sitio de documentación compila (`task docs:build`)
- [x] 7.2 Ejecutar la batería de cierre (`dotnet build`, `dotnet test`, `npm run typecheck`, `npm run lint`, `npm test`, `npm run build`) y confirmar que todo queda en verde

## Workflow follow-up

- Revisar los artefactos antes de implementar con `/opsx:apply`.
- Implementar después de `concurrencia-stock`: la atomicidad con reintentos depende de su `UnitOfWork`.
- Coordinar la edición de `lib/navigation.tsx` con `reportes-kpis-export`.
- Archivar el cambio con `/opsx:archive` cuando las pruebas estén en verde.
