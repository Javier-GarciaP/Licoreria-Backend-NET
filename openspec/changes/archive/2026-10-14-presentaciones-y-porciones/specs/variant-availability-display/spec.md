# Spec Delta

## Purpose

Muestra al personal, al elegir una variante, cuánto se puede servir y qué hacer cuando falta algo, para que el pedido no falle por sorpresa.

## ADDED Requirements

### Requirement: Disponibilidad por variante
Al listar un producto, el sistema SHALL informar por cada variante su naturaleza y su disponibilidad: para una presentación, las unidades disponibles; para una porción, las porciones disponibles en envases abiertos y las presentaciones cerradas que pueden abrirse.

#### Scenario: Producto con presentaciones y vaso
- **GIVEN** Coca-Cola con 3 botellas de 1 L, 5 de 2 L y un vaso con 4 porciones abiertas
- **WHEN** se consulta el producto
- **THEN** la botella de 1 L informa 3, la de 2 L informa 5 y el vaso informa 4 porciones disponibles y 8 botellas cerradas abribles

### Requirement: Selector de variantes del POS y del mesonero
El POS y la carta del mesonero SHALL mostrar todas las variantes vendibles de un producto con su precio y disponibilidad, y no permitir pedir una variante sin existencia ni porciones disponibles.

#### Scenario: Elegir presentación
- **WHEN** el mesonero toca Coca-Cola, que tiene varias variantes
- **THEN** ve «Botella 1 L», «Botella 2 L» y «Vaso» con su precio y su disponibilidad

#### Scenario: Variante agotada
- **WHEN** una variante no tiene disponibilidad
- **THEN** aparece deshabilitada con el texto «Agotado»

### Requirement: Mensajes diferenciados
Cuando una porción no pueda pedirse, la interfaz SHALL distinguir «Sin abrir» (hay botellas cerradas: pedir a barra que destape una) de «Agotado» (no hay existencia).

#### Scenario: Vaso sin envase abierto
- **GIVEN** botellas cerradas pero ningún envase abierto
- **WHEN** el mesonero intenta pedir un vaso
- **THEN** ve el aviso «Sin abrir: pide a barra que destape una botella» y el pedido no se envía

### Requirement: Panel de envases abiertos para la barra
La pantalla de barra SHALL mostrar los envases abiertos con sus porciones restantes y permitir destapar una presentación para una porción y descartar un envase, según el permiso del usuario.

#### Scenario: Destapar desde la barra
- **WHEN** el barman elige «Destapar», la porción vaso y la botella de 2 L
- **THEN** el panel muestra un nuevo envase con 8 vasos y la botella de 2 L baja en una unidad

### Requirement: Envases abiertos en Existencias
La pantalla de Existencias del administrador SHALL incluir una pestaña con los envases abiertos y su historial reciente de aperturas y descartes.

#### Scenario: Consultar la pestaña
- **WHEN** el administrador abre la pestaña de envases abiertos
- **THEN** ve lo abierto, quién lo abrió y cuándo

### Requirement: Datos al día
Las pantallas SHALL refrescar la disponibilidad cuando otra persona destape, sirva o cancele, y ante un rechazo por falta de porciones SHALL conservar el pedido en pantalla.

#### Scenario: Otro mesonero toma el último vaso
- **WHEN** otro mesonero toma el último vaso mientras se arma un pedido
- **THEN** al enviar se muestra el aviso, se refresca la disponibilidad y el pedido sigue en pantalla
