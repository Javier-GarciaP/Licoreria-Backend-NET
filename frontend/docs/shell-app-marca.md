# Shell del admin · Sidebar, header y marca CORCHO

> Documento de trabajo. Registra **qué se hizo** en el shell de `@licoreria/admin`
> (AppShell): sidebar colapsable, header con buscador, marca CORCHO y cierre de
> sesión. Complementa a `rediseno-convencional.md` y a `DESIGN.md` (fuente de
> verdad visual).

---

## 1. Objetivo

Rediseñar la estructura principal del panel admin:

- Sidebar a la izquierda, **colapsable** (rail de iconos al contraer).
- Iconos de navegación en el **color de marca oscuro** (`text-accent-ink`).
- **Buscador fuera del sidebar**, en el header a la izquierda.
- **Toggle de tema** solo en el header (a la derecha).
- **Botón de cerrar sesión** simplificado (icono a la izquierda, texto a la
  derecha), sin la card de usuario.
- **Marca propia del sistema** (nombre + icono SVG simple).

---

## 2. Marca CORCHO (`components/Logo.tsx`)

Nuevo componente con dos exportaciones:

- **`LogoCorcho`**: SVG inline (24×24) de un corcho estilizado — cuerpo
  redondeado con `fill="currentColor"` y estrías horizontales con
  `stroke="rgb(var(--card))"` (se recortan contra el fondo del chip). Sin
  dependencias externas.
- **`Marca`**: chip `bg-primary text-primary-foreground` con el corcho + el
  nombre **CORCHO**. Prop `compacto` para mostrar solo el icono (sidebar
  contraído). Props opcionales `chipClassName` y `nombreClassName` para
  ajustar el tamaño responsivo.

Uso:
- `AppShell`: cabecera del sidebar (`Marca compacto={contraido}`) y header
  móvil (`chipClassName="h-8 w-8 lg:hidden"`).
- `LoginPage`: chip grande con `LogoCorcho` + título **CORCHO**.

> Regla: la marca es el único lugar donde se usa el corcho; los iconos de nav
> usan `text-accent-ink`, no el logo.

---

## 3. Sidebar (`AppShell.tsx`)

### Colapsable con persistencia
- Estado `contraido` persistido en `localStorage` (`licoreria.nav.contraido`).
- `<aside>`: `w-72` expandido ↔ `w-16` contraído, con `transition-[width]
  duration-200`.
- **Toggle dentro del sidebar**, en el pie (debajo del logout):
  - Expandido: fila completa `[PanelLeft] Contraer`.
  - Contraído: icono `PanelRight` con `title="Expandir"`.
- **Clic en cualquier parte del sidebar contraído expande** (`onClick` en el
  `<aside>` + `cursor-pointer`). Los botones de grupo además abren ese grupo.

### Navegación
- Grupos con 1 ítem: `NavLink` directo (icono + label).
- Grupos con subitems:
  - Expandido: botón cabecera con chevron (rotación 90° al abrir) + lista
    anidada con `border-l`.
  - Contraído: solo el **icono del grupo** (`grupo.icon`); al hacer clic
    expande el sidebar y abre el grupo.
- Contraído: `items-center` (evita el descuadre a la izquierda) y
  `gap-[15px]` para separar los iconos.
- Tooltips nativos (`title` / `aria-label`) con el label cuando está
  contraído.

### Colores
- Iconos de nav (grupos, ítems, subítems, bottom-nav móvil, buscador):
  `text-accent-ink` (`#6b4fc9` en claro, lavanda clara en oscuro) — más
  oscuros que el lavanda `text-primary`.
- Ítem activo: `bg-primary/15 font-medium text-foreground`.

---

## 4. Header

- Altura **`h-16`** (64px; +15% frente a `h-14`) para dar respiro al buscador.
- **Buscador a la izquierda con `lg:ml-5`** (20px desde el sidebar): botón
  estilo input con `Search` + "Buscar…" + kbd `⌘K`, que abre la
  `CommandPalette`. En móvil: botón solo-icono.
- **Tema a la derecha**: botón `Moon`/`Sun` (móvil y escritorio).
- **Marca en móvil**: `Marca` con corcho + "CORCHO" (`lg:hidden`); en
  escritorio el sidebar ya muestra la marca.
- Logout en header: solo móvil (icono), porque en escritorio vive en el
  sidebar.

---

## 5. Cerrar sesión (sidebar)

- Se **eliminó la card de usuario** (nombre + rol) que estaba encima.
- Botón mejorado: `rounded-lg border border-border bg-muted/40` con
  `[LogOut] Cerrar sesión` (icono a la izquierda, texto a la derecha) y hover
  destructivo (`border-destructive/40 bg-destructive/10 text-destructive-fg`).
- Contraído: solo icono con `title="Cerrar sesión"`.

---

## 6. Archivos tocados

| Archivo | Cambio |
| :--- | :--- |
| `apps/admin/src/components/Logo.tsx` | **nuevo**: `LogoCorcho` + `Marca` (CORCHO) |
| `apps/admin/src/components/AppShell.tsx` | sidebar colapsable, toggle en pie, clic expande, iconos `text-accent-ink`, buscador en header (izq), header `h-16`, logout simplificado, marca |
| `apps/admin/src/components/MobileNavSheet.tsx` | iconos a `text-accent-ink` (consistencia) |
| `apps/admin/src/pages/LoginPage.tsx` | marca CORCHO en lugar del emoji 🍷 |
| `DESIGN.md` | §4 layout + marca; §5 `SidebarNav` colapsable con `text-accent-ink` |

---

## 7. Reglas del patrón (a mantener)

1. Sidebar colapsable con estado persistido; **toggle dentro del sidebar** (pie).
2. Sidebar contraído: solo iconos, `items-center`, `gap-[15px]`, tooltip nativo.
3. Iconos de navegación en **`text-accent-ink`** (más oscuro que `text-primary`).
4. Buscador siempre en el **header a la izquierda** (20px del sidebar), abre la
   `CommandPalette`.
5. Tema solo en el **header a la derecha**.
6. Logout: botón simple con icono izquierda + texto derecha; sin card de usuario.
7. Marca = `LogoCorcho`/`Marca`; no reemplazar por iconos de lucide.

---

## 8. Verificación

```bash
npm run typecheck -w @licoreria/admin   # 0 errores
npx eslint apps/admin/src/components/Logo.tsx apps/admin/src/components/AppShell.tsx \
  apps/admin/src/components/MobileNavSheet.tsx apps/admin/src/pages/LoginPage.tsx   # 0 problemas
npm run test -w @licoreria/admin        # 29/29
npx prettier --write <archivos>         # formateado
```

> Probar visualmente en tema Claro/Oscuro, escritorio y móvil: colapsar/
> expandir el sidebar, clic en un grupo contraído, buscador abriendo la paleta,
> toggle de tema en header y botón de cerrar sesión.
