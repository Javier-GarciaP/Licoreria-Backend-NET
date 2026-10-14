# Spec Delta

## Purpose

Define qué comportamiento de los servicios de aplicación debe quedar verificado por pruebas unitarias aisladas con dobles de prueba, sin base de datos, para que las reglas de negocio críticas estén protegidas contra regresiones.

## ADDED Requirements

### Requirement: Aislamiento de las pruebas unitarias
Cada prueba unitaria de un servicio de aplicación SHALL aislar todas sus dependencias (repositorios, reloj, contexto de usuario, transacción, auditoría) con dobles de prueba, y MUST NOT abrir conexiones a la base de datos, a la red ni al sistema de archivos.

#### Scenario: Ejecución sin infraestructura
- **WHEN** se ejecutan las pruebas unitarias en un equipo sin PostgreSQL
- **THEN** todas pasan y ninguna intenta conectarse a un servicio externo

#### Scenario: Resultado determinista
- **WHEN** una prueba depende de la fecha o del usuario
- **THEN** usa un reloj y un contexto simulados, y su resultado no cambia entre ejecuciones

### Requirement: Inventario
Las pruebas de inventario SHALL verificar el registro de mermas (motivo, movimiento de merma, cortesía opcional y auditoría), los ajustes (signo y existencia insuficiente), las tomas físicas (diferencias convertidas en ajustes) y los lotes.

#### Scenario: Merma con reposición sin cobro
- **WHEN** se registra una merma con reposición sin cobro
- **THEN** se genera el movimiento de merma y el de cortesía, y queda registrada la auditoría

#### Scenario: Ajuste que dejaría existencia negativa
- **WHEN** se registra un ajuste negativo mayor que la existencia
- **THEN** la operación se rechaza con un error de regla de negocio y no se guarda nada

#### Scenario: Toma física con diferencias
- **WHEN** se registra una toma física donde el conteo difiere de la existencia
- **THEN** se generan ajustes solo por las líneas con diferencia

### Requirement: Caja
Las pruebas de caja SHALL verificar la apertura de turno (turno ya abierto, fondo negativo, usuario no identificado), los movimientos (monto no positivo, sesión cerrada) y el cierre (esperado, contado, descuadre, denominaciones y desalojo de cuentas con saldo).

#### Scenario: Apertura con turno abierto
- **WHEN** se abre un turno existiendo otro abierto
- **THEN** la operación falla con un conflicto

#### Scenario: Cierre con descuadre
- **WHEN** se cierra un turno cuyo contado es menor que el esperado
- **THEN** el descuadre queda negativo y el turno pasa a cerrado

#### Scenario: Cierre con mesas abiertas
- **WHEN** se cierra un turno con una cuenta con saldo y otra sin saldo
- **THEN** la primera queda por cobrar y la segunda se cierra

### Requirement: Compras
Las pruebas de compras SHALL verificar la creación de órdenes, las transiciones de estado (aprobar, enviar, cancelar) con sus restricciones, la recepción (estados inválidos, líneas ajenas, sobre-recepción, entrada al kardex, costo, cuenta por pagar con vencimiento por días de crédito) y el pago de cuentas por pagar.

#### Scenario: Recepción de orden cancelada
- **WHEN** se intenta recibir mercancía de una orden cancelada
- **THEN** la operación se rechaza con un error de regla de negocio

#### Scenario: Recepción parcial
- **WHEN** se recibe una parte de lo ordenado
- **THEN** la orden queda recibida parcial, el kardex registra la entrada y se crea la cuenta por pagar por lo recibido

#### Scenario: Pago superior al saldo
- **WHEN** se registra un pago mayor que el saldo de la cuenta por pagar
- **THEN** la operación se rechaza y el saldo no cambia

### Requirement: Usuarios
Las pruebas de usuarios SHALL verificar la creación (correo duplicado, contraseña con hash), la edición, la eliminación lógica, el cambio de contraseña y la revocación de sesiones, junto con su auditoría.

#### Scenario: Correo duplicado
- **WHEN** se crea un usuario con un correo existente
- **THEN** la operación falla con un conflicto y no se guarda nada

#### Scenario: Cambio de contraseña
- **WHEN** se cambia la contraseña de un usuario
- **THEN** se guarda el nuevo hash, se revocan sus sesiones y se registra la auditoría sin la contraseña

### Requirement: Auditoría
Las pruebas de auditoría SHALL verificar el registro de entradas (usuario, correo, IP, serialización de los datos y fecha) y la consulta paginada con sus filtros de entidad, usuario y rango de fechas.

#### Scenario: Registro con contexto
- **WHEN** se registra una acción con un usuario autenticado
- **THEN** la entrada guarda su identificador, correo e IP y los datos serializados

#### Scenario: Filtros combinados
- **WHEN** se consulta con entidad y rango de fechas
- **THEN** el servicio pasa ambos filtros al repositorio y devuelve la página mapeada

### Requirement: Reportes
Las pruebas de reportes SHALL verificar la normalización del rango de fechas (valores por defecto, rango invertido), los límites del dashboard (día y mes) y el diagnóstico de salud del inventario con sus conteos por estado.

#### Scenario: Rango por defecto
- **WHEN** se consulta un reporte sin fechas
- **THEN** se usan los últimos 30 días respecto del reloj

#### Scenario: Diagnóstico de inventario
- **WHEN** el repositorio devuelve productos sin stock, óptimos y en exceso
- **THEN** los conteos por estado y las unidades de compra sugeridas son los esperados

### Requirement: Kardex y autenticación
Las pruebas SHALL cubrir también el kardex (movimiento que deja existencia negativa, desglose de recetas, productos de Cocina sin inventario, verificación de stock) y la autenticación (credenciales válidas e inválidas, usuario inactivo, rotación del token de refresco, cierre de sesión).

#### Scenario: Credenciales inválidas
- **WHEN** se inicia sesión con una contraseña incorrecta
- **THEN** el servicio no devuelve tokens y no revela si el usuario existe

#### Scenario: Refresco con token revocado
- **WHEN** se refresca con un token de refresco revocado
- **THEN** la operación no devuelve tokens

### Requirement: Casos exitosos, de borde y de error
Para cada servicio, las pruebas SHALL incluir al menos un caso exitoso, un caso de borde y un caso de error por operación pública con lógica de negocio, y cada prueba MUST verificar el resultado observable y las interacciones relevantes con los dobles.

#### Scenario: Operación sin pruebas de error
- **WHEN** una operación pública con reglas de negocio no tiene un caso de error
- **THEN** la revisión de cobertura la señala como pendiente
