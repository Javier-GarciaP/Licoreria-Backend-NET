# audit-log-viewer Specification

## Purpose

Permite al Administrador consultar la bitácora de auditoría para saber quién hizo qué, cuándo y qué cambió, con filtros y detalle por entrada.

## Requirements

### Requirement: Acceso restringido al Administrador
La consulta de la bitácora MUST estar disponible solo para el rol Administrador; los demás usuarios SHALL recibir 403 y no ver la opción en la navegación.

#### Scenario: Empleado sin permiso
- **WHEN** un cajero solicita la bitácora
- **THEN** la respuesta es 403

#### Scenario: Navegación del Administrador
- **WHEN** un administrador abre la aplicación
- **THEN** la opción Auditoría aparece en el menú de Analítica

### Requirement: Filtros combinables
El listado SHALL permitir filtrar por usuario, entidad, acción, tipo y rango de fechas, y combinar los filtros.

#### Scenario: Filtrar por usuario y fechas
- **WHEN** el administrador filtra por un usuario y un rango de fechas
- **THEN** el listado muestra solo las entradas de ese usuario dentro del rango

#### Scenario: Sin resultados
- **WHEN** ningún registro cumple los filtros
- **THEN** la vista muestra un estado vacío y la opción de limpiar filtros

### Requirement: Orden y paginación
El listado SHALL mostrar primero las entradas más recientes y paginar los resultados.

#### Scenario: Muchas entradas
- **WHEN** existen más entradas que el tamaño de página
- **THEN** el listado devuelve la primera página con el total de páginas y permite avanzar

### Requirement: Detalle antes y después
El sistema SHALL permitir abrir una entrada y ver, campo por campo, el valor anterior y el nuevo, junto con usuario, IP, entidad e identificador.

#### Scenario: Ver una edición
- **WHEN** el administrador abre una entrada de edición
- **THEN** ve una tabla con cada campo modificado, su valor anterior y su valor nuevo

#### Scenario: Ver un alta o un borrado
- **WHEN** el administrador abre una entrada de alta o de borrado
- **THEN** ve los valores iniciales o los últimos valores conocidos de la entidad

### Requirement: Inmutabilidad de la bitácora
El sistema MUST NOT ofrecer ninguna operación para editar ni eliminar entradas de auditoría.

#### Scenario: Intento de borrar una entrada
- **WHEN** se intenta eliminar o modificar una entrada mediante la API
- **THEN** la API no expone esa operación y responde 404 o 405
