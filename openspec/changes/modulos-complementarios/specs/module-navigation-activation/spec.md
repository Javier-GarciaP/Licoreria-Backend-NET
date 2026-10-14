# Spec Delta

## Purpose

Define qué módulos complementarios ve cada usuario según el modo de operación y su rol, de modo que la interfaz solo muestre pantallas que la API le permite usar.

## ADDED Requirements

### Requirement: Módulos visibles en ambos modos
Promociones, Existencias, Clientes, Cuentas por cobrar y Compras SHALL aparecer en la navegación tanto en modo Licorería como en modo Discoteca para los roles autorizados.

#### Scenario: Cajero en modo Licorería
- **WHEN** un cajero abre la aplicación en modo Licorería
- **THEN** ve Existencias, Clientes y Cuentas por cobrar, y no ve Compras

#### Scenario: Administrador en modo Discoteca
- **WHEN** un administrador abre la aplicación en modo Discoteca
- **THEN** ve Promociones, Existencias, Clientes, Cuentas por cobrar y Compras

### Requirement: Módulos exclusivos de Discoteca
Entradas y Lista VIP SHALL aparecer solo en modo Discoteca y solo para Administrador y Host.

#### Scenario: Modo Licorería
- **WHEN** un administrador cambia a modo Licorería
- **THEN** Entradas y Lista VIP no aparecen y su dirección directa redirige a una pantalla permitida

#### Scenario: Host en modo Discoteca
- **WHEN** un Host abre la aplicación en modo Discoteca
- **THEN** ve Entradas y Lista VIP

### Requirement: Promociones solo para el Administrador
La gestión de promociones (crear, editar, eliminar) SHALL estar reservada al Administrador mediante un permiso propio, y los roles que venden SHALL poder consultar las promociones vigentes desde el POS.

#### Scenario: Mesero crea una promoción
- **WHEN** un mesero intenta crear una promoción por la API
- **THEN** la respuesta es 403 y no se crea

#### Scenario: Cajero aplica una promoción
- **WHEN** un cajero abre el POS
- **THEN** puede elegir una promoción vigente en la orden

### Requirement: Acceso a Clientes para el Cajero
El Cajero SHALL poder consultar y registrar clientes, puntos y pagos de cuentas por cobrar, de modo que las pantallas que la navegación le muestra no respondan 403.

#### Scenario: Cajero abre Clientes
- **WHEN** un cajero abre la pantalla de Clientes
- **THEN** la lista carga y puede crear un cliente

#### Scenario: Mesero consulta clientes
- **WHEN** un mesero solicita la lista de clientes
- **THEN** la respuesta es 403

### Requirement: Navegación coherente con la API
Ninguna opción de la navegación SHALL mostrarse a un rol que no tenga el permiso necesario para cargar su pantalla.

#### Scenario: Revisión de la matriz
- **WHEN** se recorre cada módulo reactivado con cada rol
- **THEN** quien ve el módulo recibe datos y quien no tiene permiso no lo ve ni accede por dirección directa

### Requirement: Módulos aún ocultos
Los lotes, las tomas físicas y el contenido de marca SHALL seguir fuera de la navegación en este cambio.

#### Scenario: Navegación del administrador
- **WHEN** un administrador revisa el menú
- **THEN** no aparecen lotes, tomas físicas ni Marca y contenido
