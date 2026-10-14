# Spec Delta

## Purpose

Permite precargar un croquis como imagen de fondo del lienzo para que el Administrador coloque mesas y elementos sobre la distribución real del local.

## ADDED Requirements

### Requirement: Subir la imagen de fondo
El sistema SHALL permitir al Administrador subir una imagen PNG, JPG o WebP de hasta 5 MB como fondo de un plano, y SHALL devolver el plano actualizado con la dirección de la imagen.

#### Scenario: Subida correcta
- **WHEN** el administrador sube un PNG de 2 MB al plano
- **THEN** el plano queda con la imagen de fondo y la respuesta incluye su dirección

#### Scenario: Plano inexistente
- **WHEN** se sube una imagen a un plano que no existe
- **THEN** la respuesta es 404 y no se guarda ningún archivo

### Requirement: Validación del archivo
El sistema MUST rechazar con 400 los archivos que no sean una imagen de un formato permitido, con contenido que no coincida con su extensión, vacíos o mayores de 5 MB.

#### Scenario: Archivo disfrazado
- **WHEN** se sube un archivo `.png` cuyo contenido no es una imagen PNG
- **THEN** la respuesta es 400 y no se guarda el archivo

#### Scenario: Archivo demasiado grande
- **WHEN** se sube una imagen de 8 MB
- **THEN** la respuesta es 400 indicando el límite de 5 MB

### Requirement: Fondo detrás de los elementos
El fondo SHALL mostrarse debajo de los elementos y zonas del plano en el editor y en la vista de operación del personal, sin interceptar clics ni arrastres.

#### Scenario: Arrastrar sobre el croquis
- **WHEN** el administrador arrastra una mesa sobre la imagen de fondo
- **THEN** se mueve la mesa y no la imagen

### Requirement: Opacidad y ajuste
El sistema SHALL permitir configurar la opacidad del fondo (de 0 a 1) y su ajuste al lienzo (contener, cubrir o estirar), y SHALL conservar esos valores al guardar el plano.

#### Scenario: Ajustar opacidad
- **WHEN** el administrador cambia la opacidad a 0,3 y guarda
- **THEN** al reabrir el plano el fondo se muestra con opacidad 0,3

#### Scenario: Valor fuera de rango
- **WHEN** se intenta guardar una opacidad de 1,5
- **THEN** la respuesta es 400 con el detalle del error

### Requirement: Reemplazar y quitar el fondo
El sistema SHALL permitir reemplazar o quitar la imagen de fondo y SHALL eliminar el archivo anterior del almacenamiento.

#### Scenario: Reemplazo
- **WHEN** el administrador sube una imagen nueva a un plano que ya tenía fondo
- **THEN** el plano usa la nueva imagen y el archivo anterior se elimina

#### Scenario: Quitar el fondo
- **WHEN** el administrador quita el fondo
- **THEN** el plano queda sin imagen y el archivo se elimina

### Requirement: Dirección de imagen solo por subida
El sistema MUST NOT aceptar una dirección de imagen enviada en la edición del plano; la dirección solo se asigna al subir un archivo.

#### Scenario: URL externa en la edición
- **WHEN** una solicitud de edición incluye una dirección de imagen externa
- **THEN** el valor se ignora y el fondo no cambia

### Requirement: Limpieza al eliminar el plano
Al eliminar un plano, el sistema SHALL eliminar también el archivo de su imagen de fondo.

#### Scenario: Plano eliminado
- **WHEN** el administrador elimina un plano con fondo
- **THEN** el archivo de la imagen deja de existir en el almacenamiento
