# open-container-management Specification

## Purpose

Permite a la barra destapar una botella o envase para servir porciones, saber cuáles hay abiertos y descartar uno, dejando cada acción registrada en el kardex.

## Requirements

### Requirement: Destapar un envase
El sistema SHALL permitir destapar una presentación elegida para servir una porción: descuenta una unidad de esa presentación, registra un movimiento de apertura en el kardex y crea un envase abierto con las porciones que da según el origen definido.

#### Scenario: Destapar la botella de 1 L para vasos
- **GIVEN** «Botella 1 L» con 5 unidades y el vaso con origen 4 vasos por botella de 1 L
- **WHEN** la barra destapa una botella de 1 L para vasos
- **THEN** la existencia de «Botella 1 L» baja a 4 y existe un envase abierto con 4 vasos restantes

#### Scenario: Elegir de cuál destapar
- **GIVEN** que el vaso puede salir de la botella de 1 L o de 2 L
- **WHEN** la barra elige destapar la de 2 L
- **THEN** el envase abierto tiene 8 vasos y la botella de 2 L baja en una unidad

### Requirement: Sin presentación disponible no se destapa
El sistema MUST rechazar con 422 destapar una presentación sin existencia disponible (cantidad menos reservado) o que no sea origen de la porción indicada.

#### Scenario: Botella agotada
- **WHEN** la barra intenta destapar una presentación con existencia cero
- **THEN** la respuesta es 422 y no se crea ningún envase

#### Scenario: Origen no definido
- **WHEN** se intenta destapar una presentación que no es origen del vaso
- **THEN** la respuesta es 422

### Requirement: Listado de envases abiertos
El sistema SHALL listar los envases abiertos con la porción, la presentación de origen, las porciones iniciales y restantes, quién lo abrió y cuándo, con el más antiguo primero.

#### Scenario: Consultar lo abierto
- **WHEN** la barra consulta los envases abiertos
- **THEN** ve cada envase con sus porciones restantes y su hora de apertura

### Requirement: Cierre automático al agotarse
Un envase abierto SHALL pasar a agotado cuando sus porciones restantes llegan a cero y dejar de listarse como abierto.

#### Scenario: Última porción
- **WHEN** se sirve la última porción de un envase
- **THEN** el envase queda agotado y no cuenta como abierto

### Requirement: Descartar un envase abierto
El sistema SHALL permitir descartar un envase abierto con un motivo, registrando como merma las porciones restantes y retirándolo de los abiertos.

#### Scenario: Botella derramada
- **GIVEN** un envase abierto con 3 vasos restantes
- **WHEN** la barra lo descarta por derrame
- **THEN** se registra una merma de 3 vasos con el motivo y el envase deja de estar abierto

#### Scenario: Descartar con porciones reservadas
- **WHEN** descartar el envase dejaría menos porciones abiertas que las reservadas por comandas pendientes
- **THEN** la respuesta es 409 indicando que hay pedidos pendientes que dependen de esas porciones

### Requirement: Permiso de apertura
Destapar y descartar MUST exigir el permiso `inventory:open`, asignado a Administrador, Barra y Cajero; los demás roles SHALL recibir 403 y solo pueden consultar.

#### Scenario: Mesero intenta destapar
- **WHEN** un mesero intenta destapar una botella
- **THEN** la respuesta es 403

### Requirement: Destapar simultáneamente la última botella
Si dos personas destapan al mismo tiempo la última unidad de una presentación, el sistema SHALL abrir un solo envase y SHALL responder al otro con un conflicto o con existencia agotada, sin dejar existencia negativa.

#### Scenario: Dos barras, una botella
- **GIVEN** una sola botella de 1 L
- **WHEN** dos usuarios la destapan al mismo tiempo
- **THEN** se crea un envase y la otra solicitud recibe 409 o 422
