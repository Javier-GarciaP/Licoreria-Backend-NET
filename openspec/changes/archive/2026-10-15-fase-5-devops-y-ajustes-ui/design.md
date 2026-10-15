# Design

## Decisiones

- **Compatibilidad sobre reescritura**: el repositorio ya tenía compose, Dockerfiles y CI; se renombraron servicios (`postgres`→`db`, `admin`→`spa`) y se conservaron `public-web`, `servicio`, `edge` y `pgadmin`. La API escucha internamente en 8080 (el `edge` apunta a `api:8080`) y se publica como 5000.
- **Imagen de la API**: Alpine con `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true` y `fontconfig`/`freetype` para QuestPDF. La `spa` pesa ~106 MB en disco; la API ~248 MB descomprimida (base ~204 MB). Reducirla requiere recorte/ReadyToRun, pendiente.
- **CI**: `ci.yml` convive con `backend.yml` (cobertura, contrato OpenAPI, integración) y `frontend.yml`. El error inicial fue YAML inválido por `": "` en un escalar sin comillas.
- **PDF del Dashboard**: se genera en el cliente con `jspdf` y `jspdf-autotable` a partir de los datos ya cargados, sin endpoints nuevos. Omite secciones sin datos o sin permiso.
- **Patrón de listados**: `Card` > `CardHeader` con buscador + acción, línea divisoria y filtros + "Limpiar filtros"; `DataTable` y paginación. Sin `PageHeader` salvo en Salón. `CardHeader` solo trae relleno superior: las tarjetas sin cuerpo (barras de filtros) añaden `pb-5 lg:pb-6`.
- **Cajero**: la restricción es de interfaz (`roles` en `lib/navigation.tsx`); los permisos del backend no cambian.

## Riesgos

- Las pruebas de `FormularioProducto` agotan el tiempo bajo carga al correr toda la suite; pasan aisladas.
- `Ubicacion` no está expuesta aún en DTOs ni en la SPA.
