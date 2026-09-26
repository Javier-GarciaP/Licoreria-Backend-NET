# Alcance

## Dentro del alcance

### Web pública
- Catálogo de productos con precios y tasas de cambio del día.
- Menú de comida y bebida; generación de menú digital (QR) y físico (PDF).
- Información del local: galería, días laborables, horarios y servicios.
- Reserva de mesas con selección de zona y mesa sobre un plano interactivo.
- Pago de la seña mediante comprobante (PagoMóvil/Zelle) con validación manual.
- Sección de eventos próximos y contacto por WhatsApp.

### Sistema interno
- Gestión de zonas, mesas y plano del local.
- Gestión de reservas, incluidas las tomadas por WhatsApp u otros medios.
- Comandas por área con estados (recibido, preparado, entregado).
- Cuentas de mesa con consumos y abonos; cuentas divididas.
- POS de caja, cierre de turno y arqueo.
- Inventario con kardex, compras, proveedores, mermas y cortesías.
- CRM, fidelización y cuentas por cobrar.
- Facturación fiscal venezolana (número de control, correlativos, IVA/IGTF).
- Tablero de KPIs y reportes.
- Gestión de contenido de la web pública.

### Módulo de IA
- Digitalización del plano desde un croquis en papel.
- Generación de secciones del sitio a partir de plantillas y design tokens.
- Pronóstico de demanda y sugerencias de reabastecimiento.
- Generación de imágenes y posters de eventos.
- Detección de anomalías en caja y mermas.

## Fuera del alcance

- **Múltiples sucursales / multi-tenant.** El sistema es para **una única sucursal**.
- Aplicación móvil nativa (se contempla como evolución futura; la web es PWA).
- Integración directa con pasarelas de pago internacionales (el pago de seña se
  valida manualmente).
- Chatbot conversacional: el módulo de IA no es un asistente de chat.

## Supuestos

- El local opera con conexión a internet estable.
- El personal cuenta con dispositivos (PC/tablet) para el sistema interno.
- La tasa de cambio se registra diariamente de forma manual o desde una fuente externa.

## Restricciones

- Backend bajo **Onion Architecture con .NET** (no negociable).
- Base de datos **PostgreSQL**.
- Frontend en **React + Tailwind CSS**.
- Entregable **académico por fases**, según lo indicado por el docente.
