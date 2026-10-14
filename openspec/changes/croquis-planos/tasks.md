# Tasks

## 1. Permiso de distribución

- [ ] 1.1 Añadir `Permisos.ClubDistribucion` (`club:layout`) con su descripción y asignarlo solo a `Administrador` en `RolesSeguridad`; verificar con una prueba unitaria que solo el Administrador lo tiene y que el Host conserva `club:manage`
- [ ] 1.2 Cambiar a `ClubDistribucion` las rutas `POST`, `PUT` y `DELETE` de `PlanosController`, `ZonasController` y `MesasController`, manteniendo `desalojar` en `VentasEscribir`; verificar con pruebas de integración (Host 403 al editar un plano y al crear una mesa, mesero 200 al desalojar, Administrador 200)
- [ ] 1.3 Verificar que Lista VIP y Entradas siguen protegidas con `ClubGestionar`; verificar con una prueba de integración de que un Host sigue gestionando la lista VIP

## 2. Modelo y migración del croquis

- [ ] 2.1 Añadir `FondoImagenUrl`, `FondoOpacidad`, `FondoAjuste` y `EsPlantilla` a `Plano`, a `PlanoConfiguration` (longitudes, precisión y valores por defecto) y a los DTOs (`PlanoDto` con la URL; `PlanoCrearDto` y `PlanoEditarDto` solo con opacidad y ajuste, con valores por defecto); verificar que el proyecto compila y que los DTOs conservan compatibilidad con las pruebas existentes
- [ ] 2.2 Validar opacidad entre 0 y 1 y ajuste dentro de los valores permitidos con un validador de FluentValidation; verificar con pruebas unitarias de los valores límite, 1,5 y un ajuste desconocido
- [ ] 2.3 Crear la migración de las cuatro columnas con sus valores por defecto; verificar aplicándola sobre una BD con planos existentes, que quedan sin fondo y no son plantillas

## 3. Imagen de fondo

- [ ] 3.1 Añadir `CopiarAsync` a `IAlmacenamientoArchivos` e implementarlo en `AlmacenamientoArchivosLocal` con nombre nuevo; verificar con una prueba unitaria sobre una carpeta temporal de que el archivo copiado es independiente del original
- [ ] 3.2 Crear el validador de imagen (extensión, tipo declarado, tamaño máximo de 5 MB y firma de archivo para PNG, JPG y WebP); verificar con pruebas unitarias de un archivo correcto por formato, un `.png` falso, un archivo vacío y uno de 8 MB
- [ ] 3.3 Implementar en `ServicioClub` la asignación, el reemplazo y la eliminación del fondo, borrando el archivo anterior, y borrar el archivo al eliminar el plano; verificar con pruebas Moq de subida, reemplazo, quitar y eliminar plano
- [ ] 3.4 Exponer `POST` y `DELETE /api/v1/planos/{id}/fondo` con `RequestSizeLimit` de 5 MB y el permiso de distribución, y hacer que `PUT /api/v1/planos/{id}` ignore cualquier URL recibida; verificar con pruebas de integración (subida 200, archivo inválido 400, Host 403, URL externa ignorada)

## 4. Duplicado y plantillas

- [ ] 4.1 Implementar `DuplicarPlanoAsync` en `ServicioClub` (elementos con `Id` nuevos, fondo copiado, inactivo, sin tocar el plano activo, nombre por defecto con "(copia)"); verificar con pruebas Moq de los escenarios del spec, incluido duplicar el plano activo
- [ ] 4.2 Aplicar la regla de vínculos (conservar `MesaId` solo entre planos normales; limpiarlo desde o hacia plantilla y conservar `ZonaId`) y la opción `comoPlantilla`; verificar con pruebas Moq de plano a plano, plano a plantilla y plantilla a plano
- [ ] 4.3 Filtrar las plantillas de `ObtenerPlanosAsync` y `ObtenerPlanoAsync` salvo con el permiso de distribución y `?plantillas=true`, y rechazar con 422 activar una plantilla; verificar con pruebas Moq y de integración (visitante anónimo no ve plantillas, 404 por id, 422 al activar)
- [ ] 4.4 Exponer `POST /api/v1/planos/{id}/duplicar` con el permiso de distribución y actualizar `openapi/licoreria.yaml` con las rutas y campos nuevos; verificar con una prueba de integración (201, original intacto, Host 403) y con `task backend:openapi:check`

## 5. Frontend

- [ ] 5.1 Actualizar `packages/types` y `packages/api-client` (campos de fondo, `subirFondo`, `quitarFondo`, `duplicarPlano`, filtro de plantillas) y `lib/plano.ts` (`planoAPayload` sin la URL); verificar con `npm run typecheck` y una prueba Vitest del payload
- [ ] 5.2 Dibujar el fondo en `MapaView` de admin (`<image>` con ajuste y opacidad, sin capturar eventos) en el editor y en la operación, no en `public-web`; verificar con pruebas Vitest de que se renderiza con los atributos esperados y que no está en el árbol de `public-web`
- [ ] 5.3 Añadir al editor el panel de croquis (subir o reemplazar con validación de tipo y tamaño en cliente, quitar, opacidad, ajuste, interruptor local); verificar con pruebas Vitest de la subida, el error de tamaño y el guardado de opacidad y ajuste
- [ ] 5.4 Añadir a `PlanosPage` "Duplicar", "Guardar como plantilla", la lista de plantillas y "Nuevo desde plantilla"; verificar con pruebas Vitest de cada flujo y de que el nuevo plano queda inactivo
- [ ] 5.5 Ocultar con `<Can permiso="club:layout">` los controles de edición, proteger la ruta `/salon/planos/:id` y ajustar `lib/navigation.tsx` para que el Host vea el plano activo en solo lectura; verificar con pruebas Vitest de rol (Administrador ve los controles, Host no, redirección por URL directa)
- [ ] 5.6 Recorrer en el stack levantado: subir un croquis, alinear mesas sobre él, duplicar, guardar como plantilla y crear un plano desde ella; verificar visualmente en escritorio y en 375 px

## 6. Documentación y cierre

- [ ] 6.1 Documentar el croquis, las plantillas, el permiso `club:layout` y el efecto en las sesiones abiertas en `docs/` y en `CHANGELOG.md` (marcando el cambio como BREAKING para el Host); verificar que `task docs:build` compila
- [ ] 6.2 Ejecutar la batería de cierre (`dotnet build`, `dotnet test`, `npm run typecheck`, `npm run lint`, `npm test`, `npm run build`) y confirmar que todo queda en verde

## Workflow follow-up

- Revisar los artefactos antes de implementar con `/opsx:apply`.
- Los cambios de plano quedarán auditados cuando se aplique `auditoria-total`.
- Archivar el cambio con `/opsx:archive` cuando las pruebas estén en verde.
