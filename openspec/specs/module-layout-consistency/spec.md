# module-layout-consistency Specification

## Purpose

Mantiene un patrón visual común en todos los módulos.

## Requirements

### Requirement: Listados con toolbar
Los listados SHALL usar una tarjeta con buscador y acción principal en la primera fila, filtros desplegables con "Limpiar filtros" en la segunda, tabla y paginación cuando aplique.

#### Scenario: Filtros activos sin resultados
- **WHEN** los filtros no devuelven filas
- **THEN** el mensaje de vacío indica que no hay resultados con esos filtros

### Requirement: Sin encabezado de página fuera de Salón
Los módulos SHALL NOT mostrar un encabezado de página con título y subtítulo, ni pestañas de sección, salvo Salón.

#### Scenario: Envases abiertos
- **WHEN** el usuario abre Inventario
- **THEN** no hay pestañas y Envases abiertos es un ítem propio del sidebar

### Requirement: Barras de filtros sin desborde
Las tarjetas que solo contienen controles SHALL incluir relleno inferior para que ningún control quede pegado al borde.
