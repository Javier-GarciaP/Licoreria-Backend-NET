# Licorería / Discoteca · Plataforma Web

Plataforma web integral para la gestión de un local de **licorería y discoteca**
(una sola sucursal): web pública para clientes y sistema interno para el personal.

> **Backend** .NET 10 con **Onion Architecture** · **Base de datos** PostgreSQL ·
> **Frontend** React + Tailwind CSS · **IA** aplicada al negocio.

---

## Documentación

| Recurso | Descripción |
| :--- | :--- |
| [Documentación técnica](docs/dev/README.md) | Visión, requerimientos, arquitectura, modelo de datos, API, seguridad, módulos, IA y guías. |
| [Evidencias por fase](docs/assets/evidencias/README.md) | Capturas y colecciones Postman de Fases 1, 2 y 3. |
| [Sitio de documentación](docs/site) | Sitio Astro Starlight (secciones *Desarrolladores* y *Cliente*). |
| [Contrato OpenAPI](openapi/licoreria.yaml) | Especificación de la API v1 (contract-first). |
| [Guía de contribución](CONTRIBUTING.md) | Flujo de trabajo, ramas y commits. |
| [CHANGELOG](CHANGELOG.md) | Historial de cambios del repositorio. |

---

## Estructura del monorepo

```
.
├── backend/          # Solución .NET (Onion: Domain, Application, Infrastructure, WebAPI)
├── frontend/         # Apps React (admin, public-web, servicio) y paquetes compartidos
├── database/         # Scripts PostgreSQL, seeds y diagramas
├── openapi/          # Contrato OpenAPI de la API
├── docs/             # Documentación
│   ├── dev/          #   Documentación técnica canónica (GitHub)
│   └── site/         #   Sitio Astro Starlight (dev + cliente)
├── infra/            # Despliegue (docker, nginx, terraform)
└── .github/          # CI/CD, plantillas y CODEOWNERS
```

La documentación vive **aislada** en `docs/`, de modo que no interfiere con el flujo de
desarrollo de `backend/`, `frontend/` y `database/`.

---

## Arquitectura (resumen)

Onion Architecture con dependencias hacia el dominio:

```
WebAPI → Application → Domain
              ↑
        Infrastructure
```

Detalle en [docs/dev/02-arquitectura](docs/dev/02-arquitectura/onion.md).

---

## Puesta en marcha

```bash
# Backend
dotnet build backend/Licoreria.slnx
dotnet test backend/Licoreria.slnx
dotnet run --project backend/src/Licoreria.WebAPI

# Base de datos local (PostgreSQL, objetivo de la Fase 2)
docker compose up -d postgres

# Documentación
cd docs/site && npm install && npm run dev
```

> El proyecto incluye un `Taskfile.yml` con atajos: `task --list`.
> La persistencia usa **PostgreSQL** (ver `docker-compose.yml`); las migraciones se
> aplican automáticamente al arrancar la API.

### Puertos del despliegue Docker

`docker compose up -d --build` levanta los servicios con estos puertos
(configurables en `.env`, ver `.env.example`):

| Servicio | Contenido | Puerto |
| :--- | :--- | :---: |
| `api` | WebAPI .NET (migraciones + seed al arrancar) | `5190` |
| `admin` | Panel interno (SPA nginx) | `4173` |
| `public-web` | Web pública (SPA nginx) | `4174` |
| `servicio` | Web de servicio: mesonero / KDS (SPA nginx) | `4175` |
| `edge` | Reverse proxy nginx (API + `/hubs` + web pública) | `8090` |
| `postgres` | PostgreSQL 15 | `5432` |

Swagger UI queda habilitado en el despliegue Docker (`Swagger:Enabled=true`) en
`http://localhost:5190/swagger`.

Credenciales de prueba (contraseña `demo123`): `admin@licoreria.com`, `cajero1@licoreria.com`,
`mesero1@licoreria.com`, `barra1@licoreria.com`, `cocina1@licoreria.com`, `host1@licoreria.com`
y `editor1@licoreria.com`.

Variables de negocio opcionales (`.env`): `FIDELIDAD_PUNTOS_POR_USD` (puntos por USD pagado, `1` por defecto, `0` los desactiva) y
`CREDITO_DIAS_VENCIMIENTO` (días de plazo del fiado, `30` por defecto).

---

## Fases del proyecto

Cada fase tiene su propio documento con alcance, decisiones, evidencias y pruebas.

| Fase | Alcance | Estado | Documentación |
| :---: | :--- | :---: | :--- |
| 1 | Fundamentos, DI, RFC 7807 y dominio | Completada | [fase-1](docs/dev/10-fases/fase-1.md) |
| 2 | Persistencia PostgreSQL 15 y seeding | Completada | [fase-2](docs/dev/10-fases/fase-2.md) |
| 3 | Seguridad JWT / RBAC y validación | Completada | [fase-3](docs/dev/10-fases/fase-3.md) |
| 4 | Frontend React + Docker | Completada | [fase-4](docs/dev/10-fases/fase-4.md) |
| 5 | Despliegue | Pendiente | [roadmap](docs/dev/10-fases/roadmap.md) |

> Las fases académicas se rigen por lo indicado por el docente; las mejoras de estructura,
> documentación e infraestructura del repositorio son independientes. Ver el
> [roadmap](docs/dev/10-fases/roadmap.md). La API final está descrita en la
> [guía para el frontend](docs/dev/04-api/guia-frontend.md).

---

## Arreglo de bugs y mejoras por spec (OpenSpec)

Las mejoras posteriores a la Fase 4 (observaciones del docente y bugs detectados) se planifican
como cambios [OpenSpec](openspec/changes) y se implementan con `/opsx:apply <cambio>`.

### Estado

| Spec | Qué resuelve | Estado |
| :--- | :--- | :---: |
| [`concurrencia-stock`](openspec/changes/archive/2026-10-14-concurrencia-stock) | Reserva de stock al pedir, transacciones con reintento, 409/422 y restricciones en BD. | Hecho (25/25) ✅ archivado |
| [`presentaciones-y-porciones`](openspec/changes/archive/2026-10-14-presentaciones-y-porciones) | Varias presentaciones por producto, porciones (vaso/trago) y envases abiertos por la barra. | Hecho (30/30) ✅ archivado |
| [`auditoria-total`](openspec/changes/archive/2026-10-14-auditoria-total) | Bitácora automática de quién cambió qué, con antes y después. | Hecho (21/21) ✅ archivado |
| [`modulos-complementarios`](openspec/changes/archive/2026-10-14-modulos-complementarios) | Reactivar Promociones, Clientes/CxC, Compras, Existencias, Entradas y Lista VIP; cliente en la venta, puntos automáticos y fiado. | Hecho (25/25) ✅ archivado |
| [`reportes-kpis-export`](openspec/changes/reportes-kpis-export) | KPIs (incl. desempeño del mesonero) y exportación PDF/Excel. | Pendiente (0/30) |
| [`croquis-planos`](openspec/changes/archive/2026-10-14-croquis-planos) | Precargar un croquis como fondo del lienzo, plantillas y duplicar. | Hecho (22/22) ✅ archivado |
| [`pruebas-y-cobertura`](openspec/changes/pruebas-y-cobertura) | Pruebas con Moq, concurrencia, separar integración y cobertura con coverlet. | Pendiente (0/32) |

### Orden sugerido y dependencias

| Orden | Spec | Depende de | Comparte archivos con |
| :---: | :--- | :--- | :--- |
| 1 | `concurrencia-stock` | — | — |
| 2 | `presentaciones-y-porciones` | `concurrencia-stock` | Rutas de comanda, venta y cierre |
| 3 | `auditoria-total` | `concurrencia-stock` (reintentos y transacciones) | `lib/navigation.tsx` (con `reportes-kpis-export`) |
| 4 | `modulos-complementarios` | — (conviene tras auditoría para quedar trazados) | `lib/navigation.tsx` |
| 5 | `reportes-kpis-export` | — (migraciones ordenadas tras `concurrencia-stock`) | `lib/navigation.tsx`, `ComandaDetalle` |
| 6 | `croquis-planos` | — | Plano y reservas |
| 7 | `pruebas-y-cobertura` | `concurrencia-stock`; cubre además el resto | Pruebas de todos los módulos |

`presentaciones-y-porciones` registra aperturas y descartes en la bitácora de `auditoria-total`
(ya aplicada). `pruebas-y-cobertura` va al final para medir la cobertura del sistema completo.

---

## Contribuir

Lee la [guía de contribución](CONTRIBUTING.md) y la
[convención de commits](docs/dev/11-guias/commits.md) antes de abrir un Pull Request.
