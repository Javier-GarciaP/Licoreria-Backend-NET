# Spec Delta

## Purpose

Garantiza que ejecutar las pruebas por defecto sea rápido, determinista y verde en cualquier equipo, y que las pruebas que necesitan PostgreSQL se ejecuten de forma explícita y obligatoria donde corresponde.

## ADDED Requirements

### Requirement: Ejecución por defecto verde sin base de datos
La ejecución de todas las pruebas de la solución (`dotnet test`) SHALL terminar sin fallos en un equipo sin PostgreSQL, ejecutando las pruebas unitarias y omitiendo las de integración con un motivo visible.

#### Scenario: Equipo sin PostgreSQL
- **WHEN** un desarrollador ejecuta `dotnet test` sin base de datos disponible
- **THEN** las pruebas unitarias pasan y las de integración se reportan como omitidas indicando que no hay base de datos

#### Scenario: Equipo con PostgreSQL
- **WHEN** el mismo comando se ejecuta con PostgreSQL disponible
- **THEN** las pruebas de integración también se ejecutan y pasan

### Requirement: Categorías de pruebas
Las pruebas de integración y las de concurrencia con base de datos SHALL declarar una categoría, de modo que puedan incluirse o excluirse con un filtro.

#### Scenario: Solo unitarias
- **WHEN** se ejecuta el filtro que excluye la categoría de integración
- **THEN** solo corren las pruebas unitarias y no se intenta ninguna conexión

#### Scenario: Solo integración
- **WHEN** se ejecuta el filtro que incluye únicamente la categoría de integración
- **THEN** corren solo las pruebas con base de datos

### Requirement: Base de datos obligatoria en CI
Cuando la variable `LICORERIA_TESTS_REQUIRE_DB` vale `true`, la ausencia de PostgreSQL SHALL hacer fallar las pruebas de integración en lugar de omitirlas, para que la integración continua no oculte una infraestructura rota.

#### Scenario: CI sin base de datos
- **GIVEN** `LICORERIA_TESTS_REQUIRE_DB=true`
- **WHEN** PostgreSQL no responde
- **THEN** las pruebas de integración fallan con un mensaje claro

### Requirement: Conexión configurable
La conexión de las pruebas de integración SHALL poder indicarse con una variable de entorno, con el valor actual (`localhost:5432`, usuario `licoreria`) como predeterminado.

#### Scenario: Servidor distinto
- **WHEN** se define la variable de conexión con otro servidor
- **THEN** las pruebas usan ese servidor y su base de datos de pruebas

### Requirement: Disponibilidad comprobada una vez y sin colgarse
La comprobación de disponibilidad de la base de datos SHALL ejecutarse una sola vez por corrida y SHALL expirar en pocos segundos si el servidor no responde.

#### Scenario: Servidor que no responde
- **WHEN** el servidor de base de datos no responde
- **THEN** la comprobación termina en menos de 5 segundos y se omiten o fallan las pruebas según la configuración

### Requirement: Tareas separadas y documentadas
El proyecto SHALL ofrecer tareas para ejecutar por separado las pruebas unitarias, las de integración y todas, y SHALL documentarlas junto con las variables de entorno.

#### Scenario: Ejecutar solo unitarias con la tarea
- **WHEN** un desarrollador ejecuta la tarea de pruebas unitarias
- **THEN** corren solo las unitarias y el resultado se muestra por consola
