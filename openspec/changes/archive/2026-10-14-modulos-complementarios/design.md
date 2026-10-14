# Design

## Context

Estado actual verificado en el código:

- `lib/navigation.tsx` marca `oculto: true` en Promociones, Existencias, Entradas, Lista VIP, el grupo Clientes (Clientes y Cuentas por cobrar), lotes, tomas físicas, el grupo Contenido y el grupo Analítica. El grupo Compras no está oculto y es solo para Administrador. El grupo Salón es solo de modo Discoteca. `puedeAcceder` deja pasar al administrador aunque el ítem esté oculto, pero no a otros roles.
- Las páginas de los módulos existen (`PromocionesPage`, `ClientesPage`, `CuentasPorCobrarPage`, `InventarioPage`, `EntradasPage`, `ListaVipPage`, `ComprasPage` y sus hermanas); solo `Mermas`, `Pos`, `Usuarios`, `Login` y `NotFound` tienen pruebas de página.
- `RolesSeguridad` no da al Cajero `crm:read` ni `crm:write`, pero la navegación muestra Clientes al Cajero. `PromocionesController` exige `ventas:write` para crear, editar y eliminar, y ese permiso lo tienen Cajero, Mesero, Barra y Cocina. El POS ya consulta las promociones (`promocionesApi.listar`) y envía `promocionId`.
- `Venta` no tiene cliente. `SesionMesa.Cliente` es un texto libre. `Cliente.Puntos` y `CuentaPorCobrar` se manejan solo por endpoints manuales (`puntos/acumular`, `cuentas-por-cobrar`), sin relación con ventas. `TipoMovimientoPuntos` tiene solo `Acumulacion` y `Canje`. `EstadoCuentaPorCobrar.Vencida` existe pero nada lo asigna.
- Existe el método de pago `Crédito` (`MetodoPago.Codigo = "Credito"`) en la siembra. `ServicioVentas.CrearVentaAsync` solo exige que los pagos cubran el total, así que un pago en Crédito se acepta como dinero cobrado sin generar deuda. El POS envía un único pago con `metodos.data?.[0]`.
- `ServicioVentas.RegistrarVentaAsync` envuelve la creación en `EjecutarEnTransaccionAsync`; `CerrarCuentaAsync` arma un `RegistrarVentaDto` desde los ítems y los abonos (que pasan a ser pagos). `RegistrarDevolucionAsync` guarda por separado y registra auditoría.
- El Cajero, al cerrar caja, calcula el esperado solo con el fondo y los movimientos manuales; las ventas no alimentan ese esperado (hallazgo ajeno a este cambio).

Motivación y alcance: ver proposal.md. Comportamiento exigido: ver specs/.

## Goals / Non-Goals

**Goals:**
- Que cada módulo reactivado funcione de punta a punta para los roles que lo ven.
- Que cliente, puntos y fiado dependan de la venta de forma atómica, sin pasos manuales.
- Que las reglas nuevas se prueben con Moq sin base de datos.

**Non-Goals:**
- Canje de puntos como descuento en la venta (sigue siendo el endpoint manual).
- Límite de crédito por cliente, intereses, recordatorios o cobranza automática.
- Ajuste automático de la cuenta por cobrar en devoluciones.
- Registrar los cobros de cuentas por cobrar como ingresos de caja, y corregir el cálculo del esperado de caja.
- Reactivar lotes, tomas físicas y contenido.
- Unificar `SesionMesa.Cliente` (texto) con el cliente del CRM.

## Decisions

**D1. Navegación: quitar `oculto` y fijar modo y rol por ítem.**
Se quita `oculto` de Promociones, Existencias, Entradas, Lista VIP y del grupo Clientes (sus ítems ya no lo tienen). Promociones, Existencias, Clientes, Cuentas por cobrar y Compras no declaran `modos` (ambos modos). Entradas y Lista VIP heredan `discoteca` del grupo Salón. El ítem Promociones pasa a `roles: ADMIN` porque la gestión es del Administrador; el POS sigue leyendo promociones. Se amplía `navigation.test.ts` con una matriz modo × rol × módulo.
*Alternativa:* dejar Promociones para CAJA con escritura reservada al Administrador (mostraría una pantalla con botones que fallan). Se descarta.

**D2. Permisos.**
`Permisos.PromocionesGestionar = "promotions:manage"` ("Crear, editar y eliminar promociones"), solo para Administrador; `PromocionesController` lo exige en `POST`, `PUT` y `DELETE`, y `VentasLeer` sigue protegiendo el `GET`. El Cajero suma `CrmLeer` y `CrmEscribir`. Como en `croquis-planos`, los permisos viajan en el JWT: las sesiones abiertas se actualizan al renovar el token (60 minutos).

**D3. Modelo de datos.**
- `Venta.ClienteId` (Guid?, FK a `Cliente`, índice) y `ClienteId` opcional en `RegistrarVentaDto` y `CerrarCuentaDto`.
- `CuentaPorCobrar.VentaId` (Guid?, índice único filtrado `WHERE venta_id IS NOT NULL`: una cuenta por venta). Las cuentas manuales existentes quedan con `VentaId` nulo.
- `TipoMovimientoPuntos.Reversion = 3`.
- `MetodoPago.CodigoCredito = "Credito"` como constante del dominio; la regla identifica el crédito por código, no por `Guid` sembrado.
- Migración con las dos columnas nuevas, sin relleno. El valor de `Reversion` es un entero nuevo, sin cambios de esquema si el enum se guarda como número.

**D4. Un servicio de aplicación para la relación con el cliente.**
`IServicioClienteVenta` (nuevo, en Application) concentra tres operaciones: `ValidarCliente` (existe, no eliminado, activo), `AcumularPorVentaAsync` (calcula puntos = ⌊USD pagado sin crédito × `Fidelidad:PuntosPorUSD`⌋, agrega el movimiento y suma al cliente) y `RegistrarCreditoAsync` (crea la `CuentaPorCobrar` en USD con la tasa de la venta, enlazada a la venta, con el vencimiento por defecto o el indicado), más `RevertirPorDevolucionAsync`. `ServicioVentas` lo invoca dentro de `CrearVentaAsync`, antes del guardado, para que todo viaje en la misma transacción y falle junto con la venta.
*Alternativa:* meter la lógica en `ServicioVentas` (lo engorda y mezcla CRM con ventas) o publicar un evento de dominio (más piezas que el problema merece).

**D5. Reglas del crédito.**
Un pago es de crédito si su método tiene `Codigo == "Credito"`. Si hay al menos uno: el cliente es obligatorio y activo (422 si no); el monto a crédito en USD es la suma de esos pagos (los pagos en Bs se convierten con la tasa de la venta); el vencimiento por defecto es `fecha de la venta + Credito:DiasVencimiento` (30) y, si se indica, debe ser posterior a la venta. `RegistrarAbonoAsync` rechaza con 422 un abono con método Crédito. El pago en Crédito sigue cubriendo el total de la venta (la deuda queda en la cuenta por cobrar).
*Alternativa:* crear el crédito fuera de la venta con un paso manual en Cuentas por cobrar (el estado actual, que desconecta el dinero de la deuda). Se descarta.

**D6. Puntos.**
`Fidelidad:PuntosPorUSD` (decimal, 1 por defecto; 0 desactiva) en configuración; se calcula sobre el total pagado sin crédito, en USD, redondeado hacia abajo. La reversión en `RegistrarDevolucionAsync` resta ⌊monto devuelto × tasa⌋ puntos, limitado al saldo del cliente (nunca negativo), con un `PuntosMovimiento` de tipo `Reversion` referenciando la venta. La devolución no falla si no hay puntos suficientes. Las decisiones de qué tasa y qué base usar son supuestos de este cambio, fácilmente ajustables por configuración.

**D7. Estado vencida derivado al leer.**
`ServicioCrm.MapearCuenta` devuelve `Vencida` cuando el estado persistido es `Pendiente`, `SaldoUSD > 0` y `Vencimiento < ahora`; no hay tarea programada ni escritura. El filtro de pendientes del listado incluye las vencidas. Se descarta un servicio en segundo plano por ser infraestructura adicional sin beneficio a esta escala.

**D8. Historial del cliente.**
`GET /api/v1/clientes/{id}/ventas` (paginado, política `crm:read`) devuelve las ventas del cliente (fecha, total, comprobante, estado). `ClientesPage` agrega un panel de detalle con ventas, puntos y cuentas por cobrar usando los endpoints existentes de puntos y cuentas por cobrar filtradas por cliente.

**D9. Devolución de venta fiada.**
`RegistrarDevolucionAsync` consulta si la venta tiene una cuenta por cobrar pendiente y, si es así, agrega un campo informativo `Aviso` en `DevolucionDto` ("Ajuste la cuenta por cobrar manualmente"). No modifica el saldo.

**D10. Interfaz del POS y del cierre de cuenta.**
`PosPage` lee los métodos de pago (`ventasApi.metodosPago`) y `OrdenPanel` agrega un selector de método (efectivo en la moneda actual por defecto) y un buscador de clientes (`clientesApi.listar` con `busqueda`, con creación rápida de nombre y teléfono). Con Crédito, se exige el cliente y se muestra el vencimiento previsto. `TicketVenta` imprime el cliente, los puntos ganados y el monto a crédito (la respuesta de venta incluye `puntosGanados` y `creditoUSD`). El diálogo de cierre en `CuentaPage` suma los mismos selectores. Las pantallas de Clientes, Cuentas por cobrar y Promociones se ajustan: Cuentas por cobrar muestra la venta enlazada y el estado vencida; Promociones oculta las acciones de edición a quien no tenga `promotions:manage`.

## Risks / Trade-offs

- [Sin límite de crédito, un cajero puede fiar sin tope] → Se documenta; la cuenta por cobrar queda auditada (`auditoria-total`) y visible; el límite queda como mejora futura.
- [La tasa de puntos y los 30 días de vencimiento son supuestos de negocio] → Configurables sin cambiar código y documentados; confirmar con el docente.
- [Concurrencia sobre `Cliente.Puntos`: dos ventas simultáneas al mismo cliente] → `xmin` detecta el conflicto y el reintento de `concurrencia-stock` lo resuelve; sin ese cambio se obtiene un 409 limpio.
- [Reversión por devolución puede dejar menos puntos de los debidos si ya canjeó] → Se limita al saldo y se documenta; el canje manual sigue fuera de la venta.
- [Quitar la escritura de promociones a Cajero, Mesero, Barra y Cocina rompe flujos si alguien las creaba] → **BREAKING** documentado en el `CHANGELOG`; las promociones las gestiona el Administrador.
- [Reactivar pantallas sin pruebas puede exponer defectos latentes] → Una prueba de página por módulo reactivado y un recorrido manual por rol antes de cerrar.
- [Crédito en Bs convertido con la tasa de la venta puede diferir de la tasa del cobro posterior] → La deuda se fija en USD al momento de la venta; el cobro se expresa en USD.
- [Cambios simultáneos en `ServicioVentas` con `concurrencia-stock`] → Este cambio añade llamadas dentro de `CrearVentaAsync`, que ese cambio ya vuelve transaccional; se implementa después y se resuelven los conflictos de merge en esos dos métodos.

## Migration Plan

1. Aplicar la migración (`venta.cliente_id`, `cuenta_por_cobrar.venta_id`).
2. Añadir las claves de configuración (`Fidelidad:PuntosPorUSD`, `Credito:DiasVencimiento`) con sus valores por defecto a `appsettings` y al `.env.example`.
3. Desplegar backend y frontend juntos (el frontend nuevo envía `clienteId` y métodos de pago que el backend antiguo ignoraría).
4. Avisar al personal que los permisos nuevos se aplican al volver a iniciar sesión.
5. Rollback: revertir la migración elimina las columnas; la configuración sobrante es inofensiva.
