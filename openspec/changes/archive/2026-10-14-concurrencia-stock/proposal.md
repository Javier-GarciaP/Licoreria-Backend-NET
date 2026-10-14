# Proposal

## Why

En la discoteca varios mesoneros toman pedidos a la vez sobre el mismo inventario. Hoy `ServicioCuentas.AgregarComandaAsync` solo *verifica* el stock (`VerificarStockAsync`) pero no lo compromete: dos mesoneros pueden pedir la última Coca-Cola, ambos pasan la verificación y el fallo aparece tarde, cuando barra la sirve. Además, los conflictos que detecta el token `xmin` terminan como HTTP 500 genérico (el `ExceptionMiddleware` no mapea `DbUpdateConcurrencyException`), no se reintenta nada y varias operaciones siguen el patrón "consultar y luego insertar" sin respaldo en la base de datos. La defensa anterior lo dejó como limitación conocida; el docente pidió robustez de la lógica de negocio.

## What Changes

- **Reserva de stock al tomar el pedido**: `AgregarComandaAsync` reserva (`StockProducto.Reservar`, hoy sin uso) los insumos dentro de la misma transacción que crea la comanda. Servir consume la reserva y descuenta el kardex; cancelar, desalojar la mesa o cerrar el turno la liberan. Se agrega el campo `Reservado` por detalle de comanda para saber qué liberar.
- **Respuesta 409 con reintento acotado**: `ExceptionMiddleware` mapea `DbUpdateConcurrencyException` y violaciones de unicidad a 409 (RFC 7807). `UnitOfWork.EjecutarEnTransaccionAsync` reintenta un número acotado de veces (3) recargando y revalidando antes de rendirse; si el stock realmente se agotó, responde 422 con mensaje claro.
- **Red de seguridad en la BD**: restricciones `CHECK (cantidad >= 0)` y `CHECK (cantidad_reservada >= 0 AND cantidad_reservada <= cantidad)` en `StockProducto`.
- **Correlativo del comprobante**: reemplazar `COUNT + 1` por una secuencia de PostgreSQL y un índice único sobre `ComprobanteFiscal.Numero`.
- **Cierre de cuenta atómico**: `CerrarCuentaAsync` registra la venta y cierra la cuenta en una sola transacción; un segundo cierre simultáneo recibe 409. Los abonos concurrentes no pierden actualizaciones sobre `TotalAbonado`.
- **Exclusividad de mesas y reservas**: apertura de mesa protegida por índice único parcial (una cuenta abierta por mesa) y creación de reservas serializada por mesa con un bloqueo asesor de PostgreSQL, conservando `DetectorConflictosReserva` como autoridad de la regla.
- **Turno de caja**: un solo turno abierto garantizado por índice único parcial; el cierre de turno serializa contra las ventas en curso con bloqueo de fila.
- **Estado de comandas**: cambios simultáneos de estado sobre el mismo ítem (barra/cocina/mesonero) se resuelven con el token de concurrencia: gana uno, el otro recibe 409 con el estado actual.
- **Frontend**: el cliente HTTP trata 409 como conflicto recuperable (mensaje "se agotó mientras ordenabas" y refresco de stock/cuenta) en POS, mesonero y KDS.
- **Pruebas**: unitarias con Moq (conflicto simulado → reintento → 409) y de integración con dos tareas paralelas sobre la última unidad.

## Capabilities

### New Capabilities
- `inventory-stock-reservation`: reserva de insumos al tomar la comanda y su liberación o consumo (servir, cancelar, desalojar, cierre de turno), sin sobreventa.
- `concurrency-conflict-handling`: traducción uniforme de conflictos de concurrencia y unicidad a 409/422 (RFC 7807) con reintento acotado y mensajes recuperables en el cliente.
- `invoice-numbering`: numeración correlativa única de comprobantes bajo concurrencia.
- `account-operations-concurrency`: cierre de cuenta, abonos, apertura de mesa y cambios de estado de ítems seguros ante operaciones simultáneas.
- `table-reservation-exclusivity`: una mesa no puede quedar en dos reservas con intervalos solapados aunque se creen a la vez.
- `cash-shift-concurrency`: un único turno de caja abierto y cierre de turno consistente con las ventas en curso.

### Modified Capabilities
<!-- No hay specs principales todavía (openspec/specs está vacío); todo se introduce como capacidad nueva. -->

## Impact

- **Backend (Domain)**: `StockProducto` (invariantes de reserva), `ComandaDetalle` (marca `Reservado`).
- **Backend (Application)**: `ServicioCuentas`, `ServicioKardex`, `ServicioVentas`, `ServicioClub`, `ServicioCaja`, `IUnitOfWork`.
- **Backend (Infrastructure)**: `UnitOfWork` (reintento), `VentaRepository` (secuencia), repositorios de caja y reservas, configuraciones de EF, nueva migración (CHECK, índices únicos parciales, secuencia).
- **Backend (WebAPI)**: `ExceptionMiddleware` (nuevos mapeos 409/422).
- **Frontend**: `packages/api-client` (manejo de 409), POS, mesonero y KDS.
- **Pruebas**: `Licoreria.UnitTests` (nuevas) e `IntegrationTests` (concurrencia).
- **Base de datos**: la migración requiere datos limpios (sin comprobantes duplicados ni más de un turno abierto); se valida antes de aplicar. No hay cambios de contrato en endpoints, salvo los nuevos códigos 409.
- **Coordinación**: debe quedar listo antes de `auditoria-total`, porque la reserva y la liberación pasan por el interceptor de auditoría.
