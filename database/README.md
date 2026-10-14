# Base de Datos

Artefactos de base de datos **PostgreSQL** complementarios al código.

> Las migraciones de EF Core viven en
> `backend/src/Licoreria.Infrastructure/Persistence/Migrations`. Este directorio
> contiene scripts de infraestructura de datos, no el esquema generado por EF.

## Estructura

| Carpeta | Contenido |
| :--- | :--- |
| `init/` | Scripts que se ejecutan al crear el contenedor (extensiones, roles, esquemas). |
| `seeds/` | Datos iniciales o de referencia (catálogo base, roles, denominaciones). |
| `scripts/` | Utilidades (backup, restore, migración de datos). |
| `diagrams/` | Exportaciones del modelo entidad-relación. |

## Entorno local

```bash
task db:up     # levanta PostgreSQL con docker-compose
task db:down   # detiene los servicios
```

La configuración de conexión se define en `.env` (ver `.env.example`).

## Restricciones de integridad y concurrencia

La migración `RestriccionesConcurrencia` pone en la base de datos las garantías que la
lógica de aplicación no debe poder romper, ni siquiera ante operaciones simultáneas:

| Restricción | Tabla | Garantiza |
| :--- | :--- | :--- |
| `ck_stock_cantidad_no_negativa` | `stock_producto` | la existencia nunca es negativa |
| `ck_stock_reservada_no_negativa` | `stock_producto` | lo reservado nunca es negativo |
| `ck_stock_reservada_no_supera_cantidad` | `stock_producto` | no se reserva más de lo que hay |
| `ux_comprobantes_numero` | `comprobantes_fiscales` | el número de comprobante no se repite |
| `comprobante_numero_seq` | (secuencia) | numeración correlativa (puede haber saltos, nunca duplicados) |
| `ux_sesiones_mesa_abierta_por_mesa` | `sesiones_mesa` | una sola sesión abierta por mesa |
| `ux_sesiones_caja_abierta` | `sesiones_caja` | un solo turno de caja abierto |
| `ux_ventas_por_cuenta` | `ventas` | una cuenta genera una sola venta vigente |

Antes de crearlas, la migración **verifica los datos existentes** y se detiene con un mensaje
claro (por ejemplo «hay 2 sesiones de caja abiertas») si alguno viola una restricción; se
corrige el dato y se vuelve a aplicar. La secuencia de comprobantes continúa después del mayor
número ya emitido. El código traduce las violaciones a **409** (unicidad) y **422** (CHECK).

## Documentación

- [Modelo entidad-relación](../docs/dev/03-base-datos/modelo-er.md)
- [Esquemas y tablas](../docs/dev/03-base-datos/esquemas.md)
- [Estrategia PostgreSQL](../docs/dev/03-base-datos/postgresql.md)
