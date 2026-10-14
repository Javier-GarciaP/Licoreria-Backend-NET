# Design

## Context

Estado actual verificado en el código:

- `Plano` (`Domain/Entities/Zona.cs`) tiene `Nombre`, `Version`, `Activo`, `AnchoFondo`, `AltoFondo`, `Rejilla` y `Piso` (textura por nombre). No hay imagen de fondo. `PlanoElemento` guarda tipo, forma, color, etiqueta, posición, tamaño, rotación y vínculos opcionales a `Zona` y `Mesa`.
- `ServicioClub.CrearPlanoAsync` crea siempre el plano con `Activo = true` y llama a `DesactivarOtrosPlanosAsync`. `EditarPlanoAsync` hace un upsert de elementos por `Id` y suma 1 a `Version`.
- `GET /api/v1/planos` y `GET /api/v1/planos/{id}` son anónimos (los usa la reserva web pública) y devuelven todo plano no eliminado.
- Las rutas de escritura de `PlanosController`, `ZonasController` y `MesasController` (crear, editar, eliminar) usan `Permisos.ClubGestionar` (`club:manage`). El Host lo tiene (`RolesSeguridad`), y el mismo permiso protege `ListaVipController` y `EntradasController`, y el ítem de navegación de Entradas y Lista VIP.
- El editor (`EditorMapaPage`, 840 líneas) también llama a `clubApi.crearMesa` y `actualizarMesa` al enlazar y mover mesas.
- `MapaView` dibuja un SVG con `FondoMapa` (piso por nombre), `CapaZonas` y los elementos. Existe una copia casi idéntica en `public-web`.
- El almacenamiento (`IAlmacenamientoArchivos`, `AlmacenamientoArchivosLocal`) guarda con nombre GUID bajo `wwwroot/uploads` y se sirve como estático y público. `ArchivosController.Subir` exige `ContenidoPublicar` y registra un `MediaAsset`, lo cual no encaja con un fondo de plano restringido al Administrador.
- `ServicioClubPlanoTests` tiene 6 pruebas con Moq de crear, editar y activar planos.

Motivación y alcance: ver proposal.md. Comportamiento exigido: ver specs/.

## Goals / Non-Goals

**Goals:**
- Que el croquis sea un dato del plano, con ciclo de vida propio (subir, reemplazar, quitar, copiar, borrar).
- Que duplicar y usar plantillas no pueda alterar el plano activo ni exponer nada en la web pública.
- Que la restricción de edición viva en la API, con la interfaz como reflejo.

**Non-Goals:**
- Vectorizar o reconocer automáticamente el croquis (la IA de planos sigue siendo simulada).
- Calibrar escala en metros ni recortar o rotar la imagen en el servidor.
- Plantillas predefinidas sembradas (las crea el Administrador desde sus propios planos).
- Mostrar el croquis en la web pública.
- Control de edición simultánea del mismo plano (lo cubre el manejo de conflictos de `concurrencia-stock`).

## Decisions

**D1. El croquis es un dato del `Plano`.**
`Plano` gana `FondoImagenUrl` (texto, nulo), `FondoOpacidad` (0 a 1, por defecto 0,5), `FondoAjuste` (`contener` | `cubrir` | `estirar`, por defecto `contener`) y `EsPlantilla` (por defecto falso). `PlanoDto`, `PlanoCrearDto` y `PlanoEditarDto` incorporan opacidad y ajuste con valores por defecto para mantener la compatibilidad; la URL solo aparece en `PlanoDto`.
*Alternativa:* un elemento de tipo `croquis` dentro de `PlanoElemento` (podría moverse y escalarse, pero se mezcla con el mobiliario, se arrastra por error y complica la copia). Se descarta.

**D2. Endpoints dedicados para el fondo.**
`POST /api/v1/planos/{id}/fondo` (multipart) y `DELETE /api/v1/planos/{id}/fondo`, con el permiso de distribución. Se evita `ArchivosController` porque exige otro permiso y registra `MediaAsset`. Validación en aplicación: extensión y tipo declarados en la lista permitida (png, jpg/jpeg, webp), tamaño máximo 5 MB (`RequestSizeLimit`) y verificación de los primeros bytes (firma de archivo) contra el formato declarado. Se guarda en la carpeta `planos`. Al reemplazar o quitar, se elimina el archivo anterior; al eliminar el plano, también.
La edición del plano (`PUT`) ignora cualquier URL: así no se puede apuntar el fondo a un recurso externo.
*Alternativa:* decodificar la imagen con una librería para validar dimensiones (más estricto, más dependencia). Se descarta por ahora; la firma y el tamaño bastan para el riesgo.

**D3. Duplicado y plantillas por un solo endpoint.**
`POST /api/v1/planos/{id}/duplicar` con `{ nombre?, comoPlantilla? }`:
- Copia dimensiones, rejilla, piso, opacidad y ajuste; los elementos con `Id` nuevos; y el archivo de fondo con nombre nuevo, mediante un nuevo método `CopiarAsync` del puerto de almacenamiento, para que original y copia no compartan archivo.
- La copia siempre nace `Activo = false` y no llama a `DesactivarOtrosPlanosAsync`.
- Si origen y destino son planos normales, conserva `ZonaId` y `MesaId`. Si el origen o el destino es plantilla, `MesaId` queda en nulo (una plantilla no representa mesas reales; las mesas se enlazan después en el editor) y se conserva `ZonaId`.
- `comoPlantilla = true` crea la copia con `EsPlantilla = true`; con el origen en plantilla y sin ese indicador, crea un plano normal.
*Alternativa:* endpoints separados para plantilla e instanciación (más rutas con la misma lógica). Se descarta.

**D4. Plantillas fuera de lo público y del plano activo.**
`ObtenerPlanosAsync` y `ObtenerPlanoAsync` filtran `EsPlantilla = false` salvo que se pida con el permiso de distribución (`?plantillas=true`); consultar una plantilla por `id` sin ese permiso responde 404. `EditarPlanoAsync` rechaza con 422 activar una plantilla. `DesactivarOtrosPlanosAsync` ya solo afecta a no eliminados; una plantilla siempre está inactiva, así que no entra en la regla del plano activo único.

**D5. Permiso `club:layout` para la distribución.**
Se agrega `Permisos.ClubDistribucion = "club:layout"` ("Editar la distribución del local: planos, zonas y mesas") asignado solo a `Administrador`. Se aplica a `POST`, `PUT` y `DELETE` de `PlanosController`, `ZonasController` y `MesasController`, y a las rutas nuevas. `ClubGestionar` se conserva para `ListaVipController` y `EntradasController`, y `desalojar` mantiene `VentasEscribir`. **BREAKING** para el Host.
*Alternativa:* quitar `ClubGestionar` al Host (rompería Lista VIP y Entradas). Se descarta.
Los permisos viajan en el JWT: tras el despliegue, las sesiones abiertas conservan los permisos antiguos hasta renovar el token (60 minutos). Se documenta; no se revocan sesiones automáticamente.

**D6. Interfaz.**
`MapaView` recibe el fondo y dibuja un `<image>` con `preserveAspectRatio` según el ajuste, `opacity` y `pointer-events: none`, entre `FondoMapa` y `CapaZonas`; se aplica a `MapaView` de admin (editor y operación) y no a la copia de `public-web`. El editor añade un panel de croquis (subir o reemplazar, quitar, control de opacidad, selector de ajuste y un interruptor local para ocultarlo mientras se edita). `PlanosPage` suma "Duplicar", "Guardar como plantilla" y "Nuevo desde plantilla". Los controles se envuelven con `<Can permiso="club:layout">` y la ruta `/salon/planos/:id` con `ProtectedRoute`. `planoAPayload` (`lib/plano.ts`) incluye opacidad y ajuste pero no la URL. El ítem de navegación Salón pasa a exigir `club:read` para que el Host siga viendo el plano activo en solo lectura; la pestaña de mesas del salón y las acciones de edición exigen `club:layout`.

**D7. Migración.**
Una migración agrega las cuatro columnas con valores por defecto. No hay relleno: los planos existentes quedan sin fondo, no son plantillas y mantienen su estado.

## Risks / Trade-offs

- [Las imágenes subidas se sirven como archivos estáticos públicos] → Nombres GUID no adivinables, la web pública no las enlaza y se documenta; si se quisiera protegerlas, se serviría por un endpoint autenticado (fuera de alcance).
- [Copiar el archivo en cada duplicado consume disco] → Límite de 5 MB por imagen; el volumen esperado (decenas de planos) es bajo.
- [Archivos huérfanos si falla la eliminación del archivo tras eliminar el plano] → La eliminación del archivo es tolerante a fallos (registra y continúa); se documenta una limpieza manual.
- [Un Host con sesión abierta conserva el permiso hasta 60 minutos] → Documentado; la API autoriza con los claims del token vigente y se renuevan al iniciar sesión.
- [El upsert de `EditarPlanoAsync` borra elementos que no vienen en la solicitud: un editor desactualizado puede borrar lo que otro agregó] → Fuera de alcance; lo cubre el conflicto de versión de `concurrencia-stock` (`xmin`) y queda auditado por `auditoria-total`.
- [`MapaView` está duplicado entre `admin` y `public-web`] → El cambio toca solo el de `admin`; unificar queda como mejora futura.

## Migration Plan

1. Aplicar la migración de columnas (sin relleno).
2. Desplegar backend y frontend juntos: el frontend nuevo usa rutas y campos que el backend antiguo no tiene, y el backend nuevo rechaza al Host que use el frontend antiguo.
3. Avisar al personal que el Host ya no edita distribución; la sesión se actualiza al volver a iniciar sesión.
4. Rollback: revertir la migración elimina las columnas; los archivos de `uploads/planos` quedan sin referencias y se pueden borrar a mano.
