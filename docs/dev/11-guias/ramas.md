# Estrategia de Ramas

## Modelo

Se adopta un flujo **trunk-based** simplificado: `master` es la rama principal y
siempre debe estar en estado desplegable. Todo cambio entra por **Pull Request**
desde una rama corta.

```
master ──●────────●──────────●────────●──▶
          \      /          \      /
           ●────●            ●────●
         feat/...          fix/...
```

## Reglas

- `master` protegida: sin *push* directo, requiere PR y CI en verde.
- Una rama por actividad, con nombre `tipo/descripcion-corta` en `kebab-case`.
- Ramas de vida corta; se eliminan tras integrarse.
- Sincroniza con `master` mediante *rebase* para evitar merges innecesarios.
- Los commits atómicos se preservan al integrar (rebase/merge); evitar *squash*
  salvo que la rama contenga commits de trabajo ruidosos.

## Nombres de rama

| Prefijo | Uso |
| :--- | :--- |
| `feat/` | Nueva funcionalidad. |
| `fix/` | Corrección de error. |
| `docs/` | Documentación. |
| `refactor/` | Reestructuración. |
| `chore/` | Mantenimiento y tooling. |
| `ci/` | Integración continua. |
| `db/` | Cambios de base de datos. |

## Integración con las fases académicas

Las fases indicadas por el docente son el marco de entrega. Las ramas técnicas se
nombran y agrupan según esas fases cuando aplique, por ejemplo:

- `fase-2/api-final`
- `fase-3/seguridad-jwt`
- `fase-4/frontend`
- `fase-5/despliegue`

Esto permite que la rama técnica y la fase académica sean trazables entre sí sin
alterar el orden definido por el docente.
