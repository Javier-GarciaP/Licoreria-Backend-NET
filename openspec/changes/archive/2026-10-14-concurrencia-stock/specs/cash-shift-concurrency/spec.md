# Spec Delta

## Purpose

Garantiza que exista un único turno de caja abierto a la vez y que el cierre de turno sea consistente con las ventas y operaciones que ocurren mientras se cierra.

## ADDED Requirements

### Requirement: Un único turno abierto
El sistema MUST impedir que haya más de un turno de caja abierto, incluso si dos cajeros intentan abrir turno al mismo tiempo.

#### Scenario: Apertura simultánea
- **WHEN** dos cajeros abren turno al mismo tiempo
- **THEN** un turno queda abierto y la otra operación recibe 409 indicando que ya existe una sesión abierta

### Requirement: Cierre de turno consistente con las ventas en curso
El cierre de turno SHALL calcular el esperado y el descuadre sobre un conjunto de ventas estable: toda venta iniciada antes del cierre se incluye, y toda venta posterior se rechaza porque el turno ya no está abierto.

#### Scenario: Venta durante el cierre
- **WHEN** se registra una venta mientras otro usuario cierra el turno
- **THEN** la venta se incluye en el arqueo o se rechaza con un error de turno cerrado, nunca queda fuera del arqueo asociada al turno cerrado

#### Scenario: Cierre repetido
- **WHEN** dos usuarios cierran el mismo turno al mismo tiempo
- **THEN** el turno se cierra una sola vez y la otra operación recibe 409

### Requirement: Liberación de reservas al cerrar el turno
Al cerrar el turno el sistema SHALL liberar las reservas de insumos de comandas que no se sirvieron ni se cobraron.

#### Scenario: Comandas pendientes al cierre
- **GIVEN** comandas con ítems reservados sin servir
- **WHEN** se cierra el turno
- **THEN** las reservas de esos ítems se liberan y la existencia disponible se recupera
