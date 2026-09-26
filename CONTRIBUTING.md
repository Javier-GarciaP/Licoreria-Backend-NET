# Guía de Contribución

Gracias por contribuir al proyecto **Licorería / Discoteca**. Este documento define
cómo trabajamos el repositorio para mantener un historial limpio y trazable.

## Principios

1. **Commits atómicos**: cada commit debe representar **una sola intención**.
2. **Historial legible**: el mensaje explica *qué* y *por qué*, no *cómo*.
3. **Ramas cortas**: una rama por actividad, con vida útil reducida.
4. **Documentación como código**: los cambios de comportamiento actualizan `docs/`.

> **Importante:** Las fases de desarrollo académico se rigen por lo indicado por el
> docente. Las convenciones de este repositorio aplican a la evolución técnica
> (estructura, documentación, infraestructura y refactorizaciones) y no sustituyen
> dichas fases.

## Flujo de trabajo

1. Actualiza `master`:
   ```bash
   git checkout master
   git pull origin master
   ```
2. Crea una rama con la convención `tipo/descripcion-corta`:
   ```bash
   git checkout -b feat/reservas-mesa
   ```
3. Realiza cambios **en commits pequeños y coherentes**.
4. Ejecuta las validaciones antes de subir:
   ```bash
   task backend:build
   task docs:build
   ```
5. Sube la rama y abre un Pull Request hacia `master`.
6. Tras la revisión y el CI en verde, se integra preservando los commits atómicos
   (rebase/merge). Evita el *squash* salvo que la rama contenga commits ruidosos.

## Ramas

| Prefijo | Uso | Ejemplo |
| :--- | :--- | :--- |
| `feat/` | Nueva funcionalidad | `feat/pedido-barra` |
| `fix/` | Corrección de error | `fix/total-cuenta` |
| `docs/` | Documentación | `docs/esquema-postgresql` |
| `refactor/` | Reestructuración sin cambio funcional | `refactor/repositorios` |
| `chore/` | Mantenimiento, tooling, config | `chore/monorepo-docs` |
| `ci/` | Integración continua | `ci/pages-docs` |
| `db/` | Cambios de base de datos | `db/migracion-inventario` |

- La rama principal es `master`.
- No se hace *push* directo a `master`; todo entra por Pull Request.
- Las ramas se eliminan tras integrarse.

## Commits

Usamos **Conventional Commits** adaptado al español. El detalle completo está en
[`docs/dev/11-guias/commits.md`](docs/dev/11-guias/commits.md).

Formato:

```
<tipo>(<alcance>): <descripción imperativa y breve>

<cuerpo opcional: qué y por qué, en viñetas>

<footer opcional: Refs #12, Closes #34, BREAKING CHANGE: ...>
```

Ejemplo:

```
feat(inventory): registrar merma y reposición sin cobro

- Agrega el tipo de movimiento Merma con motivo Dañado.
- Descuenta inventario y genera línea de cortesía a precio cero.

Refs #21
```

### Tipos permitidos

`feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `build`, `ci`,
`chore`, `revert`.

### Alcances sugeridos

`repo`, `backend`, `frontend`, `db`, `api`, `security`, `infra`, `docs`, `site`,
`ci`, y los módulos de negocio (`catalog`, `inventory`, `purchasing`, `sales`,
`cash`, `club`, `crm`, `finance`, `content`, `ai`).

### Plantilla de commit

Configura la plantilla incluida en el repositorio:

```bash
git config commit.template .gitmessage
```

## Estilo de código

- **Backend:** seguir `.editorconfig`; ejecutar `task backend:format`.
- **Frontend/Docs:** ESLint + Prettier; indentación de 2 espacios.
- **Markdown:** diagramas en Mermaid; enlaces relativos entre documentos.
- **Idioma:** español para documentación y mensajes de commit.

## Pull Requests

- Título con el mismo formato de Conventional Commits.
- Descripción con: contexto, cambios, cómo probar y capturas si aplica.
- Un PR = una actividad concreta. Si crece, divídelo.
- Debe pasar el CI y actualizar `CHANGELOG.md` cuando corresponda.
