# Spec Delta

## Purpose

Permite al cajero elegir cómo se paga cada venta y a quién se le vende desde el punto de venta, en lugar de usar siempre el mismo método.

## ADDED Requirements

### Requirement: Elegir el método de pago
El POS SHALL permitir elegir el método de pago activo de la venta entre los métodos disponibles, y SHALL registrar el pago con el método elegido.

#### Scenario: Pago con Pago móvil
- **WHEN** el cajero elige Pago móvil y confirma el cobro
- **THEN** la venta se registra con ese método de pago

#### Scenario: Sin elección
- **WHEN** el cajero no elige un método
- **THEN** se usa efectivo en la moneda seleccionada

### Requirement: Elegir el cliente
El POS SHALL permitir buscar y seleccionar un cliente, o crear uno nuevo con los datos mínimos, y SHALL mostrar su nombre y sus puntos antes de cobrar.

#### Scenario: Buscar cliente
- **WHEN** el cajero escribe parte del nombre o documento de un cliente
- **THEN** el POS lista los clientes que coinciden y permite elegir uno

#### Scenario: Quitar el cliente
- **WHEN** el cajero quita el cliente seleccionado
- **THEN** la venta se cobra sin cliente

### Requirement: Crédito condicionado al cliente
El POS SHALL ofrecer el método Crédito y MUST exigir un cliente seleccionado antes de permitir cobrar con él, mostrando la fecha de vencimiento prevista.

#### Scenario: Crédito sin cliente
- **WHEN** el cajero elige Crédito sin cliente
- **THEN** el botón de cobrar queda deshabilitado con el motivo visible

#### Scenario: Crédito con cliente
- **WHEN** el cajero elige Crédito con un cliente
- **THEN** ve la fecha de vencimiento y puede cobrar

### Requirement: Resumen de puntos y crédito en el comprobante
El ticket de la venta SHALL mostrar el cliente, los puntos ganados y el monto a crédito cuando existan.

#### Scenario: Venta con puntos y crédito
- **WHEN** se muestra el ticket de una venta con cliente, puntos y crédito
- **THEN** incluye el nombre del cliente, los puntos ganados y el monto fiado
