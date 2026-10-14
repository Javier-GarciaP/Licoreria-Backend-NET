# inventory-stock-reservation Specification

## Purpose

Garantiza que el inventario no se sobrevenda cuando varios mesoneros toman pedidos a la vez, comprometiendo (reservando) los insumos al crear la comanda y liberándolos o consumiéndolos según el ciclo de vida del ítem.

## Requirements

### Requirement: Reserva de insumos al tomar la comanda
El sistema SHALL reservar los insumos de cada ítem (incluyendo los de su receta) en la misma transacción que crea la comanda, y MUST rechazar la comanda completa si algún insumo no tiene existencia disponible (cantidad menos reservado).

#### Scenario: Comanda con stock suficiente
- **WHEN** un mesonero agrega una comanda y hay existencia disponible para todos los ítems
- **THEN** la comanda se crea y la cantidad reservada de cada insumo aumenta en lo pedido

#### Scenario: Comanda con un ítem sin existencia
- **WHEN** un mesonero agrega una comanda y un insumo no tiene existencia disponible
- **THEN** el sistema responde 422 indicando el producto, lo requerido y lo disponible
- **AND** no se crea la comanda ni se reserva ningún otro insumo

### Requirement: Sin sobreventa entre mesoneros simultáneos
El sistema MUST garantizar que, ante pedidos simultáneos sobre la misma existencia, la suma de lo reservado nunca supere la cantidad disponible.

#### Scenario: Dos mesoneros piden la última unidad
- **GIVEN** que queda una unidad de un producto
- **WHEN** dos mesoneros crean una comanda con esa unidad al mismo tiempo
- **THEN** exactamente una comanda se crea
- **AND** la otra recibe un error por existencia agotada

#### Scenario: Pedidos simultáneos con stock de sobra
- **GIVEN** que hay cinco unidades de un producto
- **WHEN** dos mesoneros piden una unidad cada uno al mismo tiempo
- **THEN** ambas comandas se crean y quedan dos unidades reservadas

### Requirement: Consumo de la reserva al servir
Al servir un ítem (Preparado o Entregado) el sistema SHALL descontar la existencia en el kardex y liberar la reserva de ese ítem, sin descontar dos veces.

#### Scenario: Ítem servido
- **WHEN** barra marca un ítem reservado como Preparado
- **THEN** la existencia baja en la cantidad del ítem y la cantidad reservada baja en la misma cantidad

#### Scenario: Ítem ya servido
- **WHEN** un ítem servido se vuelve a marcar como servido
- **THEN** no se genera otro movimiento de kardex ni se altera la reserva

### Requirement: Liberación de la reserva
El sistema SHALL liberar la reserva de un ítem cuando se cancela, cuando se desaloja o cierra la mesa sin servirlo, y cuando se cierra el turno con comandas sin servir.

#### Scenario: Ítem cancelado
- **WHEN** se cancela un ítem que estaba reservado y no servido
- **THEN** la cantidad reservada baja en lo que ocupaba el ítem y la existencia no cambia

#### Scenario: Mesa desalojada con ítems pendientes
- **WHEN** se desaloja una mesa con ítems reservados sin servir
- **THEN** todas sus reservas pendientes se liberan

### Requirement: Existencia nunca negativa
El sistema MUST impedir que la existencia o la cantidad reservada queden en valores negativos, y que lo reservado supere la existencia, incluso ante fallos de la lógica de aplicación.

#### Scenario: Intento de dejar existencia negativa
- **WHEN** una operación intentaría dejar la existencia por debajo de cero
- **THEN** la operación falla completa y los datos permanecen sin cambios
