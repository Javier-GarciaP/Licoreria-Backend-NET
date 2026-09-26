# Visión del Producto

## Descripción

**Licorería / Discoteca** es una plataforma web de gestión integral para un local
nocturno y de venta de licores de **una sola sucursal**. Combina una **web pública**
para clientes con un **sistema interno** para el personal, sobre un backend .NET con
Onion Architecture y una base de datos PostgreSQL.

## Problema

Los locales de licorería y discoteca suelen operar con un punto de venta básico y
procesos manuales: reservas por mensajería, comandas en papel, control de inventario
informal y sin trazabilidad de mermas. Esto genera errores de cobro, pérdidas no
justificadas y una experiencia de cliente pobre.

## Solución

Una plataforma que unifica:

- **Web pública:** catálogo y precios, menú de comida y bebida, tasas de cambio del
  día, horarios, galería del local, eventos, reserva de mesas sobre un plano
  interactivo y pago de la seña.
- **Sistema interno:** gestión de mesas y zonas, reservas, comandas por área
  (barra y cocina), cuentas con abonos, POS de caja, inventario con kardex, compras,
  CRM, contenido web y tablero de KPIs.
- **Módulo de IA:** digitalización del plano desde un croquis en papel, generación de
  secciones del sitio a partir de plantillas, pronóstico de demanda, generación de
  imágenes y detección de anomalías.

## Objetivos

1. Reducir errores de cobro y descuadres de caja.
2. Dar trazabilidad completa al inventario, incluidas mermas y cortesías.
3. Mejorar la ocupación mediante reservas en línea y gestión visual de mesas.
4. Agilizar la operación con comandas en tiempo real entre mesero, barra y cocina.
5. Aportar información para decidir con un tablero de KPIs.
6. Publicar y actualizar la web pública sin depender del desarrollador.

## Actores

| Actor | Descripción |
| :--- | :--- |
| Cliente | Persona que consulta la web pública, reserva y paga la seña. |
| Mesero | Abre mesas, toma pedidos y gestiona la cuenta. |
| Barra | Prepara y entrega bebidas/tobos. |
| Cocina | Prepara y entrega comida. |
| Cajero | Opera el POS, registra abonos y cierra cuentas. |
| Host / Recepción | Gestiona reservas y la asignación de mesas. |
| Administrador | Configura el sistema, el plano, el catálogo y supervisa KPIs. |
| Editor de contenido | Administra la web pública y los eventos. |

## Propuesta de valor

- **Una sola fuente de verdad** para ventas, inventario y reservas.
- **Experiencia de cliente** moderna con reserva y pago en línea.
- **Operación en tiempo real** entre todas las áreas del local.
- **IA aplicada al negocio**, no como chatbot, sino como herramienta de diseño,
  previsión y control.
