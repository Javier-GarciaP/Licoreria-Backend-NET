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
- Migrar `*Tabs.tsx` (FolderTabs) a `Tabs` Radix donde aporte accesibilidad.
- Retirar los aliases legacy restantes del preset/`theme.css`.
- QA visual con capturas por módulo (playwright-cli disponible).