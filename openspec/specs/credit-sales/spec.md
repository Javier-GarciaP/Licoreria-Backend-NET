# credit-sales Specification

## Purpose

Permite vender a crédito (fiado) a clientes conocidos, dejando una cuenta por cobrar enlazada a la venta para su seguimiento y cobro.

## Requirements

### Requirement: Crédito exige cliente
El sistema MUST rechazar con 422 una venta que incluya un pago con el método Crédito si no tiene un cliente activo asociado.

#### Scenario: Crédito sin cliente
- **WHEN** se registra una venta con un pago en Crédito y sin cliente
- **THEN** la respuesta es 422 y no se registra la venta

### Requirement: Cuenta por cobrar automática
Al registrar una venta con pago en Crédito, el sistema SHALL crear una cuenta por cobrar del cliente por el monto a crédito en USD, enlazada a la venta, en la misma transacción.

#### Scenario: Venta parcialmente a crédito
- **WHEN** una venta de 40 USD se paga 30 USD en efectivo y 10 USD a crédito
- **THEN** existe una cuenta por cobrar de 10 USD con saldo 10 USD, enlazada a la venta y al cliente

#### Scenario: Crédito en Bs
- **WHEN** el pago a crédito se expresa en Bs
- **THEN** la cuenta por cobrar se registra en USD con la tasa de la venta

#### Scenario: Falla la venta
- **WHEN** la venta falla después de calcular el crédito
- **THEN** no queda ninguna cuenta por cobrar

### Requirement: Vencimiento por defecto
La cuenta por cobrar SHALL vencer a los 30 días de la venta, plazo configurable, y la venta SHALL poder indicar otra fecha de vencimiento posterior a la fecha de la venta.

#### Scenario: Vencimiento por defecto
- **WHEN** se vende a crédito sin indicar vencimiento
- **THEN** la cuenta por cobrar vence 30 días después de la venta

#### Scenario: Vencimiento en el pasado
- **WHEN** se indica un vencimiento anterior a la fecha de la venta
- **THEN** la respuesta es 422

### Requirement: Cuentas vencidas
El sistema SHALL mostrar como vencida toda cuenta por cobrar con saldo pendiente cuyo vencimiento ya pasó, sin requerir una tarea programada.

#### Scenario: Cuenta pasada de fecha
- **GIVEN** una cuenta pendiente con vencimiento de ayer
- **WHEN** el administrador consulta las cuentas por cobrar
- **THEN** aparece con estado vencida

#### Scenario: Cuenta pagada
- **WHEN** una cuenta vencida se paga por completo
- **THEN** su estado es pagada

### Requirement: Crédito no permitido en abonos de cuenta
El sistema MUST rechazar con 422 un abono de cuenta de mesa que use el método Crédito; el crédito solo se otorga al cobrar la venta o cerrar la cuenta.

#### Scenario: Abono en Crédito
- **WHEN** un mesero registra un abono con método Crédito
- **THEN** la respuesta es 422 indicando que el crédito se otorga al cerrar la cuenta

### Requirement: Cierre de cuenta a crédito
El cierre de una cuenta de mesa con pago en Crédito SHALL seguir las mismas reglas de cliente y cuenta por cobrar que la venta directa.

#### Scenario: Cierre con crédito
- **WHEN** un cajero cierra una cuenta con un pago en Crédito y un cliente
- **THEN** se registra la venta y una cuenta por cobrar enlazada a ella

### Requirement: Devolución de una venta a crédito
El sistema SHALL informar en la devolución de una venta con cuenta por cobrar que el saldo de esa cuenta no se ajusta automáticamente.

#### Scenario: Devolución de venta fiada
- **WHEN** se devuelve parte de una venta que tiene cuenta por cobrar pendiente
- **THEN** la respuesta indica que la cuenta por cobrar debe ajustarse manualmente
