# cashier-minimal-navigation Specification

## Purpose

Limita la interfaz del Cajero a lo que necesita para vender y cobrar.

## Requirements

### Requirement: Sin Dashboard ni gestión
El rol Cajero SHALL NOT ver el Dashboard, Categorías y marcas, Unidades, Kardex ni Tasas de cambio, y SHALL iniciar en el POS.

#### Scenario: Cajero inicia sesión
- **WHEN** un cajero inicia sesión
- **THEN** llega al POS y su navegación incluye POS, Ventas, Catálogo, Existencias, Envases abiertos, Mermas y cortesías, Caja, Clientes y Cuentas por cobrar (y Cuentas en modo Discoteca)

#### Scenario: Acceso directo al Dashboard
- **WHEN** un cajero abre la ruta del Dashboard
- **THEN** es redirigido a una pantalla permitida
