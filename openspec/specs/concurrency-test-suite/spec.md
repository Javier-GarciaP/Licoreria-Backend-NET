# concurrency-test-suite Specification

## Purpose

Permite verificar de forma reproducible que las invariantes del negocio se mantienen cuando varias operaciones ocurren al mismo tiempo, en lugar de confiar en que "normalmente no pasa".

## Requirements

### Requirement: Arnés de ejecución concurrente
El sistema de pruebas SHALL ofrecer un arnés que lance N operaciones en paralelo detenidas por una señal de salida común, recoja el resultado o la excepción de cada una y falle por tiempo de espera si alguna no termina.

#### Scenario: Salida simultánea
- **WHEN** se lanzan 10 operaciones con el arnés
- **THEN** todas esperan la señal y arrancan juntas, y el arnés devuelve 10 resultados

#### Scenario: Operación colgada
- **WHEN** una operación no termina dentro del tiempo máximo
- **THEN** la prueba falla indicando la operación que no terminó

### Requirement: Repetición para reducir falsos verdes
Las pruebas de carrera con base de datos SHALL repetir el escenario varias veces, y MUST verificar invariantes (y no el orden de llegada) para no depender de la planificación de hilos.

#### Scenario: Escenario repetido
- **WHEN** se ejecuta una prueba de carrera
- **THEN** repite el escenario al menos 10 veces y valida las invariantes en cada repetición

### Requirement: Existencia no negativa bajo mermas y ajustes simultáneos
Ante mermas y ajustes simultáneos sobre la misma variante, la existencia final SHALL ser igual a la inicial más los movimientos confirmados y MUST NOT ser negativa, y toda operación que no se confirme SHALL terminar con un conflicto o un error de regla de negocio, nunca con un error interno.

#### Scenario: Dos mermas sobre la última unidad
- **GIVEN** una variante con una unidad
- **WHEN** dos mermas de una unidad se registran al mismo tiempo
- **THEN** una se confirma y la otra falla con conflicto o regla de negocio, y la existencia final es cero

#### Scenario: Ajustes concurrentes
- **WHEN** varios ajustes se registran a la vez sobre la misma variante
- **THEN** la existencia final equivale a la suma de los ajustes confirmados

### Requirement: Recepción simultánea de una orden
Ante dos recepciones simultáneas de la misma línea de una orden de compra, la cantidad recibida total SHALL NOT superar la ordenada y el kardex SHALL registrar la entrada de cada recepción confirmada una sola vez.

#### Scenario: Doble recepción de lo pendiente
- **GIVEN** una línea con 10 unidades pendientes
- **WHEN** dos recepciones de 10 unidades se registran al mismo tiempo
- **THEN** solo una se confirma y la existencia aumenta en 10

### Requirement: Pagos simultáneos sin sobrepasar el saldo
Ante pagos simultáneos de una cuenta por pagar o por cobrar, la suma de los pagos confirmados SHALL NOT superar el monto de la cuenta y el saldo MUST NOT ser negativo.

#### Scenario: Dos pagos que juntos exceden el saldo
- **GIVEN** una cuenta con saldo de 100 USD
- **WHEN** dos pagos de 70 USD se registran al mismo tiempo
- **THEN** solo uno se confirma y el saldo final es de 30 USD

### Requirement: Creación simultánea de usuarios
Ante la creación simultánea de dos usuarios con el mismo correo, el sistema SHALL crear uno solo y SHALL responder al otro con un conflicto, nunca con un error interno.

#### Scenario: Mismo correo
- **WHEN** dos solicitudes crean usuarios con el mismo correo al mismo tiempo
- **THEN** una responde 201 y la otra 409

### Requirement: Simulación de conflicto con dobles de prueba
Las pruebas unitarias de los servicios con operaciones transaccionales SHALL simular un conflicto de concurrencia al guardar y verificar que el servicio lo propaga o lo reintenta según lo definido, sin dejar cambios parciales.

#### Scenario: Conflicto al guardar
- **WHEN** el repositorio simulado lanza un conflicto de concurrencia en el primer guardado
- **THEN** el servicio reintenta o responde con conflicto, según corresponda, y no registra efectos secundarios duplicados

### Requirement: Reutilización del arnés
Las pruebas de concurrencia de otros cambios (reserva de stock, cierre de cuenta, reservas de mesa, correlativo y turno de caja) SHALL reutilizar el mismo arnés.

#### Scenario: Prueba de la última unidad
- **WHEN** se escribe la prueba de dos mesoneros pidiendo la última unidad
- **THEN** usa el arnés de concurrencia común y no una implementación propia
