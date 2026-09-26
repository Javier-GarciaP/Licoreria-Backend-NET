# Convención de Commits

Este proyecto utiliza **Conventional Commits** con los textos en español. El objetivo
es un historial legible, automatizable y trazable con las actividades del proyecto.

## Formato

```
<tipo>(<alcance>): <descripción>

[cuerpo]

[footer]
```

- **tipo**: naturaleza del cambio (obligatorio).
- **alcance**: área afectada (opcional pero recomendado).
- **descripción**: imperativo, minúscula inicial, sin punto final, ≤ 72 caracteres.
- **cuerpo**: qué y por qué, en viñetas (opcional).
- **footer**: referencias a issues o cambios disruptivos (opcional).

## Tipos permitidos

| Tipo | Cuándo usarlo |
| :--- | :--- |
| `feat` | Nueva funcionalidad para el usuario. |
| `fix` | Corrección de un error. |
| `docs` | Cambios solo en documentación. |
| `style` | Formato, espacios, punto y coma (sin cambio de lógica). |
| `refactor` | Reestructuración sin cambiar el comportamiento. |
| `perf` | Mejora de rendimiento. |
| `test` | Añadir o corregir pruebas. |
| `build` | Sistema de compilación o dependencias. |
| `ci` | Configuración de integración continua. |
| `chore` | Tareas de mantenimiento que no encajan en los anteriores. |
| `revert` | Revertir un commit previo. |

## Alcances sugeridos

- **Transversales:** `repo`, `backend`, `frontend`, `db`, `api`, `security`, `infra`, `docs`, `site`, `ci`.
- **Módulos de negocio:** `catalog`, `inventory`, `purchasing`, `sales`, `cash`, `club`, `crm`, `finance`, `content`, `ai`.

## Reglas de atomicidad

1. **Un commit, una intención.** Si el mensaje necesita "y", probablemente son dos commits.
2. **Commits que compilan.** No dejar el árbol roto entre commits.
3. **Cambios grandes y relacionados** se pueden agrupar en un mismo commit bajo una
   misma actividad, siempre que persigan un único objetivo.
4. **No mezclar** refactor con funcionalidad nueva ni con formato.
5. **Sin secretos** ni credenciales en el historial.

## Ejemplos

```text
feat(club): agregar reserva de mesa con seña

- Modela Reserva, ReservaMesa y ReservaPago.
- Permite adjuntar comprobante para validación manual.

Refs #18
```

```text
fix(sales): corregir total al dividir una cuenta

El redondeo acumulaba centavos al dividir en partes iguales.
```

```text
docs(db): documentar estrategia de migración a PostgreSQL
```

```text
refactor(inventory): extraer kardex a un servicio de dominio
```

## Cambios disruptivos

Cuando un cambio rompe compatibilidad, se indica en el footer:

```text
feat(api)!: versionar endpoints de ventas bajo /api/v1

BREAKING CHANGE: las rutas previas sin versión dejan de estar disponibles.
```

## Plantilla y validación

- Plantilla local: `.gitmessage` (actívala con `git config commit.template .gitmessage`).
- Recomendado instalar `commitlint` para validar el formato en el CI (ver
  [`docs/dev/11-guias/ramas.md`](ramas.md)).
