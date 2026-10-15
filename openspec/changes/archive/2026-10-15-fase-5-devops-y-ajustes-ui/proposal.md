# Proposal

## Why

La Fase 5 de la asignatura exige contenerización multi-stage, orquestación con Docker Compose, CI/CD con GitHub Actions, modelo de dominio con las 4 dimensiones y una demostración con Dashboard KPI. Durante su entrega surgieron además ajustes de interfaz: el Dashboard necesitaba más indicadores y exportación a PDF, los módulos reactivados no seguían el patrón visual de los demás, había encabezados de página y pestañas fuera del módulo Salón, el logo era poco legible y el Cajero veía pantallas de gestión que no necesita.

## What Changes

- **Orquestación**: servicios `db`, `api` y `spa` (puertos 5432, 5000 y 8080), healthchecks en `db` y `api`, `depends_on` con `service_healthy` y `VITE_API_URL` inyectada al construir la SPA.
- **Imágenes**: la API pasa a `aspnet:10.0-alpine`, publicada solo para `linux-musl-x64`.
- **CI/CD**: `.github/workflows/ci.yml` (restore, build, xUnit/Moq y frontend) que publica artefacto y construye imágenes solo si todo pasa.
- **Dominio**: `StockProducto.Ubicacion` con migración `UbicacionStock`.
- **Dashboard**: más KPIs, paneles y exportación a PDF en el navegador.
- **Patrón visual**: se unifican los módulos (toolbar con buscador, filtros y acción; sin encabezado de página salvo Salón); Reportes y Dashboard usan una barra de filtros en tarjeta; Envases abiertos pasa al sidebar y se retiran las pestañas de inventario.
- **Marca**: nuevo logo de corcho de champán.
- **Cajero**: sin Dashboard ni vistas de gestión (Categorías y marcas, Unidades, Kardex, Tasas de cambio).

## Capabilities

### New Capabilities
- `container-orchestration`: pila `db` + `api` + `spa` con salud y dependencias.
- `ci-pipeline`: flujo de CI que condiciona la publicación a pruebas en verde.
- `dashboard-extended-kpis`: indicadores adicionales y exportación del Dashboard a PDF.
- `module-layout-consistency`: patrón visual común de los módulos.
- `cashier-minimal-navigation`: navegación mínima del rol Cajero.

### Modified Capabilities
- `analytics-dashboard`: la barra de periodo del Dashboard y de Reportes vive en una tarjeta de filtros.

## Impact

- **Infra**: `docker-compose.yml`, `backend/src/Licoreria.WebAPI/Dockerfile`, `frontend/apps/admin/Dockerfile`, `.env.example`, `.github/workflows/ci.yml`.
- **Backend**: `StockProducto`, su configuración EF y la migración. Sin cambios de API.
- **Frontend**: `DashboardPage`, `ReportesPage`, `lib/dashboardPdf.ts`, `lib/navigation.tsx`, `components/Logo.tsx` y las páginas de Promociones, Clientes, Cuentas por cobrar, Entradas, Lista VIP, Lotes, Tomas físicas, Contenido, POS e Inventario.
- **Documentación**: `README.md`, `CHANGELOG.md` y `docs/dev/09-operaciones/fase5-contenerizacion.md`.
