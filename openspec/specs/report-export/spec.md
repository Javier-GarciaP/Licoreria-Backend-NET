# report-export Specification

## Purpose

Permite descargar cualquier reporte del sistema como documento PDF o como hoja de cálculo Excel, con los mismos datos y filtros que ve el usuario.

## Requirements

### Requirement: Exportación en PDF y Excel
El sistema SHALL permitir descargar cada reporte exportable en formato PDF y en formato Excel (`.xlsx`), aplicando los filtros y el periodo de la consulta.

#### Scenario: Descargar ventas en Excel
- **WHEN** el usuario exporta el reporte de ventas del periodo en Excel
- **THEN** recibe un archivo `.xlsx` con las mismas filas y totales que muestra la pantalla

#### Scenario: Descargar inventario en PDF
- **WHEN** el usuario exporta el inventario valorizado en PDF
- **THEN** recibe un archivo PDF con el título, la fecha de generación, la tabla y los totales

#### Scenario: Formato no soportado
- **WHEN** se solicita un formato distinto de PDF o Excel
- **THEN** la respuesta es 400 con el detalle del error

### Requirement: Reportes exportables
El sistema SHALL ofrecer exportación para ventas, productos más vendidos, inventario valorizado, compras, propinas, desempeño de mesoneros, caja, ocupación, mermas vs ventas y la bitácora de auditoría.

#### Scenario: Reporte de mesoneros
- **WHEN** el administrador exporta el desempeño de mesoneros
- **THEN** el archivo contiene una fila por mesonero con todos sus indicadores

### Requirement: Contenido del documento
Cada documento SHALL incluir el nombre del local, el título del reporte, el periodo y los filtros aplicados, la fecha y hora de generación y el usuario que lo generó, y los totales cuando corresponda.

#### Scenario: Metadatos
- **WHEN** se abre un documento exportado
- **THEN** se ven el título, el periodo, los filtros, quién lo generó y cuándo

### Requirement: Valores numéricos utilizables
El archivo Excel SHALL guardar los importes, cantidades y fechas como valores numéricos y de fecha con formato, no como texto, para que se puedan ordenar y sumar.

#### Scenario: Sumar una columna
- **WHEN** el usuario suma la columna de importes en Excel
- **THEN** Excel calcula la suma sin convertir texto

### Requirement: Mismos permisos que la consulta
La exportación de un reporte MUST exigir el mismo permiso que su consulta en pantalla.

#### Scenario: Usuario sin permiso
- **WHEN** un usuario sin permiso de lectura de compras exporta el reporte de compras
- **THEN** la respuesta es 403 y no se genera el archivo

### Requirement: Límite de tamaño
El sistema SHALL rechazar con 422 una exportación PDF que supere 5 000 filas indicando que acote el periodo, y SHALL permitir la exportación Excel de hasta 100 000 filas.

#### Scenario: Reporte demasiado grande en PDF
- **WHEN** un reporte PDF tendría más de 5 000 filas
- **THEN** la respuesta es 422 con un mensaje que sugiere acotar el periodo o usar Excel

### Requirement: Descarga autenticada con nombre claro
La descarga SHALL funcionar con la sesión del usuario y entregar un archivo con nombre descriptivo que incluya el reporte y el periodo.

#### Scenario: Nombre del archivo
- **WHEN** se exporta ventas del 1 al 13 de octubre de 2026
- **THEN** el archivo se llama `reporte-ventas-2026-10-01_2026-10-13` con su extensión
