# Proposal

## Why

El catálogo trata igual una botella física y una porción servida: ambas son «variantes» y lo que no tiene stock propio consume una fracción de **un solo** insumo mediante una receta. En los datos reales, «Gaseosa Cola 2L» tiene `Botella 2L` (stock 13), `Litro` (receta: 0,5 de la botella de 2 L) y `Vaso` (receta: 0,125). Pero en el local la botella de 1 L es **otra presentación física**, con su propio proveedor y existencia, y un vaso se obtiene **destapando** una botella (de 1 L o de 2 L, a elección de la barra). Con el modelo actual no se puede registrar una presentación de 1 L junto a la de 2 L del mismo producto sin fingir que es media botella; no existe el concepto de botella abierta; los tragos arrastran decimales (45 ml de 750 ml = 0,06) que acumulan error; y el inventario muestra «2,4 botellas» sin saber cuántas están cerradas. Además, el mesonero no tenía selector de variantes (corregido aparte) y no hay forma de saber si ya se destapó una botella para servir.

## What Changes

- **Tres naturalezas de variante**, mutuamente excluyentes: **presentación** (existencia propia y precio: botella 1 L, botella 2 L, lata, paquete; se compra al proveedor), **porción** (se sirve a partir de una presentación: vaso, trago, ración) y **receta** (preparados con varios insumos, como hoy).
- **Orígenes de una porción**: cada porción declara de qué presentaciones del mismo producto puede salir y cuántas porciones da cada una (vaso = 4 por botella de 1 L y 8 por botella de 2 L; trago de whisky = 16 por botella de 750 ml), con números enteros.
- **Envase abierto**: la barra **destapa** manualmente una botella elegida: se descuenta 1 unidad de esa presentación (movimiento `Apertura` en el kardex) y nace un envase abierto con sus porciones. Se puede **descartar** (merma de las porciones restantes). Al agotarse sus porciones se cierra solo.
- **Control de porciones**: la disponibilidad de una porción es la suma de porciones en envases abiertos menos las reservadas por comandas pendientes. Pedir una porción reserva; servir consume (el envase abierto más antiguo primero); cancelar, desalojar, cerrar cuenta o cerrar turno libera. Si hay botellas cerradas pero ninguna abierta, el pedido se rechaza con un mensaje que pide a barra destapar una (distinto de «agotado»).
- **Recetas con porciones**: un preparado (por ejemplo, un cóctel) puede usar una porción como insumo (1 trago de ron), y consume del envase abierto.
- **Catálogo**: el formulario de producto elige la naturaleza de cada variante y edita sus orígenes; las recetas fraccionarias existentes siguen funcionando y se marcan como «heredadas», sin conversión automática (un «Litro» que hoy es media botella no se puede convertir sin criterio humano).
- **Selección con disponibilidad**: el POS y el mesonero muestran por variante su precio y su disponibilidad («Quedan 5», «3 vasos abiertos», «Sin abrir · 2 botellas cerradas», «Agotado») y no permiten pedir lo que no se puede servir.
- **Barra y existencias**: panel de envases abiertos con «Destapar» y «Descartar» en la pantalla de barra y en una pestaña de Existencias.
- **Permiso nuevo** `inventory:open` (Administrador, Barra y Cajero) para destapar y descartar.
- **Pruebas**: dominio, servicios con Moq, concurrencia (última porción, dos barras destapando la última botella) e interfaz.

## Capabilities

### New Capabilities
- `presentation-portion-catalog`: naturaleza de las variantes (presentación, porción, receta), orígenes de las porciones y su definición en el catálogo.
- `open-container-management`: destapar, listar y descartar envases abiertos con su registro en el kardex.
- `portion-stock-control`: disponibilidad, reserva, consumo, liberación y venta directa de porciones, incluida su participación en recetas.
- `variant-availability-display`: selección de variantes con disponibilidad y mensajes claros en POS, mesonero y barra.

### Modified Capabilities
<!-- No hay specs principales todavía (openspec/specs está vacío). -->

## Impact

- **Backend (Domain)**: nuevas entidades `PorcionOrigen`, `EnvaseAbierto` y `StockPorcion`; `TipoMovimientoInventario.Apertura`; excepción de porciones sin abrir.
- **Backend (Application)**: `ServicioKardex` (verificar, reservar, liberar y consumir según la naturaleza de la variante), `ServicioCatalogo` (orígenes, validaciones, campos de disponibilidad en `ProductoVarianteDto`), nuevo `ServicioEnvases`, `ServicioVentas` y `ServicioCuentas` (porciones en venta directa y comandas), `Permisos`/`RolesSeguridad`.
- **Backend (Infrastructure)**: configuraciones de EF, restricciones `CHECK`, repositorio de envases, migración.
- **Backend (WebAPI)**: `VariantesController` (orígenes), nuevo `EnvasesAbiertosController`, `openapi/licoreria.yaml`.
- **Frontend**: `FormularioProducto` y editores del catálogo, `SelectorProductoModal` (POS), `SelectorVarianteModal` (mesonero), Existencias, pantalla de barra (KDS), `packages/types` y `packages/api-client`.
- **Datos existentes**: no se modifican; las variantes con receta fraccionaria siguen vendiéndose igual hasta que el administrador las reconfigure.
- **Coordinación**: depende de `concurrencia-stock` (reservas, transacciones, 409/422); toca las mismas rutas de comanda, venta y cierre. La auditoría automática de `auditoria-total` registrará aperturas y descartes cuando esté aplicada.
