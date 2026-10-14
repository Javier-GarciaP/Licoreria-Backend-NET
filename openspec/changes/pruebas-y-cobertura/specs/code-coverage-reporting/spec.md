# Spec Delta

## Purpose

Hace visible y exigible cuánto código de negocio está verificado por las pruebas, con un reporte legible para la defensa y un umbral mínimo que evita que la cobertura retroceda.

## ADDED Requirements

### Requirement: Recolección de cobertura
El proyecto SHALL permitir medir la cobertura de líneas y de ramas de las pruebas unitarias con una sola orden, excluyendo código que no aporta información: migraciones, punto de entrada, registros de datos (DTOs), configuraciones de EF y siembra de datos.

#### Scenario: Medir la cobertura
- **WHEN** un desarrollador ejecuta la tarea de cobertura
- **THEN** se genera un archivo de cobertura de las pruebas unitarias sin incluir migraciones ni el punto de entrada

### Requirement: Reporte legible
El sistema SHALL generar un reporte HTML navegable por proyecto, clase y método, y un resumen de texto con los porcentajes de líneas y ramas.

#### Scenario: Abrir el reporte
- **WHEN** termina la tarea de cobertura
- **THEN** existe un reporte HTML y un resumen con el porcentaje global y por ensamblado

### Requirement: Umbral mínimo
El sistema SHALL fallar la verificación de cobertura cuando la cobertura de líneas combinada de los ensamblados Domain y Application sea inferior al umbral configurado (70 % inicialmente), mostrando el valor medido y el exigido.

#### Scenario: Cobertura por debajo del umbral
- **GIVEN** un umbral de 70 %
- **WHEN** la cobertura medida es 65 %
- **THEN** la verificación falla indicando 65 % medido frente a 70 % exigido

#### Scenario: Cobertura suficiente
- **WHEN** la cobertura medida supera el umbral
- **THEN** la verificación pasa

### Requirement: Umbral configurable y sin retroceso
El umbral SHALL estar definido en un único archivo versionado y SHALL poder aumentarse a medida que mejora la cobertura, sin permitir que una modificación lo reduzca por debajo del valor base sin dejar rastro en el repositorio.

#### Scenario: Cambiar el umbral
- **WHEN** el umbral se modifica en el archivo de configuración
- **THEN** el cambio queda visible en el historial del repositorio y la verificación usa el nuevo valor

### Requirement: Integración continua
La integración continua SHALL ejecutar las pruebas con cobertura, verificar el umbral y publicar el reporte como artefacto descargable, mostrando el resumen en la ejecución.

#### Scenario: Ejecución en CI
- **WHEN** corre el flujo de integración continua del backend
- **THEN** publica el reporte de cobertura y falla si no se alcanza el umbral

### Requirement: Línea base conocida
Antes de aplicar el umbral, el proyecto SHALL registrar la cobertura medida al empezar y la alcanzada al terminar, para mostrar la mejora en la defensa.

#### Scenario: Comparar antes y después
- **WHEN** se prepara la defensa
- **THEN** la documentación muestra la cobertura inicial y la final por ensamblado
