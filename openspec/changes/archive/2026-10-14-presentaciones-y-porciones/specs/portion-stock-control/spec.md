# Spec Delta

## Purpose

Controla la disponibilidad de las porciones (vasos, tragos) desde que el mesonero las pide hasta que se sirven, sin sobreventa y distinguiendo «agotado» de «falta destapar una botella».

## ADDED Requirements

### Requirement: Disponibilidad de una porción
La disponibilidad de una porción SHALL ser la suma de las porciones restantes en sus envases abiertos menos las porciones reservadas por comandas pendientes.

#### Scenario: Vasos disponibles
- **GIVEN** un envase abierto con 6 vasos restantes y 2 vasos reservados
- **WHEN** se consulta la disponibilidad
- **THEN** hay 4 vasos disponibles

### Requirement: Reserva de porciones al pedir
Al pedir una porción, el sistema SHALL reservar las porciones solicitadas en la misma transacción que crea la comanda, y MUST rechazar la comanda completa si no hay porciones disponibles en envases abiertos.

#### Scenario: Pedido con vasos disponibles
- **GIVEN** 4 vasos disponibles
- **WHEN** un mesonero pide 2 vasos
- **THEN** la comanda se crea y quedan 2 vasos disponibles

#### Scenario: Sin envase abierto pero con botellas cerradas
- **GIVEN** que no hay envases abiertos de vaso y existen 3 botellas cerradas que pueden abrirse
- **WHEN** un mesonero pide un vaso
- **THEN** la respuesta es 422 con el código `porciones-sin-abrir` indicando que debe pedir a barra que destape una botella

#### Scenario: Sin envase abierto ni botellas
- **GIVEN** que no hay envases abiertos ni presentaciones con existencia para abrir
- **WHEN** un mesonero pide un vaso
- **THEN** la respuesta es 422 con el código `existencia-agotada`

### Requirement: Sin sobreventa de porciones
Ante pedidos simultáneos de la misma porción, la suma reservada MUST NOT superar las porciones abiertas, y los pedidos que no quepan SHALL recibir 422.

#### Scenario: Dos mesoneros piden el último vaso
- **GIVEN** un solo vaso disponible
- **WHEN** dos mesoneros lo piden al mismo tiempo
- **THEN** exactamente una comanda se crea y la otra recibe 422

### Requirement: Consumo al servir
Al servir un ítem de porción, el sistema SHALL descontar las porciones del envase abierto más antiguo con porciones restantes, pasando al siguiente cuando se agote, registrar el consumo en el kardex de la porción y liberar la reserva.

#### Scenario: Servir 3 vasos con dos envases abiertos
- **GIVEN** un envase antiguo con 2 vasos restantes y otro más reciente con 8
- **WHEN** se sirven 3 vasos
- **THEN** el envase antiguo queda agotado y el reciente queda con 7

#### Scenario: Servir dos veces el mismo ítem
- **WHEN** un ítem servido cambia de Preparado a Entregado
- **THEN** no se descuentan porciones por segunda vez

### Requirement: Liberación de la reserva
La reserva de porciones SHALL liberarse al cancelar el ítem, desalojar la mesa, cerrar la cuenta o cerrar el turno, sin modificar los envases.

#### Scenario: Cancelar un vaso pedido
- **WHEN** se cancela un vaso pedido y aún no servido
- **THEN** las porciones disponibles vuelven a contar ese vaso

### Requirement: Venta directa de porciones
Una venta directa en el POS SHALL consumir de inmediato las porciones de los envases abiertos y MUST rechazarse con 422 si no alcanzan, sin tocar las porciones reservadas por mesas.

#### Scenario: Venta de un trago desde el mostrador
- **GIVEN** un envase abierto de trago con 5 porciones y 5 reservadas por mesas
- **WHEN** el cajero vende un trago en el POS
- **THEN** la respuesta es 422 porque las porciones están comprometidas

### Requirement: Porciones dentro de recetas
Cuando un preparado usa una porción como insumo, el sistema SHALL reservar y consumir esas porciones con las mismas reglas, multiplicadas por la cantidad pedida.

#### Scenario: Cuba libre
- **GIVEN** una receta con 1 trago de ron y 1 vaso de Coca-Cola
- **WHEN** se piden 2 cuba libres
- **THEN** se reservan 2 tragos de ron y 2 vasos de Coca-Cola, y el pedido se rechaza si falta alguno

### Requirement: Lo servido no se reintegra
Una porción ya servida MUST NOT volver a un envase al cancelar o devolver el ítem, porque no puede recuperarse; el sistema SHALL registrar el hecho sin modificar los envases.

#### Scenario: Cancelar un vaso ya servido
- **WHEN** se cancela un ítem de vaso que ya se había servido
- **THEN** las porciones de los envases no cambian y la cancelación se completa

### Requirement: Integridad de los envases
Las porciones restantes de un envase MUST NOT ser negativas ni superar las iniciales, y las porciones reservadas de una porción MUST NOT ser negativas, aunque falle la lógica de aplicación.

#### Scenario: Consumo mayor al restante
- **WHEN** una operación intentaría dejar porciones restantes negativas
- **THEN** la operación falla completa y los datos permanecen sin cambios
