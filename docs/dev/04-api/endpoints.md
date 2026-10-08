# Referencia de endpoints por modulo

Indice legible de la API v1 agrupado por modulo/controlador. La fuente
canonica es el contrato [`openapi/licoreria.yaml`](../../../openapi/licoreria.yaml).
Todos los recursos estan bajo `/api/v1`; la autenticacion bajo `/api/auth`.
Las rutas marcadas como **publica** no requieren token.

## AjustesInventario

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `POST` | `/api/v1/ajustes-inventario` | token |

## Archivos

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/archivos` | token |
| `POST` | `/api/v1/archivos` | token |
| `DELETE` | `/api/v1/archivos/{id}` | token |

## Auditoria

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/auditoria` | token |

## Auth

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `POST` | `/api/auth/login` | token |
| `POST` | `/api/auth/logout` | token |
| `GET` | `/api/auth/me` | token |
| `POST` | `/api/auth/refresh` | token |

## Categorias

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/categorias` | token |
| `POST` | `/api/v1/categorias` | token |
| `DELETE` | `/api/v1/categorias/{id}` | token |
| `GET` | `/api/v1/categorias/{id}` | token |
| `PUT` | `/api/v1/categorias/{id}` | token |

## Clientes

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/clientes` | token |
| `POST` | `/api/v1/clientes` | token |
| `DELETE` | `/api/v1/clientes/{id}` | token |
| `GET` | `/api/v1/clientes/{id}` | token |
| `PUT` | `/api/v1/clientes/{id}` | token |
| `GET` | `/api/v1/clientes/{id}/puntos` | token |
| `POST` | `/api/v1/clientes/{id}/puntos/acumular` | token |
| `POST` | `/api/v1/clientes/{id}/puntos/canjear` | token |

## Contenido

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/horarios` | token |
| `PUT` | `/api/v1/horarios` | token |
| `GET` | `/api/v1/local-info` | token |
| `PUT` | `/api/v1/local-info` | token |
| `GET` | `/api/v1/local-info/whatsapp` | token |

## Cuentas

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/cuentas` | token |
| `POST` | `/api/v1/cuentas` | token |
| `GET` | `/api/v1/cuentas/{id}` | token |
| `POST` | `/api/v1/cuentas/{id}/abonos` | token |
| `POST` | `/api/v1/cuentas/{id}/cerrar` | token |
| `POST` | `/api/v1/cuentas/{id}/comandas` | token |
| `PUT` | `/api/v1/cuentas/{id}/comandas/{comandaId}/detalles/{detalleId}/estado` | token |
| `POST` | `/api/v1/cuentas/{id}/dividir` | token |

## CuentasPorCobrar

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/cuentas-por-cobrar` | token |
| `POST` | `/api/v1/cuentas-por-cobrar` | token |
| `POST` | `/api/v1/cuentas-por-cobrar/{id}/pagos` | token |

## CuentasPorPagar

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/cuentas-por-pagar` | token |
| `POST` | `/api/v1/cuentas-por-pagar/{id}/pagos` | token |

## Denominaciones

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/denominaciones` | token |

## Entradas

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/entradas` | token |
| `POST` | `/api/v1/entradas` | token |
| `POST` | `/api/v1/entradas/{codigo}/validar` | token |
| `POST` | `/api/v1/entradas/{id}/cancelar` | token |

## Eventos

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/eventos` | token |
| `POST` | `/api/v1/eventos` | token |
| `GET` | `/api/v1/eventos/todos` | token |
| `DELETE` | `/api/v1/eventos/{id}` | token |
| `PUT` | `/api/v1/eventos/{id}` | token |

## Ia

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/ai/generaciones` | token |
| `GET` | `/api/v1/ai/generaciones/{id}` | token |
| `POST` | `/api/v1/ai/generaciones/{id}/aprobar` | token |
| `POST` | `/api/v1/ai/imagenes` | token |
| `POST` | `/api/v1/ai/planos` | token |
| `POST` | `/api/v1/ai/secciones` | token |

## Impuestos

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/impuestos` | token |
| `POST` | `/api/v1/impuestos` | token |
| `DELETE` | `/api/v1/impuestos/{id}` | token |
| `PUT` | `/api/v1/impuestos/{id}` | token |

## ListaVip

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/lista-vip` | token |
| `POST` | `/api/v1/lista-vip` | token |
| `DELETE` | `/api/v1/lista-vip/{id}` | token |
| `PUT` | `/api/v1/lista-vip/{id}` | token |

## ListasPrecio

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/listas-precio` | token |
| `POST` | `/api/v1/listas-precio` | token |
| `DELETE` | `/api/v1/listas-precio/{id}` | token |
| `PUT` | `/api/v1/listas-precio/{id}` | token |

## Lotes

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/lotes` | token |
| `POST` | `/api/v1/lotes` | token |
| `DELETE` | `/api/v1/lotes/{id}` | token |
| `PUT` | `/api/v1/lotes/{id}` | token |

## Marcas

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/marcas` | token |
| `POST` | `/api/v1/marcas` | token |
| `DELETE` | `/api/v1/marcas/{id}` | token |
| `PUT` | `/api/v1/marcas/{id}` | token |

## MenuDigital

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/menu-digital` | token |
| `GET` | `/api/v1/menu-digital/pdf` | token |
| `GET` | `/api/v1/menu-digital/qr` | token |

## Mermas

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/mermas` | token |
| `POST` | `/api/v1/mermas` | token |

## Mesas

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/mesas` | token |
| `POST` | `/api/v1/mesas` | token |
| `DELETE` | `/api/v1/mesas/{id}` | token |
| `PUT` | `/api/v1/mesas/{id}` | token |
| `POST` | `/api/v1/mesas/{id}/desalojar` | token |

## MetodosPago

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/metodos-pago` | pública |

## Modificadores

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/modificadores` | token |
| `POST` | `/api/v1/modificadores` | token |
| `DELETE` | `/api/v1/modificadores/{id}` | token |
| `PUT` | `/api/v1/modificadores/{id}` | token |

## MovimientosInventario

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/movimientos-inventario` | token |

## OrdenesCompra

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/ordenes-compra` | token |
| `POST` | `/api/v1/ordenes-compra` | token |
| `GET` | `/api/v1/ordenes-compra/{id}` | token |
| `POST` | `/api/v1/ordenes-compra/{id}/aprobar` | token |
| `POST` | `/api/v1/ordenes-compra/{id}/cancelar` | token |
| `POST` | `/api/v1/ordenes-compra/{id}/enviar` | token |

## Paginas

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/paginas` | token |
| `POST` | `/api/v1/paginas` | token |
| `GET` | `/api/v1/paginas/todos` | token |
| `DELETE` | `/api/v1/paginas/{id}` | token |
| `GET` | `/api/v1/paginas/{id}` | token |
| `PUT` | `/api/v1/paginas/{id}` | token |

## Permisos

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/permisos` | token |

## Planos

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/planos` | token |
| `POST` | `/api/v1/planos` | token |
| `DELETE` | `/api/v1/planos/{id}` | token |
| `GET` | `/api/v1/planos/{id}` | token |
| `PUT` | `/api/v1/planos/{id}` | token |

## PreciosProducto

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/precios-producto` | token |
| `PUT` | `/api/v1/precios-producto` | token |
| `GET` | `/api/v1/precios-producto/vigente` | token |
| `DELETE` | `/api/v1/precios-producto/{id}` | token |

## Productos

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/productos` | token |
| `POST` | `/api/v1/productos` | token |
| `DELETE` | `/api/v1/productos/{id}` | token |
| `GET` | `/api/v1/productos/{id}` | token |
| `PUT` | `/api/v1/productos/{id}` | token |
| `GET` | `/api/v1/productos/{id}/recetas` | token |
| `POST` | `/api/v1/productos/{id}/recetas` | token |
| `DELETE` | `/api/v1/productos/{id}/recetas/{recetaId}` | token |
| `GET` | `/api/v1/productos/{id}/modificadores` | token |
| `POST` | `/api/v1/productos/{id}/modificadores` | token |
| `DELETE` | `/api/v1/productos/{id}/modificadores/{productoModificadorId}` | token |

## Promociones

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/promociones` | token |
| `POST` | `/api/v1/promociones` | token |
| `DELETE` | `/api/v1/promociones/{id}` | token |
| `PUT` | `/api/v1/promociones/{id}` | token |

## Proveedores

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/proveedores` | token |
| `POST` | `/api/v1/proveedores` | token |
| `DELETE` | `/api/v1/proveedores/{id}` | token |
| `GET` | `/api/v1/proveedores/{id}` | token |
| `PUT` | `/api/v1/proveedores/{id}` | token |

## Recepciones

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/recepciones` | token |
| `POST` | `/api/v1/recepciones` | token |
| `GET` | `/api/v1/recepciones/{id}` | token |

## Reportes

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/reportes/dashboard` | token |
| `GET` | `/api/v1/reportes/mermas` | token |
| `GET` | `/api/v1/reportes/ventas` | token |
| `GET` | `/api/v1/reportes/inventario` | token |
| `GET` | `/api/v1/reportes/compras` | token |
| `GET` | `/api/v1/reportes/propinas` | token |
| `GET` | `/api/v1/reportes/heatmap` | token |
| `GET` | `/api/v1/reportes/inventario-salud` | token |
| `GET` | `/api/v1/reportes/mermas-vs-ventas` | token |

## Reservas

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/reservas` | token |
| `POST` | `/api/v1/reservas` | token |
| `GET` | `/api/v1/reservas/{id}` | token |
| `PUT` | `/api/v1/reservas/{id}/estado` | token |
| `POST` | `/api/v1/reservas/{id}/pagos` | token |
| `POST` | `/api/v1/reservas/{id}/pagos/{pagoId}/validar` | token |
| `GET` | `/api/v1/reservas/{id}/pedidos` | token |
| `POST` | `/api/v1/reservas/{id}/pedidos` | token |
| `DELETE` | `/api/v1/reservas/{id}/pedidos/{pedidoId}` | token |

## Roles

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/roles` | token |

## SesionesCaja

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/sesiones-caja` | token |
| `POST` | `/api/v1/sesiones-caja` | token |
| `GET` | `/api/v1/sesiones-caja/activa` | token |
| `POST` | `/api/v1/sesiones-caja/{id}/cerrar` | token |
| `POST` | `/api/v1/sesiones-caja/{id}/movimientos` | token |

## Stock

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/stock` | token |

## TasasCambio

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/tasas-cambio` | token |
| `POST` | `/api/v1/tasas-cambio` | token |
| `GET` | `/api/v1/tasas-cambio/actual` | token |

## TomasFisicas

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/tomas-fisicas` | token |
| `POST` | `/api/v1/tomas-fisicas` | token |
| `GET` | `/api/v1/tomas-fisicas/{id}` | token |

## UnidadesMedida

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/unidades-medida` | token |
| `POST` | `/api/v1/unidades-medida` | token |
| `DELETE` | `/api/v1/unidades-medida/{id}` | token |
| `PUT` | `/api/v1/unidades-medida/{id}` | token |

## Usuarios

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/usuarios` | token |
| `POST` | `/api/v1/usuarios` | token |
| `DELETE` | `/api/v1/usuarios/{id}` | token |
| `GET` | `/api/v1/usuarios/{id}` | token |
| `PUT` | `/api/v1/usuarios/{id}` | token |
| `POST` | `/api/v1/usuarios/{id}/password` | token |
| `POST` | `/api/v1/usuarios/{id}/revocar-sesiones` | token |

## Ventas

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/ventas` | token |
| `POST` | `/api/v1/ventas` | token |
| `GET` | `/api/v1/ventas/{id}` | token |
| `POST` | `/api/v1/ventas/{id}/devoluciones` | token |
| `POST` | `/api/v1/ventas/{id}/pagos` | token |

## Zonas

| Metodo | Ruta | Acceso |
| :--- | :--- | :--- |
| `GET` | `/api/v1/zonas` | token |
| `POST` | `/api/v1/zonas` | token |
| `DELETE` | `/api/v1/zonas/{id}` | token |
| `PUT` | `/api/v1/zonas/{id}` | token |

> Total: 200 endpoints.
