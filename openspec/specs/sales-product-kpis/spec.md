# sales-product-kpis Specification

## Purpose

Ofrece indicadores de ventas y de producto para un periodo elegido, de modo que la dirección vea cuánto se vende, cómo se paga y qué productos mueven o frenan el inventario.

## Requirements

### Requirement: Ticket promedio del periodo
El sistema SHALL calcular el ticket promedio en USD de las ventas completadas del periodo solicitado, excluyendo las ventas anuladas.

#### Scenario: Periodo con ventas
- **WHEN** se consulta un periodo con 10 ventas completadas por un total de 250 USD
- **THEN** el ticket promedio es 25 USD y la cantidad de ventas es 10

#### Scenario: Periodo sin ventas
- **WHEN** se consulta un periodo sin ventas
- **THEN** el ticket promedio es 0 y la respuesta no falla

### Requirement: Ventas por método de pago
El sistema SHALL desglosar los pagos del periodo por método de pago y moneda, con el monto original y la cantidad de pagos.

#### Scenario: Pagos mixtos
- **WHEN** una venta se pagó con efectivo en USD y pago móvil en Bs
- **THEN** el desglose muestra cada método con su moneda y su monto original

### Requirement: Top productos
El sistema SHALL listar los productos más vendidos del periodo, ordenados por unidades o por ingresos a elección, con las unidades, el ingreso en USD y el porcentaje que representan del total, limitado a un número configurable (10 por defecto).

#### Scenario: Orden por unidades
- **WHEN** se pide el top 5 ordenado por unidades
- **THEN** la lista trae a lo sumo 5 productos con las unidades descendentes

#### Scenario: Cortesías
- **WHEN** un producto se entregó como cortesía
- **THEN** sus unidades cuentan como salida pero no suman ingreso

### Requirement: Rotación e inventario sin movimiento
El sistema SHALL calcular la rotación de cada producto como unidades vendidas del periodo entre la existencia actual, y SHALL identificar los productos con existencia y ninguna venta en el periodo.

#### Scenario: Producto sin ventas
- **GIVEN** un producto con existencia y sin ventas en el periodo
- **WHEN** se consulta la rotación
- **THEN** aparece como sin movimiento con rotación 0

#### Scenario: Existencia en cero
- **WHEN** un producto vendido tiene existencia actual 0
- **THEN** su rotación se marca como no calculable en lugar de dividir entre cero

### Requirement: Periodo válido
El sistema SHALL rechazar con 400 un periodo con fecha final anterior a la inicial o mayor a 366 días, y SHALL usar los últimos 30 días cuando no se indique ninguno.

#### Scenario: Rango invertido
- **WHEN** la fecha final es anterior a la inicial
- **THEN** la respuesta es 400 con el detalle del error
