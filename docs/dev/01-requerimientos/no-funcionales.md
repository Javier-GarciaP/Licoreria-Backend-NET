# Requerimientos No Funcionales

## Rendimiento

- Las respuestas de la API para operaciones de lectura deben responder en menos de
  **300 ms** en condiciones normales.
- La propagación de una comanda entre áreas (mesero → barra/cocina) debe ser
  **casi instantánea** (menos de 1 s) mediante comunicación en tiempo real.
- La web pública debe cargar su contenido principal en menos de **2 s** en 3G rápido.

## Escalabilidad

- El diseño soporta una única sucursal, pero el dominio se modela por agregados que
  permitan evolucionar sin reescribir el núcleo.
- La API es **stateless** (salvo SignalR) para facilitar el escalado horizontal.

## Disponibilidad

- Disponibilidad objetivo del **99 %** en horario de operación del local.
- El POS debe tolerar cortes breves de conexión sin perder operaciones (reintento).

## Seguridad

- Autenticación mediante **JWT** con tokens de acceso y refresco.
- Autorización **RBAC** por rol y recurso.
- Contraseñas almacenadas con hash fuerte (bcrypt/Argon2).
- Comunicación exclusivamente por **HTTPS**.
- Registro de auditoría de acciones sensibles.
- Validación de datos de entrada y protección contra inyección (ORM parametrizado).

## Usabilidad

- Interfaz del sistema interno operable en **tablet y PC**.
- Web pública **responsive** y accesible (contraste, navegación por teclado).
- Flujos de comanda y caja optimizados para **pocos clics**.

## Mantenibilidad

- **Onion Architecture** con dependencias hacia el dominio.
- Cobertura de pruebas del dominio y casos de uso críticos.
- Documentación viva junto al código.
- Estilo uniforme validado por `.editorconfig` y linters.

## Portabilidad

- Backend ejecutable en Linux mediante contenedores.
- Base de datos **PostgreSQL**.
- Configuración por variables de entorno; sin secretos en el repositorio.

## Interoperabilidad

- API REST documentada con **OpenAPI 3**.
- Errores estandarizados con **RFC 7807**.
- Integración con WhatsApp mediante enlace `wa.me`.

## Observabilidad

- Logs estructurados con niveles configurables.
- Trazas de peticiones y correlación por identificador.
- Métricas básicas de uso y errores.

## Localización

- Interfaz y documentación en **español**.
- Manejo de montos en **USD** y **BS** con tasas de cambio históricas.
