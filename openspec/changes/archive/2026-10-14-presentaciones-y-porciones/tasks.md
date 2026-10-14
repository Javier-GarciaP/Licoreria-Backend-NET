# Tasks

## 1. Dominio y migración

- [x] 1.1 Crear las entidades `PorcionOrigen`, `EnvaseAbierto` (con `Consumir`, estado y cierre al agotarse) y `StockPorcion` (con `Reservar`, `Liberar` y `Consumir` de reservadas), el valor `TipoMovimientoInventario.Apertura` y el enum `EstadoEnvase`; verificar con pruebas de dominio (consumir hasta agotar, no consumir de más, reservar y liberar, estados)
- [x] 1.2 Añadir `PorcionesSinAbrirException` (subclase de `ExistenciaAgotadaException` con `EnvasesCerrados`) y el código `porciones-sin-abrir` con sus datos en `ExceptionMiddleware`; verificar con pruebas de la excepción y del middleware (422, código y campos)
- [x] 1.3 Configurar las tres tablas en EF (índices únicos de orígenes y de `StockPorcion`, índice de envases por porción, estado y fecha) y crear la migración con los `CHECK` (`porciones_restantes` entre 0 y las iniciales, `porciones_reservadas >= 0`, `porciones_por_unidad >= 1`); verificar aplicándola sobre una base con los datos actuales sin tocar filas existentes y con los `CHECK` activos

## 2. Catálogo: presentaciones y porciones

- [x] 2.1 Implementar en `ServicioCatalogo` la lectura y el reemplazo de los orígenes de una porción (`GET/PUT /api/v1/variantes/{id}/origenes`) validando mismo producto, presentación con existencia propia, enteros mayores que cero y exclusividad con recetas y con `EsBase`; verificar con pruebas Moq de cada rechazo del spec y del caso correcto (vaso con 1 L = 4 y 2 L = 8)
- [x] 2.2 Permitir como insumo de una receta una porción (cantidad entera) y rechazar recetas en una porción; verificar con pruebas Moq del cóctel con trago y vaso, de cantidad fraccionaria en una porción y de receta en una porción
- [x] 2.3 Proteger la eliminación y el cambio de `EsBase` de una presentación con orígenes o envases abiertos (409); verificar con pruebas Moq de ambos casos
- [x] 2.4 Ampliar `ProductoVarianteDto` con `Naturaleza`, `PorcionesDisponibles`, `PorcionesPotenciales` y `EnvasesCerradosAbribles`, calculados por lotes en el mapeo del catálogo y de las búsquedas; verificar con pruebas Moq (porción con envases y reservas, presentación, receta heredada) y una prueba de integración del producto Coca-Cola con 1 L, 2 L y vaso
- [x] 2.5 Exponer los endpoints de orígenes en `VariantesController` con la política de catálogo y actualizar `openapi/licoreria.yaml`; verificar con pruebas de integración (200, 422 y 403) y `task backend:openapi:check`

## 3. Destapar y descartar envases

- [x] 3.1 Añadir el permiso `inventory:open` a `Permisos` y asignarlo a Administrador, Barra y Cajero en `RolesSeguridad`; verificar con pruebas de la matriz de permisos (mesero sin él)
- [x] 3.2 Implementar `ServicioEnvases.DestaparAsync` (valida origen, exige existencia disponible, descuenta 1 con movimiento `Apertura` y crea el envase con las porciones del origen); verificar con pruebas Moq: destapar 1 L (4 vasos), elegir la de 2 L (8 vasos), presentación sin existencia y origen no definido
- [x] 3.3 Implementar `DescartarAsync` (motivo obligatorio, merma por las porciones restantes, 409 si hay más reservado que lo que quedaría abierto) y el listado de envases abiertos; verificar con pruebas Moq de descartar, descartar con reservas y listado ordenado por antigüedad
- [x] 3.4 Exponer `GET/POST /api/v1/envases-abiertos` y `POST /api/v1/envases-abiertos/{id}/descartar` con `inventory:read` e `inventory:open`, y actualizar el contrato OpenAPI; verificar con pruebas de integración (flujo completo, mesero 403) y la paridad del contrato
- [x] 3.5 Prueba de integración de concurrencia: dos usuarios destapan al mismo tiempo la última botella; verificar con `dotnet test` que se crea un solo envase y el otro recibe 409 o 422 (repetida 10 veces)

## 4. Control de porciones

- [x] 4.1 Adaptar `ServicioKardex` para detectar si una variante es porción y calcular su disponibilidad (porciones en envases abiertos menos reservadas); verificar con pruebas Moq de disponibilidad con uno y varios envases y con reservas
- [x] 4.2 Reservar porciones al pedir (`PorcionesSinAbrirException` si hay presentaciones con existencia y ninguna abierta, `ExistenciaAgotadaException` si no hay nada) y liberarlas; verificar con pruebas Moq de los tres escenarios de reserva del spec y de la liberación
- [x] 4.3 Consumir porciones al servir en orden de antigüedad (pasando al siguiente envase y marcando agotados), con movimiento en el kardex de la porción y sin reintegro de lo servido; verificar con pruebas Moq: 3 vasos con envases de 2 y 8, servir dos veces el mismo ítem y cancelar un vaso ya servido
- [x] 4.4 Soportar porciones en la venta directa del POS (consumo inmediato, 422 si las porciones están comprometidas por mesas) y en recetas de preparados (reserva y consumo multiplicados por la cantidad); verificar con pruebas Moq del trago en el mostrador y de dos cuba libre con falta de un trago
- [x] 4.5 Pruebas de integración contra PostgreSQL del flujo completo (destapar, pedir, servir, cancelar, cerrar cuenta y cerrar turno) y de carrera (dos mesoneros piden el último vaso, repetida 10 veces); verificar con `dotnet test` que exactamente uno lo obtiene y que no hay porciones negativas

## 5. Frontend: catálogo

- [x] 5.1 Actualizar `packages/types` y `packages/api-client` (naturaleza, disponibilidad, orígenes, envases abiertos, códigos de error); verificar con `npm run typecheck`
- [x] 5.2 Ampliar `FormularioProducto` con la elección Presentación o Porción en las variantes de Barra Simple, el editor de orígenes (presentación del mismo producto y porciones por unidad), la marca «receta heredada» y la validación previa a guardar; verificar con pruebas Vitest (definir un vaso con dos orígenes, rechazar origen no base y variante sin definir)
- [x] 5.3 Guardar los orígenes con `PUT /variantes/{id}/origenes` tras guardar el producto y mostrarlos en el detalle del producto; verificar con una prueba Vitest del guardado y con el recorrido manual del caso Coca-Cola

## 6. Frontend: selección y disponibilidad

- [x] 6.1 Mostrar en `SelectorProductoModal` (POS) la naturaleza y la disponibilidad de cada variante con las etiquetas del spec, y deshabilitar las que no se pueden servir; verificar con pruebas Vitest (presentación con existencia, vaso con envase abierto, vaso sin abrir, agotado)
- [x] 6.2 Hacer lo mismo en `SelectorVarianteModal` y en las etiquetas de la carta del mesonero, incluido el aviso «pide a barra que destape una botella»; verificar con pruebas Vitest de los mismos casos
- [x] 6.3 Traducir el código `porciones-sin-abrir` en `mensajeDeError` y `describirConcurrencia` («Falta destapar una botella»), refrescar la carta y conservar el pedido; verificar con pruebas Vitest de `avisarError` y del POS

## 7. Frontend: barra y existencias

- [x] 7.1 Añadir en la pantalla de barra (KDS) el panel «Envases abiertos» con listado, «Destapar» (porción, luego presentación con existencia) y «Descartar» (motivo), visible según `inventory:open`; verificar con pruebas Vitest de la interfaz (si la app de servicio no tiene ejecutor, añadirlo con la configuración mínima) y con el recorrido manual
- [x] 7.2 Añadir en Existencias la pestaña «Envases abiertos» con lo abierto, quién y cuándo, y el historial reciente de aperturas y descartes; verificar con una prueba Vitest de la página
- [x] 7.3 Refrescar la carta y los paneles con los eventos de tiempo real y con las mutaciones de destapar y descartar; verificar con el recorrido manual de dos sesiones

## 8. Documentación y cierre

- [x] 8.1 Documentar el modelo (presentación, porción, receta), el flujo de la barra, la migración manual del caso Coca-Cola (`Litro` y `Vaso` heredados → presentación de 1 L y vaso con orígenes) y el permiso nuevo en `docs/` y en `CHANGELOG.md`; verificar que `task docs:build` compila
- [x] 8.2 Ejecutar la batería de cierre (`dotnet build`, `dotnet test`, `npm run typecheck`, `npm run lint`, `npm test`, `npm run build`) y confirmar que todo queda en verde
- [x] 8.3 Demo manual con el stack levantado: definir Coca-Cola con botellas de 1 L y 2 L y el vaso, destapar una, tomar pedidos de vasos desde dos mesas, comprobar «Sin abrir» y «Agotado», y servir hasta agotar el envase; verificar con capturas del recorrido

## Workflow follow-up

- Revisar los artefactos antes de implementar con `/opsx:apply presentaciones-y-porciones`.
- Implementar con `concurrencia-stock` ya aplicado (reservas, transacciones y códigos de error).
- Los eventos de apertura y descarte quedarán auditados cuando se aplique `auditoria-total`.
- Archivar el cambio con `/opsx:archive` cuando las pruebas estén en verde.
