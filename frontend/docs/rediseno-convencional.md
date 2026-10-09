# Rediseño · Convencional sobre shadcn/ui (Fase 5)

> Documento de trabajo. Registra **qué se hizo** y **con qué reglas**, para
> que cualquier pantalla nueva mantenga el mismo patrón. Sustituye a
> `rediseno-shell-glass.md`.

---

## 1. Objetivo

Cambiar la dirección visual del frontend operativo (`admin` + `servicio`) de
**vidrio opaco + pastel** a **superficies sólidas convencionales**, adoptando
**shadcn/ui** (primitivas Radix) como librería de componentes, conservando la
personalidad de marca: acento lavanda, familias pastel de estado y cifras en
IBM Plex Mono.

### Restricciones respetadas

- **Tailwind v3**: sin migrar a v4. Los componentes shadcn se **vendorean**
  adaptados a v3 (patrón `rgb(var(--x) / <alpha-value>)`), evitando el CLI
  interactivo que apunta a v4.
- **La API de `@licoreria/ui` no se rompe**: wrappers públicos conservan su
  firma; los ~60 archivos que importan del paquete migran de forma
  incremental sin dejar de compilar.
- **public-web** no se toca (tema propio "Hungry Tiger").
- **Skill oficial** `shadcn-ui/ui@shadcn` instalada en `.agents/skills/shadcn`.

---

## 2. Reglas del patrón (a mantener)

1. **Superficie sólida**: `bg-card` + `border-border` + `shadow-sm` en tarjetas;
   `bg-background` en el lienzo; sin `backdrop-filter` ni halos.
2. **Tokens canónicos shadcn**: `background`, `foreground`, `card`,
   `card-foreground`, `popover`, `primary`, `primary-foreground`, `secondary`,
   `muted`, `muted-foreground`, `accent`, `accent-foreground`, `destructive`,
   `border`, `input`, `ring`; familias `success/warning/info/butter` con texto
   `-fg` sobre el fill.
3. **Radios**: controles `8px`, tarjetas `12px`, botones `rounded-md`, chips
   píldora. **Peso máximo 500** y tracking negativo (Inter).
4. **Cifras** en `.num` (IBM Plex Mono + `tabular-nums`).
5. **Sin clases legacy**: `text-ink`→`text-foreground`, `text-muted`→
   `text-muted-foreground`, `bg-surface`→`bg-card`, `border-hairline`→
   `border-border`, `-ink`→`-fg`, `bg-accent`→`bg-primary`, `ring-accent`→
   `ring-ring`, `rounded-pill`→`rounded-full`, `glass-*`→`border border-border
   bg-card`.
6. **Aliases legacy** retirados del preset y de `theme.css` **solo cuando no
   queden usos** (hoy el barrido los eliminó de `admin` y `servicio`).

---

## 3. Qué se hizo (módulos)

### M0 · Fundación
- Skill `shadcn-ui/ui@shadcn` instalada.
- `components.json` en `frontend/` con aliases a `@licoreria/ui`.
- `cn()` → `clsx` + `tailwind-merge`.
- `theme.css`: esquema semántico shadcn + familias pastel, temas claro/oscuro;
  se retira vidrio y glow. `.glass-*` pasan a alias sólido (migración).
- `tailwind.preset.cjs`: colores semánticos, radios y sombras convencionales;
  aliases legacy conservados para la migración.

### M1 · `@licoreria/ui` sobre shadcn
- Primitivas en `packages/ui/src/components/ui/`: button, card, input, label,
  badge, separator, table, skeleton, spinner, pagination, dialog,
  dropdown-menu, tooltip, tabs, sheet, command, avatar, alert, empty,
  breadcrumb.
- Wrappers con API estable: Button (mapa primary/ghost/subtle/danger), Card*,
  Input, Select (nativo estilizado, integra react-hook-form), Modal (Radix
  Dialog: aria-modal, Escape, foco), Pill/StatusBadge, Skeleton*, DataTable,
  Pagination, PageHeader/EmptyState/Spinner, ActionMenu (Radix DropdownMenu),
  BubbleModal.
- Deps: clsx, tailwind-merge, cva, radix (dialog/dropdown/select/tabs/tooltip/
  slot), cmdk.

### M2 · Shell admin
- `AppShell`: layout convencional — sidebar fija `w-72` `border-r`, topbar
  sticky `border-b`, canvas `bg-background`. Activo de nav `bg-primary/15`.
- `MobileNavSheet` → Sheet (Radix), `CommandPalette` → cmdk, `Breadcrumbs` →
  UiBreadcrumb, toast sonner a superficie sólida.

### M3–M16 · Migración por módulo
- Barrido de tokens legacy → canónicos en `admin` (Login, Dashboard, POS,
  Salón, Catálogo, Inventario, Compras, Dinero, Clientes, Contenido, Reportes,
  Usuarios, helpers) y `servicio` (KDS, chat mesonero, login).
- Dashboard: tiles y paneles sólidos, chips pastel sólidos, se retira el
  hover-lift de framer-motion.
- Mapa/POS/KDS: solo superficies y tokens; la lógica SVG/interactiva se
  conserva.

### M17 · Cierre
- `DESIGN.md` reescrito a la dirección convencional.
- Este documento sustituye a `rediseno-shell-glass.md`.
- `README.md` actualizado (stack shadcn/ui).

### M18 · Patrón de listado (buscador + filtros)
- Nuevos componentes comunes en `@licoreria/ui`: **`Buscador`** (lupa +
  limpiar), **`FiltroDropdown`** (botón + menú Radix con check y "Todos") y
  **`FiltroRango`** (botón + popover con inputs mín/máx; sustituye al
  `RangoMinMax` inline).
- Helper `apps/admin/src/lib/filtros.ts`: `contiene`, `paginarEnMemoria` y
  `PAGE_SIZE_FILTRO_LOCAL` para filtrado client-side con paginación en memoria.
- **Patrón unificado** (documentado en `DESIGN.md` §7.5): `Card` único con
  `CardHeader` en dos filas (fila 1: buscador + filtros generales + botón
  añadir; hairline `border-b border-border`; fila 2: filtros concretos) y
  `CardBody` con tabla + paginación.
- Aplicado a los módulos **activos**: Ventas, Productos, Categorías/Marcas,
  Unidades, Kardex, Mermas, Tasas, Compras, Proveedores, Recepciones, Cuentas
  por pagar, Cuentas, Reservas, Usuarios y el historial de Caja.
- **Formularios de crear/editar movidos a `Modal`** en Compras, Proveedores,
  Usuarios y Unidades (antes `InlineForm` arriba del Card). Mermas: el form
  desplegado pasa a `Modal` con botón "Nueva merma" (patrón tabla + añadir).
- **Filtrado**: server-side donde la API soporta params; client-side (con
  `pageSize` ampliado y paginación en memoria) donde no.
- Tests actualizados: `MermasPage.test` (modal) y `UsuariosPage.test`.

### M18.1 · Correcciones de patron de filtros
- `Buscador`: la lupa pasa a la **izquierda** del input y el ancho queda fijo
  (`w-64 shrink-0`) para no estirarse de extremo a extremo; botón X de
  limpieza a la derecha. `Input` gana `leftSlot`.
- `FiltroDropdown`: el menú Radix ahora tiene **panel sólido redondeado**
  (`bg-popover border border-border rounded-xl shadow-md p-1.5`) e items con
  hover/focus, agrupando las opciones de forma legible.
- `RangoMinMax` se sustituye por **`FiltroRango`** (popover con Mín/Máx y
  resumen "5 – 20"); se elimina del paquete.
- Nuevo **`FiltroFechas`**: botón "Fecha" que abre un popover con Desde/Hasta
  (resumen "dd/mm/aaaa – …"), reemplaza los pares de inputs `type="date"`
  inline. En todos los módulos las fechas viven en la **fila 1** (generales).
- Nuevo **`FiltroPopover`**: base reutilizable (trigger + portal posicionado
  bajo el botón, cierra con clic fuera o Escape).
- Aplicado a los módulos activos: Ventas, Productos, Compras (fechas a fila 1),
  Cuentas por pagar, Cuentas, Reservas, Kardex, Tasas, Recepciones y Caja
  (historial).

### M18.2 · Label de filtros arriba + botón Limpiar
- `FiltroDropdown` y `FiltroPopover` (y por tanto `FiltroRango` y
  `FiltroFechas`): el **label del filtro va arriba del botón** (text-xs muted) y
  el botón muestra **solo el valor seleccionado** ("Todos", etiqueta, "5 – 20",
  rango de fechas…) + chevron.
- Nuevo componente **`LimpiarFiltros`**: botón ghost con icono `RotateCcw` y
  texto "Limpiar"; `disabled` cuando no hay filtros activos. Se coloca al final
  de la fila de filtros (`ml-auto`), en fila 2 o fila 1 si no hay fila 2.
- Cada página de listado define `hayFiltros` (filtros + buscador) y
  `limpiarFiltros()` que resetea todos los estados y vuelve a página 1.
- Aplicado a: Ventas, Productos, Compras, Cuentas por pagar, Cuentas,
  Reservas, Mermas, Proveedores, Usuarios, Kardex, Tasas, Recepciones, Caja
  (historial) y Categorías/Marcas. Unidades (solo buscador) no lleva botón.
- Test actualizado: `UsuariosPage.test` verifica el label "Rol" y el botón
  "Limpiar" deshabilitado.

### M19 · Núcleo mínimo, navegación y paneles (sesión de trabajo)
- **Ocultamiento de módulos fuera del núcleo mínimo.** En
  `apps/admin/src/lib/navigation.tsx` se marcan con `oculto: true` (desaparecen
  de la nav y se bloquea el acceso directo vía `puedeAcceder`): **Existencias**
  (`/inventario`). En `CatalogoAvanzadoPage` se eliminan las pestañas de
  **Impuestos, Listas de precio y Modificadores**, dejando solo **Unidades**.
  `Proveedores` se mantiene en Compras (se revirtió su ocultamiento).
- **Eliminación de las pestañas-carpeta (FolderTabs) de Catálogo y Compras.**
  Se eliminan `CatalogoTabs.tsx` y `ComprasTabs.tsx`. Catálogo y Compras pasan
  a navegarse desde la sidebar:
  - Catálogo y almacén: **Catálogo** (`/productos`), **Categorías y marcas**
    (`/catalogos`, icono `Tags`) y **Unidades** (`/catalogos-avanzado`, icono
    `Ruler`) como ítems propios del grupo.
  - Compras: Órdenes, Proveedores, Recepciones y Cuentas por pagar ya eran
    ítems propios; se quita la pestaña superior de cada página.
  - Kardex deja de usar `InventarioTabs` (ya no hay pestañas; el ítem de nav
    es "Kardex").
- **Patrón de panel unificado.** Todos los módulos usan ahora un único `<Card>`
  con borde completo (`rounded-xl border bg-card shadow-sm`), en lugar del
  patrón `FolderPanel` (esquina superior izquierda plana + `bg-card/50`).
  `FolderPanel`/`FolderTabs` quedan **solo** para el Salón (Mesas). Las páginas
  con dos secciones (Categorías/Marcas) pasan a un solo `Card` con `CardBody`
  en grid; los formularios de crear/editar quedan en `Modal` y los listados en
  `CardBody`.
- **Ajustes finales del patrón de listado** (post M18.2):
  - Botones de filtro con radio `rounded` (menos redondeado que `rounded-md`).
  - Label del filtro en `text-xs font-medium text-foreground` (negrita).
  - `Buscador` más ancho: `w-80 shrink-0` (antes `w-64`).
- No se tocan los módulos inactivos (Existencias, Lotes, Tomas, Promociones,
  Entradas, Lista VIP, Clientes, Cuentas por cobrar, Auditoría, Contenido,
  Reportes) ni las vistas operativas (POS, Salón, Dashboard, KDS).

---

## 4. Verificación

```bash
npm run typecheck          # admin + servicio + public-web
npm run lint               # 0 problemas
npm run test -w @licoreria/admin   # 29/29
npm run build              # admin + servicio + public-web
```

> Probar visualmente en tema Claro y Oscuro, escritorio y móvil, en las
> pantallas densas: Dashboard, POS, Salón (mapa), KDS, Compras e Inventario.

---

## 5. Pendiente (opcional)

- Radix `Select` para los casos que no usen `register` (hoy: nativo estilizado).
- Retirar los aliases legacy restantes del preset/`theme.css`.
- QA visual con capturas por módulo (playwright-cli disponible).
- Revisar `FormularioProducto` / `PanelImagenProducto` (error de tipo TS
  preexistente en el working tree, ajeno a los patrones de listado).