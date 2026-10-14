# Proposal

## Why

En la defensa, el docente pidió poder precargar un croquis en el lienzo para distribuir las mesas y restringir la edición del mobiliario únicamente a quien corresponda. Hoy el editor de planos parte de un lienzo vacío: `Plano` solo guarda dimensiones, rejilla y una textura de piso, sin imagen de fondo; cada plano nuevo se arma desde cero; y no hay forma de reutilizar una distribución. Además, la edición de planos, zonas y mesas se protege con `club:manage`, un permiso que tiene también el Host y que a la vez protege Lista VIP y Entradas, así que no se puede restringir sin separarlo.

## What Changes

- **Imagen de fondo (croquis)**: el Administrador sube una imagen (PNG, JPG o WebP, hasta 5 MB) por plano; se muestra detrás de los elementos en el editor y en la vista de operación del personal, con opacidad y ajuste configurables. Se puede reemplazar y quitar. La imagen solo se cambia por el endpoint de subida, nunca por una URL externa.
- **Duplicar plano**: crea una copia inactiva (nunca activa) de un plano, con sus elementos y su imagen de fondo copiada, para probar una distribución alternativa sin tocar la vigente.
- **Plantillas de plano**: un plano se guarda como plantilla (sin vínculos a mesas reales) y se usa como punto de partida para planos nuevos. Las plantillas no se publican, no se activan y no aparecen en la web pública.
- **Edición de distribución solo para el Administrador**: nuevo permiso `club:layout` para crear, editar, duplicar, eliminar y dar fondo a los planos, y para crear, editar y eliminar zonas y mesas. El resto de los roles consulta el plano en solo lectura. **BREAKING**: el Host deja de poder editar planos, zonas y mesas.
- **Frontend**: el editor suma subida y ajuste del fondo, "Duplicar" y "Guardar como plantilla" en la lista de planos y "Nuevo desde plantilla"; los controles de edición se ocultan a quien no tiene el permiso y la ruta del editor queda protegida.
- **Pruebas**: unitarias con Moq de duplicado, plantillas y validación de imagen; de integración de los permisos por rol y del almacenamiento de la imagen.

## Capabilities

### New Capabilities
- `floor-plan-background`: croquis como imagen de fondo del plano, con subida validada, ajuste, reemplazo y eliminación.
- `floor-plan-reuse`: duplicado de planos y plantillas de plano como punto de partida de planos nuevos.
- `floor-plan-edit-access`: edición de la distribución del local (planos, zonas y mesas) reservada al Administrador, con consulta de solo lectura para los demás.

### Modified Capabilities
<!-- No hay specs principales todavía (openspec/specs está vacío). -->

## Impact

- **Backend (Domain)**: `Plano` (`FondoImagenUrl`, `FondoOpacidad`, `FondoAjuste`, `EsPlantilla`).
- **Backend (Application)**: `ServicioClub` (duplicar, plantillas, filtrado de plantillas, fondo), `PlanoDto`, `PlanoCrearDto`, `PlanoEditarDto`, `Permisos` y `RolesSeguridad` (permiso nuevo), puerto `IAlmacenamientoArchivos` (copiar).
- **Backend (Infrastructure)**: `AlmacenamientoArchivosLocal` (copiar), `PlanoConfiguration`, migración de las columnas nuevas.
- **Backend (WebAPI)**: `PlanosController` (fondo, duplicar, plantillas), `ZonasController` y `MesasController` (cambio de política), `openapi/licoreria.yaml`.
- **Frontend**: `EditorMapaPage`, `PlanosPage`, `SalonMesasPage`, `components/mapa/MapaView.tsx`, `lib/plano.ts`, `lib/navigation.tsx`, `packages/api-client`, `packages/types`.
- **Sesiones existentes**: los permisos viajan en el JWT, así que un Host con sesión abierta conserva el acceso hasta que expire su token (60 minutos).
- **Coordinación**: los cambios de plano quedan auditados por `auditoria-total`; la edición simultánea de un mismo plano por dos administradores se resuelve con el manejo de conflictos de `concurrencia-stock`.
