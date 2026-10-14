# Spec Delta

## Purpose

Reserva al Administrador la edición de la distribución del local (planos, zonas y mesas), manteniendo la consulta de solo lectura para quienes operan el salón.

## ADDED Requirements

### Requirement: Permiso de distribución
El sistema SHALL definir un permiso de edición de distribución asignado únicamente al Administrador, separado del permiso de gestión del club que protege la lista VIP y las entradas.

#### Scenario: Host con gestión del club
- **WHEN** un Host administra la lista VIP
- **THEN** la operación sigue permitida, porque ese permiso no cambia

### Requirement: Escritura de planos solo para el Administrador
La creación, edición, duplicado, eliminación, subida de fondo y guardado como plantilla de planos MUST exigir el permiso de distribución; quien no lo tenga SHALL recibir 403.

#### Scenario: Host edita un plano
- **WHEN** un Host intenta editar un plano
- **THEN** la respuesta es 403 y el plano no cambia

#### Scenario: Administrador edita un plano
- **WHEN** un administrador edita un plano
- **THEN** la operación se completa

### Requirement: Zonas y mesas solo para el Administrador
La creación, edición y eliminación de zonas y mesas MUST exigir el permiso de distribución, mientras que desalojar una mesa SHALL conservar su permiso actual.

#### Scenario: Mesero desaloja una mesa
- **WHEN** un mesero desaloja una mesa con su cuenta
- **THEN** la operación sigue permitida

#### Scenario: Host crea una mesa
- **WHEN** un Host intenta crear una mesa
- **THEN** la respuesta es 403

### Requirement: Consulta de solo lectura
Los usuarios con permiso de lectura del club SHALL poder consultar el plano activo, las zonas y las mesas, y la consulta pública de planos no activos ni plantillas MUST NOT exponer más de lo que expone hoy.

#### Scenario: Mesero consulta el plano
- **WHEN** un mesero abre el plano activo
- **THEN** ve la distribución y el estado de las mesas, sin controles de edición

### Requirement: Interfaz coherente con los permisos
La interfaz SHALL ocultar los controles de edición, duplicado, plantillas y fondo a quien no tiene el permiso de distribución, y la ruta del editor MUST estar protegida; la API sigue siendo la autoridad.

#### Scenario: Host abre el salón
- **WHEN** un Host abre la sección Salón
- **THEN** no ve botones de crear, editar, duplicar ni eliminar planos o mesas

#### Scenario: Acceso directo por URL
- **WHEN** un Host abre directamente la dirección del editor de un plano
- **THEN** es redirigido a una pantalla permitida
