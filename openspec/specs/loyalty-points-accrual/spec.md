# loyalty-points-accrual Specification

## Purpose

Premia a los clientes frecuentes acumulando puntos de forma automática por cada venta y manteniendo su saldo correcto cuando hay devoluciones.

## Requirements

### Requirement: Acumulación automática por venta
Al registrar una venta con cliente, el sistema SHALL sumar al cliente los puntos que correspondan al total pagado sin crédito, según la tasa configurada (1 punto por USD por defecto, redondeado hacia abajo), y SHALL registrar el movimiento con referencia a la venta.

#### Scenario: Venta de 25,80 USD
- **GIVEN** la tasa de 1 punto por USD
- **WHEN** se registra una venta con cliente por 25,80 USD pagados en efectivo
- **THEN** el cliente suma 25 puntos y queda un movimiento de acumulación ligado a la venta

#### Scenario: Venta menor a un punto
- **WHEN** el total pagado equivale a menos de un punto
- **THEN** no se registra ningún movimiento de puntos

### Requirement: Sin puntos por la porción a crédito
La parte de una venta pagada con el método Crédito SHALL NOT generar puntos.

#### Scenario: Venta mixta
- **WHEN** una venta de 40 USD se paga 30 USD en efectivo y 10 USD a crédito
- **THEN** los puntos se calculan sobre 30 USD

### Requirement: Atomicidad con la venta
El registro de la venta y la acumulación de puntos MUST confirmarse o descartarse juntos.

#### Scenario: Falla la venta
- **WHEN** la venta falla por falta de stock
- **THEN** el cliente no recibe puntos

### Requirement: Reversión por devolución
Al registrar una devolución de una venta con cliente, el sistema SHALL restar los puntos proporcionales al monto devuelto mediante un movimiento de reversión, sin dejar el saldo del cliente por debajo de cero.

#### Scenario: Devolución parcial
- **GIVEN** una venta de 20 USD que dio 20 puntos
- **WHEN** se devuelven productos por 5 USD
- **THEN** se registra una reversión de 5 puntos

#### Scenario: Puntos ya canjeados
- **WHEN** la reversión supera los puntos que el cliente conserva
- **THEN** se revierten solo los disponibles, la devolución se completa y el saldo queda en cero

### Requirement: Tasa configurable
La tasa de puntos por USD SHALL poder configurarse sin cambiar el código, y un valor de cero SHALL desactivar la acumulación automática.

#### Scenario: Acumulación desactivada
- **GIVEN** la tasa configurada en 0
- **WHEN** se registra una venta con cliente
- **THEN** no se registran puntos
