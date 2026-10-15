# Tasks

## 1. DevOps
- [x] 1.1 Renombrar servicios a `db`, `api`, `spa`; puertos 5432/5000/8080; healthcheck de `api`; `depends_on` con `service_healthy`
- [x] 1.2 Imagen de la API sobre `aspnet:10.0-alpine` publicada para `linux-musl-x64`; `VITE_API_URL` en la SPA
- [x] 1.3 `ci.yml` con backend, frontend, publicación de artefacto e imágenes condicionadas; corregir YAML inválido
- [x] 1.4 Verificar `docker compose up --build -d` con `db` y `api` en `healthy` y CI en verde

## 2. Dominio
- [x] 2.1 `StockProducto.Ubicacion` y migración `UbicacionStock`

## 3. Dashboard y Reportes
- [x] 3.1 KPIs adicionales y paneles (método de pago, top por ingresos, usuarios, mesoneros, zonas)
- [x] 3.2 Exportación del Dashboard a PDF
- [x] 3.3 Barra de filtros en tarjeta para Dashboard y Reportes, con relleno inferior corregido

## 4. Patrón visual
- [x] 4.1 Alinear Promociones, Clientes, Cuentas por cobrar, Entradas y Lista VIP con el patrón de listados
- [x] 4.2 Quitar `PageHeader` de todos los módulos salvo Salón
- [x] 4.3 Quitar pestañas de inventario y mostrar Envases abiertos en el sidebar
- [x] 4.4 Nuevo logo de corcho de champán

## 5. Rol Cajero
- [x] 5.1 Dashboard solo para Administrador y ocultar Categorías y marcas, Unidades, Kardex y Tasas de cambio al Cajero

## 6. Documentación
- [x] 6.1 README (puertos y ejecución), CHANGELOG, guía de Fase 5, evidencias y este cambio
