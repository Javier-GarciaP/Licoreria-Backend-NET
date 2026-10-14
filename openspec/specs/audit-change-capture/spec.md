# audit-change-capture Specification

## Purpose

Garantiza que todo cambio de datos del sistema (alta, edición y borrado) quede registrado automáticamente, con quién lo hizo, desde dónde y qué cambió, sin depender de que cada funcionalidad recuerde registrarlo.

## Requirements

### Requirement: Registro de altas, ediciones y borrados
El sistema SHALL registrar una entrada de auditoría por cada alta, edición y borrado de una entidad auditable, con acción, entidad, identificador, usuario, correo, IP y fecha.

#### Scenario: Alta de un registro
- **WHEN** un usuario crea un producto
- **THEN** existe una entrada con acción de creación, la entidad `Producto`, su identificador y el usuario que lo creó

#### Scenario: Borrado lógico
- **WHEN** un usuario elimina un registro que se borra de forma lógica
- **THEN** la entrada registra la acción de eliminación, no una edición

#### Scenario: Cambio sin usuario autenticado
- **WHEN** un cambio ocurre fuera de una petición de usuario (arranque, siembra de datos)
- **THEN** la entrada se registra con el origen "sistema" y sin usuario

### Requirement: Valores antes y después
El sistema SHALL guardar en cada edición los valores anteriores y nuevos de los campos modificados, en el alta los valores iniciales y en el borrado los últimos valores conocidos.

#### Scenario: Edición de un precio
- **WHEN** un usuario cambia el precio de venta de una variante de 10 a 12
- **THEN** la entrada contiene el campo modificado con el valor anterior 10 y el nuevo 12
- **AND** no incluye los campos que no cambiaron

#### Scenario: Edición sin cambios reales
- **WHEN** se guarda una entidad sin que ningún valor cambie
- **THEN** no se genera ninguna entrada

### Requirement: Atomicidad con el cambio auditado
El registro de auditoría MUST guardarse en la misma transacción que el cambio que describe, de modo que ambos se confirmen o se descarten juntos.

#### Scenario: Transacción revertida
- **WHEN** una operación falla y su transacción se revierte
- **THEN** no queda ninguna entrada de auditoría de esa operación

#### Scenario: Conflicto con reintento
- **WHEN** una operación se reintenta tras un conflicto de concurrencia
- **THEN** el cambio final deja una sola entrada, sin duplicados de los intentos fallidos

### Requirement: Datos sensibles protegidos
El sistema MUST NOT guardar en la bitácora contraseñas, hashes ni tokens; los campos sensibles se registran como modificados con un valor enmascarado.

#### Scenario: Cambio de contraseña
- **WHEN** un administrador cambia la contraseña de un usuario
- **THEN** la entrada indica que el campo de contraseña cambió, sin revelar el hash anterior ni el nuevo

### Requirement: Entidades excluidas
El sistema SHALL excluir de la auditoría automática las entidades que son la propia bitácora, los tokens de sesión y los mensajes de chat, para evitar recursión y ruido.

#### Scenario: Guardado de una entrada de auditoría
- **WHEN** el sistema guarda una entrada de auditoría
- **THEN** no se genera otra entrada que la describa

### Requirement: IP de origen real
El sistema SHALL registrar la dirección IP del cliente que originó la petición, incluso cuando pasa por un proxy de confianza.

#### Scenario: Petición a través del proxy
- **WHEN** una petición llega con el encabezado `X-Forwarded-For` desde el proxy de confianza
- **THEN** la entrada registra la IP del cliente y no la del proxy
