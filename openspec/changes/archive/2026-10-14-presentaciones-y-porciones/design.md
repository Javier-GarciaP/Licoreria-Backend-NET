# Design

## Context

Estado actual verificado en el código y en los datos:

- `ProductoVariante` tiene `EsBase` (presentación con existencia propia, que se ordena al proveedor) y `RecetasVendidas` (`Receta`: variante vendida → variante insumo × cantidad decimal). Una variante sin stock propio consume sus insumos fraccionariamente; `ServicioKardex.DesglosarInsumosAsync` devuelve una fila por insumo (o la propia variante si no hay receta; vacío para Cocina).
- `ServicioCatalogo.ValidarConsistenciaArea` exige para Barra Simple al menos una presentación base (puede haber varias) y prohíbe base y stock en Preparado y en Cocina. `AgregarRecetaAsync` exige que el insumo sea base o tenga fila de stock.
- Datos reales: «Gaseosa Cola 2L» = `Botella 2L` (base, stock 13) + `Litro` (receta 0,5 de la de 2 L) + `Vaso` (receta 0,125); los tragos usan recetas de 0,04 a 0,05 de una botella de 0,75 L. No hay forma de declarar una botella de 1 L independiente ni una botella abierta.
- Con `concurrencia-stock`: `StockProducto` (cantidad, reservada, `Disponible`), `ComandaDetalle.Reservado`, `ServicioKardex.ReservarAsync/LiberarReservaAsync/ConsumirReservaAsync/LiberarReservasAsync`, `ExistenciaAgotadaException` (422 con `codigo`), `UnitOfWork` reentrante con reintento y respuestas 409/422 con `codigo` estable.
- `ProductoVarianteDto` ya expone `Cantidad`, `CantidadReservada`, `EsBase`. El POS (`SelectorProductoModal`) y la carta del mesonero (`SelectorVarianteModal`, nuevo) muestran variantes.
- Permisos de inventario: `inventory:read`, `inventory:write`, `inventory:merma`. Barra tiene `inventory:read`.

Motivación y alcance: ver proposal.md. Comportamiento exigido: ver specs/.

## Goals / Non-Goals

**Goals:**
- Que el modelo refleje cómo se opera: presentaciones físicas independientes y porciones que salen de botellas destapadas a mano.
- Que servir porciones respete el mismo contrato de reserva y concurrencia que las presentaciones.
- Que el personal vea qué puede pedir y qué hacer cuando falta algo.

**Non-Goals:**
- Conversión automática de las recetas fraccionarias existentes.
- Reintegrar porciones ya servidas al cancelar o devolver.
- Conteo de envases abiertos al cerrar el turno, caducidad de lo abierto o merma automática por tiempo.
- Notificar a la barra cuando un mesonero intenta pedir sin envase abierto (queda como mejora; el mensaje ya orienta).
- Porciones cuyo origen es una presentación de otro producto.
- Elegir de qué envase abierto sale cada porción (se usa el más antiguo).

## Decisions

**D1. La naturaleza se deriva; no hay columna nueva en `ProductoVariante`.**
Presentación = `EsBase`; porción = tiene `PorcionOrigen`; receta = tiene `Receta`; variantes de Cocina siguen sin existencia. El servicio de catálogo rechaza combinarlas en el momento de definir orígenes o recetas (el formulario valida lo demás), porque los orígenes y las recetas se guardan por endpoints propios después del producto. `ProductoVarianteDto` incorpora `Naturaleza` calculada.
*Alternativa:* una columna `Naturaleza` explícita (más clara, pero exige migrar todas las variantes y decidir qué hacer con las de Cocina o sin definir). Se descarta por ahora.

**D2. Tres entidades nuevas.**
- `PorcionOrigen` (`PorcionVarianteId`, `PresentacionVarianteId`, `PorcionesPorUnidad` entero ≥ 1), con índice único por par. El origen debe ser una presentación (`EsBase`) del **mismo producto**.
- `EnvaseAbierto` (`PorcionVarianteId`, `PresentacionVarianteId`, `PorcionesIniciales`, `PorcionesRestantes`, `Estado` Abierto/Agotado/Descartado, `AbiertoEn`, `AbiertoPorUsuarioId`, `CerradoEn`, `MotivoDescarte`), con `Consumir(n)` que pasa a Agotado en cero.
- `StockPorcion` (`VarianteId` único, `PorcionesReservadas`), análoga a la reserva de `StockProducto`; su fila es el punto de serialización de las reservas de esa porción (token `xmin`).
Disponibilidad = Σ `PorcionesRestantes` de envases Abiertos − `PorcionesReservadas`.
*Alternativa:* guardar lo reservado dentro de cada envase (obliga a repartir reservas entre envases y complica cancelar). Se descarta.

**D3. Kardex.**
Nuevo `TipoMovimientoInventario.Apertura = 7`: al destapar, movimiento −1 sobre la presentación con referencia al envase. El consumo de porciones se registra como movimiento `Venta` sobre la variante porción (que no tiene fila de stock, solo kardex); el descarte, como `Merma` sobre la porción con las porciones restantes. No se agrega `MovimientoPorcion`.

**D4. `ServicioKardex` decide por naturaleza.**
`Verificar`, `Reservar`, `Liberar` y `ConsumirReserva` consultan si la variante es porción (tiene orígenes). Para una presentación siguen el camino actual; para una porción trabajan con `StockPorcion` y los envases:
- `Reservar`: calcula la disponibilidad; si falta, lanza `PorcionesSinAbrirException` (subclase de `ExistenciaAgotadaException`, código `porciones-sin-abrir`, con `EnvasesCerrados`) cuando alguna presentación origen tiene existencia disponible, o `ExistenciaAgotadaException` si no hay nada.
- `ConsumirReserva` y la venta directa: consumen de los envases Abiertos en orden de `AbiertoEn` (FIFO), restan de `PorcionesReservadas` lo que estaba reservado y registran el movimiento. Un envase que llega a cero pasa a Agotado.
- Orden estable de bloqueo (por `Id`) al tocar varias filas, como en `concurrencia-stock`.
- Reintegro (`+cantidad`) sobre una porción: no se modifica ningún envase (lo servido no vuelve).
`DesglosarInsumosAsync` no cambia: una porción sin receta se devuelve a sí misma y un preparado que la usa como insumo la devuelve con su cantidad.

**D5. Destapar y descartar (`ServicioEnvases`).**
`DestaparAsync(porcionId, presentacionId)` en transacción: valida el origen, toma la presentación con rastreo, exige `Disponible ≥ 1`, aplica `AplicarAsync(-1, Apertura)` y crea el envase con `PorcionesPorUnidad` porciones. `DescartarAsync(envaseId, motivo)` exige que, tras retirarlo, las porciones abiertas restantes cubran lo reservado (409 si no), lo marca Descartado y registra la merma. El «cuál botella destapar» lo decide quien opera: el endpoint exige indicar la presentación. Ambas operaciones exigen `inventory:open`.

**D6. Receta con porción como insumo.**
`AgregarRecetaAsync` acepta como insumo una presentación con stock o una porción; para una porción exige cantidad entera. No se permite que una variante porción tenga recetas ni que una variante con receta tenga orígenes (422).

**D7. API y contratos.**
`GET/PUT /api/v1/variantes/{id}/origenes` (el `PUT` reemplaza la lista y valida mismo producto, presentación base y enteros); `GET /api/v1/envases-abiertos?porcionVarianteId&estado`; `POST /api/v1/envases-abiertos` (destapar); `POST /api/v1/envases-abiertos/{id}/descartar`. `ProductoVarianteDto` agrega `Naturaleza`, `PorcionesDisponibles` y `EnvasesCerradosAbribles` (suma de unidades disponibles de sus orígenes) y `PorcionesPotenciales`; se calculan por lotes en el mapeo del catálogo para no consultar por variante. Eliminar o desmarcar una presentación con orígenes o envases abiertos responde 409.

**D8. Base de datos.**
Migración con las tres tablas, el índice único de orígenes y de `StockPorcion`, y `CHECK` de integridad: `porciones_restantes >= 0 AND <= porciones_iniciales`, `porciones_reservadas >= 0`, `porciones_por_unidad >= 1`. La regla «reservadas ≤ abiertas» cruza filas y la garantiza la aplicación con el token de versión de `StockPorcion`.

**D9. Interfaz.**
- Catálogo: en `FormularioProducto`, cada variante de Barra Simple elige Presentación o Porción (las de receta siguen en Preparado y en las heredadas, con una marca «receta heredada»). La porción muestra una tabla de orígenes (presentación del mismo producto + porciones por unidad) y guarda con `PUT /variantes/{id}/origenes` después del producto, como las recetas hoy.
- Selección: `SelectorProductoModal` (POS) y `SelectorVarianteModal` (mesonero) muestran por variante la etiqueta de disponibilidad («Quedan 5», «3 vasos abiertos», «Sin abrir · 2 botellas», «Agotado»); la porción sin envase abierto muestra el aviso «pide a barra que destape una botella» y no se puede agregar. El 422 con código `porciones-sin-abrir` se traduce en `mensajeDeError` y `describirConcurrencia` con título «Falta destapar una botella».
- Barra: en la pantalla de barra (KDS) un panel «Envases abiertos» con «Destapar» (porción → presentación origen con existencia) y «Descartar» (motivo); en Existencias, una pestaña con lo abierto y el historial reciente.
- Tiempo real: los eventos de comanda y de apertura invalidan la carta y los paneles.

**D10. Datos heredados.**
Las variantes con receta fraccionaria siguen igual. El catálogo las marca como heredadas. Para el caso de Cola (`Litro` = 0,5 de la de 2 L, `Vaso` = 0,125) el administrador redefine: crea la presentación real de 1 L y convierte `Vaso` en porción con orígenes. Se documenta en la guía de catálogo; no hay conversión automática.

## Risks / Trade-offs

- [Si la barra olvida destapar, los pedidos de vasos se rechazan con «Sin abrir»] → Mensaje explícito y visible para el mesonero, panel con las porciones restantes en barra y, como mejora posterior, notificar a la barra.
- [Más piezas: tres tablas, nuevo servicio y varias pantallas] → Se entrega por capas (dominio, catálogo, envases, control de porciones, pantallas) con pruebas en cada una.
- [Los pedidos de preparados con porciones fallan si falta un trago] → La reserva es atómica por comanda y el mensaje indica el producto faltante.
- [Lo servido no se reintegra al cancelar] → Documentado; una merma manual corrige diferencias.
- [Descartar un envase con reservas pendientes] → Se rechaza con 409 hasta resolver los pedidos.
- [La derivación de la naturaleza admite variantes sin definir] → El formulario las rechaza al guardar y el catálogo las marca; una columna explícita queda como evolución.
- [Concurrencia sobre `StockPorcion` y los envases] → Mismo mecanismo de `concurrencia-stock` (token de versión, reintento y 409), con orden de bloqueo estable y pruebas de carrera.

## Migration Plan

1. Aplicar la migración (tablas nuevas y restricciones; sin tocar filas existentes).
2. Desplegar backend y frontend juntos: el frontend nuevo usa campos y rutas que el backend antiguo no tiene.
3. Asignar el permiso nuevo (se aplica al iniciar sesión de nuevo, como los demás permisos del token).
4. Redefinir en el catálogo los productos con porciones (la guía explica el caso de Coca-Cola).
5. Rollback: revertir la migración elimina las tablas nuevas; las variantes con recetas siguen funcionando.
