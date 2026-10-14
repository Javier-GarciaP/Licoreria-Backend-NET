# floor-plan-reuse Specification

## Purpose

Permite reutilizar distribuciones del local: duplicar un plano para probar variantes y usar plantillas como punto de partida de planos nuevos, sin afectar el plano vigente.

## Requirements

### Requirement: Duplicar un plano
El sistema SHALL permitir duplicar un plano creando uno nuevo con el nombre indicado, las mismas dimensiones, rejilla y piso, una copia de todos sus elementos con identificadores nuevos y una copia de su imagen de fondo.

#### Scenario: Duplicado correcto
- **WHEN** el administrador duplica un plano de 12 elementos con el nombre "Evento Halloween"
- **THEN** existe un plano nuevo con 12 elementos propios y su propia imagen de fondo

#### Scenario: Nombre por defecto
- **WHEN** se duplica un plano sin indicar nombre
- **THEN** el nuevo plano se llama como el original seguido de "(copia)"

### Requirement: El duplicado nace inactivo
El plano duplicado SHALL crearse inactivo y SHALL NOT desactivar el plano activo vigente.

#### Scenario: Duplicar el plano activo
- **GIVEN** que el plano "Salón principal" es el plano activo
- **WHEN** el administrador lo duplica
- **THEN** "Salón principal" sigue activo y la copia queda inactiva

### Requirement: Independencia entre original y copia
Una vez creada la copia, los cambios en cualquiera de los dos planos, incluida su imagen de fondo, MUST NOT afectar al otro.

#### Scenario: Quitar el fondo del original
- **WHEN** el administrador quita el fondo del plano original
- **THEN** la copia conserva su imagen

### Requirement: Vínculos con mesas al duplicar
Al duplicar un plano que no es plantilla, el sistema SHALL conservar la zona y la mesa operativa vinculadas a cada elemento, porque la copia representa otra distribución del mismo local.

#### Scenario: Elemento de mesa
- **WHEN** se duplica un plano con un elemento enlazado a la mesa 5
- **THEN** el elemento copiado sigue enlazado a la mesa 5

### Requirement: Guardar un plano como plantilla
El sistema SHALL permitir guardar un plano como plantilla, copiando su distribución y su fondo sin los vínculos a mesas operativas.

#### Scenario: Crear plantilla
- **WHEN** el administrador guarda un plano como plantilla "Terraza base"
- **THEN** existe una plantilla con los mismos elementos, ninguno enlazado a una mesa real

### Requirement: Plano nuevo desde plantilla
El sistema SHALL permitir crear un plano nuevo a partir de una plantilla, inactivo y sin vínculos a mesas, listo para editar.

#### Scenario: Usar una plantilla
- **WHEN** el administrador crea un plano desde la plantilla "Terraza base"
- **THEN** el nuevo plano es inactivo, tiene la distribución de la plantilla y no depende de ella

### Requirement: Las plantillas no se activan ni se publican
Una plantilla MUST NOT poder ser el plano activo, SHALL NOT aparecer en el listado público de planos y SHALL ser visible solo para el Administrador.

#### Scenario: Listado público
- **WHEN** un visitante anónimo consulta los planos
- **THEN** la respuesta no incluye plantillas

#### Scenario: Activar una plantilla
- **WHEN** se intenta activar una plantilla
- **THEN** la respuesta es 422 indicando que una plantilla no puede ser el plano activo
