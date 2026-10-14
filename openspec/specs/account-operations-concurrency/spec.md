# account-operations-concurrency Specification

## Purpose

Garantiza que la apertura de mesa, los abonos, el cambio de estado de ítems y el cierre de cuenta se comporten de forma consistente cuando varias personas operan sobre la misma cuenta a la vez.

## Requirements

### Requirement: Una sola cuenta abierta por mesa
El sistema MUST impedir que una misma mesa tenga dos cuentas abiertas, incluso si dos mesoneros la abren simultáneamente.

#### Scenario: Apertura simultánea de la misma mesa
- **WHEN** dos mesoneros abren la misma mesa al mismo tiempo
- **THEN** una cuenta se crea y la otra operación recibe 409 indicando que la mesa ya está ocupada

### Requirement: Cierre de cuenta único y atómico
El cierre de una cuenta SHALL registrar la venta y marcar la cuenta como cerrada en una sola operación indivisible, y una cuenta MUST generar como máximo una venta.

#### Scenario: Dos cajeros cobran la misma mesa
- **WHEN** dos cajeros cierran la misma cuenta al mismo tiempo
- **THEN** se registra una sola venta y la otra operación recibe 409

#### Scenario: Fallo durante el cierre
- **WHEN** el registro de la venta falla durante el cierre
- **THEN** la cuenta permanece abierta y no queda ninguna venta huérfana

### Requirement: Abonos concurrentes sin pérdida
El sistema SHALL acumular todos los abonos registrados simultáneamente sobre una cuenta, sin perder ninguno ni exceder el consumo total.

#### Scenario: Dos abonos a la vez
- **WHEN** dos abonos se registran al mismo tiempo sobre la misma cuenta
- **THEN** el total abonado refleja la suma de ambos, o uno de ellos recibe 409 y puede reintentarse

#### Scenario: Abono sobre cuenta que se cierra
- **WHEN** se registra un abono mientras otra operación cierra la cuenta
- **THEN** el abono se incluye en el cierre o se rechaza (409, o no encontrada si la cuenta ya figura cerrada), nunca se pierde en silencio

### Requirement: Cambio de estado de ítem sin doble aplicación
El sistema SHALL aplicar una sola vez cada transición de estado de un ítem de comanda cuando barra, cocina y mesonero actúan sobre él al mismo tiempo, y MUST informar de forma clara a quien pierde la carrera.

#### Scenario: Dos áreas marcan el mismo ítem
- **WHEN** dos usuarios cambian el estado del mismo ítem al mismo tiempo
- **THEN** una transición se aplica y la otra recibe 409 con el estado actual del ítem
- **AND** el inventario se descuenta una sola vez

#### Scenario: Cancelar un ítem que se está sirviendo
- **WHEN** un usuario cancela un ítem mientras otro lo marca como servido
- **THEN** el resultado final es uno solo y el inventario y el total de la cuenta quedan coherentes con él
