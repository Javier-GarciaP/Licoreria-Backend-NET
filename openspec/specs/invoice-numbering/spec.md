# invoice-numbering Specification

## Purpose

Asegura que cada comprobante fiscal reciba un número correlativo único, incluso cuando varias ventas se registran al mismo tiempo.

## Requirements

### Requirement: Número de comprobante único
El sistema MUST asignar a cada comprobante un número y un número de control únicos, de modo que nunca existan dos comprobantes con el mismo valor.

#### Scenario: Ventas simultáneas
- **WHEN** dos ventas se registran al mismo tiempo
- **THEN** cada comprobante recibe un número distinto

#### Scenario: Intento de duplicado
- **WHEN** una operación intentaría guardar un número de comprobante ya existente
- **THEN** la operación se rechaza y no se registra el duplicado

### Requirement: Correlativo creciente sin depender del conteo
El sistema SHALL generar los números en orden creciente a partir de una secuencia propia, independiente de la cantidad de comprobantes visibles, de modo que anulaciones o eliminaciones lógicas no provoquen repeticiones.

#### Scenario: Venta posterior a registros eliminados
- **GIVEN** que existen comprobantes eliminados lógicamente
- **WHEN** se registra una nueva venta
- **THEN** su número es mayor que el de todos los comprobantes anteriores

### Requirement: Venta fallida no reutiliza ni rompe el orden
El sistema SHALL tolerar saltos en el correlativo causados por ventas que no se completaron, sin que eso permita duplicados.

#### Scenario: Venta revertida
- **WHEN** una venta falla después de reservar su número
- **THEN** el siguiente comprobante recibe un número mayor y nunca uno repetido
