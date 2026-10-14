# Design

## Context

Estado actual verificado en el código:

- `LicoreriaDbContext` mapea `xmin` como token de concurrencia en toda `BaseEntity`. Un conflicto lanza `DbUpdateConcurrencyException`, que `ExceptionMiddleware` no mapea y acaba en 500.
- `UnitOfWork.EjecutarEnTransaccionAsync` abre una transacción dentro de `CreateExecutionStrategy()`, pero no reintenta conflictos de versión y no es reentrante: `ServicioVentas.RegistrarVentaAsync` ya la usa, así que `CerrarCuentaAsync` no puede envolverla sin cambiar eso.
- `ServicioCuentas.AgregarComandaAsync` llama a `VerificarStockAsync` y guarda sin transacción ni reserva; `StockProducto.Reservar/LiberarReserva` existen pero nadie los usa.
- El descuento de kardex ocurre al servir (`CambiarEstadoItemAsync` → `AplicarKardexComandaAsync`). `CerrarCuentaAsync` pasa `YaDescontado` por ítem servido y cierra la cuenta en un `SaveChanges` posterior a la venta.
- El correlativo sale de `ContarComprobantesAsync() + 1`; no hay índice único sobre `Numero`.
- `AbrirSesionAsync` y `AbrirMesaAsync` consultan "¿ya hay una abierta?" y luego insertan; `Cuenta` solo tiene índice único por `SesionMesaId`.
- `CrearReservaAsync` consulta intervalos activos y luego inserta; el detector es una regla de dominio sin respaldo en la BD.

Motivación y alcance: ver proposal.md. Requisitos de comportamiento: ver specs/.

## Goals / Non-Goals

**Goals:**
- Ninguna combinación de operaciones simultáneas deja inventario, cuentas, comprobantes, reservas o turnos en estado inconsistente.
- El perdedor de una carrera recibe un error accionable (409/422), no un 500.
- Las garantías viven en la BD (restricciones) y no dependen de que cada servicio recuerde validar.

**Non-Goals:**
- Aislamiento `Serializable` global ni colas distribuidas.
- Multi-sucursal o múltiples instancias con caché de stock.
- Cambiar el momento en que el kardex descuenta (sigue siendo al servir).
- Cambiar los contratos de endpoints salvo los nuevos códigos 409.

## Decisions

**D1. Reserva como contador en `StockProducto` y marca en el ítem.**
`ComandaDetalle` gana `Reservado` (bool). Al crear la comanda se reserva cada insumo desglosado (recetas incluidas); al servir se resta de `Cantidad` y de `CantidadReservada` en el mismo guardado; al cancelar, desalojar o cerrar turno se libera lo reservado y aún no servido.
Disponible = `Cantidad - CantidadReservada`. `VerificarStockAsync` compara contra disponible.
*Alternativas:* tabla de reservas con vencimiento (más precisa, pero añade un job de limpieza y no aporta aquí: el ciclo de vida del ítem ya dicta cuándo liberar); no reservar y solo reintentar (el aviso al mesonero llegaría tarde, descartado en la exploración).
Los productos de Cocina siguen sin stock ni reserva (`DesglosarInsumosAsync` devuelve vacío).

**D2. Concurrencia optimista con reintento acotado como mecanismo principal.**
Se conserva `xmin`. `EjecutarEnTransaccionAsync` captura `DbUpdateConcurrencyException`, hace `ChangeTracker.Clear()` y vuelve a ejecutar la operación hasta 3 veces (el delegado debe releer todo en cada intento). Agotado, relanza como `ConflictoException`.
*Alternativas:* `UPDATE ... WHERE cantidad - reservado >= @n` atómico (sin carrera, pero esquiva el modelo de entidades y el interceptor de auditoría); `SELECT ... FOR UPDATE` por insumo (serializa siempre aunque no haya contención). Se reevalúa solo si las pruebas de carga muestran contención real.

**D3. `EjecutarEnTransaccionAsync` reentrante.**
Si ya hay una transacción activa (`Database.CurrentTransaction`), ejecuta la operación dentro de ella sin abrir otra ni reintentar (el reintento lo hace la capa exterior). Esto permite que `CerrarCuentaAsync` envuelva `RegistrarVentaAsync` y el cierre de la cuenta en una sola transacción, y que `AgregarComandaAsync` y `CambiarEstadoItemAsync` también sean transaccionales.

**D4. Cierre de cuenta: venta y estado en una transacción.**
Al cerrar: liberar reservas de ítems no servidos, registrar la venta (que descuenta los no servidos y consume `YaDescontado` de los servidos) y marcar `Cuenta` cerrada, todo en una transacción. El segundo cierre simultáneo choca por `xmin` de `Cuenta` y, tras releer, ve la cuenta cerrada. Se añade además un índice único parcial sobre `Venta.CuentaId` para ventas no anuladas como red de seguridad.

**D5. Errores en el middleware.**
`DbUpdateConcurrencyException` y `DbUpdateException` por violación de unicidad (SQLSTATE `23505`) → 409 `application/problem+json`. Violación de CHECK (`23514`) → 422. `ConflictoException` y `ReglaNegocioException` conservan su mapeo. Se agrega a las excepciones de dominio un tipo `ExistenciaAgotadaException : ReglaNegocioException` con producto, requerido y disponible, para que el cliente distinga agotado (422) de carrera (409).

**D6. Restricciones en la BD (migración).**
- `CHECK (cantidad >= 0)`, `CHECK (cantidad_reservada >= 0)`, `CHECK (cantidad_reservada <= cantidad)` en `StockProducto`.
- Secuencia `comprobante_numero_seq` y `UNIQUE (numero)` en `ComprobanteFiscal`; `ServicioVentas` obtiene `nextval` en vez de contar. Los saltos son aceptables (ver spec).
- Índice único parcial: una `SesionMesa` abierta por `MesaId` (`WHERE cerrada_en IS NULL AND NOT is_deleted`).
- Índice único parcial: una `SesionCaja` abierta (`WHERE cerrada_en IS NULL AND NOT is_deleted`).
La migración incluye una verificación previa que falla con un mensaje claro si hay datos que violan las restricciones.

**D7. Reservas de mesa: bloqueo asesor por mesa.**
`CrearReservaAsync` corre en transacción y toma `pg_advisory_xact_lock` por cada mesa solicitada, en orden ascendente de id para evitar interbloqueos, antes de consultar intervalos y llamar a `DetectorConflictosReserva`. La regla sigue en el dominio.
*Alternativa:* restricción `EXCLUDE USING gist` sobre mesa e intervalo. Se descarta porque la fecha vive en `Reserva` y la mesa en `ReservaMesa`: exigiría duplicar fecha, duración y estado en `ReservaMesa` y mantenerlos sincronizados.

**D8. Turno de caja vs ventas en curso.**
`RegistrarVentaAsync` y los demás movimientos de caja leen la sesión abierta con `FOR SHARE` dentro de su transacción; `CerrarSesionAsync` la lee con `FOR UPDATE`. El cierre espera a las ventas en curso y las ventas posteriores ven la sesión cerrada. Implementación vía consulta SQL en `SesionCajaRepository`.

**D9. Estado de ítems.**
`CambiarEstadoItemAsync` pasa a transaccional. Dos cambios simultáneos sobre el mismo `ComandaDetalle` chocan por `xmin`; el que pierde, tras recargar, o bien repite una transición ya aplicada (no hace nada) o recibe 422 por transición no válida; si agota reintentos, 409.

**D10. Cliente.**
`packages/api-client` distingue `ApiError.status === 409` y `422` de existencia agotada; POS, mesonero y KDS muestran el mensaje, invalidan las consultas de cuenta, stock y mesas, y conservan el borrador del pedido.

## Risks / Trade-offs

- [Reservas huérfanas si un camino de salida olvida liberar] → Una función única de liberación usada por cancelar, desalojar, cerrar cuenta y cerrar turno; prueba que verifica `CantidadReservada = 0` al cerrar el turno; consulta de diagnóstico en el kardex.
- [Contención en productos muy pedidos produce reintentos] → Límite de 3 intentos y respuesta 409 clara; medir en la prueba de carga antes de pasar a `UPDATE` atómico.
- [Secuencia genera saltos de numeración fiscal] → Aceptado y documentado; no hay duplicados. Si la normativa exigiera continuidad estricta se reabre la decisión.
- [Reintento reejecuta efectos secundarios (notificaciones SignalR, auditoría)] → Las notificaciones se emiten después de confirmar la transacción; la auditoría se guarda en la misma transacción (coordinar con `auditoria-total`).
- [Migración falla en una BD con datos que ya violan restricciones] → Verificación previa con mensaje accionable y script de corrección documentado.
- [Bloqueo asesor mal ordenado causa interbloqueo] → Orden determinista de mesas y prueba con reservas multi-mesa cruzadas.

## Migration Plan

1. Respaldo de la BD de demo; ejecutar la verificación previa de datos.
2. Aplicar la migración (CHECK, índices, secuencia inicializada con el máximo número existente).
3. Desplegar backend y frontend juntos: el frontend nuevo es compatible con el backend antiguo, pero el 409 solo existe tras el despliegue del backend.
4. Rollback: revertir la migración (elimina restricciones, índices y secuencia); el código previo sigue funcionando porque `Reservado` e índices son aditivos.
