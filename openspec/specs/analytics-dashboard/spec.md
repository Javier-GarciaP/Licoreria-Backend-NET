# analytics-dashboard Specification

## Purpose

Presenta los indicadores en un Dashboard y en Reportes con un periodo común, gráficas legibles y una navegación de Analítica disponible para quien debe usarla.

## Requirements

### Requirement: Selector de periodo común
El Dashboard SHALL ofrecer los periodos hoy, últimos 7 días, últimos 30 días, mes actual y rango personalizado, y SHALL actualizar todas sus tarjetas y gráficas con el periodo elegido.

#### Scenario: Cambiar el periodo
- **WHEN** el administrador elige "Últimos 30 días"
- **THEN** todas las tarjetas y gráficas muestran datos de ese periodo sin recargar la página

#### Scenario: Rango personalizado inválido
- **WHEN** el administrador elige un rango con fecha final anterior a la inicial
- **THEN** la interfaz no consulta y muestra el motivo

### Requirement: Tarjetas de KPIs nuevos
El Dashboard SHALL mostrar tarjetas con ticket promedio, productos más vendidos, ocupación de mesas, tasa de no asistencia, descuadre de caja y mermas, cada una con su valor del periodo.

#### Scenario: Datos disponibles
- **WHEN** el Dashboard carga con datos
- **THEN** cada tarjeta muestra su valor y la unidad correspondiente

#### Scenario: Sin datos
- **WHEN** un indicador no tiene datos en el periodo
- **THEN** su tarjeta muestra un estado vacío legible, no un cero engañoso ni un error

### Requirement: Gráficas de tamaño uniforme y legible
Las gráficas SHALL tener alturas consistentes y suficientes para leerse en escritorio y móvil, con etiquetas y tooltips, y SHALL mostrar un esqueleto de carga mientras esperan datos.

#### Scenario: Pantalla móvil
- **WHEN** el Dashboard se abre en un ancho de 375 px
- **THEN** las gráficas se apilan en una columna sin desbordar horizontalmente

### Requirement: Reportes por pestaña
La página de Reportes SHALL agrupar los reportes en pestañas (ventas, productos, inventario, compras, propinas, mesoneros, caja, ocupación y mermas) con el mismo selector de periodo.

#### Scenario: Cambiar de pestaña
- **WHEN** el usuario cambia de pestaña
- **THEN** se conserva el periodo elegido y se cargan solo los datos de esa pestaña

### Requirement: Analítica visible según rol
El grupo Analítica y el ítem Reportes SHALL aparecer en la navegación del Administrador, y los indicadores por mesonero MUST quedar fuera de la vista de los demás roles.

#### Scenario: Navegación del administrador
- **WHEN** un administrador abre la aplicación
- **THEN** ve Analítica con Reportes en el menú

#### Scenario: Navegación de un cajero
- **WHEN** un cajero abre la aplicación
- **THEN** no ve la pestaña de mesoneros ni accede por URL
