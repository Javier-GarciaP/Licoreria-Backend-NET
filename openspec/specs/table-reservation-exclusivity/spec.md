# table-reservation-exclusivity Specification

## Purpose

Evita que una mesa quede asignada a dos reservas con horarios solapados, incluso cuando las reservas se crean simultáneamente desde la web pública o desde el personal.

## Requirements

### Requirement: Sin reservas solapadas para una mesa
El sistema MUST rechazar una reserva si alguna de sus mesas ya está asignada a otra reserva vigente cuyo intervalo se solapa con el solicitado.

#### Scenario: Reserva en horario libre
- **WHEN** se crea una reserva para una mesa sin otras reservas en ese intervalo
- **THEN** la reserva se crea correctamente

#### Scenario: Reserva en horario ocupado
- **WHEN** se crea una reserva para una mesa ya reservada en un intervalo solapado
- **THEN** el sistema responde 409 indicando el número de mesa y el horario ocupado

### Requirement: Exclusión ante solicitudes simultáneas
El sistema MUST garantizar la exclusión aunque dos reservas para la misma mesa y horario lleguen al mismo tiempo.

#### Scenario: Dos reservas simultáneas
- **WHEN** la web pública y un host crean al mismo tiempo una reserva para la misma mesa y hora
- **THEN** una reserva se crea y la otra recibe 409

### Requirement: Reservas canceladas liberan la mesa
Una reserva cancelada o completada SHALL dejar de ocupar la mesa para efectos del solapamiento.

#### Scenario: Reserva cancelada
- **GIVEN** una reserva cancelada para una mesa y horario
- **WHEN** se crea una nueva reserva para esa mesa y horario
- **THEN** la nueva reserva se crea correctamente
