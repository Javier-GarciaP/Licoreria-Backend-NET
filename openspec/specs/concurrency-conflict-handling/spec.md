# concurrency-conflict-handling Specification

## Purpose

Define cómo responde el sistema ante conflictos de concurrencia y violaciones de unicidad: reintento acotado, códigos HTTP uniformes en RFC 7807 y mensajes recuperables en las interfaces.

## Requirements

### Requirement: Reintento acotado ante conflicto de versión
El sistema SHALL reintentar, hasta 3 veces, una operación transaccional que falle por conflicto de concurrencia, recargando y revalidando los datos en cada intento.

#### Scenario: Conflicto transitorio
- **WHEN** dos operaciones modifican la misma existencia y la segunda detecta un conflicto de versión
- **AND** tras recargar todavía hay existencia suficiente
- **THEN** la segunda operación se completa sin que el usuario perciba error

#### Scenario: Reintentos agotados
- **WHEN** una operación sigue en conflicto después de los reintentos permitidos
- **THEN** el sistema responde 409 y no deja cambios parciales

### Requirement: Respuesta 409 para conflictos
El sistema MUST responder 409 con un cuerpo `application/problem+json` (RFC 7807) cuando una operación no puede aplicarse porque otra la modificó, o porque viola una restricción de unicidad.

#### Scenario: Conflicto de versión
- **WHEN** un cliente edita un registro que otro ya modificó
- **THEN** la respuesta es 409 con título, detalle legible y sin datos técnicos internos

#### Scenario: Violación de unicidad
- **WHEN** una operación intenta crear un registro duplicado protegido por una restricción de unicidad
- **THEN** la respuesta es 409 y no se registra el duplicado

### Requirement: Distinción entre conflicto y regla de negocio
El sistema SHALL responder 422 cuando la causa es una regla de negocio (por ejemplo existencia agotada) y 409 cuando la causa es una carrera de concurrencia, de modo que el cliente pueda reaccionar de forma distinta.

#### Scenario: Existencia agotada de verdad
- **WHEN** tras recargar ya no queda existencia para el pedido
- **THEN** la respuesta es 422 con el producto y las cantidades involucradas

### Requirement: Mensajes recuperables en el cliente
Las interfaces SHALL mostrar un mensaje comprensible ante un 409 o un 422 de existencia, refrescar los datos afectados y conservar lo que el usuario estaba haciendo.

#### Scenario: Conflicto en el punto de venta
- **WHEN** el mesonero envía una comanda y recibe un 409
- **THEN** la interfaz informa que otra operación cambió los datos, refresca la cuenta y la existencia, y mantiene el pedido en pantalla
