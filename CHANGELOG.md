# Changelog

Todos los cambios relevantes de este proyecto se documentan en este archivo.

El formato esta basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/)
y el proyecto se adhiere a [Versionado Semantico](https://semver.org/lang/es/).

> **Nota:** Las fases de desarrollo academico se rigen por lo indicado por el docente.
> Este changelog registra la evolucion tecnica del repositorio (estructura, documentacion,
> infraestructura y refactorizaciones), independientemente de dichas fases.

## [No publicado]

### Agregado

- **Frontend (Fase 4):** monorepo de workspaces con `apps/admin` (SPA interna),
  `apps/public-web` (web pública) y paquetes `types`, `api-client`, `ui` y `config`.
  Context API (`AuthContext`, `ThemeContext` Azul UNET/Oscuro, `ModoContext`
  Licorería/Discoteca), guards de RBAC, dashboard KPI (mapa de calor, salud de
  inventario, mermas vs. ventas), plano interactivo SVG, cuentas con pagos mixtos,
  KDS de barra en tiempo real y cliente API tipado con refresh de JWT y RFC 7807.
- **Reportes de operación:** `GET /api/v1/reportes/heatmap`,
  `/inventario-salud` y `/mermas-vs-ventas`.
- **Desalojo de mesa en tiempo real:** `POST /api/v1/mesas/{id}/desalojar` y evento
  SignalR `mesa:actualizada` para sincronizar el plano de mesas entre clientes.
- **Orquestación Docker completa:** `Dockerfile` de la API y de las SPA, servicios
  `api`, `admin`, `public-web` y `edge` (nginx) en `docker-compose.yml`.
- **Pruebas con Moq:** `ServicioCatalogoProductoTests` aísla `IProductoRepository`.
- **Modificadores/extras:** entidades `modificador` y `producto_modificadores`, CRUD del
  catálogo y asignación a productos con límites de selección (min/max/requerido).
- **Reportes:** ventas por período/usuario/método de pago, inventario valorizado,
  compras y cuentas por pagar, y propinas por usuario (`/api/v1/reportes/*`).
- **Contrato OpenAPI generado en build:** `Microsoft.AspNetCore.OpenApi`
  (`OpenApiGenerateDocuments`) exporta `openapi/licoreria.yaml`; tareas
  `task backend:openapi` y `task backend:openapi:check` y verificación de paridad en CI.
- **Completitud de la API:** compras (proveedores, órdenes, recepciones y cuentas por
  pagar), precios por lista y moneda, tablero de KPIs, menú PDF (QuestPDF) y contacto
  por WhatsApp, división de cuentas y promociones, lotes/vencimientos y tomas físicas,
  lista VIP, entradas con QR y pedido anticipado.
- **Endurecimiento:** auditoría (`audit_log`) de acciones sensibles, revocación de sesiones
  al cambiar contraseña, concurrencia optimista con `xmin` en todas las entidades,
  rate limiting global y en autenticación, y SignalR por área (barra/cocina/meseros).
- **Pruebas:** proyecto de pruebas de integración (`WebApplicationFactory` + PostgreSQL) y
  colección Postman de la API final; CI del backend con servicio PostgreSQL.
- **API final (Fase 2 tecnica):** implementacion completa del nucleo operativo y la
  web publica sobre PostgreSQL, manteniendo Onion Architecture al 100%.
  - Transversales: paginacion/filtrado, errores `409`/`422` en RFC 7807, Swagger con
    Bearer, health checks, `UnitOfWork`, interceptor de auditoria (`created_by`/`updated_by`)
    y puertos de almacenamiento, reloj y notificaciones.
  - Seguridad: refresh tokens con rotacion/revocacion, `/auth/me`, permisos `modulo:accion`
    como claims y politicas por permiso; CRUD de usuarios y catalogos de roles/permisos.
  - Catalogo: producto -> variante -> codigos de barras, categorias jerarquicas, marcas,
    unidades, impuestos, listas de precio y recetas de productos preparados.
  - Inventario: stock por variante, kardex inmutable, mermas/cortesias, ajustes y reporte.
  - Finanzas: tasas persistidas (BCV/paralelo), monedas y movimientos de tesoreria.
  - Ventas/POS: ventas con pago mixto, comprobante fiscal, devoluciones y descuento de
    inventario en una transaccion.
  - Cuentas y comandas: sesion de mesa, cuentas, abonos, comandas por area y cierre.
  - Tiempo real: hub SignalR `/hubs/comandas` con notificaciones de comandas e items.
  - Caja: sesiones, movimientos, arqueo por denominaciones y reporte de cierre (Z).
  - Club: zonas, mesas, planos, reservas con sena y validacion, y eventos.
  - CRM: clientes, puntos de fidelidad y cuentas por cobrar.
  - Contenido: paginas/secciones/bloques, horarios, info del local, menu digital, QR y
    almacenamiento local de archivos.
  - IA: modulo simulado con trabajos, historial y aprobacion humana.
- Guia de uso de la API para el frontend en `docs/dev/04-api/guia-frontend.md`.
- Contrato OpenAPI regenerado desde el codigo (`openapi/licoreria.yaml`).
- Pruebas unitarias adicionales de la logica de dominio operativa (stock, cuentas,
  fidelidad y cuentas por cobrar).

### Agregado (fases previas)

- Estructura base de monorepo (`backend`, `frontend`, `database`, `docs`, `infra`, `openapi`).
- Archivos de configuracion raiz (`.editorconfig`, `Taskfile.yml`, `docker-compose.yml`, `.env.example`).
- Guia de contribucion, convencion de commits y plantilla `.gitmessage`.
- Documentacion tecnica completa en `docs/dev` (vision, requerimientos, arquitectura,
  base de datos, API, seguridad, modulos, frontend, IA, operaciones, fases y ADRs).
- Contrato OpenAPI 3 de la API v1 en `openapi/licoreria.yaml`.
- Sitio de documentacion Astro Starlight con secciones *Desarrolladores* y *Cliente*.
- Workflows de GitHub Actions para el sitio de documentacion y el backend.
- Plantillas de Pull Request, issues y `CODEOWNERS`.
- Encapsulamiento de `Producto` con `Sku`, `StockMaximo` y metodos de dominio.
- Enums `RolUsuario` y `EstadoSaludStock`.
- Servicios de dominio: salud de stock, generador de SKU, cuenta/abonos,
  merma/cortesia, conversion de moneda, estados de comanda y conflictos de reserva.
- DTOs, interfaces de servicios y validadores con FluentValidation.
- Controlador de simulacion (`/api/v1/simulacion`) para la logica de dominio.
- Proyecto de pruebas `Licoreria.UnitTests` (xUnit) con 45 pruebas.
- Fase 2: tablas en minusculas, `Descripcion` en `Producto`, PostgreSQL 15 y migracion
  `InitialInfrastructureCatalog`.
- Fase 3: autenticacion JWT (`POST /api/auth/login`), `IPasswordHasher` (PBKDF2),
  `ITokenService` (HMAC-SHA256), controladores de catalogo con RBAC y `ValidationFilter`.
- Coleccion Postman de la Fase 3 con los 4 escenarios.
- Paginas de documentacion de Fase 2 y Fase 3.
- Pagina de la Fase 1 en la documentacion.
- Entidades `Marca` y `UnidadMedida` con su siembra y relacion con `Producto`.
- Configuraciones Fluent API por entidad (`IEntityTypeConfiguration<T>`) en `Configurations/`.
- Politica de CORS configurable desde `appsettings.json`.
- Migracion inicial de PostgreSQL (`InicialPostgreSql`).
- Evidencias de Fase 2 (capturas de tablas, DDL y siembra) y Fase 3 (login, 401, 403 y 400)
  organizadas en `docs/assets/evidencias/`.
- Indice de evidencias (`docs/assets/evidencias/README.md`) y enlaces por fase.

### Cambiado

- La solucion .NET se reubico en `backend/src` conservando la arquitectura Onion.
- El `README.md` raiz se reescribio como portada del monorepo.
- La inyeccion de dependencias se organiza por ciclos de vida (Transient, Scoped, Singleton).
- El middleware RFC 7807 usa titulos en espanol y `https://httpstatuses.com/{status}`,
  y mapea `ArgumentException` (400) y `UnauthorizedAccessException` (401).
- `Program.cs` valida scopes para evitar dependencias cautivas.
- La persistencia migra de SQL Server a **PostgreSQL** (Npgsql) y aplica `MigrateAsync`
  al arrancar.

### Eliminado

- El scaffolding de plantilla `WeatherForecast`.

[No publicado]: https://github.com/Javier-GarciaP/Licoreria-Backend-NET/commits/master
