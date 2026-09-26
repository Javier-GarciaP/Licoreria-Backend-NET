# Historias de Usuario

Formato: **Como** \<rol\>, **quiero** \<acción\> **para** \<beneficio\>.
Cada historia incluye criterios de aceptación.

## Web pública

### HU-01 · Reservar una mesa
**Como** cliente, **quiero** reservar una mesa desde la web **para** asegurar mi lugar.

- Dado un día y hora disponibles, cuando elijo zona y mesa en el plano, entonces el
  sistema muestra el resumen de la reserva.
- Cuando adjunto el comprobante de la seña, entonces la reserva queda *pendiente de
  validación*.
- Cuando el administrador valida el pago, entonces la reserva pasa a *confirmada*.

### HU-02 · Ver el menú y los precios
**Como** cliente, **quiero** ver el menú y los precios **para** decidir qué consumir.

- El menú muestra categorías, productos, precios en USD y su equivalente en BS según
  la tasa del día.

### HU-03 · Contactar por WhatsApp
**Como** cliente, **quiero** contactar al local por WhatsApp **para** resolver dudas.

- Al pulsar el botón, se abre WhatsApp con un mensaje precargado.

## Mesero

### HU-04 · Abrir mesa y tomar pedido
**Como** mesero, **quiero** abrir una mesa y tomar el pedido **para** iniciar el servicio.

- Al abrir la mesa se crea una sesión y una cuenta.
- Al agregar ítems, cada uno se enruta a barra o cocina según su tipo.

### HU-05 · Atender una reserva
**Como** mesero, **quiero** seleccionar una reserva al abrir la mesa **para** cargar lo
que el cliente ya había pedido.

- El sistema muestra el paquete reservado.
- Puedo marcar qué se entregó y qué falta, y añadir ítems adicionales.

### HU-06 · Reportar un producto dañado
**Como** mesero, **quiero** reportar cervezas dañadas **para** reponerlas sin cobro.

- Al reportar el daño, se descuenta del inventario como merma.
- Al reponer, se descuenta como cortesía y la línea se agrega a la cuenta a precio cero.

## Barra / Cocina

### HU-07 · Ver y actualizar comandas
**Como** barra/cocina, **quiero** ver las comandas entrantes y actualizar su estado
**para** coordinarme con el servicio.

- Las comandas aparecen en tiempo real.
- Puedo marcar cada ítem como recibido, preparado y entregado.

## Cajero

### HU-08 · Registrar abonos
**Como** cajero, **quiero** registrar abonos sobre una cuenta **para** llevar el saldo
pendiente.

- La cuenta muestra el total consumido, lo abonado y el saldo.

### HU-09 · Cobrar y cerrar la cuenta
**Como** cajero, **quiero** cerrar la cuenta con pago mixto **para** finalizar la venta.

- Puedo combinar varias formas de pago.
- Al cerrar, se genera la venta y el comprobante fiscal.

### HU-10 · Añadir ítems a una mesa
**Como** cajero, **quiero** añadir ítems a una mesa **para** completar el pedido.

## Administrador

### HU-11 · Diseñar el plano del local
**Como** administrador, **quiero** diseñar la distribución del local **para** que los
clientes seleccionen su mesa.

- Puedo crear zonas y colocar mesas con capacidad y forma.
- El plano se refleja en la web pública.

### HU-12 · Ver indicadores
**Como** administrador, **quiero** un tablero de KPIs **para** tomar decisiones.

- Veo ventas, ocupación, ticket promedio, productos top, reservas y mermas.

### HU-13 · Gestionar el contenido web
**Como** administrador, **quiero** publicar eventos y actualizar secciones **para**
mantener la web al día sin depender del desarrollador.

- Puedo crear un evento y la IA propone una sección basada en plantillas.
