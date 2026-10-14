# Spec Delta

## Purpose

Permite asociar una venta a un cliente del local para que los puntos, el crédito y el historial de compras queden ligados a la persona correcta.

## ADDED Requirements

### Requirement: Cliente opcional en la venta
El sistema SHALL permitir indicar un cliente al registrar una venta, y la venta SHALL conservar esa asociación; una venta sin cliente sigue siendo válida.

#### Scenario: Venta con cliente
- **WHEN** un cajero registra una venta indicando un cliente activo
- **THEN** la venta queda asociada a ese cliente y la respuesta lo incluye

#### Scenario: Venta sin cliente
- **WHEN** un cajero registra una venta sin cliente
- **THEN** la venta se registra normalmente y no genera puntos ni crédito

### Requirement: Cliente válido
El sistema MUST rechazar con 404 un cliente inexistente o eliminado, y con 422 un cliente inactivo, sin registrar la venta.

#### Scenario: Cliente inactivo
- **WHEN** se registra una venta con un cliente inactivo
- **THEN** la respuesta es 422 y no se registra la venta ni se descuenta inventario

### Requirement: Cliente en el cierre de cuenta
El cierre de una cuenta de mesa SHALL permitir indicar un cliente, que se asocia a la venta resultante.

#### Scenario: Cierre con cliente
- **WHEN** un cajero cierra una cuenta indicando un cliente
- **THEN** la venta generada queda asociada a ese cliente

### Requirement: Historial de compras del cliente
La pantalla de un cliente SHALL mostrar sus ventas recientes, sus puntos y sus cuentas por cobrar.

#### Scenario: Cliente con compras
- **WHEN** un cajero abre el detalle de un cliente con ventas
- **THEN** ve las ventas asociadas con su fecha y total
