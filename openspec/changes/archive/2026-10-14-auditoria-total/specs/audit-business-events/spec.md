# Spec Delta

## Purpose

Registra de forma explícita las acciones con significado de negocio o de seguridad que no se reducen a un cambio de datos, y permite distinguirlas de los cambios automáticos.

## ADDED Requirements

### Requirement: Tipo de entrada
Cada entrada de auditoría SHALL tener un tipo que distinga un cambio de datos registrado automáticamente de un evento de negocio o de acceso registrado explícitamente.

#### Scenario: Filtrar por tipo
- **WHEN** el Administrador consulta la bitácora filtrando por eventos
- **THEN** solo ve las entradas de eventos de negocio o de acceso

### Requirement: Eventos de negocio conservados
El sistema SHALL registrar como eventos las operaciones con semántica propia: apertura y cierre de caja, devolución, recepción de compra, merma, toma física, cambio de contraseña y revocación de sesiones.

#### Scenario: Cierre de caja
- **WHEN** un cajero cierra el turno
- **THEN** existe un evento de cierre de caja con el esperado, lo contado y el descuadre

### Requirement: Eventos de acceso
El sistema SHALL registrar el inicio de sesión, el cierre de sesión y los intentos de inicio de sesión fallidos, incluyendo el correo usado y la IP, sin guardar la contraseña.

#### Scenario: Inicio de sesión correcto
- **WHEN** un usuario inicia sesión con credenciales válidas
- **THEN** se registra un evento de acceso con su usuario e IP

#### Scenario: Intento fallido
- **WHEN** alguien intenta iniciar sesión con credenciales inválidas
- **THEN** se registra un evento de intento fallido con el correo usado y la IP
- **AND** la respuesta al cliente no revela si el correo existe

### Requirement: Sin duplicados con los cambios automáticos
El sistema SHALL NOT registrar como evento una acción que ya queda completamente descrita por un cambio automático de datos.

#### Scenario: Edición de un usuario
- **WHEN** un administrador edita un usuario
- **THEN** existe una entrada de cambio automática y ninguna entrada de evento equivalente
