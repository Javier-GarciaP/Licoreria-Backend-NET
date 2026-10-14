# Tasks

## 1. Infraestructura de concurrencia

- [x] 1.1 Añadir `ExistenciaAgotadaException` (producto, requerido, disponible) a `Domain/Common/ExcepcionesDominio.cs`; verificar con una prueba unitaria que hereda de `ReglaNegocioException` y expone sus datos
- [x] 1.2 Hacer `UnitOfWork.EjecutarEnTransaccionAsync` reentrante (si hay `CurrentTransaction`, ejecuta sin abrir otra); verificar con una prueba que dos llamadas anidadas comparten transacción y que un fallo interno revierte todo
- [x] 1.3 Añadir reintento acotado (3 intentos, `ChangeTracker.Clear()` entre intentos) ante `DbUpdateConcurrencyException`, relanzando `ConflictoException` al agotarse; verificar con pruebas Moq/EF en memoria: conflicto único se recupera, conflicto persistente lanza `ConflictoException`
- [x] 1.4 Mapear en `ExceptionMiddleware` `DbUpdateConcurrencyException` y SQLSTATE `23505` a 409 y `23514` a 422, en RFC 7807 sin detalle técnico; verificar con pruebas del middleware (status, `application/problem+json`, mensaje)

## 2. Reserva de stock

- [x] 2.1 Añadir `Reservado` a `ComandaDetalle` y exponer en `StockProducto` la propiedad `Disponible` y las invariantes de `Reservar/LiberarReserva/ConsumirReserva`; verificar con pruebas de dominio (reserva excedida, liberación mayor que lo reservado, consumo)
- [x] 2.2 Añadir en `ServicioKardex` operaciones de reservar, liberar y consumir por insumos desglosados (recetas incluidas) y hacer que `VerificarStockAsync` use disponible; verificar con pruebas Moq (receta con varios insumos, producto de Cocina sin reserva, existencia agotada lanza `ExistenciaAgotadaException`)
- [x] 2.3 Hacer `AgregarComandaAsync` transaccional y reservar al crear la comanda (marcando `Reservado`); verificar con prueba Moq que una comanda con un ítem sin existencia no reserva nada
- [x] 2.4 Consumir la reserva al servir en `CambiarEstadoItemAsync` y liberarla al cancelar, sin doble descuento; verificar con pruebas Moq (servir, cancelar, servir dos veces)
- [x] 2.5 Liberar reservas al desalojar mesa, cerrar cuenta y cerrar turno mediante una única función de liberación; verificar con pruebas Moq que `CantidadReservada` vuelve a 0 en los tres caminos
- [x] 2.6 Hacer que `RegistrarVentaAsync` y `CerrarCuentaAsync` descuenten sin chocar con la reserva propia de los ítems de la cuenta; verificar con una prueba que cerrar una cuenta con ítems reservados sin servir deja existencia y reserva correctas

## 3. Restricciones, correlativo y exclusividad en la base de datos

- [x] 3.1 Crear la migración con `CHECK` de `StockProducto`, secuencia `comprobante_numero_seq` (inicializada con el máximo actual), `UNIQUE (numero)` en `ComprobanteFiscal` e índices únicos parciales de mesa abierta, turno abierto y venta por cuenta; verificar aplicándola sobre una BD con datos de demo y con `database` vacía
- [x] 3.2 Añadir en la migración la verificación previa de datos (duplicados, turnos abiertos múltiples, cantidades negativas) con mensaje accionable; verificar sobre una BD con datos que violan cada restricción
- [x] 3.3 Reemplazar `ContarComprobantesAsync() + 1` por la secuencia en `VentaRepository` y `ServicioVentas`; verificar con prueba unitaria (Moq del repositorio) y de integración con dos ventas simultáneas que obtienen números distintos
- [x] 3.4 Documentar las restricciones nuevas en `database/README.md` y en la lista de limitaciones de `docs/dev/10-fases/fase-4.md`; verificar que el documento ya no lista el 500 por concurrencia ni la falta de CHECK

## 4. Cuentas, mesas, reservas y turno de caja

- [x] 4.1 Hacer atómico `CerrarCuentaAsync` (liberar reservas, registrar venta y cerrar cuenta en una transacción); verificar con prueba Moq de que un fallo de la venta deja la cuenta abierta y con prueba de integración de dos cierres simultáneos (una venta, un 409)
- [x] 4.2 Proteger `AbrirMesaAsync` con el índice único parcial y traducir la violación a `ConflictoException`; verificar con prueba de integración de dos aperturas simultáneas de la misma mesa
- [x] 4.3 Hacer transaccionales `RegistrarAbonoAsync` y `CambiarEstadoItemAsync` con reintento; verificar con pruebas de integración de dos abonos simultáneos (suma correcta) y de dos cambios de estado sobre el mismo ítem (un solo descuento)
- [x] 4.4 Serializar `CrearReservaAsync` con `pg_advisory_xact_lock` por mesa en orden determinista; verificar con una prueba de integración de dos reservas simultáneas de la misma mesa y otra con reservas multi-mesa cruzadas sin interbloqueo
- [x] 4.5 Implementar lectura `FOR SHARE` de la sesión en ventas y `FOR UPDATE` en el cierre de turno, y traducir la violación del índice de turno abierto a `ConflictoException`; verificar con pruebas de integración (aperturas simultáneas, venta durante el cierre, cierre repetido)

## 5. Frontend

- [x] 5.1 Tratar 409 y 422 de existencia agotada en `packages/api-client` (propiedades en `ApiError` para distinguirlos); verificar con una prueba Vitest sobre `apiFetch` con respuestas simuladas por msw
- [x] 5.2 Mostrar el mensaje, invalidar las consultas de cuenta, stock y mesas y conservar el borrador del pedido en POS, mesonero y KDS; verificar con pruebas de `PosPage` y de la vista del mesonero que simulan un 409
- [x] 5.3 Mostrar la existencia disponible (no solo la total) en la carta del mesonero para anticipar agotados; verificar visualmente con el stack levantado

## 6. Verificación integral

- [x] 6.1 Prueba de integración "última Coca-Cola": dos mesoneros piden la última unidad en paralelo y exactamente uno la obtiene; verificar con `dotnet test` contra PostgreSQL
- [x] 6.2 Ejecutar la batería de cierre (`dotnet build`, `dotnet test`, `npm run typecheck`, `npm run lint`, `npm test`, `npm run build`) y confirmar que todo queda en verde
- [x] 6.3 Ejecutar la demo manual con el stack en Docker: dos sesiones del navegador pidiendo la última unidad y comprobar el mensaje del perdedor y la existencia final

## Workflow follow-up

- Revisar los artefactos antes de implementar con `/opsx:apply`.
- Coordinar con `auditoria-total`: la reserva y la liberación deben quedar auditadas en la misma transacción.
- Archivar el cambio con `/opsx:archive` cuando las pruebas estén en verde.
