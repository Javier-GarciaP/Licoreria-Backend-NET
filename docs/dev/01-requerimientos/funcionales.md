# Requerimientos Funcionales

Identificados por módulo. La prioridad usa MoSCoW: **M** (Must), **S** (Should),
**C** (Could).

## Catálogo

| ID | Requerimiento | Prioridad |
| :--- | :--- | :---: |
| RF-CAT-01 | Gestionar categorías jerárquicas de productos. | M |
| RF-CAT-02 | Gestionar marcas, unidades de medida e impuestos. | M |
| RF-CAT-03 | Gestionar productos y sus variantes (presentaciones). | M |
| RF-CAT-04 | Administrar múltiples códigos de barras por variante. | S |
| RF-CAT-05 | Definir listas de precios (detal, mayorista, happy hour). | S |
| RF-CAT-06 | Definir recetas para productos preparados (cócteles, tobos). | S |
| RF-CAT-07 | Definir modificadores/extras por producto. | C |

## Inventario

| ID | Requerimiento | Prioridad |
| :--- | :--- | :---: |
| RF-INV-01 | Registrar movimientos de inventario (kardex) inmutables. | M |
| RF-INV-02 | Consultar existencias por producto/variante. | M |
| RF-INV-03 | Alertar cuando el stock alcance el mínimo. | M |
| RF-INV-04 | Registrar mermas con motivo (dañado, partido, vencido). | M |
| RF-INV-05 | Registrar cortesías y reposiciones sin cobro. | M |
| RF-INV-06 | Gestionar lotes y fechas de vencimiento. | S |
| RF-INV-07 | Realizar ajustes y tomas físicas de inventario. | S |

## Compras

| ID | Requerimiento | Prioridad |
| :--- | :--- | :---: |
| RF-COM-01 | Gestionar proveedores. | M |
| RF-COM-02 | Crear y aprobar órdenes de compra. | S |
| RF-COM-03 | Registrar recepciones y actualizar inventario/costos. | M |
| RF-COM-04 | Gestionar cuentas por pagar a proveedores. | S |

## Ventas / POS

| ID | Requerimiento | Prioridad |
| :--- | :--- | :---: |
| RF-VEN-01 | Registrar ventas con detalle por variante. | M |
| RF-VEN-02 | Aplicar la tasa de cambio vigente a la venta. | M |
| RF-VEN-03 | Soportar pagos mixtos (varias formas en una venta). | M |
| RF-VEN-04 | Emitir comprobante fiscal con número de control. | M |
| RF-VEN-05 | Registrar devoluciones y notas de crédito. | S |
| RF-VEN-06 | Aplicar descuentos y promociones. | S |

## Cuenta y abonos

| ID | Requerimiento | Prioridad |
| :--- | :--- | :---: |
| RF-CUE-01 | Acumular consumos de una mesa en una cuenta. | M |
| RF-CUE-02 | Registrar abonos parciales sobre la cuenta. | M |
| RF-CUE-03 | Dividir una cuenta en partes. | S |
| RF-CUE-04 | Cerrar la cuenta y generar la venta. | M |
| RF-CUE-05 | Registrar propinas por mesero. | C |

## Caja y turnos

| ID | Requerimiento | Prioridad |
| :--- | :--- | :---: |
| RF-CAJ-01 | Abrir y cerrar sesiones de caja por turno. | M |
| RF-CAJ-02 | Registrar movimientos de ingreso/egreso de caja. | M |
| RF-CAJ-03 | Realizar arqueo por denominaciones al cierre. | M |
| RF-CAJ-04 | Emitir reporte de cierre (Z). | S |

## Club / Mesas / Reservas

| ID | Requerimiento | Prioridad |
| :--- | :--- | :---: |
| RF-CLB-01 | Gestionar zonas y mesas con su capacidad. | M |
| RF-CLB-02 | Diseñar el plano del local (editor gráfico). | M |
| RF-CLB-03 | Crear reservas desde el sistema interno. | M |
| RF-CLB-04 | Permitir reservas desde la web pública sobre el plano. | M |
| RF-CLB-05 | Registrar seña con comprobante y validación manual. | M |
| RF-CLB-06 | Gestionar eventos y su publicación en la web. | S |
| RF-CLB-07 | Gestionar lista VIP y control de acceso. | C |

## Comandas y operación

| ID | Requerimiento | Prioridad |
| :--- | :--- | :---: |
| RF-OPS-01 | Abrir sesión de mesa y crear comandas. | M |
| RF-OPS-02 | Enrutar ítems a barra o cocina. | M |
| RF-OPS-03 | Actualizar el estado de cada ítem (recibido/preparado/entregado). | M |
| RF-OPS-04 | Reflejar los cambios en tiempo real entre áreas. | M |
| RF-OPS-05 | Cargar una reserva y marcar lo entregado y lo faltante. | M |
| RF-OPS-06 | Añadir ítems adicionales a una reserva en curso. | M |
| RF-OPS-07 | Permitir que el cajero añada ítems a una mesa. | M |

## CRM

| ID | Requerimiento | Prioridad |
| :--- | :--- | :---: |
| RF-CRM-01 | Gestionar clientes y sus datos fiscales. | S |
| RF-CRM-02 | Acumular y canjear puntos de fidelidad. | C |
| RF-CRM-03 | Gestionar cuentas por cobrar de clientes. | C |

## Finanzas

| ID | Requerimiento | Prioridad |
| :--- | :--- | :---: |
| RF-FIN-01 | Registrar la tasa de cambio diaria (BCV/paralelo). | M |
| RF-FIN-02 | Consultar histórico de tasas por fecha. | S |
| RF-FIN-03 | Gestionar movimientos de tesorería. | S |

## Contenido y web pública

| ID | Requerimiento | Prioridad |
| :--- | :--- | :---: |
| RF-WEB-01 | Publicar catálogo, menú, horarios y galería. | M |
| RF-WEB-02 | Generar menú digital (QR) y menú físico (PDF). | M |
| RF-WEB-03 | Mostrar tasas de cambio del día. | M |
| RF-WEB-04 | Contacto por WhatsApp con mensaje precargado. | M |
| RF-WEB-05 | Administrar secciones y eventos sin tocar código. | S |

## IA

| ID | Requerimiento | Prioridad |
| :--- | :--- | :---: |
| RF-IA-01 | Digitalizar el plano desde una foto de un croquis en papel. | S |
| RF-IA-02 | Generar secciones del sitio desde plantillas y design tokens. | S |
| RF-IA-03 | Pronosticar demanda y sugerir reabastecimiento. | C |
| RF-IA-04 | Generar imágenes de platos y posters de eventos. | C |
| RF-IA-05 | Detectar anomalías en caja y mermas. | C |

## Administración y seguridad

| ID | Requerimiento | Prioridad |
| :--- | :--- | :---: |
| RF-ADM-01 | Gestionar usuarios, roles y permisos (RBAC). | M |
| RF-ADM-02 | Auditar acciones sensibles. | S |
| RF-ADM-03 | Tablero de KPIs para el administrador. | M |
