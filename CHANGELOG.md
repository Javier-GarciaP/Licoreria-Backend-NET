# Changelog

Todos los cambios relevantes de este proyecto se documentan en este archivo.

El formato esta basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/)
y el proyecto se adhiere a [Versionado Semantico](https://semver.org/lang/es/).

> **Nota:** Las fases de desarrollo academico se rigen por lo indicado por el docente.
> Este changelog registra la evolucion tecnica del repositorio (estructura, documentacion,
> infraestructura y refactorizaciones), independientemente de dichas fases.

## [No publicado]

### Agregado

- **Completitud de la Fase 4 (5 brechas):**
  - **Modo dual real:** `ModoProvider` montado en `main.tsx` (envuelve `ErrorBoundary`,
    `Toaster` y `AuthProvider`) con botón de conmutación en el header de `AppShell`.
    `navigation.tsx` añade `modos?: Modo[]` por item/grupo y los helpers
    `visiblePorModo`/`filtrarGrupos`/`puedeAcceder` filtran por modo **antes** del
    bypass de admin: licorería oculta el grupo Salón, `/plano` y `/cuentas`;
    discoteca usa la nav completa; Dashboard visible en ambos. `App.tsx` aterriza en
    `/pos` (licorería) o `/` (discoteca) según el modo. Tests: `navigation.test.ts` (9).
  - **Dashboard sin desborde:** el heatmap se envuelve en `overflow-x-auto`
    (wrapper) con `min-w-[560px]` y el bento de KPI usa scroll horizontal en móvil.
  - **Máquina de estados conectada:** `MaquinaEstadosComanda` reescrita sobre
    `EstadoItemComanda` (Recibido → EnProceso/Preparado → Entregado; Cancelado
    terminal) e inyectada en `ServicioCuentas.CambiarEstadoItemAsync`, que valida
    transiciones (`ReglaNegocioException`), ignora no-ops y deriva el estado de la
    comanda (`CalcularEstadoComanda`) + notificación SignalR. El estado **Entregado**
    es alcanzable desde `PosCarta` (botón junto al badge, mesonero).
  - **Pagos mixtos USD/Bs:** el backend convierte Bs → USD con la tasa Paralelo
    vigente en abonos (`ServicioCuentas.RegistrarAbonoAsync`) y en ventas
    (`ServicioVentas`), validando `tasa > 0`; el abono persiste monto/moneda
    originales. El frontend añade selector USD/Bs en el cobro y abonos de
    `CuentaPage`, en `PosCarta` y en el panel de orden de `PosPage` (monto y propina
    normalizados), con `formatBS`/`formatUSD` en el historial.
  - **`EvaluadorMerma` integrado** en `ServicioInventario.RegistrarMermaAsync`
    (movimientos sugeridos Merma/Cortesía con paridad de comportamiento) y
    acepta `decimal`.
  - **Pruebas Moq nuevas:** `ServicioVentasTests` (5: pago USD, mixto, insuficientes,
    sin turno, sin tasa) y `ServicioCuentasTests` (8: abonos con/ sin tasa, cuenta
    cerrada, transiciones y derivación de comanda). Total backend: **116 tests**.
  - **Documentación:** `docs/dev/10-fases/fase-4.md` actualiza checklist y registra
    la **desviación de tema aprobada** (se conserva la paleta lavanda; el azul UNET
    `#003366` no se adopta), `frontend/README.md` §7.1/§7.5 se armoniza con el
    comportamiento real y `CHANGELOG.md` queda al día.

- **Flujo de servicio por turno (mesonero ↔ barra/cocina):**
  - **Guard de turno abierto:** abrir mesa, comandas, abonos, dividir, cerrar cuenta y ventas
    POS se rechazan (`422`) si no hay una sesión de caja abierta; `AbrirMesaAsync` además
    valida que la mesa exista, esté activa y libre (evita doble asignación).
  - **Retomar cuentas:** `POST /api/v1/cuentas/{id}/reabrir` vuelve una cuenta `PorCobrar`
    a `Abierta` (y la mesa a `Ocupada`) para que el mesonero siga trabajándola; la cuenta
    ya no pasa a `PorCobrar` automáticamente al acumular consumos (`Cuenta.Acumular`).
  - **Cierre de turno con auto-desalojo:** al cerrar la caja (Z) se cierran todas las
    sesiones de mesa (con saldo → `PorCobrar`, sin saldo → `Cerrada`), las mesas vuelven
    a `Libre` con notificación en vivo, y el turno registra `VentasDelTurnoUSD` y
    `CuentasDesalojadas`. Las ventas quedan ligadas al turno (`Venta.SesionCajaId`).
  - **Estado del turno para operadores:** `GET /api/v1/sesiones-caja/turno`
    (`{ abierto, abiertaEn }`) con policy `sales:read` (sin permisos de caja); eventos
    SignalR `turno:abierto` / `turno:cerrado` hacia barra, cocina, meseros, mesas y staff.
  - **KDS con cuentas en producción:** el listado de cuentas acepta `?estados=Abierta,PorCobrar`
    para que barra/cocina sigan viendo ítems pendientes de mesas liberadas o abandonadas.
  - **Frontend de servicio:** `AtenderView` con banner "Sin turno abierto" y sección
    "Por cobrar" con botón *Retomar*; `PosCarta` enruta ítems por `producto.areaDestino`
    (Auto/Barra/Cocina) según el patrón de `AgregarComandaModal`; `KDS` muestra
    `Abierta,PorCobrar` con sonido y refresco ante eventos de turno; hook `useTurno`.
  - **Carta del mesonero completa:** pestañas "Por enviar"/"Consumos", pendientes
    agrupados por área (Barra/Cocina) y consumos con badge de estado
    (Recibido/En proceso/Listo); las líneas pendientes **persisten** al salir y volver
    a la mesa (estado elevado en `MesoneroPage` por cuenta).
  - **Notificaciones al mesonero:** con `item:actualizado` se avisa en vivo
    "En preparación" (EnProceso) y "Tu pedido está listo" (Preparado) para las mesas
    del mesonero, con campana + sonido.
  - **Panel de notificaciones:** campana con badge de no leídas y panel desplegable
    (abrir/cerrar, cierre al tocar fuera, marcar leídas al abrir, descartar individual
    al pasar el cursor y "Limpiar"); dedupe por ítem/estado y nombre de mesa;
    sonido más fiable (AudioContext único reanudable, tonos distintos por tipo).
  - **Notificaciones con persistencia:** se guardan en `localStorage` por mesonero
    (`licoreria.notificaciones.<usuarioId>`) y sobreviven a refrescar la página o
    reabrir la app; estado inicial perezoso para evitar que se borren al cargar.
  - **Drag & drop del KDS corregido:** sensores de puntero/táctil con umbrales,
    medición continua de droppables (`MeasuringStrategy.Always`) y colisión por
    `pointerWithin`; el KDS usa `DragOverlay` para que la tarjeta siga al puntero por
    todo el canvas y las columnas muestren "Soltar aquí" al entrar a la zona.
  - **Layout a viewport completo:** la app de servicio usa `h-dvh overflow-hidden`
    y la carta expande paneles (comanda 26rem, menú flexible con grilla de 4 columnas).
  - **Caja (admin):** la sesión activa y el historial muestran "Ventas del turno" y
    "Cuentas de mesa / mesas desalojadas".
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
- **Área destino en productos:** campo `AreaDestino` (Barra/Cocina) en `Producto`
  (entidad, DTOs, validadores, servicio y `MapearProducto`) con columna en `productos`
  vía migración `ProductoAreaDestino` (default `Barra`). El frontend lo expone en el
  formulario de productos, en la tabla/detalle, y lo usa el modal de comandas para
  enrutar cada ítem a su área sin intervención manual.
- **`AgregarComandaModal` reutilizable:** picker de productos con búsqueda, filtro por
  categoría, fotos (`urlDeImagen`) y **área por ítem** (Auto/Barra/Cocina) con bolsas
  separadas; envía **una comanda por área** (`Promise.all`) y sirve de base para los
  módulos del mesonero.

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

### Corregido

- **Control de stock en el servicio (mesonero ↔ POS):**
  - `AgregarComandaAsync` verifica la existencia actual (desglosando recetas) y rechaza
    (`422`) pedidos que la barra/cocina no podría servir, con mensaje que nombra el
    producto y las cantidades requeridas/disponibles.
  - Al **servir** (ítem en `Preparado`/`Entregado`) se descuenta el insumo del kardex;
    si un ítem servido se cancela o regresa se reintegra. Al **cerrar la cuenta** solo se
    descuentan los ítems aún no servidos (`YaDescontado`), evitando la doble salida.
  - La venta POS valida stock antes de aplicar el kardex y reporta el producto faltante
    en lugar de un error genérico de stock negativo.
- **Cancelar ítem ajusta el total:** `Cuenta.Descontar` resta del total el ítem cancelado,
  de modo que el saldo coincide con lo realmente cobrable (afectaba saldo, abonos y división).
- **Stock inicial en cualquier variante:** `AsegurarStockAsync` aplica `StockInicial` y
  mín/máx también en variantes no base (p. ej. productos Cocina), que antes se descartaban
  en silencio y dejaban existencia 0.
- **Validación de recetas:** `AgregarRecetaAsync` exige insumo existente, activo, distinto
  de la variante vendida y con existencia propia (base o fila de stock), evitando
  presentaciones derivadas invendibles y auto-referencias.
- **Modelo de inventario por área (Cocina = solo vende):** `DesglosarInsumosAsync` no genera
  movimientos para productos de Cocina, con lo que se pueden pedir y vender sin stock ni
  receta (repara la regresión que rechazaba comandas/ventas de Cocina). La devolución
  reintegra a los mismos insumos que descontó la venta (tragos incluidos) y omite Cocina.
- **Catálogo coherente por área:** `CrearProductoAsync`/`EditarProductoAsync` rechazan
  Cocina con presentación base o stock (solo Simple, sin receta ni stock) y exigen al menos
  una presentación base activa en productos de Barra Simple. El formulario oculta stock,
  recetas y tipo en Cocina y valida la base en Barra.
- **Editor de órdenes reparable:** búsqueda server-side (ya no se pierden productos tras
  el tope de 100) y aviso de productos de Barra no ordenables (inactivos o sin presentación
  base) para que no desaparezcan sin explicación. Se repararon los datos de desarrollo
  (licores/base de Pepsi) que dejaban solo las gaseosas ordenables.
- **Tests frontend:** se agregó el alias `@licoreria/auth` a `vitest.config.ts`, que dejaba
  4 suites de admin sin poder transformar/ejecutar.
- **Productos Preparado (tragos) editables y vendibles:** el formulario iniciaba
  `esBase: true` y solo renderizaba el editor de receta en variantes no base, de modo que
  un Preparado nuevo se guardaba sin receta y la venta no descontaba ningún insumo.
  Ahora `tipo === 'Preparado'` fuerza `esBase: false`, oculta el checkbox de presentación
  base y los campos de stock, y muestra siempre el editor de consumo.
- **Receta obligatoria en Preparado:** `superRefine` de zod exige al menos un insumo con
  cantidad > 0 por presentación (mensaje bajo `variantes`) y `ValidarConsistenciaArea`
  responde `422` si un Preparado llega con presentación base o con stock propio/mín/máx
  ("no llevan stock propio: al vender consumen sus insumos según la receta").
- **Insumos visibles en el editor de consumo:** se listan las presentaciones base de
  productos Barra Simple ∪ variantes con fila de stock (antes solo consultaba el stock y
  quedaban invisibles las bases sin fila, p. ej. `PEPSI-1`), excluyendo la variante actual.
- **Errores de variantes renderizados:** react-hook-form guarda los errores del arreglo en
  `variantes.root`, por lo que ni la regla de presentación base ni la de receta aparecían
  en pantalla; ahora se leen `message ?? root.message`.
- **Pruebas de Preparado:** 3 tests de integración (venta con receta descuenta el insumo,
  con presentación base → `422`, con stock propio → `422`), 1 de POS (muestra lo que
  consume antes de agregarlo) y 3 del formulario (editor visible, guardado con receta y
  bloqueo sin receta).
- **Imágenes reales en la carta del mesonero:** `PosCarta` usaba `producto.imagenUrl` tal
  cual (`/uploads/productos/x.jpg`), que en el navegador se resolvía contra el puerto de
  Vite y devolvía el HTML de la SPA en lugar de la foto (imagen rota). Ahora se resuelve
  contra el origen del API con `urlDeImagen` (mismo criterio que admin y web pública) y la
  foto genérica de Unsplash se sustituye por un marcador con la inicial del producto, con
  `onError` de respaldo si el archivo no existe.
- **La foto del producto se ve completa en la carta:** la caja fija de 80 px con
  `object-cover` recortaba la imagen; ahora el área usa `aspect-[4/3]` con
  `object-contain` y padding (misma proporción que las tarjetas del admin), de modo que
  todas las tarjetas mantienen altura uniforme con o sin foto.

### Cambiado

- **Caja en dólares (solo USD):** el arqueo y los movimientos de caja operan únicamente
  en dólares. Se eliminaron los selectores USD/Bs y las denominaciones en Bs del UI;
  las etiquetas usan `$` (sin decimales), los campos de conteo arrancan vacíos y `Enter`
  avanza al siguiente billete.
- **Módulo de cuentas reestructurado (monitoreo primero):** la página de cuenta prioriza
  la supervisión (resumen de saldo/total/abonado/consumos y consumos read-only con
  badges de estado) y concentra el **cobro en un modal** con líneas de pago, restante y
  ticket térmico. Agregar a la comanda, abonar y dividir pasan a acciones secundarias en
  modales. El listado de cuentas gana filtro por estado (Todas/Abiertas/PorCobrar/Cerradas).
- **Formulario de productos coherente para Barra y Cocina:** el **grado alcohólico** y el
  **código de barras** solo se muestran para productos de Barra; el **SKU** ya no es
  obligatorio (se autogenera `{PRODUCTO}-{n}` si queda vacío) y se sugiere el **tipo**
  según el área elegida en productos nuevos.
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
