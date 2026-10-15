# waiter-performance-kpis Specification

## Purpose

Permite a la administración medir el desempeño de cada mesonero con indicadores objetivos de ventas, calidad de servicio y rapidez.

## Requirements

### Requirement: Atribución al mesonero que toma el pedido
El sistema SHALL atribuir cada comanda, y los ítems y ventas que genera, al usuario que la creó, aunque la cuenta la cobre otra persona.

#### Scenario: Cuenta cobrada por el cajero
- **WHEN** un mesonero toma los pedidos de una mesa y un cajero cobra la cuenta
- **THEN** las ventas generadas se atribuyen al mesonero

### Requirement: Indicadores por mesonero
El sistema SHALL calcular por mesonero y periodo: comandas, ítems servidos, ventas generadas en USD, número de cuentas atendidas, ticket promedio por cuenta y tasa de cancelación de ítems.

#### Scenario: Mesonero con actividad
- **WHEN** se consulta el periodo
- **THEN** cada mesonero con comandas muestra sus indicadores, ordenados por ventas generadas

#### Scenario: Ítems cancelados y cortesías
- **WHEN** un mesonero tiene ítems cancelados y cortesías
- **THEN** los cancelados no suman ventas, cuentan en la tasa de cancelación y las cortesías no suman ingreso

### Requirement: Tiempo promedio de servicio
El sistema SHALL registrar el momento en que un ítem se sirve por primera vez y SHALL calcular el tiempo promedio entre su creación y ese momento, ignorando los ítems sin esa marca.

#### Scenario: Ítem servido
- **WHEN** barra marca un ítem como preparado
- **THEN** se guarda el momento de servicio y entra en el promedio de su mesonero

#### Scenario: Datos anteriores
- **WHEN** un ítem es anterior a esta funcionalidad y no tiene momento de servicio
- **THEN** no se incluye en el tiempo promedio ni lo distorsiona

#### Scenario: Servir de nuevo
- **WHEN** un ítem ya servido cambia de Preparado a Entregado
- **THEN** el momento de servicio original no cambia

### Requirement: Acceso restringido al Administrador
Los indicadores por mesonero MUST ser visibles solo para el rol Administrador.

#### Scenario: Cajero consulta el desempeño
- **WHEN** un cajero solicita los indicadores por mesonero
- **THEN** la respuesta es 403
