# venue-operations-kpis Specification

## Purpose

Ofrece indicadores de la operación del local (mesas, reservas, caja y mermas) para detectar cuellos de botella, ausencias y pérdidas.

## Requirements

### Requirement: Ocupación actual de mesas
El sistema SHALL informar cuántas mesas activas tienen una cuenta abierta en este momento y el porcentaje que representan, en total y por zona.

#### Scenario: Mesas ocupadas
- **GIVEN** 20 mesas activas de las cuales 5 tienen cuenta abierta
- **WHEN** se consulta la ocupación
- **THEN** el total muestra 5 de 20 (25 %) y cada zona muestra su propio conteo

### Requirement: Permanencia y rotación de mesas
El sistema SHALL calcular, para el periodo, la permanencia promedio de las sesiones de mesa y el número promedio de sesiones por mesa, por zona.

#### Scenario: Sesiones cerradas
- **WHEN** se consulta un periodo con sesiones de mesa cerradas
- **THEN** cada zona muestra el tiempo promedio entre apertura y cierre y las sesiones por mesa

#### Scenario: Sesión aún abierta
- **WHEN** una sesión sigue abierta al consultar
- **THEN** se cuenta hasta el momento de la consulta y se identifica como abierta

### Requirement: Asistencia de reservas
El sistema SHALL informar, para el periodo, el total de reservas por estado y la tasa de no asistencia sobre las reservas que debieron concretarse.

#### Scenario: Tasa de no asistencia
- **GIVEN** 8 reservas que asistieron y 2 que no asistieron
- **WHEN** se consulta el periodo
- **THEN** la tasa de no asistencia es 20 %

#### Scenario: Reservas canceladas
- **WHEN** existen reservas canceladas en el periodo
- **THEN** se cuentan aparte y no entran en la tasa de no asistencia

### Requirement: Descuadre de caja por turno
El sistema SHALL listar los turnos cerrados del periodo con el monto esperado, el contado y el descuadre, y resumir el descuadre total, el promedio y el mayor.

#### Scenario: Turno con faltante
- **WHEN** un turno se cerró con 5 USD menos de lo esperado
- **THEN** aparece con descuadre -5 y entra en el resumen del periodo

#### Scenario: Turnos abiertos
- **WHEN** hay un turno abierto
- **THEN** no se incluye en el descuadre porque aún no tiene arqueo

### Requirement: Mermas por motivo
El sistema SHALL resumir las mermas del periodo por motivo, con unidades, costo estimado en USD y su proporción frente a las ventas.

#### Scenario: Mermas del periodo
- **WHEN** se consulta un periodo con mermas por daño y vencimiento
- **THEN** se muestra cada motivo con sus unidades y costo, y el total frente a las ventas
