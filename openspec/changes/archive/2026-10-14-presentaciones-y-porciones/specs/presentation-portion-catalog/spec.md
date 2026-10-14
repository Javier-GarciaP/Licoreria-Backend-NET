# Spec Delta

## Purpose

Permite describir en el catálogo cómo se vende cada producto: en presentaciones físicas con existencia propia (botella de 1 L, de 2 L, lata) y en porciones que se sirven abriendo una de esas presentaciones (vaso, trago).

## ADDED Requirements

### Requirement: Varias presentaciones por producto
Un producto de Barra SHALL poder tener varias presentaciones con su propio SKU, precio y existencia, que se compran al proveedor de forma independiente.

#### Scenario: Coca-Cola de 1 L y de 2 L
- **WHEN** el administrador define el producto Coca-Cola con las presentaciones «Botella 1 L» y «Botella 2 L»
- **THEN** cada una guarda su propia existencia y se vende con su propio precio

### Requirement: Naturaleza de la variante
Cada variante SHALL ser una sola de estas naturalezas: presentación (existencia propia), porción (se sirve a partir de presentaciones) o receta (consume insumos de un preparado); el sistema MUST rechazar combinarlas.

#### Scenario: Orígenes en una variante con receta
- **WHEN** se intenta definir orígenes de porción en una variante que ya tiene una receta
- **THEN** la respuesta es 422 indicando que debe elegir una sola naturaleza

#### Scenario: Receta en una porción
- **WHEN** se intenta agregar una receta a una variante que ya es porción
- **THEN** la respuesta es 422

#### Scenario: Presentación convertida en porción
- **WHEN** se intenta definir orígenes en una variante marcada como presentación con existencia propia
- **THEN** la respuesta es 422

### Requirement: Orígenes de una porción
Una porción SHALL declarar uno o más orígenes: presentaciones con existencia propia del mismo producto y cuántas porciones da cada una, como número entero mayor que cero.

#### Scenario: Vaso desde dos botellas
- **WHEN** se define el vaso con origen «Botella 1 L» = 4 vasos y «Botella 2 L» = 8 vasos
- **THEN** la porción queda con dos orígenes y puede servirse de cualquiera de ellas

#### Scenario: Porciones por unidad inválidas
- **WHEN** se define un origen con 0 o con 2,5 porciones por unidad
- **THEN** la respuesta es 422 y el origen no se guarda

#### Scenario: Origen de otro producto
- **WHEN** se intenta usar como origen una presentación de otro producto
- **THEN** la respuesta es 422

### Requirement: La porción no tiene existencia propia
Una porción MUST NOT tener existencia, mínimo ni máximo propios, y su precio de compra SHALL calcularse a partir de sus orígenes para validar que el precio de venta no sea menor que el costo.

#### Scenario: Stock inicial en una porción
- **WHEN** se intenta crear una porción con stock inicial
- **THEN** la respuesta es 422 indicando que la existencia está en sus presentaciones

### Requirement: Recetas con porciones como insumo
Una receta de un preparado SHALL poder usar como insumo una porción además de una presentación, de modo que un cóctel consuma, por ejemplo, un trago de ron.

#### Scenario: Cuba libre con trago y vaso
- **WHEN** se define la receta de un cóctel con 1 trago de ron y 1 vaso de Coca-Cola
- **THEN** la receta se guarda y al venderse consumirá porciones de envases abiertos

### Requirement: Integridad al editar o eliminar
Una presentación que es origen de una porción o tiene envases abiertos SHALL NOT eliminarse ni dejar de ser presentación sin antes retirar esos vínculos o cerrar los envases.

#### Scenario: Eliminar una botella con envase abierto
- **WHEN** se intenta eliminar una presentación con un envase abierto
- **THEN** la respuesta es 409 indicando que primero debe cerrarse o descartarse el envase

### Requirement: Recetas heredadas siguen funcionando
Las variantes con recetas fraccionarias existentes SHALL seguir vendiéndose y descontando como hasta ahora y SHALL identificarse como heredadas, sin convertirse automáticamente.

#### Scenario: Vaso heredado
- **GIVEN** un vaso con receta de 0,125 de una botella de 2 L
- **WHEN** se vende
- **THEN** descuenta 0,125 de la botella como antes

### Requirement: Definición desde el catálogo
El formulario de producto SHALL permitir elegir la naturaleza de cada variante y editar sus orígenes con la presentación y las porciones por unidad, validando los datos antes de guardar.

#### Scenario: Definir un vaso en el formulario
- **WHEN** el administrador elige «Porción» en una variante y agrega los orígenes
- **THEN** el formulario guarda la variante con sus orígenes y muestra cuántas porciones da cada presentación
