# Proposal

## Why

Varios módulos ya construidos están ocultos del menú (`oculto: true` en `lib/navigation.tsx`): Promociones, Existencias, Clientes y Cuentas por cobrar, Entradas y Lista VIP. El proyecto exige un negocio híbrido (licorería de mostrador y discoteca) y el docente observó que la aplicación está incompleta. Reactivarlos tal cual no basta: Clientes y Cuentas por cobrar están desconectados de la venta (`Venta` no guarda cliente, los puntos y el fiado se registran a mano), el método de pago "Crédito" está sembrado pero pagar con él no crea ninguna cuenta por cobrar, el POS siempre usa el primer método de pago sin dejar elegir, el Cajero no tiene permisos de CRM (recibiría 403 en pantallas que la navegación le muestra) y cualquier rol con `sales:write` (mesero, barra, cocina) puede crear promociones con descuento por la API. Compras ya está visible para el Administrador y solo requiere verificación.

## What Changes

- **Reactivación por modo y rol**: se quita `oculto` a Promociones, Existencias, Clientes, Cuentas por cobrar, Entradas y Lista VIP. Promociones, Existencias, Clientes, Cuentas por cobrar y Compras aparecen en ambos modos; Entradas y Lista VIP solo en modo Discoteca (ya pertenecen al grupo Salón). Los lotes, tomas físicas y el contenido siguen ocultos.
- **Permisos coherentes**: el Cajero recibe `crm:read` y `crm:write`; las rutas de escritura de promociones pasan a un permiso nuevo `promotions:manage` exclusivo del Administrador (la lectura sigue abierta a quienes venden, porque el POS la usa). **BREAKING** para quien creaba promociones sin ser Administrador.
- **Cliente en la venta**: `Venta` guarda un cliente opcional; el POS y el cierre de cuenta permiten elegirlo.
- **Puntos automáticos**: una venta con cliente acumula puntos (tasa configurable, 1 punto por USD pagado por defecto) dentro de la misma transacción; una devolución revierte los puntos proporcionales. El canje sigue siendo manual.
- **Fiado**: pagar con el método Crédito exige un cliente activo y crea automáticamente una cuenta por cobrar enlazada a la venta, con vencimiento por defecto de 30 días (configurable). Las cuentas vencidas se muestran como tales. No se aceptan abonos de cuenta en Crédito.
- **Selector de método de pago en el POS** (hoy fijo en el primer método) y selector de cliente en el POS y en el cierre de cuenta.
- **Pruebas**: unitarias con Moq de puntos, crédito y devolución; de integración de la venta con cliente y fiado; y pruebas de página de cada módulo reactivado.

## Capabilities

### New Capabilities
- `module-navigation-activation`: visibilidad de los módulos reactivados según modo (Licorería o Discoteca), rol y permiso, con acceso directo coherente.
- `sale-customer-link`: asociación opcional de un cliente a una venta y a un cierre de cuenta.
- `loyalty-points-accrual`: acumulación automática de puntos por venta y reversión por devolución.
- `credit-sales`: venta a crédito que genera una cuenta por cobrar enlazada, con vencimiento y estado vencida.
- `pos-payment-method-selection`: elección del método de pago y del cliente en el punto de venta.

### Modified Capabilities
<!-- No hay specs principales todavía (openspec/specs está vacío). -->

## Impact

- **Backend (Domain)**: `Venta.ClienteId`, `CuentaPorCobrar.VentaId`, `TipoMovimientoPuntos.Reversion`, constante del código del método Crédito.
- **Backend (Application)**: `ServicioVentas` (cliente, crédito, puntos, devolución), `ServicioCrm` (estado vencida), `ServicioCuentas` (cliente en el cierre, abonos en Crédito), nuevo servicio de fidelidad y crédito, opciones de configuración, `Permisos` y `RolesSeguridad`.
- **Backend (Infrastructure)**: configuraciones de EF, migración, `ClienteRepository`.
- **Backend (WebAPI)**: `PromocionesController` (política de escritura), DTOs de venta y cierre, `openapi/licoreria.yaml`.
- **Frontend**: `lib/navigation.tsx`, `PosPage`, `OrdenPanel`, `CuentaPage` (cierre), `ClientesPage`, `CuentasPorCobrarPage`, `PromocionesPage`, `packages/api-client` y `packages/types`.
- **Configuración**: `Fidelidad:PuntosPorUSD` y `Credito:DiasVencimiento`.
- **Coordinación**: `ServicioVentas` y `CerrarCuentaAsync` también cambian en `concurrencia-stock` (transacción única); las actualizaciones de puntos viajan en esa transacción. `lib/navigation.tsx` se comparte con `auditoria-total`, `reportes-kpis-export` y `croquis-planos`.
