# Caja · Cuentas · Productos (simplificación operativa)

> Documento de trabajo. Registra el trabajo hecho sobre el módulo de **caja**,
> el de **cuentas/comandas** y el de **productos** para que el patrón se mantenga
> en cualquier pantalla futura del mesonero.

---

## 1. Caja — solo dólares

Archivo: `apps/admin/src/pages/CajaPage.tsx`

### Qué cambió

- **Movimientos siempre USD**: se eliminó el selector de moneda (USD/Bs) del
  formulario de movimientos; se envía `moneda: 'USD'` fija al API y la columna
  "Monto" usa `formatUSD`.
- **Arqueo solo billetes USD**: la grilla de arqueo filtra `moneda === 'USD'`
  ($1, $5, $10, $20, $50, $100); al cerrar se envían solo denominaciones USD.
- **Etiquetas `$` simples**: `$100` en lugar de `USD 100` o `$100.00`.
- **Inputs vacíos**: el campo de cantidad arranca vacío (`value ?? ''`) y al
  borrarlo queda vacío (se elimina la clave) en lugar de mostrar `0`.
- **Enter avanza al siguiente billete**: `onKeyDown` busca el siguiente input
  con clase `arqueo-cantidad` y le da el foco.

### Reglas a mantener

- Nunca reintroducir Bs en caja (ni selector, ni denominaciones, ni render).
- Los montos siempre con `formatUSD` y clase `.num`.

---

## 2. Cuenta — monitoreo primero, cobro destacado

Archivos: `apps/admin/src/pages/CuentasPage.tsx`, `apps/admin/src/pages/CuentaPage.tsx`

### Listado (`CuentasPage`)

- Chips de filtro por estado: **Todas / Abiertas / PorCobrar / Cerradas** (usa el
  query param `estado` de `GET /api/v1/cuentas`). Por defecto **Abiertas**.
- Columna de **Consumos** (nº de ítems) y **Saldo** destacado en `.num`.

### Detalle (`CuentaPage`)

Estructura de la página:

1. **PageHeader**: `Mesa {n}` + cliente, subtítulo "Abierta hace X", badge de
   estado y botón primario **Cobrar** (solo si la cuenta está Abierta).
2. **Resumen (KpiTile local)**: Saldo, Total, Abonado, Consumos.
3. **Consumos read-only**: ítems agrupados por comanda con pill de área
   (Barra/Cocina), hora y `StatusBadge` por ítem. Sin botones de avanzar estado
   (eso lo maneja barra/KDS).
4. **Acciones secundarias** (card al final): Agregar a la comanda / Abonar /
   Dividir, cada una abre su modal.

### Modales

- **Cobrar**: saldo grande, líneas de pago (método + monto, **solo USD**),
  restante ("Falta X" / "Saldo cubierto"), permite quitar líneas; el botón
  **Cobrar y cerrar** se habilita cuando el restante ≤ 0.01. Al éxito muestra el
  `TicketVenta` y vuelve a `/cuentas`. El monto por defecto se precarga con el
  saldo (`String(datos.saldo)`).
- **Abonar ("Pago parcial")**: saldo pendiente y "Quedará pendiente", método de
  pago como **chips** (botones, no `<select>`), presets **Mitad** / **Saldo
  completo**, historial de abonos.
- **Dividir**: saldo a dividir, presets 2–6 + input `N`, vista previa
  "N partes de $X aprox.", lista de partes actuales con estado (pagada/pendiente)
  y texto aclaratorio.

### Reglas a mantener

- Pagos y abonos **siempre en USD** (el backend valida `pagos + abonos ≥ total`).
- El cobro se hace con `cuentasApi.cerrar(id, { pagos, moneda: 'USD' })`; los
  abonos previos se suman como pagos en el backend.
- No tocar los estados de los ítems desde la vista de monitoreo.

---

## 3. Agregar a la comanda — área por ítem

Archivo: `apps/admin/src/components/cuentas/AgregarComandaModal.tsx`

Componente **reutilizable** para el mesonero (props `cuentaId`, `abierto`,
`onCerrar`, `onEnviado`).

### Comportamiento

- **Picker con fotos**: buscador con autofoco, chips de categoría
  (`catalogoApi.categorias`), grilla de productos con `urlDeImagen` (placeholder
  si no hay foto), rango de precio y chip "Receta" para `Preparado`.
  - 1 variante → agrega directo; varias → mini-modal de variante.
- **Área por ítem**: el switch tiene 3 estados — **Auto** (por defecto) usa
  `producto.areaDestino`; **Barra**/**Cocina** fuerzan el valor. Cada ítem
  hereda el switch al agregarse.
- **Dos bolsas** (Barra/Cocina): cada línea con stepper −/+, botón para mover a
  la otra área (→/←) y quitar (X), subtotal por bolsa.
- **Enviar comandas**: `Promise.all` sobre una llamada por área (`solo las no
  vacías`) porque el backend crea **una comanda por área** (`CrearComandaDto
  { area, items }`). Muestra el desglose en el botón.

### Regla clave

El backend no permite mezclar áreas en una comanda: **siempre se agrupa por
área antes de enviar**. Esto corrige el bug de "Enviar a Cocina mandaba todo".

---

## 4. Productos — área destino y formulario coherente

Archivos backend: `Producto` (entidad), `ProductoDto`, validadores,
`ServicioCatalogo`, `ProductoConfiguration`, migración `ProductoAreaDestino`.
Archivos frontend: `FormularioProducto`, `ProductosPage`, `PanelDetalleProducto`,
tipo `Producto` (packages/types).

### API / datos

- Nuevo campo **`AreaDestino`** en `Producto` (enum `Barra | Cocina`, default
  `Barra`). Se expone en `ProductoDto`, se valida con `IsInEnum`, se persiste como
  string (`.HasConversion<string>().HasMaxLength(20)`) y se migra con
  `20261008230341_ProductoAreaDestino` (`defaultValue: "Barra"`). La API lo aplica
  en arranque (`MigrateAsync`).
- El frontend lo muestra en la tabla de productos (columna Área), en el detalle
  (`PanelDetalleProducto`) y lo usa el `AgregarComandaModal`.

### Formulario (`FormularioProducto`)

- **Grado alcohólico**: solo se muestra si `areaDestino === 'Barra'` (opcional);
  en Cocina se oculta y se envía `null`. Zod: `z.union([z.literal(''), z.coerce.number()...])`.
- **SKU opcional + autogenerado**: `sku` ya no es obligatorio. Al guardar,
  `completarSkus()` rellena los vacíos con `generarSku(nombreProducto, indice)` →
  `{PRODUCTO}-{n}` (ej. `TACOS-1`). En edición se conserva el SKU existente.
  El backend sigue recibiendo SKU (sigue siendo `NOT NULL` + índice único).
- **Código de barras**: campo y vista previa (`CodigoBarras`) solo en Barra; la
  grilla de variantes pasa a 5 columnas en Cocina.
- **Tipo sugerido por área** (solo producto nuevo): Cocina → `Preparado`,
  Barra → `Simple` (useEffect sobre `areaDestino`).
- `PanelDetalleProducto`: la celda de código muestra barras solo si hay un código
  real (si no, `—`), sin usar el SKU como fallback.

### Reglas a mantener

- El SKU es un identificador único usado por inventario/reportes/compras: no se
  elimina del backend, se **autogenera** en el formulario.
- Grado alcohólico y código de barras son campos **de Barra**; Cocina solo pide
  nombre, unidad, costos y precios.

---

## 5. Verificación

- Backend: `dotnet build Licoreria.slnx` y `dotnet test Licoreria.slnx`
  (89 unit + 16 integración).
- Frontend: `npm run typecheck`, `npm run lint` y `npm run test`
  (29 tests del admin).