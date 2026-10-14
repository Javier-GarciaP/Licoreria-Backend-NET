# Tasks

## 1. Permisos y navegación

- [ ] 1.1 Añadir `Permisos.PromocionesGestionar` (`promotions:manage`) solo para Administrador y dar `CrmLeer` y `CrmEscribir` al Cajero en `RolesSeguridad`; verificar con pruebas unitarias de la matriz de permisos de Administrador, Cajero y Mesero
- [ ] 1.2 Exigir `promotions:manage` en `POST`, `PUT` y `DELETE` de `PromocionesController` manteniendo `VentasLeer` en el `GET`; verificar con pruebas de integración (mesero 403 al crear, cajero 200 al listar, administrador 200 al crear)
- [ ] 1.3 Quitar `oculto` a Promociones, Existencias, Entradas, Lista VIP y al grupo Clientes en `lib/navigation.tsx`, fijar `roles: ADMIN` en Promociones y conservar ocultos lotes, tomas y contenido; verificar ampliando `navigation.test.ts` con la matriz modo × rol × módulo del spec
- [ ] 1.4 Ocultar en `PromocionesPage` las acciones de edición a quien no tenga `promotions:manage`; verificar con una prueba de página con un usuario sin el permiso

## 2. Modelo y migración

- [ ] 2.1 Añadir `Venta.ClienteId`, `CuentaPorCobrar.VentaId`, `TipoMovimientoPuntos.Reversion` y la constante `MetodoPago.CodigoCredito`, con sus configuraciones de EF (FK, índice y índice único filtrado por venta); verificar que compila y que las pruebas de dominio existentes siguen en verde
- [ ] 2.2 Crear la migración de las dos columnas nuevas; verificar aplicándola sobre una BD con ventas y cuentas por cobrar existentes (quedan con valores nulos)
- [ ] 2.3 Añadir las opciones `Fidelidad:PuntosPorUSD` y `Credito:DiasVencimiento` con sus valores por defecto, enlazadas a clases de opciones, a `appsettings` y a `.env.example`; verificar con una prueba que se leen y que el valor 0 desactiva puntos

## 3. Cliente, puntos y crédito en la venta

- [ ] 3.1 Crear `IServicioClienteVenta` con `ValidarCliente` (404 inexistente o eliminado, 422 inactivo) y registrarlo en `DependencyInjection`; verificar con pruebas Moq de los tres casos
- [ ] 3.2 Implementar `AcumularPorVentaAsync` (⌊USD pagado sin crédito × tasa⌋, movimiento con referencia a la venta); verificar con pruebas Moq: 25,80 USD → 25 puntos, venta menor a un punto, venta mixta con 10 USD a crédito, tasa 0
- [ ] 3.3 Implementar `RegistrarCreditoAsync` (cliente obligatorio, monto en USD con la tasa de la venta, vencimiento por defecto o indicado y posterior a la venta, cuenta enlazada a la venta); verificar con pruebas Moq de crédito sin cliente, crédito en Bs, vencimiento por defecto y vencimiento pasado
- [ ] 3.4 Integrar el servicio en `ServicioVentas.CrearVentaAsync` (y los campos `ClienteId` y vencimiento en `RegistrarVentaDto`, `CerrarCuentaDto` y las respuestas con `puntosGanados` y `creditoUSD`) dentro de la misma transacción; verificar con pruebas Moq de que una falla de stock no deja puntos ni cuenta por cobrar y con una prueba de integración de venta con cliente y fiado
- [ ] 3.5 Pasar el cliente y el vencimiento en `CerrarCuentaAsync` y rechazar con 422 los abonos con método Crédito en `RegistrarAbonoAsync`; verificar con pruebas Moq de cierre a crédito y de abono en Crédito
- [ ] 3.6 Revertir puntos en `RegistrarDevolucionAsync` (proporcional, limitado al saldo, movimiento `Reversion`) y agregar el `Aviso` en `DevolucionDto` si la venta tiene cuenta por cobrar pendiente; verificar con pruebas Moq de devolución parcial, puntos ya canjeados y venta fiada

## 4. Cuentas por cobrar e historial del cliente

- [ ] 4.1 Derivar el estado `Vencida` en `ServicioCrm.MapearCuenta` y hacer que el filtro de pendientes incluya las vencidas; verificar con pruebas Moq de cuenta pasada de fecha, cuenta pagada y cuenta vigente
- [ ] 4.2 Añadir `GET /api/v1/clientes/{id}/ventas` (paginado, `crm:read`) con su DTO y actualizar `openapi/licoreria.yaml`; verificar con una prueba de integración y con `task backend:openapi:check`

## 5. Frontend

- [ ] 5.1 Actualizar `packages/types` y `packages/api-client` (cliente y vencimiento en la venta, `puntosGanados`, `creditoUSD`, `Aviso`, ventas del cliente, estado vencida); verificar con `npm run typecheck`
- [ ] 5.2 Añadir en `PosPage` y `OrdenPanel` el selector de método de pago (efectivo por defecto), el buscador de clientes con creación rápida y la condición de Crédito (cliente obligatorio y vencimiento visible, cobro deshabilitado sin cliente); verificar con pruebas Vitest ampliando `PosPage.test.tsx` (método elegido, crédito sin cliente deshabilitado, quitar cliente)
- [ ] 5.3 Mostrar cliente, puntos ganados y crédito en `TicketVenta`; verificar con una prueba Vitest del ticket con y sin esos datos
- [ ] 5.4 Añadir el selector de cliente y el método de pago en el diálogo de cierre de `CuentaPage`; verificar con una prueba Vitest de cierre con crédito
- [ ] 5.5 Mejorar `ClientesPage` con el detalle (ventas, puntos y cuentas por cobrar) y `CuentasPorCobrarPage` con la venta enlazada y el estado vencida; verificar con pruebas Vitest de ambas páginas (lista, vacío, error y detalle)
- [ ] 5.6 Añadir pruebas de página de los módulos reactivados que aún no tienen (Promociones, Existencias, Entradas, Lista VIP y Compras: lista, estado vacío y error de carga); verificar con `npm test`

## 6. Verificación por rol y cierre

- [ ] 6.1 Verificar Compras: recorrer órdenes, proveedores, recepciones y cuentas por pagar con el Administrador y confirmar que cargan y operan (aprobar, recibir, pagar); anotar y corregir cualquier defecto encontrado; verificar con el recorrido manual y una prueba de integración de recepción
- [ ] 6.2 Recorrer en el stack levantado cada módulo con cada rol y modo (Administrador, Cajero, Host) confirmando que quien lo ve carga datos sin 403, y que Entradas y Lista VIP solo aparecen en Discoteca; verificar con el recorrido manual y la matriz de `navigation.test.ts`
- [ ] 6.3 Documentar los módulos reactivados, las reglas de puntos y crédito, la configuración y el cambio BREAKING de promociones en `docs/` y en `CHANGELOG.md`; verificar que `task docs:build` compila
- [ ] 6.4 Ejecutar la batería de cierre (`dotnet build`, `dotnet test`, `npm run typecheck`, `npm run lint`, `npm test`, `npm run build`) y confirmar que todo queda en verde

## Workflow follow-up

- Revisar los artefactos antes de implementar con `/opsx:apply`.
- Implementar después de `concurrencia-stock` (ambos tocan `ServicioVentas` y `CerrarCuentaAsync`).
- Coordinar `lib/navigation.tsx` con `auditoria-total`, `reportes-kpis-export` y `croquis-planos`.
- Confirmar con el docente la tasa de puntos y el plazo del crédito antes de la defensa.
- Archivar el cambio con `/opsx:archive` cuando las pruebas estén en verde.
